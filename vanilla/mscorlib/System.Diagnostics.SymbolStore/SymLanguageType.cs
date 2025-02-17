using System.Runtime.InteropServices;

namespace System.Diagnostics.SymbolStore;

/// <summary>Holds the public GUIDs for language types to be used with the symbol store.</summary>
[ComVisible(true)]
public class SymLanguageType
{
	/// <summary>Specifies the GUID of the Basic language type to be used with the symbol store.</summary>
	public static readonly Guid Basic;

	/// <summary>Specifies the GUID of the C language type to be used with the symbol store.</summary>
	public static readonly Guid C;

	/// <summary>Specifies the GUID of the Cobol language type to be used with the symbol store.</summary>
	public static readonly Guid Cobol;

	/// <summary>Specifies the GUID of the C++ language type to be used with the symbol store.</summary>
	public static readonly Guid CPlusPlus;

	/// <summary>Specifies the GUID of the C# language type to be used with the symbol store.</summary>
	public static readonly Guid CSharp;

	/// <summary>Specifies the GUID of the ILAssembly language type to be used with the symbol store.</summary>
	public static readonly Guid ILAssembly;

	/// <summary>Specifies the GUID of the Java language type to be used with the symbol store.</summary>
	public static readonly Guid Java;

	/// <summary>Specifies the GUID of the JScript language type to be used with the symbol store.</summary>
	public static readonly Guid JScript;

	/// <summary>Specifies the GUID of the C++ language type to be used with the symbol store.</summary>
	public static readonly Guid MCPlusPlus;

	/// <summary>Specifies the GUID of the Pascal language type to be used with the symbol store.</summary>
	public static readonly Guid Pascal;

	/// <summary>Specifies the GUID of the SMC language type to be used with the symbol store.</summary>
	public static readonly Guid SMC;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.SymbolStore.SymLanguageType" /> class.</summary>
	public SymLanguageType()
	{
	}
}
