namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.TYPELIBATTR" /> instead.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
[Obsolete]
public struct TYPELIBATTR
{
	/// <summary>Represents a globally unique library ID of a type library.</summary>
	public Guid guid;

	/// <summary>Represents a locale ID of a type library.</summary>
	public int lcid;

	/// <summary>Represents the target hardware platform of a type library.</summary>
	public SYSKIND syskind;

	/// <summary>Represents the major version number of a type library.</summary>
	public short wMajorVerNum;

	/// <summary>Represents the minor version number of a type library.</summary>
	public short wMinorVerNum;

	/// <summary>Represents library flags.</summary>
	public LIBFLAGS wLibFlags;
}
