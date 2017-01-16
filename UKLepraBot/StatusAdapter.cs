using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;

namespace UKLepraBot
{
    public class StatusAdapter
    {
        private readonly IConnectorClient _connector;
        private readonly Random _rnd;

        public StatusAdapter(IConnectorClient connector)
        {
            _connector = connector;
            _rnd = new Random();
        }

        public async Task Process(Activity activity)
        {
            var messageText = activity.Text;
            var conversationId = activity.Conversation.Id;

            var delaySettings = WebApiApplication.ChatSettings.DelaySettings.ContainsKey(conversationId)
                ? WebApiApplication.ChatSettings.DelaySettings[conversationId]
                : null;
            var state = WebApiApplication.ChatSettings.State.ContainsKey(conversationId)
                ? WebApiApplication.ChatSettings.State[conversationId]
                : (bool?)null;
            var currentDelay = WebApiApplication.ChatSettings.Delay.ContainsKey(conversationId)
                ? WebApiApplication.ChatSettings.Delay[conversationId]
                : (int?)null;

            var reply = activity.CreateReply();
            reply.Locale = "ru";

            if (messageText.ToLower().Contains("/huify"))
            {
                reply.Text = "Хуятор успешно активирован.";
                WebApiApplication.ChatSettings.State[conversationId] = true;

                if (delaySettings != null)
                {
                    WebApiApplication.ChatSettings.Delay[conversationId] = _rnd.Next(delaySettings.Item1, delaySettings.Item2 + 1);
                }
                else
                {
                    WebApiApplication.ChatSettings.Delay[conversationId] = _rnd.Next(4);
                }
            }
            else if (messageText.ToLower().Contains("/unhuify"))
            {
                reply.Text = "Хуятор успешно деактивирован.";
                WebApiApplication.ChatSettings.State[conversationId] = false;
            }
            else if (messageText.ToLower().Contains("/status"))
            {
                if (!state.HasValue)
                {
                    reply.Text = "Хуятор не инициализирован." + Environment.NewLine;
                }
                else if (state.Value)
                {
                    reply.Text = "Хуятор активирован." + Environment.NewLine;
                }
                else
                {
                    reply.Text = "Хуятор не активирован." + Environment.NewLine;
                }

                if (!currentDelay.HasValue)
                {
                    reply.Text += "Я не знаю когда отреагирую в следующий раз." + Environment.NewLine;
                }
                else
                {
                    reply.Text += $"В следующий раз я отреагирую через {currentDelay.Value} сообщений." +
                                  Environment.NewLine;
                }

                if (delaySettings == null)
                {
                    reply.Text += "Настройки задержки не найдены. Использую стандартные от 0 до 4 сообщений.";
                }
                else
                {
                    reply.Text +=
                        $"Сейчас я пропускаю случайное число сообщений от {delaySettings.Item1} до {delaySettings.Item2}";
                }
            }

            await _connector.Conversations.ReplyToActivityAsync(reply);
        }
    }
}