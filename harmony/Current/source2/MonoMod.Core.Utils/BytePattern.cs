using System;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace MonoMod.Core.Utils;

internal sealed class BytePattern
{
	private enum SegmentKind
	{
		Literal,
		MaskedLiteral,
		Any,
		AnyRepeating,
		Address
	}

	private record struct PatternSegment(int Start, int Length, SegmentKind Kind)
	{
		public ReadOnlySpan<T> SliceOf<T>(ReadOnlySpan<T> span)
		{
			return span.Slice(Start, Length);
		}

		public ReadOnlyMemory<T> SliceOf<T>(ReadOnlyMemory<T> mem)
		{
			return mem.Slice(Start, Length);
		}
	}

	private readonly record struct ComputeSegmentsResult(PatternSegment[] Segments, int MinLen, int AddrBytes);

	private const ushort MaskMask = 65280;

	public const byte BAnyValue = 0;

	public const ushort SAnyValue = 65280;

	public const byte BAnyRepeatingValue = 1;

	public const ushort SAnyRepeatingValue = 65281;

	public const byte BAddressValue = 2;

	public const ushort SAddressValue = 65282;

	private readonly ReadOnlyMemory<byte> pattern;

	private readonly ReadOnlyMemory<byte> bitmask;

	private readonly PatternSegment[] segments;

	private (ReadOnlyMemory<byte> Bytes, int Offset)? lazyFirstLiteralSegment;

	public int AddressBytes { get; }

	public int MinLength { get; }

	public AddressMeaning AddressMeaning { get; }

	public bool MustMatchAtStart { get; }

