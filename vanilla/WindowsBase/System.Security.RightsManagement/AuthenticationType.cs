namespace System.Security.RightsManagement;

/// <summary>Specifies the method of rights management authentication.</summary>
public enum AuthenticationType
{
	/// <summary>Windows authentication in a corporate domain environment.</summary>
	Windows,
	/// <summary>Windows Live ID authentication.</summary>
	Passport,
	/// <summary>Either Windows authentication or Windows Live ID authentication.</summary>
	WindowsPassport,
	/// <summary>Implicit authentication to any requesting user.</summary>
	Internal
}
