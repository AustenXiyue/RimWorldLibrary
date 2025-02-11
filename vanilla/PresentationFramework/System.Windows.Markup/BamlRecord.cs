using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal abstract class BamlRecord
{
	internal BitVector32 _flags;

	private static BitVector32.Section _pinnedFlagSection = BitVector32.CreateSection(3);

	private BamlRecord _nextRecord;

	internal const int RecordTypeFieldLength = 1;

	internal virtual int RecordSize
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	internal virtual BamlRecordType RecordType => BamlRecordType.Unknown;

	internal BamlRecord Next
	{
		get
		{
			return _nextRecord;
		}
		set
		{
			_nextRecord = value;
		}
	}

	internal bool IsPinned => PinnedCount > 0;

	internal int PinnedCount
	{
		get
		{
			return _flags[_pinnedFlagSection];
		}
		set
		{
			_flags[_pinnedFlagSection] = value;
		}
	}

	internal static BitVector32.Section LastFlagsSection => _pinnedFlagSection;

	internal virtual bool LoadRecordSize(BinaryReader bamlBinaryReader, long bytesAvailable)
	{
		return true;
	}

	internal virtual void LoadRecordData(BinaryReader bamlBinaryReader)
	{
	}

	internal virtual void Write(BinaryWriter bamlBinaryWriter)
	{
		if (bamlBinaryWriter != null)
		{
			bamlBinaryWriter.Write((byte)RecordType);
			WriteRecordData(bamlBinaryWriter);
		}
	}

	internal virtual void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
	}

	internal void Pin()
	{
		if (PinnedCount < 3)
		{
			int pinnedCount = PinnedCount + 1;
			PinnedCount = pinnedCount;
		}
	}

	internal void Unpin()
	{
		if (PinnedCount < 3)
		{
			int pinnedCount = PinnedCount - 1;
			PinnedCount = pinnedCount;
		}
	}

	internal virtual void Copy(BamlRecord record)
	{
		record._flags = _flags;
		record._nextRecord = _nextRecord;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}", RecordType);
	}

	protected static string GetTypeName(int typeId)
	{
		string result = typeId.ToString(CultureInfo.InvariantCulture);
		if (typeId < 0)
		{
			result = ((KnownElements)(-typeId)/*cast due to .constrained prefix*/).ToString();
		}
		return result;
	}

	internal static bool IsContentRecord(BamlRecordType bamlRecordType)
	{
		if (bamlRecordType != BamlRecordType.PropertyComplexStart && bamlRecordType != BamlRecordType.PropertyArrayStart && bamlRecordType != BamlRecordType.PropertyIListStart && bamlRecordType != BamlRecordType.PropertyIDictionaryStart)
		{
			return bamlRecordType == BamlRecordType.Text;
		}
		return true;
	}
}
