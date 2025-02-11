using System.IO;

namespace System.Windows.Markup;

internal class BamlPropertyTypeReferenceRecord : BamlPropertyComplexStartRecord
{
	private short _typeId;

	internal override BamlRecordType RecordType => BamlRecordType.PropertyTypeReference;

	internal short TypeId
	{
		get
		{
			return _typeId;
		}
		set
		{
			_typeId = value;
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
		TypeId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(base.AttributeId);
		bamlBinaryWriter.Write(TypeId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlPropertyTypeReferenceRecord)record)._typeId = _typeId;
	}
}
