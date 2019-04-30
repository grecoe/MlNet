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
