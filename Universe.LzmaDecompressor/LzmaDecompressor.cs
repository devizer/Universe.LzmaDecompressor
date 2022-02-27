namespace Universe
{
	using System;
	using System.IO;
	using Universe.LzmaDecompressionImplementation.SevenZip;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA;

	public class LzmaDecompressor
	{
		public static void LzmaDecompressTo(Stream inStream, Stream plainStream)
		{
			byte[] properties = new byte[5];
			// TODO: stream can returns 5 bytes in 2+ calls
			if (inStream.Read(properties, 0, 5) != 5)
				throw new WrongLzmaHeaderException("LZMA Header too short. Missed parameters block");

			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			long outSize = 0;
			for (var i = 0; i < 8; i++)
			{
				var v = inStream.ReadByte();
				if (v < 0)
					throw new WrongLzmaHeaderException("LZMA Header too short. Missed plain size block");

				outSize |= (long) (byte) v << (8 * i);
			}

			long compressedSize = inStream.Length - inStream.Position;
			decoder.Code(inStream, plainStream, compressedSize, outSize, null);
		}
	}
}
