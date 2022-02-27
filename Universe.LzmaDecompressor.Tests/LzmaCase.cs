namespace LzmaDecompressor.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Universe;

    public class LzmaCases
    {
        public static LzmaCase[] GetCases()
        {
            var levels = Enumerable.Repeat(42, 9).Select((x, index) => index + 1).ToArray();
            return CreateCases(
                new char?[] { 'Z', null },
                new[] { 1, 100, 1000, 10000, 100000 },
                levels);
        }

        public static LzmaCase[] CreateCases(char?[] chars, int[] sizes, int[] levels)
        {
            var dir = new DirectoryInfo("LZMA-Test-Temp").FullName;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            List<LzmaCase> ret = new List<LzmaCase>();
            foreach (int level in levels)
            foreach (char? ch in chars)
            foreach (int size in sizes)
            {
                var lzmaCase = new LzmaCase()
                {
                    Size = size,
                    Content = ch,
                    Level = level,
                };
                lzmaCase.PlainFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.txt");
                lzmaCase.CompressedFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.txt.lzma");
                lzmaCase.ActualFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.actual");
                ret.Add(lzmaCase);
            }

            // return ret.ToArray();

            Random rnd = new Random(42);
            foreach (var lzmaCase in ret)
            {
                byte[] content;
                if (lzmaCase.Content.HasValue)
                    content = Enumerable.Repeat((byte) lzmaCase.Content.Value, lzmaCase.Size).ToArray();
                else
                    content = Enumerable.Repeat(42, lzmaCase.Size).Select(x => (byte)rnd.Next(65, 95)).ToArray();

                using (FileStream fs = new FileStream(lzmaCase.PlainFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.Write(content, 0, content.Length);
                }

                Console.WriteLine($"Preparing {lzmaCase}");
                var exe = (CrossInfo.ThePlatform == CrossInfo.Platform.Windows) ? "Binaries\\xz.exe" : "xz";
                ProcessStartInfo si = new ProcessStartInfo(exe, $"--format=lzma -f -z -{lzmaCase.Level} -k \"{lzmaCase.PlainFile}\"");
                using (Process p = Process.Start(si))
                {
                    p.WaitForExit();
                }
            }

            return ret.ToArray();
        }

    }
    public class LzmaCase
    {
        public string CompressedFile;
        public string PlainFile;
        public string ActualFile;

        public int Level;
        public int Size;
        public char? Content; // null - random

        public override string ToString()
        {
            return GetFileName();
        }
        public string GetFileName()
        {
            return $"{Size:n0} bytes, lvl={Level}, {(Content == null ? "random" : $"char {Content}")}";
        }
    }
}
