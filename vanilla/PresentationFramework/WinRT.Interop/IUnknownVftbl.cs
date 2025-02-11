using System;
using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("00000000-0000-0000-C000-000000000046")]
internal struct IUnknownVftbl
{
	public delegate int _QueryInterface(nint pThis, ref Guid iid, out nint vftbl);

	internal delegate uint _AddRef(nint pThis);

	internal delegate uint _Release(nint pThis);

	public _QueryInterface QueryInterface;

	public _AddRef AddRef;

	public _Release Release;

	public static readonly IUnknownVftbl AbiToProjectionVftbl;

	public static readonly nint AbiToProjectionVftblPtr;

	static IUnknownVftbl()
	{
		AbiToProjectionVftbl = GetVftbl();
		AbiToProjectionVftblPtr = Marshal.AllocHGlobal(Marshal.SizeOf<IUnknownVftbl>());
		Marshal.StructureToPtr(AbiToProjectionVftbl, AbiToProjectionVftblPtr, fDeleteOld: false);
	}

	private static IUnknownVftbl GetVftbl()
	{
		return ComWrappersSupport.IUnknownVftbl;
	}
}
