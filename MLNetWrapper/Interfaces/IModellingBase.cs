using Microsoft.Data.DataView;
using Microsoft.ML;
using System;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Declaration of the interface that represents the ability to Create, Evaluate,
    /// and save a model. 
    /// </summary>
    public interface IModellingBase
    {
        /// <summary>
        /// File that contains the training data for the model.
        /// </summary>
        String DataFile { get; set; }
        /// <summary>
        /// Identifies where the model is stored.
        /// </summary>
        IModelPersistence ModelPersistence { get; set; }
        /// <summary>
        /// The MLContext object required by ML.NET
        /// </summary>
        MLContext Context { get; set; }
        /// <summary>
        /// An instance of the model.
        /// </summary>
        ITransformer Model { get; set; }

        /// <summary>
        /// Load the training file and split it into Train and Test 
        /// data sets.
        /// </summary>
        /// <returns>TrainCatalogBase.TrainTestData that has the data split into
        /// test and train datasets.</returns>
        TrainCatalogBase.TrainTestData LoadData();

        /// <summary>
        /// Bulds the model using training data.
        /// </summary>
        /// <param name="splitTrainSet">Training view from TrainCatalogBase.TrainTestData</param>
        /// <returns>True if built, false otherwise.</returns>
        bool BuildModel(IDataView splitTrainSet);
        /// <summary>
        /// Evaluates the model (but only prints to the console the results)
        /// </summary>
        /// <param name="splitTestSet">Testing view from TrainCatalogBase.TrainTestData</param>
        void EvaluateModel(IDataView splitTestSet);
        /// <summary>
        /// Save the model to the location identified by ModelPersistence.
        /// </summary>
        void SaveModel();

    }
}
