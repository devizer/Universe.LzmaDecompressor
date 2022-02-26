namespace Universe.LzmaDecompressor.Tests
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Universe.NUnitTests;

    public class LzmaTests : NUnitTestsBase
    {
        [SetUp]
        public void Setup()
        {
            LzmaCase lzmaCase = LzmaCases.CreateCases(new char?[] {'A'}, new int[] {1}, new int[] {1}).First();
            Console.WriteLine($"JIT LZMA Case: {lzmaCase}");
        }

        [Test]
        [TestCaseSource(typeof(LzmaCases), nameof(LzmaCases.GetCases))]
        public void TestAll(LzmaCase lzmaCase)
        {
            Assert.Pass();
        }
    }
}