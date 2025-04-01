namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to perform long-integer validation on a configuration property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LongValidatorAttribute : ConfigurationValidatorAttribute
{
	private bool excludeRange;

	private long maxValue;

	private long minValue;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets or sets a value that indicates whether to include or exclude the integers in the range defined by the <see cref="P:System.Configuration.LongValidatorAttribute.MinValue" /> and <see cref="P:System.Configuration.LongValidatorAttribute.MaxValue" /> property values.</summary>
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
	/// <returns>A long integer that indicates the allowed maximum value.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The selected value is less than <see cref="P:System.Configuration.LongValidatorAttribute.MinValue" />.</exception>
	public long MaxValue
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
	/// <exception cref="T:System.ArgumentOutOfRangeException">The selected value is greater than <see cref="P:System.Configuration.LongValidatorAttribute.MaxValue" />.</exception>
	public long MinValue
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

	/// <summary>Gets an instance of the <see cref="T:System.Configuration.LongValidator" /> class.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator instance.</returns>
	public override ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = new LongValidator(minValue, maxValue, excludeRange);
			}
			return instance;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.LongValidatorAttribute" /> class.</summary>
	public LongValidatorAttribute()
	{
	}
}
