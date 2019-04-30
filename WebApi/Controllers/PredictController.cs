
//
// Copyright  Microsoft Corporation ("Microsoft").
//
// Microsoft grants you the right to use this software in accordance with your subscription agreement, if any, to use software 
// provided for use with Microsoft Azure ("Subscription Agreement").  All software is licensed, not sold.  
// 
// If you do not have a Subscription Agreement, or at your option if you so choose, Microsoft grants you a nonexclusive, perpetual, 
// royalty-free right to use and modify this software solely for your internal business purposes in connection with Microsoft Azure 
// and other Microsoft products, including but not limited to, Microsoft R Open, Microsoft R Server, and Microsoft SQL Server.  
// 
// Unless otherwise stated in your Subscription Agreement, the following applies.  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT 
// WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL MICROSOFT OR ITS LICENSORS BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE SAMPLE CODE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

using Microsoft.AspNetCore.Mvc;
using FactoryModel;
using MLNetWrapper.Interfaces;
using MLNetWrapper.BaseImplementations;
using Microsoft.ML;
using Newtonsoft.Json;
using System;

namespace WebApi.Controllers
{
    /// <summary>
    /// To get a prediction
    ///     POST .../api/predict with a MaintenenceData object (JSON) in the body
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
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


        // POST api/predict
        [HttpPost]
        public IActionResult Post(MaintenanceData value)
        {
            ActionResult returnResult = new BadRequestObjectResult("Please pass an instance of MaintenanceData in the request body");

            if (value != null)
            {
                this.LoadExecutionEngine();
                if (PredictController.ExecutionEngine != null)
                {
                    MaintenancePrediction single = PredictController.ExecutionEngine.GetPrediciton(value);
                    returnResult = (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(single, Formatting.Indented));
                }
                else
                {
                    returnResult = new BadRequestObjectResult("Unable to obtain an instance of the execution engine");
                }
            }

            return returnResult;
        }

        /// <summary>
        /// Load an instance of IExecutionBase[MaintenanceData, MaintenancePrediction] into memory
        /// as a static variable to reduce response times. 
        /// </summary>
        private void LoadExecutionEngine()
        {
            if (PredictController.ExecutionEngine == null ||
                PredictController.LastLoad == null ||
                (DateTime.Now - PredictController.LastLoad.Value).TotalMinutes > 10)
            {
                // Set the last load time
                PredictController.LastLoad = DateTime.Now;

                // You'll need an MLContext object for most of the operations.
                MLContext machineContext = new MLContext();

                // Let the engines know where to save/load the model.
                AzureStorageFileConfiguration machineModelConfiguration =
                    new AzureStorageFileConfiguration(_machineModelName, _storageContainer, _storageConnectionString);
                ModelPersistence machineModelPersistence = new ModelPersistence(machineModelConfiguration);

                PredictController.ExecutionEngine = new MaintenanceExecutionEngine(machineContext, machineModelPersistence);
            }
        }

    }
}
