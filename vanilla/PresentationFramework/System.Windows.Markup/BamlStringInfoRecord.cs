using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlStringInfoRecord : BamlVariableSizedRecord
{
	private static BitVector32.Section _stringIdLowSection = BitVector32.CreateSection(255, BamlVariableSizedRecord.LastFlagsSection);

	private static BitVector32.Section _stringIdHighSection = BitVector32.CreateSection(255, _stringIdLowSection);

	private string _value;

	internal short StringId
	{
		get
		{
			return (short)((short)_flags[_stringIdLowSection] | (short)(_flags[_stringIdHighSection] << 8));
		}
		set
		{
			_flags[_stringIdLowSection] = (short)(value & 0xFF);
			_flags[_stringIdHighSection] = (short)((value & 0xFF00) >> 8);
		}
	}

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

	internal override BamlRecordType RecordType => BamlRecordType.StringInfo;

	internal virtual bool HasSerializer => false;

	internal new static BitVector32.Section LastFlagsSection => _stringIdHighSection;

	internal BamlStringInfoRecord()
	{
		Pin();
		StringId = -1;
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		StringId = bamlBinaryReader.ReadInt16();
		Value = bamlBinaryReader.ReadString();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(StringId);
		bamlBinaryWriter.Write(Value);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlStringInfoRecord)record)._value = _value;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} stringId({1}='{2}'", RecordType, StringId, _value);
	}
}
