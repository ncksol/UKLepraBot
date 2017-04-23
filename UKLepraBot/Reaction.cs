using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UKLepraBot
{
    public class Reaction
    {
        public List<string> Triggers { get; set; } = new List<string>();
        public List<Reply> Replies { get; set; } = new List<Reply>();
        public bool IsAlwaysReply { get; set; }
        public bool IsMentionReply { get; set; }

        public Reply GetReply()
        {
            if (Replies.Count <= 1)
                return Replies.FirstOrDefault();

            return Replies[HelperMethods.RandomInt(Replies.Count)];
        }
    }

    public class ReactionsList
    {
        public List<Reaction> Items { get; set; }

        public Reaction GetReaction(Activity activity)
        {
            var messageText = activity.Text?.ToLower() ?? string.Empty;
            //var channelData = (JObject)activity.ChannelData;
            //var messageData = JsonConvert.DeserializeObject<JsonModels.Message>(channelData["message"].ToString());
            
            var reaction = Items.FirstOrDefault(x => x.Triggers.Any(messageText.Contains));

            if (reaction == null) return null;
            if (reaction.IsMentionReply && !HelperMethods.MentionsBot(activity)) return null;
            if (!reaction.IsAlwaysReply && !HelperMethods.YesOrNo()) return null;

            return reaction;            
        }
    }
}