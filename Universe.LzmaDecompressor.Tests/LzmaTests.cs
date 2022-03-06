namespace LzmaDecompressor.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
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
			var lzmaCase = new LzmaCase()
			{
				Content = 'S',
				Level = 1,
				Size = 1
			};

			Console.WriteLine($"JIT LZMA Case: {lzmaCase}");
			try
			{
				LzmaCases.PrepareCases(new List<LzmaCase> {lzmaCase});
				TestAll(lzmaCase);
			}
			catch
			{
			}
		}

		[Test]
		[TestCaseSource(typeof(LzmaCases), nameof(LzmaCases.GetCases))]
		public void TestAll(LzmaCase lzmaCase)
		{

			bool hasProgressNotification = false;
			// var actualFileName = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "tmp-lzma-actual", Path.GetFileName(lzmaCase.PlainFile));
			Console.WriteLine($"actualFileName: {lzmaCase.ActualFile}, Size: {lzmaCase.Size}");
			// var actual = new MemoryStream();
			using (var actual = new FileStream(lzmaCase.ActualFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 8*1024))
			using (var compressed = new FileStream(lzmaCase.CompressedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024))
			{
				Stopwatch startAt = Stopwatch.StartNew();
				void ShowProgress(LzmaDecompressor.Progress info)
				{
					Console.WriteLine($"{startAt.Elapsed} Progress: {info}");
					hasProgressNotification = true;
				}

				var step = lzmaCase.Size >= 100000 ? lzmaCase.Size / 10 : 4000;
				var progressOptions = new LzmaDecompressor.ProgressOptions()
				{
					Bytes = step,
					NotifyProgress = ShowProgress
				};

				if (lzmaCase.Size <= 1)
					progressOptions = null;

				LzmaDecompressor.LzmaDecompressTo(compressed, actual, progressOptions);
			}

			// actual.Position = 0;
			using (var actual = new FileStream(lzmaCase.ActualFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8 * 1024))
			using (var expected = new FileStream(lzmaCase.PlainFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8*1024))
				FileUtils.AssertStreamsAreEquals(expected, actual);

			if (lzmaCase.Size >= 10000)
				Assert.IsTrue(hasProgressNotification, "Tests sized over 10,000 bytes should have progress notification");

		}


	}
}
