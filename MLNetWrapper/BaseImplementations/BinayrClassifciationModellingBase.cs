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
using System.IO;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLNetWrapper.Interfaces;

namespace MLNetWrapper.BaseImplementations
{
    /// <summary>
    /// Base implementation of IModellingBase
    /// </summary>
    public abstract class BinayrClassifciationModellingBase : IModellingBase
    {
        #region Public Properties
        /// <summary>
        /// File that contains the training data for the model.
        /// </summary>
        public string DataFile { get; set; }
        /// <summary>
        /// Identifies where the model is stored.
        /// </summary>
        public IModelPersistence ModelPersistence{ get; set; }
        /// <summary>
        /// The MLContext object required by ML.NET
        /// </summary>
        public MLContext Context { get; set; }
        /// <summary>
        /// An instance of the model.
        /// </summary>
        public ITransformer Model { get; set; }
        #endregion

        #region Load Data

        /// <summary>
        /// Load the training file and split it into Train and Test data sets.
        /// 
        /// Left abstract to force derivers to figure out how to load thier own data by format.
        /// </summary>
        /// <returns>TrainCatalogBase.TrainTestData that has the data split into
        /// test and train datasets.</returns>
        abstract public TrainCatalogBase.TrainTestData LoadData();

        /// <summary>
        /// Protected method called inside of LoadData() after the caller has implemented whatever 
        /// logic they need for this.
        /// </summary>
        /// <typeparam name="DataClass">Class representing input data class for model</typeparam>
        /// <param name="headers">True indicates header row in data</param>
        /// <param name="testSplit">How to split data, this fraction will be used for the test set</param>
        /// <param name="seperator">Identifies seperator in the file, default TSV</param>
        /// <returns>Data split into test and train groups.</returns>
        protected TrainCatalogBase.TrainTestData LoadData<DataClass>(bool headers, float testSplit, char seperator = '\t') where DataClass : class, new()
        {
            Console.WriteLine("BinayrClassifciationModellingBase:LoadData");

            TrainCatalogBase.TrainTestData splitDataView;

            IDataView dataView = this.Context.Data.LoadFromTextFile<DataClass>(this.DataFile, separatorChar: seperator, hasHeader: headers);
            splitDataView = this.Context.BinaryClassification.TrainTestSplit(dataView, testFraction: testSplit);

            return splitDataView;
        }
        #endregion

        #region Build model
        /// <summary>
        /// Bulds the model using training data.
        /// 
        /// Abstract to force derivers to identify specifics.
        /// </summary>
        /// <param name="splitTrainSet">Training view from TrainCatalogBase.TrainTestData</param>
        /// <returns>True if built, false otherwise.</returns>
        abstract public bool BuildModel(IDataView splitTrainSet);
        #endregion Build model

        #region Evaluate Model
        /// <summary>
        /// Evaluates the model (but only prints to the console the results)
        /// </summary>
        /// <param name="splitTestSet">Testing view from TrainCatalogBase.TrainTestData</param>
        public void EvaluateModel(IDataView splitTestSet)
        {
            Console.WriteLine("BinayrClassifciationModellingBase:EvaluateModel");

            // Evaluate the model and show accuracy stats
            //Take the data in, make transformations, output the data. 
            IDataView predictions = this.Model.Transform(splitTestSet);

            // BinaryClassificationContext.Evaluate returns a BinaryClassificationEvaluator.CalibratedResult
            // that contains the computed overall metrics.
            CalibratedBinaryClassificationMetrics metrics = this.Context.BinaryClassification.Evaluate(predictions, "Label");

            // The Accuracy metric gets the accuracy of a classifier, which is the proportion 
            // of correct predictions in the test set.
            // The Auc metric gets the area under the ROC curve.
            // The area under the ROC curve is equal to the probability that the classifier ranks
            // a randomly chosen positive instance higher than a randomly chosen negative one
            // (assuming 'positive' ranks higher than 'negative').

            // The F1Score metric gets the classifier's F1 score.
            // The F1 score is the harmonic mean of precision and recall:
            //  2 * precision * recall / (precision + recall).
            Console.WriteLine();
            Console.WriteLine("Model quality metrics evaluation");

            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
        }
        #endregion Evaluate model

        #region Save model
        /// <summary>
        /// Save the model to the location identified by ModelPersistence.
        /// </summary>
        public void SaveModel()
        {
            if (this.Model != null)
            {
                this.ModelPersistence.SetModel(this.Model);
                bool saveResult = this.ModelPersistence.SaveModel(this.Context);
                Console.WriteLine("The model is saved");
            }
        }
        #endregion Save model
    }
}
