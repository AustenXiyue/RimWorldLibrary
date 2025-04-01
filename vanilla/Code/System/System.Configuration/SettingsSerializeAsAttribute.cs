namespace System.Configuration;

/// <summary>Specifies the serialization mechanism that the settings provider should use. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class SettingsSerializeAsAttribute : Attribute
{
	private SettingsSerializeAs serializeAs;

	/// <summary>Gets the <see cref="T:System.Configuration.SettingsSerializeAs" /> enumeration value that specifies the serialization scheme.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsSerializeAs" /> enumerated value that specifies the serialization scheme.</returns>
	/// <filterpriority>2</filterpriority>
	public SettingsSerializeAs SerializeAs => serializeAs;

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.SettingsSerializeAsAttribute" /> class.</summary>
	/// <param name="serializeAs">A <see cref="T:System.Configuration.SettingsSerializeAs" /> enumerated value that specifies the serialization scheme.</param>
	public SettingsSerializeAsAttribute(SettingsSerializeAs serializeAs)
	{
		this.serializeAs = serializeAs;
	}
}
