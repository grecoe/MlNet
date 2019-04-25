using System;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Interface describing the location of the model file.
    /// </summary>
    public interface IModelLocation
    {
        /// <summary>
        /// Whether local or blob storage, this is the file name.
        /// </summary>
        String FileName { get; }
        /// <summary>
        /// For a local file, contains the directory for the model file.
        /// </summary>
        String LocalDirectory { get; }
        /// <summary>
        /// For Azure Storage, contains the storage account connection string/
        /// </summary>
        String StorageConnectionString { get; }
        /// <summary>
        /// For Azure Storage contains the storage container name.
        /// </summary>
        String StorageContainer { get; }
    }
}
