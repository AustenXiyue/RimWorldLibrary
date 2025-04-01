using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Reflection;

/// <summary>Provides a wrapper class for pointers.</summary>
[Serializable]
[ComVisible(true)]
[CLSCompliant(false)]
public sealed class Pointer : ISerializable
{
	[SecurityCritical]
	private unsafe void* _ptr;

	private RuntimeType _ptrType;

	private Pointer()
	{
	}

	[SecurityCritical]
	private unsafe Pointer(SerializationInfo info, StreamingContext context)
	{
		_ptr = ((IntPtr)info.GetValue("_ptr", typeof(IntPtr))).ToPointer();
		_ptrType = (RuntimeType)info.GetValue("_ptrType", typeof(RuntimeType));
	}

	/// <summary>Boxes the supplied unmanaged memory pointer and the type associated with that pointer into a managed <see cref="T:System.Reflection.Pointer" /> wrapper object. The value and the type are saved so they can be accessed from the native code during an invocation.</summary>
	/// <returns>A pointer object.</returns>
	/// <param name="ptr">The supplied unmanaged memory pointer. </param>
	/// <param name="type">The type associated with the <paramref name="ptr" /> parameter. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a pointer. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	[SecurityCritical]
	public unsafe static object Box(void* ptr, Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (!type.IsPointer)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a Pointer."), "ptr");
		}
		RuntimeType runtimeType = type as RuntimeType;
		if (runtimeType == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a Pointer."), "ptr");
		}
		return new Pointer
		{
			_ptr = ptr,
			_ptrType = runtimeType
		};
	}

	/// <summary>Returns the stored pointer.</summary>
	/// <returns>This method returns void.</returns>
	/// <param name="ptr">The stored pointer. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ptr" /> is not a pointer. </exception>
	[SecurityCritical]
	public unsafe static void* Unbox(object ptr)
	{
		if (!(ptr is Pointer))
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a Pointer."), "ptr");
		}
		return ((Pointer)ptr)._ptr;
	}

	internal RuntimeType GetPointerType()
	{
		return _ptrType;
	}

	[SecurityCritical]
	internal unsafe object GetPointerValue()
	{
		return (IntPtr)_ptr;
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the file name, fusion log, and additional exception information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	[SecurityCritical]
	unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("_ptr", new IntPtr(_ptr));
		info.AddValue("_ptrType", _ptrType);
	}
}
