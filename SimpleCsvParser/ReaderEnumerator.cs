using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCsvParser
{
    /// <summary>
    /// Allows iterating through records returned by a Reader.
    /// </summary>
    public class ReaderEnumerator : IEnumerator<Record>
    {
        private readonly Reader reader;

        /// <summary>
        /// Initializes a new instance of the ReaderEnumerator class.
        /// </summary>
        /// <param name="reader">Reader to wrap.</param>
        public ReaderEnumerator(Reader reader)
        {
            this.reader = reader;
            Current = null;
        }

        /// <summary>
        /// Gets the current record from the input
        /// </summary>
        public Record Current { get; private set; }

        /// <summary>
        /// Gets the current record from the input.
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Moves to the next <see cref="Record"/> in the input.
        /// </summary>
        /// <returns>True in case there is a record to read next, otherwise false.</returns>
        public bool MoveNext()
        {
            Current = reader.Read();

            return Current != null;
        }

        /// <summary>
        /// NOTE: Currently not implemented.
        /// Sets the enumerator to the first record in the input.
        /// </summary>
        public void Reset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes iterator.
        /// </summary>
        public void Dispose()
        {
            reader.Dispose();
        }
    }
}