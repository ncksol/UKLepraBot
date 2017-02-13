using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UKLepraBot
{
    public class ActivatorsManager
    {
        private readonly string[] _brexitActivators = { "брексит", "брекзит", "brexit" };
        private readonly string[] _politiciansActivators = { "путин", "корбин", "навальный" };
        private readonly string[] _robotActivators = { "слава роботам" };
        public string[] RudeActivators { get; private set; } = {"пошел нахуй", "иди нахуй", "охуел", "пошёл нахуй", "нахуй пойди"};
        private readonly string[] _kissingEmojiActivators = { "😘", "😚", "😍" };

        public ActivatorsManager()
        {
            ReloadActivators();
        }

        public void ReloadActivators()
        {
            var rudeActivatorsString = WebApiApplication.AzureStorageAdapter.ReadBlobFromSettings("rudeActivators.txt").Trim(Environment.NewLine.ToCharArray());
            if (!string.IsNullOrEmpty(rudeActivatorsString))
            {
                var rudeActivators = rudeActivatorsString.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (rudeActivators.Length > 0)
                    RudeActivators = rudeActivators;
            }
        }
    }
}