using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;

namespace MS.Internal.Security.RightsManagement;

internal class IssuanceLicense : IDisposable
{
	private List<SafeRightsManagementPubHandle> _pubHandlesList = new List<SafeRightsManagementPubHandle>(50);

	private SafeRightsManagementPubHandle _issuanceLicenseHandle;

	private const string DefaultContentType = "MS-GUID";

	private const string UnspecifiedAuthenticationType = "Unspecified";

	internal SafeRightsManagementPubHandle Handle
	{
		get
		{
			CheckDisposed();
			return _issuanceLicenseHandle;
		}
	}

	internal IssuanceLicense(DateTime validFrom, DateTime validUntil, string referralInfoName, Uri referralInfoUri, ContentUser owner, string issuanceLicense, SafeRightsManagementHandle boundLicenseHandle, Guid contentId, ICollection<ContentGrant> grantCollection, IDictionary<int, LocalizedNameDescriptionPair> localizedNameDescriptionDictionary, IDictionary<string, string> applicationSpecificDataDictionary, int rightValidityIntervalDays, RevocationPoint revocationPoint)
	{
		Initialize(validFrom, validUntil, referralInfoName, referralInfoUri, owner, issuanceLicense, boundLicenseHandle, contentId, grantCollection, localizedNameDescriptionDictionary, applicationSpecificDataDictionary, rightValidityIntervalDays, revocationPoint);
	}

