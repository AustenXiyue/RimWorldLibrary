namespace System.Runtime.InteropServices;

/// <summary>Indicates which IDispatch implementation the common language runtime uses when exposing dual interfaces and dispinterfaces to COM.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false)]
[Obsolete("This attribute is deprecated and will be removed in a future version.", false)]
[ComVisible(true)]
public sealed class IDispatchImplAttribute : Attribute
{
	internal IDispatchImplType _val;

	/// <summary>Gets the <see cref="T:System.Runtime.InteropServices.IDispatchImplType" /> value used by the class.</summary>
	/// <returns>The <see cref="T:System.Runtime.InteropServices.IDispatchImplType" /> value used by the class.</returns>
	public IDispatchImplType Value => _val;

	/// <summary>Initializes a new instance of the IDispatchImplAttribute class with specified <see cref="T:System.Runtime.InteropServices.IDispatchImplType" /> value.</summary>
	/// <param name="implType">Indicates which <see cref="T:System.Runtime.InteropServices.IDispatchImplType" /> enumeration will be used. </param>
	public IDispatchImplAttribute(IDispatchImplType implType)
	{
		_val = implType;
	}

	/// <summary>Initializes a new instance of the IDispatchImplAttribute class with specified <see cref="T:System.Runtime.InteropServices.IDispatchImplType" /> value.</summary>
	/// <param name="implType">Indicates which <see cref="T:System.Runtime.InteropServices.IDispatchImplType" /> enumeration will be used. </param>
	public IDispatchImplAttribute(short implType)
	{
		_val = (IDispatchImplType)implType;
	}
}
