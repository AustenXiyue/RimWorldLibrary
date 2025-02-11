namespace System.Security;

/// <summary>Represents the type of manifest that the signature information applies to.</summary>
[Flags]
public enum ManifestKinds
{
	/// <summary>The manifest is for an application. </summary>
	Application = 2,
	/// <summary>The manifest is for deployment and application. The is the default value for verifying signatures. </summary>
	ApplicationAndDeployment = 3,
	/// <summary>The manifest is for deployment only.</summary>
	Deployment = 1,
	/// <summary>The manifest is of no particular type. </summary>
	None = 0
}
