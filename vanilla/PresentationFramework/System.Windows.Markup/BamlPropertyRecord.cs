using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlPropertyRecord : BamlStringValueRecord
{
	private short _attributeId = -1;

	internal override BamlRecordType RecordType => BamlRecordType.Property;

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

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		AttributeId = bamlBinaryReader.ReadInt16();
		base.Value = bamlBinaryReader.ReadString();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AttributeId);
		bamlBinaryWriter.Write(base.Value);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlPropertyRecord)record)._attributeId = _attributeId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} attr({1}) <== '{2}'", RecordType, _attributeId, base.Value);
	}
}
