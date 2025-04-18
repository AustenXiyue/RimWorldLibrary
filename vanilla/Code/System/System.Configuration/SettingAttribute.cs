namespace System.Configuration;

/// <summary>Represents a custom settings attribute used to associate settings information with a settings property.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingAttribute" /> class. </summary>
	public SettingAttribute()
	{
	}
}
