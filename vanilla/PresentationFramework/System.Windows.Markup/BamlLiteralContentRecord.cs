using System.IO;

namespace System.Windows.Markup;

internal class BamlLiteralContentRecord : BamlStringValueRecord
{
	internal override BamlRecordType RecordType => BamlRecordType.LiteralContent;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.Value = bamlBinaryReader.ReadString();
		bamlBinaryReader.ReadInt32();
		bamlBinaryReader.ReadInt32();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(base.Value);
		bamlBinaryWriter.Write(0);
		bamlBinaryWriter.Write(0);
	}
}
