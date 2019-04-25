using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MLNetWrapper.Storage
{
    class StorageHelper
    {
        public static async Task<bool> DownloadBlob(String connectionString, String container, String blob, String localFile)
        {
            bool returnValue = false;
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(container);

                bool containerExists = await cloudBlobContainer.ExistsAsync();
                if (containerExists)
                {
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blob);
                    bool blobExists = await cloudBlockBlob.ExistsAsync();
                    if (blobExists)
                    {
                        await cloudBlockBlob.DownloadToFileAsync(localFile, FileMode.Create);
                        returnValue = true;
                    }
                }

            }

            return returnValue;
        }

        public static async Task<bool> UploadBlob(String connectionString, String container, String blob, String localFile)
        {
            bool returnValue = false;
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(container);

                bool containerExists = await cloudBlobContainer.ExistsAsync();

                if (!containerExists)
                {
                    await cloudBlobContainer.CreateAsync();

                    // Set the permissions so the blobs are public. 
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);
                }

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blob);
                await cloudBlockBlob.UploadFromFileAsync(localFile);

                returnValue = true;
            }

            return returnValue;
        }
    }
}
