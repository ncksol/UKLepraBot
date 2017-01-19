using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class MessageAdapterFactory
    {
        private readonly IConnectorClient _connectorClient;
        public static readonly List<string> DelayAdapterActivators = new List<string> {"/delay"};
        public static readonly List<string> StatusAdapterActivators = new List<string> {"/status", "/huify", "/unhuify", "/uptime" };
        public static readonly List<string> MiscAdapterActivators = new List<string> { "слава роботам", "брексит", "брекзит", "brexit" };

        public MessageAdapterFactory(IConnectorClient connectorClient)
        {
            _connectorClient = connectorClient;
        }

        public MessageAdapterBase CreateAdapter(Activity activity)
        {
            var messageText = Convert.ToString(activity.Text);

            if (DelayAdapterActivators.Any(x => messageText.ToLower().Contains(x)))
                return new DelayAdapter(_connectorClient);

            if (StatusAdapterActivators.Any(x => messageText.ToLower().Contains(x)))
                return new StatusAdapter(_connectorClient);

            if (MiscAdapterActivators.Any(x => messageText.ToLower().Contains(x)))
                return new MiscAdapter(_connectorClient);

            return new HuifyAdapter(_connectorClient);
        }

    }
}