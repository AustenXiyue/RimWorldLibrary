using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlDefAttributeRecord : BamlStringValueRecord
{
	private string _name;

	private short _nameId;

	private BamlAttributeUsage _attributeUsage;

	internal override BamlRecordType RecordType => BamlRecordType.DefAttribute;

	internal short NameId
	{
		get
		{
			return _nameId;
		}
		set
		{
			_nameId = value;
		}
	}

	internal string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal BamlAttributeUsage AttributeUsage
	{
		get
		{
			return _attributeUsage;
		}
		set
		{
			_attributeUsage = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.Value = bamlBinaryReader.ReadString();
		NameId = bamlBinaryReader.ReadInt16();
		Name = null;
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(base.Value);
		bamlBinaryWriter.Write(NameId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlDefAttributeRecord obj = (BamlDefAttributeRecord)record;
		obj._name = _name;
		obj._nameId = _nameId;
		obj._attributeUsage = _attributeUsage;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} nameId({1}) is '{2}' usage={3}", RecordType, NameId, Name, AttributeUsage);
	}
}
