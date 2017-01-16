using System;
using System.IO;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace UKLepraBot
{
    public class WebApiApplication : HttpApplication
    {
        private static ChatSettings _chatSettings;

        public static ChatSettings ChatSettings
        {
            get
            {
                if (_chatSettings == null)
                {
                    LoadChatSettings();
                }

                return _chatSettings;
            }
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        public override void Dispose()
        {
            var botSettingsString = JsonConvert.SerializeObject(ChatSettings);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/chatsettings.json", botSettingsString);

            base.Dispose();
        }


        private static void LoadChatSettings()
        {
            var settingsString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/chatsettings.json");
            _chatSettings = JsonConvert.DeserializeObject<ChatSettings>(settingsString);

            if (_chatSettings == null)
                _chatSettings = new ChatSettings();
        }
    }
}