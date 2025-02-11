namespace System.Diagnostics.Contracts;

/// <summary>Enables you to set contract and tool options at assembly, type, or method granularity.</summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public sealed class ContractOptionAttribute : Attribute
{
	private string _category;

	private string _setting;

	private bool _enabled;

	private string _value;

	/// <summary>Gets the category of the option.</summary>
	/// <returns>The category of the option.</returns>
	public string Category => _category;

	/// <summary>Gets the setting for the option.</summary>
	/// <returns>The setting for the option.</returns>
	public string Setting => _setting;

	/// <summary>Determines if an option is enabled.</summary>
	/// <returns>true if the option is enabled; otherwise, false.</returns>
	public bool Enabled => _enabled;

	/// <summary>Gets the value for the option.</summary>
	/// <returns>The value for the option.</returns>
	public string Value => _value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractOptionAttribute" /> class by using the provided category, setting, and enable/disable value.</summary>
	/// <param name="category">The category for the option to be set.</param>
	/// <param name="setting">The option setting.</param>
	/// <param name="enabled">true to enable the option; false to disable the option.</param>
	public ContractOptionAttribute(string category, string setting, bool enabled)
	{
		_category = category;
		_setting = setting;
		_enabled = enabled;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractOptionAttribute" /> class by using the provided category, setting, and value.</summary>
	/// <param name="category">The category of the option to be set.</param>
	/// <param name="setting">The option setting.</param>
	/// <param name="value">The value for the setting.</param>
	public ContractOptionAttribute(string category, string setting, string value)
	{
		_category = category;
		_setting = setting;
		_value = value;
	}
}
