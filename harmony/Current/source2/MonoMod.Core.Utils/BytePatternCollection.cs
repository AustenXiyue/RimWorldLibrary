using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace MonoMod.Core.Utils;

internal sealed class BytePatternCollection : IEnumerable<BytePattern>, IEnumerable
{
	private struct HomogenousPatternCollection
	{
		public BytePattern[]?[] Lut;

		public readonly int Offset;

		public int MinLength;

		public HomogenousPatternCollection(int offs)
		{
			Offset = offs;
			Lut = null;
			MinLength = int.MaxValue;
		}

		public void AddFirstBytes(ref FirstByteCollection bytes)
		{
			for (int i = 0; i < Lut.Length; i++)
			{
				if (Lut[i] != null)
				{
					bytes.Add((byte)i);
				}
			}
		}
	}

	private ref struct FirstByteCollection
	{
		private Span<byte> firstByteStore;

		private Span<byte> byteIndicies;

		private int firstBytesRecorded;

		public const int SingleAllocationSize = 512;

		public ReadOnlySpan<byte> FirstBytes => firstByteStore.Slice(0, firstBytesRecorded);

		public FirstByteCollection(Span<byte> store)
			: this(store.Slice(0, 256), store.Slice(256, 256))
		{
		}

		public FirstByteCollection(Span<byte> store, Span<byte> indicies)
		{
			firstByteStore = store;
			byteIndicies = indicies;
			firstBytesRecorded = 0;
			byteIndicies.Fill(byte.MaxValue);
		}

		public void Add(byte value)
		{
			ref byte reference = ref byteIndicies[value];
			if (reference == byte.MaxValue)
			{
				reference = (byte)firstBytesRecorded;
				firstByteStore[reference] = value;
				firstBytesRecorded = Math.Min(firstBytesRecorded + 1, 256);
			}
		}
	}

	private readonly HomogenousPatternCollection[] patternCollections;

	private readonly BytePattern[]? emptyPatterns;

	private ReadOnlyMemory<byte>? lazyPossibleFirstBytes;

	public int MinLength { get; }

	public int MaxMinLength { get; }

	public int MaxAddressLength { get; }

