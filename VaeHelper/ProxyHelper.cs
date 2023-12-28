using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VaeHelper
{
    public class ProxyHelper
    {
        protected readonly ILogger _logger;
        private string _user;
        private string _token;
        private static readonly string _url = "https://dvapi.doveproxy.net/cmapi.php"; //"https://dvapi.doveip.com/cmapi.php";

        private List<string> _geos;
        private string _currentGeos;
        public ProxyHelper(ILoggerFactory logger, IConfiguration configuration)
        {
            _logger = logger.CreateLogger(this.GetType());
            _user = configuration.GetValue<string>("ProxyUser:User") ?? "fridayliem2211";
            var password = configuration.GetValue<string>("ProxyUser:Password") ?? "Tbquf4pRkwB7F2X";
            var result = HttpHelper.Get($"{_url}?rq=login&user={_user}&password={password}", timeOut: (int)TimeSpan.FromMinutes(1).TotalMilliseconds).Result;
            if (string.IsNullOrEmpty(result.ResponseString)) return;
            var tokenData = JObject.Parse(result.ResponseString);
            _token = tokenData.SelectToken("data.token").ToString();
            var ges = "lc,nc,cf,mq,ht,bb,fj,fm,kr,sg,dj,cv,mw,cg,bw,km,mr,ml,re,ly,mu,ci,sc,rw,mg,kg,gm,lr,az,kz,am,cz,tj,pl,om,qa,it,rs,bj,na,cl,do,cr,pt,ba,bo,tr,sl,gb,iq,dz,kh,py,ls,cu,de,ec,cm,eg,kw,ar,sn,cd,ng,fr,ma,gh,pr,ua,vn,ke,np,br,sa,lk,my,tw,pe,gt,ir,hn,pk,ni,us,et,co,sv,ae,ru,ve,ao,th,ph,za,bd,mx,id,in,bg,gy,gr,ge,ro,uz,zm,uy,so,es,tn,tz,pg,mm,zw,sd,sz,ug,mz,tg,pa,hu,jm,tt,ca,al,lu,cy,be,hr,nl,sk,si,md,ch,se,by,ye,il,lb,ps,la,jo,jp,hk";
            _geos = ges.Split(',').ToList();
        }

        private WebProxy GetProxy(Func<WebProxy, bool> proxyTestFunc = null, string geo = null)
        {
            if (string.IsNullOrEmpty(_token))
            {
                _logger.LogError("获取代理登陆token为空,当前代理未生效!!!");
                return null;
            }
            _currentGeos = geo == null ? _geos.FindAll(g => g != _currentGeos)[new Random(Guid.NewGuid().GetHashCode()).Next(0, _geos.Count - 1)] : geo;
            var ips = HttpHelper.Get($"{_url}?rq=distribute&user={_user}&token={_token}&auth=1&geo={_currentGeos}&city=all&agreement=1&timeout=30&num=1", timeOut: (int)TimeSpan.FromMinutes(1).TotalMilliseconds).Result;
            var ipsObject = JObject.Parse(ips.ResponseString);
            WebProxy wp = null;
            //设置身份验证凭据 账号 密码
            _logger.LogInformation($"代理所属地区:{_currentGeos}");
            if ((int)ipsObject.SelectToken("errno") == 906)
            {
                _logger.LogInformation($"获取代理失败:{ips.ResponseString}");
                _geos.Remove(_currentGeos);
                if (geo == null)
                    return GetProxy(proxyTestFunc);
                return null;
            }
            else if ((int)ipsObject.SelectToken("errno") == 403)
            {
                _logger.LogInformation($"获取代理失败:已欠费");
                throw new ProxyExceptiton($"获取代理失败,已欠费");
            }
            else if ((int)ipsObject.SelectToken("errno") == 200)
            {
                wp = new WebProxy($"{ipsObject.SelectToken("data.ip")}:{ipsObject.SelectToken("data.port")}");
                wp.Credentials = new NetworkCredential(ipsObject.SelectToken("data.username").ToString(), ipsObject.SelectToken("data.password").ToString());
                if (proxyTestFunc != null && proxyTestFunc?.Invoke(wp) != true)
                {
                    if (geo == null)
                        return GetProxy(proxyTestFunc, geo);
                    else
                        return wp;
                }
            }
            else
            {
                _logger.LogInformation($"获取代理失败:{ips.ResponseString}");
                if (geo == null)
                    return GetProxy(proxyTestFunc, geo);
                else
                    return null;
            }
            return wp;
        }


        public WebProxy GetProxy(string geo = null, int timeOut = 5000)
        {
            WebProxy proxy = null;
            try
            {
                proxy = GetProxy(wp =>
               {
                   try
                   {
                       HttpHelper.Get("https://www.baidu.com/", webProxy: wp, timeOut: timeOut).Wait();
                   }
                   catch (Exception ex)
                   {
                       _logger.LogWarning($"代理Ip:{wp.Address}不可用,{ex.Message}!");
                       return false;
                   }
                   return true;
               }, geo);
            }
            catch (ProxyExceptiton ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取代理超时");
            }
            _logger.LogInformation($"已切换代理Ip:{proxy?.Address}");
            return proxy;
        }
    }

    public class ProxyExceptiton : Exception
    {
        public ProxyExceptiton(string ex) : base(ex)
        {

        }
    }
}
