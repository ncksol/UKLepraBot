using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using UKLepraBot.MessageAdapters;

namespace UKLepraBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        ///     POST: api/Messages
        ///     Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                    await HandleMessage(activity);
                else if (activity.Type == ActivityTypes.ConversationUpdate)
                    await HandleConversationUpdate(activity);
                else
                    HandleSystemMessage(activity);
            }
            catch (Exception e)
            {
                ReportException(e);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleMessage(Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var messageAdapterFactory = new MessageAdapterFactory(connector);

            var messageAdapter = messageAdapterFactory.CreateAdapter(activity);
            await messageAdapter.Process(activity);
        }

        private async Task<Activity> HandleConversationUpdate(Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity.MembersAdded != null && activity.MembersAdded.Any())
            {
                var tBot = new ChannelAccount
                {
                    Id = WebApiApplication.TelegramBotId,
                    Name = WebApiApplication.TelegramBotName
                };
                var name = string.Empty;
                if (activity.MembersAdded != null && activity.MembersAdded.Any())
                    name = activity.MembersAdded.First().Name;

                var reply = activity.CreateReply();
                reply.From = tBot;

                if (!string.IsNullOrEmpty(name))
                    reply.Text = $"@{name} ты вообще с какого посткода?";
                else
                    reply.Text = "А ты вообще с какого посткода?";

                await connector.Conversations.SendToConversationAsync(reply);
            }
            else if (activity.MembersRemoved != null && activity.MembersRemoved.Any())
            {
                var tBot = new ChannelAccount
                {
                    Id = WebApiApplication.TelegramBotId,
                    Name = WebApiApplication.TelegramBotName
                };

                var reply = activity.CreateReply();
                reply.From = tBot;

                var channelData = new JsonModels.ChannelData
                {
                    method = "sendSticker",
                    parameters = new JsonModels.Parameters
                    {
                        sticker = "CAADAgADXgEAAhmGAwABgntLLoS0m94C"
                    }
                };
                reply.ChannelData = channelData;

                await connector.Conversations.SendToConversationAsync(reply);
            }

            return null;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async void ReportException(Exception exception)
        {
            //var tBot = new ChannelAccount
            //{
            //    Id = WebApiApplication.TelegramBotId,
            //    Name = WebApiApplication.TelegramBotName
            //};

            //var managementChatId = ConfigurationManager.AppSettings["ManagementChatId"];

            //var reply = new Activity
            //{
            //    From = tBot,
            //    Type = ActivityTypes.Message,
            //    Conversation = new ConversationAccount(true, managementChatId),
            //    Timestamp = DateTime.Now,
            //    Text = $"Exception! {exception.Message} || {exception.StackTrace}"
            //};

            //var connector = new ConnectorClient(new Uri("https://telegram.botframework.com"));

            //await connector.Conversations.SendToConversationAsync(reply);

            System.Diagnostics.Trace.TraceError(exception.Message);
            System.Diagnostics.Trace.TraceError(exception.StackTrace);
        }

    }

}