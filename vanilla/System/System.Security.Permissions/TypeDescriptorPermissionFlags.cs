namespace System.Security.Permissions;

/// <summary>Defines permission settings for type descriptors. </summary>
[Serializable]
[Flags]
public enum TypeDescriptorPermissionFlags
{
	/// <summary>No permission flags are set on the type descriptor.</summary>
	NoFlags = 0,
	/// <summary>The type descriptor may be called from partially trusted code.</summary>
	RestrictedRegistrationAccess = 1
}
