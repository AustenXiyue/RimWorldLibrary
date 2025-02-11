using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlPropertyComplexStartRecord : BamlRecord
{
	private short _attributeId = -1;

	internal override BamlRecordType RecordType => BamlRecordType.PropertyComplexStart;

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

	internal override int RecordSize
	{
		get
		{
			return 2;
		}
		set
		{
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		AttributeId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AttributeId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlPropertyComplexStartRecord)record)._attributeId = _attributeId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} attr({1})", RecordType, _attributeId);
	}
}
