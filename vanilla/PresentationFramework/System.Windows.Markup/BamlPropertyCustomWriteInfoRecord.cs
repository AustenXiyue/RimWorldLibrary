using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace System.Windows.Markup;

internal class BamlPropertyCustomWriteInfoRecord : BamlPropertyCustomRecord
{
	private short _valueId;

	private Type _valueType;

	private string _value;

	private string _valueMemberName;

	private Type _serializerType;

	private ITypeDescriptorContext _typeContext;

	internal short ValueId
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

	internal string ValueMemberName
	{
		get
		{
			return _valueMemberName;
		}
		set
		{
			_valueMemberName = value;
		}
	}

	internal Type ValueType
	{
		get
		{
			return _valueType;
		}
		set
		{
			_valueType = value;
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

	internal Type SerializerType
	{
		get
		{
			return _serializerType;
		}
		set
		{
			_serializerType = value;
		}
	}

	internal ITypeDescriptorContext TypeContext
	{
		get
		{
			return _typeContext;
		}
		set
		{
			_typeContext = value;
		}
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		short serializerTypeId = base.SerializerTypeId;
		bamlBinaryWriter.Write(base.AttributeId);
		if (serializerTypeId == 137)
		{
			if (ValueMemberName != null)
			{
				bamlBinaryWriter.Write((short)(serializerTypeId | BamlPropertyCustomRecord.TypeIdValueMask));
			}
			else
			{
				bamlBinaryWriter.Write(serializerTypeId);
			}
			bamlBinaryWriter.Write(ValueId);
			if (ValueMemberName != null)
			{
				bamlBinaryWriter.Write(ValueMemberName);
			}
			return;
		}
		bamlBinaryWriter.Write(serializerTypeId);
		bool flag = false;
		if (ValueType != null && ValueType.IsEnum)
		{
			uint num = 0u;
			string[] array = Value.Split(',');
			foreach (string text in array)
			{
				FieldInfo field = ValueType.GetField(text.Trim(), BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					object rawConstantValue = field.GetRawConstantValue();
					num += (uint)Convert.ChangeType(rawConstantValue, typeof(uint), TypeConverterHelper.InvariantEnglishUS);
					flag = true;
					continue;
				}
				flag = false;
				break;
			}
			if (flag)
			{
				bamlBinaryWriter.Write(num);
			}
		}
		else if (ValueType == typeof(bool))
		{
			object value = TypeDescriptor.GetConverter(typeof(bool)).ConvertFromString(TypeContext, TypeConverterHelper.InvariantEnglishUS, Value);
			bamlBinaryWriter.Write((byte)Convert.ChangeType(value, typeof(byte), TypeConverterHelper.InvariantEnglishUS));
			flag = true;
		}
		else if (SerializerType == typeof(XamlBrushSerializer))
		{
			flag = new XamlBrushSerializer().ConvertStringToCustomBinary(bamlBinaryWriter, Value);
		}
		else if (SerializerType == typeof(XamlPoint3DCollectionSerializer))
		{
			flag = new XamlPoint3DCollectionSerializer().ConvertStringToCustomBinary(bamlBinaryWriter, Value);
		}
		else if (SerializerType == typeof(XamlVector3DCollectionSerializer))
		{
			flag = new XamlVector3DCollectionSerializer().ConvertStringToCustomBinary(bamlBinaryWriter, Value);
		}
		else if (SerializerType == typeof(XamlPointCollectionSerializer))
		{
			flag = new XamlPointCollectionSerializer().ConvertStringToCustomBinary(bamlBinaryWriter, Value);
		}
		else if (SerializerType == typeof(XamlInt32CollectionSerializer))
		{
			flag = new XamlInt32CollectionSerializer().ConvertStringToCustomBinary(bamlBinaryWriter, Value);
		}
		else if (SerializerType == typeof(XamlPathDataSerializer))
		{
			flag = new XamlPathDataSerializer().ConvertStringToCustomBinary(bamlBinaryWriter, Value);
		}
		if (flag)
		{
			return;
		}
		throw new XamlParseException(SR.Format(SR.ParserBadString, Value, ValueType.Name));
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlPropertyCustomWriteInfoRecord obj = (BamlPropertyCustomWriteInfoRecord)record;
		obj._valueId = _valueId;
		obj._valueType = _valueType;
		obj._value = _value;
		obj._valueMemberName = _valueMemberName;
		obj._serializerType = _serializerType;
		obj._typeContext = _typeContext;
	}
}
