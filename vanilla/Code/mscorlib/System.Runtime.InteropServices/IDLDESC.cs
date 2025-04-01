namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.IDLDESC" /> instead.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
[Obsolete("Use System.Runtime.InteropServices.ComTypes.IDLDESC instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
public struct IDLDESC
{
	/// <summary>Reserved; set to null.</summary>
	public int dwReserved;

	/// <summary>Indicates an <see cref="T:System.Runtime.InteropServices.IDLFLAG" /> value describing the type.</summary>
	public IDLFLAG wIDLFlags;
}
