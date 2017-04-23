using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace UKLepraBot
{
    public class ReactionsManager
    {
        public ReactionsList Reactions { get; private set; } = new ReactionsList();
        
        public ReactionsManager()
        {
            ReloadReactions();
        }

        public void ReloadReactions()
        {
            var reactionsJsonString = WebApiApplication.AzureStorageAdapter.ReadBlobFromSettings("reactions.json");
            Reactions = JsonConvert.DeserializeObject<ReactionsList>(reactionsJsonString);
        }
    }
}