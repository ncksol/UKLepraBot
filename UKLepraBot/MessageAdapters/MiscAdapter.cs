using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class MiscAdapter : MessageAdapterBase
    {
        public MiscAdapter(IConnectorClient connector):base(connector)
        {
        }

        public override async Task Process(Activity activity)
        {
            var messageText = activity.Text;

            var reply = activity.CreateReply();
            reply.Locale = "ru";
            if (messageText.ToLower().Contains("слава роботам"))
            {
                var foo = Rnd.Next();
                if (foo % 2 == 0)
                {
                    var user = activity.From?.Name;
                    reply.Text = (!string.IsNullOrEmpty(user) ? $"@{user} " : "") + "Воистину слава!";
                    reply.Locale = "ru";
                }
                else
                {
                    var channelData = new ChannelData
                    {
                        method = "sendSticker",
                        parameters = new Parameters
                        {
                            sticker = "BQADBAADHQADmDVxAh2h6gc7L-sLAg"
                        }
                    };
                    reply.ChannelData = channelData;
                }
            }
            else if (messageText.ToLower().Contains("брексит") || messageText.ToLower().Contains("брекзит") || messageText.ToLower().Contains("brexit"))
            {
                var foo = Rnd.Next(0, 2);
                var sticker = string.Empty;
                if (foo == 0)
                    sticker = "BQADBAADgwADe9a5BsXsWEDlCe3bAg";
                else if (foo == 1)
                    sticker = "BQADBAADiQADe9a5BiskCu-ELBwsAg";
                else if (foo == 2)
                    sticker = "BQADBAADiwADe9a5BlEg69510dKIAg";

                var channelData = new ChannelData
                {
                    method = "sendSticker",
                    parameters = new Parameters
                    {
                        sticker = sticker
                    }
                };
                reply.ChannelData = channelData;
            }

            await Connector.Conversations.ReplyToActivityAsync(reply);
        }
    }
}