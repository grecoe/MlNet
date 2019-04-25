using Microsoft.ML.Data;
using MLNetWrapper.Interfaces;

namespace SentimentModel
{
    /// <summary>
    /// This is the data class that is passed into the model for scoring.
    /// 
    /// Note that each field that comes from the original data is marked with
    /// a TrainingDataAttribute. This is used by the ITrainingDataAccumulator 
    /// that keeps track of all records that are scored with the scoring label. 
    /// 
    /// Doing so allows an easy way to create, or append, new training data to
    /// the original training data making retraining trivial. 
    /// </summary>
    public class SentimentData
    {
        [TrainingDataAttribute("Text", 0)]
        [LoadColumn(0)]
        public string SentimentText;

        [TrainingDataAttribute("Sentiment", 1)]
        [LoadColumn(1), ColumnName("Label")]
        public bool Sentiment;
    }

    /// <summary>
    /// This is the response object from a scoring call to the model.
    /// </summary>
    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        // [ColumnName("Probability")]
        public float Probability { get; set; }

        //  [ColumnName("Score")]
        public float Score { get; set; }
    }

}
