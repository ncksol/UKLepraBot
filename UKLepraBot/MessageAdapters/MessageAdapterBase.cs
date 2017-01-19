using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public abstract class MessageAdapterBase
    {
        protected IConnectorClient Connector;
        protected Random Rnd;

        protected MessageAdapterBase(IConnectorClient connector)
        {
            Connector = connector;
            Rnd = new Random();
        }

        public abstract Task Process(Activity activity);
    }
}