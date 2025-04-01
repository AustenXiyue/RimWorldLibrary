namespace System.Configuration;

/// <summary>Specifies that an application settings group or property contains distinct values for each user of an application. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserScopedSettingAttribute : SettingAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.UserScopedSettingAttribute" /> class.</summary>
	public UserScopedSettingAttribute()
	{
	}
}
