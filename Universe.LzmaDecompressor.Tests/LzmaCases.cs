namespace LzmaDecompressor.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using KernelManagementJam.Benchmarks;
	using Universe;

	public class LzmaCases
	{
		public static LzmaCase[] GetCases()
		{
			var levels = Enumerable.Repeat(42, 10).Select((x, index) => index + 1).ToArray();
			return CreateCases(
				new char?[] {'Z', null},
				new[] {1, 100, 1000, 10000, 100000},
				levels);
		}

		private static LzmaCase[] CreateCases(char?[] chars, int[] sizes, int[] levels)
		{
			var ret = new List<LzmaCase>();
			foreach (var level in levels)
			foreach (var ch in chars)
			foreach (var size in sizes)
			{
				var lzmaCase = new LzmaCase()
				{
					Size = size,
					Content = ch,
					Level = level
				};
				ret.Add(lzmaCase);
			}

			foreach(var size in new[] { 10000000 , 100000000})
			ret.Add(new LzmaCase()
			{
				Size = size,
				Level = 1,
				Content = null
			});

			PrepareCases(ret);
			return ret.ToArray();
		}

		public static void PrepareCases(List<LzmaCase> ret)
		{
			var dir = new DirectoryInfo("LZMA-Test-Temp").FullName;
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			var rnd = new Random(42);
			foreach (var lzmaCase in ret)
			{
				lzmaCase.PlainFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.txt");
				lzmaCase.CompressedFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.txt.lzma");
				lzmaCase.ActualFile = Path.Combine(dir, $"{lzmaCase.GetFileName()}.actual");

				byte[] content;
				if (lzmaCase.Content.HasValue)
					content = Enumerable.Repeat((byte) lzmaCase.Content.Value, lzmaCase.Size).ToArray();
				else
				{
					// content = new byte[lzmaCase.Size];
					// XorShiftRandom.FillByteArray(content, 42);
					// for (int i = 0; i < lzmaCase.Size; i++) content[i] = (byte) (33 + (content[i] & 0x1F));
					content = Enumerable.Repeat(42, lzmaCase.Size).Select(x => (byte) rnd.Next(65, 90)).ToArray();
				}

				using (var fs = new FileStream(lzmaCase.PlainFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
				{
					fs.Write(content, 0, content.Length);
				}

				Console.WriteLine($"Preparing {lzmaCase}");
				var exe = CrossInfo.ThePlatform == CrossInfo.Platform.Windows ? "Binaries\\xz.exe" : "xz";
				var lvl = lzmaCase.Level < 10 ? $"-{lzmaCase.Level}" : "-9 -e";
				var si = new ProcessStartInfo(exe, $"--format=lzma -f -z {lvl} -k \"{lzmaCase.PlainFile}\"");
				using (var p = Process.Start(si))
				{
					p.WaitForExit();
				}
			}
		}
	}
}
