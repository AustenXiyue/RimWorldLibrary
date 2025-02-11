using System;
using System.Collections.Generic;
using System.IO;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class GuidList
{
	private readonly List<Guid> _customGuids = new List<Guid>();

	public bool Add(Guid guid)
	{
		if (FindTag(guid, bFindInKnownListFirst: true) == KnownTagCache.KnownTagIndex.Unknown)
		{
			_customGuids.Add(guid);
			return true;
		}
		return false;
	}

	public static KnownTagCache.KnownTagIndex FindKnownTag(Guid guid)
	{
		for (byte b = 0; b < KnownIdCache.OriginalISFIdTable.Length; b++)
		{
			if (guid == KnownIdCache.OriginalISFIdTable[b])
			{
				return KnownIdCache.KnownGuidBaseIndex + b;
			}
		}
		return KnownTagCache.KnownTagIndex.Unknown;
	}

	private KnownTagCache.KnownTagIndex FindCustomTag(Guid guid)
	{
		for (int i = 0; i < _customGuids.Count; i++)
		{
			if (guid.Equals(_customGuids[i]))
			{
				return (KnownTagCache.KnownTagIndex)(KnownIdCache.CustomGuidBaseIndex + i);
			}
		}
		return KnownTagCache.KnownTagIndex.Unknown;
	}

	public KnownTagCache.KnownTagIndex FindTag(Guid guid, bool bFindInKnownListFirst)
	{
		KnownTagCache.KnownTagIndex knownTagIndex = KnownTagCache.KnownTagIndex.Unknown;
		if (bFindInKnownListFirst)
		{
			knownTagIndex = FindKnownTag(guid);
			if (knownTagIndex == KnownTagCache.KnownTagIndex.Unknown)
			{
				knownTagIndex = FindCustomTag(guid);
			}
		}
		else
		{
			knownTagIndex = FindCustomTag(guid);
			if (knownTagIndex == KnownTagCache.KnownTagIndex.Unknown)
			{
				knownTagIndex = FindKnownTag(guid);
			}
		}
		return knownTagIndex;
	}

	private static Guid FindKnownGuid(KnownTagCache.KnownTagIndex tag)
	{
		if (tag < KnownIdCache.KnownGuidBaseIndex)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Tag is outside of the known guid tag range"));
		}
		uint num = tag - KnownIdCache.KnownGuidBaseIndex;
		if (KnownIdCache.OriginalISFIdTable.Length <= num)
		{
			return Guid.Empty;
		}
		return KnownIdCache.OriginalISFIdTable[num];
	}

	private Guid FindCustomGuid(KnownTagCache.KnownTagIndex tag)
	{
		if ((int)tag < (int)KnownIdCache.CustomGuidBaseIndex)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Tag is outside of the known guid tag range"));
		}
		int num = (int)(tag - KnownIdCache.CustomGuidBaseIndex);
		if (0 > num || _customGuids.Count <= num)
		{
			return Guid.Empty;
		}
		return _customGuids[num];
	}

	public Guid FindGuid(KnownTagCache.KnownTagIndex tag)
	{
		if ((uint)tag < KnownIdCache.CustomGuidBaseIndex)
		{
			Guid guid = FindKnownGuid(tag);
			if (Guid.Empty != guid)
			{
				return guid;
			}
			return FindCustomGuid(tag);
		}
		Guid guid2 = FindCustomGuid(tag);
		if (Guid.Empty != guid2)
		{
			return guid2;
		}
		return FindKnownGuid(tag);
	}

	public static uint GetDataSizeIfKnownGuid(Guid guid)
	{
		for (uint num = 0u; num < KnownIdCache.OriginalISFIdTable.Length; num++)
		{
			if (guid == KnownIdCache.OriginalISFIdTable[num])
			{
				return KnownIdCache.OriginalISFIdPersistenceSize[num];
			}
		}
		return 0u;
	}

	public uint Save(Stream stream)
	{
		uint num = (uint)(_customGuids.Count * Native.SizeOfGuid);
		if (num == 0)
		{
			return 0u;
		}
		if (stream == null)
		{
			return num + SerializationHelper.VarSize(num) + SerializationHelper.VarSize(1u);
		}
		uint num2 = SerializationHelper.Encode(stream, 1u);
		num2 += SerializationHelper.Encode(stream, num);
		for (int i = 0; i < _customGuids.Count; i++)
		{
			stream.Write(_customGuids[i].ToByteArray(), 0, (int)Native.SizeOfGuid);
		}
		return num2 + num;
	}

	public uint Load(Stream strm, uint size)
	{
		uint num = 0u;
		_customGuids.Clear();
		uint num2 = size / Native.SizeOfGuid;
		byte[] array = new byte[Native.SizeOfGuid];
		for (uint num3 = 0u; num3 < num2; num3++)
		{
			uint num4 = StrokeCollectionSerializer.ReliableRead(strm, array, Native.SizeOfGuid);
			num += num4;
			if (num4 != Native.SizeOfGuid)
			{
				break;
			}
			_customGuids.Add(new Guid(array));
		}
		return num;
	}
}
