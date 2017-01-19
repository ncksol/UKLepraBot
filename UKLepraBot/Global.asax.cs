using System;
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

        public const string TelegramBotId = "ukleprabot";

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

            _azureStorageAdapter = new AzureStorageAdapter();
        }

        public override void Dispose()
        {
            var botSettingsString = JsonConvert.SerializeObject(ChatSettings);
            _azureStorageAdapter.SaveBlobToSettings("chatsettings.json", botSettingsString);

            base.Dispose();
        }

        private static void LoadChatSettings()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("settings");
            container.CreateIfNotExists();

            var settingsString = _azureStorageAdapter.ReadBlobFromSettings("chatsettings.json");
            _chatSettings = JsonConvert.DeserializeObject<ChatSettings>(settingsString);

            if (_chatSettings == null)
                _chatSettings = new ChatSettings();
        }
    }
}