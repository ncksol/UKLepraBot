using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace UKLepraBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private const string TelegramBotId = "ukleprabot";
        private static bool _isActive = true;
        private static Random _rnd = new Random();

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    await HandleMessage(activity);
                }
                else if (activity.Type == ActivityTypes.ConversationUpdate)
                {
                    await HandleConversationUpdate(activity);
                }
                else
                {
                    HandleSystemMessage(activity);
                }
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
            var messageText = Convert.ToString(activity.Text);
            var conversationId = activity.Conversation.Id;
            if (string.IsNullOrEmpty(messageText)) return;

            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));           

            if (MentionsId(activity, TelegramBotId) && messageText.ToLower().Contains("/delay"))
            {
                var delayAdapter = new DelayAdapter(connector);
                await delayAdapter.Process(activity);
            }
            else if (MentionsId(activity, TelegramBotId) && (messageText.ToLower().Contains("/status") || messageText.ToLower().Contains("/huify") || messageText.ToLower().Contains("/unhuify")))
            {
                var delayAdapter = new StatusAdapter(connector);
                await delayAdapter.Process(activity);
            }
            else if (messageText.ToLower().Contains("слава роботам"))
            {
                var reply = activity.CreateReply();

                var foo = _rnd.Next();
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
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (messageText.ToLower().Contains("брексит") || messageText.ToLower().Contains("брекзит") || messageText.ToLower().Contains("brexit"))
            {
                var reply = activity.CreateReply();

                var foo = _rnd.Next(0, 2);
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
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                var state = WebApiApplication.ChatSettings.State;
                var delay = WebApiApplication.ChatSettings.Delay;
                var delaySettings = WebApiApplication.ChatSettings.DelaySettings;
                if (state.ContainsKey(conversationId) && state[conversationId])
                {
                    if (delay.ContainsKey(conversationId))
                    {
                        delay[conversationId] -= 1;
                    }
                    else
                    {
                        Tuple<int, int> delaySetting;
                        if (delaySettings.TryGetValue(conversationId, out delaySetting))
                        {
                            delay[conversationId] = _rnd.Next(delaySetting.Item1, delaySetting.Item2 + 1);
                        }
                        else
                        {
                            delay[conversationId] = _rnd.Next(4);
                        }
                    }

                    if (delay[conversationId] == 0)
                    {
                        delay.Remove(conversationId);
                        var huifiedMessage = Huify.HuifyMe(messageText);
                        if (!String.IsNullOrEmpty(huifiedMessage))
                        {
                            var reply = activity.CreateReply();
                            reply.Text = huifiedMessage;
                            reply.Locale = "ru";
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                }
            }            
        }

        private async Task<Activity> HandleConversationUpdate(Activity activity)
        {
            if (!_isActive) return null;

            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity.MembersAdded != null && activity.MembersAdded.Any())
            {
                var tBot = new ChannelAccount
                {
                    Id = TelegramBotId,
                    Name = TelegramBotId,
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

        private bool MentionsId(Activity activity, string id)
        {
            return activity.Text.Contains($"@{id}");
        }

        private async void ReportException(Exception exception)
        {
            var tBot = new ChannelAccount
            {
                Id = TelegramBotId,
                Name = TelegramBotId,
            };

            var managementChatId = ConfigurationManager.AppSettings["ManagementChatId"];
            var managementChatName = ConfigurationManager.AppSettings["ManagementChatName"];

            var reply = new Activity
            {
                From = tBot,
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount(true, managementChatId, managementChatName),
                Timestamp = DateTime.Now,
                Text = $"Exception! {exception.Message} || {exception.StackTrace}"
            };

            var connector = new ConnectorClient(new Uri("https://telegram.botframework.com"));

            await connector.Conversations.SendToConversationAsync(reply);
        }
    }

    public class Sticker
    {
        public string url { get; set; }
        public string mediaType { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string file_id { get; set; }
    }

    public class Parameters
    {
        public string sticker { get; set; }
    }

    public class ChannelData
    {
        public string method { get; set; }
        public Parameters parameters { get; set; }
    }
}
