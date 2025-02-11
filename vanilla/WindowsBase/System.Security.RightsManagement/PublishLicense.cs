using MS.Internal.Security.RightsManagement;

namespace System.Security.RightsManagement;

/// <summary>Represents a signed rights managed publish license.</summary>
public class PublishLicense
{
	private string _serializedPublishLicense;

	private string _referralInfoName;

	private Uri _referralInfoUri;

	private Guid _contentId;

	private Uri _useLicenseAcquisitionUriFromPublishLicense;

	/// <summary>Gets the contact name for the author or publisher of the content.</summary>
	/// <returns>The contact name for the author or publisher of the content.</returns>
	public string ReferralInfoName => _referralInfoName;

	/// <summary>Gets the contact URI for the author or publisher of the content.</summary>
	/// <returns>The contact uniform resource identifier (URI) for the author or publisher of the content.</returns>
	public Uri ReferralInfoUri => _referralInfoUri;

	/// <summary>Gets the publisher-created content identifier. </summary>
	/// <returns>The publisher-created content identifier.</returns>
	public Guid ContentId => _contentId;

	/// <summary>Gets the URI to use for acquiring a <see cref="T:System.Security.RightsManagement.UseLicense" />.</summary>
	/// <returns>The URI to use for acquiring a <see cref="T:System.Security.RightsManagement.UseLicense" />.</returns>
	public Uri UseLicenseAcquisitionUrl => _useLicenseAcquisitionUriFromPublishLicense;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.PublishLicense" /> class from a specified serialized and signed <see cref="T:System.Security.RightsManagement.PublishLicense" />.</summary>
	/// <param name="signedPublishLicense">A signed and serialized publish license.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="signedPublishLicense" /> is null.</exception>
	/// <exception cref="T:System.Security.RightsManagement.RightsManagementException">The license is invalid.</exception>
	public PublishLicense(string signedPublishLicense)
	{
		if (signedPublishLicense == null)
		{
			throw new ArgumentNullException("signedPublishLicense");
		}
		_serializedPublishLicense = signedPublishLicense;
		_useLicenseAcquisitionUriFromPublishLicense = ClientSession.GetUseLicenseAcquisitionUriFromPublishLicense(_serializedPublishLicense);
		if (_useLicenseAcquisitionUriFromPublishLicense == null)
		{
			throw new RightsManagementException(RightsManagementFailureCode.InvalidLicense);
		}
		string contentIdFromPublishLicense = ClientSession.GetContentIdFromPublishLicense(_serializedPublishLicense);
		if (contentIdFromPublishLicense == null)
		{
			throw new RightsManagementException(RightsManagementFailureCode.InvalidLicense);
		}
		_contentId = new Guid(contentIdFromPublishLicense);
		ClientSession.GetReferralInfoFromPublishLicense(_serializedPublishLicense, out _referralInfoName, out _referralInfoUri);
	}

	/// <summary>Returns a decrypted <see cref="T:System.Security.RightsManagement.UnsignedPublishLicense" /> version of this signed <see cref="T:System.Security.RightsManagement.PublishLicense" />.</summary>
	/// <returns>A decrypted, unsigned version of this license.</returns>
	/// <param name="cryptoProvider">The rights management service to use for decrypting the license.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cryptoProvider" /> is null.</exception>
	public UnsignedPublishLicense DecryptUnsignedPublishLicense(CryptoProvider cryptoProvider)
	{
		if (cryptoProvider == null)
		{
			throw new ArgumentNullException("cryptoProvider");
		}
		return cryptoProvider.DecryptPublishLicense(_serializedPublishLicense);
	}

	/// <summary>Returns the serialized XrML string that was used to create this license.</summary>
	/// <returns>The serialized Extensible Rights Markup Language (XrML) string that was used to create this license.</returns>
	public override string ToString()
	{
		return _serializedPublishLicense;
	}

	/// <summary>Attempts to acquire a <see cref="T:System.Security.RightsManagement.UseLicense" /> for a user or user group in a specified <see cref="T:System.Security.RightsManagement.SecureEnvironment" />.</summary>
	/// <returns>The <see cref="T:System.Security.RightsManagement.UseLicense" /> for a user or user group in the specified <paramref name="secureEnvironment" />.</returns>
	/// <param name="secureEnvironment">The secure environment for license activation and binding.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="secureEnvironment" /> is null.</exception>
	/// <exception cref="T:System.Security.RightsManagement.RightsManagementException">The authentication failed.</exception>
	public UseLicense AcquireUseLicense(SecureEnvironment secureEnvironment)
	{
		if (secureEnvironment == null)
		{
			throw new ArgumentNullException("secureEnvironment");
		}
		return secureEnvironment.ClientSession.AcquireUseLicense(_serializedPublishLicense, noUI: false);
	}

	/// <summary>Attempts to acquire a <see cref="T:System.Security.RightsManagement.UseLicense" /> for a user or user group in a specified <see cref="T:System.Security.RightsManagement.SecureEnvironment" />.</summary>
	/// <returns>The <see cref="T:System.Security.RightsManagement.UseLicense" /> for a user or user group in the specified <paramref name="secureEnvironment" />.</returns>
	/// <param name="secureEnvironment">The secure environment for license activation and binding.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="secureEnvironment" /> is null.</exception>
	/// <exception cref="T:System.Security.RightsManagement.RightsManagementException">The authentication failed.</exception>
	public UseLicense AcquireUseLicenseNoUI(SecureEnvironment secureEnvironment)
	{
		if (secureEnvironment == null)
		{
			throw new ArgumentNullException("secureEnvironment");
		}
		return secureEnvironment.ClientSession.AcquireUseLicense(_serializedPublishLicense, noUI: true);
	}
}
