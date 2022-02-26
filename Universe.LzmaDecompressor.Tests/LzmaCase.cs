namespace Universe.LzmaDecompressor.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using NUnit.Framework;

    public class LzmaCases
    {
        public static LzmaCase[] GetCases()
        {
            var dir = new DirectoryInfo("LZMA-Test-Temp").FullName;
            List<LzmaCase> ret = new List<LzmaCase>();
            foreach (char? ch in new char?[] {'Z', null})
                foreach(int size in new [] { 1, 100, 1000, 10000})
                {
                    var lzmaCase = new LzmaCase()
                    {
                        Size = size,
                        Content = ch,
                    };
                    lzmaCase.PlainFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.plain");
                    lzmaCase.CompressedFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.lzma");
                    lzmaCase.ActualFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.actual");
                    ret.Add(lzmaCase);
                }

            return ret.ToArray();
        }
    }
    public class LzmaCase
    {
        public string CompressedFile;
        public string PlainFile;
        public string ActualFile;

        public int Size;
        public char? Content; // null - random

        public override string ToString()
        {
            return GetFileName();
        }
        public string GetFileName()
        {
            return $"{Size:n0} bytes, {(Content == null ? "random" : $"char {Content}")}";
        }
    }
}