namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.RangeCoder
{
	using System.IO;

	internal class Decoder
	{
		public const uint kTopValue = 1 << 24;
		public uint Code;

		public uint Range;

		public Stream Stream;

		public void Init(Stream stream)
		{
			Stream = stream;

			Code = 0;
			Range = 0xFFFFFFFF;
			for (int i = 0; i < 5; i++)
				Code = (Code << 8) | (byte) Stream.ReadByte();
		}

		public void ReleaseStream()
		{
			Stream = null;
		}

		public void CloseStream()
		{
			Stream.Dispose();
		}

		public uint DecodeDirectBits(int numTotalBits)
		{
			uint range = Range;
			uint code = Code;
			uint result = 0;
			for (int i = numTotalBits; i > 0; i--)
			{
				range >>= 1;
				uint t = (code - range) >> 31;
				code -= range & (t - 1);
				result = (result << 1) | (1 - t);

				if (range < kTopValue)
				{
					code = (code << 8) | (byte) Stream.ReadByte();
					range <<= 8;
				}
			}

			Range = range;
			Code = code;
			return result;
		}

	}
}
