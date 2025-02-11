using System;
using System.IO;

namespace MS.Internal.Shaping;

internal class OpenTypeLayoutWorkspace
{
	private const byte AggregatedFlagMask = 1;

	private const byte RequiredFeatureFlagMask = 2;

	private const int FeatureFlagsStartBit = 2;

	private int _bytesPerLookup;

	private byte[] _lookupUsageFlags;

	private ushort[] _cachePointers;

	private byte[] _tableCache;

	public ushort[] CachePointers => _cachePointers;

	public byte[] TableCacheData
	{
		get
		{
			return _tableCache;
		}
		set
		{
			_tableCache = value;
		}
	}

	internal OpenTypeLayoutWorkspace()
	{
		_bytesPerLookup = 0;
		_lookupUsageFlags = null;
		_cachePointers = null;
	}

	internal OpenTypeLayoutResult Init(IOpenTypeFont font, OpenTypeTags tableTag, uint scriptTag, uint langSysTag)
	{
		return OpenTypeLayoutResult.Success;
	}

	public void InitLookupUsageFlags(int lookupCount, int featureCount)
	{
		_bytesPerLookup = featureCount + 2 + 7 >> 3;
		int num = lookupCount * _bytesPerLookup;
		if (_lookupUsageFlags == null || _lookupUsageFlags.Length < num)
		{
			_lookupUsageFlags = new byte[num];
		}
		Array.Clear(_lookupUsageFlags, 0, num);
	}

	public bool IsAggregatedFlagSet(int lookupIndex)
	{
		return (_lookupUsageFlags[lookupIndex * _bytesPerLookup] & 1) != 0;
	}

	public bool IsFeatureFlagSet(int lookupIndex, int featureIndex)
	{
		int num = featureIndex + 2;
		int num2 = lookupIndex * _bytesPerLookup + (num >> 3);
		byte b = (byte)(1 << num % 8);
		return (_lookupUsageFlags[num2] & b) != 0;
	}

	public bool IsRequiredFeatureFlagSet(int lookupIndex)
	{
		return (_lookupUsageFlags[lookupIndex * _bytesPerLookup] & 2) != 0;
	}

	public void SetFeatureFlag(int lookupIndex, int featureIndex)
	{
		int num = lookupIndex * _bytesPerLookup;
		int num2 = featureIndex + 2;
		int num3 = num + (num2 >> 3);
		byte b = (byte)(1 << num2 % 8);
		if (num3 >= _lookupUsageFlags.Length)
		{
			throw new FileFormatException();
		}
		_lookupUsageFlags[num3] |= b;
		_lookupUsageFlags[num] |= 1;
	}

	public void SetRequiredFeatureFlag(int lookupIndex)
	{
		int num = lookupIndex * _bytesPerLookup;
		if (num >= _lookupUsageFlags.Length)
		{
			throw new FileFormatException();
		}
		_lookupUsageFlags[num] |= 3;
	}

	public void AllocateCachePointers(int glyphRunLength)
	{
		if (_cachePointers == null || _cachePointers.Length < glyphRunLength)
		{
			_cachePointers = new ushort[glyphRunLength];
		}
	}

	public void UpdateCachePointers(int oldLength, int newLength, int firstGlyphChanged, int afterLastGlyphChanged)
	{
		if (oldLength != newLength)
		{
			int num = afterLastGlyphChanged - (newLength - oldLength);
			if (_cachePointers.Length < newLength)
			{
				ushort[] array = new ushort[newLength];
				Array.Copy(_cachePointers, array, firstGlyphChanged);
				Array.Copy(_cachePointers, num, array, afterLastGlyphChanged, oldLength - num);
				_cachePointers = array;
			}
			else
			{
				Array.Copy(_cachePointers, num, _cachePointers, afterLastGlyphChanged, oldLength - num);
			}
		}
	}
}
