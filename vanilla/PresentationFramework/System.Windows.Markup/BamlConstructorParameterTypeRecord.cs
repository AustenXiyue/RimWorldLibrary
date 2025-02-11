using System.IO;

namespace System.Windows.Markup;

internal class BamlConstructorParameterTypeRecord : BamlRecord
{
	private short _typeId;

	internal override BamlRecordType RecordType => BamlRecordType.ConstructorParameterType;

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
			return 2;
		}
		set
		{
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		TypeId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(TypeId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlConstructorParameterTypeRecord)record)._typeId = _typeId;
	}
}
