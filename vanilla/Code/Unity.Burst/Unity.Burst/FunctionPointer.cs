using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Burst;

public readonly struct FunctionPointer<T> : IFunctionPointer
{
	[NativeDisableUnsafePtrRestriction]
	private readonly IntPtr _ptr;

	public IntPtr Value => _ptr;

	public T Invoke => Marshal.GetDelegateForFunctionPointer<T>(_ptr);

	public bool IsCreated => _ptr != IntPtr.Zero;

	public FunctionPointer(IntPtr ptr)
	{
		_ptr = ptr;
	}

	IFunctionPointer IFunctionPointer.FromIntPtr(IntPtr ptr)
	{
		return new FunctionPointer<T>(ptr);
	}
}
