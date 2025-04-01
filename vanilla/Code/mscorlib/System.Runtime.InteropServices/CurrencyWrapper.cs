namespace System.Runtime.InteropServices;

/// <summary>Wraps objects the marshaler should marshal as a VT_CY.</summary>
[Serializable]
[ComVisible(true)]
public sealed class CurrencyWrapper
{
	private decimal m_WrappedObject;

	/// <summary>Gets the wrapped object to be marshaled as type VT_CY.</summary>
	/// <returns>The wrapped object to be marshaled as type VT_CY.</returns>
	public decimal WrappedObject => m_WrappedObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.CurrencyWrapper" /> class with the Decimal to be wrapped and marshaled as type VT_CY.</summary>
	/// <param name="obj">The Decimal to be wrapped and marshaled as VT_CY. </param>
	public CurrencyWrapper(decimal obj)
	{
		m_WrappedObject = obj;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.CurrencyWrapper" /> class with the object containing the Decimal to be wrapped and marshaled as type VT_CY.</summary>
	/// <param name="obj">The object containing the Decimal to be wrapped and marshaled as VT_CY. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="obj" /> parameter is not a <see cref="T:System.Decimal" /> type.</exception>
	public CurrencyWrapper(object obj)
	{
		if (!(obj is decimal))
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type Decimal."), "obj");
		}
		m_WrappedObject = (decimal)obj;
	}
}
