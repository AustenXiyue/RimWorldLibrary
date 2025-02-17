namespace System.Runtime.InteropServices;

/// <summary>Indicates whether a managed interface is dual, dispatch-only, or IUnknown -only when exposed to COM.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class InterfaceTypeAttribute : Attribute
{
	internal ComInterfaceType _val;

	/// <summary>Gets the <see cref="T:System.Runtime.InteropServices.ComInterfaceType" /> value that describes how the interface should be exposed to COM.</summary>
	/// <returns>The <see cref="T:System.Runtime.InteropServices.ComInterfaceType" /> value that describes how the interface should be exposed to COM.</returns>
	public ComInterfaceType Value => _val;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.InterfaceTypeAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.ComInterfaceType" /> enumeration member.</summary>
	/// <param name="interfaceType">One of the <see cref="T:System.Runtime.InteropServices.ComInterfaceType" /> values that describes how the interface should be exposed to COM clients. </param>
	public InterfaceTypeAttribute(ComInterfaceType interfaceType)
	{
		_val = interfaceType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.InterfaceTypeAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.ComInterfaceType" /> enumeration member.</summary>
	/// <param name="interfaceType">Describes how the interface should be exposed to COM clients. </param>
	public InterfaceTypeAttribute(short interfaceType)
	{
		_val = (ComInterfaceType)interfaceType;
	}
}
