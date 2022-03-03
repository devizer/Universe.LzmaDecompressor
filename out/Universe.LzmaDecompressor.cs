namespace Universe
{
	using System;
	using System.IO;
	using Universe.LzmaDecompressionImplementation;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA;

	public class LzmaDecompressor
	{
		public class ProgressOptions
		{
			public int Milliseconds = 900;
			public int Bytes = 1024 * 1024;
			public Action<Progress> NotifyProgress;
		}

		public class Progress
		{
			public ulong Current;

			public Progress()
			{
			}

			public Progress(ulong current)
			{
				Current = current;
			}

			public override string ToString()
			{
				return Current.ToString("n0");
			}
		}


		public static void LzmaDecompressTo(Stream inStream, Stream plainStream)
		{
			LzmaDecompressTo(inStream, plainStream, null);
		}


		public static void LzmaDecompressTo(Stream inStream, Stream plainStream, /* Nullable*/ ProgressOptions progressOptions)
		{
			byte[] properties = new byte[5];
			// a stream can returns 5 bytes in 2+ calls, but FileStream never
			int remainBytes = properties.Length;
			while (remainBytes > 0)
			{
				int n = inStream.Read(properties, properties.Length - remainBytes, remainBytes);
				remainBytes -= n;
				if (n == 0) break;
			}

			if (remainBytes > 0)
				throw new WrongLzmaHeaderException("LZMA Header too short. Missed parameters block");

			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			long outSize = 0;
			for (int i = 0; i < 8; i++)
			{
				int v = inStream.ReadByte();
				if (v < 0)
					throw new WrongLzmaHeaderException("LZMA Header too short. Missed plain size block");

				outSize |= (long) (byte) v << (8 * i);
			}

			long compressedSize = inStream.Length - inStream.Position;
			decoder.Code(inStream, plainStream, compressedSize, outSize, progressOptions);
		}
	}
}
namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA
{
	internal abstract class Base
	{
		public const uint kNumRepDistances = 4;
		public const uint kNumStates = 12;

		public const int kNumPosSlotBits = 6;

		public const int kDicLogSizeMin = 0;

		public const int kNumLenToPosStatesBits = 2; // it's for speed optimization
		public const uint kNumLenToPosStates = 1 << kNumLenToPosStatesBits;

		public const uint kMatchMinLen = 2;

		public const int kNumAlignBits = 4;
		public const uint kAlignTableSize = 1 << kNumAlignBits;
		public const uint kAlignMask = kAlignTableSize - 1;

		public const uint kStartPosModelIndex = 4;
		public const uint kEndPosModelIndex = 14;
		public const uint kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;

		public const uint kNumFullDistances = 1 << ((int) kEndPosModelIndex / 2);

		public const uint kNumLitPosStatesBitsEncodingMax = 4;
		public const uint kNumLitContextBitsMax = 8;

		public const int kNumPosStatesBitsMax = 4;
		public const uint kNumPosStatesMax = 1 << kNumPosStatesBitsMax;
		public const int kNumPosStatesBitsEncodingMax = 4;
		public const uint kNumPosStatesEncodingMax = 1 << kNumPosStatesBitsEncodingMax;

		public const int kNumLowLenBits = 3;
		public const int kNumMidLenBits = 3;
		public const int kNumHighLenBits = 8;
		public const uint kNumLowLenSymbols = 1 << kNumLowLenBits;
		public const uint kNumMidLenSymbols = 1 << kNumMidLenBits;

		public const uint kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols +
		                                   (1 << kNumHighLenBits);

		public const uint kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;

		public static uint GetLenToPosState(uint len)
		{
			len -= kMatchMinLen;
			if (len < kNumLenToPosStates)
				return len;
			return kNumLenToPosStates - 1;
		}

		public struct State
		{
			public uint Index;

			public void Init()
			{
				Index = 0;
			}

			public void UpdateChar()
			{
				if (Index < 4) Index = 0;
				else if (Index < 10) Index -= 3;
				else Index -= 6;
			}

			public void UpdateMatch()
			{
				Index = (uint) (Index < 7 ? 7 : 10);
			}

			public void UpdateRep()
			{
				Index = (uint) (Index < 7 ? 8 : 11);
			}

			public void UpdateShortRep()
			{
				Index = (uint) (Index < 7 ? 9 : 11);
			}

			public bool IsCharState()
			{
				return Index < 7;
			}
		}
	}
}
namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZMA
{
	using System;
	using System.IO;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZ;
	using Universe.LzmaDecompressionImplementation.SevenZip.Compression.RangeCoder;

	internal class Decoder
	{
		private readonly BitDecoder[] m_IsMatchDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

		private readonly BitDecoder[] m_IsRep0LongDecoders =
			new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

		private readonly BitDecoder[] m_IsRepDecoders = new BitDecoder[Base.kNumStates];
		private readonly BitDecoder[] m_IsRepG0Decoders = new BitDecoder[Base.kNumStates];
		private readonly BitDecoder[] m_IsRepG1Decoders = new BitDecoder[Base.kNumStates];
		private readonly BitDecoder[] m_IsRepG2Decoders = new BitDecoder[Base.kNumStates];

		private readonly LenDecoder m_LenDecoder = new LenDecoder();

		private readonly LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

		private readonly OutWindow m_OutWindow = new OutWindow();
		private readonly BitDecoder[] m_PosDecoders = new BitDecoder[Base.kNumFullDistances - Base.kEndPosModelIndex];

		private readonly BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[Base.kNumLenToPosStates];
		private readonly RangeCoder.Decoder m_RangeDecoder = new RangeCoder.Decoder();
		private readonly LenDecoder m_RepLenDecoder = new LenDecoder();

		private uint m_DictionarySize;
		private uint m_DictionarySizeCheck;

		private BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(Base.kNumAlignBits);

		private uint m_PosStateMask;

		public Decoder()
		{
			m_DictionarySize = 0xFFFFFFFF;
			for (int i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i] = new BitTreeDecoder(Base.kNumPosSlotBits);
		}

		public void Code(Stream inStream, Stream outStream,
			long inSize, long outSize, LzmaDecompressor.ProgressOptions progressOptions)
		{
			bool needProgress = progressOptions != null && progressOptions.NotifyProgress != null;
			ulong stepBytesProgress = needProgress ? (ulong) progressOptions.Bytes : 0;
			ulong prevProgress = 0;

			Init(inStream, outStream);

			Base.State state = new Base.State();
			state.Init();
			uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

			ulong nowPos64 = 0;
			ulong outSize64 = (ulong) outSize;
			if (nowPos64 < outSize64)
			{
				if (m_IsMatchDecoders[state.Index << Base.kNumPosStatesBitsMax].Decode(m_RangeDecoder) != 0)
					throw new LzmaDataErrorException();
				state.UpdateChar();
				byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0, 0);
				m_OutWindow.PutByte(b);
				nowPos64++;
			}

			while (nowPos64 < outSize64)
				// UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
				// while(nowPos64 < next)
			{
				uint posState = (uint) nowPos64 & m_PosStateMask;
				if (m_IsMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) ==
				    0)
				{
					byte b;
					byte prevByte = m_OutWindow.GetByte(0);
					if (!state.IsCharState())
						b = m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder,
							(uint) nowPos64, prevByte, m_OutWindow.GetByte(rep0));
					else
						b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint) nowPos64, prevByte);
					m_OutWindow.PutByte(b);
					state.UpdateChar();
					nowPos64++;
				}
				else
				{
					uint len;
					if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
					{
						if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
						{
							if (m_IsRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState]
								.Decode(m_RangeDecoder) == 0)
							{
								state.UpdateShortRep();
								m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
								nowPos64++;
								continue;
							}
						}
						else
						{
							uint distance;
							if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
							{
								distance = rep1;
							}
							else
							{
								if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
								{
									distance = rep2;
								}
								else
								{
									distance = rep3;
									rep3 = rep2;
								}

								rep2 = rep1;
							}

							rep1 = rep0;
							rep0 = distance;
						}

						len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + Base.kMatchMinLen;
						state.UpdateRep();
					}
					else
					{
						rep3 = rep2;
						rep2 = rep1;
						rep1 = rep0;
						len = Base.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
						state.UpdateMatch();
						uint posSlot = m_PosSlotDecoder[Base.GetLenToPosState(len)].Decode(m_RangeDecoder);
						if (posSlot >= Base.kStartPosModelIndex)
						{
							int numDirectBits = (int) ((posSlot >> 1) - 1);
							rep0 = (2 | (posSlot & 1)) << numDirectBits;
							if (posSlot < Base.kEndPosModelIndex)
							{
								rep0 += BitTreeDecoder.ReverseDecode(m_PosDecoders,
									rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
							}
							else
							{
								rep0 += m_RangeDecoder.DecodeDirectBits(
									numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits;
								rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
							}
						}
						else
						{
							rep0 = posSlot;
						}
					}

					if (rep0 >= m_OutWindow.TrainSize + nowPos64 || rep0 >= m_DictionarySizeCheck)
					{
						if (rep0 == 0xFFFFFFFF)
							break;
						throw new LzmaDataErrorException();
					}

					m_OutWindow.CopyBlock(rep0, len);
					nowPos64 += len;
				}

				if (needProgress && nowPos64 - prevProgress > stepBytesProgress)
				{
					prevProgress = nowPos64;
					progressOptions.NotifyProgress(new LzmaDecompressor.Progress(nowPos64));
				}
			}

			m_OutWindow.Flush();
			m_OutWindow.ReleaseStream();
			m_RangeDecoder.ReleaseStream();
		}

		public void SetDecoderProperties(byte[] properties)
		{
			if (properties.Length < 5)
				throw new InvalidLzmaParameterException();
			int lc = properties[0] % 9;
			int remainder = properties[0] / 9;
			int lp = remainder % 5;
			int pb = remainder / 5;
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidLzmaParameterException();
			uint dictionarySize = 0;
			for (int i = 0; i < 4; i++)
				dictionarySize += (uint) properties[1 + i] << (i * 8);
			SetDictionarySize(dictionarySize);
			SetLiteralProperties(lp, lc);
			SetPosBitsProperties(pb);
		}

		private void SetDictionarySize(uint dictionarySize)
		{
			if (m_DictionarySize != dictionarySize)
			{
				m_DictionarySize = dictionarySize;
				m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
				uint blockSize = Math.Max(m_DictionarySizeCheck, 1 << 12);
				m_OutWindow.Create(blockSize);
			}
		}

		private void SetLiteralProperties(int lp, int lc)
		{
			if (lp > 8)
				throw new InvalidLzmaParameterException();
			if (lc > 8)
				throw new InvalidLzmaParameterException();
			m_LiteralDecoder.Create(lp, lc);
		}

		private void SetPosBitsProperties(int pb)
		{
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidLzmaParameterException();
			uint numPosStates = (uint) 1 << pb;
			m_LenDecoder.Create(numPosStates);
			m_RepLenDecoder.Create(numPosStates);
			m_PosStateMask = numPosStates - 1;
		}

		private void Init(Stream inStream, Stream outStream)
		{
			m_RangeDecoder.Init(inStream);
			m_OutWindow.Init(outStream, false);

			uint i;
			for (i = 0; i < Base.kNumStates; i++)
			{
				for (uint j = 0; j <= m_PosStateMask; j++)
				{
					uint index = (i << Base.kNumPosStatesBitsMax) + j;
					m_IsMatchDecoders[index].Init();
					m_IsRep0LongDecoders[index].Init();
				}

				m_IsRepDecoders[i].Init();
				m_IsRepG0Decoders[i].Init();
				m_IsRepG1Decoders[i].Init();
				m_IsRepG2Decoders[i].Init();
			}

			m_LiteralDecoder.Init();
			for (i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i].Init();
			// m_PosSpecDecoder.Init();
			for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
				m_PosDecoders[i].Init();

			m_LenDecoder.Init();
			m_RepLenDecoder.Init();
			m_PosAlignDecoder.Init();
		}

		private class LenDecoder
		{
			private readonly BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
			private readonly BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
			private BitDecoder m_Choice;
			private BitDecoder m_Choice2;
			private BitTreeDecoder m_HighCoder = new BitTreeDecoder(Base.kNumHighLenBits);
			private uint m_NumPosStates;

			public void Create(uint numPosStates)
			{
				for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
				{
					m_LowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
					m_MidCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
				}

				m_NumPosStates = numPosStates;
			}

			public void Init()
			{
				m_Choice.Init();
				for (uint posState = 0; posState < m_NumPosStates; posState++)
				{
					m_LowCoder[posState].Init();
					m_MidCoder[posState].Init();
				}

				m_Choice2.Init();
				m_HighCoder.Init();
			}

			public uint Decode(RangeCoder.Decoder rangeDecoder, uint posState)
			{
				if (m_Choice.Decode(rangeDecoder) == 0) return m_LowCoder[posState].Decode(rangeDecoder);

				uint symbol = Base.kNumLowLenSymbols;
				if (m_Choice2.Decode(rangeDecoder) == 0)
				{
					symbol += m_MidCoder[posState].Decode(rangeDecoder);
				}
				else
				{
					symbol += Base.kNumMidLenSymbols;
					symbol += m_HighCoder.Decode(rangeDecoder);
				}

				return symbol;
			}
		}

		private class LiteralDecoder
		{
			private Decoder2[] m_Coders;
			private int m_NumPosBits;
			private int m_NumPrevBits;
			private uint m_PosMask;

			public void Create(int numPosBits, int numPrevBits)
			{
				if (m_Coders != null && m_NumPrevBits == numPrevBits &&
				    m_NumPosBits == numPosBits)
					return;
				m_NumPosBits = numPosBits;
				m_PosMask = ((uint) 1 << numPosBits) - 1;
				m_NumPrevBits = numPrevBits;
				uint numStates = (uint) 1 << (m_NumPrevBits + m_NumPosBits);
				m_Coders = new Decoder2[numStates];
				for (uint i = 0; i < numStates; i++)
					m_Coders[i].Create();
			}

			public void Init()
			{
				uint numStates = (uint) 1 << (m_NumPrevBits + m_NumPosBits);
				for (uint i = 0; i < numStates; i++)
					m_Coders[i].Init();
			}

			private uint GetState(uint pos, byte prevByte)
			{
				return ((pos & m_PosMask) << m_NumPrevBits) + (uint) (prevByte >> (8 - m_NumPrevBits));
			}

			public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
			{
				return m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder);
			}

			public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
			{
				return m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte);
			}

			private struct Decoder2
			{
				private BitDecoder[] m_Decoders;

				public void Create()
				{
					m_Decoders = new BitDecoder[0x300];
				}

				public void Init()
				{
					for (int i = 0; i < 0x300; i++) m_Decoders[i].Init();
				}

				public byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
				{
					uint symbol = 1;
					do
					{
						symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
					} while (symbol < 0x100);

					return (byte) symbol;
				}

				public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, byte matchByte)
				{
					uint symbol = 1;
					do
					{
						uint matchBit = (uint) (matchByte >> 7) & 1;
						matchByte <<= 1;
						uint bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
						symbol = (symbol << 1) | bit;
						if (matchBit != bit)
						{
							while (symbol < 0x100)
								symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
							break;
						}
					} while (symbol < 0x100);

					return (byte) symbol;
				}
			}
		}
	}
}
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
namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.LZ
{
	using System.IO;

	internal class OutWindow
	{
		private byte[] _buffer;
		private uint _pos;
		private Stream _stream;
		private uint _streamPos;
		private uint _windowSize;

		public uint TrainSize;

		public void Create(uint windowSize)
		{
			if (_windowSize != windowSize)
				// System.GC.Collect();
				_buffer = new byte[windowSize];
			_windowSize = windowSize;
			_pos = 0;
			_streamPos = 0;
		}

		public void Init(Stream stream, bool solid)
		{
			ReleaseStream();
			_stream = stream;
			if (!solid)
			{
				_streamPos = 0;
				_pos = 0;
				TrainSize = 0;
			}
		}

		public void ReleaseStream()
		{
			Flush();
			_stream = null;
		}

		public void Flush()
		{
			uint size = _pos - _streamPos;
			if (size == 0)
				return;
			_stream.Write(_buffer, (int) _streamPos, (int) size);
			if (_pos >= _windowSize)
				_pos = 0;
			_streamPos = _pos;
		}

		public void CopyBlock(uint distance, uint len)
		{
			uint pos = _pos - distance - 1;
			if (pos >= _windowSize)
				pos += _windowSize;
			for (; len > 0; len--)
			{
				if (pos >= _windowSize)
					pos = 0;
				_buffer[_pos++] = _buffer[pos++];
				if (_pos >= _windowSize)
					Flush();
			}
		}

		public void PutByte(byte b)
		{
			_buffer[_pos++] = b;
			if (_pos >= _windowSize)
				Flush();
		}

		public byte GetByte(uint distance)
		{
			uint pos = _pos - distance - 1;
			if (pos >= _windowSize)
				pos += _windowSize;
			return _buffer[pos];
		}
	}
}
namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.RangeCoder
{
	using System.IO;

	internal class Decoder
	{
		public const uint kTopValue = 1 << 24;
		public uint Code;

		public uint Range;

		public Stream Stream;

		public void Init(Stream stream)
		{
			Stream = stream;

			Code = 0;
			Range = 0xFFFFFFFF;
			for (int i = 0; i < 5; i++)
				Code = (Code << 8) | (byte) Stream.ReadByte();
		}

		public void ReleaseStream()
		{
			Stream = null;
		}

		public void CloseStream()
		{
			Stream.Dispose();
		}

		public uint DecodeDirectBits(int numTotalBits)
		{
			uint range = Range;
			uint code = Code;
			uint result = 0;
			for (int i = numTotalBits; i > 0; i--)
			{
				range >>= 1;
				uint t = (code - range) >> 31;
				code -= range & (t - 1);
				result = (result << 1) | (1 - t);

				if (range < kTopValue)
				{
					code = (code << 8) | (byte) Stream.ReadByte();
					range <<= 8;
				}
			}

			Range = range;
			Code = code;
			return result;
		}

	}
}
namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.RangeCoder
{

	internal struct BitDecoder
	{
		public const int kNumBitModelTotalBits = 11;
		public const uint kBitModelTotal = 1 << kNumBitModelTotalBits;
		private const int kNumMoveBits = 5;

		private uint Prob;

		public void Init()
		{
			Prob = kBitModelTotal >> 1;
		}

		public uint Decode(Decoder rangeDecoder)
		{
			uint newBound = (rangeDecoder.Range >> kNumBitModelTotalBits) * Prob;
			if (rangeDecoder.Code < newBound)
			{
				rangeDecoder.Range = newBound;
				Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
				if (rangeDecoder.Range < Decoder.kTopValue)
				{
					rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte) rangeDecoder.Stream.ReadByte();
					rangeDecoder.Range <<= 8;
				}

				return 0;
			}

			rangeDecoder.Range -= newBound;
			rangeDecoder.Code -= newBound;
			Prob -= Prob >> kNumMoveBits;
			if (rangeDecoder.Range < Decoder.kTopValue)
			{
				rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte) rangeDecoder.Stream.ReadByte();
				rangeDecoder.Range <<= 8;
			}

			return 1;
		}
	}
}
namespace Universe.LzmaDecompressionImplementation.SevenZip.Compression.RangeCoder
{
	internal struct BitTreeDecoder
	{
		private readonly BitDecoder[] Models;
		private readonly int NumBitLevels;

		public BitTreeDecoder(int numBitLevels)
		{
			NumBitLevels = numBitLevels;
			Models = new BitDecoder[1 << numBitLevels];
		}

		public void Init()
		{
			for (uint i = 1; i < 1 << NumBitLevels; i++)
				Models[i].Init();
		}

		public uint Decode(Decoder rangeDecoder)
		{
			uint m = 1;
			for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
				m = (m << 1) + Models[m].Decode(rangeDecoder);
			return m - ((uint) 1 << NumBitLevels);
		}

		public uint ReverseDecode(Decoder rangeDecoder)
		{
			uint m = 1;
			uint symbol = 0;
			for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
			{
				uint bit = Models[m].Decode(rangeDecoder);
				m <<= 1;
				m += bit;
				symbol |= bit << bitIndex;
			}

			return symbol;
		}

		public static uint ReverseDecode(BitDecoder[] Models, uint startIndex,
			Decoder rangeDecoder, int NumBitLevels)
		{
			uint m = 1;
			uint symbol = 0;
			for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
			{
				uint bit = Models[startIndex + m].Decode(rangeDecoder);
				m <<= 1;
				m += bit;
				symbol |= bit << bitIndex;
			}

			return symbol;
		}
	}
}
