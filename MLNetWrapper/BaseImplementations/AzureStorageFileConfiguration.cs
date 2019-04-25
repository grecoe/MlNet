using System;
using MLNetWrapper.Interfaces;

namespace MLNetWrapper.BaseImplementations
{
    /// <summary>
    /// Base implementation for IModelLocation for Azure Blob Storage model files..
    /// </summary>
    public class AzureStorageFileConfiguration : IModelLocation
    {
        /// <summary>
        /// Blob name
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Not relevant for this type
        /// </summary>
        public string LocalDirectory => throw new NotImplementedException();
        /// <summary>
        /// Contains the storage account connection string/
        /// </summary>
        public string StorageConnectionString { get; private set; }
        /// <summary>
        /// Contains the storage container name.
        /// </summary>
        public string StorageContainer { get; private set; }

        public AzureStorageFileConfiguration(string file, string container, string connectionString)
        {
            this.FileName = file;
            this.StorageContainer = container;
            this.StorageConnectionString = connectionString;
        }
        
    }
}
