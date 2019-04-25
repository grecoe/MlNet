using Microsoft.ML;
using System.Collections.Generic;
using System;
using MLNetWrapper.BaseImplementations;
using MLNetWrapper.Interfaces;

namespace FactoryModel
{
    /// <summary>
    /// An instance of IExecutionBase.
    /// 
    /// This class is used to score single or multiple records. 
    /// </summary>
    public class MaintenanceExecutionEngine : ExecutionBase<MaintenanceData, MaintenancePrediction>
    {
        public MaintenanceExecutionEngine(MLContext context, IModelPersistence modelfile)
        {
            this.ModelPersistence = modelfile;
            this.Context = context;
        }

        public override MaintenancePrediction GetPrediciton(MaintenanceData record)
        {
            Console.WriteLine("MaintenanceExecutionEngine:GetPrediciton");
            MaintenancePrediction prediction = base.GetPrediciton(record);
            if (this.Training != null)
            {
                record.State = prediction.Prediction;
                this.Training.AddPrediction(record);
            }
            return prediction;
        }

        public override IDictionary<MaintenanceData, MaintenancePrediction> GetPredictions(IEnumerable<MaintenanceData> records)
        {
            Console.WriteLine("MaintenanceExecutionEngine:GetPredicitons");
            IDictionary<MaintenanceData, MaintenancePrediction> predictions = base.GetPredictions(records);

            // If we have an instance of ITrainingDataAccumulator, save the scored record with the 
            // scoring result to the new training file.
            if (this.Training != null)
            {
                foreach (KeyValuePair<MaintenanceData, MaintenancePrediction> kvp in predictions)
                {
                    MaintenanceData data = kvp.Key;
                    data.State = kvp.Value.Prediction;
                    this.Training.AddPrediction(data);
                }
            }

            return predictions;
        }
    }
}
