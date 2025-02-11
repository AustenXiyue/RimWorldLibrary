using System.IO;

namespace System.Windows.Markup;

internal class BamlPropertyStringReferenceRecord : BamlPropertyComplexStartRecord
{
	private short _stringId;

	internal override BamlRecordType RecordType => BamlRecordType.PropertyStringReference;

	internal short StringId
	{
		get
		{
			return _stringId;
		}
		set
		{
			_stringId = value;
		}
	}

	internal override int RecordSize
	{
		get
		{
			return 4;
		}
		set
		{
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.AttributeId = bamlBinaryReader.ReadInt16();
		StringId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(base.AttributeId);
		bamlBinaryWriter.Write(StringId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlPropertyStringReferenceRecord)record)._stringId = _stringId;
	}
}
