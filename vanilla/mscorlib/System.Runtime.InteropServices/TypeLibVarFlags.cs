namespace System.Runtime.InteropServices;

/// <summary>Describes the original settings of the <see cref="T:System.Runtime.InteropServices.VARFLAGS" /> in the COM type library from which the variable was imported.</summary>
[Serializable]
[Flags]
[ComVisible(true)]
public enum TypeLibVarFlags
{
	/// <summary>Assignment to the variable should not be allowed.</summary>
	FReadOnly = 1,
	/// <summary>The variable returns an object that is a source of events.</summary>
	FSource = 2,
	/// <summary>The variable supports data binding.</summary>
	FBindable = 4,
	/// <summary>Indicates that the property supports the COM OnRequestEdit notification.</summary>
	FRequestEdit = 8,
	/// <summary>The variable is displayed as bindable. <see cref="F:System.Runtime.InteropServices.TypeLibVarFlags.FBindable" /> must also be set.</summary>
	FDisplayBind = 0x10,
	/// <summary>The variable is the single property that best represents the object. Only one variable in a type info can have this value.</summary>
	FDefaultBind = 0x20,
	/// <summary>The variable should not be displayed in a browser, though it exists and is bindable.</summary>
	FHidden = 0x40,
	/// <summary>This flag is intended for system-level functions or functions that type browsers should not display.</summary>
	FRestricted = 0x80,
	/// <summary>Permits an optimization in which the compiler looks for a member named "xyz" on the type "abc". If such a member is found and is flagged as an accessor function for an element of the default collection, then a call is generated to that member function.</summary>
	FDefaultCollelem = 0x100,
	/// <summary>The default display in the user interface.</summary>
	FUiDefault = 0x200,
	/// <summary>The variable appears in an object browser, but not in a properties browser.</summary>
	FNonBrowsable = 0x400,
	/// <summary>Tags the interface as having default behaviors.</summary>
	FReplaceable = 0x800,
	/// <summary>The variable is mapped as individual bindable properties.</summary>
	FImmediateBind = 0x1000
}
