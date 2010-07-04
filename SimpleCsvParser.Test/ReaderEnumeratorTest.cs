using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class ReaderEnumeratorTest
    {
        private const string ID = "ID";

        [TestMethod]
        public void ReaderTest()
        {
            var records = new List<Record> 
            {
                new Record {{ID, 1}}, 
                new Record {{ID, 2}},
                new Record {{ID, 3}}
            };

            var reader = new MockReader(records);

            var count = 0;

            foreach (var record in reader)
            {
                Assert.IsTrue(records.Contains(record), "Couldn't find record with ID=" + record[ID] + "in the input");
                count++;
            }

            Assert.AreEqual(3, count, "Records count in the input don't equal to the readed count by ReaderEnumerator");
        }

        [TestMethod]
        public void EmptyReaderTest()
        {
            var records = new List<Record>();

            var reader = new MockReader(records);

            var enumerator = reader.GetEnumerator();

            Assert.IsNull(enumerator.Current, "In the empty input current record should be null");

            Assert.IsFalse(enumerator.MoveNext(), "Empty input hasn't next record");
        }
    }
}
