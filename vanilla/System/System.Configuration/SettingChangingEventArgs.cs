using System.ComponentModel;

namespace System.Configuration;

/// <summary>Provides data for the <see cref="E:System.Configuration.ApplicationSettingsBase.SettingChanging" /> event.</summary>
/// <filterpriority>2</filterpriority>
public class SettingChangingEventArgs : CancelEventArgs
{
	private string settingName;

	private string settingClass;

	private string settingKey;

	private object newValue;

	/// <summary>Gets the name of the application setting associated with the application settings property.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the name of the application setting. </returns>
	/// <filterpriority>2</filterpriority>
	public string SettingName => settingName;

	/// <summary>Gets the application settings property category.</summary>
	/// <returns>A <see cref="T:System.String" /> containing a category description of the setting. Typically, this parameter is set to the application settings group name.</returns>
	/// <filterpriority>2</filterpriority>
	public string SettingClass => settingClass;

	/// <summary>Gets the application settings key associated with the property.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the application settings key.</returns>
	/// <filterpriority>2</filterpriority>
	public string SettingKey => settingKey;

	/// <summary>Gets the new value being assigned to the application settings property.</summary>
	/// <returns>An <see cref="T:System.Object" /> that contains the new value to be assigned to the application settings property.</returns>
	/// <filterpriority>1</filterpriority>
	public object NewValue => newValue;

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.SettingChangingEventArgs" /> class.</summary>
	/// <param name="settingName">A <see cref="T:System.String" /> containing the name of the application setting.</param>
	/// <param name="settingClass">A <see cref="T:System.String" /> containing a category description of the setting. Often this parameter is set to the application settings group name.</param>
	/// <param name="settingKey">A <see cref="T:System.String" /> containing the application settings key.</param>
	/// <param name="newValue">An <see cref="T:System.Object" /> that contains the new value to be assigned to the application settings property.</param>
	/// <param name="cancel">true to cancel the event; otherwise, false. </param>
	public SettingChangingEventArgs(string settingName, string settingClass, string settingKey, object newValue, bool cancel)
		: base(cancel)
	{
		this.settingName = settingName;
		this.settingClass = settingClass;
		this.settingKey = settingKey;
		this.newValue = newValue;
	}
}
