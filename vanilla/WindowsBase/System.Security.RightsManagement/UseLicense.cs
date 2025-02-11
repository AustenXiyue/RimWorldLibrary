using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal.Security.RightsManagement;

namespace System.Security.RightsManagement;

/// <summary>Represents a license that enables access to protected rights managed content.    </summary>
public class UseLicense
{
	private string _serializedUseLicense;

	private Guid _contentId;

	private ContentUser _owner;

	private IDictionary<string, string> _applicationSpecificDataDictionary;

	/// <summary>Gets the owner of the license. </summary>
	/// <returns>The owner of the license. </returns>
	public ContentUser Owner => _owner;

	/// <summary>Gets the content identifier created by the publisher.</summary>
	/// <returns>The content identifier created by the publisher.</returns>
	public Guid ContentId => _contentId;

	/// <summary>Gets the application data dictionary that contains key/value pairs passed from the publishing application to the consuming application.</summary>
	/// <returns>The application data dictionary that contains key/value pairs passed from the publishing application to the consuming application.</returns>
	public IDictionary<string, string> ApplicationData => _applicationSpecificDataDictionary;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.UseLicense" /> class. </summary>
	/// <param name="useLicense">A use license in serialized Extensible Rights Markup Language (XrML) form.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="useLicense" /> is null.</exception>
	/// <exception cref="T:System.Security.RightsManagement.RightsManagementException">
	///   <paramref name="useLicense" /> is invalid.</exception>
	public UseLicense(string useLicense)
	{
		if (useLicense == null)
		{
			throw new ArgumentNullException("useLicense");
		}
		_serializedUseLicense = useLicense;
		ClientSession.GetContentIdFromLicense(_serializedUseLicense, out var contentId, out var _);
		if (contentId == null)
		{
			throw new RightsManagementException(RightsManagementFailureCode.InvalidLicense);
		}
		_contentId = new Guid(contentId);
		_owner = ClientSession.ExtractUserFromCertificateChain(_serializedUseLicense);
		_applicationSpecificDataDictionary = new ReadOnlyDictionary<string, string>(ClientSession.ExtractApplicationSpecificDataFromLicense(_serializedUseLicense));
	}

	/// <summary>Returns the serialized XrML string used to create this license.</summary>
	/// <returns>The serialized Extensible Rights Markup Language (XrML) string originally passed to the <see cref="M:System.Security.RightsManagement.UseLicense.#ctor(System.String)" /> constructor.</returns>
	public override string ToString()
	{
		return _serializedUseLicense;
	}

	/// <summary>Binds the license to a given <see cref="T:System.Security.RightsManagement.SecureEnvironment" />.</summary>
	/// <returns>A <see cref="T:System.Security.RightsManagement.CryptoProvider" /> instance if the license binding succeeded; otherwise, null.</returns>
	/// <param name="secureEnvironment">The environment to bind the license to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="secureEnvironment" /> is null.</exception>
	public CryptoProvider Bind(SecureEnvironment secureEnvironment)
	{
		if (secureEnvironment == null)
		{
			throw new ArgumentNullException("secureEnvironment");
		}
		return secureEnvironment.ClientSession.TryBindUseLicenseToAllIdentites(_serializedUseLicense);
	}

	/// <summary>Indicates if this license is equivalent to another given license.</summary>
	/// <returns>true if both licenses are the equivalent; otherwise, false.</returns>
	/// <param name="x">The license to compare.</param>
	public override bool Equals(object x)
	{
		if (x == null)
		{
			return false;
		}
		if (x.GetType() != GetType())
		{
			return false;
		}
		UseLicense useLicense = (UseLicense)x;
		return string.CompareOrdinal(_serializedUseLicense, useLicense._serializedUseLicense) == 0;
	}

	/// <summary>Returns the hash code associated with this license.</summary>
	/// <returns>A hash code for this license. </returns>
	public override int GetHashCode()
	{
		return _serializedUseLicense.GetHashCode();
	}
}
