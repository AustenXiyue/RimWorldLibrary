using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.RightsManagement;
using System.Text;
using Microsoft.Win32;
using MS.Internal.WindowsBase;

namespace MS.Internal.Security.RightsManagement;

internal class ClientSession : IDisposable
{
	private const string _defaultUserName = "DefaultUser@DefaultDomain.DefaultCom";

	private const string _distributionPointLicenseAcquisitionType = "License-Acquisition-URL";

	private const string _distributionPointReferralInfoType = "Referral-Info";

	private const string _passportActivationRegistryFullKeyName = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\MSDRM\\ServiceLocation\\PassportActivation";

	private const string _passportActivationRegistryKeyName = "Software\\Microsoft\\MSDRM\\ServiceLocation\\PassportActivation";

	private ContentUser _user;

	private CallbackHandler _callbackHandler;

	private SafeRightsManagementSessionHandle _hSession;

	private UserActivationMode _userActivationMode;

	private SafeRightsManagementEnvironmentHandle _envHandle;

	private SafeRightsManagementHandle _defaultLibraryHandle;

	private List<CryptoProvider> _cryptoProviderList;

	private static ContentRight[] _rightEnums = new ContentRight[13]
	{
		ContentRight.View,
		ContentRight.Edit,
		ContentRight.Print,
		ContentRight.Extract,
		ContentRight.ObjectModel,
		ContentRight.Owner,
		ContentRight.ViewRightsData,
		ContentRight.Forward,
		ContentRight.Reply,
		ContentRight.ReplyAll,
		ContentRight.Sign,
		ContentRight.DocumentEdit,
		ContentRight.Export
	};

	private static string[] _rightNames = new string[13]
	{
		"VIEW", "EDIT", "PRINT", "EXTRACT", "OBJMODEL", "OWNER", "VIEWRIGHTSDATA", "FORWARD", "REPLY", "REPLYALL",
		"SIGN", "DOCEDIT", "EXPORT"
	};

	private List<CryptoProvider> CryptoProviderList
	{
		get
		{
			if (_cryptoProviderList == null)
			{
				_cryptoProviderList = new List<CryptoProvider>(5);
			}
			return _cryptoProviderList;
		}
	}

	internal ClientSession(ContentUser user)
		: this(user, UserActivationMode.Permanent)
	{
	}

