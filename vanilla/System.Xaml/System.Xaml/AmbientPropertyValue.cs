namespace System.Xaml;

/// <summary>Reports information about an ambient property, as part of an <see cref="T:System.Xaml.IAmbientProvider" /> implementation.</summary>
public class AmbientPropertyValue
{
	private XamlMember _property;

	private object _value;

	/// <summary>Gets the value of the ambient property.</summary>
	/// <returns>The value of the ambient property.</returns>
	public object Value => _value;

	/// <summary>Gets the XAML type system identifier (<see cref="T:System.Xaml.XamlMember" />) that represents the ambient property.</summary>
	/// <returns>The identifier that represents the ambient property.</returns>
	public XamlMember RetrievedProperty => _property;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.AmbientPropertyValue" /> class.</summary>
	/// <param name="property">The identifier that represents the ambient property.</param>
	/// <param name="value">The value to report.</param>
	public AmbientPropertyValue(XamlMember property, object value)
	{
		_property = property;
		_value = value;
	}
}
