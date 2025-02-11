using System.IO;

namespace System.Windows.Markup;

internal abstract class BamlStringValueRecord : BamlVariableSizedRecord
{
	private string _value;

	internal string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		Value = bamlBinaryReader.ReadString();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(Value);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlStringValueRecord)record)._value = _value;
	}
}
