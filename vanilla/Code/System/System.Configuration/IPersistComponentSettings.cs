namespace System.Configuration;

/// <summary>Defines standard functionality for controls or libraries that store and retrieve application settings.</summary>
/// <filterpriority>2</filterpriority>
public interface IPersistComponentSettings
{
	/// <summary>Gets or sets a value indicating whether the control should automatically persist its application settings properties.</summary>
	/// <returns>true if the control should automatically persist its state; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	bool SaveSettings { get; set; }

	/// <summary>Gets or sets the value of the application settings key for the current instance of the control.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the settings key for the current instance of the control.</returns>
	/// <filterpriority>2</filterpriority>
	string SettingsKey { get; set; }

	/// <summary>Reads the control's application settings into their corresponding properties and updates the control's state.</summary>
	/// <filterpriority>2</filterpriority>
	void LoadComponentSettings();

	/// <summary>Resets the control's application settings properties to their default values.</summary>
	/// <filterpriority>1</filterpriority>
	void ResetComponentSettings();

	/// <summary>Persists the control's application settings properties.</summary>
	/// <filterpriority>2</filterpriority>
	void SaveComponentSettings();
}
