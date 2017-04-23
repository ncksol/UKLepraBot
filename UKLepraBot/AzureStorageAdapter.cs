using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace UKLepraBot
{
    public class AzureStorageAdapter
    {
        private readonly CloudBlobContainer _settingsContainer;

        public AzureStorageAdapter()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));            
            var blobClient = storageAccount.CreateCloudBlobClient();
            _settingsContainer = blobClient.GetContainerReference("settings");
            _settingsContainer.CreateIfNotExists();
        }
        
        public string ReadBlobFromSettings(string blobName)
        {
            var blockBlob = _settingsContainer.GetBlockBlobReference(blobName);

            if (!blockBlob.Exists()) return string.Empty;

            return blockBlob.DownloadText();
        }

        public void SaveBlobToSettings(string blobName, string content)
        {
            var blockBlob = _settingsContainer.GetBlockBlobReference(blobName);            
            blockBlob.UploadText(content);
        }
    }
}