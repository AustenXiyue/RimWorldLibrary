namespace System.ComponentModel;

/// <summary>Specifies which properties should be reported by type descriptors, specifically the <see cref="M:System.ComponentModel.TypeDescriptor.GetProperties(System.Object)" /> method.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class PropertyFilterAttribute : Attribute
{
	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	public static readonly PropertyFilterAttribute Default = new PropertyFilterAttribute(PropertyFilterOptions.All);

	private PropertyFilterOptions _filter;

	/// <summary>Gets the filter options for this <see cref="T:System.ComponentModel.PropertyFilterAttribute" /> .NET Framework attribute.</summary>
	/// <returns>The property filter options.</returns>
	public PropertyFilterOptions Filter => _filter;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyFilterAttribute" /> class.</summary>
	/// <param name="filter">The options that describe this property filter.</param>
	public PropertyFilterAttribute(PropertyFilterOptions filter)
	{
		_filter = filter;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.ComponentModel.PropertyFilterAttribute" /> .NET Framework attribute is equal to a specified object.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.PropertyFilterAttribute" /> is equal to the current <see cref="T:System.ComponentModel.PropertyFilterAttribute" />; otherwise, false. </returns>
	/// <param name="value">The object to compare to this <see cref="T:System.ComponentModel.PropertyFilterAttribute" />.</param>
	public override bool Equals(object value)
	{
		if (value is PropertyFilterAttribute propertyFilterAttribute && propertyFilterAttribute._filter.Equals(_filter))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.ComponentModel.PropertyFilterAttribute" /> .NET Framework attribute. </summary>
	/// <returns>A signed 32-bit integer value.</returns>
	public override int GetHashCode()
	{
		return _filter.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether the property filter options of the current <see cref="T:System.ComponentModel.PropertyFilterAttribute" /> .NET Framework attribute match the property filter options of the provided object. </summary>
	/// <returns>true if a match exists; otherwise, false.</returns>
	/// <param name="value">The object to compare. This object is expected to be a <see cref="T:System.ComponentModel.PropertyFilterAttribute" />.</param>
	public override bool Match(object value)
	{
		if (!(value is PropertyFilterAttribute propertyFilterAttribute))
		{
			return false;
		}
		return (_filter & propertyFilterAttribute._filter) == _filter;
	}
}
