namespace System.Runtime.InteropServices;

/// <summary>Indicates how an assembly should be produced.</summary>
[Serializable]
[ComVisible(true)]
[Flags]
public enum TypeLibImporterFlags
{
	/// <summary>Generates a primary interop assembly. For more information, see the <see cref="T:System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute" /> attribute. A keyfile must be specified.</summary>
	PrimaryInteropAssembly = 1,
	/// <summary>Imports all interfaces as interfaces that suppress the common language runtime's stack crawl for <see cref="F:System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode" /> permission. Be sure you understand the responsibilities associated with suppressing this security check. </summary>
	UnsafeInterfaces = 2,
	/// <summary>Imports all SAFEARRAY instances as <see cref="T:System.Array" /> instead of typed, single-dimensional, zero-based managed arrays. This option is useful when dealing with multi-dimensional, non-zero-based SAFEARRAY instances, which otherwise cannot be accessed unless you edit the resulting assembly by using the MSIL Disassembler (Ildasm.exe) and MSIL Assembler (Ilasm.exe) tools.</summary>
	SafeArrayAsSystemArray = 4,
	/// <summary>Transforms [out, retval] parameters of methods on dispatch-only interfaces (dispinterface) into return values.</summary>
	TransformDispRetVals = 8,
	/// <summary>No special settings. This is the default.</summary>
	None = 0,
	/// <summary>Not used.</summary>
	PreventClassMembers = 0x10,
	/// <summary>Imports a type library for any platform.</summary>
	ImportAsAgnostic = 0x800,
	/// <summary>Imports a type library for the Itanium platform.</summary>
	ImportAsItanium = 0x400,
	/// <summary>Imports a type library for the x86 64-bit platform.</summary>
	ImportAsX64 = 0x200,
	/// <summary>Imports a type library for the x86 platform.</summary>
	ImportAsX86 = 0x100,
	/// <summary>Uses reflection-only loading.</summary>
	ReflectionOnlyLoading = 0x1000,
	/// <summary>Uses serializable classes.</summary>
	SerializableValueClasses = 0x20,
	/// <summary>Prevents inclusion of a version resource in the interop assembly. For more information, see the <see cref="M:System.Reflection.Emit.AssemblyBuilder.DefineVersionInfoResource" /> method.</summary>
	NoDefineVersionResource = 0x2000,
	/// <summary>Imports a library for the ARM platform.</summary>
	ImportAsArm = 0x4000
}
