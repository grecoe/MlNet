using System;
using System.Collections.Generic;
using Microsoft.Data.DataView;
using Microsoft.ML;
using MLNetWrapper.Interfaces;
using System.Linq;

namespace MLNetWrapper.BaseImplementations
{
    /// <summary>
    /// Base implementation of IExecutionBase
    /// </summary>
    /// <typeparam name="DataClass">Generic classs that identifies the expected model data.</typeparam>
    /// <typeparam name="PredictionClass">Generic class that identifies the model scoring result</typeparam>
    public class ExecutionBase<DataClass, PredictionClass>  : IExecutionBase<DataClass, PredictionClass>
        where DataClass : class, new()
        where PredictionClass : class, new()
    {
        #region Public Properties
        /// <summary>
        /// Identifies where the model is stored.
        /// </summary>
        public IModelPersistence ModelPersistence { get; set; }
        /// <summary>
        /// An instance of the model.
        /// </summary>
        public ITransformer Model { get; set; }
        /// <summary>
        /// The MLContext object required by ML.NET
        /// </summary>
        public MLContext Context { get; set; }
        /// <summary>
        /// Optional ITrainingDataAccumulator to collect results of all scored records.
        /// In a production environment this should be null.
        /// </summary>
        public ITraningDataAccumulator<DataClass> Training { get; set; }
        #endregion

        #region Private Properties
        /// <summary>
        /// Internal version of engine so it's not reloaded every time.
        /// </summary>
        private PredictionEngine<DataClass, PredictionClass> _predictionEngine;
        private PredictionEngine<DataClass, PredictionClass> PredictionEngine
        {
            get
            {
                if (_predictionEngine == null)
                {
                    _predictionEngine = this.Model.CreatePredictionEngine<DataClass, PredictionClass>(this.Context);
                }

                return _predictionEngine;
            }
        }
        #endregion


        /// <summary>
        /// Load the model identified in the IModelPersistence.
        /// </summary>
        /// <returns>True if the model is loaded</returns>
        public bool LoadModel()
        {

            if (this.Model == null)
            {
                this.Model = this.ModelPersistence.LoadModel(this.Context);
            }

            return (this.Model != null);
        }


        /// <summary>
        /// Score a single record.
        /// </summary>
        /// <param name="record">Record to score</param>
        /// <returns>Result of scoring</returns>
        public virtual PredictionClass GetPrediciton(DataClass record)
        {
            Console.WriteLine("ExecutionBase:GetPrediciton");
            PredictionClass resultprediction = null;
            if (this.LoadModel())
            {
                resultprediction = this.PredictionEngine.Predict(record);
            }
            return resultprediction;
        }

        /// <summary>
        /// Score multipe records.
        /// </summary>
        /// <param name="records">Records to score</param>
        /// <returns>A map of scored record and result.</returns>
        public virtual IDictionary<DataClass, PredictionClass> GetPredictions(IEnumerable<DataClass> records)
        {
            Console.WriteLine("ExecutionBase:GetPredicitons");
            Dictionary<DataClass, PredictionClass> results = new Dictionary<DataClass, PredictionClass>();

            if (this.LoadModel())
            {
                // Load test data  
                IDataView deviceStreamingDataView = this.Context.Data.LoadFromEnumerable(records);
                IDataView predictions = this.Model.Transform(deviceStreamingDataView);

                // Use model to predict whether comment data is Positive (1) or Negative (0).
                IEnumerable<PredictionClass> predictedResults = this.Context.Data.CreateEnumerable<PredictionClass>(predictions, reuseRowObject: false);

                // Builds pairs of (sentiment, prediction)
                IEnumerable<(DataClass sourceData, PredictionClass prediction)> dataAndPredictions = records.Zip(predictedResults, (sourceData, prediction) => (sourceData, prediction));

                foreach ((DataClass sourceData, PredictionClass prediction) item in dataAndPredictions)
                {
                    results.Add(item.sourceData, item.prediction);
                }
            }

            return results;
        }
    }
}
