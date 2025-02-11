using System.Collections.Generic;
using System.Security.RightsManagement;
using MS.Internal.IO.Packaging.CompoundFile;

namespace System.IO.Packaging;

/// <summary>Represents Digital Rights Management (DRM) information that is stored in an <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />.</summary>
public class RightsManagementInformation
{
	private RightsManagementEncryptionTransform _rmet;

	/// <summary>Gets or sets the <see cref="T:System.Security.RightsManagement.CryptoProvider" /> for accessing the package's encrypted rights management data stream.</summary>
	/// <returns>The <see cref="T:System.Security.RightsManagement.CryptoProvider" /> for accessing the rights management information.</returns>
	public CryptoProvider CryptoProvider
	{
		get
		{
			return _rmet.CryptoProvider;
		}
		set
		{
			_rmet.CryptoProvider = value;
		}
	}

	internal RightsManagementInformation(RightsManagementEncryptionTransform rmet)
	{
		_rmet = rmet;
	}

	/// <summary>Returns the embedded <see cref="T:System.Security.RightsManagement.PublishLicense" /> from the encrypted rights management data stream.</summary>
	/// <returns>The embedded <see cref="T:System.Security.RightsManagement.PublishLicense" />; or null, if the package does not contain a <see cref="T:System.Security.RightsManagement.PublishLicense" />.</returns>
	/// <exception cref="T:System.IO.FileFormatException">The rights management information in this package cannot be read by the current version of this class.</exception>
	public PublishLicense LoadPublishLicense()
	{
		return _rmet.LoadPublishLicense();
	}

	/// <summary>Saves a given <see cref="T:System.Security.RightsManagement.PublishLicense" /> to the encrypted rights management data stream.</summary>
	/// <param name="publishLicense">The publish license to store and embed in the package.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="publishLicense" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The rights management information in this package cannot be read by the current version of this class.</exception>
	public void SavePublishLicense(PublishLicense publishLicense)
	{
		_rmet.SavePublishLicense(publishLicense);
	}

	/// <summary>Returns a specified user's embedded <see cref="T:System.Security.RightsManagement.UseLicense" /> from the encrypted rights management data stream.</summary>
	/// <returns>The <see cref="T:System.Security.RightsManagement.UseLicense" /> for the specified user; or null, if the package does not contain a <see cref="T:System.Security.RightsManagement.UseLicense" /> that matches the given <paramref name="userKey" />.</returns>
	/// <param name="userKey">The user or user group to return the <see cref="T:System.Security.RightsManagement.UseLicense" /> for.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="userKey" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The rights management information in this package cannot be read by the current version of this class.</exception>
	public UseLicense LoadUseLicense(ContentUser userKey)
	{
		return _rmet.LoadUseLicense(userKey);
	}

	/// <summary>Saves a given <see cref="T:System.Security.RightsManagement.UseLicense" /> for a specified user to the encrypted rights management data stream.</summary>
	/// <param name="userKey">The user of the <see cref="T:System.Security.RightsManagement.UseLicense" />.</param>
	/// <param name="useLicense">The use license to store and embed in the package.</param>
	/// <exception cref="T:System.ArgumentNullException">Either the <paramref name="userKey" /> or <paramref name="useLicense" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The rights management information in this package cannot be read by the current version of this class.</exception>
	public void SaveUseLicense(ContentUser userKey, UseLicense useLicense)
	{
		_rmet.SaveUseLicense(userKey, useLicense);
	}

	/// <summary>Deletes the <see cref="T:System.Security.RightsManagement.UseLicense" /> for a specified user from the encrypted rights management data stream.</summary>
	/// <param name="userKey">The user of the <see cref="T:System.Security.RightsManagement.UseLicense" />   to be deleted.</param>
	public void DeleteUseLicense(ContentUser userKey)
	{
		_rmet.DeleteUseLicense(userKey);
	}

	/// <summary>Returns a dictionary collection of user and <see cref="T:System.Security.RightsManagement.UseLicense" /> key/value pairs from the encrypted rights management data stream.</summary>
	/// <returns>A collection of user and <see cref="T:System.Security.RightsManagement.UseLicense" /> key/value pairs that are contained in the rights managed protected package.</returns>
	public IDictionary<ContentUser, UseLicense> GetEmbeddedUseLicenses()
	{
		return _rmet.GetEmbeddedUseLicenses();
	}
}
