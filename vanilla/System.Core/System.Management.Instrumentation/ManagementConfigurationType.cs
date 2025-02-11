namespace System.Management.Instrumentation;

/// <summary>Represents the possible commit behaviors of a read/write property. It is used as the value of a parameter of the <see cref="T:System.Management.Instrumentation.ManagementConfigurationAttribute" /> attribute.</summary>
public enum ManagementConfigurationType
{
	/// <summary>Set values take effect only when Commit is called.</summary>
	Apply,
	/// <summary>Set values are applied immediately.</summary>
	OnCommit
}
