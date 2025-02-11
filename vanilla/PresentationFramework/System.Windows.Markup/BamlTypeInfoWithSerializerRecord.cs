using System.IO;

namespace System.Windows.Markup;

internal class BamlTypeInfoWithSerializerRecord : BamlTypeInfoRecord
{
	private short _serializerTypeId;

	private Type _serializerType;

	internal short SerializerTypeId
	{
		get
		{
			return _serializerTypeId;
		}
		set
		{
			_serializerTypeId = value;
		}
	}

	internal override BamlRecordType RecordType => BamlRecordType.TypeSerializerInfo;

	internal Type SerializerType
	{
		get
		{
			return _serializerType;
		}
		set
		{
			_serializerType = value;
		}
	}

	internal override bool HasSerializer => true;

	internal BamlTypeInfoWithSerializerRecord()
	{
		Pin();
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.LoadRecordData(bamlBinaryReader);
		SerializerTypeId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		base.WriteRecordData(bamlBinaryWriter);
		bamlBinaryWriter.Write(SerializerTypeId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlTypeInfoWithSerializerRecord obj = (BamlTypeInfoWithSerializerRecord)record;
		obj._serializerTypeId = _serializerTypeId;
		obj._serializerType = _serializerType;
	}
}
