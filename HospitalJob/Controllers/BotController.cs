using BotHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VaeEntity;

namespace HospitalJob.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BotController : Controller
    {
        protected readonly ILogger _logger;
        private readonly TelegramBotHelper _telegramBotHelper;
        public BotController(ILoggerFactory logger,
                                TelegramBotHelper telegramBotHelper)
        {
            _logger = logger.CreateLogger(this.GetType());
            _telegramBotHelper = telegramBotHelper;
        }
        [HttpPost("webhook")]
        public async Task<JsonResult> TgBotCallback()
        {
            var data = "";
            using (var reader = new StreamReader(ControllerContext.HttpContext.Request.Body))
            {
                //读取原始请求流的内容
                data = await reader.ReadToEndAsync();
            }
            Trace.WriteLine(data);
            return Json(new { RetCode = RetCode.BizOK, RetMessage = "回调成功" });
        }

    }
}
