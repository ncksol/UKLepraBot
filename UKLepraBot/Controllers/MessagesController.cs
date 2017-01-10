using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        private const string BotId = "ukleprabot";
        private static bool _isActive = true;
        private static bool _isEchoMode;
        private static Random _rnd = new Random();
        private static Dictionary<string, Tuple<int, int>> _delaySettings = new Dictionary<string, Tuple<int, int>>();
        private static Dictionary<string, int> _delay = new Dictionary<string, int>();
        private static Dictionary<string, bool> _state = new Dictionary<string, bool>();
        
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
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
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleMessage(Activity activity)
        {          
            var messageText = Convert.ToString(activity.Text);
            var conversationId = activity.Conversation.Id;
            if (string.IsNullOrEmpty(messageText)) return;

            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity.MentionsId(BotId) && messageText.ToLower().Contains("/delay"))
            {
                /*await Conversation.SendAsync(activity, delegate()
                {
                    var chatId = Convert.ToInt64(activity.Conversation.Id);

                    var delayDialogue = new DelayDialogue() {ChatId = chatId};                    
                    return delayDialogue;
                });*/
                var reply = activity.CreateReply();
                reply.Locale = "ru";
                var messageParts = messageText.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                if (messageParts.Length == 1)
                {
                    var currentDelay = new Tuple<int, int>(0, 4);
                    if(_delaySettings.ContainsKey(conversationId))
                        currentDelay = _delaySettings[conversationId];

                    reply.Text = $"Сейчас я пропускаю случайное число сообщений от {currentDelay.Item1} до {currentDelay.Item2}";
                }
                else if (messageParts.Length == 2)
                {
                    int newMaxDelay;
                    if (!int.TryParse(messageParts[1], out newMaxDelay))
                    {
                        reply.Text = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                    }
                    else
                    {
                        _delaySettings[conversationId] = new Tuple<int, int>(0, newMaxDelay);
                        reply.Text = $"Я буду пропускать случайное число сообщений от 0 до {newMaxDelay}";
                    }
                }
                else if (messageParts.Length == 3)
                {
                    int newMaxDelay;
                    int newMinDelay;
                    if (!int.TryParse(messageParts[2], out newMaxDelay))
                    {
                        reply.Text = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                    }
                    else if (!int.TryParse(messageParts[1], out newMinDelay))
                    {
                        reply.Text = "Неправильный аргумент, отправьте /delay N [[M]], где N, M любое натуральное число";
                    }
                    else
                    {
                        if (newMinDelay == newMaxDelay)
                        {
                            newMinDelay = 0;                            
                        }
                        else if (newMinDelay > newMaxDelay)
                        {
                            var i = newMinDelay;
                            newMinDelay = newMaxDelay;
                            newMaxDelay = i;
                        }

                        _delaySettings[conversationId] = new Tuple<int, int>(newMinDelay, newMaxDelay);
                        reply.Text = $"Я буду пропускать случайное число сообщений от {newMinDelay} до {newMaxDelay}";
                    }
                }
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (activity.MentionsId(BotId) && messageText.ToLower().Contains("/huify"))
            {
                var reply = activity.CreateReply();
                reply.Text = "Хуятор успешно активирован.";
                reply.Locale = "ru";
                await connector.Conversations.ReplyToActivityAsync(reply);
                _state[conversationId] = true;
            }
            else if (activity.MentionsId(BotId) && messageText.ToLower().Contains("/unhuify"))
            {
                var reply = activity.CreateReply();
                reply.Text = "Хуятор успешно деактивирован.";
                reply.Locale = "ru";
                await connector.Conversations.ReplyToActivityAsync(reply);
                _state[conversationId] = false;
            }
            else if (messageText.ToLower().Contains("слава роботам"))
            {
                var reply = activity.CreateReply();

                var foo = _rnd.Next();
                if (foo%2 == 0)
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
            }/*
            else if(messageText.ToLower().Contains("/secret") && activity.Conversation.Id == "178846839")
            {
                
            }*/
            else if(_state.ContainsKey(conversationId) && _state[conversationId])
            {
                if (_delay.ContainsKey(conversationId))
                    _delay[conversationId] -= 1;
                else
                {
                    Tuple<int, int> delaySetting;
                    if (_delaySettings.TryGetValue(conversationId, out delaySetting))
                    {
                        _delay[conversationId] = _rnd.Next(delaySetting.Item1, delaySetting.Item2 + 1);
                    }
                    else
                    {
                        _delay[conversationId] = _rnd.Next(4);
                    }
                }

                if (_delay[conversationId] == 0)
                {
                    _delay.Remove(conversationId);
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

            /*  
            if (messageText.Contains("/start"))
            {
                _isActive = true;
                var reply = activity.CreateReply("Bot is active.");
                await Connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (messageText.Contains("/stop"))
            {
                _isActive = false;
                var reply = activity.CreateReply("Bot is inactive.");
                await Connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (messageText.Contains("/echo"))
            {
                _isEchoMode = !_isEchoMode;
                var reply = activity.CreateReply("Echo mode is " + (_isEchoMode ? "active" : "inactive") + ".");
                await Connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (messageText.Contains("/status"))
            {
                var reply = activity.CreateReply("Bot is "+ (_isActive ? "active":"inactive") +". Echo mode is " + (_isEchoMode ? "active" : "inactive") + ".");
                await Connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (_isEchoMode)
            {
                var reply = activity.CreateReply(messageText);
                await Connector.Conversations.ReplyToActivityAsync(reply);
            }*/

            return;
        }

        private async Task<APIResponse> HandleConversationUpdate(Activity activity)
        {
            if (!_isActive) return null;

            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity.MembersAdded != null && activity.MembersAdded.Any())
            {
                var tBot = new ChannelAccount
                {
                    Id = BotId,
                    Name = BotId,
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
