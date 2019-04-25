using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FactoryModel;
using MLNetWrapper.Interfaces;
using MLNetWrapper.BaseImplementations;
using Microsoft.ML;


namespace ServerlessFunction
{
    public static class ModelTest
    {
        /// <summary>
        /// These static variables identify where the model is found in Azure Storage. Better location...AppSettings
        /// </summary>
        static readonly string _machineModelName = @"MachineModel.zip";
        static readonly string _storageContainer = @"models";
        static readonly string _storageConnectionString = @"YOUR_STORAGE_ACCOUNT_CONNECTION_STRING";

        /// <summary>
        /// These static variables are used to hold a version of the execution engine. This will help keep the API as responsive 
        /// as possible by reducing the number of times the model needs to be loaded from storage. 
        /// </summary>
        static DateTime? LastLoad = null;
        static IExecutionBase<MaintenanceData, MaintenancePrediction> ExecutionEngine = null;

        /// <summary>
        /// Keep a copy of the engine in memory, if possible, to reduce prediction times. This static variable
        /// should live for all versions of the app on the same server. While it's unknown when it will be reset
        /// ensure that we re-load it every 10 minutes to catch new versions of the model in storage.
        /// </summary>
        private static void LoadExecutionEngine()
        {
            // Add a timeout....
            if (ModelTest.ExecutionEngine == null || 
                ModelTest.LastLoad == null ||
                (DateTime.Now - ModelTest.LastLoad.Value).TotalMinutes > 10)
            {
                // Set the last load time
                ModelTest.LastLoad = DateTime.Now;

                // You'll need an MLContext object for most of the operations.
                MLContext machineContext = new MLContext();

                // Let the engines know where to save/load the model.
                AzureStorageFileConfiguration machineModelConfiguration =
                    new AzureStorageFileConfiguration(_machineModelName, _storageContainer, _storageConnectionString);
                ModelPersistence machineModelPersistence = new ModelPersistence(machineModelConfiguration);

                ModelTest.ExecutionEngine = new MaintenanceExecutionEngine(machineContext, machineModelPersistence);
            }
        }

        /// <summary>
        /// This example uses the factory model in a request only mode (no retraining data collected) that is loaded
        /// from an Azure Storage account on request. 
        /// </summary>
        /// <returns>
        ///     OKResult -> Body is a MaintenancePrediction prediction object (JSON)
        ///     BadRequestObjectResult -> 
        ///         Request body missing
        ///         Request body NOT a MaintenancePrediction
        /// </returns>
        [FunctionName("FactoryModel")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            ActionResult returnResult = new BadRequestObjectResult("Please pass an instance of MaintenanceData in the request body");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
                MaintenanceData processingData = JsonConvert.DeserializeObject<MaintenanceData>(requestBody);

                if (processingData != null)
                {
                    log.LogInformation("Processing data valid.....");
                    ModelTest.LoadExecutionEngine();
                    if (ModelTest.ExecutionEngine != null)
                    {
                        MaintenancePrediction single = ModelTest.ExecutionEngine.GetPrediciton(processingData);
                        returnResult = (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(single, Formatting.Indented));
                    }
                    else
                    {
                        returnResult = new BadRequestObjectResult("Execution engine could not be retrieved.");
                    }
                }
            }

            return returnResult;
        }
    }
}
