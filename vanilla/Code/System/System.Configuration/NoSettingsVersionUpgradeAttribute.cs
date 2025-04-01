namespace System.Configuration;

/// <summary>Specifies that a settings provider should disable any logic that gets invoked when an application upgrade is detected. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NoSettingsVersionUpgradeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.NoSettingsVersionUpgradeAttribute" /> class.</summary>
	public NoSettingsVersionUpgradeAttribute()
	{
	}
}
