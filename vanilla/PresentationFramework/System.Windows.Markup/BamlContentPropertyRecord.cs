using System.IO;

namespace System.Windows.Markup;

internal class BamlContentPropertyRecord : BamlRecord
{
	private short _attributeId = -1;

	internal short AttributeId
	{
		get
		{
			return _attributeId;
		}
		set
		{
			_attributeId = value;
		}
	}

	internal override BamlRecordType RecordType => BamlRecordType.ContentProperty;

	internal virtual bool HasSerializer => false;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		AttributeId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AttributeId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlContentPropertyRecord)record)._attributeId = _attributeId;
	}
}
