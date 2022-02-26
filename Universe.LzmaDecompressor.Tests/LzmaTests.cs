namespace Universe.LzmaDecompressor.Tests
{
    using NUnit.Framework;
    using Universe.NUnitTests;

    public class LzmaTests : NUnitTestsBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCaseSource(typeof(LzmaCases), nameof(LzmaCases.GetCases))]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}