using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class MessageAdapterFactory
    {
        private readonly IConnectorClient _connectorClient;
        public static readonly List<string> CommandAdapterActivators = new List<string> {"/status", "/huify", "/unhuify", "/uptime", "/delay", "/secret", "/reload", "/lmgtfy" };

        public MessageAdapterFactory(IConnectorClient connectorClient)
        {
            _connectorClient = connectorClient;
        }

        public MessageAdapterBase CreateAdapter(Activity activity)
        {
            if (HelperMethods.MentionsBot(activity) && !String.IsNullOrEmpty(activity.Text) && CommandAdapterActivators.Any(x => activity.Text.ToLower().Contains(x)))
                return new CommandAdapter(_connectorClient);

            return new MessageAdapter(_connectorClient);
        }

    }
}