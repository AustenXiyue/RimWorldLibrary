using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents a field using an internal metadata token.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public struct RuntimeFieldHandle : ISerializable
{
	private IntPtr value;

	/// <summary>Gets a handle to the field represented by the current instance.</summary>
	/// <returns>An <see cref="T:System.IntPtr" /> that contains the handle to the field represented by the current instance.</returns>
	/// <filterpriority>2</filterpriority>
	public IntPtr Value => value;

	internal RuntimeFieldHandle(IntPtr v)
	{
		value = v;
	}

	private RuntimeFieldHandle(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		MonoField monoField = (MonoField)info.GetValue("FieldObj", typeof(MonoField));
		value = monoField.FieldHandle.Value;
		if (value == IntPtr.Zero)
		{
			throw new SerializationException(Locale.GetText("Insufficient state."));
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data necessary to deserialize the field represented by the current instance.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object to populate with serialization information. </param>
	/// <param name="context">(Reserved) The place to store and retrieve serialized data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="P:System.RuntimeFieldHandle.Value" /> property of the current instance is not a valid handle. </exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		if (value == IntPtr.Zero)
		{
			throw new SerializationException("Object fields may not be properly initialized");
		}
		info.AddValue("FieldObj", (MonoField)FieldInfo.GetFieldFromHandle(this), typeof(MonoField));
	}

	/// <summary>Indicates whether the current instance is equal to the specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.RuntimeFieldHandle" /> and equal to the value of the current instance; otherwise, false.</returns>
	/// <param name="obj">The object to compare to the current instance.</param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return value == ((RuntimeFieldHandle)obj).Value;
	}

	/// <summary>Indicates whether the current instance is equal to the specified <see cref="T:System.RuntimeFieldHandle" />.</summary>
	/// <returns>true if the value of <paramref name="handle" /> is equal to the value of the current instance; otherwise, false.</returns>
	/// <param name="handle">The <see cref="T:System.RuntimeFieldHandle" /> to compare to the current instance.</param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public bool Equals(RuntimeFieldHandle handle)
	{
		return value == handle.Value;
	}

	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	/// <summary>Indicates whether two <see cref="T:System.RuntimeFieldHandle" /> structures are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.RuntimeFieldHandle" /> to compare to <paramref name="right" />.</param>
	/// <param name="right">The <see cref="T:System.RuntimeFieldHandle" /> to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(RuntimeFieldHandle left, RuntimeFieldHandle right)
	{
		return left.Equals(right);
	}

	/// <summary>Indicates whether two <see cref="T:System.RuntimeFieldHandle" /> structures are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.RuntimeFieldHandle" /> to compare to <paramref name="right" />.</param>
	/// <param name="right">The <see cref="T:System.RuntimeFieldHandle" /> to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(RuntimeFieldHandle left, RuntimeFieldHandle right)
	{
		return !left.Equals(right);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetValueInternal(FieldInfo fi, object obj, object value);

	internal static void SetValue(RtFieldInfo field, object obj, object value, RuntimeType fieldType, FieldAttributes fieldAttr, RuntimeType declaringType, ref bool domainInitialized)
	{
		SetValueInternal(field, obj, value);
	}

	internal unsafe static object GetValueDirect(RtFieldInfo field, RuntimeType fieldType, void* pTypedRef, RuntimeType contextType)
	{
		throw new NotImplementedException("GetValueDirect");
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal unsafe static extern void SetValueDirect(RtFieldInfo field, RuntimeType fieldType, void* pTypedRef, object value, RuntimeType contextType);
}
