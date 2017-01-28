using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Http;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace UKLepraBot
{
    public class WebApiApplication : HttpApplication
    {
        private static ChatSettings _chatSettings;
        private static AzureStorageAdapter _azureStorageAdapter;
        private static DateTimeOffset _startupTime;

        public static string TelegramBotId;
        public static string TelegramBotName;
        public static string TelegramBotNumber;

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

        public static DateTimeOffset StartupTime
        {
            get { return _startupTime; }
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            _startupTime = DateTimeOffset.UtcNow;
        }

        public override void Init()
        {
            base.Init();
            TelegramBotId = ConfigurationManager.AppSettings["TelegramBotId"];
            TelegramBotName = ConfigurationManager.AppSettings["TelegramBotName"];
            TelegramBotNumber = ConfigurationManager.AppSettings["TelegramBotNumber"];

#if !DEBUG
            _azureStorageAdapter = new AzureStorageAdapter();
#endif
        }

        public override void Dispose()
        {
#if !DEBUG
            var botSettingsString = JsonConvert.SerializeObject(ChatSettings);
            _azureStorageAdapter.SaveBlobToSettings("chatsettings.json", botSettingsString);
#endif
            base.Dispose();
        }

        private static void LoadChatSettings()
        {
#if !DEBUG
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("settings");
            container.CreateIfNotExists();

            var settingsString = _azureStorageAdapter.ReadBlobFromSettings("chatsettings.json");
            _chatSettings = JsonConvert.DeserializeObject<ChatSettings>(settingsString);
#endif
            if (_chatSettings == null)
                _chatSettings = new ChatSettings();
        }
    }
}