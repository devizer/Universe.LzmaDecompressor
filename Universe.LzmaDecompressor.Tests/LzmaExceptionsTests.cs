namespace LzmaDecompressor.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using NUnit.Framework;
	using Universe;
	using Universe.NUnitTests;

	public class LzmaExceptionsTests : NUnitTestsBase
	{

		private static byte[] ValidLzma;
		static LzmaExceptionsTests()
		{
			var raw = Raw.Replace("0x", "").Replace(",", "").Replace(" ", "").Replace("\r", "").Replace("\n", "");
			List<byte> bytes = new List<byte>();
			for (int i = 0; i < raw.Length; i += 2)
				bytes.Add((byte) int.Parse(raw.Substring(i,2), NumberStyles.HexNumber));

			ValidLzma = bytes.ToArray();
		}

		[Test]
		public void SimpleExceptionTests()
		{
			// var lengths = Enumerable.Range(0, ValidLzma.Length + 1).ToList();
			var lengths = Enumerable.Range(0, 101).ToList();
			if (!lengths.Contains(ValidLzma.Length)) lengths.Add(ValidLzma.Length);
			lengths.Remove(16); // OOM
			lengths.Remove(17);
			foreach (int len in lengths)
			{
				TestInvalidLzma(len);
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		private static void TestInvalidLzma(int len)
		{
			bool isOkExpected = len == ValidLzma.Length;
			bool isOkActual;
			try
			{
				using (MemoryStream compressed = new MemoryStream(ValidLzma, 0, len))
				using (MemoryStream actual = new MemoryStream())
				{
					LzmaDecompressor.LzmaDecompressTo(compressed, actual);
					Console.WriteLine($"OK LEN={len}");
					isOkActual = true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR LEN={len}, {ex.GetType()} {ex.Message}");
				isOkActual = false;
			}

			if (!isOkExpected && isOkActual) Assert.Fail("Should throw exception");
			if (isOkExpected && !isOkActual) Assert.Fail("Should NOT throw exception");
		}


		private const string Raw = @"
0x5D, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x22, 0x09, 
0x06, 0x00, 0x62, 0xEB, 0xF6, 0x70, 0x31, 0xDA, 0x41, 0xFD, 0xE1, 0x81, 0x13, 0x1D, 0xD6, 0xB9, 
0x98, 0xA6, 0x11, 0xCD, 0xC1, 0x5F, 0xB1, 0x88, 0x3E, 0xAA, 0x13, 0x79, 0xAF, 0xF7, 0x33, 0xF1, 
0x45, 0xFD, 0x1B, 0x3F, 0x85, 0x71, 0x16, 0x06, 0x1D, 0xEF, 0x88, 0xA8, 0xBC, 0x5C, 0x52, 0x34, 
0xA9, 0x90, 0xB3, 0xEE, 0x6A, 0xDE, 0xFB, 0x9E, 0xB4, 0x02, 0x6F, 0xB3, 0xAD, 0x51, 0xC4, 0xAA, 
0xA4, 0xB9, 0xC1, 0x86, 0xE6, 0x12, 0xEA, 0x11, 0x06, 0xE0, 0xF8, 0x68, 0x4F, 0x04, 0xEA, 0x16, 
0xBF, 0x16, 0x19, 0x89, 0xBD, 0xF9, 0x29, 0x15, 0x7C, 0xDF, 0x4E, 0xE8, 0xB4, 0x32, 0xC1, 0xFB, 
0x00, 0xBE, 0x48, 0x2F, 0x6E, 0x7A, 0x95, 0x95, 0xF2, 0x20, 0x5C, 0x40, 0x67, 0x13, 0xAD, 0xC8, 
0x8E, 0xF7, 0xEE, 0xF7, 0x78, 0x6B, 0xF6, 0x63, 0x28, 0x32, 0x70, 0xDA, 0xF4, 0x58, 0x55, 0x82, 
0x02, 0x4A, 0x29, 0xE8, 0x98, 0xC5, 0x58, 0x24, 0x6C, 0x25, 0xF3, 0x8B, 0x9A, 0x7D, 0x16, 0xAE, 
0x5B, 0xC4, 0x83, 0x3B, 0x5A, 0x92, 0x54, 0x70, 0x4E, 0xD9, 0x50, 0x1B, 0x8F, 0x2D, 0x65, 0x2C, 
0x4A, 0x7F, 0xF1, 0x24, 0x3C, 0x27, 0x9F, 0x7A, 0xA5, 0x5C, 0x4D, 0x02, 0x61, 0x87, 0x9D, 0xF8, 
0x29, 0x2A, 0xF3, 0xE6, 0xF0, 0xA4, 0x1F, 0x45, 0x68, 0x48, 0x2D, 0x10, 0xD8, 0x8D, 0xB0, 0xE6, 
0x6A, 0x37, 0x8C, 0xE1, 0xEC, 0x7A, 0x56, 0x6E, 0x2D, 0x10, 0x13, 0x38, 0x46, 0x39, 0x9A, 0x68, 
0xF1, 0xF6, 0xD8, 0x30, 0xFF, 0x76, 0xB2, 0xA8, 0x1C, 0x21, 0xDA, 0xE4, 0xF1, 0xBB, 0x0D, 0x46, 
0xCB, 0x34, 0xFB, 0xDA, 0xF8, 0x55, 0xA9, 0xE5, 0x40, 0x4F, 0x96, 0x33, 0x5E, 0xA8, 0x31, 0x07, 
0xF7, 0xEF, 0x1E, 0x94, 0xD7, 0x6F, 0x27, 0x9B, 0x32, 0x8D, 0x30, 0x85, 0x4A, 0xA7, 0x42, 0x4F, 
0xD1, 0x67, 0x0C, 0xDE, 0x66, 0xE4, 0xA3, 0x08, 0x3A, 0x75, 0xB9, 0xD7, 0xEC, 0xE2, 0x64, 0x1D, 
0xB2, 0xD2, 0x84, 0x3D, 0xFC, 0x1E, 0x31, 0x22, 0x88, 0x0E, 0xC4, 0x53, 0x0D, 0x71, 0xFD, 0x62, 
0xE4, 0x0A, 0x6A, 0x4B, 0x30, 0x59, 0x54, 0x7C, 0x26, 0x74, 0x3D, 0xFF, 0x03, 0xD4, 0xBC, 0x73, 
0x62, 0x2E, 0x35, 0x19, 0x91, 0xE6, 0x0B, 0xDD, 0x51, 0x30, 0x1F, 0xB6, 0x87, 0x0D, 0x2E, 0x23, 
0x93, 0xBD, 0x90, 0x97, 0x4B, 0x87, 0xC5, 0x82, 0x7D, 0x84, 0x13, 0x13, 0x05, 0xDE, 0xE1, 0xAF, 
0x55, 0xD1, 0x92, 0x64, 0x8E, 0xC6, 0x11, 0xD4, 0x8A, 0xBB, 0x2B, 0xD9, 0x31, 0x82, 0xA4, 0xAD, 
0x36, 0x29, 0x9C, 0x12, 0xD3, 0xED, 0xF6, 0x60, 0xCA, 0x11, 0x41, 0x8F, 0x88, 0x61, 0x04, 0x68, 
0xF1, 0x68, 0xE2, 0x5A, 0x42, 0x2C, 0xAF, 0xE7, 0x6E, 0x1C, 0x3F, 0xC3, 0x0C, 0xEF, 0x47, 0x2C, 
0xF3, 0xF2, 0xA1, 0x5B, 0xBA, 0xB9, 0x8F, 0x30, 0xB9, 0xA9, 0xEF, 0x6D, 0xA4, 0x6B, 0xE3, 0xF1, 
0xB9, 0x0B, 0x3C, 0x29, 0xF5, 0x69, 0x0E, 0x14, 0x75, 0x33, 0x15, 0xE1, 0xAA, 0xD3, 0x94, 0x1A, 
0x2A, 0x5A, 0x8F, 0x7B, 0x0B, 0xDB, 0xA2, 0x73, 0x51, 0x74, 0x2B, 0x0C, 0xB5, 0xFD, 0x0C, 0x0E, 
0x6D, 0xED, 0x1E, 0xF2, 0x7F, 0xDB, 0x4C, 0x33, 0xF3, 0xDD, 0xD8, 0x86, 0x89, 0x2F, 0x28, 0xF1, 
0x5A, 0xC3, 0x41, 0x64, 0xF7, 0x0C, 0xA9, 0xCB, 0x65, 0x04, 0x7A, 0x61, 0x43, 0x51, 0x6F, 0xDF, 
0xE7, 0x58, 0xDB, 0x93, 0x2F, 0xDA, 0xC2, 0xAD, 0xA9, 0xAE, 0x7F, 0xB3, 0x39, 0x57, 0xFC, 0xD5, 
0x99, 0x0C, 0x09, 0xC9, 0xAB, 0x37, 0xEF, 0x85, 0xC4, 0x60, 0xAE, 0xC9, 0x03, 0x4C, 0x7D, 0x6F, 
0x76, 0xBD, 0x21, 0x36, 0x5E, 0xB9, 0x9E, 0x42, 0x3A, 0x16, 0xD4, 0x79, 0x33, 0x91, 0xC7, 0x90, 
0x42, 0x89, 0xEB, 0x8C, 0x74, 0xDC, 0x93, 0xF6, 0x1D, 0xC0, 0xC8, 0x9A, 0x98, 0x33, 0x25, 0x19, 
0x6A, 0xED, 0x51, 0x20, 0xC5, 0x56, 0x77, 0x65, 0x4F, 0xE8, 0xA6, 0x36, 0xEB, 0xFD, 0x5B, 0xE9, 
0x95, 0x18, 0x5F, 0x53, 0x4A, 0xB6, 0xA6, 0xFD, 0xEB, 0xF5, 0x56, 0x77, 0x0A, 0x93, 0x76, 0x8A, 
0x4A, 0x9C, 0xF3, 0x75, 0xF8, 0x80, 0x1B, 0x31, 0x39, 0xEC, 0x0F, 0x6E, 0xFD, 0xB2, 0xD9, 0x7C, 
0x33, 0xBE, 0x01, 0x05, 0x33, 0x97, 0x11, 0xBC, 0x05, 0xB9, 0x03, 0x15, 0x97, 0xAB, 0x27, 0x64, 
0x3C, 0x75, 0x51, 0x2B, 0x9F, 0x9E, 0x07, 0xC0, 0xB0, 0xBC, 0x12, 0x42, 0x16, 0x1A, 0x45, 0xF3, 
0x9D, 0x2D, 0xC3, 0x39, 0x7E, 0x80, 0xFD, 0xA3, 0xEE, 0x71, 0x1C, 0xE7, 0xC5, 0xD1, 0xD2, 0x9D, 
0x92, 0xD7, 0xC9, 0x63, 0x05, 0xAD, 0xFE, 0x58, 0x26, 0x68, 0x6F, 0x89, 0x3A, 0xAE, 0x38, 0x22, 
0xA3, 0x4B, 0x4F, 0x6F, 0x8D, 0xFA, 0xF6, 0xBD, 0xAF, 0x82, 0xAC, 0x6A, 0xF9, 0xC3, 0xD9, 0x1F, 
0x51, 0xD1, 0xC7, 0xCB, 0xC9, 0xBC, 0x4B, 0x46, 0x3D, 0xC3, 0x33, 0xCD, 0x22, 0x4F, 0xD1, 0xAE, 
0xC7, 0x41, 0x11, 0x68, 0x2B, 0x3A, 0xB6, 0x4A, 0xF8, 0x6A, 0xD6, 0xEF, 0x17, 0x2A, 0x73, 0x7D, 
0xF3, 0xCE, 0x90, 0x87, 0xBD, 0x18, 0x2E, 0x25, 0x6E, 0xC7, 0xAB, 0xE6, 0x49, 0xB0, 0x9C, 0x76, 
0x4E, 0xE7, 0x04, 0x29, 0x99, 0x6C, 0x3F, 0x21, 0xCB, 0xF0, 0x25, 0xE7, 0xDB, 0x05, 0x27, 0xCA, 
0xF4, 0x50, 0xCA, 0x54, 0xD9, 0x05, 0x51, 0x29, 0x28, 0x38, 0xB3, 0xFB, 0x28, 0x5F, 0xAA, 0x20, 
0xE2, 0x82, 0x15, 0x93, 0xA1, 0x99, 0xB9, 0x58, 0x7B, 0x91, 0x5F, 0x13, 0xC3, 0x9C, 0xB2, 0xA6, 
0xC5, 0x44, 0x04, 0x67, 0x26, 0x5D, 0x47, 0x56, 0x79, 0x38, 0x60, 0x41, 0x86, 0x42, 0xA8, 0x19, 
0xF9, 0xAC, 0x53, 0xCD, 0x74, 0xF4, 0x0C, 0x64, 0x31, 0xCB, 0x61, 0x60, 0xF8, 0x38, 0xED, 0x3E, 
0xA8, 0xA8, 0xF9, 0x4E, 0xD2, 0x49, 0x36, 0xCC, 0x6B, 0xF8, 0x5A, 0x95, 0xF6, 0x91, 0x66, 0x5E, 
0xEC, 0x87, 0x68, 0x98, 0x42, 0xFC, 0x6B, 0xE3, 0xFB, 0x91, 0x58, 0xDF, 0xA8, 0x7F, 0xB9, 0x0F, 
0x6C, 0xE9, 0x10, 0x63, 0x9B, 0x3A, 0x16, 0xB4, 0x6C, 0xEA, 0x46, 0x09, 0xA6, 0x30, 0x49, 0xE5, 
0x9A, 0x46, 0x68, 0x00, 0xA1, 0x90, 0xAD, 0xD5, 0x39, 0xDF, 0xA9, 0xB0, 0xE9, 0xC9, 0x4A, 0x06, 
0x80, 0x9A, 0x7D, 0xB5, 0x54, 0xC5, 0xFB, 0x78, 0xA5, 0x9E, 0xDD, 0x3C, 0x8F, 0x53, 0x06, 0x28, 
0x8C, 0x06, 0xE4, 0x18, 0xD1, 0x70, 0xEA, 0xF5, 0x42, 0x37, 0x9D, 0x1A, 0xB3, 0xE8, 0x1D, 0xD6, 
0x70, 0x37, 0x03, 0x1B, 0xF3, 0xA0, 0x4F, 0xDA, 0xC2, 0x6A, 0xCD, 0x52, 0x7E, 0x1D, 0x57, 0xAD, 
0xDE, 0xC1, 0x77, 0x93, 0xCC, 0x5E, 0xCE, 0xA8, 0xFF, 0xE3, 0x26, 0x3D, 0xAE, 0x51, 0x07, 0x1C, 
0x94, 0x9A, 0x1D, 0x19, 0x34, 0x98, 0x56, 0x0B, 0xAE, 0xC6, 0x1B, 0xB5, 0x1A, 0xBE, 0x8F, 0xE0, 
0x60, 0x24, 0xA3, 0xB4, 0x97, 0xCF, 0xF9, 0x45, 0xE1, 0x37, 0x9B, 0x22, 0xD9, 0xA0, 0x35, 0xBE, 
0xD9, 0xDE, 0x2E, 0x79, 0x15, 0xED, 0x1E, 0x84, 0xFF, 0x62, 0x09, 0x69, 0x59, 0x6C, 0x47, 0xAB, 
0xFE, 0x6C, 0xC5, 0x06, 0x8C, 0xD8, 0x6E, 0x52, 0x44, 0xC4, 0xF8, 0x4E, 0xE0, 0x5B, 0x28, 0xE3, 
0x65, 0xD6, 0x20, 0x9D, 0x98, 0x5F, 0xB0, 0x0A, 0xCC, 0x9A, 0x6D, 0x2D, 0x44, 0x91, 0xC7, 0x1F, 
0xB5, 0x52, 0x32, 0xC6, 0xD7, 0x2D, 0xCC, 0x8C, 0x8B, 0xB5, 0x71, 0x4B, 0xD9, 0x6F, 0x17, 0x92, 
0x55, 0xCE, 0x32, 0xA9, 0xFC, 0x71, 0x8A, 0xEC, 0x54, 0xC6, 0xD2, 0xA4, 0xD5, 0xBB, 0xD9, 0x6C, 
0x2F, 0x28, 0xEC, 0x7E, 0x73, 0xD3, 0x3A, 0xC2, 0x97, 0x97, 0x2A, 0x34, 0x98, 0xBB, 0x5E, 0x41, 
0x4E, 0xEF, 0xEE, 0x54, 0x29, 0x6B, 0x61, 0x79, 0x22, 0xC0, 0x52, 0x23, 0x98, 0xAC, 0xB3, 0xDD, 
0xE6, 0x8D, 0x5C, 0x1C, 0xA2, 0x54, 0xD8, 0x28, 0x2C, 0xD4, 0x10, 0x6E, 0xAC, 0x10, 0xC5, 0x75, 
0x8E, 0x0A, 0xFB, 0x29, 0xCD, 0x3B, 0xBE, 0x94, 0xED, 0xA6, 0x82, 0x2D, 0xDF, 0x73, 0x86, 0xE7, 
0x58, 0x01, 0x9E, 0xFD, 0x0B, 0x5F, 0x18, 0x46, 0x1C, 0x25, 0x15, 0x78, 0xE7, 0xEC, 0x9A, 0x17, 
0x63, 0x2F, 0x66, 0x85, 0xA4, 0xCC, 0x32, 0x03, 0x3A, 0x02, 0xFF, 0x44, 0x1D, 0xF2, 0xAF, 0xC1, 
0x11, 0x89, 0xFF, 0xE8, 0x7A, 0x85, 0xE6, 0xFB, 0x66, 0xCD, 0xB6, 0xEF, 0x4D, 0x43, 0xFD, 0x6B, 
0xFC, 0xE1, 0xB6, 0xE2, 0x9B, 0x70, 0xAE, 0x00, 0x18, 0x38, 0xBD, 0xFC, 0xF4, 0x47, 0xD4, 0xEC, 
0xF1, 0x1D, 0x5B, 0xF4, 0x99, 0x7A, 0xC2, 0x2B, 0x2C, 0xA9, 0x62, 0xD1, 0x44, 0x61, 0x32, 0xA1, 
0x5C, 0x11, 0x1B, 0x77, 0x64, 0xCD, 0xDA, 0x27, 0xF8, 0xE9, 0x6F, 0x42, 0x0B, 0xAA, 0xD9, 0x5A, 
0x38, 0x3E, 0x28, 0xEF, 0x66, 0xF3, 0x72, 0x02, 0x94, 0x5E, 0xAB, 0xB2, 0xCF, 0x20, 0x56, 0xED, 
0x7B, 0x01, 0xF1, 0x71, 0xA7, 0x4D, 0x95, 0xA5, 0xC2, 0xB8, 0x3A, 0xE3, 0x9D, 0x76, 0xFE, 0x6F, 
0x59, 0x42, 0xF2, 0xC5, 0x4A, 0x10, 0x56, 0x4D, 0x67, 0x57, 0x8A, 0x5B, 0x26, 0xFB, 0xBE, 0xA2, 
0xA7, 0x78, 0x52, 0x73, 0x1B, 0x41, 0xD5, 0x30, 0xD9, 0x8E, 0x05, 0x21, 0xB4, 0x25, 0x78, 0x64, 
0x4C, 0x72, 0x75, 0x0F, 0xE0, 0x34, 0xA8, 0x0D, 0x24, 0xFD, 0xFA, 0x9C, 0x45, 0x1B, 0x3A, 0x5F, 
0x8B, 0xA8, 0xE2, 0x90, 0xD5, 0x0C, 0x44, 0xBD, 0x53, 0x2D, 0xB3, 0x12, 0x96, 0xC4, 0xCC, 0x3A, 
0x65, 0x9D, 0xEA, 0x3E, 0x4B, 0x32, 0xDF, 0x3C, 0xE4, 0xF1, 0x13, 0x9D, 0xD0, 0xFD, 0xC3, 0x79, 
0x55, 0x32, 0x67, 0x6D, 0x82, 0x5E, 0xFE, 0x79, 0x54, 0xA3, 0xE2, 0x18, 0x5C, 0x72, 0x87, 0x42, 
0x6C, 0x86, 0x42, 0x8C, 0x15, 0x0A, 0xA5, 0x9E, 0x21, 0xB8, 0x63, 0xE0, 0x1D, 0x94, 0xDD, 0x84, 
0x0C, 0xEE, 0x1F, 0xF2, 0x4C, 0x82, 0xC1, 0x6A, 0x5A, 0x8A, 0x4A, 0x6A, 0x8F, 0xB7, 0x6E, 0x44, 
0x04, 0x94, 0x8B, 0x99, 0x3B, 0xDB, 0xFF, 0xC7, 0xDD, 0xA3, 0x45, 0xDA, 0x5B, 0x8C, 0x52, 0x2D, 
0xEA, 0x13, 0x44, 0x2A, 0x10, 0x83, 0xB3, 0x80, 0x69, 0xF1, 0xB6, 0xE0, 0xDB, 0x00, 0x84, 0x0C, 
0xDF, 0xA7, 0xB2, 0x0A, 0x5E, 0x6A, 0xD0, 0xDC, 0x07, 0x84, 0x2B, 0x3F, 0x92, 0xF3, 0x85, 0xE6, 
0x2C, 0x3B, 0xD4, 0x90, 0x5B, 0x65, 0x5E, 0x2B, 0xFA, 0x20, 0x98, 0x25, 0x79, 0x3F, 0xDB, 0x09, 
0x15, 0x35, 0xFA, 0x15, 0x60, 0x1E, 0x48, 0x44, 0x34, 0x68, 0xD9, 0x01, 0x80, 0x75, 0x5A, 0xC8, 
0xEF, 0x78, 0x2C, 0x69, 0x8C, 0xC7, 0xD8, 0xC4, 0xDD, 0xA7, 0xB6, 0xBE, 0x08, 0xE8, 0x84, 0xFF, 
0xA4, 0x35, 0xDE, 0x1A, 0x84, 0x5E, 0xB4, 0xB7, 0xD1, 0x1C, 0x97, 0xE4, 0xEE, 0x93, 0x0D, 0xB6, 
0x50, 0x48, 0x83, 0xB9, 0x42, 0x42, 0x1E, 0xED, 0x69, 0x71, 0xDB, 0x00, 0x37, 0xE3, 0x6D, 0x58, 
0x3D, 0x68, 0x61, 0xD5, 0x03, 0x99, 0xB0, 0xC6, 0x57, 0xC7, 0xA0, 0x38, 0x29, 0x1C, 0x89, 0x80, 
0xE3, 0x4E, 0x25, 0x17, 0x40, 0xCE, 0xAD, 0xE1, 0x40, 0x03, 0xE7, 0xFA, 0x79, 0xF4, 0x88, 0x22, 
0xEE, 0xBD, 0xDC, 0x4D, 0x3A, 0xC0, 0xFE, 0x18, 0xCD, 0x0C, 0xDC, 0xED, 0x73, 0x7F, 0x62, 0x22, 
0x36, 0x53, 0x53, 0x44, 0xEF, 0x09, 0xA8, 0x73, 0x03, 0x8D, 0xE3, 0xB2, 0x4B, 0x30, 0x9D, 0xBA, 
0x47, 0xE9, 0xBE, 0x3D, 0x8E, 0xC9, 0x03, 0xDB, 0x2F, 0xF8, 0x57, 0x10, 0x6A, 0x2F, 0xA8, 0xAB, 
0x36, 0x85, 0x66, 0x0F, 0x37, 0x5A, 0x1F, 0xDC, 0x37, 0xEE, 0xD5, 0x5D, 0x45, 0x7D, 0xF6, 0xA1, 
0xA5, 0x7E, 0xA6, 0x5E, 0x3E, 0x90, 0x37, 0x72, 0x96, 0x96, 0x3D, 0x9B, 0x56, 0x84, 0x6A, 0xF7, 
0x34, 0xC1, 0x48, 0xAA, 0x5A, 0x46, 0xA7, 0xBE, 0x64, 0xB9, 0xC2, 0x0F, 0x3D, 0x8E, 0x4F, 0x66, 
0xB1, 0x6F, 0xEC, 0xA9, 0x36, 0x3D, 0xAC, 0x23, 0xAA, 0xC8, 0x60, 0x73, 0x99, 0x32, 0x88, 0x9D, 
0x4D, 0x51, 0xCA, 0xBA, 0x87, 0x37, 0x9E, 0x77, 0xEC, 0x8B, 0xAC, 0x1B, 0x51, 0x27, 0xA1, 0xA7, 
0x2D, 0x47, 0xFB, 0x79, 0xAE, 0x8B, 0x7B, 0xE7, 0xF7, 0x6A, 0xF7, 0xFA, 0xAA, 0x8A, 0x13, 0x65, 
0x22, 0xDB, 0x46, 0x24, 0xC1, 0x8B, 0x8E, 0x16, 0xBF, 0x8B, 0x1B, 0x05, 0xDD, 0x97, 0x08, 0x60, 
0x39, 0x63, 0x0E, 0x37, 0x1A, 0x4A, 0xDD, 0x4E, 0x55, 0x2F, 0xD0, 0x5E, 0x9B, 0xDD, 0x22, 0x2B, 
0x6A, 0x97, 0x23, 0x81, 0x56, 0xA5, 0xE7, 0xC4, 0xDF, 0xE7, 0xD1, 0x38, 0x64, 0x78, 0x0B, 0x19, 
0x32, 0x45, 0x4E, 0xB8, 0x8E, 0x79, 0x27, 0xFE, 0x0D, 0x93, 0x3F, 0xB8, 0x9D, 0xDF, 0xC5, 0x52, 
0xAC, 0x63, 0xC4, 0x4E, 0x5E, 0xF5, 0x47, 0xA8, 0xE1, 0x81, 0x32, 0x70, 0xC4, 0x1F, 0xD7, 0x64, 
0xCF, 0x9F, 0x4D, 0xA7, 0x62, 0xCE, 0xA1, 0x40, 0x66, 0x17, 0xAA, 0x54, 0x03, 0xB6, 0x23, 0xD4, 
0x5F, 0x6D, 0x43, 0x85, 0x9B, 0xF6, 0x07, 0x58, 0x9D, 0xA7, 0x7C, 0x2C, 0xDC, 0x9A, 0x5F, 0x00, 
0x25, 0xD9, 0x77, 0x59, 0xD9, 0x9D, 0xF0, 0xBB, 0x81, 0x5D, 0x98, 0x4E, 0xD3, 0xC6, 0xFD, 0x89, 
0x0B, 0x72, 0x85, 0x69, 0x10, 0xD4, 0xD8, 0xC6, 0x61, 0xE2, 0x07, 0x6E, 0xDA, 0x97, 0x5F, 0x71, 
0x1B, 0x25, 0x11, 0x87, 0x4E, 0xD9, 0xD5, 0x6D, 0xFF, 0xEE, 0x78, 0xAF, 0x94";
	}
}
