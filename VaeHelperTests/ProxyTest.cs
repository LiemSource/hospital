using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VaeHelper;

namespace VaeHelperTests
{
    [TestClass()]
    public class ProxyTest
    {
        private IContainer container;
        public ProxyTest()
        {
            container = TestInitialize.Initialize<HospitalJob.Startup>();
        }
        [TestMethod()]
        public void TokenTest()
        {
            var result = HttpHelper.Get("https://dvapi.doveip.com/cmapi.php?rq=login&user=fridayliem&password=liemdaili1").Result;
            var tokenData = JObject.Parse(result.ResponseString);
            var token = tokenData.SelectToken("data.token").ToString();
            var ips = HttpHelper.Get($"https://dvapi.doveip.com/cmapi.php?rq=distribute&user=fridayliem&token={token}&geo=hk&auth=1&agreement=1timeout=180&repeat=0&num=10").Result;
            var ipsObject = JObject.Parse(ips.ResponseString);
            WebProxy wp = new WebProxy($"{ipsObject.SelectToken("data.ip")}:{ipsObject.SelectToken("data.port")}");
            //代理地址
            //wp.Address = new Uri($"{ipsObject.SelectToken("data.ip")}:{ipsObject.SelectToken("data.port")}");
            //设置身份验证凭据 账号 密码
            wp.Credentials = new NetworkCredential(ipsObject.SelectToken("data.username").ToString(), ipsObject.SelectToken("data.password").ToString());
           // var schema = HospitalHelper.QuerySchema(wp).Result;
        }

        [TestMethod()]
        public void ValidGEOTest()
        {
            var ges = "in,id,mx,bd,za,ph,th,ao,ve,ru,ae,sv,co,et,us,ni,pk,hn,ir,gt,pe,tw,my,lk,sa,br,np,ke,vn,ua,pr,gh,ma,fr,ng,cd,sn,ar,kw,eg,cm,ec,de,cu,ls,py,kh,dz,iq,gb,tt,jm,hu,pa,tg,mz,ug,sz,sd,zw,bi,mm,pg,tz,tn,es,so,uy,zm,uz,ro,ge,gr,gy,bg,sl,tr,bo,ba,pt,cr,do,cl,na,bj,rs,it,qa,om,pl,tj,cz,am,kz,az,lr,gm,kg,bh,mg,rw,sc,cn,ci,mu,bf,er,ly,gn,re,ga,ml,mr,td,yt,km,bw,cg,ne,mw,cv,dj,gq,sg,kr,sy,jp,jo,la,af,mo,kp,ps,hk,lb,bn,mv,il,mn,tl,ye,bt,tm,by,is,mt,mc,no,sm,se,ch,ee,lv,lt,md,si,sk,mk,va,nl,hr,ie,be,cy,dk,lu,al,ad,li,at,fi,gi,dm,bm,ca,gl,to,au,ck,nr,vu,sb,ws,tv,fm,mh,ki,pf,nz,fj,pw,sr,ag,aw,bs,bb,ky,gd,ht,mq,ms,pm,bz,as,ax,bl,bv,cc,cw,cx,fk,fo,gf,gg,gu,hm,im,je,lc,mf,mp,nf,nu,pm,pn,sh,sj,sx,tc,tf,tk,um,vg,vi,wf,xk,kn,gw,cf,eh,st,nc";
            var _geos = ges.Split(',').ToList();
            var proxyHelper = container.Resolve<ProxyHelper>();
            var validGeos = new ConcurrentBag<string>();
            var pages = 1;
            var pageSize = (int)Math.Ceiling((double)_geos.Count / pages);
            var tasks = new Task[pages];

            for (int pageIndex = 0; pageIndex < pages; pageIndex++)
            {
                var geoPages = _geos.Skip(pageIndex * pageSize).Take(pageSize);
                Trace.WriteLine($"线程:{pageIndex}:{string.Join(",", geoPages)}");
                tasks[pageIndex] = Task.Factory.StartNew((i) =>
                {
                    var page = (IEnumerable<string>)i;
                    foreach (var geo in page)
                    {
                        var proxy = proxyHelper.GetProxy(geo);
                        if (proxy != null)
                            validGeos.Add(geo);
                    }
                }, geoPages);
            }
            Task.WaitAll(tasks);
            Trace.WriteLine(string.Join(",", validGeos));
        }
    }
}
