namespace System.Runtime.InteropServices;

/// <summary>Marshals data of type VT_VARIANT | VT_BYREF from managed to unmanaged code. This class cannot be inherited.</summary>
[Serializable]
public sealed class VariantWrapper
{
	private object m_WrappedObject;

	/// <summary>Gets the object wrapped by the <see cref="T:System.Runtime.InteropServices.VariantWrapper" /> object.</summary>
	/// <returns>The object wrapped by the <see cref="T:System.Runtime.InteropServices.VariantWrapper" /> object.</returns>
	public object WrappedObject => m_WrappedObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.VariantWrapper" /> class for the specified <see cref="T:System.Object" /> parameter.</summary>
	/// <param name="obj">The object to marshal. </param>
	public VariantWrapper(object obj)
	{
		m_WrappedObject = obj;
	}
}
