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
}