	private void Initialize(DateTime validFrom, DateTime validUntil, string referralInfoName, Uri referralInfoUri, ContentUser owner, string issuanceLicense, SafeRightsManagementHandle boundLicenseHandle, Guid contentId, ICollection<ContentGrant> grantCollection, IDictionary<int, LocalizedNameDescriptionPair> localizedNameDescriptionDictionary, IDictionary<string, string> applicationSpecificDataDictionary, int rightValidityIntervalDays, RevocationPoint revocationPoint)
	{
		Invariant.Assert(boundLicenseHandle.IsInvalid || issuanceLicense != null);
		SystemTime timeFrom = null;
		SystemTime timeUntil = null;
		if (validFrom != DateTime.MinValue || validUntil != DateTime.MaxValue)
		{
			timeFrom = new SystemTime(validFrom);
			timeUntil = new SystemTime(validUntil);
		}
		string referralInfoUrl = null;
		if (referralInfoUri != null)
		{
			referralInfoUrl = referralInfoUri.ToString();
		}
		SafeRightsManagementPubHandle ownerUserHandle = ((owner == null) ? SafeRightsManagementPubHandle.InvalidHandle : GetHandleFromUser(owner));
		_issuanceLicenseHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMCreateIssuanceLicense(timeFrom, timeUntil, referralInfoName, referralInfoUrl, ownerUserHandle, issuanceLicense, boundLicenseHandle, out _issuanceLicenseHandle));
		Invariant.Assert(_issuanceLicenseHandle != null && !_issuanceLicenseHandle.IsInvalid);
		if (rightValidityIntervalDays > 0)
		{
			SafeNativeMethods.DRMSetIntervalTime(_issuanceLicenseHandle, (uint)rightValidityIntervalDays);
		}
		if (grantCollection != null)
		{
			foreach (ContentGrant item in grantCollection)
			{
				AddGrant(item);
			}
		}
		if (localizedNameDescriptionDictionary != null)
		{
			foreach (KeyValuePair<int, LocalizedNameDescriptionPair> item2 in localizedNameDescriptionDictionary)
			{
				AddNameDescription(item2.Key, item2.Value);
			}
		}
		if (applicationSpecificDataDictionary != null)
		{
			foreach (KeyValuePair<string, string> item3 in applicationSpecificDataDictionary)
			{
				AddApplicationSpecificData(item3.Key, item3.Value);
			}
		}
		if (contentId != Guid.Empty)
		{
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMSetMetaData(_issuanceLicenseHandle, contentId.ToString("B"), "MS-GUID", null, null, null, null));
		}
		if (revocationPoint != null)
		{
			SetRevocationPoint(revocationPoint);
		}
	}

	public override string ToString()
	{
		uint issuanceLicenseTemplateLength = 0u;
		StringBuilder stringBuilder = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetIssuanceLicenseTemplate(_issuanceLicenseHandle, ref issuanceLicenseTemplateLength, null));
		stringBuilder = new StringBuilder(checked((int)issuanceLicenseTemplateLength));
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetIssuanceLicenseTemplate(_issuanceLicenseHandle, ref issuanceLicenseTemplateLength, stringBuilder));
		return stringBuilder.ToString();
	}

	internal void UpdateUnsignedPublishLicense(UnsignedPublishLicense unsignedPublishLicense)
	{
		Invariant.Assert(unsignedPublishLicense != null);
		DistributionPointInfo distributionPointInfo = DistributionPointInfo.ReferralInfo;
		GetIssuanceLicenseInfo(out var _, out var _, distributionPointInfo, out var distributionPointName, out var distributionPointUri, out var owner, out var _);
		unsignedPublishLicense.ReferralInfoName = distributionPointName;
		if (distributionPointUri != null)
		{
			unsignedPublishLicense.ReferralInfoUri = new Uri(distributionPointUri);
		}
		else
		{
			unsignedPublishLicense.ReferralInfoUri = null;
		}
		unsignedPublishLicense.Owner = owner;
		uint days = 0u;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetIntervalTime(_issuanceLicenseHandle, ref days));
		unsignedPublishLicense.RightValidityIntervalDays = checked((int)days);
		int num = 0;
		while (true)
		{
			SafeRightsManagementPubHandle userHandle = null;
			ContentUser issuanceLicenseUser = GetIssuanceLicenseUser(num, out userHandle);
			if (issuanceLicenseUser == null || userHandle == null)
			{
				break;
			}
			int num2 = 0;
			while (true)
			{
				SafeRightsManagementPubHandle rightHandle = null;
				DateTime validFrom;
				DateTime validUntil;
				ContentRight? issuanceLicenseUserRight = GetIssuanceLicenseUserRight(userHandle, num2, out rightHandle, out validFrom, out validUntil);
				if (rightHandle == null)
				{
					break;
				}
				if (issuanceLicenseUserRight.HasValue)
				{
					unsignedPublishLicense.Grants.Add(new ContentGrant(issuanceLicenseUser, issuanceLicenseUserRight.Value, validFrom, validUntil));
				}
				num2++;
			}
			num++;
		}
		int num3 = 0;
		while (true)
		{
			int localeId;
			LocalizedNameDescriptionPair localizedNameDescriptionPair = GetLocalizedNameDescriptionPair(num3, out localeId);
			if (localizedNameDescriptionPair == null)
			{
				break;
			}
			unsignedPublishLicense.LocalizedNameDescriptionDictionary.Add(localeId, localizedNameDescriptionPair);
			num3++;
		}
		int num4 = 0;
		while (true)
		{
			KeyValuePair<string, string>? applicationSpecificData = GetApplicationSpecificData(num4);
			if (!applicationSpecificData.HasValue)
			{
				break;
			}
			unsignedPublishLicense.ApplicationSpecificDataDictionary.Add(applicationSpecificData.Value.Key, applicationSpecificData.Value.Value);
			num4++;
		}
		unsignedPublishLicense.RevocationPoint = GetRevocationPoint();
	}

	~IssuanceLicense()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		try
		{
			if (_issuanceLicenseHandle == null)
			{
				return;
			}
			_issuanceLicenseHandle.Dispose();
			foreach (SafeRightsManagementPubHandle pubHandles in _pubHandlesList)
			{
				pubHandles.Dispose();
			}
		}
		finally
		{
			_issuanceLicenseHandle = null;
			_pubHandlesList.Clear();
		}
	}

	private void AddGrant(ContentGrant grant)
	{
		Invariant.Assert(grant != null);
		Invariant.Assert(grant.User != null);
		SafeRightsManagementPubHandle rightHandle = GetRightHandle(grant);
		SafeRightsManagementPubHandle handleFromUser = GetHandleFromUser(grant.User);
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMAddRightWithUser(_issuanceLicenseHandle, rightHandle, handleFromUser));
	}

	private void AddNameDescription(int localeId, LocalizedNameDescriptionPair nameDescription)
	{
		uint localeId2 = checked((uint)localeId);
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMSetNameAndDescription(_issuanceLicenseHandle, flagDelete: false, localeId2, nameDescription.Name, nameDescription.Description));
	}

	private void AddApplicationSpecificData(string name, string value)
	{
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMSetApplicationSpecificData(_issuanceLicenseHandle, flagDelete: false, name, value));
	}

	private SafeRightsManagementPubHandle GetRightHandle(ContentGrant grant)
	{
		SafeRightsManagementPubHandle rightHandle = null;
		SystemTime timeFrom = null;
		SystemTime timeUntil = null;
		if (grant.ValidFrom != DateTime.MinValue || grant.ValidUntil != DateTime.MaxValue)
		{
			timeFrom = new SystemTime(grant.ValidFrom);
			timeUntil = new SystemTime(grant.ValidUntil);
		}
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMCreateRight(ClientSession.GetStringFromRight(grant.Right), timeFrom, timeUntil, 0u, null, null, out rightHandle));
		_pubHandlesList.Add(rightHandle);
		return rightHandle;
	}

	private SafeRightsManagementPubHandle GetHandleFromUser(ContentUser user)
	{
		SafeRightsManagementPubHandle userHandle = null;
		int hr = ((!user.GenericEquals(ContentUser.AnyoneUser) && !user.GenericEquals(ContentUser.OwnerUser)) ? SafeNativeMethods.DRMCreateUser(user.Name, null, ConvertAuthenticationTypeToString(user), out userHandle) : SafeNativeMethods.DRMCreateUser(user.Name, user.Name, ConvertAuthenticationTypeToString(user), out userHandle));
		Errors.ThrowOnErrorCode(hr);
		_pubHandlesList.Add(userHandle);
		return userHandle;
	}

	private static ContentRight? GetRightFromHandle(SafeRightsManagementPubHandle rightHandle, out DateTime validFrom, out DateTime validUntil)
	{
		uint rightNameLength = 0u;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetRightInfo(rightHandle, ref rightNameLength, null, null, null));
		StringBuilder stringBuilder = new StringBuilder(checked((int)rightNameLength));
		SystemTime systemTime = new SystemTime(DateTime.Now);
		SystemTime systemTime2 = new SystemTime(DateTime.Now);
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetRightInfo(rightHandle, ref rightNameLength, stringBuilder, systemTime, systemTime2));
		validFrom = systemTime.GetDateTime(DateTime.MinValue);
		validUntil = systemTime2.GetDateTime(DateTime.MaxValue);
		return ClientSession.GetRightFromString(stringBuilder.ToString());
	}

	private static ContentUser GetUserFromHandle(SafeRightsManagementPubHandle userHandle)
	{
		uint userNameLength = 0u;
		StringBuilder stringBuilder = null;
		uint userIdLength = 0u;
		StringBuilder stringBuilder2 = null;
		uint userIdTypeLength = 0u;
		StringBuilder stringBuilder3 = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUserInfo(userHandle, ref userNameLength, null, ref userIdLength, null, ref userIdTypeLength, null));
		checked
		{
			if (userNameLength != 0)
			{
				stringBuilder = new StringBuilder((int)userNameLength);
			}
			if (userIdLength != 0)
			{
				stringBuilder2 = new StringBuilder((int)userIdLength);
			}
			if (userIdTypeLength != 0)
			{
				stringBuilder3 = new StringBuilder((int)userIdTypeLength);
			}
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUserInfo(userHandle, ref userNameLength, stringBuilder, ref userIdLength, stringBuilder2, ref userIdTypeLength, stringBuilder3));
			string name = null;
			if (stringBuilder != null)
			{
				name = stringBuilder.ToString();
			}
			string strA = null;
			if (stringBuilder3 != null)
			{
				strA = stringBuilder3.ToString().ToUpperInvariant();
			}
			string name2 = null;
			if (stringBuilder2 != null)
			{
				name2 = stringBuilder2.ToString().ToUpperInvariant();
			}
			if (string.CompareOrdinal(strA, AuthenticationType.Windows.ToString().ToUpperInvariant()) == 0)
			{
				return new ContentUser(name, AuthenticationType.Windows);
			}
			if (string.CompareOrdinal(strA, AuthenticationType.Passport.ToString().ToUpperInvariant()) == 0)
			{
				return new ContentUser(name, AuthenticationType.Passport);
			}
			if (string.CompareOrdinal(strA, AuthenticationType.Internal.ToString().ToUpperInvariant()) == 0)
			{
				if (ContentUser.CompareToAnyone(name2))
				{
					return ContentUser.AnyoneUser;
				}
				if (ContentUser.CompareToOwner(name2))
				{
					return ContentUser.OwnerUser;
				}
			}
			else if (string.CompareOrdinal(strA, "Unspecified".ToUpperInvariant()) == 0)
			{
				return new ContentUser(name, AuthenticationType.WindowsPassport);
			}
			throw new RightsManagementException(RightsManagementFailureCode.InvalidLicense);
		}
	}

	private ContentUser GetIssuanceLicenseUser(int index, out SafeRightsManagementPubHandle userHandle)
	{
		Invariant.Assert(index >= 0);
		int num = SafeNativeMethods.DRMGetUsers(_issuanceLicenseHandle, (uint)index, out userHandle);
		if (num == -2147168461)
		{
			userHandle = null;
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		_pubHandlesList.Add(userHandle);
		return GetUserFromHandle(userHandle);
	}

	private LocalizedNameDescriptionPair GetLocalizedNameDescriptionPair(int index, out int localeId)
	{
		Invariant.Assert(index >= 0);
		uint localeId2 = 0u;
		uint nameLength = 0u;
		StringBuilder stringBuilder = null;
		uint descriptionLength = 0u;
		StringBuilder stringBuilder2 = null;
		int num = SafeNativeMethods.DRMGetNameAndDescription(_issuanceLicenseHandle, (uint)index, out localeId2, ref nameLength, stringBuilder, ref descriptionLength, stringBuilder2);
		if (num == -2147168461)
		{
			localeId = 0;
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		checked
		{
			if (nameLength != 0)
			{
				stringBuilder = new StringBuilder((int)nameLength);
			}
			if (descriptionLength != 0)
			{
				stringBuilder2 = new StringBuilder((int)descriptionLength);
			}
			num = SafeNativeMethods.DRMGetNameAndDescription(_issuanceLicenseHandle, unchecked((uint)index), out localeId2, ref nameLength, stringBuilder, ref descriptionLength, stringBuilder2);
			Errors.ThrowOnErrorCode(num);
			localeId = (int)localeId2;
			return new LocalizedNameDescriptionPair(stringBuilder?.ToString(), stringBuilder2?.ToString());
		}
	}

	private KeyValuePair<string, string>? GetApplicationSpecificData(int index)
	{
		Invariant.Assert(index >= 0);
		uint nameLength = 0u;
		uint valueLength = 0u;
		int num = SafeNativeMethods.DRMGetApplicationSpecificData(_issuanceLicenseHandle, (uint)index, ref nameLength, null, ref valueLength, null);
		if (num == -2147168461)
		{
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		StringBuilder stringBuilder = null;
		StringBuilder stringBuilder2;
		checked
		{
			if (nameLength != 0)
			{
				stringBuilder = new StringBuilder((int)nameLength);
			}
			stringBuilder2 = null;
			if (valueLength != 0)
			{
				stringBuilder2 = new StringBuilder((int)valueLength);
			}
		}
		num = SafeNativeMethods.DRMGetApplicationSpecificData(_issuanceLicenseHandle, (uint)index, ref nameLength, stringBuilder, ref valueLength, stringBuilder2);
		Errors.ThrowOnErrorCode(num);
		string key = stringBuilder?.ToString();
		string value = stringBuilder2?.ToString();
		return new KeyValuePair<string, string>(key, value);
	}

	private ContentRight? GetIssuanceLicenseUserRight(SafeRightsManagementPubHandle userHandle, int index, out SafeRightsManagementPubHandle rightHandle, out DateTime validFrom, out DateTime validUntil)
	{
		Invariant.Assert(index >= 0);
		int num = SafeNativeMethods.DRMGetUserRights(_issuanceLicenseHandle, userHandle, (uint)index, out rightHandle);
		if (num == -2147168461)
		{
			rightHandle = null;
			validFrom = DateTime.MinValue;
			validUntil = DateTime.MaxValue;
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		_pubHandlesList.Add(rightHandle);
		return GetRightFromHandle(rightHandle, out validFrom, out validUntil);
	}

	private void GetIssuanceLicenseInfo(out DateTime timeFrom, out DateTime timeUntil, DistributionPointInfo distributionPointInfo, out string distributionPointName, out string distributionPointUri, out ContentUser owner, out bool officialFlag)
	{
		uint distributionPointNameLength = 0u;
		uint distributionPointUriLength = 0u;
		bool officialFlag2 = false;
		SafeRightsManagementPubHandle ownerHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetIssuanceLicenseInfo(_issuanceLicenseHandle, null, null, (uint)distributionPointInfo, ref distributionPointNameLength, null, ref distributionPointUriLength, null, out ownerHandle, out officialFlag2));
		if (ownerHandle != null)
		{
			ownerHandle.Dispose();
			ownerHandle = null;
		}
		StringBuilder stringBuilder = null;
		StringBuilder stringBuilder2;
		SystemTime systemTime;
		SystemTime systemTime2;
		checked
		{
			if (distributionPointNameLength != 0)
			{
				stringBuilder = new StringBuilder((int)distributionPointNameLength);
			}
			stringBuilder2 = null;
			if (distributionPointUriLength != 0)
			{
				stringBuilder2 = new StringBuilder((int)distributionPointUriLength);
			}
			systemTime = new SystemTime(DateTime.Now);
			systemTime2 = new SystemTime(DateTime.Now);
		}
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetIssuanceLicenseInfo(_issuanceLicenseHandle, systemTime, systemTime2, (uint)distributionPointInfo, ref distributionPointNameLength, stringBuilder, ref distributionPointUriLength, stringBuilder2, out ownerHandle, out officialFlag2));
		timeFrom = systemTime.GetDateTime(DateTime.MinValue);
		timeUntil = systemTime2.GetDateTime(DateTime.MaxValue);
		if (stringBuilder != null)
		{
			distributionPointName = stringBuilder.ToString();
		}
		else
		{
			distributionPointName = null;
		}
		if (stringBuilder2 != null)
		{
			distributionPointUri = stringBuilder2.ToString();
		}
		else
		{
			distributionPointUri = null;
		}
		owner = null;
		if (ownerHandle != null)
		{
			_pubHandlesList.Add(ownerHandle);
			if (!ownerHandle.IsInvalid)
			{
				owner = GetUserFromHandle(ownerHandle);
			}
		}
		officialFlag = officialFlag2;
	}

	private void SetRevocationPoint(RevocationPoint revocationPoint)
	{
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMSetRevocationPoint(_issuanceLicenseHandle, flagDelete: false, revocationPoint.Id, revocationPoint.IdType, revocationPoint.Url.AbsoluteUri, revocationPoint.Frequency, revocationPoint.Name, revocationPoint.PublicKey));
	}

	private RevocationPoint GetRevocationPoint()
	{
		uint idLength = 0u;
		uint idTypeLength = 0u;
		uint urlLength = 0u;
		uint nameLength = 0u;
		uint publicKeyLength = 0u;
		SystemTime frequency = new SystemTime(DateTime.Now);
		int num = SafeNativeMethods.DRMGetRevocationPoint(_issuanceLicenseHandle, ref idLength, null, ref idTypeLength, null, ref urlLength, null, frequency, ref nameLength, null, ref publicKeyLength, null);
		if (num == -2147168432)
		{
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		StringBuilder stringBuilder = null;
		checked
		{
			if (idLength != 0)
			{
				stringBuilder = new StringBuilder((int)idLength);
			}
			StringBuilder stringBuilder2 = null;
			if (idTypeLength != 0)
			{
				stringBuilder2 = new StringBuilder((int)idTypeLength);
			}
			StringBuilder stringBuilder3 = null;
			if (urlLength != 0)
			{
				stringBuilder3 = new StringBuilder((int)urlLength);
			}
			StringBuilder stringBuilder4 = null;
			if (nameLength != 0)
			{
				stringBuilder4 = new StringBuilder((int)nameLength);
			}
			StringBuilder stringBuilder5 = null;
			if (publicKeyLength != 0)
			{
				stringBuilder5 = new StringBuilder((int)publicKeyLength);
			}
			num = SafeNativeMethods.DRMGetRevocationPoint(_issuanceLicenseHandle, ref idLength, stringBuilder, ref idTypeLength, stringBuilder2, ref urlLength, stringBuilder3, frequency, ref nameLength, stringBuilder4, ref publicKeyLength, stringBuilder5);
			Errors.ThrowOnErrorCode(num);
			return new RevocationPoint
			{
				Id = stringBuilder?.ToString(),
				IdType = stringBuilder2?.ToString(),
				Url = ((stringBuilder3 == null) ? null : new Uri(stringBuilder3.ToString())),
				Name = stringBuilder4?.ToString(),
				PublicKey = stringBuilder5?.ToString(),
				Frequency = frequency
			};
		}
	}

	private void CheckDisposed()
	{
		Invariant.Assert(_issuanceLicenseHandle != null && !_issuanceLicenseHandle.IsInvalid);
	}

	private string ConvertAuthenticationTypeToString(ContentUser user)
	{
		if (user.AuthenticationType == AuthenticationType.WindowsPassport)
		{
			return "Unspecified";
		}
		return user.AuthenticationType.ToString();
	}
}
