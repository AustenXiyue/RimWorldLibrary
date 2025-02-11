namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.BINDPTR" /> instead.</summary>
[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
[Obsolete]
public struct BINDPTR
{
	/// <summary>Represents a pointer to a <see cref="T:System.Runtime.InteropServices.FUNCDESC" /> structure.</summary>
	[FieldOffset(0)]
	public IntPtr lpfuncdesc;

	/// <summary>Represents a pointer to a <see cref="F:System.Runtime.InteropServices.BINDPTR.lptcomp" /> interface.</summary>
	[FieldOffset(0)]
	public IntPtr lptcomp;

	/// <summary>Represents a pointer to a <see cref="T:System.Runtime.InteropServices.VARDESC" /> structure.</summary>
	[FieldOffset(0)]
	public IntPtr lpvardesc;
}
