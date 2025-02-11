using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System;

/// <summary>
///   <see cref="T:System.RuntimeMethodHandle" /> is a handle to the internal metadata representation of a method.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public struct RuntimeMethodHandle : ISerializable
{
	private IntPtr value;

	/// <summary>Gets the value of this instance.</summary>
	/// <returns>A <see cref="T:System.RuntimeMethodHandle" /> that is the internal metadata representation of a method.</returns>
	/// <filterpriority>2</filterpriority>
	public IntPtr Value => value;

	internal RuntimeMethodHandle(IntPtr v)
	{
		value = v;
	}

	private RuntimeMethodHandle(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		MonoMethod monoMethod = (MonoMethod)info.GetValue("MethodObj", typeof(MonoMethod));
		value = monoMethod.MethodHandle.Value;
		if (value == IntPtr.Zero)
		{
			throw new SerializationException(Locale.GetText("Insufficient state."));
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data necessary to deserialize the field represented by this instance.</summary>
	/// <param name="info">The object to populate with serialization information. </param>
	/// <param name="context">(Reserved) The place to store and retrieve serialized data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">
	///   <see cref="P:System.RuntimeMethodHandle.Value" /> is invalid. </exception>
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
		info.AddValue("MethodObj", (MonoMethod)MethodBase.GetMethodFromHandle(this), typeof(MonoMethod));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr GetFunctionPointer(IntPtr m);

	/// <summary>Obtains a pointer to the method represented by this instance.</summary>
	/// <returns>A pointer to the method represented by this instance.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the necessary permission to perform this operation.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public IntPtr GetFunctionPointer()
	{
		return GetFunctionPointer(value);
	}

	/// <summary>Indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.RuntimeMethodHandle" /> and equal to the value of this instance; otherwise, false.</returns>
	/// <param name="obj">A <see cref="T:System.Object" /> to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return value == ((RuntimeMethodHandle)obj).Value;
	}

	/// <summary>Indicates whether this instance is equal to a specified <see cref="T:System.RuntimeMethodHandle" />.</summary>
	/// <returns>true if <paramref name="handle" /> is equal to the value of this instance; otherwise, false.</returns>
	/// <param name="handle">A <see cref="T:System.RuntimeMethodHandle" /> to compare to this instance.</param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public bool Equals(RuntimeMethodHandle handle)
	{
		return value == handle.Value;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	/// <summary>Indicates whether two instances of <see cref="T:System.RuntimeMethodHandle" /> are equal.</summary>
	/// <returns>true if the value of <paramref name="left" /> is equal to the value of <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">A <see cref="T:System.RuntimeMethodHandle" /> to compare to <paramref name="right" />.</param>
	/// <param name="right">A <see cref="T:System.RuntimeMethodHandle" /> to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(RuntimeMethodHandle left, RuntimeMethodHandle right)
	{
		return left.Equals(right);
	}

	/// <summary>Indicates whether two instances of <see cref="T:System.RuntimeMethodHandle" /> are not equal.</summary>
	/// <returns>true if the value of <paramref name="left" /> is unequal to the value of <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">A <see cref="T:System.RuntimeMethodHandle" /> to compare to <paramref name="right" />.</param>
	/// <param name="right">A <see cref="T:System.RuntimeMethodHandle" /> to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(RuntimeMethodHandle left, RuntimeMethodHandle right)
	{
		return !left.Equals(right);
	}

	internal static string ConstructInstantiation(RuntimeMethodInfo method, TypeNameFormatFlags format)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Type[] genericArguments = method.GetGenericArguments();
		stringBuilder.Append("[");
		for (int i = 0; i < genericArguments.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(genericArguments[i].Name);
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	internal bool IsNullHandle()
	{
		return value == IntPtr.Zero;
	}
}
