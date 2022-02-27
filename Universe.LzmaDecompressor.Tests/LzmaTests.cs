namespace LzmaDecompressor.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using Universe;
    using Universe.NUnitTests;

    public class LzmaTests : NUnitTestsBase
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LzmaCase lzmaCase = LzmaCases.CreateCases(new char?[] {'A'}, new int[] {1}, new int[] {1}).First();
            Console.WriteLine($"JIT LZMA Case: {lzmaCase}");
            try
            {
                TestAll(lzmaCase);
            }
            catch{}
        }

        [Test]
        [TestCaseSource(typeof(LzmaCases), nameof(LzmaCases.GetCases))]
        public void TestAll(LzmaCase lzmaCase)
        {
            MemoryStream expected = new MemoryStream();
            using (FileStream fs = new FileStream(lzmaCase.PlainFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.CopyTo(expected);
            }

            MemoryStream actual = new MemoryStream();
            using (FileStream compressed = new FileStream(lzmaCase.CompressedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                LzmaDecompressor.LzmaDecompressTo(compressed, actual);
            }

            Assert.AreEqual(Convert.ToBase64String(expected.ToArray()), Convert.ToBase64String(actual.ToArray()));
        }


    }
}
