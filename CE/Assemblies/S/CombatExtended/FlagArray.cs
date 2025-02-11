using System;
using System.Runtime.CompilerServices;

namespace CombatExtended;

public class FlagArray
{
	private int[] map;

	private const int Bit = 1;

	private const int ChunkSize = 32;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return map.Length;
		}
	}

	public bool this[int key]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Get(key);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			Set(key, value);
		}
	}

	public FlagArray(int size)
	{
		map = new int[size / 32 + 32];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Get(int key)
	{
		return (map[key / 32] & GetOp(key)) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FlagArray Set(int key, bool value)
	{
		map[key / 32] = (value ? (map[key / 32] | GetOp(key)) : (map[key / 32] & ~GetOp(key)));
		return this;
	}

	public void Expand(int targetLength)
	{
		targetLength = targetLength / 32 + 32;
		if ((float)targetLength > (float)Length * 0.75f)
		{
			int[] destinationArray = new int[targetLength];
			Array.Copy(map, 0, destinationArray, 0, map.Length);
			map = destinationArray;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetOp(int key)
	{
		return 1 << key % 32;
	}
}
