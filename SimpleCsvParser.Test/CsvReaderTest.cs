using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    /// <summary>
    /// Tests for CsvReader.
    /// </summary>
    [TestClass]
    public class CsvReaderTest
    {
        /// <summary>
        /// Gets full file path where plain pipe delimited test data will be stored. 
        /// </summary>
        public static string InputPlainPipeSeparatedFilePath { get; private set; }

        /// <summary>
        /// Gets full file path where gzipped pipe delimited test data will be stored.
        /// </summary>
        public static string InputGzipPipeSeparatedFilePath { get; private set; }

        /// <summary>
        /// Gets full file path where plain pipe delimited test data will be stored. 
        /// </summary>
        public static string InputPlainCommaSeparatedFilePath { get; private set; }

        /// <summary>
        /// Gets full file path where gzipped pipe delimited test data will be stored.
        /// </summary>
        public static string InputGzipCommaSeparatedFilePath { get; private set; }

        /// <summary>
        /// Performes these actions once prior to executing any of the tests in the fixture:
        /// 1. Creates on disk a uniquely named temporary file with plain CSV data.
        ///    File name is <see cref="InputPlainFilePath"/>.
        /// 2. Creates on disk a uniquely named temporary file with the same, but gzipped CSV data.
        ///    File name is <see cref="InputGzipFilePath"/>.
        /// </summary>
        /// <param name="testContext">Test context.</param>
        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var pipeDelimitedContent = "A|B|C\n10|20|30\nXXX|YYY|ZZZ";

            var commaSeparatedContent = pipeDelimitedContent.Replace('|', ',');

            InputPlainPipeSeparatedFilePath = EnsureTempFileExists(InputPlainPipeSeparatedFilePath);
            InputGzipPipeSeparatedFilePath = EnsureTempFileExists(InputGzipPipeSeparatedFilePath);
            InputPlainCommaSeparatedFilePath = EnsureTempFileExists(InputPlainCommaSeparatedFilePath);
            InputGzipCommaSeparatedFilePath = EnsureTempFileExists(InputGzipCommaSeparatedFilePath);

            File.WriteAllText(InputPlainPipeSeparatedFilePath, pipeDelimitedContent);
            GzipFile(InputPlainPipeSeparatedFilePath, InputGzipPipeSeparatedFilePath);

            File.WriteAllText(InputPlainCommaSeparatedFilePath, commaSeparatedContent);
            GzipFile(InputPlainCommaSeparatedFilePath, InputGzipCommaSeparatedFilePath);
        }

        /// <summary>
        /// Deletes both plain and gzipped files.
        /// </summary>
        [ClassCleanup]
        public static void Cleanup()
        {
            DeleteIfExists(InputPlainPipeSeparatedFilePath);
            DeleteIfExists(InputGzipPipeSeparatedFilePath);
            DeleteIfExists(InputPlainCommaSeparatedFilePath);
            DeleteIfExists(InputGzipCommaSeparatedFilePath);
        }

        /// <summary>
        /// Tests Read() method on plain comma separated text file (not gzipped).
        /// </summary>
        [TestMethod]
        public void ReadPlainCommaSeparatedTextTest()
        {
            ReadTest(false, false);
        }

        /// <summary>
        /// Tests Read() method on gzipped comma separated text file.
        /// </summary>
        [TestMethod]
        public void ReadGzipCommaSeparatedTextTest()
        {
            ReadTest(false, true);
        }

        /// <summary>
        /// Tests Read() method on plain pipe delimited text file (not gzipped).
        /// </summary>
        [TestMethod]
        public void ReadPlainPipeDelimitedTextTest()
        {
            ReadTest(true, false);
        }

        /// <summary>
        /// Tests Read() method on gzipped pipe delimited text file.
        /// </summary>
        [TestMethod]
        public void ReadGzipPipeDelimitedTextTest()
        {
            ReadTest(true, true);
        }

        /// <summary>
        /// Parent method for all for CSV tests.
        /// </summary>
        /// <param name="pipeDelimited">Whether CSV reader should be pipe delimited or comma delimited.</param>
        /// <param name="gzipped">Whether CSV reader should be gzipped or not.</param>
        public void ReadTest(bool pipeDelimited, bool gzipped)
        {
            var firstExpectedRecord = new Record();
            firstExpectedRecord["A"] = "10";
            firstExpectedRecord["B"] = "20";
            firstExpectedRecord["C"] = "30";

            var secondExpectedRecord = new Record();
            secondExpectedRecord["A"] = "XXX";
            secondExpectedRecord["B"] = "YYY";
            secondExpectedRecord["C"] = "ZZZ";

            using (Reader reader = CreateReader(pipeDelimited, gzipped))
            {
                Record firstRecord = reader.Read();
                CollectionAssert.AreEquivalent(firstExpectedRecord, firstRecord);

                var secondRecord = reader.Read();
                CollectionAssert.AreEquivalent(secondExpectedRecord, secondRecord);

                var thirdRecord = reader.Read();
                Assert.IsNull(thirdRecord);
            }
        }

        /// <summary>
        /// If <paramref name="filePath"/> is null,
        /// creates new temporary file and returns <paramref name="filePath"/>
        /// its full path. Otherwise returns <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">File path to check.</param>
        /// <returns>Path to temporary file.</returns>
        private static string EnsureTempFileExists(string filePath)
        {
            return filePath == null ? Path.GetTempFileName() : filePath;
        }

        /// <summary>
        /// Deletes file if it exists.
        /// </summary>
        /// <param name="fileName">File to delete.</param>
        private static void DeleteIfExists(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        /// <summary>
        /// Gzips content from <paramref name="inputFileName"/> to <paramref name="outputFileName"/>.
        /// </summary>
        /// <param name="inputFileName">File to compress.</param>
        /// <param name="outputFileName">Output file.</param>
        private static void GzipFile(string inputFileName, string outputFileName)
        {
            var uncompressedData = File.ReadAllBytes(inputFileName);

            using (var stream = new FileStream(outputFileName, FileMode.Open))
            using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, false))
            {
                gzipStream.Write(uncompressedData, 0, uncompressedData.Length);
            }
        }

        [TestMethod]
        public void SkipLinesTest()
        {
            StringBuilder csvFile = new StringBuilder();

            csvFile.AppendLine("header");
            csvFile.AppendLine("column1|column2");
            csvFile.AppendLine("value1|value2");

            using (StringReader stringReader = new StringReader(csvFile.ToString()))
            using (CsvReader reader = new CsvReader(stringReader, '|', 1))
            {
                Assert.AreEqual(1, reader.LinesToSkip);
                IEnumerable<Record> records = reader.ReadAll();
                Assert.AreEqual(1, records.Count());
                Assert.AreEqual("value1", records.First()["column1"]);
            }
        }

        [TestMethod]
        public void QuotedValueTest()
        {
            StringBuilder csvFile = new StringBuilder();
            csvFile.AppendLine("column1,column2,column3");
            csvFile.AppendLine("v1,\"v21, v22\",\"v31, \"\"v32\"\", v33\"");
            using (StringReader stringReader = new StringReader(csvFile.ToString()))
            {
                CsvReader reader = new CsvReader(stringReader);
                IEnumerable<Record> records = reader.ReadAll();
                Assert.AreEqual(1, records.Count());
                Assert.AreEqual("v1", records.First()["column1"]);
                Assert.AreEqual("v21, v22", records.First()["column2"]);
                Assert.AreEqual("v31, \"v32\", v33", records.First()["column3"]);
            }
        }

        [TestMethod]
        public void SkipEmptyValuesTest()
        {
            StringBuilder csvFile = new StringBuilder();

            csvFile.AppendLine("column1|column2");
            csvFile.AppendLine("value1|");

            using (StringReader skippingStringReader = new StringReader(csvFile.ToString()))
            using (StringReader nonSkippingstringReader = new StringReader(csvFile.ToString()))
            using (CsvReader skippingReader = new CsvReader(skippingStringReader, '|', 0, true))
            using (CsvReader nonSkippingReader = new CsvReader(nonSkippingstringReader, '|', 0))
            {
                var records = skippingReader.ReadAll();
                Assert.IsFalse(records.First().ContainsKey("column2"));
                Assert.AreEqual("value1", records.First()["column1"]);

                records = nonSkippingReader.ReadAll();
                Assert.IsTrue(records.First().ContainsKey("column2"));
            }
        }

        /// <summary>
        /// Creates a new CSV reader depending on parameters.
        /// </summary>
        /// <param name="pipeDelimited">Whether CSV reader should be pipe delimited or comma delimited.</param>
        /// <param name="gzipped">Whether CSV reader should be gzipped or not.</param>
        /// <returns>New instance of CSV reader.</returns>
        private Reader CreateReader(bool pipeDelimited, bool gzipped)
        {
            if (pipeDelimited)
            {
                return gzipped
                       ? new CsvReader(InputGzipPipeSeparatedFilePath, '|', true)
                       : new CsvReader(InputPlainPipeSeparatedFilePath, '|');
            }
            else
            {
                return gzipped
                       ? new CsvReader(InputGzipCommaSeparatedFilePath, ',', true)
                       : new CsvReader(InputPlainCommaSeparatedFilePath);
            }
        }
    }
}
