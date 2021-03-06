﻿//
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
using System;
using MLNetWrapper.BaseImplementations;
using MLNetWrapper.Interfaces;

namespace SentimentModel
{
    /// <summary>
    /// An instance of IExecutionBase.
    /// 
    /// This class is used to score single or multiple records. 
    /// </summary>
    public class SentimentExecutionEngine : ExecutionBase<SentimentData, SentimentPrediction>
    {
        public SentimentExecutionEngine(MLContext context, IModelPersistence modelfile)
        {
            this.ModelPersistence = modelfile;
            this.Context = context;
        }

        public override SentimentPrediction GetPrediciton(SentimentData record)
        {
            Console.WriteLine("SentimentExecutionEngine:GetPrediciton");
            SentimentPrediction prediction = base.GetPrediciton(record);
            if (this.Training != null)
            {
                record.Sentiment = prediction.Prediction;
                this.Training.AddPrediction(record);
            }
            return prediction;
        }

        public override IDictionary<SentimentData, SentimentPrediction> GetPredictions(IEnumerable<SentimentData> records)
        {
            Console.WriteLine("SentimentExecutionEngine:GetPredicitons");

            IDictionary<SentimentData, SentimentPrediction> predictions = base.GetPredictions(records);

            // If we have an instance of ITrainingDataAccumulator, save the scored record with the 
            // scoring result to the new training file.
            if (this.Training != null)
            {
                foreach (KeyValuePair<SentimentData, SentimentPrediction> kvp in predictions)
                {
                    SentimentData data = kvp.Key;
                    data.Sentiment = kvp.Value.Prediction;
                    this.Training.AddPrediction(data);
                }
            }

            return predictions;
        }

    }
}
