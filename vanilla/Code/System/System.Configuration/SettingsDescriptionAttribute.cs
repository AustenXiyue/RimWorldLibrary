namespace System.Configuration;

/// <summary>Provides a string that describes an individual configuration property. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SettingsDescriptionAttribute : Attribute
{
	private string desc;

	/// <summary>Gets the descriptive text for the associated configuration property.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the descriptive text for the associated configuration property.</returns>
	/// <filterpriority>2</filterpriority>
	public string Description => desc;

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.SettingsDescriptionAttribute" /> class.</summary>
	/// <param name="description">The <see cref="T:System.String" /> used as descriptive text.</param>
	public SettingsDescriptionAttribute(string description)
	{
		desc = description;
	}
}
