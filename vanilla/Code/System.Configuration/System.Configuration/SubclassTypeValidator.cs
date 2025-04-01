namespace System.Configuration;

/// <summary>Validates that an object is a derived class of a specified type.</summary>
public sealed class SubclassTypeValidator : ConfigurationValidatorBase
{
	private Type baseClass;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SubclassTypeValidator" /> class. </summary>
	/// <param name="baseClass">The base class to validate against.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="baseClass" /> is null.</exception>
	public SubclassTypeValidator(Type baseClass)
	{
		this.baseClass = baseClass;
	}

	/// <summary>Determines whether an object can be validated based on type.</summary>
	/// <returns>true if the <paramref name="type" /> parameter matches a type that can be validated; otherwise, false. </returns>
	/// <param name="type">The object type.</param>
	public override bool CanValidate(Type type)
	{
		return type == typeof(Type);
	}

	/// <summary>Determines whether the value of an object is valid. </summary>
	/// <param name="value">The object value.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not of a <see cref="T:System.Type" /> that can be derived from <paramref name="baseClass" /> as defined in the constructor.</exception>
	public override void Validate(object value)
	{
		Type c = (Type)value;
		if (!baseClass.IsAssignableFrom(c))
		{
			throw new ArgumentException("The value must be a subclass");
		}
	}
}
