namespace System.Configuration;

/// <summary>Provides validation of an <see cref="T:System.Int32" /> value.</summary>
public class IntegerValidator : ConfigurationValidatorBase
{
	private bool rangeIsExclusive;

	private int minValue;

	private int maxValue = int.MaxValue;

	private int resolution;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.IntegerValidator" /> class. </summary>
	/// <param name="minValue">An <see cref="T:System.Int32" /> object that specifies the minimum length of the integer value.</param>
	/// <param name="maxValue">An <see cref="T:System.Int32" /> object that specifies the maximum length of the integer value.</param>
	/// <param name="rangeIsExclusive">A <see cref="T:System.Boolean" /> value that specifies whether the validation range is exclusive.</param>
	/// <param name="resolution">An <see cref="T:System.Int32" /> object that specifies a value that must be matched.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="resolution" /> is less than 0.- or -<paramref name="minValue" /> is greater than <paramref name="maxValue" />.</exception>
	public IntegerValidator(int minValue, int maxValue, bool rangeIsExclusive, int resolution)
	{
		if (minValue != 0)
		{
			this.minValue = minValue;
		}
		if (maxValue != 0)
		{
			this.maxValue = maxValue;
		}
		this.rangeIsExclusive = rangeIsExclusive;
		this.resolution = resolution;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.IntegerValidator" /> class. </summary>
	/// <param name="minValue">An <see cref="T:System.Int32" /> object that specifies the minimum value.</param>
	/// <param name="maxValue">An <see cref="T:System.Int32" /> object that specifies the maximum value.</param>
	/// <param name="rangeIsExclusive">true to specify that the validation range is exclusive. Inclusive means the value to be validated must be within the specified range; exclusive means that it must be below the minimum or above the maximum.</param>
	public IntegerValidator(int minValue, int maxValue, bool rangeIsExclusive)
		: this(minValue, maxValue, rangeIsExclusive, 0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.IntegerValidator" /> class. </summary>
	/// <param name="minValue">An <see cref="T:System.Int32" /> object that specifies the minimum value.</param>
	/// <param name="maxValue">An <see cref="T:System.Int32" /> object that specifies the maximum value.</param>
	public IntegerValidator(int minValue, int maxValue)
		: this(minValue, maxValue, rangeIsExclusive: false, 0)
	{
	}

	/// <summary>Determines whether the type of the object can be validated.</summary>
	/// <returns>true if the <paramref name="type" /> parameter matches an <see cref="T:System.Int32" /> value; otherwise, false. </returns>
	/// <param name="type">The type of the object.</param>
	public override bool CanValidate(Type type)
	{
		return type == typeof(int);
	}

	/// <summary>Determines whether the value of an object is valid.</summary>
	/// <param name="value">The value to be validated.</param>
	public override void Validate(object value)
	{
		int num = (int)value;
		if (!rangeIsExclusive)
		{
			if (num < minValue || num > maxValue)
			{
				throw new ArgumentException("The value must be in the range " + minValue + " - " + maxValue);
			}
		}
		else if (num >= minValue && num <= maxValue)
		{
			throw new ArgumentException("The value must not be in the range " + minValue + " - " + maxValue);
		}
		if (resolution != 0 && num % resolution != 0)
		{
			throw new ArgumentException("The value must have a resolution of " + resolution);
		}
	}
}
