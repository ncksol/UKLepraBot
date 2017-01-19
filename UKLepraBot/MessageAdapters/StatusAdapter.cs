using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class StatusAdapter: MessageAdapterBase
    {
        public StatusAdapter(IConnectorClient connector) : base(connector)
        {
        }

        public override async Task Process(Activity activity)
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
                    WebApiApplication.ChatSettings.Delay[conversationId] = Rnd.Next(delaySettings.Item1, delaySettings.Item2 + 1);
                }
                else
                {
                    WebApiApplication.ChatSettings.Delay[conversationId] = Rnd.Next(4);
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
            else if (messageText.ToLower().Contains("/uptime"))
            {
                var uptime = DateTimeOffset.UtcNow - WebApiApplication.StartupTime;
                reply.Text =
                    $"Uptime: {uptime.TotalDays} days, {uptime.Hours} hours, {uptime.Minutes}, {uptime.Seconds} seconds.";
            }

            await Connector.Conversations.ReplyToActivityAsync(reply);
        }
    }
}