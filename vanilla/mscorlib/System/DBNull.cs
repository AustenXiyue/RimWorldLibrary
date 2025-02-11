using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents a nonexistent value. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class DBNull : ISerializable, IConvertible
{
	/// <summary>Represents the sole instance of the <see cref="T:System.DBNull" /> class.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly DBNull Value = new DBNull();

	private DBNull()
	{
	}

	private DBNull(SerializationInfo info, StreamingContext context)
	{
		throw new NotSupportedException(Environment.GetResourceString("Only one DBNull instance may exist, and calls to DBNull deserialization methods are not allowed."));
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.DBNull" /> object.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing information required to serialize the <see cref="T:System.DBNull" /> object. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object containing the source and destination of the serialized stream associated with the <see cref="T:System.DBNull" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		UnitySerializationHolder.GetUnitySerializationInfo(info, 2, null, null);
	}

	/// <summary>Returns an empty string (<see cref="F:System.String.Empty" />).</summary>
	/// <returns>An empty string (<see cref="F:System.String.Empty" />).</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return string.Empty;
	}

	/// <summary>Returns an empty string using the specified <see cref="T:System.IFormatProvider" />.</summary>
	/// <returns>An empty string (<see cref="F:System.String.Empty" />).</returns>
	/// <param name="provider">The <see cref="T:System.IFormatProvider" /> to be used to format the return value.-or- null to obtain the format information from the current locale setting of the operating system. </param>
	/// <filterpriority>2</filterpriority>
	public string ToString(IFormatProvider provider)
	{
		return string.Empty;
	}

	/// <summary>Gets the <see cref="T:System.TypeCode" /> value for <see cref="T:System.DBNull" />.</summary>
	/// <returns>The <see cref="T:System.TypeCode" /> value for <see cref="T:System.DBNull" />, which is <see cref="F:System.TypeCode.DBNull" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.DBNull;
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>This conversion is not supported. Attempting to make this conversion throws an <see cref="T:System.InvalidCastException" />.</summary>
	/// <returns>None. The return value for this member is not used.</returns>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(Environment.GetResourceString("Object cannot be cast from DBNull to other types."));
	}

	/// <summary>Converts the current <see cref="T:System.DBNull" /> object to the specified type.</summary>
	/// <returns>The boxed equivalent of the current <see cref="T:System.DBNull" /> object, if that conversion is supported; otherwise, an exception is thrown and no value is returned. </returns>
	/// <param name="type">The type to convert the current <see cref="T:System.DBNull" /> object to. </param>
	/// <param name="provider">An object that implements the <see cref="T:System.IFormatProvider" /> interface and is used to augment the conversion. If null is specified, format information is obtained from the current culture. </param>
	/// <exception cref="T:System.InvalidCastException">This conversion is not supported for the <see cref="T:System.DBNull" /> type.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}
}
