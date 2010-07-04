using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SimpleCsvParser.Test
{
    public class MockReader : Reader
    {
        protected ReadOnlyCollection<Record> recordsToOutput;

        public ReadOnlyCollection<Record> RecordsToOutput
        {
            get
            {
                return recordsToOutput;
            }
        }

        protected IEnumerator<Record> enumerator;

        public MockReader(IEnumerable<Record> recordsToOutput)
        {
            this.recordsToOutput = new ReadOnlyCollection<Record>(new List<Record>(recordsToOutput));
        }

        public MockReader(params Record[] recordsToOutput)
            : this((IEnumerable<Record>)recordsToOutput)
        {
        }

        public MockReader() { }

        protected override void Open()
        {
            base.Open();
            if (RecordsToOutput != null)
            {
                enumerator = RecordsToOutput.GetEnumerator();
            }
        }

        protected override Record ReadInternal()
        {
            return enumerator != null && enumerator.MoveNext()
                   ? enumerator.Current
                   : null;
        }
    }
}
