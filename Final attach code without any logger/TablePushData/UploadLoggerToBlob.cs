using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace TableAttach
{
    class UploadLoggerToBlob
    {
        public static async Task AzureBlobStorage(LogInfo log)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=storageacc05;AccountKey=TjxKqK5zwLX8FATPncCKD5MmyjXVABmReqf2nBbAL2XLni8Ajo2Qml4PYEVnHP0zD2bauuMtnGLx+AStnWEJ1Q==;EndpointSuffix=core.windows.net";
            string containerName = "blobcontainer";
            var serviceClient = new BlobServiceClient(connectionString);
            var containerClient = serviceClient.GetBlobContainerClient(containerName);
            var fileName = log.Method_Name+".txt";
            var jsondata = JsonConvert.SerilizeObject(log);
            await File.WriteAllTextAsync(fileName,jsondata);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(uploadFileStream, true);

        }

    }

}
