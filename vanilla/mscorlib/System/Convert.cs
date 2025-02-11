using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System;

/// <summary>Converts a base data type to another base data type.</summary>
/// <filterpriority>1</filterpriority>
public static class Convert
{
	internal static readonly RuntimeType[] ConvertTypes = new RuntimeType[19]
	{
		(RuntimeType)typeof(Empty),
		(RuntimeType)typeof(object),
		(RuntimeType)typeof(DBNull),
		(RuntimeType)typeof(bool),
		(RuntimeType)typeof(char),
		(RuntimeType)typeof(sbyte),
		(RuntimeType)typeof(byte),
		(RuntimeType)typeof(short),
		(RuntimeType)typeof(ushort),
		(RuntimeType)typeof(int),
		(RuntimeType)typeof(uint),
		(RuntimeType)typeof(long),
		(RuntimeType)typeof(ulong),
		(RuntimeType)typeof(float),
		(RuntimeType)typeof(double),
		(RuntimeType)typeof(decimal),
		(RuntimeType)typeof(DateTime),
		(RuntimeType)typeof(object),
		(RuntimeType)typeof(string)
	};

	private static readonly RuntimeType EnumType = (RuntimeType)typeof(Enum);

	internal static readonly char[] base64Table = new char[65]
	{
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
		'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
		'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
		'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
		'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
		'8', '9', '+', '/', '='
	};

	private const int base64LineBreakPosition = 76;

	/// <summary>A constant that represents a database column that is absent of data; that is, database null.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly object DBNull = System.DBNull.Value;

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for the specified object.</summary>
	/// <returns>The <see cref="T:System.TypeCode" /> for <paramref name="value" />, or <see cref="F:System.TypeCode.Empty" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <filterpriority>1</filterpriority>
	public static TypeCode GetTypeCode(object value)
	{
		if (value == null)
		{
			return TypeCode.Empty;
		}
		if (value is IConvertible convertible)
		{
			return convertible.GetTypeCode();
		}
		return TypeCode.Object;
	}

	/// <summary>Returns an indication whether the specified object is of type <see cref="T:System.DBNull" />.</summary>
	/// <returns>true if <paramref name="value" /> is of type <see cref="T:System.DBNull" />; otherwise, false.</returns>
	/// <param name="value">An object. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsDBNull(object value)
	{
		if (value == System.DBNull.Value)
		{
			return true;
		}
		if (!(value is IConvertible convertible))
		{
			return false;
		}
		return convertible.GetTypeCode() == TypeCode.DBNull;
	}

	/// <summary>Returns an object of the specified type whose value is equivalent to the specified object.</summary>
	/// <returns>An object whose underlying type is <paramref name="typeCode" /> and whose value is equivalent to <paramref name="value" />.-or-A null reference (Nothing in Visual Basic), if <paramref name="value" /> is null and <paramref name="typeCode" /> is <see cref="F:System.TypeCode.Empty" />, <see cref="F:System.TypeCode.String" />, or <see cref="F:System.TypeCode.Object" />.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="typeCode">The type of object to return. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.  -or-<paramref name="value" /> is null and <paramref name="typeCode" /> specifies a value type.-or-<paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in a format recognized by the <paramref name="typeCode" /> type.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is out of the range of the <paramref name="typeCode" /> type.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeCode" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static object ChangeType(object value, TypeCode typeCode)
	{
		return ChangeType(value, typeCode, Thread.CurrentThread.CurrentCulture);
	}

	/// <summary>Returns an object of the specified type whose value is equivalent to the specified object. A parameter supplies culture-specific formatting information.</summary>
	/// <returns>An object whose underlying type is <paramref name="typeCode" /> and whose value is equivalent to <paramref name="value" />.-or- A null reference (Nothing in Visual Basic), if <paramref name="value" /> is null and <paramref name="typeCode" /> is <see cref="F:System.TypeCode.Empty" />, <see cref="F:System.TypeCode.String" />, or <see cref="F:System.TypeCode.Object" />.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="typeCode">The type of object to return. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.  -or-<paramref name="value" /> is null and <paramref name="typeCode" /> specifies a value type.-or-<paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in a format for the <paramref name="typeCode" /> type recognized by <paramref name="provider" />.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is out of the range of the <paramref name="typeCode" /> type.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeCode" /> is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public static object ChangeType(object value, TypeCode typeCode, IFormatProvider provider)
	{
		if (value == null && (typeCode == TypeCode.Empty || typeCode == TypeCode.String || typeCode == TypeCode.Object))
		{
			return null;
		}
		if (!(value is IConvertible convertible))
		{
			throw new InvalidCastException(Environment.GetResourceString("Object must implement IConvertible."));
		}
		return typeCode switch
		{
			TypeCode.Boolean => convertible.ToBoolean(provider), 
			TypeCode.Char => convertible.ToChar(provider), 
			TypeCode.SByte => convertible.ToSByte(provider), 
			TypeCode.Byte => convertible.ToByte(provider), 
			TypeCode.Int16 => convertible.ToInt16(provider), 
			TypeCode.UInt16 => convertible.ToUInt16(provider), 
			TypeCode.Int32 => convertible.ToInt32(provider), 
			TypeCode.UInt32 => convertible.ToUInt32(provider), 
			TypeCode.Int64 => convertible.ToInt64(provider), 
			TypeCode.UInt64 => convertible.ToUInt64(provider), 
			TypeCode.Single => convertible.ToSingle(provider), 
			TypeCode.Double => convertible.ToDouble(provider), 
			TypeCode.Decimal => convertible.ToDecimal(provider), 
			TypeCode.DateTime => convertible.ToDateTime(provider), 
			TypeCode.String => convertible.ToString(provider), 
			TypeCode.Object => value, 
			TypeCode.DBNull => throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast to DBNull.")), 
			TypeCode.Empty => throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast to Empty.")), 
			_ => throw new ArgumentException(Environment.GetResourceString("Unknown TypeCode value.")), 
		};
	}

