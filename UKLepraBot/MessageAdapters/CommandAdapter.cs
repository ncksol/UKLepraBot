using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class CommandAdapter : MessageAdapterBase
    {
        public CommandAdapter(IConnectorClient connector) : base(connector)
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
                : (bool?) null;
            var currentDelay = WebApiApplication.ChatSettings.Delay.ContainsKey(conversationId)
                ? WebApiApplication.ChatSettings.Delay[conversationId]
                : (int?) null;

            Activity reply = null;

            if (messageText.ToLower().Contains("/huify"))
                reply = StartHuifyCommand(activity, conversationId, delaySettings);
            else if (messageText.ToLower().Contains("/unhuify"))
                reply = StopHuifyCommand(activity, conversationId);
            else if (messageText.ToLower().Contains("/status"))
                reply = StatusCommand(activity, state, currentDelay, delaySettings);
            else if (messageText.ToLower().Contains("/uptime"))
                reply = UptimeCommand(activity);
            else if (messageText.ToLower().Contains("/delay"))
                reply = DelayCommand(activity);
            else if (messageText.ToLower().Contains("/secret"))
                await SecretCommand(activity);
            else if (messageText.ToLower().Contains("/lmgtfy"))
                reply = GoogleCommand(activity);
            else if (messageText.ToLower().Contains("/reload"))
            {
                ReloadReactionsCommand();
                return;
            }

            if(reply != null)
                await Connector.Conversations.ReplyToActivityAsync(reply);
        }

        private Activity GoogleCommand(Activity activity)
        {
            var messageText = activity.Text;

            var reply = activity.CreateReply();            
            var messageParts = messageText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (messageParts.Length == 1) return null;

            var query = String.Join("%20", messageParts.Skip(1));
            reply.Text = $"http://lmgtfy.com/?q={query}";

            return reply;
        }
        

        private void ReloadReactionsCommand()
        {
            WebApiApplication.ReactionsManager.ReloadReactions();
        }

        private Activity DelayCommand(Activity activity)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            if(VerifyAdminCommandAccess(activity) == false)
            {
                reply.Text = GetAcccessDeniedCommandText();
                return reply;
            }

            var messageText = activity.Text;
            var conversationId = activity.Conversation.Id;
            var delaySettings = WebApiApplication.ChatSettings.DelaySettings;

            var messageParts = messageText.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (messageParts.Length == 1)
            {
                var currentDelay = new Tuple<int, int>(0, 4);
                if (delaySettings.ContainsKey(conversationId))
                    currentDelay = delaySettings[conversationId];

                reply.Text = $"Сейчас я пропускаю случайное число сообщений от {currentDelay.Item1} до {currentDelay.Item2}";
            }
            else if (messageParts.Length == 2)
            {
                int newMaxDelay;
                if (!int.TryParse(messageParts[1], out newMaxDelay))
                {
                    reply.Text = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                }
                else
                {
                    delaySettings[conversationId] = new Tuple<int, int>(0, newMaxDelay);
                    reply.Text = $"Я буду пропускать случайное число сообщений от 0 до {newMaxDelay}";
                }
            }
            else if (messageParts.Length == 3)
            {
                int newMaxDelay;
                int newMinDelay;
                if (!int.TryParse(messageParts[2], out newMaxDelay))
                {
                    reply.Text = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                }
                else if (!int.TryParse(messageParts[1], out newMinDelay))
                {
                    reply.Text = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                }
                else
                {
                    if (newMinDelay == newMaxDelay)
                    {
                        newMinDelay = 0;
                    }
                    else if (newMinDelay > newMaxDelay)
                    {
                        var i = newMinDelay;
                        newMinDelay = newMaxDelay;
                        newMaxDelay = i;
                    }

                    WebApiApplication.ChatSettings.DelaySettings[conversationId] = new Tuple<int, int>(newMinDelay, newMaxDelay);
                    reply.Text = $"Я буду пропускать случайное число сообщений от {newMinDelay} до {newMaxDelay}";
                }
            }

            return reply;
        }

        private static Activity UptimeCommand(Activity activity)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            var uptime = DateTimeOffset.UtcNow - WebApiApplication.StartupTime;
            reply.Text =
                $"Uptime: {(int) uptime.TotalDays} days, {uptime.Hours} hours, {uptime.Minutes} minutes, {uptime.Seconds} seconds.";
            return reply;
        }

        private static Activity StatusCommand(Activity activity, bool? state, int? currentDelay, Tuple<int, int> delaySettings)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            if (!state.HasValue)
                reply.Text = "Хуятор не инициализирован." + Environment.NewLine;
            else if (state.Value)
                reply.Text = "Хуятор активирован." + Environment.NewLine;
            else
                reply.Text = "Хуятор не активирован." + Environment.NewLine;

            if (!currentDelay.HasValue)
                reply.Text += "Я не знаю когда отреагирую в следующий раз." + Environment.NewLine;
            else
                reply.Text += $"В следующий раз я отреагирую через {currentDelay.Value} сообщений." +
                              Environment.NewLine;

            if (delaySettings == null)
                reply.Text += "Настройки задержки не найдены. Использую стандартные от 0 до 4 сообщений.";
            else
                reply.Text +=
                    $"Сейчас я пропускаю случайное число сообщений от {delaySettings.Item1} до {delaySettings.Item2}";
            return reply;
        }

        private Activity StopHuifyCommand(Activity activity, string conversationId)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            if(VerifyAdminCommandAccess(activity) == false)
            {
                reply.Text = GetAcccessDeniedCommandText();
                return reply;
            }

            reply.Text = "Хуятор успешно деактивирован.";
            WebApiApplication.ChatSettings.State[conversationId] = false;
            return reply;
        }

        private bool VerifyAdminCommandAccess(Activity activity)
        {
            var masterId = ConfigurationManager.AppSettings["MasterId"];
            var adminIds = ConfigurationManager.AppSettings["AdminIds"]?.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            adminIds.Add(masterId);

            return adminIds.Contains(activity.From.Id);
        }

        private string GetAcccessDeniedCommandText()
        {
            return "Не положено холопам королеве указывать!";
        }

        private Activity StartHuifyCommand(Activity activity, string conversationId, Tuple<int, int> delaySettings)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            if(VerifyAdminCommandAccess(activity) == false)
            {
                reply.Text = GetAcccessDeniedCommandText();
                return reply;
            }

            reply.Text = "Хуятор успешно активирован.";
            WebApiApplication.ChatSettings.State[conversationId] = true;

            if (delaySettings != null)
                WebApiApplication.ChatSettings.Delay[conversationId] = Rnd.Next(delaySettings.Item1, delaySettings.Item2 + 1);
            else
                WebApiApplication.ChatSettings.Delay[conversationId] = Rnd.Next(4);
            return reply;
        }

        private async Task SecretCommand(Activity activity)
        {
            var messageText = activity.Text;
            var messageParts = messageText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var secretKey = ConfigurationManager.AppSettings["SecretKey"];
            if (messageParts[1] != secretKey) return;

            var secretMessage = string.Join(" ", messageParts.Skip(2));

            var tBot = new ChannelAccount
            {
                Id = WebApiApplication.TelegramBotId,
                Name = WebApiApplication.TelegramBotName
            };

            var managementChatId = ConfigurationManager.AppSettings["ManagementChatId"];
            var managementChatName = ConfigurationManager.AppSettings["ManagementChatName"];

            var reply = new Activity
            {
                From = tBot,
                Type = ActivityTypes.Message,
                //Conversation = new ConversationAccount(true, managementChatId, managementChatName),
                Conversation = new ConversationAccount(false, "-1001067824111"),
                Timestamp = DateTime.Now,
                Text = secretMessage
            };

            var connector = new ConnectorClient(new Uri("https://telegram.botframework.com"));

            await connector.Conversations.SendToConversationAsync(reply);
        }
    }
}