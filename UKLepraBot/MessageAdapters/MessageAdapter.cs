using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UKLepraBot.MessageAdapters
{
    public class MessageAdapter : MessageAdapterBase
    {
        private readonly string[] _brexitActivators = {"брексит", "брекзит", "brexit"};
        private readonly string[] _politiciansActivators = {"путин", "корбин", "навальный"};
        private readonly string[] _robotActivators = {"слава роботам"};
        private readonly string[] _rudeActivators = {"пошел нахуй", "иди нахуй", "охуел", "пошёл нахуй", "нахуй пойди" };
        private readonly string[] _kissingEmojiActivators = {"😘", "😚", "😍" };

        public MessageAdapter(IConnectorClient connector) : base(connector)
        {
        }

        public override async Task Process(Activity activity)
        {
            var messageText = activity.Text?.ToLower() ?? string.Empty;
            var channelData = (JObject) activity.ChannelData;
            var messageData = JsonConvert.DeserializeObject<JsonModels.Message>(channelData["message"].ToString());
            
            Activity reply;

            if (_robotActivators.Any(messageText.Contains))
                reply = ProcessSlavaRobotamMessage(activity);
            else if (_brexitActivators.Any(messageText.Contains))
                reply = ProcessBrexitMessage(activity);
            else if (_politiciansActivators.Any(messageText.Contains) && HelperMethods.YesOrNo())
                reply = ProcessPoliticiansNamesMessage(activity);
            else if (_rudeActivators.Any(messageText.Contains) && HelperMethods.MentionsBot(activity))
                reply = ProcessRudeMessage(activity);
            else if ((_kissingEmojiActivators.Any(messageText.Contains) || (messageData.sticker != null && _kissingEmojiActivators.Any(messageData.sticker.emoji.Contains))) && HelperMethods.MentionsBot(activity))
                reply = ProcessKissingMessage(activity);
            else
                reply = ProcessHuifyMessage(activity);

            if (reply == null) return;

            await Connector.Conversations.ReplyToActivityAsync(reply);
        }

        private Activity ProcessKissingMessage(Activity activity)
        {
            var reply = activity.CreateReply();

            var channelData = new JsonModels.ChannelData
            {
                method = "sendSticker",
                parameters = new JsonModels.Parameters
                {
                    sticker = "BQADAwADnwEAAr-MkATl4c1rX1QTGwI"
                },
            };
            reply.ChannelData = channelData;
            return reply;
        }

        private Activity CreateAndPrepopulateReply(Activity activity)
        {
            var user = activity.From?.Name;

            var reply = activity.CreateReply();
            reply.Locale = "ru";
            reply.Text = !string.IsNullOrEmpty(user) ? $"@{user} " : "";

            return reply;
        }

        private Activity ProcessRudeMessage(Activity activity)
        {
            var reply = CreateAndPrepopulateReply(activity);

            reply.Text += "отвали козлина";

            return reply;
        }

        private Activity ProcessBrexitMessage(Activity activity)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            var foo = Rnd.Next(0, 2);
            var sticker = string.Empty;
            if (foo == 0)
                sticker = "BQADBAADgwADe9a5BsXsWEDlCe3bAg";
            else if (foo == 1)
                sticker = "BQADBAADiQADe9a5BiskCu-ELBwsAg";
            else if (foo == 2)
                sticker = "BQADBAADiwADe9a5BlEg69510dKIAg";

            var channelData = new JsonModels.ChannelData
            {
                method = "sendSticker",
                parameters = new JsonModels.Parameters
                {
                    sticker = sticker
                }
            };
            reply.ChannelData = channelData;
            return reply;
        }

        private Activity ProcessPoliticiansNamesMessage(Activity activity)
        {
            var user = activity.From?.Name;
            var messageText = activity.Text.ToLower();

            var reply = activity.CreateReply();
            reply.Locale = "ru";
            reply.Text = !string.IsNullOrEmpty(user) ? $"@{user} " : "";

            if (messageText.Contains(_politiciansActivators[0]))
                reply.Text += "путин - хуютин";
            else if (messageText.Contains(_politiciansActivators[1]))
                reply.Text += "корбин - хуёрбин";
            else if (messageText.Contains(_politiciansActivators[2]))
                reply.Text += "навальный - овальный";
            else
                return null;

            return reply;
        }

        private Activity ProcessSlavaRobotamMessage(Activity activity)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            if (HelperMethods.YesOrNo())
            {
                var user = activity.From?.Name;
                reply.Text = (!string.IsNullOrEmpty(user) ? $"@{user} " : "") + "Воистину слава!";
                reply.Locale = "ru";
            }
            else
            {
                var channelData = new JsonModels.ChannelData
                {
                    method = "sendSticker",
                    parameters = new JsonModels.Parameters
                    {
                        sticker = "BQADBAADHQADmDVxAh2h6gc7L-sLAg"
                    }
                };
                reply.ChannelData = channelData;
            }
            return reply;
        }

        private Activity ProcessHuifyMessage(Activity activity)
        {
            if (activity.From.Id == ConfigurationManager.AppSettings["MasterId"])
                return null;

            var messageText = activity.Text;
            var conversationId = activity.Conversation.Id;

            var state = WebApiApplication.ChatSettings.State;
            var delay = WebApiApplication.ChatSettings.Delay;
            var delaySettings = WebApiApplication.ChatSettings.DelaySettings;
            if (!state.ContainsKey(conversationId) || !state[conversationId])
                return null;

            if (delay.ContainsKey(conversationId))
            {
                delay[conversationId] -= 1;
            }
            else
            {
                Tuple<int, int> delaySetting;
                if (delaySettings.TryGetValue(conversationId, out delaySetting))
                    delay[conversationId] = Rnd.Next(delaySetting.Item1, delaySetting.Item2 + 1);
                else
                    delay[conversationId] = Rnd.Next(4);
            }

            if (delay[conversationId] != 0) return null;

            delay.Remove(conversationId);
            var huifiedMessage = Huify.HuifyMe(messageText);
            if (string.IsNullOrEmpty(huifiedMessage)) return null;

            var reply = activity.CreateReply();
            reply.Text = huifiedMessage;
            reply.Locale = "ru";
            return reply;
        }
    }
}