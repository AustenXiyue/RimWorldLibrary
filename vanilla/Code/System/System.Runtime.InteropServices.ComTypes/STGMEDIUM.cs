namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides the managed definition of the STGMEDIUM structure.</summary>
public struct STGMEDIUM
{
	/// <summary>Represents a pointer to an interface instance that allows the sending process to control the way the storage is released when the receiving process calls the ReleaseStgMedium function. If <see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> is null, ReleaseStgMedium uses default procedures to release the storage; otherwise, ReleaseStgMedium uses the specified IUnknown interface.</summary>
	[MarshalAs(UnmanagedType.IUnknown)]
	public object pUnkForRelease;

	/// <summary>Specifies the type of storage medium. The marshaling and unmarshaling routines use this value to determine which union member was used. This value must be one of the elements of the <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED" /> enumeration.</summary>
	public TYMED tymed;

	/// <summary>Represents a handle, string, or interface pointer that the receiving process can use to access the data being transferred.</summary>
	public IntPtr unionmember;
}
