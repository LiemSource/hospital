using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VaeHelper;

namespace VaeTest
{
    public partial class Form1 : Form
    {
        CookieContainer cookies;
        public Form1()
        {
            InitializeComponent();

            txtJsessid.Text = "6a9d087906c4dfe682A62129d9d8835bb98f6564a9";

            var defaultParam = "q=5nlNRNaxheCW6Zt3/XITI5G69SdXP2mirM%2Bnv16ADnXgExxUeOsttYwiihx4Xs4%2B19dwhvZhTSdyR91tP1W67VcHCnHzZ/IDq8pwDmGyVNTdqwWCyIE2ES%2BUmTKJvlR41kDoDNb9I71BcXYaO6CmTQ1NtRWw%2BJDjHRxuhVsIS/ySxWUOdzapNNiOn6UFtVlPbcdGCbOvT0SGLy3kuoWaKTosrY%2BEaKaB4XvxCJCclbByJwhpQbLpc8Q7MJjsZ7VvfYacA6p1IrrZPClfg3E4LvsIBPBT4QbPMSU8Zm3nrLNlHI7Ei%2BWPxnW4/o/yezl8k4p0Retm1E/Bv3Wj3MhKBVmCjcxlGNIpPvHaGp%2BrVUIHWOQTkivpyfzPS6fp3VWXsW4iO1Y4taYDlnX4jdVa2kZSiaXW98y39U%2BodZPXdiT7KEMvtBjnqWvNv6J9ZA88YtxYvHcUl9QSu4qipYX7E0i4X3HwfFvsODA9zjLGuvqB258idThrEk5H5nTKE3%2BsTJACaNLduWJ73AfEL/iNxcmJCUvdGofb7t/HbueqBpG7taUfWedaXi8bCHTpGwOJkdGo02nWqUxqgq//0yCN2tlJOkVyeAFEq22EW7dMT51DiXwdFrFTFuf%2BnqpQdzBsiAiKJ6kZEc7FSF2bwsEUP/ziAPFvFZp7AjF%2BJXsyA4Og1rPBHzZCjdTLVmlkexwf%2BuL26oS2Z4Aj8BA41V6bEzCh5WW75Tdub5qj890k3gDoyyHe86ncjX8m0qnIDMspdIk8QYcoRwxZ3wSBzpTaF448xjFyEoIneOcfd1TGhuSDPcZCDLiqo72ly874sgZTQjMqa4Y0xq1H6SP1q4HZmw%3D%3D";
            txtParams.Text = defaultParam;
            cookies = new CookieContainer();
            cookies.Add(new Uri("https://api1-xusong.taihe.com"), new Cookie("JSESSID", txtJsessid.Text));
        }
        CancellationTokenSource _tokenSource;
        ManualResetEvent _resetEvent = new ManualResetEvent(true);
        ConcurrentQueue<string> responseBag;
        ConcurrentQueue<string> signResultBag;
        private async void btnSend_Click(object sender, EventArgs e)
        {

            responseBag = new ConcurrentQueue<string>();
            signResultBag = new ConcurrentQueue<string>();
            txtResponse.Text = "";
            txtSignRsult.Text = "";
            btnSend.Enabled = false;
            btnStop.Enabled = true;
            var requestInterval = (int)numericRequestInterval.Value;
            _tokenSource = new CancellationTokenSource();
            CancellationToken token = _tokenSource.Token;
            //SetResponse(token);
            SetSignResult(token);
            var taskCount = (int)numericTaskCount.Value;
            var tasks = new Task[taskCount];
            var jsessid = txtJsessid.Text;
            var queryParams = txtParams.Text;
            var locationParams = "q=tsfeBdcbvqiR6qCa5CxBfIeca52k5NhSfQXWvpWQTypmIi00wgv9q6R%2BNeQEiZyk9cOYv5rnhkkyy2/yqaCGRd09FsScGiT8ffhfciL0GJrlNfa24kWuNNmCeTwYzigiUY2xIs0gPeXUyX6/vHlFz2kak5BJD8j4cb%2BJwkVK3bkcdM8OCzJ1Rv3/bYZRV27YUlG8hiw6gvE3sU8MJbgvj4K32Yj36OKSCKbqrpc2oJAmpC2BTQQUyJ1hHw4QcLzmO5jtMviPmlSixZEjMAOwYHYzosHFghhxEKrXNPxcg0KDRZPRSru54GHefRhfwKbwz87AVlyHWyM1T/pZQktAtijoXJv994PRitgd3E2pjzA/1lz3JiVIdbjEbJ0FmQ0a70W1V4vlD7QNRMg4SNamOGk91dLQFMWPxnl6kFNaOQQ8P0YI3DGIzmOTP0x1/xaRTg7u7RW1mwOs/KMqphUAheyvMiWza0Qjll20QZXK6SLHxCDZaAv4Kvd8QfUjtBp%2BZmLR53EIc2jMuEstAA3KunhUMMPGOm1okiH5VIyIZjMTKHe%2BIpnPo/s2tXtv/QRQkjVH1CCAMQ03eiVur3wyLHqyUYqpChhY5hZQ/jcuwXQUR3cyhiVpL8AmAOZdR27x5DFG9ASFFsNspnc5meGSpQjm0KK6%2BQASBoO5CvzjX/aQNRzxNBwNXNC8RAW5DzEYojpK%2BCS%2BWULJfjxOzK5r5qKTPke2N22/UXQhqkqGcoCEsNIV%2BeVJMHo0YXrzuDl1TmoIOs9NV9r4eW0ASG3Xin8zn3qUSBV3PV7FgvYOFGqmQl428XAgMfKNAhlVihUhFhQoIsBhosIOIvp%2BpInFtS08P8NwjCdrJ3UiWwW5zPLU2DucJTZhr/XNjxkto3cMeB7P18gMsYHumO4PzM13AhQieSfqSmGLD4MYlFgPsfolILEgXd/FHnpSv9G0NKGSD7ydyDWzHIsVwUHsTYmBLKomwfOAmtRAcpPjjfaSROyuf0Uue3aYI6J0a7hcz1Gs";
            for (var taskIndex = 0; taskIndex < taskCount; taskIndex++)
            {
                tasks[taskIndex] = Task.Factory.StartNew(async (index) =>
                 {
                     try
                     {
                         while (!token.IsCancellationRequested)
                         {
                             await VaeSignHelper.RequestVaeWithParams(jsessid, locationParams, cookies: cookies);
                             await VaeSignHelper.RequestVae(jsessid, queryParams, (int)index,
                             (error) =>
                             {
                                 this.Invoke(new Action(() => MessageBox.Show(error)));
                             }, (response) =>
                             {
                                 responseBag.Enqueue(response);

                             }, (signResult) =>
                             {
                                 signResultBag.Enqueue(signResult);
                             },
                              (signSuccess) =>
                             {
                                 StopRun();
                                 this.Invoke(new Action(() => MessageBox.Show("signSuccess")));
                             }, cookies);
                             Thread.Sleep(requestInterval);
                         }
                     }
                     catch (Exception ex)
                     {
                         StopRun();
                         MessageBox.Show(ex.Message);
                     }
                 }, taskIndex);
                Thread.Sleep((int)numericInterval.Value);
            }

        }
        private async Task SetResponse(CancellationToken token)
        {
            await Task.Factory.StartNew(() =>
             {
                 var split = new List<char>();
                 for (int index = 0; index < txtResponse.Width / 7; index++) split.Add('-');
                 var splitString = string.Join("", split);
                 while (!token.IsCancellationRequested)
                 {
                     var msg = "";
                     while (responseBag.TryDequeue(out msg))
                     {
                         this.Invoke(new Action(() => txtResponse.Text += msg));
                     }
                     Thread.Sleep(200);
                 }
             }, token);
        }
        private async Task SetSignResult(CancellationToken token)
        {
            await Task.Factory.StartNew(() =>
            {

                while (!token.IsCancellationRequested)
                {
                    var msg = "";
                    while (signResultBag.TryDequeue(out msg))
                    {
                        this.Invoke(new Action(() => txtSignRsult.Text = $"{Environment.NewLine}{msg}"));
                    }
                    Thread.Sleep(200);
                }
            }, token);
        }

