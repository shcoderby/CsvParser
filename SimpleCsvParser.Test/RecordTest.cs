using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Configuration;

namespace SimpleCsvParser.Test
{

    /// <summary>
    ///This is a test class for RecordTest and is intended
    ///to contain all RecordTest Unit Tests
    ///</summary>
    [TestClass]
    public class RecordTest
    {
        Record targetRecord;
        Record recordToAdd;

        /// <summary>
        /// Initailizes two records. Will be performed before each test method.
        /// </summary>
        [TestInitialize]
        public void CreateRecords()
        {
            targetRecord = new Record();
            targetRecord["A"] = 1;
            targetRecord["B"] = "2";

            recordToAdd = new Record();
            recordToAdd["B"] = "200";
            recordToAdd["C"] = "300";
        }

        /// <summary>
        ///A test for Record's Add/Get
        ///</summary>
        [TestMethod()]
        public void RecordAddGetTest()
        {
            targetRecord.Add("myfield", 1);
            Assert.AreEqual(1, targetRecord["myfield"]);
        }

        /// <summary>
        /// Tests record merging (with property overwrite)
        /// </summary>
        [TestMethod]
        public void AddFieldsWithOverwriteTest()
        {
            AddFieldsTest(true);
        }

        /// <summary>
        /// Tests record merging (without property overwrite)
        /// </summary>
        [TestMethod]
        public void AddFieldsWithoutOverwriteTest()
        {
            AddFieldsTest(false);
        }

        /// <summary>
        /// Used by AddFieldsWithoutOverwriteTest and AddFieldsWithOverwriteTest methods.
        /// </summary>
        private void AddFieldsTest(bool overwrite)
        {
            targetRecord.Add(recordToAdd, overwrite);

            var expected = new Record();
            expected["A"] = 1;
            expected["B"] = overwrite ? "200" : "2";
            expected["C"] = "300";

            CollectionAssert.AreEquivalent(expected, targetRecord);
        }

        /// <summary>
        /// Tests WithField method
        /// </summary>
        [TestMethod]
        public void WithFieldTest()
        {
            var record = new Record();
            record["ID"] = 123;

            var updatedRecord = record.WithField("Field", "Value");

            var expectedRecord = new Record();
            expectedRecord["ID"] = 123;
            expectedRecord["Field"] = "Value";

            CollectionAssert.AreEquivalent(expectedRecord, updatedRecord);

            Assert.AreSame(record, updatedRecord);
        }

        [TestMethod]
        public void AddIfNotEmptyTest()
        {
            var record = new Record { { "ID", 123 } };
            var expectedRecord = new Record { { "ID", 123 }, { "A", "B" } };
            record.AddIfNotEmpty("A", "B");
            CollectionAssert.AreEquivalent(expectedRecord, record);

            record.AddIfNotEmpty("A", "");
            CollectionAssert.AreEquivalent(expectedRecord, record);

            record.AddIfNotEmpty("A", null);
            CollectionAssert.AreEquivalent(expectedRecord, record);

            record.AddIfNotEmpty("E", "");
            CollectionAssert.AreEquivalent(expectedRecord, record);

            record.AddIfNotEmpty("E", null);
            CollectionAssert.AreEquivalent(expectedRecord, record);
        }

        /// <summary>
        /// Tests Record.Equals() and Record.GetHashCode().
        /// </summary>
        [TestMethod]
        public void TestEqualsAndGetHashCode()
        {
            var recordAB1 = new Record();
            recordAB1["A"] = "B";

            var recordAB2 = new Record();
            recordAB2["A"] = "B";

            var recordABCD = new Record();
            recordABCD["A"] = "B";
            recordABCD["C"] = "D";

            var recordAnull1 = new Record();
            recordAnull1["A"] = null;

            var recordAnull2 = new Record();
            recordAnull2["A"] = null;

            AssertEquals(recordAB1, recordAB2);
            AssertEquals(recordAnull1, recordAnull2);
            AssertNotEquals(recordAB1, recordABCD);
            AssertNotEquals(recordABCD, null);
        }

        /// <summary>
        /// Cloning record values test
        /// </summary>
        [TestMethod]
        public void TectCloneValues()
        {
            Record record = new Record
            {
                {"Field1", "Value1"},
                {"Field2", "Value2"},
                {"Field3", "Value3"}
            };
            Record newRecord = record.CloneValues();
            Assert.IsFalse(Record.ReferenceEquals(newRecord, record));//new instance has been created
            Assert.IsTrue(RecordsAreEqual(newRecord, record));//record values are equally
        }

        [TestMethod]
        public void WithoutFieldTest()
        {
            Record record = new Record
            {
                {"Field1", "Value1"},
                {"Field2", "Value2"},
                {"Field3", "Value3"}
            };
            record = record.WithoutField("Field2");
            Assert.IsFalse(record.ContainsKey("Field2"));
        }

        /// <summary>
        /// Two records are equal when are fileds of them are equal
        /// </summary>
        /// <param name="a">first record</param>
        /// <param name="b">second record</param>
        /// <returns>true/false</returns>
        private bool RecordsAreEqual(Record a, Record b)
        {
            bool result = true;
            result = a.Aggregate(result,
                (res, kv) =>
                {
                    res &= kv.Value.ToString() == b.GetValueAsString(kv.Key);//all field values are equal
                    return res;
                });
            return result;
            //a.All(avp => b.Any(bvp => avp.Key == bvp.Key && Object.Equals(avp.Value, bvp.Value))) && b.All(bvp => a.Any(avp => avp.Key == bvp.Key && Object.Equals(avp.Value, bvp.Value)));
        }

        /// <summary>
        /// Helper method to check whether records 
        /// and their hash codes are equal.
        /// </summary>
        /// <param name="record1">First record.</param>
        /// <param name="record2">Second record.</param>
        private void AssertEquals(Record record1, Record record2)
        {
            Assert.IsTrue(record1.Equals(record2));
            Assert.IsTrue(record2.Equals(record1));
            Assert.AreEqual(record1.GetHashCode(), record2.GetHashCode());
        }

        /// <summary>
        /// Helper method to check whether records are not equal.
        /// </summary>
        /// <param name="record1">First record.</param>
        /// <param name="record2">Second record.</param>
        private void AssertNotEquals(Record record1, Record record2)
        {
            Assert.IsFalse(record1.Equals(record2));
            if (record2 != null)
            {
                Assert.IsFalse(record2.Equals(record1));
            }
        }
    }
}
