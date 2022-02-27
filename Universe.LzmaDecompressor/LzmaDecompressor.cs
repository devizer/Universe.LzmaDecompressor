namespace Universe
{
	using System;
	using System.IO;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA;

	public class LzmaDecompressor
	{
		public static void LzmaDecompressTo(Stream inStream, Stream plainStream)
		{
			var properties = new byte[5];
			if (inStream.Read(properties, 0, 5) != 5)
				throw new Exception("input .lzma is too short");

			var decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			long outSize = 0;
			for (var i = 0; i < 8; i++)
			{
				var v = inStream.ReadByte();
				if (v < 0)
					throw new Exception("Can't Read 1");
				outSize |= (long) (byte) v << (8 * i);
			}

			var compressedSize = inStream.Length - inStream.Position;
			decoder.Code(inStream, plainStream, compressedSize, outSize, null);
		}
	}
}
