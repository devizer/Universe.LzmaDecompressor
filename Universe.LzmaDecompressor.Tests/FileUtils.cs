namespace LzmaDecompressor.Tests
{
	using System;
	using System.IO;
	using NUnit.Framework;

	class FileUtils
	{
		public static string GetTempDir()
		{
			string[] candidates = new[] {"TEMP", "TMPDIR"};
			throw new NotImplementedException();
		}

		public static void AssertStreamsAreEquals(Stream expected, Stream actual)
		{
			Assert.AreEqual(expected.Length, actual.Length, "Streams lengths are different");
			long left = actual.Length, pos = 0;
			const int bufferLength = 62 * 1024;
			byte[] buf1 = new byte[bufferLength], buf2 = new byte[bufferLength];
			while (left > 0)
			{
				int count = (int) Math.Min(bufferLength, left);
				int n1 = expected.Read(buf1, 0, count);
				int n2 = actual.Read(buf2, 0, count);
				if (n1 <= 0 || n2 <= 0 || n1 != n2) throw new InvalidOperationException("Only FileStream and MemoryStream are tested");
				for(int i=0; i<n1; i++)
					if (buf1[i] != buf2[i])
						Assert.Fail($"Streams content at position {(pos + i)}");

				left -= n1;
				pos += n1;
			}
		}
	}
}
