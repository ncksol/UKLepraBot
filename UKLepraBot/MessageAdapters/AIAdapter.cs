using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Chronic;
using Microsoft.Bot.Connector;

namespace UKLepraBot.MessageAdapters
{
    public class AIAdapter: MessageAdapterBase
    {
        private string[] _rubbish = new[] {".", ",", "-", "=", "#", "!", "?", "%", "@", "\"", "£", "$", "^", "&", "*", "(", ")", "_", "+", "]", "[", "{", "}", ";", ":", "~", "/", "<", ">", };

        public AIAdapter(IConnectorClient connector) : base(connector)
        {
        }

        public override async Task Process(Activity activity)
        {
            var messageText = activity.Text?.ToLower() ?? string.Empty;

            Activity reply = null;

            if (messageText.ToLower().Contains("погугли"))
            {
                reply = GoogleCommand(activity);
            }
            
            if (reply == null) return;

            await Connector.Conversations.ReplyToActivityAsync(reply);
        }

        private Activity GoogleCommand(Activity activity)
        {
            var messageText = activity.Text;

            var activationWord = "погугли";

            var cleanedMessageText = messageText;
            _rubbish.ForEach(x => cleanedMessageText = cleanedMessageText.Replace(x, " "));

            var messageParts = cleanedMessageText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var activationWordPosition = messageParts.FindIndex(x => x.Equals(activationWord));
            if (activationWordPosition == -1 || activationWordPosition > 3) return null;

            var queryParts = messageParts.Skip(activationWordPosition+1);
            if (!queryParts.Any()) return null;

            var reply = activity.CreateReply();
            
            var query = String.Join("%20", queryParts);
            reply.Text = $"http://google.co.uk/search?q={query}";

            return reply;
        }
    }
}