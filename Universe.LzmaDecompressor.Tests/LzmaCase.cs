namespace LzmaDecompressor.Tests
{
	public class LzmaCase
	{
		public string CompressedFile;
		public string PlainFile;
		public string ActualFile;

		public int Level;
		public long Size;
		public char? Content; // null - random

		public override string ToString()
		{
			return GetFileName();
		}

		public string GetFileName()
		{
			var lvl = Level == 10 ? "9-extreme" : Level.ToString();
			return $"{Size:n0} bytes, lvl={lvl}, {(Content == null ? "random" : $"char {Content}")}";
		}
	}
}
