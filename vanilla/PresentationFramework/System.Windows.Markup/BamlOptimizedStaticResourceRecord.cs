using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlOptimizedStaticResourceRecord : BamlRecord, IOptimizedMarkupExtension
{
	private short _valueId;

	private static readonly byte TypeExtensionValueMask = 1;

	private static readonly byte StaticExtensionValueMask = 2;

	private static BitVector32.Section _isValueTypeExtensionSection = BitVector32.CreateSection(1, BamlRecord.LastFlagsSection);

	private static BitVector32.Section _isValueStaticExtensionSection = BitVector32.CreateSection(1, _isValueTypeExtensionSection);

	internal override BamlRecordType RecordType => BamlRecordType.OptimizedStaticResource;

	public short ExtensionTypeId => 603;

	public short ValueId
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

	public bool IsValueTypeExtension
	{
		get
		{
			return _flags[_isValueTypeExtensionSection] == 1;
		}
		set
		{
			_flags[_isValueTypeExtensionSection] = (value ? 1 : 0);
		}
	}

	public bool IsValueStaticExtension
	{
		get
		{
			return _flags[_isValueStaticExtensionSection] == 1;
		}
		set
		{
			_flags[_isValueStaticExtensionSection] = (value ? 1 : 0);
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _isValueStaticExtensionSection;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		byte b = bamlBinaryReader.ReadByte();
		ValueId = bamlBinaryReader.ReadInt16();
		IsValueTypeExtension = (b & TypeExtensionValueMask) != 0;
		IsValueStaticExtension = (b & StaticExtensionValueMask) != 0;
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		byte b = 0;
		if (IsValueTypeExtension)
		{
			b |= TypeExtensionValueMask;
		}
		else if (IsValueStaticExtension)
		{
			b |= StaticExtensionValueMask;
		}
		bamlBinaryWriter.Write(b);
		bamlBinaryWriter.Write(ValueId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlOptimizedStaticResourceRecord)record)._valueId = _valueId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} extn(StaticResourceExtension) valueId({1})", RecordType, _valueId);
	}
}
