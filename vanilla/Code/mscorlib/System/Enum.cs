using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System;

/// <summary>Provides the base class for enumerations.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class Enum : ValueType, IComparable, IFormattable, IConvertible
{
	private enum ParseFailureKind
	{
		None,
		Argument,
		ArgumentNull,
		ArgumentWithParameter,
		UnhandledException
	}

	private struct EnumResult
	{
		internal object parsedEnum;

		internal bool canThrow;

		internal ParseFailureKind m_failure;

		internal string m_failureMessageID;

		internal string m_failureParameter;

		internal object m_failureMessageFormatArgument;

		internal Exception m_innerException;

		internal void Init(bool canMethodThrow)
		{
			parsedEnum = 0;
			canThrow = canMethodThrow;
		}

		internal void SetFailure(Exception unhandledException)
		{
			m_failure = ParseFailureKind.UnhandledException;
			m_innerException = unhandledException;
		}

		internal void SetFailure(ParseFailureKind failure, string failureParameter)
		{
			m_failure = failure;
			m_failureParameter = failureParameter;
			if (canThrow)
			{
				throw GetEnumParseException();
			}
		}

		internal void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument)
		{
			m_failure = failure;
			m_failureMessageID = failureMessageID;
			m_failureMessageFormatArgument = failureMessageFormatArgument;
			if (canThrow)
			{
				throw GetEnumParseException();
			}
		}

		internal Exception GetEnumParseException()
		{
			return m_failure switch
			{
				ParseFailureKind.Argument => new ArgumentException(Environment.GetResourceString(m_failureMessageID)), 
				ParseFailureKind.ArgumentNull => new ArgumentNullException(m_failureParameter), 
				ParseFailureKind.ArgumentWithParameter => new ArgumentException(Environment.GetResourceString(m_failureMessageID, m_failureMessageFormatArgument)), 
				ParseFailureKind.UnhandledException => m_innerException, 
				_ => new ArgumentException(Environment.GetResourceString("Requested value '{0}' was not found.")), 
			};
		}
	}

	private class ValuesAndNames
	{
		public ulong[] Values;

		public string[] Names;

		public ValuesAndNames(ulong[] values, string[] names)
		{
			Values = values;
			Names = names;
		}
	}

	private static readonly char[] enumSeperatorCharArray = new char[1] { ',' };

	private const string enumSeperator = ", ";

	[SecuritySafeCritical]
	private static ValuesAndNames GetCachedValuesAndNames(RuntimeType enumType, bool getNames)
	{
		ValuesAndNames valuesAndNames = enumType.GenericCache as ValuesAndNames;
		if (valuesAndNames == null || (getNames && valuesAndNames.Names == null))
		{
			ulong[] values = null;
			string[] names = null;
			if (!GetEnumValuesAndNames(enumType, out values, out names))
			{
				Array.Sort(values, names, Comparer<ulong>.Default);
			}
			valuesAndNames = (ValuesAndNames)(enumType.GenericCache = new ValuesAndNames(values, names));
		}
		return valuesAndNames;
	}

	private static string InternalFormattedHexString(object value)
	{
		return Convert.GetTypeCode(value) switch
		{
			TypeCode.SByte => ((byte)(sbyte)value).ToString("X2", null), 
			TypeCode.Byte => ((byte)value).ToString("X2", null), 
			TypeCode.Boolean => Convert.ToByte((bool)value).ToString("X2", null), 
			TypeCode.Int16 => ((ushort)(short)value).ToString("X4", null), 
			TypeCode.UInt16 => ((ushort)value).ToString("X4", null), 
			TypeCode.Char => ((ushort)(char)value).ToString("X4", null), 
			TypeCode.UInt32 => ((uint)value).ToString("X8", null), 
			TypeCode.Int32 => ((uint)(int)value).ToString("X8", null), 
			TypeCode.UInt64 => ((ulong)value).ToString("X16", null), 
			TypeCode.Int64 => ((ulong)(long)value).ToString("X16", null), 
			_ => throw new InvalidOperationException(Environment.GetResourceString("Unknown enum type.")), 
		};
	}

	private static string InternalFormat(RuntimeType eT, object value)
	{
		if (!eT.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			string name = GetName(eT, value);
			if (name == null)
			{
				return value.ToString();
			}
			return name;
		}
		return InternalFlagsFormat(eT, value);
	}

	private static string InternalFlagsFormat(RuntimeType eT, object value)
	{
		ulong num = ToUInt64(value);
		ValuesAndNames cachedValuesAndNames = GetCachedValuesAndNames(eT, getNames: true);
		string[] names = cachedValuesAndNames.Names;
		ulong[] values = cachedValuesAndNames.Values;
		int num2 = values.Length - 1;
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		ulong num3 = num;
		while (num2 >= 0 && (num2 != 0 || values[num2] != 0L))
		{
			if ((num & values[num2]) == values[num2])
			{
				num -= values[num2];
				if (!flag)
				{
					stringBuilder.Insert(0, ", ");
				}
				stringBuilder.Insert(0, names[num2]);
				flag = false;
			}
			num2--;
		}
		if (num != 0L)
		{
			return value.ToString();
		}
		if (num3 == 0L)
		{
			if (values.Length != 0 && values[0] == 0L)
			{
				return names[0];
			}
			return "0";
		}
		return stringBuilder.ToString();
	}

	internal static ulong ToUInt64(object value)
	{
		switch (Convert.GetTypeCode(value))
		{
		case TypeCode.SByte:
		case TypeCode.Int16:
		case TypeCode.Int32:
		case TypeCode.Int64:
			return (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);
		case TypeCode.Boolean:
		case TypeCode.Char:
		case TypeCode.Byte:
		case TypeCode.UInt16:
		case TypeCode.UInt32:
		case TypeCode.UInt64:
			return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
		default:
			throw new InvalidOperationException(Environment.GetResourceString("Unknown enum type."));
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern int InternalCompareTo(object o1, object o2);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	internal static extern RuntimeType InternalGetUnderlyingType(RuntimeType enumType);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool GetEnumValuesAndNames(RuntimeType enumType, out ulong[] values, out string[] names);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern object InternalBoxEnum(RuntimeType enumType, long value);

	/// <summary>Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object. The return value indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="value" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="value">The string representation of the enumeration name or underlying value to convert.</param>
	/// <param name="result">When this method returns, <paramref name="result" /> contains an object of type <paramref name="TEnum" /> whose value is represented by <paramref name="value" /> if the parse operation succeeds. If the parse operation fails, <paramref name="result" /> contains the default value of the underlying type of <paramref name="TEnum" />. Note that this value need not be a member of the <paramref name="TEnum" /> enumeration. This parameter is passed uninitialized.</param>
	/// <typeparam name="TEnum">The enumeration type to which to convert <paramref name="value" />.</typeparam>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="TEnum" /> is not an enumeration type.</exception>
	public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
	{
		return TryParse<TEnum>(value, ignoreCase: false, out result);
	}

	/// <summary>Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object. A parameter specifies whether the operation is case-sensitive. The return value indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="value" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="value">The string representation of the enumeration name or underlying value to convert.</param>
	/// <param name="ignoreCase">true to ignore case; false to consider case.</param>
	/// <param name="result">When this method returns, <paramref name="result" /> contains an object of type <paramref name="TEnum" /> whose value is represented by <paramref name="value" /> if the parse operation succeeds. If the parse operation fails, <paramref name="result" /> contains the default value of the underlying type of <paramref name="TEnum" />. Note that this value need not be a member of the <paramref name="TEnum" /> enumeration. This parameter is passed uninitialized.</param>
	/// <typeparam name="TEnum">The enumeration type to which to convert <paramref name="value" />.</typeparam>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="TEnum" /> is not an enumeration type.</exception>
	public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
	{
		result = default(TEnum);
		EnumResult parseResult = default(EnumResult);
		parseResult.Init(canMethodThrow: false);
		bool num = TryParseEnum(typeof(TEnum), value, ignoreCase, ref parseResult);
		if (num)
		{
			result = (TEnum)parseResult.parsedEnum;
		}
		return num;
	}

	/// <summary>Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.</summary>
	/// <returns>An object of type <paramref name="enumType" /> whose value is represented by <paramref name="value" />.</returns>
	/// <param name="enumType">An enumeration type. </param>
	/// <param name="value">A string containing the name or value to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> or <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />.-or- <paramref name="value" /> is either an empty string or only contains white space.-or- <paramref name="value" /> is a name, but not one of the named constants defined for the enumeration. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is outside the range of the underlying type of <paramref name="enumType" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static object Parse(Type enumType, string value)
	{
		return Parse(enumType, value, ignoreCase: false);
	}

	/// <summary>Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object. A parameter specifies whether the operation is case-insensitive.</summary>
	/// <returns>An object of type <paramref name="enumType" /> whose value is represented by <paramref name="value" />.</returns>
	/// <param name="enumType">An enumeration type. </param>
	/// <param name="value">A string containing the name or value to convert. </param>
	/// <param name="ignoreCase">true to ignore case; false to regard case. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> or <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />.-or- <paramref name="value" /> is either an empty string ("") or only contains white space.-or- <paramref name="value" /> is a name, but not one of the named constants defined for the enumeration. </exception>
	/// <exception cref="T:System.OverflowException">
	///   <paramref name="value" /> is outside the range of the underlying type of <paramref name="enumType" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static object Parse(Type enumType, string value, bool ignoreCase)
	{
		EnumResult parseResult = default(EnumResult);
		parseResult.Init(canMethodThrow: true);
		if (TryParseEnum(enumType, value, ignoreCase, ref parseResult))
		{
			return parseResult.parsedEnum;
		}
		throw parseResult.GetEnumParseException();
	}

	private static bool TryParseEnum(Type enumType, string value, bool ignoreCase, ref EnumResult parseResult)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		RuntimeType runtimeType = enumType as RuntimeType;
		if (runtimeType == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		if (value == null)
		{
			parseResult.SetFailure(ParseFailureKind.ArgumentNull, "value");
			return false;
		}
		value = value.Trim();
		if (value.Length == 0)
		{
			parseResult.SetFailure(ParseFailureKind.Argument, "Must specify valid information for parsing in the string.", null);
			return false;
		}
		ulong num = 0uL;
		if (char.IsDigit(value[0]) || value[0] == '-' || value[0] == '+')
		{
			Type underlyingType = GetUnderlyingType(enumType);
			try
			{
				object value2 = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
				parseResult.parsedEnum = ToObject(enumType, value2);
				return true;
			}
			catch (FormatException)
			{
			}
			catch (Exception failure)
			{
				if (parseResult.canThrow)
				{
					throw;
				}
				parseResult.SetFailure(failure);
				return false;
			}
		}
		string[] array = value.Split(enumSeperatorCharArray);
		ValuesAndNames cachedValuesAndNames = GetCachedValuesAndNames(runtimeType, getNames: true);
		string[] names = cachedValuesAndNames.Names;
		ulong[] values = cachedValuesAndNames.Values;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
			bool flag = false;
			for (int j = 0; j < names.Length; j++)
			{
				if (ignoreCase)
				{
					if (string.Compare(names[j], array[i], StringComparison.OrdinalIgnoreCase) != 0)
					{
						continue;
					}
				}
				else if (!names[j].Equals(array[i]))
				{
					continue;
				}
				ulong num2 = values[j];
				num |= num2;
				flag = true;
				break;
			}
			if (!flag)
			{
				parseResult.SetFailure(ParseFailureKind.ArgumentWithParameter, "Requested value '{0}' was not found.", value);
				return false;
			}
		}
		try
		{
			parseResult.parsedEnum = ToObject(enumType, num);
			return true;
		}
		catch (Exception failure2)
		{
			if (parseResult.canThrow)
			{
				throw;
			}
			parseResult.SetFailure(failure2);
			return false;
		}
	}

	/// <summary>Returns the underlying type of the specified enumeration.</summary>
	/// <returns>The underlying type of <paramref name="enumType" />.</returns>
	/// <param name="enumType">The enumeration whose underlying type will be retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static Type GetUnderlyingType(Type enumType)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		return enumType.GetEnumUnderlyingType();
	}

	/// <summary>Retrieves an array of the values of the constants in a specified enumeration.</summary>
	/// <returns>An array that contains the values of the constants in <paramref name="enumType" />. </returns>
	/// <param name="enumType">An enumeration type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">The method is invoked by reflection in a reflection-only context, -or-<paramref name="enumType" /> is a type from an assembly loaded in a reflection-only context.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static Array GetValues(Type enumType)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		return enumType.GetEnumValues();
	}

	internal static ulong[] InternalGetValues(RuntimeType enumType)
	{
		return GetCachedValuesAndNames(enumType, getNames: false).Values;
	}

	/// <summary>Retrieves the name of the constant in the specified enumeration that has the specified value.</summary>
	/// <returns>A string containing the name of the enumerated constant in <paramref name="enumType" /> whose value is <paramref name="value" />; or null if no such constant is found.</returns>
	/// <param name="enumType">An enumeration type. </param>
	/// <param name="value">The value of a particular enumerated constant in terms of its underlying type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> or <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />.-or- <paramref name="value" /> is neither of type <paramref name="enumType" /> nor does it have the same underlying type as <paramref name="enumType" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static string GetName(Type enumType, object value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		return enumType.GetEnumName(value);
	}

	/// <summary>Retrieves an array of the names of the constants in a specified enumeration.</summary>
	/// <returns>A string array of the names of the constants in <paramref name="enumType" />. </returns>
	/// <param name="enumType">An enumeration type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> parameter is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static string[] GetNames(Type enumType)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		return enumType.GetEnumNames();
	}

	internal static string[] InternalGetNames(RuntimeType enumType)
	{
		return GetCachedValuesAndNames(enumType, getNames: true).Names;
	}

	/// <summary>Converts the specified object with an integer value to an enumeration member.</summary>
	/// <returns>An enumeration object whose value is <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> or <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />.-or- <paramref name="value" /> is not type <see cref="T:System.SByte" />, <see cref="T:System.Int16" />, <see cref="T:System.Int32" />, <see cref="T:System.Int64" />, <see cref="T:System.Byte" />, <see cref="T:System.UInt16" />, <see cref="T:System.UInt32" />, or <see cref="T:System.UInt64" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static object ToObject(Type enumType, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		TypeCode typeCode = Convert.GetTypeCode(value);
		if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8 && (typeCode == TypeCode.Boolean || typeCode == TypeCode.Char))
		{
			throw new ArgumentException(Environment.GetResourceString("The value passed in must be an enum base or an underlying type for an enum, such as an Int32."), "value");
		}
		return typeCode switch
		{
			TypeCode.Int32 => ToObject(enumType, (int)value), 
			TypeCode.SByte => ToObject(enumType, (sbyte)value), 
			TypeCode.Int16 => ToObject(enumType, (short)value), 
			TypeCode.Int64 => ToObject(enumType, (long)value), 
			TypeCode.UInt32 => ToObject(enumType, (uint)value), 
			TypeCode.Byte => ToObject(enumType, (byte)value), 
			TypeCode.UInt16 => ToObject(enumType, (ushort)value), 
			TypeCode.UInt64 => ToObject(enumType, (ulong)value), 
			TypeCode.Char => ToObject(enumType, (char)value), 
			TypeCode.Boolean => ToObject(enumType, (bool)value), 
			_ => throw new ArgumentException(Environment.GetResourceString("The value passed in must be an enum base or an underlying type for an enum, such as an Int32."), "value"), 
		};
	}

	/// <summary>Returns an indication whether a constant with a specified value exists in a specified enumeration.</summary>
	/// <returns>true if a constant in <paramref name="enumType" /> has a value equal to <paramref name="value" />; otherwise, false.</returns>
	/// <param name="enumType">An enumeration type. </param>
	/// <param name="value">The value or name of a constant in <paramref name="enumType" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> or <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an Enum.-or- The type of <paramref name="value" /> is an enumeration, but it is not an enumeration of type <paramref name="enumType" />.-or- The type of <paramref name="value" /> is not an underlying type of <paramref name="enumType" />. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> is not type <see cref="T:System.SByte" />, <see cref="T:System.Int16" />, <see cref="T:System.Int32" />, <see cref="T:System.Int64" />, <see cref="T:System.Byte" />, <see cref="T:System.UInt16" />, <see cref="T:System.UInt32" />, or <see cref="T:System.UInt64" />, or <see cref="T:System.String" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static bool IsDefined(Type enumType, object value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		return enumType.IsEnumDefined(value);
	}

	/// <summary>Converts the specified value of a specified enumerated type to its equivalent string representation according to the specified format.</summary>
	/// <returns>A string representation of <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type of the value to convert. </param>
	/// <param name="value">The value to convert. </param>
	/// <param name="format">The output format to use. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="enumType" />, <paramref name="value" />, or <paramref name="format" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="enumType" /> parameter is not an <see cref="T:System.Enum" /> type.-or- The <paramref name="value" /> is from an enumeration that differs in type from <paramref name="enumType" />.-or- The type of <paramref name="value" /> is not an underlying type of <paramref name="enumType" />. </exception>
	/// <exception cref="T:System.FormatException">The <paramref name="format" /> parameter contains an invalid value. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="format" /> equals "X", but the enumeration type is unknown.</exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	public static string Format(Type enumType, object value, string format)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		RuntimeType runtimeType = enumType as RuntimeType;
		if (runtimeType == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		Type type = value.GetType();
		Type underlyingType = GetUnderlyingType(enumType);
		if (type.IsEnum)
		{
			Type underlyingType2 = GetUnderlyingType(type);
			if (!type.IsEquivalentTo(enumType))
			{
				throw new ArgumentException(Environment.GetResourceString("Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.", type.ToString(), enumType.ToString()));
			}
			type = underlyingType2;
			value = ((Enum)value).GetValue();
		}
		else if (type != underlyingType)
		{
			throw new ArgumentException(Environment.GetResourceString("Enum underlying type and the object must be same type or object. Type passed in was '{0}'; the enum underlying type was '{1}'.", type.ToString(), underlyingType.ToString()));
		}
		if (format.Length != 1)
		{
			throw new FormatException(Environment.GetResourceString("Format String can be only \"G\", \"g\", \"X\", \"x\", \"F\", \"f\", \"D\" or \"d\"."));
		}
		switch (format[0])
		{
		case 'D':
		case 'd':
			return value.ToString();
		case 'X':
		case 'x':
			return InternalFormattedHexString(value);
		case 'G':
		case 'g':
			return InternalFormat(runtimeType, value);
		case 'F':
		case 'f':
			return InternalFlagsFormat(runtimeType, value);
		default:
			throw new FormatException(Environment.GetResourceString("Format String can be only \"G\", \"g\", \"X\", \"x\", \"F\", \"f\", \"D\" or \"d\"."));
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern object get_value();

	[SecuritySafeCritical]
	internal object GetValue()
	{
		return get_value();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern bool InternalHasFlag(Enum flags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	private extern int get_hashcode();

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is an enumeration value of the same type and with the same underlying value as this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		return ValueType.DefaultEquals(this, obj);
	}

	/// <summary>Returns the hash code for the value of this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public override int GetHashCode()
	{
		return get_hashcode();
	}

	/// <summary>Converts the value of this instance to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return InternalFormat((RuntimeType)GetType(), GetValue());
	}

	/// <summary>This method overload is obsolete; use <see cref="M:System.Enum.ToString(System.String)" />.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
	/// <param name="format">A format specification. </param>
	/// <param name="provider">(Obsolete.)</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> does not contain a valid format specification. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="format" /> equals "X", but the enumeration type is unknown.</exception>
	/// <filterpriority>2</filterpriority>
	[Obsolete("The provider argument is not used. Please use ToString(String).")]
	public string ToString(string format, IFormatProvider provider)
	{
		return ToString(format);
	}

	/// <summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed number that indicates the relative values of this instance and <paramref name="target" />.Value Meaning Less than zero The value of this instance is less than the value of <paramref name="target" />. Zero The value of this instance is equal to the value of <paramref name="target" />. Greater than zero The value of this instance is greater than the value of <paramref name="target" />.-or- <paramref name="target" /> is null. </returns>
	/// <param name="target">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> and this instance are not the same type. </exception>
	/// <exception cref="T:System.InvalidOperationException">This instance is not type <see cref="T:System.SByte" />, <see cref="T:System.Int16" />, <see cref="T:System.Int32" />, <see cref="T:System.Int64" />, <see cref="T:System.Byte" />, <see cref="T:System.UInt16" />, <see cref="T:System.UInt32" />, or <see cref="T:System.UInt64" />. </exception>
	/// <exception cref="T:System.NullReferenceException">This instance is null.</exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public int CompareTo(object target)
	{
		if (this == null)
		{
			throw new NullReferenceException();
		}
		int num = InternalCompareTo(this, target);
		if (num < 2)
		{
			return num;
		}
		if (num == 2)
		{
			Type type = GetType();
			Type type2 = target.GetType();
			throw new ArgumentException(Environment.GetResourceString("Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.", type2.ToString(), type.ToString()));
		}
		throw new InvalidOperationException(Environment.GetResourceString("Unknown enum type."));
	}

	/// <summary>Converts the value of this instance to its equivalent string representation using the specified format.</summary>
	/// <returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
	/// <param name="format">A format string. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> contains an invalid specification. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="format" /> equals "X", but the enumeration type is unknown.</exception>
	/// <filterpriority>2</filterpriority>
	public string ToString(string format)
	{
		if (format == null || format.Length == 0)
		{
			format = "G";
		}
		if (string.Compare(format, "G", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return ToString();
		}
		if (string.Compare(format, "D", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return GetValue().ToString();
		}
		if (string.Compare(format, "X", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return InternalFormattedHexString(GetValue());
		}
		if (string.Compare(format, "F", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return InternalFlagsFormat((RuntimeType)GetType(), GetValue());
		}
		throw new FormatException(Environment.GetResourceString("Format String can be only \"G\", \"g\", \"X\", \"x\", \"F\", \"f\", \"D\" or \"d\"."));
	}

	/// <summary>This method overload is obsolete; use <see cref="M:System.Enum.ToString" />.</summary>
	/// <returns>The string representation of the value of this instance.</returns>
	/// <param name="provider">(obsolete) </param>
	/// <filterpriority>2</filterpriority>
	[Obsolete("The provider argument is not used. Please use ToString().")]
	public string ToString(IFormatProvider provider)
	{
		return ToString();
	}

	/// <summary>Determines whether one or more bit fields are set in the current instance.</summary>
	/// <returns>true if the bit field or bit fields that are set in <paramref name="flag" /> are also set in the current instance; otherwise, false.</returns>
	/// <param name="flag">An enumeration value.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="flag" /> is a different type than the current instance.</exception>
	[SecuritySafeCritical]
	public bool HasFlag(Enum flag)
	{
		if (flag == null)
		{
			throw new ArgumentNullException("flag");
		}
		if (!GetType().IsEquivalentTo(flag.GetType()))
		{
			throw new ArgumentException(Environment.GetResourceString("The argument type, '{0}', is not the same as the enum type '{1}'.", flag.GetType(), GetType()));
		}
		return InternalHasFlag(flag);
	}

	/// <summary>Returns the underlying <see cref="T:System.TypeCode" /> for this instance.</summary>
	/// <returns>The type for this instance.</returns>
	/// <exception cref="T:System.InvalidOperationException">The enumeration type is unknown.</exception>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		Type underlyingType = GetUnderlyingType(GetType());
		if (underlyingType == typeof(int))
		{
			return TypeCode.Int32;
		}
		if (underlyingType == typeof(sbyte))
		{
			return TypeCode.SByte;
		}
		if (underlyingType == typeof(short))
		{
			return TypeCode.Int16;
		}
		if (underlyingType == typeof(long))
		{
			return TypeCode.Int64;
		}
		if (underlyingType == typeof(uint))
		{
			return TypeCode.UInt32;
		}
		if (underlyingType == typeof(byte))
		{
			return TypeCode.Byte;
		}
		if (underlyingType == typeof(ushort))
		{
			return TypeCode.UInt16;
		}
		if (underlyingType == typeof(ulong))
		{
			return TypeCode.UInt64;
		}
		if (underlyingType == typeof(bool))
		{
			return TypeCode.Boolean;
		}
		if (underlyingType == typeof(char))
		{
			return TypeCode.Char;
		}
		throw new InvalidOperationException(Environment.GetResourceString("Unknown enum type."));
	}

	/// <summary>Converts the current value to a Boolean value based on the underlying type.</summary>
	/// <returns>This member always throws an exception.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a Unicode character based on the underlying type.</summary>
	/// <returns>This member always throws an exception.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		return Convert.ToChar(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to an 8-bit signed integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to an 8-bit unsigned integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a 16-bit signed integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a 16-bit unsigned integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a 32-bit signed integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a 32-bit unsigned integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a 64-bit signed integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a 64-bit unsigned integer based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a single-precision floating-point number based on the underlying type.</summary>
	/// <returns>This member always throws an exception.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return Convert.ToSingle(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a double-precision floating point number based on the underlying type.</summary>
	/// <returns>This member always throws an exception.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a <see cref="T:System.Decimal" /> based on the underlying type.</summary>
	/// <returns>This member always throws an exception.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(GetValue(), CultureInfo.CurrentCulture);
	}

	/// <summary>Converts the current value to a <see cref="T:System.DateTime" /> based on the underlying type.</summary>
	/// <returns>This member always throws an exception.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.InvalidCastException">In all cases. </exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Invalid cast from '{0}' to '{1}'.", "Enum", "DateTime"));
	}

	/// <summary>Converts the current value to a specified type based on the underlying type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="type">The type to convert to. </param>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}

	/// <summary>Converts the specified 8-bit signed integer value to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(true)]
	[SecuritySafeCritical]
	public static object ToObject(Type enumType, sbyte value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 16-bit signed integer to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	[SecuritySafeCritical]
	public static object ToObject(Type enumType, short value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 32-bit signed integer to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	[SecuritySafeCritical]
	public static object ToObject(Type enumType, int value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 8-bit unsigned integer to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[ComVisible(true)]
	public static object ToObject(Type enumType, byte value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 16-bit unsigned integer value to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(true)]
	[SecuritySafeCritical]
	public static object ToObject(Type enumType, ushort value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 32-bit unsigned integer value to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	[ComVisible(true)]
	[SecuritySafeCritical]
	public static object ToObject(Type enumType, uint value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 64-bit signed integer to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	[SecuritySafeCritical]
	public static object ToObject(Type enumType, long value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	/// <summary>Converts the specified 64-bit unsigned integer value to an enumeration member.</summary>
	/// <returns>An instance of the enumeration set to <paramref name="value" />.</returns>
	/// <param name="enumType">The enumeration type to return. </param>
	/// <param name="value">The value to convert to an enumeration member. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="enumType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enumType" /> is not an <see cref="T:System.Enum" />. </exception>
	/// <filterpriority>1</filterpriority>
	[ComVisible(true)]
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public static object ToObject(Type enumType, ulong value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, (long)value);
	}

	[SecuritySafeCritical]
	private static object ToObject(Type enumType, char value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value);
	}

	[SecuritySafeCritical]
	private static object ToObject(Type enumType, bool value)
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException(Environment.GetResourceString("Type provided must be an Enum."), "enumType");
		}
		RuntimeType obj = enumType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "enumType");
		}
		return InternalBoxEnum(obj, value ? 1 : 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Enum" /> class. </summary>
	protected Enum()
	{
	}
}
