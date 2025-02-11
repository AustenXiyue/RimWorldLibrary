namespace System.Configuration;

/// <summary>Specifies that an application settings property has a common value for all users of an application. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ApplicationScopedSettingAttribute : SettingAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ApplicationScopedSettingAttribute" /> class. </summary>
	public ApplicationScopedSettingAttribute()
	{
	}
}
