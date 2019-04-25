using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Represents how the model is saved and loaded from the desired storage 
    /// location.
    /// </summary>
    public interface IModelPersistence
    {
        /// <summary>
        /// Instance of IModelLocation identifying where the model lives.
        /// </summary>
        IModelLocation ModelLocation { get; }

        /// <summary>
        /// Used to set the internal model file when first created or retrained 
        /// with an instance of IModellingBase so that the correct model is 
        /// persisted.
        /// </summary>
        /// <param name="model"></param>
        void SetModel(ITransformer model);

        /// <summary>
        /// Loads the model from the desired location.
        /// </summary>
        /// <param name="context">An instance of MLContext</param>
        /// <returns>An instance of the model.</returns>
        ITransformer LoadModel(MLContext context);

        /// <summary>
        /// Saves the internal model to the desired location.
        /// </summary>
        /// <param name="context">An instance of MLContext</param>
        /// <returns>True if persisted.</returns>
        bool SaveModel(MLContext context);
    }
}
