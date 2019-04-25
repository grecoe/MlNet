using Microsoft.ML;
using System.Collections.Generic;
using System;
using MLNetWrapper.BaseImplementations;
using MLNetWrapper.Interfaces;

namespace SentimentModel
{
    /// <summary>
    /// An instance of IExecutionBase.
    /// 
    /// This class is used to score single or multiple records. 
    /// </summary>
    public class SentimentExecutionEngine : ExecutionBase<SentimentData, SentimentPrediction>
    {
        public SentimentExecutionEngine(MLContext context, IModelPersistence modelfile)
        {
            this.ModelPersistence = modelfile;
            this.Context = context;
        }

        public override SentimentPrediction GetPrediciton(SentimentData record)
        {
            Console.WriteLine("SentimentExecutionEngine:GetPrediciton");
            SentimentPrediction prediction = base.GetPrediciton(record);
            if (this.Training != null)
            {
                record.Sentiment = prediction.Prediction;
                this.Training.AddPrediction(record);
            }
            return prediction;
        }

        public override IDictionary<SentimentData, SentimentPrediction> GetPredictions(IEnumerable<SentimentData> records)
        {
            Console.WriteLine("SentimentExecutionEngine:GetPredicitons");

            IDictionary<SentimentData, SentimentPrediction> predictions = base.GetPredictions(records);

            // If we have an instance of ITrainingDataAccumulator, save the scored record with the 
            // scoring result to the new training file.
            if (this.Training != null)
            {
                foreach (KeyValuePair<SentimentData, SentimentPrediction> kvp in predictions)
                {
                    SentimentData data = kvp.Key;
                    data.Sentiment = kvp.Value.Prediction;
                    this.Training.AddPrediction(data);
                }
            }

            return predictions;
        }

    }
}
