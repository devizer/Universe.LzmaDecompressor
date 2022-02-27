namespace Universe
{
	using System.IO;
	using Universe.LzmaDecompressionImplementation;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA;

	public class LzmaDecompressor
	{
		public static void LzmaDecompressTo(Stream inStream, Stream plainStream)
		{
			var properties = new byte[5];
			// TODO: a stream can returns 5 bytes in 2+ calls, but FileStream never
			if (inStream.Read(properties, 0, 5) != 5)
				throw new WrongLzmaHeaderException("LZMA Header too short. Missed parameters block");

			var decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			long outSize = 0;
			for (var i = 0; i < 8; i++)
			{
				var v = inStream.ReadByte();
				if (v < 0)
					throw new WrongLzmaHeaderException("LZMA Header too short. Missed plain size block");

				outSize |= (long) (byte) v << (8 * i);
			}

			var compressedSize = inStream.Length - inStream.Position;
			decoder.Code(inStream, plainStream, compressedSize, outSize, null);
		}
	}
}
