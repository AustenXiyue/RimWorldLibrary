namespace System.Security.Authentication.ExtendedProtection;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> enumeration specifies when the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> should be enforced.</summary>
public enum PolicyEnforcement
{
	/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> is never enforced and extended protection is disabled.</summary>
	Never,
	/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> is enforced only if the client and server supports extended protection.</summary>
	WhenSupported,
	/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> is always enforced. Clients that don’t support extended protection will fail to authenticate.</summary>
	Always
}
