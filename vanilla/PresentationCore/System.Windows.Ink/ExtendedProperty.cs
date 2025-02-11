using MS.Internal.Ink.InkSerializedFormat;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

internal sealed class ExtendedProperty
{
	private Guid _id;

	private object _value;

	internal Guid Id => _id;

	internal object Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ExtendedPropertySerializer.Validate(_id, value);
			_value = value;
		}
	}

	internal ExtendedProperty(Guid id, object value)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(SR.InvalidGuid);
		}
		_id = id;
		Value = value;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode() ^ Value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != GetType())
		{
			return false;
		}
		ExtendedProperty extendedProperty = (ExtendedProperty)obj;
		if (extendedProperty.Id == Id)
		{
			Type type = Value.GetType();
			Type type2 = extendedProperty.Value.GetType();
			if (!type.IsArray || !type2.IsArray)
			{
				return extendedProperty.Value.Equals(Value);
			}
			Type elementType = type.GetElementType();
			Type elementType2 = type2.GetElementType();
			if (elementType == elementType2 && elementType.IsValueType && type.GetArrayRank() == 1 && elementType2.IsValueType && type2.GetArrayRank() == 1)
			{
				Array array = (Array)Value;
				Array array2 = (Array)extendedProperty.Value;
				if (array.Length == array2.Length)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (!array.GetValue(i).Equals(array2.GetValue(i)))
						{
							return false;
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	public static bool operator ==(ExtendedProperty first, ExtendedProperty second)
	{
		if ((object)first == null && (object)second == null)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	public static bool operator !=(ExtendedProperty first, ExtendedProperty second)
	{
		return !(first == second);
	}

	public override string ToString()
	{
		return string.Concat(str2: (Value == null) ? "<undefined value>" : ((!(Value is string)) ? Value.ToString() : ("\"" + Value.ToString() + "\"")), str0: KnownIds.ConvertToString(Id), str1: ",");
	}

	internal ExtendedProperty Clone()
	{
		Guid id = _id;
		Type type = _value.GetType();
		if (type.IsValueType || type == typeof(string))
		{
			return new ExtendedProperty(id, _value);
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			if (elementType.IsValueType && type.GetArrayRank() == 1)
			{
				Array array = Array.CreateInstance(elementType, ((Array)_value).Length);
				Array.Copy((Array)_value, array, ((Array)_value).Length);
				return new ExtendedProperty(id, array);
			}
		}
		throw new InvalidOperationException(SR.InvalidDataTypeForExtendedProperty);
	}
}
