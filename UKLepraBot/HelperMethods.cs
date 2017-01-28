using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UKLepraBot
{
    public class HelperMethods
    {
        public static bool YesOrNo()
        {
            var rnd = new Random();
            return rnd.Next() % 2 == 0;
        }

        public static bool MentionsId(Activity activity, string id)
        {
            var channelData = (JObject)activity.ChannelData;
            var messageData = JsonConvert.DeserializeObject<JsonModels.Message>(channelData["message"].ToString());

            if (messageData?.reply_to_message?.@from?.username == WebApiApplication.TelegramBotName)
                return true;

            if (string.IsNullOrEmpty(activity.Text)) return false;

            return activity.Text.Contains($"@{id}");
        }

        public static bool MentionsBot(Activity activity)
        {
            return MentionsId(activity, WebApiApplication.TelegramBotId);
        }
    }
}