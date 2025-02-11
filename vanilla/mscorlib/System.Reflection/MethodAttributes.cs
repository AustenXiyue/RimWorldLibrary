using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Specifies flags for method attributes. These flags are defined in the corhdr.h file.</summary>
[Serializable]
[ComVisible(true)]
[Flags]
public enum MethodAttributes
{
	/// <summary>Retrieves accessibility information.</summary>
	MemberAccessMask = 7,
	/// <summary>Indicates that the member cannot be referenced.</summary>
	PrivateScope = 0,
	/// <summary>Indicates that the method is accessible only to the current class.</summary>
	Private = 1,
	/// <summary>Indicates that the method is accessible to members of this type and its derived types that are in this assembly only.</summary>
	FamANDAssem = 2,
	/// <summary>Indicates that the method is accessible to any class of this assembly.</summary>
	Assembly = 3,
	/// <summary>Indicates that the method is accessible only to members of this class and its derived classes.</summary>
	Family = 4,
	/// <summary>Indicates that the method is accessible to derived classes anywhere, as well as to any class in the assembly.</summary>
	FamORAssem = 5,
	/// <summary>Indicates that the method is accessible to any object for which this object is in scope.</summary>
	Public = 6,
	/// <summary>Indicates that the method is defined on the type; otherwise, it is defined per instance.</summary>
	Static = 0x10,
	/// <summary>Indicates that the method cannot be overridden.</summary>
	Final = 0x20,
	/// <summary>Indicates that the method is virtual.</summary>
	Virtual = 0x40,
	/// <summary>Indicates that the method hides by name and signature; otherwise, by name only.</summary>
	HideBySig = 0x80,
	/// <summary>Indicates that the method can only be overridden when it is also accessible.</summary>
	CheckAccessOnOverride = 0x200,
	/// <summary>Retrieves vtable attributes.</summary>
	VtableLayoutMask = 0x100,
	/// <summary>Indicates that the method will reuse an existing slot in the vtable. This is the default behavior.</summary>
	ReuseSlot = 0,
	/// <summary>Indicates that the method always gets a new slot in the vtable.</summary>
	NewSlot = 0x100,
	/// <summary>Indicates that the class does not provide an implementation of this method.</summary>
	Abstract = 0x400,
	/// <summary>Indicates that the method is special. The name describes how this method is special.</summary>
	SpecialName = 0x800,
	/// <summary>Indicates that the method implementation is forwarded through PInvoke (Platform Invocation Services).</summary>
	PinvokeImpl = 0x2000,
	/// <summary>Indicates that the managed method is exported by thunk to unmanaged code.</summary>
	UnmanagedExport = 8,
	/// <summary>Indicates that the common language runtime checks the name encoding.</summary>
	RTSpecialName = 0x1000,
	/// <summary>Indicates a reserved flag for runtime use only.</summary>
	ReservedMask = 0xD000,
	/// <summary>Indicates that the method has security associated with it. Reserved flag for runtime use only.</summary>
	HasSecurity = 0x4000,
	/// <summary>Indicates that the method calls another method containing security code. Reserved flag for runtime use only.</summary>
	RequireSecObject = 0x8000
}
