using System;
using System.Collections.Generic;
using MLNetWrapper.BaseImplementations;

namespace FactoryModel
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
    public class MaintenanceTrainingData : TrainingAccumulatorBase<MaintenanceData>
    {
        private const string ObservedValue = "UserObservation";
        private const char TrainingSeperator = ',';

        public MaintenanceTrainingData(String trainingFile) 
            :base(trainingFile, MaintenanceTrainingData.ObservedValue, MaintenanceTrainingData.TrainingSeperator)
        {

        }

        /// <summary>
        /// Load up the data that has been saved. 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> LoadSet()
        {
            Console.WriteLine("MaintenanceTrainingData:LoadSet");
            return this.LoadSetBooleanLabel(true, "state");
        }
    }
}
