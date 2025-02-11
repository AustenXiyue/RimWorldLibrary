using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal;
using MS.Internal.Security.RightsManagement;

namespace System.Security.RightsManagement;

/// <summary>Represents an unsigned rights managed <see cref="T:System.Security.RightsManagement.PublishLicense" /> or an unsigned <see cref="T:System.Security.RightsManagement.PublishLicense" /> template.</summary>
public class UnsignedPublishLicense
{
	private Guid _contentId;

	private ContentUser _owner;

	private ICollection<ContentGrant> _grantCollection;

	private string _referralInfoName;

	private Uri _referralInfoUri;

	private IDictionary<int, LocalizedNameDescriptionPair> _localizedNameDescriptionDictionary;

	private IDictionary<string, string> _applicationSpecificDataDictionary;

	private int _rightValidityIntervalDays;

	private RevocationPoint _revocationPoint;

	/// <summary>Gets or sets the content owner. </summary>
	/// <returns>The user who owns the published content.</returns>
	public ContentUser Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	/// <summary>Gets or sets the contact name for the author or publisher of the content.</summary>
	/// <returns>The contact name for the author or publisher of the content.</returns>
	public string ReferralInfoName
	{
		get
		{
			return _referralInfoName;
		}
		set
		{
			_referralInfoName = value;
		}
	}

	/// <summary>Gets or sets the contact URI for the author or publisher of the content.</summary>
	/// <returns>The contact uniform resource identifier (URI) for the author or publisher of the content.</returns>
	public Uri ReferralInfoUri
	{
		get
		{
			return _referralInfoUri;
		}
		set
		{
			_referralInfoUri = value;
		}
	}

	/// <summary>Gets or sets the publisher-created content identifier. </summary>
	/// <returns>The publisher-created rights-managed content identifier.</returns>
	public Guid ContentId
	{
		get
		{
			return _contentId;
		}
		set
		{
			_contentId = value;
		}
	}

	/// <summary>Gets a collection of assigned user rights.</summary>
	/// <returns>A collection of the assigned user rights that are provided in this license.</returns>
	public ICollection<ContentGrant> Grants => _grantCollection;

	/// <summary>Gets a collection of name and description pairs.</summary>
	/// <returns>A collection of name and description pairs.</returns>
	public IDictionary<int, LocalizedNameDescriptionPair> LocalizedNameDescriptionDictionary
	{
		get
		{
			if (_localizedNameDescriptionDictionary == null)
			{
				_localizedNameDescriptionDictionary = new Dictionary<int, LocalizedNameDescriptionPair>(10);
			}
			return _localizedNameDescriptionDictionary;
		}
	}

	internal int RightValidityIntervalDays
	{
		get
		{
			return _rightValidityIntervalDays;
		}
		set
		{
			_rightValidityIntervalDays = value;
		}
	}

	internal IDictionary<string, string> ApplicationSpecificDataDictionary
	{
		get
		{
			if (_applicationSpecificDataDictionary == null)
			{
				_applicationSpecificDataDictionary = new Dictionary<string, string>(5);
			}
			return _applicationSpecificDataDictionary;
		}
	}

	internal RevocationPoint RevocationPoint
	{
		get
		{
			return _revocationPoint;
		}
		set
		{
			_revocationPoint = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.UnsignedPublishLicense" /> class. </summary>
	public UnsignedPublishLicense()
	{
		_grantCollection = new Collection<ContentGrant>();
		_contentId = Guid.NewGuid();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.UnsignedPublishLicense" /> class from a specified XrML publish-license template. </summary>
	/// <param name="publishLicenseTemplate">The Extensible Rights Markup Language (XrML) publish-license template to use to create this license.</param>
	public UnsignedPublishLicense(string publishLicenseTemplate)
		: this()
	{
		if (publishLicenseTemplate == null)
		{
			throw new ArgumentNullException("publishLicenseTemplate");
		}
		using IssuanceLicense issuanceLicense = new IssuanceLicense(DateTime.MinValue, DateTime.MaxValue, null, null, null, publishLicenseTemplate, SafeRightsManagementHandle.InvalidHandle, _contentId, null, null, null, 0, null);
		issuanceLicense.UpdateUnsignedPublishLicense(this);
	}

	/// <summary>Creates a signed <see cref="T:System.Security.RightsManagement.PublishLicense" /> and returns a <see cref="T:System.Security.RightsManagement.UseLicense" /> for the document author.</summary>
	/// <returns>The signed <see cref="T:System.Security.RightsManagement.PublishLicense" /> that is created by signing this <see cref="T:System.Security.RightsManagement.UnsignedPublishLicense" />.</returns>
	/// <param name="secureEnvironment">The secure environment for license activation and binding.</param>
	/// <param name="authorUseLicense">A returned <see cref="T:System.Security.RightsManagement.UseLicense" /> for the document author.</param>
	public PublishLicense Sign(SecureEnvironment secureEnvironment, out UseLicense authorUseLicense)
	{
		if (secureEnvironment == null)
		{
			throw new ArgumentNullException("secureEnvironment");
		}
		using IssuanceLicense issuanceLicense = new IssuanceLicense(owner: (_owner == null) ? secureEnvironment.User : _owner, validFrom: DateTime.MinValue, validUntil: DateTime.MaxValue, referralInfoName: _referralInfoName, referralInfoUri: _referralInfoUri, issuanceLicense: null, boundLicenseHandle: SafeRightsManagementHandle.InvalidHandle, contentId: _contentId, grantCollection: Grants, localizedNameDescriptionDictionary: LocalizedNameDescriptionDictionary, applicationSpecificDataDictionary: ApplicationSpecificDataDictionary, rightValidityIntervalDays: _rightValidityIntervalDays, revocationPoint: _revocationPoint);
		return secureEnvironment.ClientSession.SignIssuanceLicense(issuanceLicense, out authorUseLicense);
	}

	/// <summary>Returns a serialized template created from the XrML of the <see cref="T:System.Security.RightsManagement.UnsignedPublishLicense" />.</summary>
	/// <returns>A serialized template created from the XrML of the <see cref="T:System.Security.RightsManagement.UnsignedPublishLicense" />.</returns>
	public override string ToString()
	{
		using IssuanceLicense issuanceLicense = new IssuanceLicense(DateTime.MinValue, DateTime.MaxValue, _referralInfoName, _referralInfoUri, _owner, null, SafeRightsManagementHandle.InvalidHandle, _contentId, Grants, LocalizedNameDescriptionDictionary, ApplicationSpecificDataDictionary, _rightValidityIntervalDays, _revocationPoint);
		return issuanceLicense.ToString();
	}

	internal UnsignedPublishLicense(SafeRightsManagementHandle boundLicenseHandle, string publishLicenseTemplate)
		: this()
	{
		Invariant.Assert(!boundLicenseHandle.IsInvalid);
		Invariant.Assert(publishLicenseTemplate != null);
		using IssuanceLicense issuanceLicense = new IssuanceLicense(DateTime.MinValue, DateTime.MaxValue, null, null, null, publishLicenseTemplate, boundLicenseHandle, _contentId, null, null, null, 0, null);
		issuanceLicense.UpdateUnsignedPublishLicense(this);
	}
}
