using System.Runtime.InteropServices;

namespace System.Diagnostics.SymbolStore;

/// <summary>Holds the public GUIDs for language vendors to be used with the symbol store.</summary>
[ComVisible(true)]
public class SymLanguageVendor
{
	/// <summary>Specifies the GUID of the Microsoft language vendor.</summary>
	public static readonly Guid Microsoft;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.SymbolStore.SymLanguageVendor" /> class. </summary>
	public SymLanguageVendor()
	{
	}
}
