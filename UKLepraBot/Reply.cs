using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;

namespace UKLepraBot
{
    public class Reply
    {
        public string Text { get; set; }
        public string Sticker { get; set; }

        public Activity CreateReplyActivity(Activity activity, bool replyToUser = false)
        {
            var reply = activity.CreateReply();

            if (!string.IsNullOrEmpty(Text))
            {
                var replyText = Text;

                if (replyToUser)
                {
                    var user = activity.From?.Name;
                    replyText = (!string.IsNullOrEmpty(user) ? $"@{user} {Text}" : $"{Text}");
                }
                
                reply.Text = replyText;
            }
            /* Uncomment me if Microsoft fixes Mention 
             if (!string.IsNullOrEmpty(Text))
            {
                var replyText = Text;

                if (replyToUser)
                {
                    var entity = new Entity();
                    entity.SetAs(new Mention
                    {
                        Text = replyText,
                        Mentioned = new ChannelAccount
                        {
                            Name = activity.From?.Name,
                            Id = activity.From?.Id
                        }
                    });

                    reply.Entities.Add(entity);
                }
            }*/
            else if (!string.IsNullOrEmpty(Sticker))
            {
                var channelData = new JsonModels.ChannelData
                {
                    method = "sendSticker",
                    parameters = new JsonModels.Parameters
                    {
                        sticker = Sticker
                    }
                };
                reply.ChannelData = channelData;
            }
            else
            {
                return null;
            }

            return reply;
        }
    }
}