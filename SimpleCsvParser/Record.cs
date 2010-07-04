using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Security;
using System.Linq;
using System.IO.Compression;

namespace SimpleCsvParser
{
    /// <summary>
    /// Represents a record in a data flow.
    /// </summary>
    public class Record : Dictionary<string, object>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Record() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="capacity">Initial number of fields to be stored in a record.</param>
        public Record(int capacity) : base(capacity) { }
 
        /// <summary>
        /// Merges fields from <paramref name="recordToAdd"/> into the record.
        /// In case of field name conflicts, overwrites properties with new ones
        /// if <paramref name="overwrite"/> is true.
        /// </summary>
        /// <param name="recordToAdd">Record containing fields to add.</param>
        /// <param name="overwrite">Whether property overwrite is needed or not.</param>
        public void Add(Record recordToAdd, bool overwrite)
        {
            foreach (var property in recordToAdd)
            {
                if (overwrite || !this.ContainsKey(property.Key))
                {
                    this[property.Key] = property.Value;
                }
            }
        }

        /// <summary>
        /// Adds new field to record and returns itself.
        /// </summary>
        /// <param name="fieldName">New field name.</param>
        /// <param name="fieldValue">New field value.</param>
        /// <returns>this</returns>
        public Record WithField(string fieldName, object fieldValue)
        {
            if (fieldValue != null)
                this[fieldName] = fieldValue;
            return this;
        }

        /// <summary>
        /// Removes field from record and returns itself
        /// </summary>
        /// <param name="fieldName">field name</param>
        /// <returns>this</returns>
        public Record WithoutField(string fieldName)
        {
            if (this.ContainsKey(fieldName))
            {
                this.Remove(fieldName);
            }
            return this;
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Record.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current Record.</param>
        /// <returns>true if the specified System.Object is Record, has the same fieldNames
        /// as the current record and their values are equal. Otherwise, returns false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            Record another = obj as Record;

            if (another == null)
            {
                return false;
            }

            if (this.Count != another.Count)
            {
                return false;
            }

            foreach (var field in this)
            {
                if (!another.ContainsKey(field.Key))
                {
                    return false;
                }

                var anotherValue = another[field.Key];

                if (field.Value == null)
                {
                    if (anotherValue != null)
                        return false;
                }
                else if (!field.Value.Equals(anotherValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets hash code based on field names and values.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 0;

            foreach (var field in this)
            {
                hash ^= field.Key.GetHashCode();
                hash ^= field.Value == null ? 0 : field.Value.GetHashCode();
            }

            return hash;
        }

        public void AddIfNotEmpty(string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                this[fieldName] = fieldValue;
            }
        }

        public string GetValueAsString(string fieldName)
        {
            object value;
            if (TryGetValue(fieldName, out value))
            {
                var strValue = value as string;
                return string.IsNullOrWhiteSpace(strValue) ? null : strValue.Trim();
            }
            return null;
        }

        public T GetValueAs<T>(string fieldName)
        {
            object value;
            if (!TryGetValue(fieldName, out value) || !(value is T))
                return default(T);

            return (T)value;
        }

        /// <summary>
        /// Clones record value and returns new instance of class Record
        /// </summary>
        /// <returns>new instance of class Record</returns>
        public Record CloneValues()
        {
            Record result = new Record();
            this.Aggregate(result,
                (res, kv) =>
                {
                    res[kv.Key] = kv.Value;
                    return res;
                });
            return result;
        }
    }
}
