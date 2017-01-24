using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class MessageAdapter : MessageAdapterBase
    {
        public MessageAdapter(IConnectorClient connector) : base(connector)
        {
        }

        public override async Task Process(Activity activity)
        {
            var messageText = activity.Text;
            Activity reply;

            if (messageText.ToLower().Contains("слава роботам"))
                reply = ProcessSlavaRobotamMessage(activity);
            else if (messageText.ToLower().Contains("брексит") || messageText.ToLower().Contains("брекзит") || messageText.ToLower().Contains("brexit"))
                reply = ProcessBrexitMessage(activity);
            else
                reply = ProcessHuifyMessage(activity);

            if (reply == null) return;

            await Connector.Conversations.ReplyToActivityAsync(reply);
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

        private Activity ProcessSlavaRobotamMessage(Activity activity)
        {
            var reply = activity.CreateReply();
            reply.Locale = "ru";

            var foo = Rnd.Next();
            if (foo % 2 == 0)
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