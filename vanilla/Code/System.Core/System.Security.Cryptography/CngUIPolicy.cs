using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Encapsulates optional configuration parameters for the user interface (UI) that Cryptography Next Generation (CNG) displays when you access a protected key.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CngUIPolicy
{
	private string m_creationTitle;

	private string m_description;

	private string m_friendlyName;

	private CngUIProtectionLevels m_protectionLevel;

	private string m_useContext;

	/// <summary>Gets the title that is displayed by the UI prompt.</summary>
	/// <returns>The title of the dialog box that appears when the key is accessed.</returns>
	public string CreationTitle => m_creationTitle;

	/// <summary>Gets the description string that is displayed by the UI prompt.</summary>
	/// <returns>The description text for the dialog box that appears when the key is accessed.</returns>
	public string Description => m_description;

	/// <summary>Gets the friendly name that is displayed by the UI prompt.</summary>
	/// <returns>The friendly name that is used to describe the key in the dialog box that appears when the key is accessed.</returns>
	public string FriendlyName => m_friendlyName;

	/// <summary>Gets the UI protection level for the key.</summary>
	/// <returns>An object that describes the level of UI protection to apply to the key.</returns>
	public CngUIProtectionLevels ProtectionLevel => m_protectionLevel;

	/// <summary>Gets the description of how the key will be used.</summary>
	/// <returns>The description of how the key will be used.</returns>
	public string UseContext => m_useContext;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngUIPolicy" /> class by using the specified protection level.</summary>
	/// <param name="protectionLevel">A bitwise combination of the enumeration values that specify the protection level.</param>
	public CngUIPolicy(CngUIProtectionLevels protectionLevel)
		: this(protectionLevel, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngUIPolicy" /> class by using the specified protection level and friendly name.</summary>
	/// <param name="protectionLevel">A bitwise combination of the enumeration values that specify the protection level.  </param>
	/// <param name="friendlyName">A friendly name for the key to be used in the UI prompt. Specify a null string to use the default name.</param>
	public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName)
		: this(protectionLevel, friendlyName, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngUIPolicy" /> class by using the specified protection level, friendly name, and description.</summary>
	/// <param name="protectionLevel">A bitwise combination of the enumeration values that specify the protection level.  </param>
	/// <param name="friendlyName">A friendly name for the key to be used in the UI prompt. Specify a null string to use the default name.</param>
	/// <param name="description">The full-text description of the key. Specify a null string to use the default description.</param>
	public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description)
		: this(protectionLevel, friendlyName, description, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngUIPolicy" /> class by using the specified protection level, friendly name, description string, and use context.</summary>
	/// <param name="protectionLevel">A bitwise combination of the enumeration values that specify the protection level.  </param>
	/// <param name="friendlyName">A friendly name for the key to be used in the UI prompt. Specify a null string to use the default name.</param>
	/// <param name="description">The full-text description of the key. Specify a null string to use the default description.</param>
	/// <param name="useContext">A description of how the key will be used. Specify a null string to use the default description.</param>
	public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description, string useContext)
		: this(protectionLevel, friendlyName, description, useContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngUIPolicy" /> class by using the specified protection level, friendly name, description string, use context, and title.</summary>
	/// <param name="protectionLevel">A bitwise combination of the enumeration values that specify the protection level.  </param>
	/// <param name="friendlyName">A friendly name for the key to be used in the UI prompt. Specify a null string to use the default name.</param>
	/// <param name="description">The full-text description of the key. Specify a null string to use the default description.</param>
	/// <param name="useContext">A description of how the key will be used. Specify a null string to use the default description.</param>
	/// <param name="creationTitle">The title for the dialog box that provides the UI prompt. Specify a null string to use the default title.</param>
	public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description, string useContext, string creationTitle)
	{
		m_creationTitle = creationTitle;
		m_description = description;
		m_friendlyName = friendlyName;
		m_protectionLevel = protectionLevel;
		m_useContext = useContext;
	}
}
