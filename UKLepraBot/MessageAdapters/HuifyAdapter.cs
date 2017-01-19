using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class HuifyAdapter:MessageAdapterBase
    {
        public HuifyAdapter(IConnectorClient connector):base(connector)
        {
        }

        public override async Task Process(Activity activity)
        {
            var messageText = activity.Text;
            var conversationId = activity.Conversation.Id;

            var state = WebApiApplication.ChatSettings.State;
            var delay = WebApiApplication.ChatSettings.Delay;
            var delaySettings = WebApiApplication.ChatSettings.DelaySettings;
            if (state.ContainsKey(conversationId) && state[conversationId])
            {
                if (delay.ContainsKey(conversationId))
                {
                    delay[conversationId] -= 1;
                }
                else
                {
                    Tuple<int, int> delaySetting;
                    if (delaySettings.TryGetValue(conversationId, out delaySetting))
                    {
                        delay[conversationId] = Rnd.Next(delaySetting.Item1, delaySetting.Item2 + 1);
                    }
                    else
                    {
                        delay[conversationId] = Rnd.Next(4);
                    }
                }

                if (delay[conversationId] != 0) return;

                delay.Remove(conversationId);
                var huifiedMessage = Huify.HuifyMe(messageText);
                if (string.IsNullOrEmpty(huifiedMessage)) return;

                var reply = activity.CreateReply();
                reply.Text = huifiedMessage;
                reply.Locale = "ru";

                await Connector.Conversations.ReplyToActivityAsync(reply);
            }
        }
    }
}