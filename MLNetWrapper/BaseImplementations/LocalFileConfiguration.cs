using System;
using MLNetWrapper.Interfaces;

namespace MLNetWrapper.BaseImplementations
{
    /// <summary>
    /// Base implementation for IModelLocation for local model files.
    /// </summary>
    public class LocalFileConfiguration : IModelLocation
    {
        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Local system directory 
        /// </summary>
        public string LocalDirectory { get; private set; }
        /// <summary>
        /// Not relevant for this type
        /// </summary>
        public string StorageConnectionString { get => throw new NotImplementedException(); }
        /// <summary>
        /// Not relevant for this type
        /// </summary>
        public string StorageContainer { get => throw new NotImplementedException(); }

        public LocalFileConfiguration(String fileName, String directory)
        {
            this.FileName = fileName;
            this.LocalDirectory = directory;
        }
    }
}
