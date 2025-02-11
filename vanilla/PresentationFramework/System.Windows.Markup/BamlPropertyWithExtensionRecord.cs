using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlPropertyWithExtensionRecord : BamlRecord, IOptimizedMarkupExtension
{
	private static BitVector32.Section _isValueTypeExtensionSection = BitVector32.CreateSection(1, BamlRecord.LastFlagsSection);

	private static BitVector32.Section _isValueStaticExtensionSection = BitVector32.CreateSection(1, _isValueTypeExtensionSection);

	private short _attributeId = -1;

	private short _extensionTypeId;

	private short _valueId;

	private static readonly short ExtensionIdMask = 4095;

	private static readonly short TypeExtensionValueMask = 16384;

	private static readonly short StaticExtensionValueMask = 8192;

	internal override BamlRecordType RecordType => BamlRecordType.PropertyWithExtension;

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

	public short ExtensionTypeId
	{
		get
		{
			return _extensionTypeId;
		}
		set
		{
			_extensionTypeId = value;
		}
	}

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
			return 6;
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
		AttributeId = bamlBinaryReader.ReadInt16();
		short num = bamlBinaryReader.ReadInt16();
		ValueId = bamlBinaryReader.ReadInt16();
		_extensionTypeId = (short)(num & ExtensionIdMask);
		IsValueTypeExtension = (num & TypeExtensionValueMask) == TypeExtensionValueMask;
		IsValueStaticExtension = (num & StaticExtensionValueMask) == StaticExtensionValueMask;
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AttributeId);
		short num = ExtensionTypeId;
		if (IsValueTypeExtension)
		{
			num |= TypeExtensionValueMask;
		}
		else if (IsValueStaticExtension)
		{
			num |= StaticExtensionValueMask;
		}
		bamlBinaryWriter.Write(num);
		bamlBinaryWriter.Write(ValueId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlPropertyWithExtensionRecord obj = (BamlPropertyWithExtensionRecord)record;
		obj._attributeId = _attributeId;
		obj._extensionTypeId = _extensionTypeId;
		obj._valueId = _valueId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} attr({1}) extn({2}) valueId({3})", RecordType, _attributeId, _extensionTypeId, _valueId);
	}
}
