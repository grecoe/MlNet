using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using MLNetWrapper.BaseImplementations;
using MLNetWrapper.Interfaces;

namespace SentimentModel
{
    /// <summary>
    /// An instance of IModellingBase
    /// 
    /// This class is used to Build, Evaluate and save the model to the location
    /// desired by the caller (local disk or Azure  Blob Storage)
    /// </summary>
    public class SentimentModelTrainer : BinayrClassifciationModellingBase
    {
        private const float DATA_SPLIT = (float)0.1;
        public SentimentModelTrainer(MLContext context, string datafile, IModelPersistence modelfile)
        {
            this.DataFile = datafile;
            this.ModelPersistence = modelfile;
            this.Context = context;
        }

        public override TrainCatalogBase.TrainTestData LoadData()
        {
            Console.WriteLine("SentimentModelTrainer:LoadData");

            TrainCatalogBase.TrainTestData returnData =
                this.LoadData<SentimentData>(
                    headers: false,
                    testSplit: SentimentModelTrainer.DATA_SPLIT);
            return returnData;
        }

        public override bool BuildModel(IDataView splitTrainSet)
        {
            Console.WriteLine("SentimentModelTrainer:BuildModel");

            var pipeline = this.Context.Transforms.Text.FeaturizeText(outputColumnName: DefaultColumnNames.Features, inputColumnName: nameof(SentimentData.SentimentText))
                .Append(this.Context.BinaryClassification.Trainers.FastTree(numLeaves: 50, numTrees: 50, minDatapointsInLeaves: 20));

            this.Model = pipeline.Fit(splitTrainSet);
            return (this.Model != null);
        }
    }
}
