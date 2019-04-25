using System;
using System.Collections.Generic;
using Microsoft.Data.DataView;
using Microsoft.ML;
using MLNetWrapper.BaseImplementations;
using MLNetWrapper.Interfaces;

namespace FactoryModel
{
    /// <summary>
    /// An instance of IModellingBase
    /// 
    /// This class is used to Build, Evaluate and save the model to the location
    /// desired by the caller (local disk or Azure  Blob Storage)
    /// </summary>
    public class MaintenanceModelTrainer : BinayrClassifciationModellingBase
    {
        private const float DATA_SPLIT = (float)0.1;

        public MaintenanceModelTrainer(MLContext context, string datafile, IModelPersistence modelfile)
        {
            this.DataFile = datafile;
            this.ModelPersistence = modelfile;
            this.Context = context;
        }

        public override TrainCatalogBase.TrainTestData LoadData()
        {
            Console.WriteLine("MaintenanceModelTrainer:LoadData");

            TrainCatalogBase.TrainTestData returnData = 
                this.LoadData<MaintenanceData>(
                    headers: true, 
                    seperator: ',', 
                    testSplit: MaintenanceModelTrainer.DATA_SPLIT);
            return returnData;
        }

        public override bool BuildModel(IDataView splitTrainSet)
        {
            Console.WriteLine("MaintenanceModelTrainer:BuildModel");

            List<String> options = new List<String>();
            List<String> features = new List<String>();
            foreach (DataViewSchema.Column c in splitTrainSet.Schema)
            {
                if (c.Index <= MaintenanceData.MAX_INDEX)
                {
                    options.Add(c.Name);
                    if (String.Compare(c.Name, "Label", true) != 0)
                    {
                        features.Add(c.Name);
                    }
                }
            }

            var pipeline = this.Context.Transforms.SelectColumns(options.ToArray())
                .Append(this.Context.Transforms.Concatenate("Features", features.ToArray()))
                .Append(this.Context.BinaryClassification.Trainers.FastTree());


            this.Model = pipeline.Fit(splitTrainSet);

            return (this.Model != null);
        }
    }
}
