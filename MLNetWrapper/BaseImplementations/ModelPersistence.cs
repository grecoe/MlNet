//
// Copyright  Microsoft Corporation ("Microsoft").
//
// Microsoft grants you the right to use this software in accordance with your subscription agreement, if any, to use software 
// provided for use with Microsoft Azure ("Subscription Agreement").  All software is licensed, not sold.  
// 
// If you do not have a Subscription Agreement, or at your option if you so choose, Microsoft grants you a nonexclusive, perpetual, 
// royalty-free right to use and modify this software solely for your internal business purposes in connection with Microsoft Azure 
// and other Microsoft products, including but not limited to, Microsoft R Open, Microsoft R Server, and Microsoft SQL Server.  
// 
// Unless otherwise stated in your Subscription Agreement, the following applies.  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT 
// WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL MICROSOFT OR ITS LICENSORS BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE SAMPLE CODE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

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