	public (ReadOnlyMemory<byte> Bytes, int Offset) FirstLiteralSegment
	{
		get
		{
			(ReadOnlyMemory<byte>, int) valueOrDefault = lazyFirstLiteralSegment.GetValueOrDefault();
			if (!lazyFirstLiteralSegment.HasValue)
			{
				valueOrDefault = GetFirstLiteralSegment();
				lazyFirstLiteralSegment = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public BytePattern(AddressMeaning meaning, params ushort[] pattern)
		: this(meaning, mustMatchAtStart: false, pattern.AsMemory())
	{
	}

	public BytePattern(AddressMeaning meaning, bool mustMatchAtStart, params ushort[] pattern)
		: this(meaning, mustMatchAtStart, pattern.AsMemory())
	{
	}

	public BytePattern(AddressMeaning meaning, ReadOnlyMemory<ushort> pattern)
		: this(meaning, mustMatchAtStart: false, pattern)
	{
	}

	public BytePattern(AddressMeaning meaning, bool mustMatchAtStart, ReadOnlyMemory<ushort> pattern)
	{
		AddressMeaning = meaning;
		MustMatchAtStart = mustMatchAtStart;
		ComputeSegmentsFromShort(pattern).Deconstruct(out PatternSegment[] Segments, out int MinLen, out int AddrBytes);
		segments = Segments;
		MinLength = MinLen;
		AddressBytes = AddrBytes;
		Memory<byte> memory = new byte[pattern.Length * 2].AsMemory();
		Memory<byte> memory2 = memory.Slice(0, pattern.Length);
		Memory<byte> memory3 = memory.Slice(pattern.Length);
		for (int i = 0; i < pattern.Length; i++)
		{
			ushort num = pattern.Span[i];
			byte b = (byte)((num & 0xFF00) >> 8);
			byte b2 = (byte)(num & -65281);
			if ((b == 0 || b == byte.MaxValue) ? true : false)
			{
				b = (byte)(~b);
			}
			memory2.Span[i] = (byte)(b2 & b);
			memory3.Span[i] = b;
		}
		this.pattern = memory2;
		bitmask = memory3;
	}

	public BytePattern(AddressMeaning meaning, ReadOnlyMemory<byte> mask, ReadOnlyMemory<byte> pattern)
		: this(meaning, mustMatchAtStart: false, mask, pattern)
	{
	}

	public BytePattern(AddressMeaning meaning, bool mustMatchAtStart, ReadOnlyMemory<byte> mask, ReadOnlyMemory<byte> pattern)
	{
		AddressMeaning = meaning;
		MustMatchAtStart = mustMatchAtStart;
		ComputeSegmentsFromMaskPattern(mask, pattern).Deconstruct(out PatternSegment[] Segments, out int MinLen, out int AddrBytes);
		segments = Segments;
		MinLength = MinLen;
		AddressBytes = AddrBytes;
		this.pattern = pattern;
		bitmask = mask;
	}

	private unsafe static ComputeSegmentsResult ComputeSegmentsFromShort(ReadOnlyMemory<ushort> pattern)
	{
		return ComputeSegmentsCore((delegate*<ReadOnlyMemory<ushort>, int, SegmentKind>)(&KindForShort), pattern.Length, pattern);
		static SegmentKind KindForShort(ReadOnlyMemory<ushort> pattern, int idx)
		{
			ushort num = pattern.Span[idx];
			switch (num & 0xFF00)
			{
			case 0:
				return SegmentKind.Literal;
			case 65280:
			{
				int num2 = num & 0xFF;
				return num2 switch
				{
					0 => SegmentKind.Any, 
					1 => SegmentKind.AnyRepeating, 
					2 => SegmentKind.Address, 
					_ => throw new ArgumentException($"Pattern contained unknown special value {num2:x2}", "pattern"), 
				};
			}
			default:
				return SegmentKind.MaskedLiteral;
			}
		}
	}

	private unsafe static ComputeSegmentsResult ComputeSegmentsFromMaskPattern(ReadOnlyMemory<byte> mask, ReadOnlyMemory<byte> pattern)
	{
		if (mask.Length < pattern.Length)
		{
			throw new ArgumentException("Mask buffer shorter than pattern", "mask");
		}
		return ComputeSegmentsCore((delegate*<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>), int, SegmentKind>)(&KindForIdx), pattern.Length, (mask, pattern));
		static SegmentKind KindForIdx((ReadOnlyMemory<byte> mask, ReadOnlyMemory<byte> pattern) t, int idx)
		{
			switch (t.mask.Span[idx])
			{
			case 0:
			{
				byte b = t.pattern.Span[idx];
				return b switch
				{
					0 => SegmentKind.Any, 
					1 => SegmentKind.AnyRepeating, 
					2 => SegmentKind.Address, 
					_ => throw new ArgumentException($"Pattern contained unknown special value {b:x2}", "pattern"), 
				};
			}
			case byte.MaxValue:
				return SegmentKind.Literal;
			default:
				return SegmentKind.MaskedLiteral;
			}
		}
	}

	private unsafe static ComputeSegmentsResult ComputeSegmentsCore<TPattern>(delegate*<TPattern, int, SegmentKind> kindForIdx, int patternLength, TPattern pattern)
	{
		if (patternLength == 0)
		{
			throw new ArgumentException("Pattern cannot be empty", "pattern");
		}
		int num = 0;
		SegmentKind segmentKind = SegmentKind.AnyRepeating;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = -1;
		for (int i = 0; i < patternLength; i++)
		{
			SegmentKind segmentKind2 = kindForIdx(pattern, i);
			int num6 = num4;
			num4 = num6 + segmentKind2 switch
			{
				SegmentKind.Literal => 1, 
				SegmentKind.MaskedLiteral => 1, 
				SegmentKind.Any => 1, 
				SegmentKind.AnyRepeating => 0, 
				SegmentKind.Address => 1, 
				_ => 0, 
			};
			if (segmentKind2 != segmentKind)
			{
				if (num5 < 0)
				{
					num5 = i;
				}
				num++;
				num2 = 1;
			}
			else
			{
				num2++;
			}
			if (segmentKind2 == SegmentKind.Address)
			{
				num3++;
			}
			segmentKind = segmentKind2;
		}
		if (num > 0 && segmentKind == SegmentKind.AnyRepeating)
		{
			num--;
		}
		if (num == 0 || num4 <= 0)
		{
			throw new ArgumentException("Pattern has no meaningful segments", "pattern");
		}
		PatternSegment[] array = new PatternSegment[num];
		num = 0;
		segmentKind = SegmentKind.AnyRepeating;
		num2 = 0;
		for (int j = num5; j < patternLength; j++)
		{
			if (num > array.Length)
			{
				break;
			}
			SegmentKind num7 = kindForIdx(pattern, j);
			if (num7 != segmentKind)
			{
				if (num > 0)
				{
					array[num - 1] = new PatternSegment(j - num2, num2, segmentKind);
					if (num > 1 && segmentKind == SegmentKind.Any && array[num - 2].Kind == SegmentKind.AnyRepeating)
					{
						Helpers.Swap(ref array[num - 2], ref array[num - 1]);
					}
				}
				num++;
				num2 = 1;
			}
			else
			{
				num2++;
			}
			segmentKind = num7;
		}
		if (segmentKind != SegmentKind.AnyRepeating && num > 0)
		{
			array[num - 1] = new PatternSegment(patternLength - num2, num2, segmentKind);
		}
		return new ComputeSegmentsResult(array, num4, num3);
	}

	public bool TryMatchAt(ReadOnlySpan<byte> data, out ulong address, out int length)
	{
		if (data.Length < MinLength)
		{
			length = 0;
			address = 0uL;
			return false;
		}
		ReadOnlySpan<byte> span = pattern.Span;
		Span<byte> addrBuf = stackalloc byte[8];
		bool result = TryMatchAtImpl(span, data, addrBuf, out length, 0);
		address = Unsafe.ReadUnaligned<ulong>(ref addrBuf[0]);
		return result;
	}

	public bool TryMatchAt(ReadOnlySpan<byte> data, Span<byte> addrBuf, out int length)
	{
		if (data.Length < MinLength)
		{
			length = 0;
			return false;
		}
		ReadOnlySpan<byte> span = pattern.Span;
		return TryMatchAtImpl(span, data, addrBuf, out length, 0);
	}

	private bool TryMatchAtImpl(ReadOnlySpan<byte> patternSpan, ReadOnlySpan<byte> data, Span<byte> addrBuf, out int length, int startAtSegment)
	{
		int num = 0;
		int num2 = startAtSegment;
		while (true)
		{
			if (num2 < segments.Length)
			{
				PatternSegment patternSegment = segments[num2];
				switch (patternSegment.Kind)
				{
				case SegmentKind.Literal:
					if (data.Length - num >= patternSegment.Length)
					{
						ReadOnlySpan<byte> span = patternSegment.SliceOf(patternSpan);
						if (span.SequenceEqual(data.Slice(num, span.Length)))
						{
							num += patternSegment.Length;
							goto IL_0192;
						}
					}
					break;
				case SegmentKind.MaskedLiteral:
					if (data.Length - num >= patternSegment.Length)
					{
						ReadOnlySpan<byte> first = patternSegment.SliceOf(patternSpan);
						ReadOnlySpan<byte> mask = patternSegment.SliceOf(bitmask.Span);
						if (Helpers.MaskedSequenceEqual(first, data.Slice(num, first.Length), mask))
						{
							num += patternSegment.Length;
							goto IL_0192;
						}
					}
					break;
				case SegmentKind.Any:
					if (data.Length - num >= patternSegment.Length)
					{
						num += patternSegment.Length;
						goto IL_0192;
					}
					break;
				case SegmentKind.Address:
					if (data.Length - num >= patternSegment.Length)
					{
						ReadOnlySpan<byte> readOnlySpan = data.Slice(num, Math.Min(patternSegment.Length, addrBuf.Length));
						readOnlySpan.CopyTo(addrBuf);
						addrBuf = addrBuf.Slice(Math.Min(addrBuf.Length, readOnlySpan.Length));
						num += patternSegment.Length;
						goto IL_0192;
					}
					break;
				case SegmentKind.AnyRepeating:
				{
					int offset;
					int length2;
					bool result = ScanForNextLiteral(patternSpan, data.Slice(num), addrBuf, out offset, out length2, num2);
					length = num + offset + length2;
					return result;
				}
				default:
					throw new InvalidOperationException();
				}
				break;
			}
			length = num;
			return true;
			IL_0192:
			num2++;
		}
		length = 0;
		return false;
	}

	public bool TryFindMatch(ReadOnlySpan<byte> data, out ulong address, out int offset, out int length)
	{
		if (data.Length < MinLength)
		{
			length = (offset = 0);
			address = 0uL;
			return false;
		}
		ReadOnlySpan<byte> span = pattern.Span;
		Span<byte> addrBuf = stackalloc byte[8];
		bool result;
		if (MustMatchAtStart)
		{
			offset = 0;
			result = TryMatchAtImpl(span, data, addrBuf, out length, 0);
		}
		else
		{
			result = ScanForNextLiteral(span, data, addrBuf, out offset, out length, 0);
		}
		address = Unsafe.ReadUnaligned<ulong>(ref addrBuf[0]);
		return result;
	}

	public bool TryFindMatch(ReadOnlySpan<byte> data, Span<byte> addrBuf, out int offset, out int length)
	{
		if (data.Length < MinLength)
		{
			length = (offset = 0);
			return false;
		}
		ReadOnlySpan<byte> span = pattern.Span;
		if (MustMatchAtStart)
		{
			offset = 0;
			return TryMatchAtImpl(span, data, addrBuf, out length, 0);
		}
		return ScanForNextLiteral(span, data, addrBuf, out offset, out length, 0);
	}

	private bool ScanForNextLiteral(ReadOnlySpan<byte> patternSpan, ReadOnlySpan<byte> data, Span<byte> addrBuf, out int offset, out int length, int segmentIndex)
	{
		var (patternSegment, num) = GetNextLiteralSegment(segmentIndex);
		if (num + patternSegment.Length > data.Length)
		{
			offset = (length = 0);
			return false;
		}
		int num2 = 0;
		while (true)
		{
			int num3 = data.Slice(num + num2).IndexOf(patternSegment.SliceOf(patternSpan));
			if (num3 < 0)
			{
				offset = (length = 0);
				return false;
			}
			if (TryMatchAtImpl(patternSpan, data.Slice(offset = num2 + num3), addrBuf, out length, segmentIndex))
			{
				break;
			}
			num2 += num3 + 1;
		}
		return true;
	}

	private (ReadOnlyMemory<byte> Bytes, int Offset) GetFirstLiteralSegment()
	{
		var (patternSegment, item) = GetNextLiteralSegment(0);
		return (Bytes: patternSegment.SliceOf(pattern), Offset: item);
	}

	private (PatternSegment Segment, int LiteralOffset) GetNextLiteralSegment(int segmentIndexId)
	{
		if (segmentIndexId < 0 || segmentIndexId >= segments.Length)
		{
			throw new ArgumentOutOfRangeException("segmentIndexId");
		}
		int num = 0;
		while (segmentIndexId < segments.Length)
		{
			PatternSegment item = segments[segmentIndexId];
			if (item.Kind == SegmentKind.Literal)
			{
				return (Segment: item, LiteralOffset: num);
			}
			SegmentKind kind = item.Kind;
			if (((uint)(kind - 1) <= 1u || kind == SegmentKind.Address) ? true : false)
			{
				num += item.Length;
			}
			else if (item.Kind != SegmentKind.AnyRepeating)
			{
				throw new InvalidOperationException("Unknown segment kind");
			}
			segmentIndexId++;
		}
		return (Segment: default(PatternSegment), LiteralOffset: num);
	}
}
