using Microsoft.ML.Data;
using MLNetWrapper.Interfaces;

namespace FactoryModel
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
    public class MaintenanceData
    {
        public const int MAX_INDEX = 5;

        [TrainingDataAttribute("temp",0)]
        [LoadColumn(0)]
        public float Temperature;
        [TrainingDataAttribute("volt", 1)]
        [LoadColumn(1)]
        public float Voltage;
        [TrainingDataAttribute("rotate", 2)]
        [LoadColumn(2)]
        public float Rotation;
        [TrainingDataAttribute("state", 3)]
        [LoadColumn(3), ColumnName("Label")]
        public bool State;
        [TrainingDataAttribute("time", 4)]
        [LoadColumn(4)]
        public float Time;
        [TrainingDataAttribute("id", 5)]
        [LoadColumn(5)]
        public float MachineIdentity;
    }

    /// <summary>
    /// This is the response object from a scoring call to the model.
    /// </summary>
    public class MaintenancePrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        // [ColumnName("Probability")]
        public float Probability { get; set; }

        //  [ColumnName("Score")]
        public float Score { get; set; }
    }
}
