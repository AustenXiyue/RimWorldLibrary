namespace System.Configuration;

/// <summary>Indicates that an application settings property has a special significance. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class SpecialSettingAttribute : Attribute
{
	private SpecialSetting setting;

	/// <summary>Gets the value describing the special setting category of the application settings property.</summary>
	/// <returns>A <see cref="T:System.Configuration.SpecialSetting" /> enumeration value defining the category of the application settings property.</returns>
	/// <filterpriority>2</filterpriority>
	public SpecialSetting SpecialSetting => setting;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SpecialSettingAttribute" /> class.</summary>
	/// <param name="specialSetting">A <see cref="T:System.Configuration.SpecialSetting" /> enumeration value defining the category of the application settings property.</param>
	public SpecialSettingAttribute(SpecialSetting specialSetting)
	{
		setting = specialSetting;
	}
}
