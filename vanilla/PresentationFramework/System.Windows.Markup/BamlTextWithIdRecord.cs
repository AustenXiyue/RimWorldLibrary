using System.IO;

namespace System.Windows.Markup;

internal class BamlTextWithIdRecord : BamlTextRecord
{
	private short _valueId;

	internal override BamlRecordType RecordType => BamlRecordType.TextWithId;

	internal short ValueId
	{
		get
		{
			return _valueId;
		}
		set
		{
			_valueId = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		ValueId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(ValueId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlTextWithIdRecord)record)._valueId = _valueId;
	}
}
