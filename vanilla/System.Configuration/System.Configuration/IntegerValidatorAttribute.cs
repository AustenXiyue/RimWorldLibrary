namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to perform integer validation on a configuration property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IntegerValidatorAttribute : ConfigurationValidatorAttribute
{
	private bool excludeRange;

	private int maxValue;

	private int minValue;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets or sets a value that indicates whether to include or exclude the integers in the range defined by the <see cref="P:System.Configuration.IntegerValidatorAttribute.MinValue" /> and <see cref="P:System.Configuration.IntegerValidatorAttribute.MaxValue" /> property values.</summary>
	/// <returns>true if the value must be excluded; otherwise, false. The default is false.</returns>
	public bool ExcludeRange
	{
		get
		{
			return excludeRange;
		}
		set
		{
			excludeRange = value;
			instance = null;
		}
	}

	/// <summary>Gets or sets the maximum value allowed for the property.</summary>
	/// <returns>An integer that indicates the allowed maximum value.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The selected value is less than <see cref="P:System.Configuration.IntegerValidatorAttribute.MinValue" />.</exception>
	public int MaxValue
	{
		get
		{
			return maxValue;
		}
		set
		{
			maxValue = value;
			instance = null;
		}
	}

	/// <summary>Gets or sets the minimum value allowed for the property.</summary>
	/// <returns>An integer that indicates the allowed minimum value.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The selected value is greater than <see cref="P:System.Configuration.IntegerValidatorAttribute.MaxValue" />.</exception>
	public int MinValue
	{
		get
		{
			return minValue;
		}
		set
		{
			minValue = value;
			instance = null;
		}
	}

	/// <summary>Gets an instance of the <see cref="T:System.Configuration.IntegerValidator" /> class.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator instance.</returns>
	public override ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = new IntegerValidator(minValue, maxValue, excludeRange);
			}
			return instance;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.IntegerValidatorAttribute" /> class.</summary>
	public IntegerValidatorAttribute()
	{
	}
}
