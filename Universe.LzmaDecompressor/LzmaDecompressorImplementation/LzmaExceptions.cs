namespace Universe.LzmaDecompressionImplementation.SevenZip
{
	using System;

	/// <summary>
	///     The exception that is thrown when an error in input stream occurs during decoding.
	/// </summary>
	internal class DataErrorException : Exception
	{
		public DataErrorException() : base("LZMA Data Error")
		{
		}
	}

	/// <summary>
	///     The exception that is thrown when the value of an argument is outside the allowable range.
	/// </summary>
	internal class InvalidParamException : Exception
	{
		public InvalidParamException() : base("Invalid LZMA Parameter")
		{
		}
	}

	internal class WrongLzmaHeaderException : Exception
	{
		public WrongLzmaHeaderException(string message) : base(message)
		{
		}
	}


}
