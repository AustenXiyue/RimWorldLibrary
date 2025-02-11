using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents a type using an internal metadata token.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public struct RuntimeTypeHandle : ISerializable
{
	private IntPtr value;

	/// <summary>Gets a handle to the type represented by this instance.</summary>
	/// <returns>A handle to the type represented by this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public IntPtr Value => value;

	internal RuntimeTypeHandle(IntPtr val)
	{
		value = val;
	}

	internal RuntimeTypeHandle(RuntimeType type)
		: this(type._impl.value)
	{
	}

	private RuntimeTypeHandle(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		RuntimeType runtimeType = (RuntimeType)info.GetValue("TypeObj", typeof(RuntimeType));
		value = runtimeType.TypeHandle.Value;
		if (value == IntPtr.Zero)
		{
			throw new SerializationException(Locale.GetText("Insufficient state."));
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data necessary to deserialize the type represented by the current instance.</summary>
	/// <param name="info">The object to be populated with serialization information. </param>
	/// <param name="context">(Reserved) The location where serialized data will be stored and retrieved. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">
	///   <see cref="P:System.RuntimeTypeHandle.Value" /> is invalid. </exception>
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
		info.AddValue("TypeObj", Type.GetTypeHandle(this), typeof(RuntimeType));
	}

	/// <summary>Indicates whether the specified object is equal to the current <see cref="T:System.RuntimeTypeHandle" /> structure.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.RuntimeTypeHandle" /> structure and is equal to the value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare to the current instance.</param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return value == ((RuntimeTypeHandle)obj).Value;
	}

	/// <summary>Indicates whether the specified <see cref="T:System.RuntimeTypeHandle" /> structure is equal to the current <see cref="T:System.RuntimeTypeHandle" /> structure.</summary>
	/// <returns>true if the value of <paramref name="handle" /> is equal to the value of this instance; otherwise, false.</returns>
	/// <param name="handle">The <see cref="T:System.RuntimeTypeHandle" /> structure to compare to the current instance.</param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public bool Equals(RuntimeTypeHandle handle)
	{
		return value == handle.Value;
	}

	/// <summary>Returns the hash code for the current instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	/// <summary>Indicates whether a <see cref="T:System.RuntimeTypeHandle" /> structure is equal to an object.</summary>
	/// <returns>true if <paramref name="right" /> is a <see cref="T:System.RuntimeTypeHandle" /> and is equal to <paramref name="left" />; otherwise, false.</returns>
	/// <param name="left">A <see cref="T:System.RuntimeTypeHandle" /> structure to compare to <paramref name="right" />.</param>
	/// <param name="right">An object to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(RuntimeTypeHandle left, object right)
	{
		if (right != null && right is RuntimeTypeHandle)
		{
			return left.Equals((RuntimeTypeHandle)right);
		}
		return false;
	}

	/// <summary>Indicates whether a <see cref="T:System.RuntimeTypeHandle" /> structure is not equal to an object.</summary>
	/// <returns>true if <paramref name="right" /> is a <see cref="T:System.RuntimeTypeHandle" /> structure and is not equal to <paramref name="left" />; otherwise, false.</returns>
	/// <param name="left">A <see cref="T:System.RuntimeTypeHandle" /> structure to compare to <paramref name="right" />.</param>
	/// <param name="right">An object to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(RuntimeTypeHandle left, object right)
	{
		if (right != null && right is RuntimeTypeHandle)
		{
			return !left.Equals((RuntimeTypeHandle)right);
		}
		return true;
	}

	/// <summary>Indicates whether an object and a <see cref="T:System.RuntimeTypeHandle" /> structure are equal.</summary>
	/// <returns>true if <paramref name="left" /> is a <see cref="T:System.RuntimeTypeHandle" /> structure and is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">An object to compare to <paramref name="right" />.</param>
	/// <param name="right">A <see cref="T:System.RuntimeTypeHandle" /> structure to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(object left, RuntimeTypeHandle right)
	{
		if (left != null && left is RuntimeTypeHandle runtimeTypeHandle)
		{
			return runtimeTypeHandle.Equals(right);
		}
		return false;
	}

	/// <summary>Indicates whether an object and a <see cref="T:System.RuntimeTypeHandle" /> structure are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is a <see cref="T:System.RuntimeTypeHandle" /> and is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">An object to compare to <paramref name="right" />.</param>
	/// <param name="right">A <see cref="T:System.RuntimeTypeHandle" /> structure to compare to <paramref name="left" />.</param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(object left, RuntimeTypeHandle right)
	{
		if (left != null && left is RuntimeTypeHandle runtimeTypeHandle)
		{
			return !runtimeTypeHandle.Equals(right);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern TypeAttributes GetAttributes(RuntimeType type);

	/// <summary>Gets a handle to the module that contains the type represented by the current instance.</summary>
	/// <returns>A <see cref="T:System.ModuleHandle" /> structure representing a handle to the module that contains the type represented by the current instance.</returns>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public ModuleHandle GetModuleHandle()
	{
		if (value == IntPtr.Zero)
		{
			throw new InvalidOperationException("Object fields may not be properly initialized");
		}
		return Type.GetTypeFromHandle(this).Module.ModuleHandle;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int GetMetadataToken(RuntimeType type);

	internal static int GetToken(RuntimeType type)
	{
		return GetMetadataToken(type);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern Type GetGenericTypeDefinition_impl(RuntimeType type);

	internal static Type GetGenericTypeDefinition(RuntimeType type)
	{
		return GetGenericTypeDefinition_impl(type);
	}

	internal static bool HasElementType(RuntimeType type)
	{
		if (!IsArray(type) && !IsByRef(type))
		{
			return IsPointer(type);
		}
		return true;
	}

	internal static bool HasProxyAttribute(RuntimeType type)
	{
		throw new NotImplementedException("HasProxyAttribute");
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool HasInstantiation(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsArray(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsByRef(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsComObject(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsInstanceOfType(RuntimeType type, object o);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsPointer(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsPrimitive(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool HasReferences(RuntimeType type);

	internal static bool IsComObject(RuntimeType type, bool isGenericCOM)
	{
		if (!isGenericCOM)
		{
			return IsComObject(type);
		}
		return false;
	}

	internal static bool IsContextful(RuntimeType type)
	{
		return typeof(ContextBoundObject).IsAssignableFrom(type);
	}

	internal static bool IsEquivalentTo(RuntimeType rtType1, RuntimeType rtType2)
	{
		return false;
	}

	internal static bool IsSzArray(RuntimeType type)
	{
		if (IsArray(type))
		{
			return type.GetArrayRank() == 1;
		}
		return false;
	}

	internal static bool IsInterface(RuntimeType type)
	{
		return (type.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern int GetArrayRank(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern RuntimeAssembly GetAssembly(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern RuntimeType GetElementType(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern RuntimeModule GetModule(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsGenericVariable(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern RuntimeType GetBaseType(RuntimeType type);

	internal static bool CanCastTo(RuntimeType type, RuntimeType target)
	{
		return type_is_assignable_from(target, type);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool type_is_assignable_from(Type a, Type b);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsGenericTypeDefinition(RuntimeType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr GetGenericParameterInfo(RuntimeType type);
}
