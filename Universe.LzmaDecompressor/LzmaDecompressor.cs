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
			public int Milliseconds = 900;
			public int MinimumStep = 1024 * 1024;
			public Action<Progress> NotifyProgress;
		}

		public class Progress
		{
			public ulong Current;

			public Progress()
			{
			}

			public Progress(ulong current)
			{
				Current = current;
			}

			public override string ToString()
			{
				return Current.ToString("n0");
			}
		}


		public static void LzmaDecompressTo(Stream inStream, Stream plainStream)
		{
			LzmaDecompressTo(inStream, plainStream, null);
		}


		public static void LzmaDecompressTo(Stream inStream, Stream plainStream, /* Nullable*/ ProgressOptions progressOptions)
		{
			byte[] properties = new byte[5];
			// a stream can returns 5 bytes in 2+ calls, but FileStream never
			int remainBytes = properties.Length;
			while (remainBytes > 0)
			{
				int n = inStream.Read(properties, properties.Length - remainBytes, remainBytes);
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
				int v = inStream.ReadByte();
				if (v < 0)
					throw new WrongLzmaHeaderException("LZMA Header too short. Missed plain size block");

				outSize |= (long) (byte) v << (8 * i);
			}

			long compressedSize = inStream.Length - inStream.Position;
			decoder.Code(inStream, plainStream, compressedSize, outSize, progressOptions);
		}
	}
}
