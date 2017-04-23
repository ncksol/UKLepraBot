using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKLepraBot.Lab
{
    class UpdateActivators
    {
        public static void Go()
        {
            var storageAdapter = new AzureStorageAdapter();
            var fileInfo = new FileInfo("activators.json");
            string content;
            using (var stream = fileInfo.OpenText())
            {
                content = stream.ReadToEnd();
            }
            storageAdapter.SaveBlobToSettings("activators.json", content);
        }
    }
}
