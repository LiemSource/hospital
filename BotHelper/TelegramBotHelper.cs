using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace BotHelper
{
    public class TelegramBotHelper
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;
        public readonly TelegramBotClient _botClient;
        public TelegramBotHelper(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(this.GetType());
            _botClient = new TelegramBotClient(_configuration["BotToken"]);
        }
        public TelegramBotHelper(IConfiguration configuration, ILoggerFactory logger, string botTokenKey)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(this.GetType());
            _botClient = new TelegramBotClient(_configuration[botTokenKey]);
        }
        public async Task<bool> SetWebHookAsync(string url)
        {
            await _botClient.SetWebhookAsync(url);
            return true;
        }
        public async Task<bool> SendMessage(string text)
        {
            try
            {
                Message message = await _botClient.SendTextMessageAsync(chatId: _configuration["NoticeCenterId"], text: text);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "发送电报消息异常");
                return false;
            }
        }
        public async Task ReplyKeyboardMarkup()
        {
            try
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new KeyboardButton[] { "Help me", "Call me ☎️" });
                Message message = await _botClient.SendTextMessageAsync(chatId: "@LiemFridayBot", "Choose a response", replyMarkup: replyKeyboardMarkup);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "发送电报消息异常");
            }
        }
        public async Task SetMyCommandsAsync()
        {
            try
            {
                var commandsList = new List<BotCommand>();
                commandsList.Add(new BotCommand { Command = "menu", Description = "menu des" });
                //await _botClient.SetMyCommandsAsync(commandsList);
                var commands = await _botClient.GetMyCommandsAsync();
                await _botClient.DeleteMyCommandsAsync();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "发送电报消息异常");
            }

        }
        public async Task WebHook(string request)
        {
            if (string.IsNullOrEmpty(request)) return;
            var hookObject = JObject.Parse(request);
            var chatId = hookObject.SelectToken("message.chat.id")?.ToString();
            if (string.IsNullOrEmpty(chatId)) return;
            var text = hookObject.SelectToken("message.text")?.ToString();
            if (string.IsNullOrEmpty(text)) return;
            await CommandExcute(text, chatId);
        }
        protected virtual async Task CommandExcute(string command, string chatId)
        {
            await Task.CompletedTask;
        }
        protected virtual async Task SetKeyboardAsync(string chatId)
        {
            await Task.CompletedTask;
        }

    }
    public static class StringExtension
    {
        /// <summary>
        /// unicode编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUnicodeString(this string str)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    strResult.Append("\\u");
                    strResult.Append(((int)str[i]).ToString("x"));
                }
            }
            return strResult.ToString();
        }


        /// <summary>
        /// unicode解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FromUnicodeString(this string str)
        {
            //最直接的方法Regex.Unescape(str);
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        int charCode = Convert.ToInt32(strlist[i], 16);
                        strResult.Append((char)charCode);
                    }
                }
                catch (FormatException ex)
                {
                    return Regex.Unescape(str);
                }
            }
            return strResult.ToString();
        }
    }
}
