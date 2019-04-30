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
using MLNetWrapper.BaseImplementations;

namespace FactoryModel
{
    /// <summary>
    /// An instance of ITrainingDataAccumulator. 
    /// 
    /// When passed to the execution engine, all records scored are recorded in a seperate 
    /// training file with the scored result. 
    /// 
    /// An additional, empty, column is also attached using the constant value in ObservedValue. 
    /// This allows another applicaiton to be written that can present the data and the scored 
    /// value and allow human intervention to determine whether the scored result is correct
    /// or to provide the correct value. 
    /// 
    /// In this way, new training data can easily be created by actual values that have passed 
    /// through the engine. 
    /// </summary>
    public class MaintenanceTrainingData : TrainingAccumulatorBase<MaintenanceData>
    {
        private const string ObservedValue = "UserObservation";
        private const char TrainingSeperator = ',';

        public MaintenanceTrainingData(String trainingFile) 
            :base(trainingFile, MaintenanceTrainingData.ObservedValue, MaintenanceTrainingData.TrainingSeperator)
        {

        }

        /// <summary>
        /// Load up the data that has been saved. 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> LoadSet()
        {
            Console.WriteLine("MaintenanceTrainingData:LoadSet");
            return this.LoadSetBooleanLabel(true, "state");
        }
    }
}
