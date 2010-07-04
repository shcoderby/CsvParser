using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCsvParser
{
    /// <summary>
    /// Base class for all readers.
    /// </summary>
    public abstract class Reader : IEnumerable<Record>, IDisposable
    {
        private bool opened = false;

        /// <summary>
        /// Reads a single record.
        /// </summary>
        /// <returns>Another record.</returns>
        public Record Read()
        {
            if (!opened)
                Open();

            return ReadInternal();
        }

        /// <summary>
        /// Reads all records.
        /// </summary>
        /// <returns>All records.</returns>
        public IEnumerable<Record> ReadAll()
        {
            List<Record> result = new List<Record>();

            for (Record record = this.Read(); record != null; record = this.Read())
            {
                result.Add(record);
            }

            return result;
        }

        /// <summary>
        /// Performs necessary actions to prepare the reader to read data.
        /// </summary>
        protected virtual void Open()
        {
            if (opened)
                throw new InvalidOperationException("A reader is already opened");

            opened = true;
        }

        /// <summary>
        /// An internal read which is actually supposed to read the next record.
        /// </summary>
        /// <returns>Next record.</returns>
        protected abstract Record ReadInternal();

        /// <summary>
        /// Disposes reader.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Reader()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Returns a enumerator object which helps to iterate 
        /// through records returned by the reader. 
        /// As opposed to ReadAll which reads all records in memory,
        /// this method reads records one by one. 
        /// </summary>
        /// <returns>Enumerator object.</returns>
        public IEnumerator<Record> GetEnumerator()
        {
            return new ReaderEnumerator(this);
        }

        /// <summary>
        /// Returns a enumerator object which helps to iterate 
        /// through records returned by the reader. 
        /// As opposed to ReadAll which reads all records in memory,
        /// this method reads records one by one. 
        /// </summary>
        /// <returns>Enumerator object.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
