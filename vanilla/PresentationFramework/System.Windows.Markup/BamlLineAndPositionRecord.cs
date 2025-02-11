using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlLineAndPositionRecord : BamlRecord
{
	private uint _lineNumber;

	private uint _linePosition;

	internal override BamlRecordType RecordType => BamlRecordType.LineNumberAndPosition;

	internal uint LineNumber
	{
		get
		{
			return _lineNumber;
		}
		set
		{
			_lineNumber = value;
		}
	}

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

	internal override int RecordSize => 8;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		LineNumber = (uint)bamlBinaryReader.ReadInt32();
		LinePosition = (uint)bamlBinaryReader.ReadInt32();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(LineNumber);
		bamlBinaryWriter.Write(LinePosition);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlLineAndPositionRecord obj = (BamlLineAndPositionRecord)record;
		obj._lineNumber = _lineNumber;
		obj._linePosition = _linePosition;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} LineNum={1} Pos={2}", RecordType, LineNumber, LinePosition);
	}
}
