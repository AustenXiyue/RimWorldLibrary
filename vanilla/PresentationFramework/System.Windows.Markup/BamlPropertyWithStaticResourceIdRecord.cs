using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlPropertyWithStaticResourceIdRecord : BamlStaticResourceIdRecord
{
	private short _attributeId = -1;

	internal override BamlRecordType RecordType => BamlRecordType.PropertyWithStaticResourceId;

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
		base.StaticResourceId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AttributeId);
		bamlBinaryWriter.Write(base.StaticResourceId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlPropertyWithStaticResourceIdRecord)record)._attributeId = _attributeId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} attr({1}) staticResourceId({2})", RecordType, AttributeId, base.StaticResourceId);
	}
}
