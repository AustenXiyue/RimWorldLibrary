using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlPresentationOptionsAttributeRecord : BamlStringValueRecord
{
	private string _name;

	private short _nameId;

	internal override BamlRecordType RecordType => BamlRecordType.PresentationOptionsAttribute;

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
		BamlPresentationOptionsAttributeRecord obj = (BamlPresentationOptionsAttributeRecord)record;
		obj._name = _name;
		obj._nameId = _nameId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} nameId({1}) is '{2}' ", RecordType, NameId, Name);
	}
}
