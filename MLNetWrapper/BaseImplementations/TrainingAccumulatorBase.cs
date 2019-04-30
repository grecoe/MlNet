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
using System.Reflection;
using MLNetWrapper.Interfaces;
using System.Linq;

namespace MLNetWrapper.BaseImplementations
{
    /// <summary>
    /// Base implementation of ITrainingDataAccumulator
    /// </summary>
    /// <typeparam name="DataClass">Data class used to feed model for predictions.</typeparam>
    public abstract class TrainingAccumulatorBase<DataClass> : ITraningDataAccumulator<DataClass>
        where DataClass : class, new()
    {

        #region Public interface properties
        /// <summary>
        /// The file where the additional training data will be stored.
        /// </summary>
        public string TrainingFile { get; set; }
        /// <summary>
        /// The mapping between the actual fields on a class and the TrainingDataAttribute.
        /// </summary>
        public List<TrainingFieldInfo> Fields { get; private set; }
        /// <summary>
        /// The header that will be written out to the file.
        /// </summary>
        public string Header { get; private set; }
        /// <summary>
        /// The character to separate fields in a file. Default is '\t' for .TSV
        /// </summary>
        public char SeparatorChar { get; private set; }
        #endregion Public interface properteis

        #region Private Members
        private String ObservedValue { get; set; }
        private List<String> CurrentPredictions { get; set; }
        #endregion

        public TrainingAccumulatorBase(string trainingFile, string observedColumn, char separator = '\t')
        {
            this.ObservedValue = observedColumn;
            this.TrainingFile = trainingFile;
            this.Fields = new List<TrainingFieldInfo>();
            this.CurrentPredictions = new List<string>();
            this.SeparatorChar = separator;

            this.LoadDataFromType();
        }


        #region Public interface methods
        /// <summary>
        /// Called by the IExeuctionBase to add a new record to the file. 
        /// </summary>
        /// <param name="data">A record with incoming values and the resulting prediction in the label column.</param>
        public void AddPrediction(DataClass data)
        {
            Console.WriteLine("TrainingAccumulatorBase:AddPrediction");
            List<String> fieldData = new List<string>();

            foreach (TrainingFieldInfo fInfo in this.Fields)
            {
                fieldData.Add(fInfo.FieldInfoData.GetValue(data).ToString());
            }
            // For re-scoring 
            fieldData.Add(String.Empty);

            this.CurrentPredictions.Add(String.Join(this.SeparatorChar.ToString(), fieldData));
        }

        /// <summary>
        /// Flush out any collected records to the secondary (this.TrainingFile) training set.
        /// </summary>
        public void Flush()
        {
            Console.WriteLine("TrainingAccumulatorBase:Flush");

            bool newFile = false;
            if (System.IO.File.Exists(this.TrainingFile) == false)
            {
                newFile = true;
            }

            using (System.IO.StreamWriter trainingFile = new System.IO.StreamWriter(this.TrainingFile, true))
            {
                if (newFile)
                {
                    trainingFile.WriteLine(this.Header);
                }

                foreach (String pred in this.CurrentPredictions)
                {
                    trainingFile.WriteLine(pred);
                }
                this.CurrentPredictions.Clear();
            }
        }

        /// <summary>
        /// Load the training data that has already been suplemented by a human. Each return record
        /// is in the correct form to be appended to the orignal training data file making that set
        /// more robust.
        /// 
        /// This abstract method forces implementing models to define the body.
        /// </summary>
        /// <returns>A collection of records to append to the base training data file.</returns>
        abstract public IEnumerable<string> LoadSet();


        /// <summary>
        /// Parses a record that contains only values separated by the SeparatorChar and maps those
        /// to an IDictionary where the key is the actual field name and the value from the parsed 
        /// incoming record.
        /// 
        /// This is typically used in conjuction with the LoadSet() method.
        /// </summary>
        /// <param name="record">SeperatorChar separated raw values</param>
        /// <returns>Map between actual field name and raw value.</returns>
        public IDictionary<string, string> ParseRecord(string record)
        {
            Console.WriteLine("TrainingAccumulatorBase:ParseRecord: " + record);

            Dictionary<string, string> returnData = new Dictionary<string, string>();

            int partIndex = 0;
            String[] parts = record.Split(new char[] { this.SeparatorChar }, StringSplitOptions.None);

            foreach(TrainingFieldInfo fieldInfo in this.Fields)
            {
                if(partIndex > parts.Length)
                {
                    break;
                }

                returnData.Add(fieldInfo.TrainingAttribute.ColumnName, parts[partIndex++]);
            }

            if (partIndex == (parts.Length - 1))
            {
                returnData.Add(this.ObservedValue, parts[partIndex]);
            }

            return returnData;
        }

        #endregion public interface methods

        #region Protected Methods
        /// <summary>
        /// Function available only to deriving classes. Used to parse the records in the file controlled 
        /// by this instance. 
        /// 
        /// Function expects that both the label column AND the observed value column are in boolean form, i.e.
        /// True or False so that it can be parsed appropriately. 
        /// </summary>
        /// <param name="useObservedValue">If true, replaces the value in the labelColumn with that observed by 
        /// the human who intervened to label the data. Otherwise, the data from the original prediction is used.</param>
        /// <param name="labelColumn">The column where the boolean label is located in the data.</param>
        /// <returns>A list of records sufficient to be appended to the original training data.</returns>
        protected IEnumerable<String> LoadSetBooleanLabel(bool useObservedValue, String labelColumn)
        {
            Console.WriteLine("TrainingAccumulatorBase:LoadSetBooleanLabel");

            List<String> returnRows = new List<string>();
            if (System.IO.File.Exists(this.TrainingFile))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(this.TrainingFile))
                {
                    // Throw the first line out.
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string data = reader.ReadLine();
                        IDictionary<string, string> record = this.ParseRecord(data);

                        // We have to figure out which one to use, the observed value that a user
                        // has manually edited, or the one that came from the prediction engine.
                        bool valueToUse = false;
                        if (useObservedValue && record.ContainsKey(this.ObservedValue))
                        {
                            if (String.IsNullOrEmpty(record[this.ObservedValue]))
                            {
                                // We can't use the observed value, so don't include it. 
                                continue;
                            }
                            valueToUse = bool.Parse(record[this.ObservedValue]);
                        }
                        else if (record.ContainsKey(labelColumn))
                        {
                            valueToUse = bool.Parse(record[labelColumn]);
                        }
                        else
                        {
                            continue;
                        }

                        // We know the data set expects a 1 or 0 so modify it.
                        record[labelColumn] = (valueToUse ? "1" : "0");

                        // Now drop the observed column...
                        if (record.ContainsKey(this.ObservedValue))
                        {
                            record.Remove(this.ObservedValue);
                        }

                        returnRows.Add(String.Join(this.SeparatorChar.ToString(), record.Values));
                    }
                }
            }

            return returnRows;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Using reflection builds up the internal map of FieldInfo and TrainingDataAttribute. Further
        /// the file header is generated.
        /// </summary>
        private void LoadDataFromType()
        {
            Type tType = typeof(DataClass);

            FieldInfo[] fInfo = tType.GetFields();

            // Get all of the data out of the columns using our custom attribute
            // of TrainingDataAttribute
            SortedDictionary<int, TrainingFieldInfo> fieldData = new SortedDictionary<int, TrainingFieldInfo>();
            for (int i = 0; i < fInfo.Length; i++)
            {
                if (fInfo[i].CustomAttributes.Count() > 0)
                {
                    var custom = fInfo[i].GetCustomAttribute(typeof(TrainingDataAttribute));
                    if (custom != null)
                    {
                        TrainingDataAttribute tda = custom as TrainingDataAttribute;
                        fieldData.Add(tda.ColumnPosition, new TrainingFieldInfo()
                        {
                            FieldInfoData = fInfo[i],
                            TrainingAttribute = custom as TrainingDataAttribute
                        });
                        
                    }
                }
            }

            // Got it sorted, now add them to the fields
            List<String> columns = new List<string>();
            foreach (KeyValuePair<int, TrainingFieldInfo> kvp in fieldData)
            {
                columns.Add(kvp.Value.TrainingAttribute.ColumnName);
                this.Fields.Add(kvp.Value);
            }

            // Create a header for the CSV file
            columns.Add(this.ObservedValue);
            this.Header = String.Join(this.SeparatorChar.ToString(), columns);
        }
        #endregion

    }
}
