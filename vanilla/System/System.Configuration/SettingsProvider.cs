using System.Configuration.Provider;

namespace System.Configuration;

/// <summary>Acts as a base class for deriving custom settings providers in the application settings architecture.</summary>
/// <filterpriority>2</filterpriority>
public abstract class SettingsProvider : ProviderBase
{
	/// <summary>Gets or sets the name of the currently running application.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract string ApplicationName { get; set; }

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.SettingsProvider" /> class.</summary>
	protected SettingsProvider()
	{
	}

	/// <summary>Returns the collection of settings property values for the specified application instance and settings property group.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> containing the values for the specified settings property group.</returns>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> describing the current application use.</param>
	/// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection" /> containing the settings property group whose values are to be retrieved.</param>
	/// <filterpriority>2</filterpriority>
	public abstract SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection);

	/// <summary>Sets the values of the specified group of property settings.</summary>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> describing the current application usage.</param>
	/// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> representing the group of property settings to set.</param>
	/// <filterpriority>2</filterpriority>
	public abstract void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection);
}
