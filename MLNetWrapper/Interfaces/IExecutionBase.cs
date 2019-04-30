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

using Microsoft.ML;
using System.Collections.Generic;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Declaration of the interface that represents the ability to score records for 
    /// a single model type.
    /// </summary>
    /// <typeparam name="DataClass">Generic classs that identifies the expected model data.</typeparam>
    /// <typeparam name="PredictionClass">Generic class that identifies the model scoring result</typeparam>
    public interface IExecutionBase<DataClass, PredictionClass>
        where DataClass : class, new()
        where PredictionClass : class, new()
    {
        /// <summary>
        /// Identifies where the model is stored.
        /// </summary>
        IModelPersistence ModelPersistence { get; set; }
        /// <summary>
        /// An instance of the model.
        /// </summary>
        ITransformer Model { get; set; }
        /// <summary>
        /// The MLContext object required by ML.NET
        /// </summary>
        MLContext Context { get; set; }
        /// <summary>
        /// Optional ITrainingDataAccumulator to collect results of all scored records.
        /// In a production environment this should be null.
        /// </summary>
        ITraningDataAccumulator<DataClass> Training { get; set; }
        /// <summary>
        /// Load the model identified in the IModelPersistence.
        /// </summary>
        /// <returns>True if the model is loaded</returns>
        bool LoadModel();
        /// <summary>
        /// Score a single record.
        /// </summary>
        /// <param name="record">Record to score</param>
        /// <returns>Result of scoring</returns>
        PredictionClass GetPrediciton(DataClass record);
        /// <summary>
        /// Score multipe records.
        /// </summary>
        /// <param name="records">Records to score</param>
        /// <returns>A map of scored record and result.</returns>
        IDictionary<DataClass, PredictionClass> GetPredictions(IEnumerable<DataClass> records);
    }
}
