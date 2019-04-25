using Microsoft.ML;
using System.Collections.Generic;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Declaration of the interface that represents the ability to score records for 
    /// a single model type.
    /// </summary>
    /// <typeparam name="DataClass">Generic classs that identifies the expected model data.</typeparam>
    /// <typeparam name="PredictionClass">Generic class that identifies the model scoring result</typeparam>
    public interface IExecutionBase<DataClass, PredictionClass>
        where DataClass : class, new()
        where PredictionClass : class, new()
    {
        /// <summary>
        /// Identifies where the model is stored.
        /// </summary>
        IModelPersistence ModelPersistence { get; set; }
        /// <summary>
        /// An instance of the model.
        /// </summary>
        ITransformer Model { get; set; }
        /// <summary>
        /// The MLContext object required by ML.NET
        /// </summary>
        MLContext Context { get; set; }
        /// <summary>
        /// Optional ITrainingDataAccumulator to collect results of all scored records.
        /// In a production environment this should be null.
        /// </summary>
        ITraningDataAccumulator<DataClass> Training { get; set; }
        /// <summary>
        /// Load the model identified in the IModelPersistence.
        /// </summary>
        /// <returns>True if the model is loaded</returns>
        bool LoadModel();
        /// <summary>
        /// Score a single record.
        /// </summary>
        /// <param name="record">Record to score</param>
        /// <returns>Result of scoring</returns>
        PredictionClass GetPrediciton(DataClass record);
        /// <summary>
        /// Score multipe records.
        /// </summary>
        /// <param name="records">Records to score</param>
        /// <returns>A map of scored record and result.</returns>
        IDictionary<DataClass, PredictionClass> GetPredictions(IEnumerable<DataClass> records);
    }
}
