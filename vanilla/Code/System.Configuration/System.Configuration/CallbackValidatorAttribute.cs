namespace System.Configuration;

/// <summary>Specifies a <see cref="T:System.Configuration.CallbackValidator" /> object to use for code validation. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CallbackValidatorAttribute : ConfigurationValidatorAttribute
{
	private string callbackMethodName = "";

	private Type type;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets or sets the name of the callback method.</summary>
	/// <returns>The name of the method to call.</returns>
	public string CallbackMethodName
	{
		get
		{
			return callbackMethodName;
		}
		set
		{
			callbackMethodName = value;
			instance = null;
		}
	}

	/// <summary>Gets or sets the type of the validator.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the current validator attribute instance.</returns>
	public Type Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			instance = null;
		}
	}

	/// <summary>Gets the validator instance.</summary>
	/// <returns>The current <see cref="T:System.Configuration.ConfigurationValidatorBase" /> instance.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value of the <see cref="P:System.Configuration.CallbackValidatorAttribute.Type" /> property is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Configuration.CallbackValidatorAttribute.CallbackMethodName" /> property is not set to a public static void method with one object parameter.</exception>
	public override ConfigurationValidatorBase ValidatorInstance => instance;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.CallbackValidatorAttribute" /> class. </summary>
	public CallbackValidatorAttribute()
	{
	}
}
