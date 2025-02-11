using System.Collections.Specialized;
using System.IO;

namespace System.Windows.Markup;

internal abstract class BamlVariableSizedRecord : BamlRecord
{
	internal const int MaxRecordSizeFieldLength = 4;

	private int _recordSize = -1;

	internal override int RecordSize
	{
		get
		{
			return _recordSize;
		}
		set
		{
			_recordSize = value;
		}
	}

	internal new static BitVector32.Section LastFlagsSection => BamlRecord.LastFlagsSection;

	internal override bool LoadRecordSize(BinaryReader bamlBinaryReader, long bytesAvailable)
	{
		int recordSize;
		bool num = LoadVariableRecordSize(bamlBinaryReader, bytesAvailable, out recordSize);
		if (num)
		{
			RecordSize = recordSize;
		}
		return num;
	}

	internal static bool LoadVariableRecordSize(BinaryReader bamlBinaryReader, long bytesAvailable, out int recordSize)
	{
		if (bytesAvailable >= 4)
		{
			recordSize = ((BamlBinaryReader)bamlBinaryReader).Read7BitEncodedInt();
			return true;
		}
		recordSize = -1;
		return false;
	}

	protected int ComputeSizeOfVariableLengthRecord(long start, long end)
	{
		int num = (int)(end - start);
		return BamlBinaryWriter.SizeOf7bitEncodedSize(BamlBinaryWriter.SizeOf7bitEncodedSize(num) + num) + num;
	}

	internal override void Write(BinaryWriter bamlBinaryWriter)
	{
		if (bamlBinaryWriter != null)
		{
			bamlBinaryWriter.Write((byte)RecordType);
			long num = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
			WriteRecordData(bamlBinaryWriter);
			long end = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
			RecordSize = ComputeSizeOfVariableLengthRecord(num, end);
			bamlBinaryWriter.Seek((int)num, SeekOrigin.Begin);
			WriteRecordSize(bamlBinaryWriter);
			WriteRecordData(bamlBinaryWriter);
		}
	}

	internal void WriteRecordSize(BinaryWriter bamlBinaryWriter)
	{
		((BamlBinaryWriter)bamlBinaryWriter).Write7BitEncodedInt(RecordSize);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlVariableSizedRecord)record)._recordSize = _recordSize;
	}
}
