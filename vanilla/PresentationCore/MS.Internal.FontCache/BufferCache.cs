using System;
using System.Threading;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

internal static class BufferCache
{
	private const int MaxBufferLength = 1024;

	private const int GlyphMetricsIndex = 0;

	private const int UIntsIndex = 1;

	private const int UShortsIndex = 2;

	private const int BuffersLength = 3;

	private static long _mutex;

	private static Array[] _buffers;

	internal static void Reset()
	{
		if (Interlocked.Increment(ref _mutex) == 1)
		{
			_buffers = null;
		}
		Interlocked.Decrement(ref _mutex);
	}

	internal static GlyphMetrics[] GetGlyphMetrics(int length)
	{
		GlyphMetrics[] array = (GlyphMetrics[])GetBuffer(length, 0);
		if (array == null)
		{
			array = new GlyphMetrics[length];
		}
		return array;
	}

	internal static void ReleaseGlyphMetrics(GlyphMetrics[] glyphMetrics)
	{
		ReleaseBuffer(glyphMetrics, 0);
	}

	internal static ushort[] GetUShorts(int length)
	{
		ushort[] array = (ushort[])GetBuffer(length, 2);
		if (array == null)
		{
			array = new ushort[length];
		}
		return array;
	}

	internal static void ReleaseUShorts(ushort[] ushorts)
	{
		ReleaseBuffer(ushorts, 2);
	}

	internal static uint[] GetUInts(int length)
	{
		uint[] array = (uint[])GetBuffer(length, 1);
		if (array == null)
		{
			array = new uint[length];
		}
		return array;
	}

	internal static void ReleaseUInts(uint[] uints)
	{
		ReleaseBuffer(uints, 1);
	}

	private static Array GetBuffer(int length, int index)
	{
		Array result = null;
		if (Interlocked.Increment(ref _mutex) == 1 && _buffers != null && _buffers[index] != null && length <= _buffers[index].Length)
		{
			result = _buffers[index];
			_buffers[index] = null;
		}
		Interlocked.Decrement(ref _mutex);
		return result;
	}

	private static void ReleaseBuffer(Array buffer, int index)
	{
		if (buffer == null)
		{
			return;
		}
		if (Interlocked.Increment(ref _mutex) == 1)
		{
			if (_buffers == null)
			{
				_buffers = new Array[3];
			}
			if (_buffers[index] == null || (_buffers[index].Length < buffer.Length && buffer.Length <= 1024))
			{
				_buffers[index] = buffer;
			}
		}
		Interlocked.Decrement(ref _mutex);
	}
}
