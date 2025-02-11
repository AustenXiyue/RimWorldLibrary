using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlElementStartRecord : BamlRecord
{
	private static BitVector32.Section _typeIdLowSection = BitVector32.CreateSection(255, BamlRecord.LastFlagsSection);

	private static BitVector32.Section _typeIdHighSection = BitVector32.CreateSection(255, _typeIdLowSection);

	private static BitVector32.Section _useTypeConverter = BitVector32.CreateSection(1, _typeIdHighSection);

	private static BitVector32.Section _isInjected = BitVector32.CreateSection(1, _useTypeConverter);

	internal override BamlRecordType RecordType => BamlRecordType.ElementStart;

	internal short TypeId
	{
		get
		{
			return (short)((short)_flags[_typeIdLowSection] | (short)(_flags[_typeIdHighSection] << 8));
		}
		set
		{
			_flags[_typeIdLowSection] = (short)(value & 0xFF);
			_flags[_typeIdHighSection] = (short)((value & 0xFF00) >> 8);
		}
	}

	internal bool CreateUsingTypeConverter
	{
		get
		{
			return _flags[_useTypeConverter] == 1;
		}
		set
		{
			_flags[_useTypeConverter] = (value ? 1 : 0);
		}
	}

	internal bool IsInjected
	{
		get
		{
			return _flags[_isInjected] == 1;
		}
		set
		{
			_flags[_isInjected] = (value ? 1 : 0);
		}
	}

	internal override int RecordSize
	{
		get
		{
			return 3;
		}
		set
		{
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _isInjected;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		TypeId = bamlBinaryReader.ReadInt16();
		byte b = bamlBinaryReader.ReadByte();
		CreateUsingTypeConverter = (b & 1) != 0;
		IsInjected = (b & 2) != 0;
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(TypeId);
		byte value = (byte)((CreateUsingTypeConverter ? 1u : 0u) | (uint)(IsInjected ? 2 : 0));
		bamlBinaryWriter.Write(value);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} typeId={1}", RecordType, BamlRecord.GetTypeName(TypeId));
	}
}
