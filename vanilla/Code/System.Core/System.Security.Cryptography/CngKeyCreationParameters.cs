using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Contains advanced properties for key creation.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CngKeyCreationParameters
{
	private CngExportPolicies? m_exportPolicy;

	private CngKeyCreationOptions m_keyCreationOptions;

	private CngKeyUsages? m_keyUsage;

	private CngPropertyCollection m_parameters = new CngPropertyCollection();

	private IntPtr m_parentWindowHandle;

	private CngProvider m_provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;

	private CngUIPolicy m_uiPolicy;

	/// <summary>Gets or sets the key export policy.</summary>
	/// <returns>An object that specifies a key export policy. The default value is null, which indicates that the key storage provider's default export policy is set.</returns>
	public CngExportPolicies? ExportPolicy
	{
		get
		{
			return m_exportPolicy;
		}
		set
		{
			m_exportPolicy = value;
		}
	}

	/// <summary>Gets or sets the key creation options.</summary>
	/// <returns>An object that specifies options for creating keys. The default value is null, which indicates that the key storage provider's default key creation options are set.</returns>
	public CngKeyCreationOptions KeyCreationOptions
	{
		get
		{
			return m_keyCreationOptions;
		}
		set
		{
			m_keyCreationOptions = value;
		}
	}

	/// <summary>Gets or sets the cryptographic operations that apply to the current key. </summary>
	/// <returns>A bitwise combination of one or more enumeration values that specify key usage. The default value is null, which indicates that the key storage provider's default key usage is set.</returns>
	public CngKeyUsages? KeyUsage
	{
		get
		{
			return m_keyUsage;
		}
		set
		{
			m_keyUsage = value;
		}
	}

	/// <summary>Gets or sets the window handle that should be used as the parent window for dialog boxes that are created by Cryptography Next Generation (CNG) classes.</summary>
	/// <returns>The HWND of the parent window that is used for CNG dialog boxes.</returns>
	public IntPtr ParentWindowHandle
	{
		get
		{
			return m_parentWindowHandle;
		}
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
		set
		{
			m_parentWindowHandle = value;
		}
	}

	/// <summary>Enables a <see cref="T:System.Security.Cryptography.CngKey" /> object to be created with additional properties that are set before the key is finalized.</summary>
	/// <returns>A collection object that contains any additional parameters that you must set on a <see cref="T:System.Security.Cryptography.CngKey" /> object during key creation.</returns>
	public CngPropertyCollection Parameters
	{
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
		get
		{
			return m_parameters;
		}
	}

	internal CngPropertyCollection ParametersNoDemand => m_parameters;

	/// <summary>Gets or sets the key storage provider (KSP) to create a key in.</summary>
	/// <returns>An object that specifies the KSP that a new key will be created in.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Cryptography.CngKeyCreationParameters.Provider" /> property is set to a null value.</exception>
	public CngProvider Provider
	{
		get
		{
			return m_provider;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_provider = value;
		}
	}

	/// <summary>Gets or sets information about the user interface to display when a key is created or accessed.</summary>
	/// <returns>An object that contains details about the user interface shown by Cryptography Next Generation (CNG) classes when a key is created or accessed. A null value indicates that the key storage provider's default user interface policy is set.</returns>
	public CngUIPolicy UIPolicy
	{
		get
		{
			return m_uiPolicy;
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, UI = true)]
		[UIPermission(SecurityAction.Demand, Window = UIPermissionWindow.SafeSubWindows)]
		set
		{
			m_uiPolicy = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngKeyCreationParameters" /> class.</summary>
	public CngKeyCreationParameters()
	{
	}
}