	internal ClientSession(ContentUser user, UserActivationMode userActivationMode)
	{
		Invariant.Assert(user != null);
		Invariant.Assert(userActivationMode == UserActivationMode.Permanent || userActivationMode == UserActivationMode.Temporary);
		_user = user;
		_userActivationMode = userActivationMode;
		_callbackHandler = new CallbackHandler();
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMCreateClientSession(_callbackHandler.CallbackDelegate, 1u, _user.AuthenticationProviderType, _user.Name, out _hSession));
		Invariant.Assert(_hSession != null && !_hSession.IsInvalid);
	}

	~ClientSession()
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
			if (_cryptoProviderList == null)
			{
				return;
			}
			foreach (CryptoProvider cryptoProvider in _cryptoProviderList)
			{
				cryptoProvider.Dispose();
			}
		}
		finally
		{
			_cryptoProviderList = null;
			try
			{
				if (_hSession != null && !_hSession.IsInvalid)
				{
					if (_userActivationMode == UserActivationMode.Temporary)
					{
						RemoveUsersCertificates(EnumerateLicenseFlags.SpecifiedClientLicensor);
						RemoveUsersCertificates(EnumerateLicenseFlags.SpecifiedGroupIdentity);
					}
					_hSession.Dispose();
				}
			}
			finally
			{
				_hSession = null;
				try
				{
					if (_defaultLibraryHandle != null && !_defaultLibraryHandle.IsInvalid)
					{
						_defaultLibraryHandle.Dispose();
					}
				}
				finally
				{
					_defaultLibraryHandle = null;
					try
					{
						if (_envHandle != null && !_envHandle.IsInvalid)
						{
							_envHandle.Dispose();
						}
					}
					finally
					{
						_envHandle = null;
						try
						{
							if (_callbackHandler != null)
							{
								_callbackHandler.Dispose();
							}
						}
						finally
						{
							_callbackHandler = null;
						}
					}
				}
			}
		}
	}

	internal static ClientSession DefaultUserClientSession(AuthenticationType authentication)
	{
		return new ClientSession(new ContentUser("DefaultUser@DefaultDomain.DefaultCom", authentication));
	}

	internal bool IsMachineActivated()
	{
		CheckDisposed();
		if (IsActivated(ActivationFlags.Machine))
		{
			return GetMachineCert() != null;
		}
		return false;
	}

	internal void ActivateMachine(AuthenticationType authentication)
	{
		CheckDisposed();
		ActivationFlags activationFlags = ActivationFlags.Machine | ActivationFlags.Silent;
		Activate(activationFlags, null);
	}

	internal bool IsUserActivated()
	{
		CheckDisposed();
		if (!IsActivated(ActivationFlags.GroupIdentity))
		{
			return false;
		}
		string groupIdentityCert = GetGroupIdentityCert();
		if (groupIdentityCert == null)
		{
			return false;
		}
		ContentUser contentUser = ExtractUserFromCertificateChain(groupIdentityCert);
		if (contentUser == null)
		{
			return false;
		}
		return _user.GenericEquals(contentUser);
	}

	internal ContentUser ActivateUser(AuthenticationType authentication, UserActivationMode userActivationMode)
	{
		CheckDisposed();
		ActivationFlags activationFlags = ActivationFlags.GroupIdentity;
		if (_user.AuthenticationType == AuthenticationType.Windows)
		{
			activationFlags |= ActivationFlags.Silent;
		}
		if (userActivationMode == UserActivationMode.Temporary)
		{
			activationFlags |= ActivationFlags.Temporary;
		}
		return ExtractUserFromCertificate(Activate(activationFlags, GetCertificationUrl(authentication)));
	}

	internal bool IsClientLicensorCertificatePresent()
	{
		CheckDisposed();
		return GetClientLicensorCert() != null;
	}

	internal void AcquireClientLicensorCertificate()
	{
		CheckDisposed();
		Uri clientLicensorUrl = GetClientLicensorUrl(_user.AuthenticationType);
		string groupIdentityCert = GetGroupIdentityCert();
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMAcquireLicense(_hSession, 0u, groupIdentityCert, null, null, clientLicensorUrl.AbsoluteUri, IntPtr.Zero));
		_callbackHandler.WaitForCompletion();
	}

	internal void BuildSecureEnvironment(string applicationManifest)
	{
		CheckDisposed();
		Invariant.Assert(_envHandle == null);
		string securityProviderPath = GetSecurityProviderPath();
		string machineCert = GetMachineCert();
		_defaultLibraryHandle = null;
		_envHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMInitEnvironment(0u, 1u, securityProviderPath, applicationManifest, machineCert, out _envHandle, out _defaultLibraryHandle));
	}

	private bool IsActivated(ActivationFlags activateFlags)
	{
		CheckDisposed();
		return SafeNativeMethods.DRMIsActivated(_hSession, (uint)activateFlags, null) == 0;
	}

	internal string GetMachineCert()
	{
		CheckDisposed();
		return EnumerateLicense(EnumerateLicenseFlags.Machine, 0);
	}

	internal List<string> EnumerateUsersCertificateIds(ContentUser user, EnumerateLicenseFlags certificateType)
	{
		CheckDisposed();
		if (certificateType != EnumerateLicenseFlags.Machine && certificateType != EnumerateLicenseFlags.GroupIdentity && certificateType != EnumerateLicenseFlags.GroupIdentityName && certificateType != EnumerateLicenseFlags.GroupIdentityLid && certificateType != EnumerateLicenseFlags.SpecifiedGroupIdentity && certificateType != EnumerateLicenseFlags.Eul && certificateType != EnumerateLicenseFlags.EulLid && certificateType != EnumerateLicenseFlags.ClientLicensor && certificateType != EnumerateLicenseFlags.ClientLicensorLid && certificateType != EnumerateLicenseFlags.SpecifiedClientLicensor && certificateType != EnumerateLicenseFlags.RevocationList && certificateType != EnumerateLicenseFlags.RevocationListLid && certificateType != EnumerateLicenseFlags.Expired)
		{
			throw new ArgumentOutOfRangeException("certificateType");
		}
		List<string> list = new List<string>();
		int num = 0;
		while (true)
		{
			string text = EnumerateLicense(certificateType, num);
			if (text == null)
			{
				break;
			}
			ContentUser userObj = ExtractUserFromCertificateChain(text);
			if (user.GenericEquals(userObj))
			{
				list.Add(ExtractCertificateIdFromCertificateChain(text));
			}
			num++;
		}
		return list;
	}

	internal void DeleteLicense(string licenseId)
	{
		CheckDisposed();
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMDeleteLicense(_hSession, licenseId));
	}

	internal void RemoveUsersCertificates(EnumerateLicenseFlags certificateType)
	{
		CheckDisposed();
		Invariant.Assert(certificateType == EnumerateLicenseFlags.SpecifiedClientLicensor || certificateType == EnumerateLicenseFlags.SpecifiedGroupIdentity);
		foreach (string item in EnumerateAllValuesOnSession(_hSession, certificateType))
		{
			DeleteLicense(ExtractCertificateIdFromCertificateChain(item));
		}
	}

	private string GetClientLicensorCert()
	{
		return GetLatestCertificate(EnumerateLicenseFlags.SpecifiedClientLicensor);
	}

	private string GetGroupIdentityCert()
	{
		return GetLatestCertificate(EnumerateLicenseFlags.SpecifiedGroupIdentity);
	}

	private string GetLatestCertificate(EnumerateLicenseFlags enumerateLicenseFlags)
	{
		int num = 0;
		string text = EnumerateLicense(enumerateLicenseFlags, num);
		if (text == null)
		{
			return null;
		}
		DateTime t = ExtractIssuedTimeFromCertificateChain(text, DateTime.MinValue);
		while (text != null)
		{
			num++;
			string text2 = EnumerateLicense(enumerateLicenseFlags, num);
			if (text2 == null)
			{
				break;
			}
			DateTime dateTime = ExtractIssuedTimeFromCertificateChain(text2, DateTime.MinValue);
			if (DateTime.Compare(t, dateTime) < 0)
			{
				text = text2;
				t = dateTime;
			}
		}
		return text;
	}

	private static ArrayList EnumerateAllValuesOnSession(SafeRightsManagementSessionHandle sessionHandle, EnumerateLicenseFlags enumerateLicenseFlags)
	{
		ArrayList arrayList = new ArrayList(5);
		int num = 0;
		while (true)
		{
			string licenseOnSession = GetLicenseOnSession(sessionHandle, enumerateLicenseFlags, num);
			if (licenseOnSession == null)
			{
				break;
			}
			arrayList.Add(licenseOnSession);
			num++;
		}
		return arrayList;
	}

	internal static string GetLicenseOnSession(SafeRightsManagementSessionHandle sessionHandle, EnumerateLicenseFlags enumerateLicenseFlags, int index)
	{
		Invariant.Assert(index >= 0);
		if (enumerateLicenseFlags != EnumerateLicenseFlags.Machine && enumerateLicenseFlags != EnumerateLicenseFlags.GroupIdentity && enumerateLicenseFlags != EnumerateLicenseFlags.GroupIdentityName && enumerateLicenseFlags != EnumerateLicenseFlags.GroupIdentityLid && enumerateLicenseFlags != EnumerateLicenseFlags.SpecifiedGroupIdentity && enumerateLicenseFlags != EnumerateLicenseFlags.Eul && enumerateLicenseFlags != EnumerateLicenseFlags.EulLid && enumerateLicenseFlags != EnumerateLicenseFlags.ClientLicensor && enumerateLicenseFlags != EnumerateLicenseFlags.ClientLicensorLid && enumerateLicenseFlags != EnumerateLicenseFlags.SpecifiedClientLicensor && enumerateLicenseFlags != EnumerateLicenseFlags.RevocationList && enumerateLicenseFlags != EnumerateLicenseFlags.RevocationListLid && enumerateLicenseFlags != EnumerateLicenseFlags.Expired)
		{
			throw new ArgumentOutOfRangeException("enumerateLicenseFlags");
		}
		int num = 0;
		bool pfSharedFlag = false;
		uint puCertDataLen = 0u;
		StringBuilder stringBuilder = null;
		num = SafeNativeMethods.DRMEnumerateLicense(sessionHandle, (uint)enumerateLicenseFlags, (uint)index, ref pfSharedFlag, ref puCertDataLen, null);
		if (num == -2147168461)
		{
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		if (puCertDataLen > int.MaxValue)
		{
			return null;
		}
		stringBuilder = new StringBuilder(checked((int)puCertDataLen));
		num = SafeNativeMethods.DRMEnumerateLicense(sessionHandle, (uint)enumerateLicenseFlags, (uint)index, ref pfSharedFlag, ref puCertDataLen, stringBuilder);
		Errors.ThrowOnErrorCode(num);
		return stringBuilder.ToString();
	}

	internal string EnumerateLicense(EnumerateLicenseFlags enumerateLicenseFlags, int index)
	{
		CheckDisposed();
		return GetLicenseOnSession(_hSession, enumerateLicenseFlags, index);
	}

	internal PublishLicense SignIssuanceLicense(IssuanceLicense issuanceLicense, out UseLicense authorUseLicense)
	{
		CheckDisposed();
		Invariant.Assert(issuanceLicense != null);
		Invariant.Assert(!_envHandle.IsInvalid);
		using CallbackHandler callbackHandler = new CallbackHandler();
		string clientLicensorCert = GetClientLicensorCert();
		if (clientLicensorCert == null)
		{
			throw new RightsManagementException(SR.UserHasNoClientLicensorCert);
		}
		clientLicensorCert = clientLicensorCert.Trim();
		if (clientLicensorCert.Length == 0)
		{
			throw new RightsManagementException(SR.UserHasNoClientLicensorCert);
		}
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetSignedIssuanceLicense(_envHandle, issuanceLicense.Handle, 50u, null, 0u, "AES", clientLicensorCert, callbackHandler.CallbackDelegate, null, 0u));
		callbackHandler.WaitForCompletion();
		PublishLicense result = new PublishLicense(callbackHandler.CallbackData);
		authorUseLicense = new UseLicense(GetOwnerLicense(issuanceLicense.Handle));
		return result;
	}

	internal UseLicense AcquireUseLicense(string publishLicense, bool noUI)
	{
		CheckDisposed();
		Invariant.Assert(!_envHandle.IsInvalid);
		SafeRightsManagementSessionHandle phLicenseStorageSession = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMCreateLicenseStorageSession(_envHandle, _defaultLibraryHandle, _hSession, 0u, publishLicense, out phLicenseStorageSession));
		using (phLicenseStorageSession)
		{
			uint num = 0u;
			if (noUI)
			{
				num |= 0x10;
			}
			string groupIdentityCert = GetGroupIdentityCert();
			ArrayList oldList = EnumerateAllValuesOnSession(phLicenseStorageSession, EnumerateLicenseFlags.EulLid);
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMAcquireLicense(phLicenseStorageSession, num, groupIdentityCert, null, null, null, IntPtr.Zero));
			_callbackHandler.WaitForCompletion();
			ArrayList newList = EnumerateAllValuesOnSession(phLicenseStorageSession, EnumerateLicenseFlags.EulLid);
			int num2 = FindNewEntryIndex(oldList, newList);
			if (num2 < 0)
			{
				throw new RightsManagementException(RightsManagementFailureCode.LicenseAcquisitionFailed);
			}
			return new UseLicense(GetLicenseOnSession(phLicenseStorageSession, EnumerateLicenseFlags.Eul, num2));
		}
	}

	private static int FindNewEntryIndex(ArrayList oldList, ArrayList newList)
	{
		Invariant.Assert(oldList != null && newList != null);
		for (int i = 0; i < newList.Count; i++)
		{
			string strA = (string)newList[i];
			bool flag = false;
			foreach (string old in oldList)
			{
				if (string.CompareOrdinal(strA, old) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return -1;
	}

	private CryptoProvider BindUseLicense(string serializedUseLicense, List<RightNameExpirationInfoPair> unboundRightsList, BoundLicenseParams boundLicenseParams, out int theFirstHrFailureCode)
	{
		List<SafeRightsManagementHandle> list = new List<SafeRightsManagementHandle>(unboundRightsList.Count);
		List<RightNameExpirationInfoPair> list2 = new List<RightNameExpirationInfoPair>(unboundRightsList.Count);
		try
		{
			theFirstHrFailureCode = 0;
			foreach (RightNameExpirationInfoPair unboundRights in unboundRightsList)
			{
				boundLicenseParams.wszRightsRequested = unboundRights.RightName;
				SafeRightsManagementHandle boundLicenseHandle = null;
				uint errorLogHandle = 0u;
				int num = SafeNativeMethods.DRMCreateBoundLicense(_envHandle, boundLicenseParams, serializedUseLicense, out boundLicenseHandle, out errorLogHandle);
				if (boundLicenseHandle != null && num == 0)
				{
					list.Add(boundLicenseHandle);
					list2.Add(unboundRights);
				}
				if (theFirstHrFailureCode == 0 && num != 0)
				{
					theFirstHrFailureCode = num;
				}
			}
			if (list.Count > 0)
			{
				ContentUser owner = ExtractUserFromCertificateChain(boundLicenseParams.wszDefaultEnablingPrincipalCredentials);
				CryptoProvider cryptoProvider = new CryptoProvider(list, list2, owner);
				CryptoProviderList.Add(cryptoProvider);
				return cryptoProvider;
			}
			return null;
		}
		catch
		{
			foreach (SafeRightsManagementHandle item in list)
			{
				item.Dispose();
			}
			throw;
		}
	}

	internal CryptoProvider TryBindUseLicenseToAllIdentites(string serializedUseLicense)
	{
		CheckDisposed();
		Invariant.Assert(serializedUseLicense != null);
		int theFirstHrFailureCode = 0;
		int num = 0;
		string rightGroupName;
		List<RightNameExpirationInfoPair> rightsInfoFromUseLicense = GetRightsInfoFromUseLicense(serializedUseLicense, out rightGroupName);
		BoundLicenseParams boundLicenseParams = new BoundLicenseParams();
		boundLicenseParams.uVersion = 0u;
		boundLicenseParams.hEnablingPrincipal = 0u;
		boundLicenseParams.hSecureStore = 0u;
		boundLicenseParams.wszRightsGroup = rightGroupName;
		GetContentIdFromLicense(serializedUseLicense, out var contentId, out var contentIdType);
		boundLicenseParams.DRMIDuVersion = 0u;
		boundLicenseParams.DRMIDIdType = contentIdType;
		boundLicenseParams.DRMIDId = contentId;
		boundLicenseParams.cAuthenticatorCount = 0u;
		boundLicenseParams.rghAuthenticators = IntPtr.Zero;
		string groupIdentityCert = GetGroupIdentityCert();
		boundLicenseParams.wszDefaultEnablingPrincipalCredentials = groupIdentityCert;
		boundLicenseParams.dwFlags = 0u;
		CryptoProvider cryptoProvider = BindUseLicense(serializedUseLicense, rightsInfoFromUseLicense, boundLicenseParams, out theFirstHrFailureCode);
		if (cryptoProvider != null)
		{
			return cryptoProvider;
		}
		if (num == 0 && theFirstHrFailureCode != 0)
		{
			num = theFirstHrFailureCode;
		}
		int num2 = 0;
		while (true)
		{
			groupIdentityCert = EnumerateLicense(EnumerateLicenseFlags.GroupIdentity, num2);
			if (groupIdentityCert == null)
			{
				break;
			}
			num2++;
			boundLicenseParams.wszDefaultEnablingPrincipalCredentials = groupIdentityCert;
			cryptoProvider = BindUseLicense(serializedUseLicense, rightsInfoFromUseLicense, boundLicenseParams, out theFirstHrFailureCode);
			if (cryptoProvider != null)
			{
				return cryptoProvider;
			}
			if (num == 0 && theFirstHrFailureCode != 0)
			{
				num = theFirstHrFailureCode;
			}
		}
		Invariant.Assert(num != 0);
		Errors.ThrowOnErrorCode(num);
		return null;
	}

	private Uri GetCertificationUrl(AuthenticationType authentication)
	{
		Uri uri = null;
		if (authentication == AuthenticationType.Windows)
		{
			uri = GetServiceLocation(ServiceType.Certification, ServiceLocation.Enterprise, null);
			if (uri == null)
			{
				uri = GetServiceLocation(ServiceType.Certification, ServiceLocation.Internet, null);
			}
		}
		else
		{
			uri = GetRegistryPassportCertificationUrl();
			if (uri == null)
			{
				uri = GetServiceLocation(ServiceType.Certification, ServiceLocation.Internet, null);
			}
		}
		return uri;
	}

	private static Uri GetRegistryPassportCertificationUrl()
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\MSDRM\\ServiceLocation\\PassportActivation");
		if (registryKey == null)
		{
			return null;
		}
		if (registryKey.GetValue(null) is string uriString)
		{
			return new Uri(uriString);
		}
		return null;
	}

	private Uri GetClientLicensorUrl(AuthenticationType authentication)
	{
		Uri serviceLocation = GetServiceLocation(ServiceType.ClientLicensor, ServiceLocation.Enterprise, null);
		if (serviceLocation == null)
		{
			serviceLocation = GetServiceLocation(ServiceType.ClientLicensor, ServiceLocation.Internet, null);
		}
		return serviceLocation;
	}

	private string Activate(ActivationFlags activationFlags, Uri url)
	{
		ActivationServerInfo activationServerInfo = null;
		if (url != null)
		{
			activationServerInfo = new ActivationServerInfo();
			activationServerInfo.PubKey = null;
			activationServerInfo.Url = url.AbsoluteUri;
			activationServerInfo.Version = 1u;
		}
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMActivate(_hSession, (uint)activationFlags, 0u, activationServerInfo, IntPtr.Zero, IntPtr.Zero));
		_callbackHandler.WaitForCompletion();
		return _callbackHandler.CallbackData;
	}

	private Uri GetServiceLocation(ServiceType serviceType, ServiceLocation serviceLocation, string issuanceLicense)
	{
		uint serviceUrlLength = 0u;
		StringBuilder stringBuilder = null;
		int num = SafeNativeMethods.DRMGetServiceLocation(_hSession, (uint)serviceType, (uint)serviceLocation, issuanceLicense, ref serviceUrlLength, null);
		if (num == -2147168439)
		{
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		stringBuilder = new StringBuilder(checked((int)serviceUrlLength));
		num = SafeNativeMethods.DRMGetServiceLocation(_hSession, (uint)serviceType, (uint)serviceLocation, issuanceLicense, ref serviceUrlLength, stringBuilder);
		Errors.ThrowOnErrorCode(num);
		return new Uri(stringBuilder.ToString());
	}

	internal static string GetOwnerLicense(SafeRightsManagementPubHandle issuanceLicenseHandle)
	{
		Invariant.Assert(!issuanceLicenseHandle.IsInvalid);
		uint ownerLicenseLength = 0u;
		StringBuilder stringBuilder = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetOwnerLicense(issuanceLicenseHandle, ref ownerLicenseLength, null));
		stringBuilder = new StringBuilder(checked((int)ownerLicenseLength));
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetOwnerLicense(issuanceLicenseHandle, ref ownerLicenseLength, stringBuilder));
		return stringBuilder.ToString();
	}

	private static string GetElementFromCertificateChain(string certificateChain, int index)
	{
		Invariant.Assert(index >= 0);
		Invariant.Assert(certificateChain != null);
		uint certificateLength = 0u;
		StringBuilder stringBuilder = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMDeconstructCertificateChain(certificateChain, (uint)index, ref certificateLength, null));
		stringBuilder = new StringBuilder(checked((int)certificateLength));
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMDeconstructCertificateChain(certificateChain, (uint)index, ref certificateLength, stringBuilder));
		return stringBuilder.ToString();
	}

	private static string GetUnboundLicenseStringAttribute(SafeRightsManagementQueryHandle queryHandle, string attributeType, uint attributeIndex)
	{
		uint bufferSize = 0u;
		byte[] array = null;
		int num = SafeNativeMethods.DRMGetUnboundLicenseAttribute(queryHandle, attributeType, attributeIndex, out var encodingType, ref bufferSize, null);
		if (num == -2147168490)
		{
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		if (bufferSize < 2)
		{
			return null;
		}
		array = new byte[checked((int)bufferSize)];
		num = SafeNativeMethods.DRMGetUnboundLicenseAttribute(queryHandle, attributeType, attributeIndex, out encodingType, ref bufferSize, array);
		Errors.ThrowOnErrorCode(num);
		return Encoding.Unicode.GetString(array, 0, array.Length - 2);
	}

	private static DateTime GetUnboundLicenseDateTimeAttribute(SafeRightsManagementQueryHandle queryHandle, string attributeType, uint attributeIndex, DateTime defaultValue)
	{
		uint bufferSize = SystemTime.Size;
		byte[] array = new byte[bufferSize];
		uint encodingType;
		int num = SafeNativeMethods.DRMGetUnboundLicenseAttribute(queryHandle, attributeType, attributeIndex, out encodingType, ref bufferSize, array);
		if (encodingType != 3)
		{
			throw new RightsManagementException(RightsManagementFailureCode.InvalidLicense);
		}
		if (num == -2147168461 || num == -2147168490)
		{
			return defaultValue;
		}
		Errors.ThrowOnErrorCode(num);
		return new SystemTime(array).GetDateTime(defaultValue);
	}

	internal static ContentUser ExtractUserFromCertificateChain(string certificateChain)
	{
		Invariant.Assert(certificateChain != null);
		return ExtractUserFromCertificate(GetElementFromCertificateChain(certificateChain, 0));
	}

	private static DateTime ExtractIssuedTimeFromCertificateChain(string certificateChain, DateTime defaultValue)
	{
		Invariant.Assert(certificateChain != null);
		return ExtractIssuedTimeFromCertificate(GetElementFromCertificateChain(certificateChain, 0), defaultValue);
	}

	private static DateTime ExtractIssuedTimeFromCertificate(string certificate, DateTime defaultValue)
	{
		SafeRightsManagementQueryHandle queryRootHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMParseUnboundLicense(certificate, out queryRootHandle));
		using (queryRootHandle)
		{
			return GetUnboundLicenseDateTimeAttribute(queryRootHandle, "issued-time", 0u, defaultValue);
		}
	}

	internal static ContentUser ExtractUserFromCertificate(string certificate)
	{
		SafeRightsManagementQueryHandle queryRootHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMParseUnboundLicense(certificate, out queryRootHandle));
		using (queryRootHandle)
		{
			SafeRightsManagementQueryHandle subQueryHandle = null;
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUnboundLicenseObject(queryRootHandle, "issued-principal", 0u, out subQueryHandle));
			using (subQueryHandle)
			{
				string unboundLicenseStringAttribute = GetUnboundLicenseStringAttribute(subQueryHandle, "name", 0u);
				string unboundLicenseStringAttribute2 = GetUnboundLicenseStringAttribute(subQueryHandle, "id-type", 0u);
				if (string.CompareOrdinal(AuthenticationType.Windows.ToString().ToUpper(CultureInfo.InvariantCulture), unboundLicenseStringAttribute2.ToUpper(CultureInfo.InvariantCulture)) == 0)
				{
					return new ContentUser(unboundLicenseStringAttribute, AuthenticationType.Windows);
				}
				return new ContentUser(unboundLicenseStringAttribute, AuthenticationType.Passport);
			}
		}
	}

	internal static string ExtractCertificateIdFromCertificateChain(string certificateChain)
	{
		Invariant.Assert(certificateChain != null);
		return ExtractCertificateIdFromCertificate(GetElementFromCertificateChain(certificateChain, 0));
	}

	internal static string ExtractCertificateIdFromCertificate(string certificate)
	{
		SafeRightsManagementQueryHandle queryRootHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMParseUnboundLicense(certificate, out queryRootHandle));
		using (queryRootHandle)
		{
			return GetUnboundLicenseStringAttribute(queryRootHandle, "id-value", 0u);
		}
	}

	internal static Dictionary<string, string> ExtractApplicationSpecificDataFromLicense(string useLicenseChain)
	{
		Invariant.Assert(useLicenseChain != null);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(3, StringComparer.Ordinal);
		string elementFromCertificateChain = GetElementFromCertificateChain(useLicenseChain, 0);
		Invariant.Assert(elementFromCertificateChain != null);
		SafeRightsManagementQueryHandle queryRootHandle = null;
		int hr = SafeNativeMethods.DRMParseUnboundLicense(elementFromCertificateChain, out queryRootHandle);
		Errors.ThrowOnErrorCode(hr);
		using (queryRootHandle)
		{
			uint num = 0u;
			while (true)
			{
				string unboundLicenseStringAttribute = GetUnboundLicenseStringAttribute(queryRootHandle, "appdata-name", num);
				if (unboundLicenseStringAttribute == null)
				{
					break;
				}
				Errors.ThrowOnErrorCode(hr);
				string unboundLicenseStringAttribute2 = GetUnboundLicenseStringAttribute(queryRootHandle, "appdata-value", num);
				Errors.ThrowOnErrorCode(hr);
				dictionary.Add(unboundLicenseStringAttribute, unboundLicenseStringAttribute2);
				num++;
			}
			return dictionary;
		}
	}

	internal static void GetContentIdFromLicense(string useLicenseChain, out string contentId, out string contentIdType)
	{
		Invariant.Assert(useLicenseChain != null);
		string elementFromCertificateChain = GetElementFromCertificateChain(useLicenseChain, 0);
		Invariant.Assert(elementFromCertificateChain != null);
		SafeRightsManagementQueryHandle queryRootHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMParseUnboundLicense(elementFromCertificateChain, out queryRootHandle));
		using (queryRootHandle)
		{
			SafeRightsManagementQueryHandle subQueryHandle = null;
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUnboundLicenseObject(queryRootHandle, "work", 0u, out subQueryHandle));
			using (subQueryHandle)
			{
				contentIdType = GetUnboundLicenseStringAttribute(subQueryHandle, "id-type", 0u);
				contentId = GetUnboundLicenseStringAttribute(subQueryHandle, "id-value", 0u);
			}
		}
	}

	private static List<RightNameExpirationInfoPair> GetRightsInfoFromUseLicense(string useLicenseChain, out string rightGroupName)
	{
		Invariant.Assert(useLicenseChain != null);
		string elementFromCertificateChain = GetElementFromCertificateChain(useLicenseChain, 0);
		Invariant.Assert(elementFromCertificateChain != null);
		List<RightNameExpirationInfoPair> list = new List<RightNameExpirationInfoPair>(10);
		SafeRightsManagementQueryHandle queryRootHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMParseUnboundLicense(elementFromCertificateChain, out queryRootHandle));
		using (queryRootHandle)
		{
			SafeRightsManagementQueryHandle subQueryHandle = null;
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUnboundLicenseObject(queryRootHandle, "work", 0u, out subQueryHandle));
			using (subQueryHandle)
			{
				SafeRightsManagementQueryHandle subQueryHandle2 = null;
				Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUnboundLicenseObject(subQueryHandle, "rights-group", 0u, out subQueryHandle2));
				using (subQueryHandle2)
				{
					rightGroupName = GetUnboundLicenseStringAttribute(subQueryHandle2, "name", 0u);
					uint num = 0u;
					while (true)
					{
						RightNameExpirationInfoPair rightInfoFromRightGroupQueryHandle = GetRightInfoFromRightGroupQueryHandle(subQueryHandle2, num);
						if (rightInfoFromRightGroupQueryHandle == null)
						{
							break;
						}
						list.Add(rightInfoFromRightGroupQueryHandle);
						num++;
					}
					return list;
				}
			}
		}
	}

	private static RightNameExpirationInfoPair GetRightInfoFromRightGroupQueryHandle(SafeRightsManagementQueryHandle rightGroupQueryHandle, uint rightIndex)
	{
		SafeRightsManagementQueryHandle subQueryHandle = null;
		int num = SafeNativeMethods.DRMGetUnboundLicenseObject(rightGroupQueryHandle, "right", rightIndex, out subQueryHandle);
		if (num == -2147168461 || num == -2147168490)
		{
			return null;
		}
		Errors.ThrowOnErrorCode(num);
		using (subQueryHandle)
		{
			string unboundLicenseStringAttribute = GetUnboundLicenseStringAttribute(subQueryHandle, "name", 0u);
			DateTime validFrom = DateTime.MinValue;
			DateTime validUntil = DateTime.MaxValue;
			SafeRightsManagementQueryHandle subQueryHandle2 = null;
			num = SafeNativeMethods.DRMGetUnboundLicenseObject(subQueryHandle, "condition-list", 0u, out subQueryHandle2);
			if (num >= 0)
			{
				using (subQueryHandle2)
				{
					SafeRightsManagementQueryHandle subQueryHandle3 = null;
					num = SafeNativeMethods.DRMGetUnboundLicenseObject(subQueryHandle2, "rangetime-condition", 0u, out subQueryHandle3);
					if (num != -2147168461 && num != -2147168490)
					{
						Errors.ThrowOnErrorCode(num);
						using (subQueryHandle3)
						{
							validFrom = GetUnboundLicenseDateTimeAttribute(subQueryHandle3, "from-time", 0u, DateTime.MinValue);
							validUntil = GetUnboundLicenseDateTimeAttribute(subQueryHandle3, "until-time", 0u, DateTime.MaxValue);
						}
					}
				}
			}
			return new RightNameExpirationInfoPair(unboundLicenseStringAttribute, validFrom, validUntil);
		}
	}

	internal static string GetContentIdFromPublishLicense(string publishLicense)
	{
		Invariant.Assert(publishLicense != null);
		SafeRightsManagementQueryHandle queryRootHandle = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMParseUnboundLicense(publishLicense, out queryRootHandle));
		using (queryRootHandle)
		{
			SafeRightsManagementQueryHandle subQueryHandle = null;
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetUnboundLicenseObject(queryRootHandle, "work", 0u, out subQueryHandle));
			using (subQueryHandle)
			{
				return GetUnboundLicenseStringAttribute(subQueryHandle, "id-value", 0u);
			}
		}
	}

	internal static Uri GetUseLicenseAcquisitionUriFromPublishLicense(string publishLicense)
	{
		GetDistributionPointInfoFromPublishLicense(publishLicense, "License-Acquisition-URL", out var _, out var addressAttributeValue);
		return new Uri(addressAttributeValue);
	}

	internal static void GetReferralInfoFromPublishLicense(string publishLicense, out string referralInfoName, out Uri referralInfoUri)
	{
		GetDistributionPointInfoFromPublishLicense(publishLicense, "Referral-Info", out var nameAttributeValue, out var addressAttributeValue);
		referralInfoName = nameAttributeValue;
		if (addressAttributeValue != null)
		{
			referralInfoUri = new Uri(addressAttributeValue);
		}
		else
		{
			referralInfoUri = null;
		}
	}

	private static void GetDistributionPointInfoFromPublishLicense(string publishLicense, string distributionPointType, out string nameAttributeValue, out string addressAttributeValue)
	{
		Invariant.Assert(publishLicense != null);
		nameAttributeValue = null;
		addressAttributeValue = null;
		SafeRightsManagementQueryHandle queryRootHandle = null;
		int hr = SafeNativeMethods.DRMParseUnboundLicense(publishLicense, out queryRootHandle);
		Errors.ThrowOnErrorCode(hr);
		using (queryRootHandle)
		{
			uint num = 0u;
			while (true)
			{
				SafeRightsManagementQueryHandle subQueryHandle = null;
				hr = SafeNativeMethods.DRMGetUnboundLicenseObject(queryRootHandle, "distribution-point", num, out subQueryHandle);
				if (hr == -2147168490)
				{
					break;
				}
				Errors.ThrowOnErrorCode(hr);
				using (subQueryHandle)
				{
					if (string.CompareOrdinal(GetUnboundLicenseStringAttribute(subQueryHandle, "object-type", 0u), distributionPointType) == 0)
					{
						nameAttributeValue = GetUnboundLicenseStringAttribute(subQueryHandle, "name", 0u);
						addressAttributeValue = GetUnboundLicenseStringAttribute(subQueryHandle, "address-value", 0u);
						break;
					}
				}
				num++;
			}
		}
	}

	internal static string GetSecurityProviderPath()
	{
		uint typeLength = 0u;
		StringBuilder stringBuilder = null;
		uint pathLength = 0u;
		StringBuilder stringBuilder2 = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetSecurityProvider(0u, ref typeLength, null, ref pathLength, null));
		checked
		{
			stringBuilder = new StringBuilder((int)typeLength);
			stringBuilder2 = new StringBuilder((int)pathLength);
			Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetSecurityProvider(0u, ref typeLength, stringBuilder, ref pathLength, stringBuilder2));
			return stringBuilder2.ToString();
		}
	}

	internal static ContentRight? GetRightFromString(string rightName)
	{
		rightName = rightName.ToString().ToUpper(CultureInfo.InvariantCulture);
		for (int i = 0; i < _rightEnums.Length; i++)
		{
			if (string.CompareOrdinal(_rightNames[i], rightName) == 0)
			{
				return _rightEnums[i];
			}
		}
		return null;
	}

	internal static string GetStringFromRight(ContentRight right)
	{
		for (int i = 0; i < _rightEnums.Length; i++)
		{
			if (_rightEnums[i] == right)
			{
				return _rightNames[i];
			}
		}
		throw new ArgumentOutOfRangeException("right");
	}

	private void CheckDisposed()
	{
		if (_hSession == null || _hSession.IsInvalid)
		{
			throw new ObjectDisposedException("SecureEnvironment");
		}
	}
}
