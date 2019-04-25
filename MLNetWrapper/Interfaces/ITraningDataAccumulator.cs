using System;
using System.Collections.Generic;
using System.Reflection;

namespace MLNetWrapper.Interfaces
{
    public class TrainingDataAttribute : Attribute
    {
        public String ColumnName { get; set; }
        public int ColumnPosition { get; set; }

        public TrainingDataAttribute(String name, int position)
        {
            this.ColumnName = name;
            this.ColumnPosition = position;
        }
    }

    public class TrainingFieldInfo
    {
        public TrainingDataAttribute TrainingAttribute;
        public FieldInfo FieldInfoData;
    }

    /// <summary>
    /// The interface that identifies how additional training data is collected. New training
    /// data is collected from an instance of IExecutionBase every time a record is scored, assuming
    /// that an instance of this interface has been supplied.
    /// 
    /// Mappings are done by tagging fields in the DataRecords class using the attribute TrainingDataAttribute.
    /// 
    /// Internally this is mapped to the actual FieldInfo of the DataRecord itself. 
    /// 
    /// Results are persisted out to the a local file only, identified by the caller.
    /// </summary>
    /// <typeparam name="DataRecord">Generic class identifying the input record to the model. This class
    /// MUST have the properties tagged with the TrainingDataAttribute to be recognized. </typeparam>
    public interface ITraningDataAccumulator<DataRecord> where DataRecord : class, new()
    {
        /// <summary>
        /// The file where the additional training data will be stored.
        /// </summary>
        String TrainingFile { get; set; }
        /// <summary>
        /// The mapping between the actual fields on a class and the TrainingDataAttribute.
        /// </summary>
        List<TrainingFieldInfo> Fields { get; }
        /// <summary>
        /// The header that will be written out to the file.
        /// </summary>
        String Header { get; }
        /// <summary>
        /// The character to separate fields in a file. Default is '\t' for .TSV
        /// </summary>
        char SeparatorChar { get; }

        /// <summary>
        /// Called by the IExeuctionBase to add a new record to the file. 
        /// </summary>
        /// <param name="data">A record with incoming values and the resulting prediction in the label column.</param>
        void AddPrediction(DataRecord data);

        /// <summary>
        /// Load the training data that has already been suplemented by a human. Each return record
        /// is in the correct form to be appended to the orignal training data file making that set
        /// more robust.
        /// </summary>
        /// <returns>A collection of records to append to the base training data file.</returns>
        IEnumerable<string> LoadSet();

        /// <summary>
        /// Parses a record that contains only values separated by the SeparatorChar and maps those
        /// to an IDictionary where the key is the actual field name and the value from the parsed 
        /// incoming record.
        /// 
        /// This is typically used in conjuction with the LoadSet() method.
        /// </summary>
        /// <param name="record">SeperatorChar separated raw values</param>
        /// <returns>Map between actual field name and raw value.</returns>
        IDictionary<string, string> ParseRecord(string record);

        /// <summary>
        /// Flush out any collected records to the secondary (this.TrainingFile) training set.
        /// </summary>
        void Flush();

    }
}
