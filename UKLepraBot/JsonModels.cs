using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UKLepraBot
{
    public class JsonModels
    {
        public class Sticker
        {
            public string url { get; set; }
            public string mediaType { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string emoji { get; set; }
            public Thumb thumb { get; set; }
            public string file_id { get; set; }
            public int file_size { get; set; }
            public string file_path { get; set; }
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

        public class From
        {
            public long id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string username { get; set; }
        }

        public class Chat
        {
            public long id { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public bool all_members_are_administrators { get; set; }
        }
        
        public class ReplyToMessage
        {
            public long message_id { get; set; }
            public From from { get; set; }
            public Chat chat { get; set; }
            public int date { get; set; }
            public string text { get; set; }
        }

        public class Thumb
        {
            public string file_id { get; set; }
            public int file_size { get; set; }
            public string file_path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }
        public class Message
        {
            public long message_id { get; set; }
            public From from { get; set; }
            public Chat chat { get; set; }
            public int date { get; set; }
            public ReplyToMessage reply_to_message { get; set; }
            public Sticker sticker { get; set; }
        }

    }
}