        private void StopRun()
        {
            var split = new List<char>();
            for (int index = 0; index < txtResponse.Width / 7; index++) split.Add('-');
            var splitString = string.Join("", split);
            this.Invoke(new Action(() =>
            {
                btnSend.Enabled = true;
                btnStop.Enabled = false;
                txtResponse.Text += Environment.NewLine + string.Join(splitString, responseBag);
                txtSignRsult.Text += Environment.NewLine + string.Join(Environment.NewLine, signResultBag);
            }));
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            StopRun();
        }
        private void StopFloorRun()
        {
            this.Invoke(new Action(() =>
            {
                btnStartFloor.Enabled = true;
                btnStopFloor.Enabled = false;
            }));
            _floorSource.Cancel();
        }
        CancellationTokenSource _floorSource;
        private void btnStartFloor_Click(object sender, EventArgs e)
        {
            _floorSource = new CancellationTokenSource();
            var JSESSID = "76d8eab43f008f070C002c12bd0cf4007e0242a3c3";
            var q = @"q=p2rouuIWFOC4/VMkzxcUMv7yLlIUqhQJ8KUOpyH5ZSsDvG5ZzBy1seSMMTFO12bwVUlA134eeoSxQ6y9HbVxcWQEUL%2Bgx7IIq3phNSfXDLcfvwkmJl7EqIyBoWzpoBqpehpwhk9o4Z6QofWOwzw3k0EwnopoNggYPjtC8jk3edSxswPQDevgwe5F0EntaCkCZ4GxfEZrk7UjFuAtUgqEjAaJnPTas3laUSv7/bT9Tis8VLeg%2BZh0F7yZyPbwyly9GtI8PMP5fT9K0BnKUQtoSVB2wOObYfO8T13lx0J0UvSCK9btl%2B9bZPagb3XFz1VfgVwpKJ61pg0/AKbYKM2w3oW4jivXJw0gxb1LvqYbFgrRJuupUIoid2zLVLyVwcUb2JoloAvNKRBQzdb%2BQrMv8xuySEhKocYwCzQbjqTzGJgiXpb7SzxIa3f/ccrCCigxzAV1H%2BAPXeqFvWoDuKh4q4oqTk9gIOBm9z8qL0WomtWBYEgvyZGayiy0b/wDlrHNBsPeQkcZPPBPt7TIDtL%2B2S24cDGGifjoPatW5NLRWdcZzbcYwjPqBl9JH3H1a99w4T30RjTnXawu2aCFUqWZu3St6pJrBjGNEfifZoFna269Hj5VtegE3UZDljRynLvR/WI1nvoo3wT7mSrBhEbCKCKdeFKllN9ByX3ah2sThFUJ%2Bssz3voznwKOcMC2LV3tt5J%2BN%2B4y%2BeYEQhbJsCkT6JPfnyZxyry%2BipmfqJlMCougRp7EcQNZ3xJyyo03X/2xzPvpAzvhqqnVWgiPYF3EKB69MB%2BRrGlYm%2Ba3HdWAWtYe80/DI/ik/LHbWmEf3E/94YZ2yAEGF9q1goauQS7OcQ/BlnHMLuQfkOZLX77T/wuitsQ6bPkfI2M3b8F4tjcyUGEdhKWCJ531sD4VnpemxnTKv3MHy9/PDZPA%2B8K2w7Q5HnaW2LTyqS4Qi/oFUAGsJ0fjzEvDaiLIESRTWS8vxP0RESun6NrqXy/r3beWpCFwYdQy7i11bNN4YOMvgTXc";
            var userAgent = "VaeHome/2.3.3 (iPhone; iOS 13.6; Scale/3.00)";
            Task.Factory.StartNew(() =>
            {
                while (!_floorSource.Token.IsCancellationRequested)
                {
                    var latestFloor = -1;
                    var targerFloor = 0;
                    try
                    {
                        var floor = VaeHelper.VaeSignHelper.RequestVaeWithParams(JSESSID, q, userAgent).Result;
                        var floorObj = JObject.Parse(floor);
                        var reviewInfos = floorObj.SelectToken("result.reviewTimeDesc.reviewInfo");
                        latestFloor = int.Parse(reviewInfos.FirstOrDefault()["floor"].ToString() ?? "-1");
                        this.Invoke(new Action(() =>
                        {
                            if (!string.IsNullOrEmpty(txtTargetFloor.Text)) targerFloor = int.Parse(txtTargetFloor.Text) - 1;
                        }));
                        if (latestFloor == targerFloor)
                        {
                            q = "q=iErleKuQSPuVDaGuUcN9rMx0hMh2rmfVe/iUNHmisOtlCIMKIB8f6vj7UZJWw9cAE98qdNXNrW9wrQ2fHgHv6oV%2B0ydRYbp%2Be927my4pvoOkbIz1mRMsYl1NZUB5QeeZWwFdSci0H8dPDvg34x2unV9z1hle2lahhVQihsBTfrsywYALilIrJ6jyB5ECFxjL4fy4RGe6i%2BnFhddpOMtACVPTQ7cxvr6FNY0BZh1JP2yK9dzLDLmKxW1%2BHKlZLLjxkn0b64JTnLJa8b36xp9ShQuTXPxvNwZ2KxrbFpiQv0wo5QAc6hdN1gzt8mG8538uJbN8O%2B37LDKWQoq8O49qZVSOcvcmYqCM0m2i7BvYbC2GLMEc2xMnIbxQWG3sbTYSeySlqB9/v/7hNSjVSospL%2BnyqFWhE7cLgEWiriG/JbITjH9cljb6rzkBZwCmh2PPOAzPsEKucRkvjr84qoLjJSYjoUUsDWWf8N%2BMPPpKlCG1d7Rqi6kUSw86xjM96F7qVQiWYHWaKgDCMychqqDsTqEqCtwtsMnBOq8AcQ/ThM%2BKG8p9169CtL/luZnnkmLMGG5vwKAWX7hrAMIohriEv3Tca/c/7vH3T1Zg5K0bC%2Bz0cqSNrqnGBBk9NiEhdlTbnLurp6ihspqh44KVQD2J2vC9pqmiQdxxYXfhVGlHt%2BUvrM8dbvG0rGIMmT1GlbuiEezTZL6%2Bn77A909YHLrQrDc79cWIGMGhvSIxEtSXbiRkpTawCMIWPMQH6NrndUK8BGinUJf6vS%2BI8VHN6ms0OuX3/VBtnKl7ahRFsZ9HzQOsEzQp5AQHTaEkcGPYn/HyCiXCJxAbkX5r96dKT0gzfrfOODhefKK05X0iDd89uH1Z2NjWdaXqGwS7uAjXgXo7yFRffoPH6FEP21AQ0bDzBOwnflJzOtMGwESHNxw0FuuRjkZCFZ5PHODaayM9GeXgnug1g/yOZ5pxIb6PHfyll01gdz%2Bc8nuyxb7BNRB/G6lHNHoT0GuoGObgKDkNJe%2B9NRyJ5CUP7Vg3FD83qtS/lJVQXiG5pDyogYqB6Njtn8i5RETY%2BetgujooS074VjAHfy72Vlt/WlgPKa%2BH%2B6vtFnAPG9fEB5o/Ys8veGCdPcbGiDhBbHJcFZmS5x/nQI4KhficI4Gn%2B6aLsmupgpTtFjGWAI2zsv5zRNyOUFKHjvc%3D";

                            var review = VaeHelper.VaeSignHelper.RequestVaeWithParams(JSESSID, q, userAgent).Result;
                            var reviewObj = JObject.Parse(review);
                            if (review.Contains("评论成功"))
                            {
                                this.Invoke(new Action(() =>
                                {
                                    labelReviewResult.Text = $"评论成功!目标楼层:{txtTargetFloor.Text},实际楼层:{ reviewObj.SelectToken("result.floor")}";
                                }));
                                StopFloorRun();
                            }
                        }
                        var floors = new List<string>();
                        if (reviewInfos != null)
                        {
                            foreach (var review in (JToken)reviewInfos)
                            {
                                var msg = $"楼层:{review["floor"]},评论时间:{review["addTime"]},评论人:{review.SelectToken("userInfo.name")},评论内容:{review["content"]}";
                                floors.Add(msg);
                            }
                        }
                        floors.Insert(0, $"楼层刷新时间:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        this.Invoke(new Action(() =>
                        {
                            txtFloors.Text = string.Join(Environment.NewLine, floors);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            txtFloors.Text = $"发生异常:{ex.Message}";
                        }));
                    }
                    var floorInterval = targerFloor - latestFloor;
                    if (floorInterval >= 500)
                    {
                        Thread.Sleep(10000);
                    }
                    else if (floorInterval < 500 && floorInterval >= 50)
                    {
                        Thread.Sleep(5000);
                    }
                    else if (floorInterval < 50 && floorInterval > 20)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
            btnStartFloor.Enabled = false;
            btnStopFloor.Enabled = true;
        }

        private void btnStopFloor_Click(object sender, EventArgs e)
        {
            StopFloorRun();
        }

        private void txtJsessid_TextChanged(object sender, EventArgs e)
        {
            cookies = new CookieContainer();
            cookies.Add(new Uri("https://api1-xusong.taihe.com"), new Cookie("JSESSID", txtJsessid.Text));
        }
    }
}
