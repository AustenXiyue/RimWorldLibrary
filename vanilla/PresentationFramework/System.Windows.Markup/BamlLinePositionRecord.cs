using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlLinePositionRecord : BamlRecord
{
	private uint _linePosition;

	internal override BamlRecordType RecordType => BamlRecordType.LinePosition;

	internal uint LinePosition
	{
		get
		{
			return _linePosition;
		}
		set
		{
			_linePosition = value;
		}
	}

	internal override int RecordSize => 4;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		LinePosition = (uint)bamlBinaryReader.ReadInt32();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(LinePosition);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlLinePositionRecord)record)._linePosition = _linePosition;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} LinePos={1}", RecordType, LinePosition);
	}
}
