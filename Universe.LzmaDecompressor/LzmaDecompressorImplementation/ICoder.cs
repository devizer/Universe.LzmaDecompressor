namespace Universe.LzmaDecompressionImplementation.SevenZip
{
	using System.IO;


	internal interface ICodeProgress
	{
		void SetProgress(ulong current, ulong total);
	}

	internal interface ICoder
	{
		/// <summary>
		///     Codes streams.
		/// </summary>
		/// <param name="inStream">
		///     input Stream.
		/// </param>
		/// <param name="outStream">
		///     output Stream.
		/// </param>
		/// <param name="inSize">
		///     input Size. -1 if unknown.
		/// </param>
		/// <param name="outSize">
		///     output Size. -1 if unknown.
		/// </param>
		/// <param name="progressOptions">
		///     callback progress reference.
		/// </param>
		/// <exception cref="LzmaDataErrorException">
		///     if input stream is not valid
		/// </exception>
		void Code(Stream inStream, Stream outStream,
			long inSize, long outSize, LzmaDecompressor.ProgressOptions progressOptions);
	}

	public interface ISetDecoderProperties
	{
		void SetDecoderProperties(byte[] properties);
	}
}
