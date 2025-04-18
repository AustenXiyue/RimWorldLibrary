namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.TYPEFLAGS" /> instead.</summary>
[Serializable]
[Flags]
[Obsolete("Use System.Runtime.InteropServices.ComTypes.TYPEFLAGS instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
public enum TYPEFLAGS : short
{
	/// <summary>A type description that describes an Application object.</summary>
	TYPEFLAG_FAPPOBJECT = 1,
	/// <summary>Instances of the type can be created by ITypeInfo::CreateInstance.</summary>
	TYPEFLAG_FCANCREATE = 2,
	/// <summary>The type is licensed.</summary>
	TYPEFLAG_FLICENSED = 4,
	/// <summary>The type is predefined. The client application should automatically create a single instance of the object that has this attribute. The name of the variable that points to the object is the same as the class name of the object.</summary>
	TYPEFLAG_FPREDECLID = 8,
	/// <summary>The type should not be displayed to browsers.</summary>
	TYPEFLAG_FHIDDEN = 0x10,
	/// <summary>The type is a control from which other types will be derived, and should not be displayed to users.</summary>
	TYPEFLAG_FCONTROL = 0x20,
	/// <summary>The interface supplies both IDispatch and VTBL binding.</summary>
	TYPEFLAG_FDUAL = 0x40,
	/// <summary>The interface cannot add members at run time.</summary>
	TYPEFLAG_FNONEXTENSIBLE = 0x80,
	/// <summary>The types used in the interface are fully compatible with Automation, including VTBL binding support. Setting dual on an interface sets this flag in addition to <see cref="F:System.Runtime.InteropServices.TYPEFLAGS.TYPEFLAG_FDUAL" />. Not allowed on dispinterfaces.</summary>
	TYPEFLAG_FOLEAUTOMATION = 0x100,
	/// <summary>Should not be accessible from macro languages. This flag is intended for system-level types or types that type browsers should not display.</summary>
	TYPEFLAG_FRESTRICTED = 0x200,
	/// <summary>The class supports aggregation.</summary>
	TYPEFLAG_FAGGREGATABLE = 0x400,
	/// <summary>The object supports IConnectionPointWithDefault, and has default behaviors.</summary>
	TYPEFLAG_FREPLACEABLE = 0x800,
	/// <summary>Indicates that the interface derives from IDispatch, either directly or indirectly. This flag is computed, there is no Object Description Language for the flag.</summary>
	TYPEFLAG_FDISPATCHABLE = 0x1000,
	/// <summary>Indicates base interfaces should be checked for name resolution before checking children, the reverse of the default behavior.</summary>
	TYPEFLAG_FREVERSEBIND = 0x2000,
	/// <summary>Indicates that the interface will be using a proxy/stub dynamic link library. This flag specifies that the type library proxy should not be unregistered when the type library is unregistered.</summary>
	TYPEFLAG_FPROXY = 0x4000
}