	private ReadOnlyMemory<byte> PossibleFirstBytes
	{
		get
		{
			ReadOnlyMemory<byte> valueOrDefault = lazyPossibleFirstBytes.GetValueOrDefault();
			if (!lazyPossibleFirstBytes.HasValue)
			{
				valueOrDefault = GetPossibleFirstBytes();
				lazyPossibleFirstBytes = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public BytePatternCollection(ReadOnlyMemory<BytePattern?> patterns)
	{
		int minLength;
		int maxMinLength;
		int maxAddrLength;
		(HomogenousPatternCollection[], BytePattern[]) tuple = ComputeLut(patterns, out minLength, out maxMinLength, out maxAddrLength);
		patternCollections = tuple.Item1;
		emptyPatterns = tuple.Item2;
		MinLength = minLength;
		MaxMinLength = maxMinLength;
		MaxAddressLength = maxAddrLength;
		Helpers.Assert(MinLength > 0, null, "MinLength > 0");
	}

	public BytePatternCollection(params BytePattern?[] patterns)
		: this(patterns.AsMemory())
	{
	}

	public IEnumerator<BytePattern> GetEnumerator()
	{
		for (int i = 0; i < patternCollections.Length; i++)
		{
			BytePattern[]?[] coll = patternCollections[i].Lut;
			for (int j = 0; j < coll.Length; j++)
			{
				if (coll[j] != null)
				{
					BytePattern[] array = coll[j];
					for (int k = 0; k < array.Length; k++)
					{
						yield return array[k];
					}
				}
			}
		}
		if (emptyPatterns != null)
		{
			BytePattern[] array = emptyPatterns;
			for (int i = 0; i < array.Length; i++)
			{
				yield return array[i];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private static (HomogenousPatternCollection[], BytePattern[]?) ComputeLut(ReadOnlyMemory<BytePattern?> patterns, out int minLength, out int maxMinLength, out int maxAddrLength)
	{
		if (patterns.Length == 0)
		{
			minLength = 0;
			maxMinLength = 0;
			maxAddrLength = 0;
			return (ArrayEx.Empty<HomogenousPatternCollection>(), null);
		}
		Span<int> span = stackalloc int[256];
		minLength = int.MaxValue;
		maxMinLength = int.MinValue;
		maxAddrLength = 0;
		int[][] array = null;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < patterns.Length; i++)
		{
			BytePattern bytePattern = patterns.Span[i];
			if (bytePattern == null)
			{
				continue;
			}
			if (bytePattern.MinLength < minLength)
			{
				minLength = bytePattern.MinLength;
			}
			if (bytePattern.MinLength > maxMinLength)
			{
				maxMinLength = bytePattern.MinLength;
			}
			if (bytePattern.AddressBytes > maxAddrLength)
			{
				maxAddrLength = bytePattern.AddressBytes;
			}
			var (readOnlyMemory, num3) = bytePattern.FirstLiteralSegment;
			if (readOnlyMemory.Length == 0)
			{
				num++;
				continue;
			}
			num2 = 1;
			if (num3 == 0)
			{
				span[readOnlyMemory.Span[0]]++;
				continue;
			}
			if (array == null || array.Length < num3)
			{
				Array.Resize(ref array, num3);
			}
			ref int[] reference = ref array[num3 - 1];
			if (reference == null)
			{
				reference = new int[256];
			}
			reference[readOnlyMemory.Span[0]]++;
		}
		if (array != null)
		{
			int[][] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] != null)
				{
					num2++;
				}
			}
		}
		BytePattern[] array3 = ((num > 0) ? new BytePattern[num] : null);
		int num4 = 0;
		HomogenousPatternCollection[] array4 = new HomogenousPatternCollection[num2];
		int num5 = 1;
		array4[0] = new HomogenousPatternCollection(0);
		for (int k = 0; k < patterns.Length; k++)
		{
			BytePattern bytePattern2 = patterns.Span[k];
			if (bytePattern2 == null)
			{
				continue;
			}
			var (readOnlyMemory2, num6) = bytePattern2.FirstLiteralSegment;
			if (readOnlyMemory2.Length == 0)
			{
				array3[num4++] = bytePattern2;
				continue;
			}
			int num7 = -1;
			for (int l = 0; l < array4.Length; l++)
			{
				if (array4[l].Offset == num6)
				{
					num7 = l;
					break;
				}
			}
			if (num7 == -1)
			{
				num7 = num5++;
				array4[num7] = new HomogenousPatternCollection(num6);
			}
			ReadOnlySpan<int> arrayCounts2 = ((num6 == 0) ? span : array[num6 - 1].AsSpan());
			AddToPatternCollection(ref array4[num7], arrayCounts2, bytePattern2);
			if (num7 > 0 && array4[num7 - 1].Offset > array4[num7].Offset)
			{
				Helpers.Swap(ref array4[num7 - 1], ref array4[num7]);
			}
		}
		return (array4, array3);
		static void AddToPatternCollection(ref HomogenousPatternCollection collection, ReadOnlySpan<int> arrayCounts, BytePattern pattern)
		{
			ReadOnlyMemory<byte> item = pattern.FirstLiteralSegment.Bytes;
			if (collection.Lut == null)
			{
				BytePattern[][] array5 = new BytePattern[256][];
				for (int m = 0; m < arrayCounts.Length; m++)
				{
					if (arrayCounts[m] > 0)
					{
						array5[m] = new BytePattern[arrayCounts[m]];
					}
				}
				collection.Lut = array5;
			}
			BytePattern[]? obj = collection.Lut[item.Span[0]];
			int num8 = Array.IndexOf(obj, null);
			obj[num8] = pattern;
			if (pattern.MinLength < collection.MinLength)
			{
				collection.MinLength = pattern.MinLength;
			}
		}
	}

	public bool TryMatchAt(ReadOnlySpan<byte> data, out ulong address, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out BytePattern matchingPattern, out int length)
	{
		if (data.Length < MinLength)
		{
			length = 0;
			address = 0uL;
			matchingPattern = null;
			return false;
		}
		Span<byte> addrBuf = stackalloc byte[8];
		bool result = TryMatchAt(data, addrBuf, out matchingPattern, out length);
		address = Unsafe.ReadUnaligned<ulong>(ref addrBuf[0]);
		return result;
	}

	public bool TryMatchAt(ReadOnlySpan<byte> data, Span<byte> addrBuf, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out BytePattern matchingPattern, out int length)
	{
		if (data.Length < MinLength)
		{
			length = 0;
			matchingPattern = null;
			return false;
		}
		for (int i = 0; i < patternCollections.Length; i++)
		{
			ref HomogenousPatternCollection reference = ref patternCollections[i];
			if (data.Length < reference.Offset + reference.MinLength)
			{
				continue;
			}
			byte b = data[reference.Offset];
			BytePattern[] array = reference.Lut[b];
			if (array == null)
			{
				continue;
			}
			BytePattern[] array2 = array;
			foreach (BytePattern bytePattern in array2)
			{
				if (bytePattern.TryMatchAt(data, addrBuf, out length))
				{
					matchingPattern = bytePattern;
					return true;
				}
			}
		}
		if (emptyPatterns != null)
		{
			BytePattern[] array2 = emptyPatterns;
			foreach (BytePattern bytePattern2 in array2)
			{
				if (bytePattern2.TryMatchAt(data, addrBuf, out length))
				{
					matchingPattern = bytePattern2;
					return true;
				}
			}
		}
		matchingPattern = null;
		length = 0;
		return false;
	}

	public bool TryFindMatch(ReadOnlySpan<byte> data, out ulong address, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out BytePattern matchingPattern, out int offset, out int length)
	{
		if (data.Length < MinLength)
		{
			length = (offset = 0);
			address = 0uL;
			matchingPattern = null;
			return false;
		}
		Span<byte> addrBuf = stackalloc byte[8];
		bool result = TryFindMatch(data, addrBuf, out matchingPattern, out offset, out length);
		address = Unsafe.ReadUnaligned<ulong>(ref addrBuf[0]);
		return result;
	}

	public bool TryFindMatch(ReadOnlySpan<byte> data, Span<byte> addrBuf, [_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMaybeNullWhen(false)] out BytePattern matchingPattern, out int offset, out int length)
	{
		if (data.Length < MinLength)
		{
			length = (offset = 0);
			matchingPattern = null;
			return false;
		}
		ReadOnlySpan<byte> span = PossibleFirstBytes.Span;
		int num = 0;
		while (true)
		{
			int num2 = data.Slice(num).IndexOfAny(span);
			if (num2 < 0)
			{
				break;
			}
			offset = num + num2;
			byte b = data[offset];
			for (int i = 0; i < patternCollections.Length; i++)
			{
				ref HomogenousPatternCollection reference = ref patternCollections[i];
				if (offset < reference.Offset || data.Length < offset + reference.MinLength)
				{
					continue;
				}
				BytePattern[] array = reference.Lut[b];
				if (array == null)
				{
					continue;
				}
				BytePattern[] array2 = array;
				foreach (BytePattern bytePattern in array2)
				{
					if ((offset == 0 || !bytePattern.MustMatchAtStart) && bytePattern.TryMatchAt(data.Slice(offset - reference.Offset), addrBuf, out length))
					{
						offset -= reference.Offset;
						matchingPattern = bytePattern;
						return true;
					}
				}
			}
			num = offset + 1;
		}
		if (emptyPatterns != null)
		{
			BytePattern[] array2 = emptyPatterns;
			foreach (BytePattern bytePattern2 in array2)
			{
				if (bytePattern2.TryFindMatch(data, addrBuf, out offset, out length))
				{
					matchingPattern = bytePattern2;
					return true;
				}
			}
		}
		matchingPattern = null;
		offset = 0;
		length = 0;
		return false;
	}

	private ReadOnlyMemory<byte> GetPossibleFirstBytes()
	{
		Memory<byte> memory = new byte[512].AsMemory();
		FirstByteCollection bytes = new FirstByteCollection(memory.Span);
		for (int i = 0; i < patternCollections.Length; i++)
		{
			patternCollections[i].AddFirstBytes(ref bytes);
		}
		return memory.Slice(0, bytes.FirstBytes.Length);
	}
}
