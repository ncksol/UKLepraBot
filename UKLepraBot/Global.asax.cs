using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace UKLepraBot
{
    public class WebApiApplication : HttpApplication
    {
        private static ChatSettings _chatSettings;

        public static string TelegramBotId;
        public static string TelegramBotName;
        public static string TelegramBotNumber;
        private static AzureStorageAdapter _azureStorageAdapter;
        private static ActivatorsManager _activatorsManager;

        public static AzureStorageAdapter AzureStorageAdapter => _azureStorageAdapter ?? (_azureStorageAdapter = new AzureStorageAdapter());

        public static ActivatorsManager ActivatorsManager => _activatorsManager ?? (_activatorsManager = new ActivatorsManager());

        public static ChatSettings ChatSettings
        {
            get
            {
                if (_chatSettings == null)
                    LoadChatSettings();

                return _chatSettings;
            }
        }

        public static DateTimeOffset StartupTime { get; private set; }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            StartupTime = DateTimeOffset.UtcNow;
        }

        public override void Init()
        {
            base.Init();
            TelegramBotId = ConfigurationManager.AppSettings["TelegramBotId"];
            TelegramBotName = ConfigurationManager.AppSettings["TelegramBotName"];
            TelegramBotNumber = ConfigurationManager.AppSettings["TelegramBotNumber"];
        }

        public override void Dispose()
        {
#if !DEBUG
            var botSettingsString = JsonConvert.SerializeObject(ChatSettings);
            AzureStorageAdapter.SaveBlobToSettings("chatsettings.json", botSettingsString);
#endif
            base.Dispose();
        }

        private static void LoadChatSettings()
        {
#if !DEBUG
            var settingsString = AzureStorageAdapter.ReadBlobFromSettings("chatsettings.json");
            _chatSettings = JsonConvert.DeserializeObject<ChatSettings>(settingsString);
#endif
            if (_chatSettings == null)
                _chatSettings = new ChatSettings();
        }
    }
}