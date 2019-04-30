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

using Microsoft.Data.DataView;
using Microsoft.ML;
using System;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Declaration of the interface that represents the ability to Create, Evaluate,
    /// and save a model. 
    /// </summary>
    public interface IModellingBase
    {
        /// <summary>
        /// File that contains the training data for the model.
        /// </summary>
        String DataFile { get; set; }
        /// <summary>
        /// Identifies where the model is stored.
        /// </summary>
        IModelPersistence ModelPersistence { get; set; }
        /// <summary>
        /// The MLContext object required by ML.NET
        /// </summary>
        MLContext Context { get; set; }
        /// <summary>
        /// An instance of the model.
        /// </summary>
        ITransformer Model { get; set; }

        /// <summary>
        /// Load the training file and split it into Train and Test 
        /// data sets.
        /// </summary>
        /// <returns>TrainCatalogBase.TrainTestData that has the data split into
        /// test and train datasets.</returns>
        TrainCatalogBase.TrainTestData LoadData();

        /// <summary>
        /// Bulds the model using training data.
        /// </summary>
        /// <param name="splitTrainSet">Training view from TrainCatalogBase.TrainTestData</param>
        /// <returns>True if built, false otherwise.</returns>
        bool BuildModel(IDataView splitTrainSet);
        /// <summary>
        /// Evaluates the model (but only prints to the console the results)
        /// </summary>
        /// <param name="splitTestSet">Testing view from TrainCatalogBase.TrainTestData</param>
        void EvaluateModel(IDataView splitTestSet);
        /// <summary>
        /// Save the model to the location identified by ModelPersistence.
        /// </summary>
        void SaveModel();

    }
}
