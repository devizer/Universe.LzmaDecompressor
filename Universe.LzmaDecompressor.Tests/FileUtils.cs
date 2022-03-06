namespace LzmaDecompressor.Tests
{
	using System;
	using System.IO;
	using NUnit.Framework;
	using Universe;

	class FileUtils
	{
		public static string GetTempDir()
		{
			string[] candidates = new[] {"TEMP", "TMPDIR"};
			throw new NotImplementedException();
		}

		public static unsafe void AssertStreamsAreEquals(Stream expected, Stream actual)
		{
			Assert.AreEqual(expected.Length, actual.Length, "Streams lengths are different");
			long left = actual.Length, pos = 0;
			const int bufferLength = 62 * 1024;
			byte[] buf1 = new byte[bufferLength], buf2 = new byte[bufferLength];
			fixed (byte* ptr1 = buf1)
			fixed (byte* ptr2 = buf2)
			while (left > 0)
			{
				int count = (int) Math.Min(bufferLength, left);
				int n1 = expected.Read(buf1, 0, count);
				int n2 = actual.Read(buf2, 0, count);
				if (n1 <= 0 || n2 <= 0 || n1 != n2) throw new InvalidOperationException("Only FileStream and MemoryStream are tested");
				var equals = Memory.Compare(ptr1, ptr2, n1);
				if (!equals)
					for (int i = 0; i < n1; i++)
						if (buf1[i] != buf2[i])
							Assert.Fail($"Streams content at position {(pos + i)} of {expected.Length}");

					left -= n1;
				pos += n1;
			}
		}
	}
}
