using Autofac;
using BotHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace VaeHelperTests
{
    [TestClass]
    public class TelegramBotTest
    {
        private IContainer container;
        public TelegramBotTest()
        {
            container = TestInitialize.Initialize<HospitalJob.Startup>();
        }
        [TestMethod()]
        public void SetWebHookTest()
        {
            var botHelper = container.Resolve<TelegramBotHelper>();
            botHelper.SetWebHookAsync("https://1dfc-156-224-0-35.jp.ngrok.io/bot/webhook/").Wait();
        }
        [TestMethod()]
        public void SendMessageTest()
        {
            try
            {
                var botHelper = container.Resolve<TelegramBotHelper>();
                botHelper.SendMessage("发财了").Wait();
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex.Message);
            }
        }
        [TestMethod()]
        public void SetMyCommandsTest()
        {
            var botHelper = container.Resolve<TelegramBotHelper>();
            botHelper.SetMyCommandsAsync().Wait();

        }
        [TestMethod()]
        public void ReplyKeyboardMarkupTest()
        {
            var botHelper = container.Resolve<TelegramBotHelper>();
            botHelper.ReplyKeyboardMarkup().Wait();
        }
        [TestMethod()]
        public void WebHookTest()
        {
            var botHelper = container.Resolve<TelegramBotHelper>();
            botHelper.WebHook("{\"update_id\":639573312,\"message\":{\"message_id\":13,\"from\":{\"id\":977472782,\"is_bot\":false,\"first_name\":\"\\u7941\\u540c\\u4f1f\",\"username\":\"fridayliem\",\"language_code\":\"zh-hans\"},\"chat\":{\"id\":977472782,\"first_name\":\"\\u7941\\u540c\\u4f1f\",\"username\":\"fridayliem\",\"type\":\"private\"},\"date\":1661332446,\"text\":\"/start\",\"entities\":[{\"offset\":0,\"length\":5,\"type\":\"bot_command\"}]}}").Wait();
        }
    }
}
