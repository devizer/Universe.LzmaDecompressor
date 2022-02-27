namespace Universe.LzmaDecompressionImplementation
{
	using System;

	/// <summary>
	///     The exception that is thrown when an error in input stream occurs during decoding.
	/// </summary>
	public class LzmaDataErrorException : Exception
	{
		public LzmaDataErrorException() : base("LZMA Data Error")
		{
		}
	}

	/// <summary>
	///     The exception that is thrown when the value of an argument is outside the allowable range.
	/// </summary>
	public class InvalidLzmaParameterException : Exception
	{
		public InvalidLzmaParameterException() : base("Invalid LZMA Parameter")
		{
		}
	}

	public class WrongLzmaHeaderException : Exception
	{
		public WrongLzmaHeaderException(string message) : base(message)
		{
		}
	}
}
