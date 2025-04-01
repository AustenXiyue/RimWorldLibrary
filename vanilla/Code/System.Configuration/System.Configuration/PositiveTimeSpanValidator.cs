namespace System.Configuration;

/// <summary>Provides validation of a <see cref="T:System.TimeSpan" /> object. This class cannot be inherited.</summary>
public class PositiveTimeSpanValidator : ConfigurationValidatorBase
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.PositiveTimeSpanValidator" /> class. </summary>
	public PositiveTimeSpanValidator()
	{
	}

	/// <summary>Determines whether the object type can be validated.</summary>
	/// <returns>true if the <paramref name="type" /> parameter matches a <see cref="T:System.TimeSpan" /> object; otherwise, false. </returns>
	/// <param name="type">The object type.</param>
	public override bool CanValidate(Type type)
	{
		return type == typeof(TimeSpan);
	}

	/// <summary>Determines whether the value of an object is valid.</summary>
	/// <param name="value">The value of an object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> cannot be resolved as a positive <see cref="T:System.TimeSpan" /> value.</exception>
	public override void Validate(object value)
	{
		if ((TimeSpan)value <= new TimeSpan(0L))
		{
			throw new ArgumentException("The time span value must be positive");
		}
	}
}
