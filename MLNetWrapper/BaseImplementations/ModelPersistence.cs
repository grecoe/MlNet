using System;
using System.IO;
using Microsoft.ML;
using MLNetWrapper.Interfaces;
using MLNetWrapper.Storage;

namespace MLNetWrapper.BaseImplementations
{
    /// <summary>
    /// Base implementation of IModelPersistence
    /// </summary>
    public class ModelPersistence : IModelPersistence
    {
        #region Private Properties
        /// <summary>
        /// Internal property of where the downloaded model file is. For AzureStorage this 
        /// is utilized to point to the TempDirectory, which that file will be cleared as soon
        /// as possible so not to have multiple versions on the system. 
        /// </summary>
        private String DownloadedVersion { get; set; }
        /// <summary>
        /// The instance of the model. 
        /// </summary>
        private ITransformer Model { get; set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// Instance of IModelLocation identifying where the model lives.
        /// </summary>
        public IModelLocation ModelLocation { get; set; }
        #endregion

        public ModelPersistence(IModelLocation location)
        {
            this.ModelLocation = location;
        }

        /// <summary>
        /// Used to set the internal model file when first created or retrained 
        /// with an instance of IModellingBase so that the correct model is 
        /// persisted.
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(ITransformer model)
        {
            this.Model = model;
        }

        /// <summary>
        /// Loads the model from the desired location.
        /// </summary>
        /// <param name="context">An instance of MLContext</param>
        /// <returns>An instance of the model.</returns>
        public ITransformer LoadModel(MLContext context)
        {
            if (this.Model == null)
            {

                if (this.ModelLocation == null)
                {
                    throw new ArgumentNullException("ModelLocation");
                }
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                if (this.ModelLocation is LocalFileConfiguration)
                {
                    String path = System.IO.Path.Combine(
                        (this.ModelLocation as LocalFileConfiguration).LocalDirectory,
                        (this.ModelLocation as LocalFileConfiguration).FileName);

                    this.Model = this.LoadFile(context, path);
                }
                else if (this.ModelLocation is AzureStorageFileConfiguration)
                {
                    String blob = (this.ModelLocation as AzureStorageFileConfiguration).FileName;
                    String container = (this.ModelLocation as AzureStorageFileConfiguration).StorageContainer;
                    String connection = (this.ModelLocation as AzureStorageFileConfiguration).StorageConnectionString;

                    this.DownloadedVersion = System.IO.Path.Combine(Path.GetTempPath(), blob);
                    this.Clear();

                    bool dowloadComplete = StorageHelper.DownloadBlob(connection, container, blob, this.DownloadedVersion).Result;
                    if (dowloadComplete)
                    {
                        this.Model = this.LoadFile(context, this.DownloadedVersion);
                    }
                    this.Clear();
                }
            }

            return this.Model;
        }

        /// <summary>
        /// Saves the internal model to the desired location.
        /// </summary>
        /// <param name="context">An instance of MLContext</param>
        /// <returns>True if persisted.</returns>
        public bool SaveModel(MLContext context)
        {
            if (this.Model == null)
            {
                throw new ArgumentNullException("Model");
            }
            if (this.ModelLocation == null)
            {
                throw new ArgumentNullException("ModelLocation");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool returnValue = false;
            if (this.ModelLocation is LocalFileConfiguration)
            {
                String path = System.IO.Path.Combine(
                    (this.ModelLocation as LocalFileConfiguration).LocalDirectory,
                    (this.ModelLocation as LocalFileConfiguration).FileName);

                this.SaveModelToDisk(context, path);
                returnValue = true;
            }
            else if (this.ModelLocation is AzureStorageFileConfiguration)
            {
                String blob = (this.ModelLocation as AzureStorageFileConfiguration).FileName;
                String container = (this.ModelLocation as AzureStorageFileConfiguration).StorageContainer;
                String connection = (this.ModelLocation as AzureStorageFileConfiguration).StorageConnectionString;

                this.DownloadedVersion = System.IO.Path.Combine(Path.GetTempPath(), blob);
                this.Clear();

                this.SaveModelToDisk(context, this.DownloadedVersion);
                bool uploadComplete = StorageHelper.UploadBlob(connection, container, blob, this.DownloadedVersion).Result;
                if (uploadComplete)
                {
                    returnValue = true;
                }
                this.Clear();
            }

            return returnValue;
        }

        #region Private Methods
        /// <summary>
        /// If a file was downloaded, delete it from the local system.
        /// </summary>
        private void Clear()
        {
            if (!String.IsNullOrEmpty(this.DownloadedVersion) && System.IO.File.Exists(this.DownloadedVersion))
            {
                System.IO.File.Delete(this.DownloadedVersion);
            }
        }

        /// <summary>
        /// Load the model from a local file.
        /// </summary>
        /// <param name="context">Required MLContext</param>
        /// <param name="path">Path to the local file containing the model.</param>
        /// <returns>An instance of the model.</returns>
        private ITransformer LoadFile(MLContext context, string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return context.Model.Load(stream);
            }
        }

        /// <summary>
        /// Write the model to disk. Create directory if needed.
        /// </summary>
        /// <param name="context">Required MLContext</param>
        /// <param name="localPath">Path to store the model. </param>
        private void SaveModelToDisk(MLContext context, string localPath)
        {
            String directory = System.IO.Path.GetDirectoryName(localPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            if(System.IO.File.Exists(localPath))
            {
                System.IO.File.Delete(localPath);
            }


            using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                context.Model.Save(this.Model, fs);
            }
        }
        #endregion
    }
}
