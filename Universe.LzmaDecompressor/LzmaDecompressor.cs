// https://github.com/devizer/Universe.LzmaDecompressor/

namespace Universe
{
	using System;
	using System.IO;
	using Universe.LzmaDecompressionImplementation;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA;

	public class LzmaDecompressor
	{
		public class ProgressOptions
		{
			public int MinimumStep = 2 * 1024 * 1024;
			public Action<Progress> NotifyProgress;
		}

		public class Progress
		{
			public ulong CurrentBytes;

			public Progress()
			{
			}

			public Progress(ulong current)
			{
				CurrentBytes = current;
			}

			public override string ToString()
			{
				return CurrentBytes.ToString("n0");
			}
		}

		public static void LzmaDecompressTo(Stream compressed, Stream plain)
		{
			LzmaDecompressTo(compressed, plain, null);
		}

		public static void LzmaDecompressTo(Stream compressed, Stream plain, /* Nullable*/ ProgressOptions progressOptions)
		{
			byte[] properties = new byte[5];
			// a stream can returns 5 bytes in 2+ calls, but FileStream never
			int remainBytes = properties.Length;
			while (remainBytes > 0)
			{
				int n = compressed.Read(properties, properties.Length - remainBytes, remainBytes);
				remainBytes -= n;
				if (n == 0) break;
			}

			if (remainBytes > 0)
				throw new WrongLzmaHeaderException("LZMA Header too short. Missed parameters block");

			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			long outSize = 0;
			for (int i = 0; i < 8; i++)
			{
				int v = compressed.ReadByte();
				if (v < 0)
					throw new WrongLzmaHeaderException("LZMA Header too short. Missed plain size block");

				outSize |= (long) (byte) v << (8 * i);
			}

			long compressedSize = compressed.Length - compressed.Position;
			decoder.Code(compressed, plain, compressedSize, outSize, progressOptions);
		}
	}
}
