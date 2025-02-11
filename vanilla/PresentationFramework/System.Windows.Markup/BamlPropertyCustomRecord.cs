using System.Collections.Specialized;
using System.IO;
using System.Windows.Media;

namespace System.Windows.Markup;

internal class BamlPropertyCustomRecord : BamlVariableSizedRecord
{
	private object _valueObject;

	private static BitVector32.Section _isValueSetSection = BitVector32.CreateSection(1, BamlVariableSizedRecord.LastFlagsSection);

	private static BitVector32.Section _isValueTypeIdSection = BitVector32.CreateSection(1, _isValueSetSection);

	private static BitVector32.Section _isRawEnumValueSetSection = BitVector32.CreateSection(1, _isValueTypeIdSection);

	internal static readonly short TypeIdValueMask = 16384;

	private short _attributeId;

	private short _serializerTypeId;

	internal override BamlRecordType RecordType => BamlRecordType.PropertyCustom;

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

	internal short SerializerTypeId
	{
		get
		{
			return _serializerTypeId;
		}
		set
		{
			_serializerTypeId = value;
		}
	}

	internal object ValueObject
	{
		get
		{
			return _valueObject;
		}
		set
		{
			_valueObject = value;
		}
	}

	internal bool ValueObjectSet
	{
		get
		{
			return _flags[_isValueSetSection] == 1;
		}
		set
		{
			_flags[_isValueSetSection] = (value ? 1 : 0);
		}
	}

	internal bool IsValueTypeId
	{
		get
		{
			return _flags[_isValueTypeIdSection] == 1;
		}
		set
		{
			_flags[_isValueTypeIdSection] = (value ? 1 : 0);
		}
	}

	internal bool IsRawEnumValueSet
	{
		get
		{
			return _flags[_isRawEnumValueSetSection] == 1;
		}
		set
		{
			_flags[_isRawEnumValueSetSection] = (value ? 1 : 0);
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _isRawEnumValueSetSection;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		AttributeId = bamlBinaryReader.ReadInt16();
		short num = bamlBinaryReader.ReadInt16();
		IsValueTypeId = (num & TypeIdValueMask) == TypeIdValueMask;
		if (IsValueTypeId)
		{
			num &= (short)(~TypeIdValueMask);
		}
		SerializerTypeId = num;
		ValueObjectSet = false;
		IsRawEnumValueSet = false;
		_valueObject = null;
	}

	internal object GetCustomValue(BinaryReader reader, Type propertyType, short serializerId, BamlRecordReader bamlRecordReader)
	{
		switch (serializerId)
		{
		case 195:
		{
			uint num = ((_valueObject != null) ? ((uint)_valueObject) : reader.ReadUInt32());
			if (propertyType.IsEnum)
			{
				_valueObject = Enum.ToObject(propertyType, num);
				ValueObjectSet = true;
				IsRawEnumValueSet = false;
			}
			else
			{
				_valueObject = num;
				ValueObjectSet = false;
				IsRawEnumValueSet = true;
			}
			return _valueObject;
		}
		case 46:
		{
			byte b = reader.ReadByte();
			_valueObject = b == 1;
			break;
		}
		case 744:
			_valueObject = SolidColorBrush.DeserializeFrom(reader, bamlRecordReader.TypeConvertContext);
			break;
		case 746:
			_valueObject = XamlPathDataSerializer.StaticConvertCustomBinaryToObject(reader);
			break;
		case 747:
			_valueObject = XamlPoint3DCollectionSerializer.StaticConvertCustomBinaryToObject(reader);
			break;
		case 752:
			_valueObject = XamlVector3DCollectionSerializer.StaticConvertCustomBinaryToObject(reader);
			break;
		case 748:
			_valueObject = XamlPointCollectionSerializer.StaticConvertCustomBinaryToObject(reader);
			break;
		case 745:
			_valueObject = XamlInt32CollectionSerializer.StaticConvertCustomBinaryToObject(reader);
			break;
		default:
			return null;
		}
		ValueObjectSet = true;
		return _valueObject;
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlPropertyCustomRecord obj = (BamlPropertyCustomRecord)record;
		obj._valueObject = _valueObject;
		obj._attributeId = _attributeId;
		obj._serializerTypeId = _serializerTypeId;
	}
}
