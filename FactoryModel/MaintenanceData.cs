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

using Microsoft.ML.Data;
using MLNetWrapper.Interfaces;

namespace FactoryModel
{
    /// <summary>
    /// This is the data class that is passed into the model for scoring.
    /// 
    /// Note that each field that comes from the original data is marked with
    /// a TrainingDataAttribute. This is used by the ITrainingDataAccumulator 
    /// that keeps track of all records that are scored with the scoring label. 
    /// 
    /// Doing so allows an easy way to create, or append, new training data to
    /// the original training data making retraining trivial. 
    /// </summary>
    public class MaintenanceData
    {
        public const int MAX_INDEX = 5;

        [TrainingDataAttribute("temp",0)]
        [LoadColumn(0)]
        public float Temperature;
        [TrainingDataAttribute("volt", 1)]
        [LoadColumn(1)]
        public float Voltage;
        [TrainingDataAttribute("rotate", 2)]
        [LoadColumn(2)]
        public float Rotation;
        [TrainingDataAttribute("state", 3)]
        [LoadColumn(3), ColumnName("Label")]
        public bool State;
        [TrainingDataAttribute("time", 4)]
        [LoadColumn(4)]
        public float Time;
        [TrainingDataAttribute("id", 5)]
        [LoadColumn(5)]
        public float MachineIdentity;
    }

    /// <summary>
    /// This is the response object from a scoring call to the model.
    /// </summary>
    public class MaintenancePrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        // [ColumnName("Probability")]
        public float Probability { get; set; }

        //  [ColumnName("Score")]
        public float Score { get; set; }
    }
}
