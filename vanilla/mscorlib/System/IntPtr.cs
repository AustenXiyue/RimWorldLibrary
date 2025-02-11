using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>A platform-specific type that is used to represent a pointer or a handle.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public struct IntPtr : ISerializable
{
	private unsafe void* m_value;

	/// <summary>A read-only field that represents a pointer or handle that has been initialized to zero.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly IntPtr Zero;

	/// <summary>Gets the size of this instance.</summary>
	/// <returns>The size of a pointer or handle in this process, measured in bytes. The value of this property is 4 in a 32-bit process, and 8 in a 64-bit process. You can define the process type by setting the /platform switch when you compile your code with the C# and Visual Basic compilers.</returns>
	/// <filterpriority>1</filterpriority>
	public unsafe static int Size
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return sizeof(void*);
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.IntPtr" /> using the specified 32-bit pointer or handle.</summary>
	/// <param name="value">A pointer or handle contained in a 32-bit signed integer. </param>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public unsafe IntPtr(int value)
	{
		m_value = (void*)value;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.IntPtr" /> using the specified 64-bit pointer.</summary>
	/// <param name="value">A pointer or handle contained in a 64-bit signed integer. </param>
	/// <exception cref="T:System.OverflowException">On a 32-bit platform, <paramref name="value" /> is too large or too small to represent as an <see cref="T:System.IntPtr" />. </exception>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public unsafe IntPtr(long value)
	{
		m_value = (void*)value;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.IntPtr" /> using the specified pointer to an unspecified type.</summary>
	/// <param name="value">A pointer to an unspecified type. </param>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	[CLSCompliant(false)]
	public unsafe IntPtr(void* value)
	{
		m_value = value;
	}

	private unsafe IntPtr(SerializationInfo info, StreamingContext context)
	{
		long @int = info.GetInt64("value");
		m_value = (void*)@int;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data needed to serialize the current <see cref="T:System.IntPtr" /> object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object to populate with data. </param>
	/// <param name="context">The destination for this serialization. (This parameter is not used; specify null.)</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("value", ToInt64());
	}

	/// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of <see cref="T:System.IntPtr" /> and equals the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance or null. </param>
	/// <filterpriority>2</filterpriority>
	public unsafe override bool Equals(object obj)
	{
		if (!(obj is IntPtr))
		{
			return false;
		}
		return ((IntPtr)obj).m_value == m_value;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public unsafe override int GetHashCode()
	{
		return (int)m_value;
	}

	/// <summary>Converts the value of this instance to a 32-bit signed integer.</summary>
	/// <returns>A 32-bit signed integer equal to the value of this instance.</returns>
	/// <exception cref="T:System.OverflowException">On a 64-bit platform, the value of this instance is too large or too small to represent as a 32-bit signed integer. </exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public unsafe int ToInt32()
	{
		return (int)m_value;
	}

	/// <summary>Converts the value of this instance to a 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer equal to the value of this instance.</returns>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public unsafe long ToInt64()
	{
		if (Size == 4)
		{
			return (int)m_value;
		}
		return (long)m_value;
	}

	/// <summary>Converts the value of this instance to a pointer to an unspecified type.</summary>
	/// <returns>A pointer to <see cref="T:System.Void" />; that is, a pointer to memory containing data of an unspecified type.</returns>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public unsafe void* ToPointer()
	{
		return m_value;
	}

	/// <summary>Converts the numeric value of the current <see cref="T:System.IntPtr" /> object to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of this instance.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return ToString(null);
	}

	/// <summary>Converts the numeric value of the current <see cref="T:System.IntPtr" /> object to its equivalent string representation.</summary>
	/// <returns>The string representation of the value of the current <see cref="T:System.IntPtr" /> object.</returns>
	/// <param name="format">A format specification that governs how the current <see cref="T:System.IntPtr" /> object is converted. </param>
	/// <filterpriority>1</filterpriority>
	public unsafe string ToString(string format)
	{
		if (Size == 4)
		{
			return ((int)m_value).ToString(format, null);
		}
		return ((long)m_value).ToString(format, null);
	}

	/// <summary>Determines whether two specified instances of <see cref="T:System.IntPtr" /> are equal.</summary>
	/// <returns>true if <paramref name="value1" /> equals <paramref name="value2" />; otherwise, false.</returns>
	/// <param name="value1">The first pointer or handle to compare.</param>
	/// <param name="value2">The second pointer or handle to compare.</param>
	/// <filterpriority>3</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public unsafe static bool operator ==(IntPtr value1, IntPtr value2)
	{
		return value1.m_value == value2.m_value;
	}

	/// <summary>Determines whether two specified instances of <see cref="T:System.IntPtr" /> are not equal.</summary>
	/// <returns>true if <paramref name="value1" /> does not equal <paramref name="value2" />; otherwise, false.</returns>
	/// <param name="value1">The first pointer or handle to compare. </param>
	/// <param name="value2">The second pointer or handle to compare. </param>
	/// <filterpriority>3</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public unsafe static bool operator !=(IntPtr value1, IntPtr value2)
	{
		return value1.m_value != value2.m_value;
	}

	/// <summary>Converts the value of a 32-bit signed integer to an <see cref="T:System.IntPtr" />.</summary>
	/// <returns>A new instance of <see cref="T:System.IntPtr" /> initialized to <paramref name="value" />.</returns>
	/// <param name="value">A 32-bit signed integer. </param>
	/// <filterpriority>3</filterpriority>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public static explicit operator IntPtr(int value)
	{
		return new IntPtr(value);
	}

	/// <summary>Converts the value of a 64-bit signed integer to an <see cref="T:System.IntPtr" />.</summary>
	/// <returns>A new instance of <see cref="T:System.IntPtr" /> initialized to <paramref name="value" />.</returns>
	/// <param name="value">A 64-bit signed integer. </param>
	/// <exception cref="T:System.OverflowException">On a 32-bit platform, <paramref name="value" /> is too large to represent as an <see cref="T:System.IntPtr" />. </exception>
	/// <filterpriority>3</filterpriority>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public static explicit operator IntPtr(long value)
	{
		return new IntPtr(value);
	}

	/// <summary>Converts the specified pointer to an unspecified type to an <see cref="T:System.IntPtr" />.</summary>
	/// <returns>A new instance of <see cref="T:System.IntPtr" /> initialized to <paramref name="value" />.</returns>
	/// <param name="value">A pointer to an unspecified type. </param>
	/// <filterpriority>3</filterpriority>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	[CLSCompliant(false)]
	public unsafe static explicit operator IntPtr(void* value)
	{
		return new IntPtr(value);
	}

	/// <summary>Converts the value of the specified <see cref="T:System.IntPtr" /> to a 32-bit signed integer.</summary>
	/// <returns>The contents of <paramref name="value" />.</returns>
	/// <param name="value">The pointer or handle to convert.</param>
	/// <exception cref="T:System.OverflowException">On a 64-bit platform, the value of <paramref name="value" /> is too large to represent as a 32-bit signed integer. </exception>
	/// <filterpriority>3</filterpriority>
	public unsafe static explicit operator int(IntPtr value)
	{
		return (int)value.m_value;
	}

	/// <summary>Converts the value of the specified <see cref="T:System.IntPtr" /> to a 64-bit signed integer.</summary>
	/// <returns>The contents of <paramref name="value" />.</returns>
	/// <param name="value">The pointer or handle to convert.</param>
	/// <filterpriority>3</filterpriority>
	public static explicit operator long(IntPtr value)
	{
		return value.ToInt64();
	}

	/// <summary>Converts the value of the specified <see cref="T:System.IntPtr" /> to a pointer to an unspecified type.</summary>
	/// <returns>The contents of <paramref name="value" />.</returns>
	/// <param name="value">The pointer or handle to convert. </param>
	/// <filterpriority>3</filterpriority>
	[CLSCompliant(false)]
	public unsafe static explicit operator void*(IntPtr value)
	{
		return value.m_value;
	}

	/// <summary>Adds an offset to the value of a pointer.</summary>
	/// <returns>A new pointer that reflects the addition of <paramref name="offset" /> to <paramref name="pointer" />.</returns>
	/// <param name="pointer">The pointer to add the offset to.</param>
	/// <param name="offset">The offset to add.</param>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public unsafe static IntPtr Add(IntPtr pointer, int offset)
	{
		return (IntPtr)((byte*)(void*)pointer + offset);
	}

	/// <summary>Subtracts an offset from the value of a pointer.</summary>
	/// <returns>A new pointer that reflects the subtraction of <paramref name="offset" /> from <paramref name="pointer" />.</returns>
	/// <param name="pointer">The pointer to subtract the offset from.</param>
	/// <param name="offset">The offset to subtract.</param>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public unsafe static IntPtr Subtract(IntPtr pointer, int offset)
	{
		return (IntPtr)((byte*)(void*)pointer - offset);
	}

	/// <summary>Adds an offset to the value of a pointer.</summary>
	/// <returns>A new pointer that reflects the addition of <paramref name="offset" /> to <paramref name="pointer" />.</returns>
	/// <param name="pointer">The pointer to add the offset to.</param>
	/// <param name="offset">The offset to add.</param>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public unsafe static IntPtr operator +(IntPtr pointer, int offset)
	{
		return (IntPtr)((byte*)(void*)pointer + offset);
	}

	/// <summary>Subtracts an offset from the value of a pointer.</summary>
	/// <returns>A new pointer that reflects the subtraction of <paramref name="offset" /> from <paramref name="pointer" />.</returns>
	/// <param name="pointer">The pointer to subtract the offset from.</param>
	/// <param name="offset">The offset to subtract.</param>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public unsafe static IntPtr operator -(IntPtr pointer, int offset)
	{
		return (IntPtr)((byte*)(void*)pointer - offset);
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal unsafe bool IsNull()
	{
		return m_value == null;
	}
}
