using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System;

/// <summary>Describes objects that contain both a managed pointer to a location and a runtime representation of the type that may be stored at that location.</summary>
/// <filterpriority>2</filterpriority>
[CLSCompliant(false)]
[ComVisible(true)]
public struct TypedReference
{
	private RuntimeTypeHandle type;

	private IntPtr Value;

	private IntPtr Type;

	internal bool IsNull
	{
		get
		{
			if (Value.IsNull())
			{
				return Type.IsNull();
			}
			return false;
		}
	}

	/// <summary>Makes a TypedReference for a field identified by a specified object and list of field descriptions.</summary>
	/// <returns>A <see cref="T:System.TypedReference" /> for the field described by the last element of <paramref name="flds" />.</returns>
	/// <param name="target">An object that contains the field described by the first element of <paramref name="flds" />. </param>
	/// <param name="flds">A list of field descriptions where each element describes a field that contains the field described by the succeeding element. Each described field must be a value type. The field descriptions must be RuntimeFieldInfo objects supplied by the type system.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> or <paramref name="flds" /> is null.-or- An element of <paramref name="flds" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="flds" /> array has no elements.-or- An element of <paramref name="flds" /> is not a RuntimeFieldInfo.-or- The <see cref="P:System.Reflection.FieldInfo.IsInitOnly" /> or <see cref="P:System.Reflection.FieldInfo.IsStatic" /> property of an element of <paramref name="flds" /> is true. </exception>
	/// <exception cref="T:System.MissingMemberException">Parameter <paramref name="target" /> does not contain the field described by the first element of <paramref name="flds" />, or an element of <paramref name="flds" /> describes a field that is not contained in the field described by the succeeding element of <paramref name="flds" />.-or- The field described by an element of <paramref name="flds" /> is not a value type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[CLSCompliant(false)]
	[SecurityCritical]
	public static TypedReference MakeTypedReference(object target, FieldInfo[] flds)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (flds == null)
		{
			throw new ArgumentNullException("flds");
		}
		if (flds.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Array must not be of length zero."));
		}
		IntPtr[] array = new IntPtr[flds.Length];
		RuntimeType runtimeType = (RuntimeType)target.GetType();
		for (int i = 0; i < flds.Length; i++)
		{
			RuntimeFieldInfo runtimeFieldInfo = flds[i] as RuntimeFieldInfo;
			if (runtimeFieldInfo == null)
			{
				throw new ArgumentException(Environment.GetResourceString("FieldInfo must be a runtime FieldInfo object."));
			}
			if (runtimeFieldInfo.IsStatic)
			{
				throw new ArgumentException(Environment.GetResourceString("Field in TypedReferences cannot be static or init only."));
			}
			if (runtimeType != runtimeFieldInfo.GetDeclaringTypeInternal() && !runtimeType.IsSubclassOf(runtimeFieldInfo.GetDeclaringTypeInternal()))
			{
				throw new MissingMemberException(Environment.GetResourceString("FieldInfo does not match the target Type."));
			}
			RuntimeType runtimeType2 = (RuntimeType)runtimeFieldInfo.FieldType;
			if (runtimeType2.IsPrimitive)
			{
				throw new ArgumentException(Environment.GetResourceString("TypedReferences cannot be redefined as primitives."));
			}
			if (i < flds.Length - 1 && !runtimeType2.IsValueType)
			{
				throw new MissingMemberException(Environment.GetResourceString("TypedReference can only be made on nested value Types."));
			}
			array[i] = runtimeFieldInfo.FieldHandle.Value;
			runtimeType = runtimeType2;
		}
		return MakeTypedReferenceInternal(target, flds);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern TypedReference MakeTypedReferenceInternal(object target, FieldInfo[] fields);

	/// <summary>Returns the hash code of this object.</summary>
	/// <returns>The hash code of this object.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		if (Type == IntPtr.Zero)
		{
			return 0;
		}
		return __reftype(this).GetHashCode();
	}

	/// <summary>Checks if this object is equal to the specified object.</summary>
	/// <returns>true if this object is equal to the specified object; otherwise, false.</returns>
	/// <param name="o">The object with which to compare the current object. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented. </exception>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object o)
	{
		throw new NotSupportedException(Environment.GetResourceString("This feature is not currently implemented."));
	}

	/// <summary>Converts the specified TypedReference to an Object.</summary>
	/// <returns>An <see cref="T:System.Object" /> converted from a TypedReference.</returns>
	/// <param name="value">The TypedReference to be converted. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static object ToObject(TypedReference value)
	{
		return InternalToObject(&value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal unsafe static extern object InternalToObject(void* value);

	/// <summary>Returns the type of the target of the specified TypedReference.</summary>
	/// <returns>The type of the target of the specified TypedReference.</returns>
	/// <param name="value">The value whose target's type is to be returned. </param>
	/// <filterpriority>1</filterpriority>
	public static Type GetTargetType(TypedReference value)
	{
		return __reftype(value);
	}

	/// <summary>Returns the internal metadata type handle for the specified TypedReference.</summary>
	/// <returns>The internal metadata type handle for the specified TypedReference.</returns>
	/// <param name="value">The TypedReference for which the type handle is requested. </param>
	/// <filterpriority>1</filterpriority>
	public static RuntimeTypeHandle TargetTypeToken(TypedReference value)
	{
		return __reftype(value).TypeHandle;
	}

	/// <summary>Converts the specified value to a TypedReference. This method is not supported.</summary>
	/// <param name="target">The target of the conversion. </param>
	/// <param name="value">The value to be converted. </param>
	/// <exception cref="T:System.NotSupportedException">In all cases. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[CLSCompliant(false)]
	public static void SetTypedReference(TypedReference target, object value)
	{
		throw new NotImplementedException("SetTypedReference");
	}
}
