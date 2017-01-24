using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class MessageAdapterFactory
    {
        private readonly IConnectorClient _connectorClient;
        public static readonly List<string> CommandAdapterActivators = new List<string> {"/status", "/huify", "/unhuify", "/uptime", "/delay", "/secret" };

        public MessageAdapterFactory(IConnectorClient connectorClient)
        {
            _connectorClient = connectorClient;
        }

        public MessageAdapterBase CreateAdapter(Activity activity)
        {
            var messageText = Convert.ToString(activity.Text);

            if (MentionsId(activity, WebApiApplication.TelegramBotId) && CommandAdapterActivators.Any(x => messageText.ToLower().Contains(x)))
                return new CommandAdapter(_connectorClient);

            return new MessageAdapter(_connectorClient);
        }

        private bool MentionsId(Activity activity, string id)
        {
            return activity.Text.Contains($"@{id}");
        }
    }
}