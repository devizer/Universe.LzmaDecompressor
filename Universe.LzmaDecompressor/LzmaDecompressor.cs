namespace Universe
{
	using System;
	using System.IO;
	using Universe.LzmaDecompressionImplementation;
	using Universe.LzmaDecompressionImplementation.SevenZip;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA;

	public class LzmaDecompressor
	{
		public class Progress
		{
			public long Compressed { get; set; }
			public long Plain { get; set; }
			public long TotalPlain { get; set; }

			public override string ToString()
			{
				return $"{nameof(Compressed)}: {Compressed}, {nameof(Plain)}: {Plain}, {nameof(TotalPlain)}: {TotalPlain}";
			}
		}


		internal class CustomProgress : ICodeProgress
		{
			public Action<Progress> ProgressCallback;
			public long TotalPlain { get; set; }
			public void SetProgress(long inSize, long outSize)
			{
				if (ProgressCallback != null)
					ProgressCallback(new Progress()
					{
						Compressed = inSize,
						Plain = outSize,
						TotalPlain = TotalPlain
					});
			}
		}

		public static void LzmaDecompressTo(Stream inStream, Stream plainStream)
		{
			LzmaDecompressTo(inStream, plainStream, null);
		}


		public static void LzmaDecompressTo(Stream inStream, Stream plainStream, /* Nullable*/ Action<Progress> progressCallback)
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
			CustomProgress progressNotifier = null;
			if (progressCallback != null)
			{
				progressNotifier = new CustomProgress() {TotalPlain = outSize, ProgressCallback = progressCallback};
			}
			decoder.Code(inStream, plainStream, compressedSize, outSize, progressNotifier);
		}
	}

}
