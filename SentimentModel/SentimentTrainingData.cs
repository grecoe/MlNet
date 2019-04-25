using System;
using System.Collections.Generic;
using MLNetWrapper.BaseImplementations;

namespace SentimentModel
{
    /// <summary>
    /// An instance of ITrainingDataAccumulator. 
    /// 
    /// When passed to the execution engine, all records scored are recorded in a seperate 
    /// training file with the scored result. 
    /// 
    /// An additional, empty, column is also attached using the constant value in ObservedValue. 
    /// This allows another applicaiton to be written that can present the data and the scored 
    /// value and allow human intervention to determine whether the scored result is correct
    /// or to provide the correct value. 
    /// 
    /// In this way, new training data can easily be created by actual values that have passed 
    /// through the engine. 
    /// </summary>
    public class SentimentTrainingData : TrainingAccumulatorBase<SentimentData>
    {
        private const string ObservedValue = "UserObservation";
        private const char TrainingSeperator = '\t';

        public SentimentTrainingData(String trainingFile)
            : base(trainingFile, SentimentTrainingData.ObservedValue, SentimentTrainingData.TrainingSeperator)
        {

        }

        /// <summary>
        /// Load up the data that has been saved. 
        /// </summary>
        /// <param name="useObservedValue">Use the record as is, or use the </param>
        /// <param name="labelColumn">Column that holds the label value</param>
        /// <returns></returns>
        public override IEnumerable<string> LoadSet()
        {
            Console.WriteLine("SentimentTrainingData:LoadSet");
            return this.LoadSetBooleanLabel(true, "Sentiment");
         }
    }
}
