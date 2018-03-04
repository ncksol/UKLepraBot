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
            var messageText = activity.Text?.ToLower() ?? string.Empty;

            Activity reply = null;

            var reaction = WebApiApplication.ReactionsManager.Reactions.GetReaction(activity);
            if (reaction != null)
            {
                reply = reaction.GetReply().CreateReplyActivity(activity, true);
            }
            else if (!string.IsNullOrEmpty(messageText))
            {
                reply = ProcessHuifyMessage(activity);
            }

            if (reply == null) return;

            await Connector.Conversations.ReplyToActivityAsync(reply);
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

            var reply = new Reply();
            reply.Text = huifiedMessage;
            
            return reply.CreateReplyActivity(activity, true);
        }
    }
}