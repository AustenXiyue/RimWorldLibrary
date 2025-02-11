using System.IO;

namespace System.Windows.Markup;

internal class BamlTextWithConverterRecord : BamlTextRecord
{
	private short _converterTypeId;

	internal short ConverterTypeId
	{
		get
		{
			return _converterTypeId;
		}
		set
		{
			_converterTypeId = value;
		}
	}

	internal override BamlRecordType RecordType => BamlRecordType.TextWithConverter;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.LoadRecordData(bamlBinaryReader);
		ConverterTypeId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		base.WriteRecordData(bamlBinaryWriter);
		bamlBinaryWriter.Write(ConverterTypeId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlTextWithConverterRecord)record)._converterTypeId = _converterTypeId;
	}
}