	internal static object DefaultToType(IConvertible value, Type targetType, IFormatProvider provider)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		RuntimeType runtimeType = targetType as RuntimeType;
		if (runtimeType != null)
		{
			if (value.GetType() == targetType)
			{
				return value;
			}
			if (runtimeType == ConvertTypes[3])
			{
				return value.ToBoolean(provider);
			}
			if (runtimeType == ConvertTypes[4])
			{
				return value.ToChar(provider);
			}
			if (runtimeType == ConvertTypes[5])
			{
				return value.ToSByte(provider);
			}
			if (runtimeType == ConvertTypes[6])
			{
				return value.ToByte(provider);
			}
			if (runtimeType == ConvertTypes[7])
			{
				return value.ToInt16(provider);
			}
			if (runtimeType == ConvertTypes[8])
			{
				return value.ToUInt16(provider);
			}
			if (runtimeType == ConvertTypes[9])
			{
				return value.ToInt32(provider);
			}
			if (runtimeType == ConvertTypes[10])
			{
				return value.ToUInt32(provider);
			}
			if (runtimeType == ConvertTypes[11])
			{
				return value.ToInt64(provider);
			}
			if (runtimeType == ConvertTypes[12])
			{
				return value.ToUInt64(provider);
			}
			if (runtimeType == ConvertTypes[13])
			{
				return value.ToSingle(provider);
			}
			if (runtimeType == ConvertTypes[14])
			{
				return value.ToDouble(provider);
			}
			if (runtimeType == ConvertTypes[15])
			{
				return value.ToDecimal(provider);
			}
			if (runtimeType == ConvertTypes[16])
			{
				return value.ToDateTime(provider);
			}
			if (runtimeType == ConvertTypes[18])
			{
				return value.ToString(provider);
			}
			if (runtimeType == ConvertTypes[1])
			{
				return value;
			}
			if (runtimeType == EnumType)
			{
				return (Enum)value;
			}
			if (runtimeType == ConvertTypes[2])
			{
				throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast to DBNull."));
			}
			if (runtimeType == ConvertTypes[0])
			{
				throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast to Empty."));
			}
		}
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", value.GetType().FullName, targetType.FullName));
	}

	/// <summary>Returns an object of the specified type and whose value is equivalent to the specified object.</summary>
	/// <returns>An object whose type is <paramref name="conversionType" /> and whose value is equivalent to <paramref name="value" />.-or-A null reference (Nothing in Visual Basic), if <paramref name="value" /> is null and <paramref name="conversionType" /> is not a value type. </returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="conversionType">The type of object to return. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported.  -or-<paramref name="value" /> is null and <paramref name="conversionType" /> is a value type.-or-<paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in a format recognized by <paramref name="conversionType" />.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is out of the range of <paramref name="conversionType" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="conversionType" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public static object ChangeType(object value, Type conversionType)
	{
		return ChangeType(value, conversionType, Thread.CurrentThread.CurrentCulture);
	}

	/// <summary>Returns an object of the specified type whose value is equivalent to the specified object. A parameter supplies culture-specific formatting information.</summary>
	/// <returns>An object whose type is <paramref name="conversionType" /> and whose value is equivalent to <paramref name="value" />.-or- <paramref name="value" />, if the <see cref="T:System.Type" /> of <paramref name="value" /> and <paramref name="conversionType" /> are equal.-or- A null reference (Nothing in Visual Basic), if <paramref name="value" /> is null and <paramref name="conversionType" /> is not a value type.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="conversionType">The type of object to return. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. -or-<paramref name="value" /> is null and <paramref name="conversionType" /> is a value type.-or-<paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in a format for <paramref name="conversionType" /> recognized by <paramref name="provider" />.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is out of the range of <paramref name="conversionType" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="conversionType" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public static object ChangeType(object value, Type conversionType, IFormatProvider provider)
	{
		if (conversionType == null)
		{
			throw new ArgumentNullException("conversionType");
		}
		if (value == null)
		{
			if (conversionType.IsValueType)
			{
				throw new InvalidCastException(Environment.GetResourceString("Null object cannot be converted to a value type."));
			}
			return null;
		}
		if (!(value is IConvertible convertible))
		{
			if (value.GetType() == conversionType)
			{
				return value;
			}
			throw new InvalidCastException(Environment.GetResourceString("Object must implement IConvertible."));
		}
		RuntimeType runtimeType = conversionType as RuntimeType;
		if (runtimeType == ConvertTypes[3])
		{
			return convertible.ToBoolean(provider);
		}
		if (runtimeType == ConvertTypes[4])
		{
			return convertible.ToChar(provider);
		}
		if (runtimeType == ConvertTypes[5])
		{
			return convertible.ToSByte(provider);
		}
		if (runtimeType == ConvertTypes[6])
		{
			return convertible.ToByte(provider);
		}
		if (runtimeType == ConvertTypes[7])
		{
			return convertible.ToInt16(provider);
		}
		if (runtimeType == ConvertTypes[8])
		{
			return convertible.ToUInt16(provider);
		}
		if (runtimeType == ConvertTypes[9])
		{
			return convertible.ToInt32(provider);
		}
		if (runtimeType == ConvertTypes[10])
		{
			return convertible.ToUInt32(provider);
		}
		if (runtimeType == ConvertTypes[11])
		{
			return convertible.ToInt64(provider);
		}
		if (runtimeType == ConvertTypes[12])
		{
			return convertible.ToUInt64(provider);
		}
		if (runtimeType == ConvertTypes[13])
		{
			return convertible.ToSingle(provider);
		}
		if (runtimeType == ConvertTypes[14])
		{
			return convertible.ToDouble(provider);
		}
		if (runtimeType == ConvertTypes[15])
		{
			return convertible.ToDecimal(provider);
		}
		if (runtimeType == ConvertTypes[16])
		{
			return convertible.ToDateTime(provider);
		}
		if (runtimeType == ConvertTypes[18])
		{
			return convertible.ToString(provider);
		}
		if (runtimeType == ConvertTypes[1])
		{
			return value;
		}
		return convertible.ToType(conversionType, provider);
	}

	/// <summary>Converts the value of a specified object to an equivalent Boolean value.</summary>
	/// <returns>true or false, which reflects the value returned by invoking the <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" /> method for the underlying type of <paramref name="value" />. If <paramref name="value" /> is null, the method returns false. </returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is a string that does not equal <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.-or-The conversion of <paramref name="value" /> to a <see cref="T:System.Boolean" /> is not supported.</exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToBoolean(null);
		}
		return false;
	}

	/// <summary>Converts the value of the specified object to an equivalent Boolean value, using the specified culture-specific formatting information.</summary>
	/// <returns>true or false, which reflects the value returned by invoking the <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" /> method for the underlying type of <paramref name="value" />. If <paramref name="value" /> is null, the method returns false.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is a string that does not equal <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.-or-The conversion of <paramref name="value" /> to a <see cref="T:System.Boolean" /> is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToBoolean(provider);
		}
		return false;
	}

	/// <summary>Returns the specified Boolean value; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The Boolean value to return. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(bool value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static bool ToBoolean(sbyte value)
	{
		return value != 0;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(char value)
	{
		return ((IConvertible)value).ToBoolean((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(byte value)
	{
		return value != 0;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(short value)
	{
		return value != 0;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static bool ToBoolean(ushort value)
	{
		return value != 0;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(int value)
	{
		return value != 0;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static bool ToBoolean(uint value)
	{
		return value != 0;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(long value)
	{
		return value != 0;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static bool ToBoolean(ulong value)
	{
		return value != 0;
	}

	/// <summary>Converts the specified string representation of a logical value to its Boolean equivalent.</summary>
	/// <returns>true if <paramref name="value" /> equals <see cref="F:System.Boolean.TrueString" />, or false if <paramref name="value" /> equals <see cref="F:System.Boolean.FalseString" /> or null.</returns>
	/// <param name="value">A string that contains the value of either <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not equal to <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(string value)
	{
		if (value == null)
		{
			return false;
		}
		return bool.Parse(value);
	}

	/// <summary>Converts the specified string representation of a logical value to its Boolean equivalent, using the specified culture-specific formatting information.</summary>
	/// <returns>true if <paramref name="value" /> equals <see cref="F:System.Boolean.TrueString" />, or false if <paramref name="value" /> equals <see cref="F:System.Boolean.FalseString" /> or null.</returns>
	/// <param name="value">A string that contains the value of either <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not equal to <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return false;
		}
		return bool.Parse(value);
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(float value)
	{
		return value != 0f;
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(double value)
	{
		return value != 0.0;
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent Boolean value.</summary>
	/// <returns>true if <paramref name="value" /> is not zero; otherwise, false.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(decimal value)
	{
		return value != 0m;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(DateTime value)
	{
		return ((IConvertible)value).ToBoolean((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to value, or <see cref="F:System.Char.MinValue" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is a null string.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.-or-The conversion of <paramref name="value" /> to a <see cref="T:System.Char" /> is not supported. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" /> or greater than <see cref="F:System.Char.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToChar(null);
		}
		return '\0';
	}

	/// <summary>Converts the value of the specified object to its equivalent Unicode character, using the specified culture-specific formatting information.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />, or <see cref="F:System.Char.MinValue" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is a null string.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion of <paramref name="value" /> to a <see cref="T:System.Char" /> is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" /> or greater than <see cref="F:System.Char.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToChar(provider);
		}
		return '\0';
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(bool value)
	{
		return ((IConvertible)value).ToChar((IFormatProvider)null);
	}

	/// <summary>Returns the specified Unicode character value; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The Unicode character to return. </param>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(char value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static char ToChar(sbyte value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."));
		}
		return (char)value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(byte value)
	{
		return (char)value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(short value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."));
		}
		return (char)value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static char ToChar(ushort value)
	{
		return (char)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" /> or greater than <see cref="F:System.Char.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(int value)
	{
		if (value < 0 || value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."));
		}
		return (char)value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Char.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static char ToChar(uint value)
	{
		if (value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."));
		}
		return (char)value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Char.MinValue" /> or greater than <see cref="F:System.Char.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(long value)
	{
		if (value < 0 || value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."));
		}
		return (char)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to its equivalent Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Char.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static char ToChar(ulong value)
	{
		if (value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a character."));
		}
		return (char)value;
	}

	/// <summary>Converts the first character of a specified string to a Unicode character.</summary>
	/// <returns>A Unicode character that is equivalent to the first and only character in <paramref name="value" />.</returns>
	/// <param name="value">A string of length 1. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The length of <paramref name="value" /> is not 1. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(string value)
	{
		return ToChar(value, null);
	}

	/// <summary>Converts the first character of a specified string to a Unicode character, using specified culture-specific formatting information.</summary>
	/// <returns>A Unicode character that is equivalent to the first and only character in <paramref name="value" />.</returns>
	/// <param name="value">A string of length 1 or null. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The length of <paramref name="value" /> is not 1. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length != 1)
		{
			throw new FormatException(Environment.GetResourceString("String must be exactly one character long."));
		}
		return value[0];
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(float value)
	{
		return ((IConvertible)value).ToChar((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(double value)
	{
		return ((IConvertible)value).ToChar((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(decimal value)
	{
		return ((IConvertible)value).ToChar((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(DateTime value)
	{
		return ((IConvertible)value).ToChar((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to an 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format. </exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToSByte(null);
		}
		return 0;
	}

	/// <summary>Converts the value of the specified object to an 8-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format. </exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToSByte(provider);
		}
		return 0;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 8-bit signed integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	/// <summary>Returns the specified 8-bit signed integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 8-bit signed integer to return. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(char value)
	{
		if (value > '\u007f')
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(byte value)
	{
		if (value > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to the equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(short value)
	{
		if (value < -128 || value > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(ushort value)
	{
		if (value > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(int value)
	{
		if (value < -128 || value > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(uint value)
	{
		if ((long)value > 127L)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(long value)
	{
		if (value < -128 || value > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(ulong value)
	{
		if (value > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 8-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 8-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(float value)
	{
		return ToSByte((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 8-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 8-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(double value)
	{
		return ToSByte(ToInt32(value));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 8-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 8-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(decimal value)
	{
		return decimal.ToSByte(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if value is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(string value)
	{
		if (value == null)
		{
			return 0;
		}
		return sbyte.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 8-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(string value, IFormatProvider provider)
	{
		return sbyte.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(DateTime value)
	{
		return ((IConvertible)value).ToSByte((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to an 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in the property format for a <see cref="T:System.Byte" /> value.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement <see cref="T:System.IConvertible" />. -or-Conversion from <paramref name="value" /> to the <see cref="T:System.Byte" /> type is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToByte(null);
		}
		return 0;
	}

	/// <summary>Converts the value of the specified object to an 8-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in the property format for a <see cref="T:System.Byte" /> value.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement <see cref="T:System.IConvertible" />. -or-Conversion from <paramref name="value" /> to the <see cref="T:System.Byte" /> type is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToByte(provider);
		}
		return 0;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 8-bit unsigned integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	/// <summary>Returns the specified 8-bit unsigned integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 8-bit unsigned integer to return. </param>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(char value)
	{
		if (value > 'ÿ')
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to be converted. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte ToByte(sbyte value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(short value)
	{
		if (value < 0 || value > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte ToByte(ushort value)
	{
		if (value > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(int value)
	{
		if (value < 0 || value > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte ToByte(uint value)
	{
		if (value > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(long value)
	{
		if (value < 0 || value > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte ToByte(ulong value)
	{
		if (value > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 8-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">A single-precision floating-point number. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" /> or less than <see cref="F:System.Byte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(float value)
	{
		return ToByte((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 8-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" /> or less than <see cref="F:System.Byte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(double value)
	{
		return ToByte(ToInt32(value));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 8-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Byte.MaxValue" /> or less than <see cref="F:System.Byte.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(decimal value)
	{
		return decimal.ToByte(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(string value)
	{
		if (value == null)
		{
			return 0;
		}
		return byte.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 8-bit unsigned integer, using specified culture-specific formatting information.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0;
		}
		return byte.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(DateTime value)
	{
		return ((IConvertible)value).ToByte((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format for an <see cref="T:System.Int16" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToInt16(null);
		}
		return 0;
	}

	/// <summary>Converts the value of the specified object to a 16-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format for an <see cref="T:System.Int16" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement <see cref="T:System.IConvertible" />. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToInt16(provider);
		}
		return 0;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 16-bit signed integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />. </returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(char value)
	{
		if (value > '翿')
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent 16-bit signed integer.</summary>
	/// <returns>A 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static short ToInt16(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static short ToInt16(ushort value)
	{
		if (value > 32767)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 16-bit signed integer.</summary>
	/// <returns>The 16-bit signed integer equivalent of <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(int value)
	{
		if (value < -32768 || value > 32767)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static short ToInt16(uint value)
	{
		if ((long)value > 32767L)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)value;
	}

	/// <summary>Returns the specified 16-bit signed integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 16-bit signed integer to return. </param>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(short value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(long value)
	{
		if (value < -32768 || value > 32767)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static short ToInt16(ulong value)
	{
		if (value > 32767)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 16-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 16-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(float value)
	{
		return ToInt16((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 16-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 16-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(double value)
	{
		return ToInt16(ToInt32(value));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 16-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 16-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(decimal value)
	{
		return decimal.ToInt16(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(string value)
	{
		if (value == null)
		{
			return 0;
		}
		return short.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 16-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0;
		}
		return short.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(DateTime value)
	{
		return ((IConvertible)value).ToInt16((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the  <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt16.MinValue" /> or greater than <see cref="F:System.UInt16.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToUInt16(null);
		}
		return 0;
	}

	/// <summary>Converts the value of the specified object to a 16-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the  <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt16.MinValue" /> or greater than <see cref="F:System.UInt16.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToUInt16(provider);
		}
		return 0;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 16-bit unsigned integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 16-bit unsigned integer.</summary>
	/// <returns>The 16-bit unsigned integer equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(char value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(sbyte value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to the equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(short value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(int value)
	{
		if (value < 0 || value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)value;
	}

	/// <summary>Returns the specified 16-bit unsigned integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 16-bit unsigned integer to return. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(ushort value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(uint value)
	{
		if (value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(long value)
	{
		if (value < 0 || value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(ulong value)
	{
		if (value > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 16-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(float value)
	{
		return ToUInt16((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 16-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(double value)
	{
		return ToUInt16(ToInt32(value));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 16-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(decimal value)
	{
		return decimal.ToUInt16(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt16.MinValue" /> or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(string value)
	{
		if (value == null)
		{
			return 0;
		}
		return ushort.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 16-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt16.MinValue" /> or greater than <see cref="F:System.UInt16.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0;
		}
		return ushort.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(DateTime value)
	{
		return ((IConvertible)value).ToUInt16((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the  <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToInt32(null);
		}
		return 0;
	}

	/// <summary>Converts the value of the specified object to a 32-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement <see cref="T:System.IConvertible" />. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToInt32(provider);
		}
		return 0;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 32-bit signed integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(char value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent 32-bit signed integer.</summary>
	/// <returns>A 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static int ToInt32(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(short value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static int ToInt32(ushort value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static int ToInt32(uint value)
	{
		if (value > int.MaxValue)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int32."));
		}
		return (int)value;
	}

	/// <summary>Returns the specified 32-bit signed integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 32-bit signed integer to return. </param>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(int value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" /> or less than <see cref="F:System.Int32.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(long value)
	{
		if (value < int.MinValue || value > int.MaxValue)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int32."));
		}
		return (int)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static int ToInt32(ulong value)
	{
		if (value > int.MaxValue)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int32."));
		}
		return (int)value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 32-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 32-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" /> or less than <see cref="F:System.Int32.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(float value)
	{
		return ToInt32((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 32-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 32-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" /> or less than <see cref="F:System.Int32.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(double value)
	{
		if (value >= 0.0)
		{
			if (value < 2147483647.5)
			{
				int num = (int)value;
				double num2 = value - (double)num;
				if (num2 > 0.5 || (num2 == 0.5 && (num & 1) != 0))
				{
					num++;
				}
				return num;
			}
		}
		else if (value >= -2147483648.5)
		{
			int num3 = (int)value;
			double num4 = value - (double)num3;
			if (num4 < -0.5 || (num4 == -0.5 && (num3 & 1) != 0))
			{
				num3--;
			}
			return num3;
		}
		throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int32."));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 32-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 32-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int32.MaxValue" /> or less than <see cref="F:System.Int32.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static int ToInt32(decimal value)
	{
		return decimal.FCallToInt32(value);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(string value)
	{
		if (value == null)
		{
			return 0;
		}
		return int.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 32-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0;
		}
		return int.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert.</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(DateTime value)
	{
		return ((IConvertible)value).ToInt32((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt32.MinValue" /> or greater than <see cref="F:System.UInt32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToUInt32(null);
		}
		return 0u;
	}

	/// <summary>Converts the value of the specified object to a 32-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt32.MinValue" /> or greater than <see cref="F:System.UInt32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToUInt32(provider);
		}
		return 0u;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(bool value)
	{
		if (!value)
		{
			return 0u;
		}
		return 1u;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(char value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(sbyte value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
		}
		return (uint)value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(short value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
		}
		return (uint)value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(ushort value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(int value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
		}
		return (uint)value;
	}

	/// <summary>Returns the specified 32-bit unsigned integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 32-bit unsigned integer to return. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(uint value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(long value)
	{
		if (value < 0 || value > uint.MaxValue)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
		}
		return (uint)value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(ulong value)
	{
		if (value > uint.MaxValue)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
		}
		return (uint)value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 32-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(float value)
	{
		return ToUInt32((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 32-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(double value)
	{
		if (value >= -0.5 && value < 4294967295.5)
		{
			uint num = (uint)value;
			double num2 = value - (double)num;
			if (num2 > 0.5 || (num2 == 0.5 && (num & 1) != 0))
			{
				num++;
			}
			return num;
		}
		throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt32."));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 32-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(decimal value)
	{
		return decimal.ToUInt32(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt32.MinValue" /> or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(string value)
	{
		if (value == null)
		{
			return 0u;
		}
		return uint.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 32-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt32.MinValue" /> or greater than <see cref="F:System.UInt32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0u;
		}
		return uint.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(DateTime value)
	{
		return ((IConvertible)value).ToUInt32((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToInt64(null);
		}
		return 0L;
	}

	/// <summary>Converts the value of the specified object to a 64-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.-or-The conversion is not supported. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToInt64(provider);
		}
		return 0L;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 64-bit signed integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(bool value)
	{
		return value ? 1 : 0;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(char value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static long ToInt64(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(short value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static long ToInt64(ushort value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(int value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static long ToInt64(uint value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static long ToInt64(ulong value)
	{
		if (value > long.MaxValue)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int64."));
		}
		return (long)value;
	}

	/// <summary>Returns the specified 64-bit signed integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">A 64-bit signed integer. </param>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(long value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 64-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 64-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int64.MaxValue" /> or less than <see cref="F:System.Int64.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(float value)
	{
		return ToInt64((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 64-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 64-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int64.MaxValue" /> or less than <see cref="F:System.Int64.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(double value)
	{
		return checked((long)Math.Round(value));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 64-bit signed integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 64-bit signed integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Int64.MaxValue" /> or less than <see cref="F:System.Int64.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(decimal value)
	{
		return decimal.ToInt64(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains a number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(string value)
	{
		if (value == null)
		{
			return 0L;
		}
		return long.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 64-bit signed integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0L;
		}
		return long.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(DateTime value)
	{
		return ((IConvertible)value).ToInt64((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt64.MinValue" /> or greater than <see cref="F:System.UInt64.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToUInt64(null);
		}
		return 0uL;
	}

	/// <summary>Converts the value of the specified object to a 64-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt64.MinValue" /> or greater than <see cref="F:System.UInt64.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToUInt64(provider);
		}
		return 0uL;
	}

	/// <summary>Converts the specified Boolean value to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(bool value)
	{
		if (!value)
		{
			return 0uL;
		}
		return 1uL;
	}

	/// <summary>Converts the value of the specified Unicode character to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(char value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(sbyte value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt64."));
		}
		return (ulong)value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(byte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(short value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt64."));
		}
		return (ulong)value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(ushort value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(int value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt64."));
		}
		return (ulong)value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(uint value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(long value)
	{
		if (value < 0)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt64."));
		}
		return (ulong)value;
	}

	/// <summary>Returns the specified 64-bit unsigned integer; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The 64-bit unsigned integer to return. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(ulong value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 64-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(float value)
	{
		return ToUInt64((double)value);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 64-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(double value)
	{
		return checked((ulong)Math.Round(value));
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>
	///   <paramref name="value" />, rounded to the nearest 64-bit unsigned integer. If <paramref name="value" /> is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is less than zero or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(decimal value)
	{
		return decimal.ToUInt64(decimal.Round(value, 0));
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt64.MinValue" /> or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(string value)
	{
		if (value == null)
		{
			return 0uL;
		}
		return ulong.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent 64-bit unsigned integer, using the specified culture-specific formatting information.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> does not consist of an optional sign followed by a sequence of digits (0 through 9). </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.UInt64.MinValue" /> or greater than <see cref="F:System.UInt64.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0uL;
		}
		return ulong.Parse(value, NumberStyles.Integer, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(DateTime value)
	{
		return ((IConvertible)value).ToUInt64((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToSingle(null);
		}
		return 0f;
	}

	/// <summary>Converts the value of the specified object to an single-precision floating-point number, using the specified culture-specific formatting information.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement <see cref="T:System.IConvertible" />. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToSingle(provider);
		}
		return 0f;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent single-precision floating-point number.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static float ToSingle(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(byte value)
	{
		return (int)value;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(char value)
	{
		return ((IConvertible)value).ToSingle((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(short value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static float ToSingle(ushort value)
	{
		return (int)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(int value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static float ToSingle(uint value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(long value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static float ToSingle(ulong value)
	{
		return value;
	}

	/// <summary>Returns the specified single-precision floating-point number; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The single-precision floating-point number to return. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(float value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.<paramref name="value" /> is rounded using rounding to nearest. For example, when rounded to two decimals, the value 2.345 becomes 2.34 and the value 2.355 becomes 2.36.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(double value)
	{
		return (float)value;
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to <paramref name="value" />.<paramref name="value" /> is rounded using rounding to nearest. For example, when rounded to two decimals, the value 2.345 becomes 2.34 and the value 2.355 becomes 2.36.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(decimal value)
	{
		return (float)value;
	}

	/// <summary>Converts the specified string representation of a number to an equivalent single-precision floating-point number.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a number in a valid format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(string value)
	{
		if (value == null)
		{
			return 0f;
		}
		return float.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent single-precision floating-point number, using the specified culture-specific formatting information.</summary>
	/// <returns>A single-precision floating-point number that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a number in a valid format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Single.MinValue" /> or greater than <see cref="F:System.Single.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0f;
		}
		return float.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
	}

	/// <summary>Converts the specified Boolean value to the equivalent single-precision floating-point number.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(bool value)
	{
		return value ? 1 : 0;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static float ToSingle(DateTime value)
	{
		return ((IConvertible)value).ToSingle((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to a double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format for a <see cref="T:System.Double" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Double.MinValue" /> or greater than <see cref="F:System.Double.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToDouble(null);
		}
		return 0.0;
	}

	/// <summary>Converts the value of the specified object to an double-precision floating-point number, using the specified culture-specific formatting information.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />, or zero if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format for a <see cref="T:System.Double" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Double.MinValue" /> or greater than <see cref="F:System.Double.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToDouble(provider);
		}
		return 0.0;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent double-precision floating-point number.</summary>
	/// <returns>The 8-bit signed integer that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static double ToDouble(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent double-precision floating-point number.</summary>
	/// <returns>The double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(byte value)
	{
		return (int)value;
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(short value)
	{
		return value;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(char value)
	{
		return ((IConvertible)value).ToDouble((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to the equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static double ToDouble(ushort value)
	{
		return (int)value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(int value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static double ToDouble(uint value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(long value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static double ToDouble(ulong value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The single-precision floating-point number. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(float value)
	{
		return value;
	}

	/// <summary>Returns the specified double-precision floating-point number; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The double-precision floating-point number to return. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(double value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified decimal number to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(decimal value)
	{
		return (double)value;
	}

	/// <summary>Converts the specified string representation of a number to an equivalent double-precision floating-point number.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a number in a valid format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Double.MinValue" /> or greater than <see cref="F:System.Double.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(string value)
	{
		if (value == null)
		{
			return 0.0;
		}
		return double.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent double-precision floating-point number, using the specified culture-specific formatting information.</summary>
	/// <returns>A double-precision floating-point number that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a number in a valid format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Double.MinValue" /> or greater than <see cref="F:System.Double.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0.0;
		}
		return double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
	}

	/// <summary>Converts the specified Boolean value to the equivalent double-precision floating-point number.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(bool value)
	{
		return value ? 1 : 0;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static double ToDouble(DateTime value)
	{
		return ((IConvertible)value).ToDouble((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format for a <see cref="T:System.Decimal" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToDecimal(null);
		}
		return 0m;
	}

	/// <summary>Converts the value of the specified object to an equivalent decimal number, using the specified culture-specific formatting information.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not in an appropriate format for a <see cref="T:System.Decimal" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface.-or-The conversion is not supported. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToDecimal(provider);
		}
		return 0m;
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to the equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static decimal ToDecimal(sbyte value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to the equivalent decimal number.</summary>
	/// <returns>The decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(byte value)
	{
		return value;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(char value)
	{
		return ((IConvertible)value).ToDecimal((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(short value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to an equivalent decimal number.</summary>
	/// <returns>The decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static decimal ToDecimal(ushort value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(int value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static decimal ToDecimal(uint value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(long value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static decimal ToDecimal(ulong value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to the equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />. </returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue" /> or less than <see cref="F:System.Decimal.MinValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(float value)
	{
		return (decimal)value;
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to <paramref name="value" />. </returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue" /> or less than <see cref="F:System.Decimal.MinValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(double value)
	{
		return (decimal)value;
	}

	/// <summary>Converts the specified string representation of a number to an equivalent decimal number.</summary>
	/// <returns>A decimal number that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains a number to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a number in a valid format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(string value)
	{
		if (value == null)
		{
			return 0m;
		}
		return decimal.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent decimal number, using the specified culture-specific formatting information.</summary>
	/// <returns>A decimal number that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains a number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a number in a valid format.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return 0m;
		}
		return decimal.Parse(value, NumberStyles.Number, provider);
	}

	/// <summary>Returns the specified decimal number; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">A decimal number. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(decimal value)
	{
		return value;
	}

	/// <summary>Converts the specified Boolean value to the equivalent decimal number.</summary>
	/// <returns>The number 1 if <paramref name="value" /> is true; otherwise, 0.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(bool value)
	{
		return value ? 1 : 0;
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static decimal ToDecimal(DateTime value)
	{
		return ((IConvertible)value).ToDecimal((IFormatProvider)null);
	}

	/// <summary>Returns the specified <see cref="T:System.DateTime" /> object; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">A date and time value. </param>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(DateTime value)
	{
		return value;
	}

	/// <summary>Converts the value of the specified object to a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The date and time equivalent of the value of <paramref name="value" />, or a date and time equivalent of <see cref="F:System.DateTime.MinValue" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface, or null. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid date and time value.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(object value)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToDateTime(null);
		}
		return DateTime.MinValue;
	}

	/// <summary>Converts the value of the specified object to a <see cref="T:System.DateTime" /> object, using the specified culture-specific formatting information.</summary>
	/// <returns>The date and time equivalent of the value of <paramref name="value" />, or the date and time equivalent of <see cref="F:System.DateTime.MinValue" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that implements the <see cref="T:System.IConvertible" /> interface. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid date and time value.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IConvertible" /> interface. -or-The conversion is not supported.</exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(object value, IFormatProvider provider)
	{
		if (value != null)
		{
			return ((IConvertible)value).ToDateTime(provider);
		}
		return DateTime.MinValue;
	}

	/// <summary>Converts the specified string representation of a date and time to an equivalent date and time value.</summary>
	/// <returns>The date and time equivalent of the value of <paramref name="value" />, or the date and time equivalent of <see cref="F:System.DateTime.MinValue" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">The string representation of a date and time.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a properly formatted date and time string. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(string value)
	{
		if (value == null)
		{
			return new DateTime(0L);
		}
		return DateTime.Parse(value, CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the specified string representation of a number to an equivalent date and time, using the specified culture-specific formatting information.</summary>
	/// <returns>The date and time equivalent of the value of <paramref name="value" />, or the date and time equivalent of <see cref="F:System.DateTime.MinValue" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains a date and time to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a properly formatted date and time string. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(string value, IFormatProvider provider)
	{
		if (value == null)
		{
			return new DateTime(0L);
		}
		return DateTime.Parse(value, provider);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static DateTime ToDateTime(sbyte value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(byte value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(short value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static DateTime ToDateTime(ushort value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(int value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static DateTime ToDateTime(uint value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(long value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static DateTime ToDateTime(ulong value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(bool value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(char value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The single-precision floating-point value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(float value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The double-precision floating-point value to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(double value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Calling this method always throws <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>This conversion is not supported. No value is returned.</returns>
	/// <param name="value">The number to convert. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported. </exception>
	/// <filterpriority>1</filterpriority>
	public static DateTime ToDateTime(decimal value)
	{
		return ((IConvertible)value).ToDateTime((IFormatProvider)null);
	}

	/// <summary>Converts the value of the specified object to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />, or <see cref="F:System.String.Empty" /> if value is null.</returns>
	/// <param name="value">An object that supplies the value to convert, or null. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(object value)
	{
		return ToString(value, null);
	}

	/// <summary>Converts the value of the specified object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />, or <see cref="F:System.String.Empty" /> if <paramref name="value" /> is null.</returns>
	/// <param name="value">An object that supplies the value to convert, or null. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(object value, IFormatProvider provider)
	{
		if (value is IConvertible convertible)
		{
			return convertible.ToString(provider);
		}
		if (value is IFormattable formattable)
		{
			return formattable.ToString(null, provider);
		}
		if (value != null)
		{
			return value.ToString();
		}
		return string.Empty;
	}

	/// <summary>Converts the specified Boolean value to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(bool value)
	{
		return value.ToString();
	}

	/// <summary>Converts the specified Boolean value to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The Boolean value to convert. </param>
	/// <param name="provider">An instance of an object. This parameter is ignored.</param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(bool value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified Unicode character to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(char value)
	{
		return char.ToString(value);
	}

	/// <summary>Converts the value of the specified Unicode character to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The Unicode character to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(char value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(sbyte value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 8-bit signed integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit signed integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(sbyte value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(byte value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 8-bit unsigned integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(byte value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(short value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 16-bit signed integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(short value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(ushort value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 16-bit unsigned integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 16-bit unsigned integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(ushort value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(int value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 32-bit signed integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(int value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(uint value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 32-bit unsigned integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 32-bit unsigned integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(uint value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(long value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 64-bit signed integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(long value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(ulong value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified 64-bit unsigned integer to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The 64-bit unsigned integer to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static string ToString(ulong value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(float value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified single-precision floating-point number to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The single-precision floating-point number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(float value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(double value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified double-precision floating-point number to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The double-precision floating-point number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(double value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified decimal number to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(decimal value)
	{
		return value.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the value of the specified decimal number to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The decimal number to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(decimal value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Converts the value of the specified <see cref="T:System.DateTime" /> to its equivalent string representation.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(DateTime value)
	{
		return value.ToString();
	}

	/// <summary>Converts the value of the specified <see cref="T:System.DateTime" /> to its equivalent string representation, using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of <paramref name="value" />.</returns>
	/// <param name="value">The date and time value to convert. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(DateTime value, IFormatProvider provider)
	{
		return value.ToString(provider);
	}

	/// <summary>Returns the specified string instance; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The string to return. </param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(string value)
	{
		return value;
	}

	/// <summary>Returns the specified string instance; no actual conversion is performed.</summary>
	/// <returns>
	///   <paramref name="value" /> is returned unchanged.</returns>
	/// <param name="value">The string to return. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
	/// <filterpriority>1</filterpriority>
	public static string ToString(string value, IFormatProvider provider)
	{
		return value;
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 8-bit unsigned integer.</summary>
	/// <returns>An 8-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a base 10 unsigned number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.Byte.MinValue" /> or greater than <see cref="F:System.Byte.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static byte ToByte(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		int num = ParseNumbers.StringToInt(value, fromBase, 4608);
		if (num < 0 || num > 255)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an unsigned byte."));
		}
		return (byte)num;
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 8-bit signed integer.</summary>
	/// <returns>An 8-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.SByte.MinValue" /> or greater than <see cref="F:System.SByte.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static sbyte ToSByte(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		int num = ParseNumbers.StringToInt(value, fromBase, 5120);
		if (fromBase != 10 && num <= 255)
		{
			return (sbyte)num;
		}
		if (num < -128 || num > 127)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a signed byte."));
		}
		return (sbyte)num;
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 16-bit signed integer.</summary>
	/// <returns>A 16-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.Int16.MinValue" /> or greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static short ToInt16(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		int num = ParseNumbers.StringToInt(value, fromBase, 6144);
		if (fromBase != 10 && num <= 65535)
		{
			return (short)num;
		}
		if (num < -32768 || num > 32767)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for an Int16."));
		}
		return (short)num;
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 16-bit unsigned integer.</summary>
	/// <returns>A 16-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.UInt16.MinValue" /> or greater than <see cref="F:System.UInt16.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		int num = ParseNumbers.StringToInt(value, fromBase, 4608);
		if (num < 0 || num > 65535)
		{
			throw new OverflowException(Environment.GetResourceString("Value was either too large or too small for a UInt16."));
		}
		return (ushort)num;
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static int ToInt32(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return ParseNumbers.StringToInt(value, fromBase, 4096);
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 32-bit unsigned integer.</summary>
	/// <returns>A 32-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.UInt32.MinValue" /> or greater than <see cref="F:System.UInt32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return (uint)ParseNumbers.StringToInt(value, fromBase, 4608);
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 signed number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static long ToInt64(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return ParseNumbers.StringToLong(value, fromBase, 4096);
	}

	/// <summary>Converts the string representation of a number in a specified base to an equivalent 64-bit unsigned integer.</summary>
	/// <returns>A 64-bit unsigned integer that is equivalent to the number in <paramref name="value" />, or 0 (zero) if <paramref name="value" /> is null.</returns>
	/// <param name="value">A string that contains the number to convert. </param>
	/// <param name="fromBase">The base of the number in <paramref name="value" />, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fromBase" /> is not 2, 8, 10, or 16. -or-<paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="value" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> contains a character that is not a valid digit in the base specified by <paramref name="fromBase" />. The exception message indicates that there are no digits to convert if the first character in <paramref name="value" /> is invalid; otherwise, the message indicates that <paramref name="value" /> contains invalid trailing characters.</exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" />, which represents a non-base 10 unsigned number, is prefixed with a negative sign.-or-<paramref name="value" /> represents a number that is less than <see cref="F:System.UInt64.MinValue" /> or greater than <see cref="F:System.UInt64.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(string value, int fromBase)
	{
		if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return (ulong)ParseNumbers.StringToLong(value, fromBase, 4608);
	}

	/// <summary>Converts the value of an 8-bit unsigned integer to its equivalent string representation in a specified base.</summary>
	/// <returns>The string representation of <paramref name="value" /> in base <paramref name="toBase" />.</returns>
	/// <param name="value">The 8-bit unsigned integer to convert. </param>
	/// <param name="toBase">The base of the return value, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="toBase" /> is not 2, 8, 10, or 16. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string ToString(byte value, int toBase)
	{
		if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return ParseNumbers.IntToString(value, toBase, -1, ' ', 64);
	}

	/// <summary>Converts the value of a 16-bit signed integer to its equivalent string representation in a specified base.</summary>
	/// <returns>The string representation of <paramref name="value" /> in base <paramref name="toBase" />.</returns>
	/// <param name="value">The 16-bit signed integer to convert. </param>
	/// <param name="toBase">The base of the return value, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="toBase" /> is not 2, 8, 10, or 16. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string ToString(short value, int toBase)
	{
		if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return ParseNumbers.IntToString(value, toBase, -1, ' ', 128);
	}

	/// <summary>Converts the value of a 32-bit signed integer to its equivalent string representation in a specified base.</summary>
	/// <returns>The string representation of <paramref name="value" /> in base <paramref name="toBase" />.</returns>
	/// <param name="value">The 32-bit signed integer to convert. </param>
	/// <param name="toBase">The base of the return value, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="toBase" /> is not 2, 8, 10, or 16. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string ToString(int value, int toBase)
	{
		if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return ParseNumbers.IntToString(value, toBase, -1, ' ', 0);
	}

	/// <summary>Converts the value of a 64-bit signed integer to its equivalent string representation in a specified base.</summary>
	/// <returns>The string representation of <paramref name="value" /> in base <paramref name="toBase" />.</returns>
	/// <param name="value">The 64-bit signed integer to convert. </param>
	/// <param name="toBase">The base of the return value, which must be 2, 8, 10, or 16. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="toBase" /> is not 2, 8, 10, or 16. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static string ToString(long value, int toBase)
	{
		if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
		{
			throw new ArgumentException(Environment.GetResourceString("Invalid Base."));
		}
		return ParseNumbers.LongToString(value, toBase, -1, ' ', 0);
	}

	/// <summary>Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64 digits.</summary>
	/// <returns>The string representation, in base 64, of the contents of <paramref name="inArray" />.</returns>
	/// <param name="inArray">An array of 8-bit unsigned integers. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static string ToBase64String(byte[] inArray)
	{
		if (inArray == null)
		{
			throw new ArgumentNullException("inArray");
		}
		return ToBase64String(inArray, 0, inArray.Length, Base64FormattingOptions.None);
	}

	/// <summary>Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64 digits. A parameter specifies whether to insert line breaks in the return value.</summary>
	/// <returns>The string representation in base 64 of the elements in <paramref name="inArray" />.</returns>
	/// <param name="inArray">An array of 8-bit unsigned integers. </param>
	/// <param name="options">
	///   <see cref="F:System.Base64FormattingOptions.InsertLineBreaks" /> to insert a line break every 76 characters, or <see cref="F:System.Base64FormattingOptions.None" /> to not insert line breaks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a valid <see cref="T:System.Base64FormattingOptions" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	public static string ToBase64String(byte[] inArray, Base64FormattingOptions options)
	{
		if (inArray == null)
		{
			throw new ArgumentNullException("inArray");
		}
		return ToBase64String(inArray, 0, inArray.Length, options);
	}

	/// <summary>Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64 digits. Parameters specify the subset as an offset in the input array, and the number of elements in the array to convert.</summary>
	/// <returns>The string representation in base 64 of <paramref name="length" /> elements of <paramref name="inArray" />, starting at position <paramref name="offset" />.</returns>
	/// <param name="inArray">An array of 8-bit unsigned integers. </param>
	/// <param name="offset">An offset in <paramref name="inArray" />. </param>
	/// <param name="length">The number of elements of <paramref name="inArray" /> to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="length" /> is negative.-or- <paramref name="offset" /> plus <paramref name="length" /> is greater than the length of <paramref name="inArray" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static string ToBase64String(byte[] inArray, int offset, int length)
	{
		return ToBase64String(inArray, offset, length, Base64FormattingOptions.None);
	}

	/// <summary>Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64 digits. Parameters specify the subset as an offset in the input array, the number of elements in the array to convert, and whether to insert line breaks in the return value.</summary>
	/// <returns>The string representation in base 64 of <paramref name="length" /> elements of <paramref name="inArray" />, starting at position <paramref name="offset" />.</returns>
	/// <param name="inArray">An array of 8-bit unsigned integers. </param>
	/// <param name="offset">An offset in <paramref name="inArray" />. </param>
	/// <param name="length">The number of elements of <paramref name="inArray" /> to convert. </param>
	/// <param name="options">
	///   <see cref="F:System.Base64FormattingOptions.InsertLineBreaks" /> to insert a line break every 76 characters, or <see cref="F:System.Base64FormattingOptions.None" /> to not insert line breaks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="length" /> is negative.-or- <paramref name="offset" /> plus <paramref name="length" /> is greater than the length of <paramref name="inArray" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a valid <see cref="T:System.Base64FormattingOptions" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	[SecuritySafeCritical]
	public unsafe static string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
	{
		if (inArray == null)
		{
			throw new ArgumentNullException("inArray");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Value must be positive."));
		}
		if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
		{
			throw new ArgumentException(Environment.GetResourceString("Illegal enum value: {0}.", (int)options));
		}
		int num = inArray.Length;
		if (offset > num - length)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Offset and length must refer to a position in the string."));
		}
		if (num == 0)
		{
			return string.Empty;
		}
		bool insertLineBreaks = options == Base64FormattingOptions.InsertLineBreaks;
		string text = string.FastAllocateString(ToBase64_CalculateAndValidateOutputLength(length, insertLineBreaks));
		fixed (char* outChars = text)
		{
			fixed (byte* inData = inArray)
			{
				ConvertToBase64Array(outChars, inData, offset, length, insertLineBreaks);
				return text;
			}
		}
	}

	/// <summary>Converts a subset of an 8-bit unsigned integer array to an equivalent subset of a Unicode character array encoded with base-64 digits. Parameters specify the subsets as offsets in the input and output arrays, and the number of elements in the input array to convert.</summary>
	/// <returns>A 32-bit signed integer containing the number of bytes in <paramref name="outArray" />.</returns>
	/// <param name="inArray">An input array of 8-bit unsigned integers. </param>
	/// <param name="offsetIn">A position within <paramref name="inArray" />. </param>
	/// <param name="length">The number of elements of <paramref name="inArray" /> to convert. </param>
	/// <param name="outArray">An output array of Unicode characters. </param>
	/// <param name="offsetOut">A position within <paramref name="outArray" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> or <paramref name="outArray" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offsetIn" />, <paramref name="offsetOut" />, or <paramref name="length" /> is negative.-or- <paramref name="offsetIn" /> plus <paramref name="length" /> is greater than the length of <paramref name="inArray" />.-or- <paramref name="offsetOut" /> plus the number of elements to return is greater than the length of <paramref name="outArray" />. </exception>
	/// <filterpriority>1</filterpriority>
	public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
	{
		return ToBase64CharArray(inArray, offsetIn, length, outArray, offsetOut, Base64FormattingOptions.None);
	}

	/// <summary>Converts a subset of an 8-bit unsigned integer array to an equivalent subset of a Unicode character array encoded with base-64 digits. Parameters specify the subsets as offsets in the input and output arrays, the number of elements in the input array to convert, and whether line breaks are inserted in the output array.</summary>
	/// <returns>A 32-bit signed integer containing the number of bytes in <paramref name="outArray" />.</returns>
	/// <param name="inArray">An input array of 8-bit unsigned integers. </param>
	/// <param name="offsetIn">A position within <paramref name="inArray" />. </param>
	/// <param name="length">The number of elements of <paramref name="inArray" /> to convert. </param>
	/// <param name="outArray">An output array of Unicode characters. </param>
	/// <param name="offsetOut">A position within <paramref name="outArray" />. </param>
	/// <param name="options">
	///   <see cref="F:System.Base64FormattingOptions.InsertLineBreaks" /> to insert a line break every 76 characters, or <see cref="F:System.Base64FormattingOptions.None" /> to not insert line breaks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> or <paramref name="outArray" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offsetIn" />, <paramref name="offsetOut" />, or <paramref name="length" /> is negative.-or- <paramref name="offsetIn" /> plus <paramref name="length" /> is greater than the length of <paramref name="inArray" />.-or- <paramref name="offsetOut" /> plus the number of elements to return is greater than the length of <paramref name="outArray" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a valid <see cref="T:System.Base64FormattingOptions" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(false)]
	[SecuritySafeCritical]
	public unsafe static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, Base64FormattingOptions options)
	{
		if (inArray == null)
		{
			throw new ArgumentNullException("inArray");
		}
		if (outArray == null)
		{
			throw new ArgumentNullException("outArray");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (offsetIn < 0)
		{
			throw new ArgumentOutOfRangeException("offsetIn", Environment.GetResourceString("Value must be positive."));
		}
		if (offsetOut < 0)
		{
			throw new ArgumentOutOfRangeException("offsetOut", Environment.GetResourceString("Value must be positive."));
		}
		if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
		{
			throw new ArgumentException(Environment.GetResourceString("Illegal enum value: {0}.", (int)options));
		}
		int num = inArray.Length;
		if (offsetIn > num - length)
		{
			throw new ArgumentOutOfRangeException("offsetIn", Environment.GetResourceString("Offset and length must refer to a position in the string."));
		}
		if (num == 0)
		{
			return 0;
		}
		bool insertLineBreaks = options == Base64FormattingOptions.InsertLineBreaks;
		int num2 = outArray.Length;
		int num3 = ToBase64_CalculateAndValidateOutputLength(length, insertLineBreaks);
		if (offsetOut > num2 - num3)
		{
			throw new ArgumentOutOfRangeException("offsetOut", Environment.GetResourceString("Either offset did not refer to a position in the string, or there is an insufficient length of destination character array."));
		}
		int result;
		fixed (char* outChars = &outArray[offsetOut])
		{
			fixed (byte* inData = inArray)
			{
				result = ConvertToBase64Array(outChars, inData, offsetIn, length, insertLineBreaks);
			}
		}
		return result;
	}

	[SecurityCritical]
	private unsafe static int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)
	{
		int num = length % 3;
		int num2 = offset + (length - num);
		int num3 = 0;
		int num4 = 0;
		fixed (char* ptr = base64Table)
		{
			int i;
			for (i = offset; i < num2; i += 3)
			{
				if (insertLineBreaks)
				{
					if (num4 == 76)
					{
						outChars[num3++] = '\r';
						outChars[num3++] = '\n';
						num4 = 0;
					}
					num4 += 4;
				}
				outChars[num3] = ptr[(inData[i] & 0xFC) >> 2];
				outChars[num3 + 1] = ptr[((inData[i] & 3) << 4) | ((inData[i + 1] & 0xF0) >> 4)];
				outChars[num3 + 2] = ptr[((inData[i + 1] & 0xF) << 2) | ((inData[i + 2] & 0xC0) >> 6)];
				outChars[num3 + 3] = ptr[inData[i + 2] & 0x3F];
				num3 += 4;
			}
			i = num2;
			if (insertLineBreaks && num != 0 && num4 == 76)
			{
				outChars[num3++] = '\r';
				outChars[num3++] = '\n';
			}
			switch (num)
			{
			case 2:
				outChars[num3] = ptr[(inData[i] & 0xFC) >> 2];
				outChars[num3 + 1] = ptr[((inData[i] & 3) << 4) | ((inData[i + 1] & 0xF0) >> 4)];
				outChars[num3 + 2] = ptr[(inData[i + 1] & 0xF) << 2];
				outChars[num3 + 3] = ptr[64];
				num3 += 4;
				break;
			case 1:
				outChars[num3] = ptr[(inData[i] & 0xFC) >> 2];
				outChars[num3 + 1] = ptr[(inData[i] & 3) << 4];
				outChars[num3 + 2] = ptr[64];
				outChars[num3 + 3] = ptr[64];
				num3 += 4;
				break;
			}
		}
		return num3;
	}

	private static int ToBase64_CalculateAndValidateOutputLength(int inputLength, bool insertLineBreaks)
	{
		long num = (long)inputLength / 3L * 4;
		num += ((inputLength % 3 != 0) ? 4 : 0);
		if (num == 0L)
		{
			return 0;
		}
		if (insertLineBreaks)
		{
			long num2 = num / 76;
			if (num % 76 == 0L)
			{
				num2--;
			}
			num += num2 * 2;
		}
		if (num > int.MaxValue)
		{
			throw new OutOfMemoryException();
		}
		return (int)num;
	}

	/// <summary>Converts the specified string, which encodes binary data as base-64 digits, to an equivalent 8-bit unsigned integer array.</summary>
	/// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="s" />.</returns>
	/// <param name="s">The string to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="s" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The length of <paramref name="s" />, ignoring white-space characters, is not zero or a multiple of 4. -or-The format of <paramref name="s" /> is invalid. <paramref name="s" /> contains a non-base-64 character, more than two padding characters, or a non-white space-character among the padding characters.</exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] FromBase64String(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		fixed (char* inputPtr = s)
		{
			return FromBase64CharPtr(inputPtr, s.Length);
		}
	}

	/// <summary>Converts a subset of a Unicode character array, which encodes binary data as base-64 digits, to an equivalent 8-bit unsigned integer array. Parameters specify the subset in the input array and the number of elements to convert.</summary>
	/// <returns>An array of 8-bit unsigned integers equivalent to <paramref name="length" /> elements at position <paramref name="offset" /> in <paramref name="inArray" />.</returns>
	/// <param name="inArray">A Unicode character array. </param>
	/// <param name="offset">A position within <paramref name="inArray" />. </param>
	/// <param name="length">The number of elements in <paramref name="inArray" /> to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inArray" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> or <paramref name="length" /> is less than 0.-or- <paramref name="offset" /> plus <paramref name="length" /> indicates a position not within <paramref name="inArray" />. </exception>
	/// <exception cref="T:System.FormatException">The length of <paramref name="inArray" />, ignoring white-space characters, is not zero or a multiple of 4. -or-The format of <paramref name="inArray" /> is invalid. <paramref name="inArray" /> contains a non-base-64 character, more than two padding characters, or a non-white-space character among the padding characters. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] FromBase64CharArray(char[] inArray, int offset, int length)
	{
		if (inArray == null)
		{
			throw new ArgumentNullException("inArray");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Value must be positive."));
		}
		if (offset > inArray.Length - length)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("Offset and length must refer to a position in the string."));
		}
		fixed (char* ptr = inArray)
		{
			return FromBase64CharPtr(ptr + offset, length);
		}
	}

	[SecurityCritical]
	private unsafe static byte[] FromBase64CharPtr(char* inputPtr, int inputLength)
	{
		while (inputLength > 0)
		{
			int num = inputPtr[inputLength - 1];
			if (num != 32 && num != 10 && num != 13 && num != 9)
			{
				break;
			}
			inputLength--;
		}
		int num2 = FromBase64_ComputeResultLength(inputPtr, inputLength);
		byte[] array = new byte[num2];
		fixed (byte* startDestPtr = array)
		{
			FromBase64_Decode(inputPtr, inputLength, startDestPtr, num2);
		}
		return array;
	}

	[SecurityCritical]
	private unsafe static int FromBase64_Decode(char* startInputPtr, int inputLength, byte* startDestPtr, int destLength)
	{
		char* ptr = startInputPtr;
		byte* ptr2 = startDestPtr;
		char* ptr3 = ptr + inputLength;
		byte* ptr4 = ptr2 + destLength;
		uint num = 255u;
		while (ptr < ptr3)
		{
			uint num2 = *ptr;
			ptr++;
			if (num2 - 65 <= 25)
			{
				num2 -= 65;
			}
			else if (num2 - 97 <= 25)
			{
				num2 -= 71;
			}
			else if (num2 - 48 <= 9)
			{
				num2 -= 4294967292u;
			}
			else
			{
				if (num2 <= 32)
				{
					if (num2 - 9 <= 1 || num2 == 13 || num2 == 32)
					{
						continue;
					}
					goto IL_0097;
				}
				if (num2 != 43)
				{
					if (num2 != 47)
					{
						if (num2 != 61)
						{
							goto IL_0097;
						}
						if (ptr == ptr3)
						{
							num <<= 6;
							if ((num & 0x80000000u) == 0)
							{
								throw new FormatException(Environment.GetResourceString("Invalid length for a Base-64 char array or string."));
							}
							if ((int)(ptr4 - ptr2) < 2)
							{
								return -1;
							}
							*(ptr2++) = (byte)(num >> 16);
							*(ptr2++) = (byte)(num >> 8);
							num = 255u;
							break;
						}
						for (; ptr < ptr3 - 1; ptr++)
						{
							int num3 = *ptr;
							if (num3 != 32 && num3 != 10 && num3 != 13 && num3 != 9)
							{
								break;
							}
						}
						if (ptr == ptr3 - 1 && *ptr == '=')
						{
							num <<= 12;
							if ((num & 0x80000000u) == 0)
							{
								throw new FormatException(Environment.GetResourceString("Invalid length for a Base-64 char array or string."));
							}
							if ((int)(ptr4 - ptr2) < 1)
							{
								return -1;
							}
							*(ptr2++) = (byte)(num >> 16);
							num = 255u;
							break;
						}
						throw new FormatException(Environment.GetResourceString("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters."));
					}
					num2 = 63u;
				}
				else
				{
					num2 = 62u;
				}
			}
			num = (num << 6) | num2;
			if ((num & 0x80000000u) != 0)
			{
				if ((int)(ptr4 - ptr2) < 3)
				{
					return -1;
				}
				*ptr2 = (byte)(num >> 16);
				ptr2[1] = (byte)(num >> 8);
				ptr2[2] = (byte)num;
				ptr2 += 3;
				num = 255u;
			}
			continue;
			IL_0097:
			throw new FormatException(Environment.GetResourceString("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters."));
		}
		if (num != 255)
		{
			throw new FormatException(Environment.GetResourceString("Invalid length for a Base-64 char array or string."));
		}
		return (int)(ptr2 - startDestPtr);
	}

	[SecurityCritical]
	private unsafe static int FromBase64_ComputeResultLength(char* inputPtr, int inputLength)
	{
		char* ptr = inputPtr + inputLength;
		int num = inputLength;
		int num2 = 0;
		while (inputPtr < ptr)
		{
			uint num3 = *inputPtr;
			inputPtr++;
			switch (num3)
			{
			case 0u:
			case 1u:
			case 2u:
			case 3u:
			case 4u:
			case 5u:
			case 6u:
			case 7u:
			case 8u:
			case 9u:
			case 10u:
			case 11u:
			case 12u:
			case 13u:
			case 14u:
			case 15u:
			case 16u:
			case 17u:
			case 18u:
			case 19u:
			case 20u:
			case 21u:
			case 22u:
			case 23u:
			case 24u:
			case 25u:
			case 26u:
			case 27u:
			case 28u:
			case 29u:
			case 30u:
			case 31u:
			case 32u:
				num--;
				break;
			case 61u:
				num--;
				num2++;
				break;
			}
		}
		switch (num2)
		{
		case 1:
			num2 = 2;
			break;
		case 2:
			num2 = 1;
			break;
		default:
			throw new FormatException(Environment.GetResourceString("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters."));
		case 0:
			break;
		}
		return num / 4 * 3 + num2;
	}
}
