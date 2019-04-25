using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using FactoryModel;
using SentimentModel;
using MLNetWrapper.Interfaces;
using MLNetWrapper.BaseImplementations;

namespace LocalApplication
{
    class Program
    {
        #region Static Members for Sentiment Model
        static readonly string _sentimentModelName = @"SentimentModel.zip";
        static readonly string _sentimentDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "yelp_labelled.txt");
        static readonly string _sentimentTrainingPath = Path.Combine(Environment.CurrentDirectory, "Data", "sentiment_training.txt");
        #endregion

        #region Static Members for FactoryModel
        static readonly string _factoryModelName = @"MachineModel.zip";
        static readonly string _factoryDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "e2e_dataset_orig.csv");
        static readonly string _factoryTrainingPath = Path.Combine(Environment.CurrentDirectory, "Data", "machine_training.txt");
        #endregion

        #region Static Members for storage locations
        static readonly string _localDirectory = @"C:\_MLNET\Models";
        static readonly string _storateContainer = @"models";
        static readonly string _storageConnectionString = @"YOUR_STORAGE_ACCOUNT_CONNECTION_STRING";
        #endregion


        static void Main(string[] args)
        {
            TestMachineModel();
            TestSentimentModel();
            Console.ReadLine();
        }

        /// <summary>
        /// Trains, creates, and executes the MachineModel for the factory example. 
        /// 
        /// Model is stored in Azure Storage.
        /// </summary>
        private static void TestMachineModel()
        {
            // Test machine model
            IEnumerable<MaintenanceData> machines = new[]
            {
                new MaintenanceData
                {
                    Temperature = (float)120.0,
                    Rotation = (float)222.5,
                    Voltage = (float)150,
                    Time = 5,
                    MachineIdentity = 4
                },
                new MaintenanceData
                {
                    Temperature = (float)50.0,
                    Rotation = (float)50.5,
                    Voltage = (float)120.0,
                    Time = 5,
                    MachineIdentity = 5
                },
            };

            // You'll need an MLContext object for most of the operations.
            MLContext machineContext = new MLContext();

            // Let the engines know where to save/load the model.
            AzureStorageFileConfiguration machineModelConfiguration =
                new AzureStorageFileConfiguration(_factoryModelName, _storateContainer, _storageConnectionString);
            ModelPersistence machineModelPersistence = new ModelPersistence(machineModelConfiguration);

            // Modelling base is used for building the model
            IModellingBase machineModel = new MaintenanceModelTrainer(machineContext, _factoryDataPath, machineModelPersistence);

            // A training accumulator is used in execution to save off more training data.
            ITraningDataAccumulator<MaintenanceData> machineTraining = new MaintenanceTrainingData(_factoryTrainingPath);

            // Finally you need the exexution base to run the model. 
            IExecutionBase<MaintenanceData, MaintenancePrediction> machineExecution = new MaintenanceExecutionEngine(machineContext, machineModelPersistence);
            machineExecution.Training = machineTraining;

            TestPattern<MaintenanceData, MaintenancePrediction>(machines, machineModel, machineExecution);
        }

        /// <summary>
        /// Trains, creates, and executes the SentimentModel . 
        /// 
        /// Model is stored on the local disk.
        /// </summary>
        private static void TestSentimentModel()
        {
            // Test the sentiment
            IEnumerable<SentimentData> sentiments = new[]
            {
                    new SentimentData
                    {
                        SentimentText = "This was a horrible meal"
                    },
                    new SentimentData
                    {
                        SentimentText = "I love this spaghetti."
                    }
            };

            // You'll need an MLContext object for most of the operations.
            MLContext sentimentContext = new MLContext();

            // Let the engines know where to save/load the model.
            LocalFileConfiguration sentimentModelConfiguration = new LocalFileConfiguration(_sentimentModelName, _localDirectory);
            ModelPersistence sentimentModelPersistence = new ModelPersistence(sentimentModelConfiguration);

            // Modelling base is used for building the model
            IModellingBase sentimentModel = new SentimentModelTrainer(sentimentContext, _sentimentDataPath, sentimentModelPersistence);

            // A training accumulator is used in execution to save off more training data.
            ITraningDataAccumulator<SentimentData> sentimentTraining = new SentimentTrainingData(_sentimentTrainingPath);

            // Finally you need the exexution base to run the model. 
            IExecutionBase<SentimentData, SentimentPrediction> sentimentExecution = new SentimentExecutionEngine(sentimentContext, sentimentModelPersistence);
            sentimentExecution.Training = sentimentTraining;

            TestPattern<SentimentData, SentimentPrediction>(sentiments, sentimentModel, sentimentExecution);
        }

        /// <summary>
        /// Generic test pattern for either model. 
        /// </summary>
        /// <typeparam name="DataClass">Generic class type of data object expected by model. </typeparam>
        /// <typeparam name="PredictionClass">Generic class type of data object returned by model. </typeparam>
        /// <param name="records">Records to score, must be greater than 0</param>
        /// <param name="modelEngine">The modelling engine used to train and create a model.</param>
        /// <param name="executionEngine">The execution engine to process records.</param>
        private static void TestPattern<DataClass, PredictionClass>(
            IEnumerable<DataClass> records,
            IModellingBase modelEngine,
            IExecutionBase<DataClass, PredictionClass> executionEngine)
            where DataClass : class, new()
            where PredictionClass : class, new()
        {
            TrainCatalogBase.TrainTestData splitView = modelEngine.LoadData();
            if (modelEngine.BuildModel(splitView.TrainSet))
            {
                modelEngine.EvaluateModel(splitView.TestSet);
                modelEngine.SaveModel();

                Console.WriteLine(Environment.NewLine + "*******Single Prediction");
                PredictionClass single = executionEngine.GetPrediciton(records.First());
                PrintPrediction(records.First(), single);

                Console.WriteLine(Environment.NewLine + "*******Multiple Predictions");
                IDictionary<DataClass, PredictionClass> multi = executionEngine.GetPredictions(records);
                foreach (KeyValuePair<DataClass, PredictionClass> kvp in multi)
                {
                    PrintPrediction<DataClass, PredictionClass>(kvp.Key, kvp.Value);
                }
                Console.WriteLine(Environment.NewLine);

                executionEngine.Training.Flush();
            }
        }

        /// <summary>
        /// For the local application, print out the results of the predictions along with the 
        /// actual data that was processed.
        /// </summary>
        /// <typeparam name="DataClass">Generic class type of data object expected by model. </typeparam>
        /// <typeparam name="PredictionClass">Generic class type of data object returned by model. </typeparam>
        /// <param name="data">Data that was scored.</param>
        /// <param name="pred">Result from the data.</param>
        private static void PrintPrediction<DataClass, PredictionClass>(DataClass data, PredictionClass pred)
        {
            if (data is MaintenanceData)
            {
                Console.WriteLine((data as MaintenanceData).MachineIdentity + " = " + (pred as MaintenancePrediction).Prediction);
            }
            else if (data is SentimentData)
            {
                Console.WriteLine((data as SentimentData).SentimentText + " = " + (pred as SentimentPrediction).Prediction);
            }
        }
    }
}
