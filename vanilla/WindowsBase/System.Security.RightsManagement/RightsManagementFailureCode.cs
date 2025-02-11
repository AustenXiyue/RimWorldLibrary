namespace System.Security.RightsManagement;

/// <summary>Specifies error conditions that can occur when performing a rights management operation. </summary>
public enum RightsManagementFailureCode
{
	/// <summary>Operation has competed successfully.</summary>
	Success = 0,
	/// <summary>The license structure in one of the certificates is invalid. </summary>
	InvalidLicense = -2147168512,
	/// <summary>When creating a bound license, an issuance license, not an end-use license, was specified.</summary>
	InfoNotInLicense = -2147168511,
	/// <summary>Rights management signed digital certificate cannot be validated. (The signed certificate may have been tampered.)</summary>
	InvalidLicenseSignature = -2147168510,
	/// <summary>Encryption is not permitted. </summary>
	EncryptionNotPermitted = -2147168508,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	RightNotGranted = -2147168507,
	/// <summary>The rights management version is incorrect.</summary>
	InvalidVersion = -2147168506,
	/// <summary>The specified encoding type is invalid. </summary>
	InvalidEncodingType = -2147168505,
	/// <summary>The numeric value specified is invalid.</summary>
	InvalidNumericalValue = -2147168504,
	/// <summary>The algorithm type specified is invalid.</summary>
	InvalidAlgorithmType = -2147168503,
	/// <summary>The <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> cannot load.</summary>
	EnvironmentNotLoaded = -2147168502,
	/// <summary>The <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> cannot load.</summary>
	EnvironmentCannotLoad = -2147168501,
	/// <summary>Too many <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> instance have been created.</summary>
	TooManyLoadedEnvironments = -2147168500,
	/// <summary>An object type passed is incompatible with this operation.</summary>
	IncompatibleObjects = -2147168498,
	/// <summary>A library operation failed.</summary>
	LibraryFail = -2147168497,
	/// <summary>The specified principal cannot be enabled</summary>
	EnablingPrincipalFailure = -2147168496,
	/// <summary>Some information is missing.</summary>
	InfoNotPresent = -2147168495,
	/// <summary>An invalid constant was passed.</summary>
	BadGetInfoQuery = -2147168494,
	/// <summary>The key type specified in a key/value pair is not supported.</summary>
	KeyTypeUnsupported = -2147168493,
	/// <summary>A cryptographic operation that was requested is not supported. For example, passing an RMS encrypting object for decrypting purposes.</summary>
	CryptoOperationUnsupported = -2147168492,
	/// <summary>Clock rollback has been detected.  Protected content cannot be accessed.</summary>
	ClockRollbackDetected = -2147168491,
	/// <summary>No instances of the requested attribute exist. </summary>
	QueryReportsNoResults = -2147168490,
	/// <summary>An unspecified error occurred.  Also thrown when an application runs in debug mode. </summary>
	UnexpectedException = -2147168489,
	/// <summary>The defined time period for the protected content has expired; access is no longer permitted.</summary>
	BindValidityTimeViolated = -2147168488,
	/// <summary>The rights management certificate chain is broken.</summary>
	BrokenCertChain = -2147168487,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	BindPolicyViolation = -2147168485,
	/// <summary>An operation is in violation of the rights management manifest policy.</summary>
	ManifestPolicyViolation = -2147183860,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	BindRevokedLicense = -2147168484,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	BindRevokedIssuer = -2147168483,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	BindRevokedPrincipal = -2147168482,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	BindRevokedResource = -2147168481,
	/// <summary>Rights management services are not properly configured.</summary>
	BindRevokedModule = -2147168480,
	/// <summary>The specified resource is not contained in any WORK node of the license.</summary>
	BindContentNotInEndUseLicense = -2147168479,
	/// <summary>The access condition is not matched to the enabling principal that is handed into the bind.</summary>
	BindAccessPrincipalNotEnabling = -2147168478,
	/// <summary>The current user does not satisfy the conditions defined in the End Use License (EUL).</summary>
	BindAccessUnsatisfied = -2147168477,
	/// <summary>The enabling principal does not match the issued principal of the End Use License (EUL).</summary>
	BindIndicatedPrincipalMissing = -2147168476,
	/// <summary>The current machine is not defined within the rights managed group identity.</summary>
	BindMachineNotFoundInGroupIdentity = -2147168475,
	/// <summary>The specified library plug-in is not supported.</summary>
	LibraryUnsupportedPlugIn = -2147168474,
	/// <summary>The license requires that a new revocation list must be acquired.</summary>
	BindRevocationListStale = -2147168473,
	/// <summary>The current user does not have rights to access the protected content.</summary>
	BindNoApplicableRevocationList = -2147168472,
	/// <summary>Either the environment or the enabling principal handle is invalid. </summary>
	InvalidHandle = -2147168468,
	/// <summary>The defined time period for the protected content has expired; access is no longer permitted.</summary>
	BindIntervalTimeViolated = -2147168465,
	/// <summary>The specified rights group is not contained in the End Use License (EUL).</summary>
	BindNoSatisfiedRightsGroup = -2147168464,
	/// <summary>The End Use License (EUL) contains no WORK node.</summary>
	BindSpecifiedWorkMissing = -2147168463,
	/// <summary>No license or certificate exists at the specified index. </summary>
	NoMoreData = -2147168461,
	/// <summary>An End Use License (EUL) could not be acquired from the rights management server.</summary>
	LicenseAcquisitionFailed = -2147168460,
	/// <summary>The content ID from the license does not match the content ID the license storage session</summary>
	IdMismatch = -2147168459,
	/// <summary>The number of certificates has exceeded the maximum number allowed.</summary>
	TooManyCertificates = -2147168458,
	/// <summary>The protected content is corrupted.</summary>
	NoDistributionPointUrlFound = -2147168457,
	/// <summary>The requested operation is already in progress.</summary>
	AlreadyInProgress = -2147168456,
	/// <summary>A user name was not specified for the client session</summary>
	GroupIdentityNotSet = -2147168455,
	/// <summary>The specified license was not found. </summary>
	RecordNotFound = -2147168454,
	/// <summary>Rights management cannot connect to the URI specified for the license server.</summary>
	NoConnect = -2147168453,
	/// <summary>A required license is not available.</summary>
	NoLicense = -2147168452,
	/// <summary>The computer must be activated before the user can be activated. </summary>
	NeedsMachineActivation = -2147168451,
	/// <summary>The user is not activated, or no Rights Account Certificate (RAC) was submitted and none was found in the license store to match the license associated with this session.</summary>
	NeedsGroupIdentityActivation = -2147168450,
	/// <summary>License activation failed; rights management services are not properly configured.</summary>
	ActivationFailed = -2147168448,
	/// <summary>Asynchronous operation canceled, or a non-recoverable error has occurred.</summary>
	Aborted = -2147168447,
	/// <summary>The license server's maximum quota of End Use Licenses (EULs) has been reached.</summary>
	OutOfQuota = -2147168446,
	/// <summary>Possible authentication error (HTTP error 401) returned by an Internet request. Or, the current user does not have valid domain credentials in a silent user activation attempt. Or, the certification server in silent user activation is not in the local intranet or trusted sites zone.</summary>
	AuthenticationFailed = -2147168445,
	/// <summary>Rights management services are not properly configured.</summary>
	ServerError = -2147168444,
	/// <summary>An installation operation failed.</summary>
	InstallationFailed = -2147168443,
	/// <summary>The Hardware ID (HID) used in a machine activation attempt is incorrectly formatted. Rights management services are not properly configured.  </summary>
	HidCorrupted = -2147168442,
	/// <summary>Rights management services are not properly configured.</summary>
	InvalidServerResponse = -2147168441,
	/// <summary>Rights management services are not properly configured.</summary>
	ServiceNotFound = -2147168440,
	/// <summary>If a request is made for computer activation or a rights account certificate, receiving <see cref="F:System.Security.RightsManagement.RightsManagementFailureCode.UseDefault" /> indicates that the application should pass null into the ActServInfo parameter</summary>
	UseDefault = -2147168439,
	/// <summary>Rights management services are not properly configured.</summary>
	ServerNotFound = -2147168438,
	/// <summary>RMS Server email address verification failed.</summary>
	InvalidEmail = -2147168437,
	/// <summary>The defined time period for the protected content has expired; access is no longer permitted.</summary>
	ValidityTimeViolation = -2147168436,
	/// <summary>Rights management services are not properly configured.</summary>
	OutdatedModule = -2147168435,
	/// <summary>Rights management services are not properly configured.</summary>
	ServiceMoved = -2147168421,
	/// <summary>Rights management services are not properly configured.</summary>
	ServiceGone = -2147168420,
	/// <summary>The current user was not found in the Active Directory (AD) for certification under Windows authentication.</summary>
	AdEntryNotFound = -2147168419,
	/// <summary>Rights managed content is corrupted.</summary>
	NotAChain = -2147168418,
	/// <summary>The Rights Management server does not allow temporary certification of the current user.</summary>
	RequestDenied = -2147168417,
	/// <summary>The item requested to delete does not exist. </summary>
	NotSet = -2147168434,
	/// <summary>The specified metadata could net be set.</summary>
	MetadataNotSet = -2147168433,
	/// <summary>Certificate or license revocation information has not been set.</summary>
	RevocationInfoNotSet = -2147168432,
	/// <summary>The time information specified is invalid.</summary>
	InvalidTimeInfo = -2147168431,
	/// <summary>The requested right was not specified when the content was published with rights management.</summary>
	RightNotSet = -2147168430,
	/// <summary>The current user credentials are not valid for acquiring a license.</summary>
	LicenseBindingToWindowsIdentityFailed = -2147168429,
	/// <summary>The Rights Management Services template contains one or more errors.</summary>
	InvalidIssuanceLicenseTemplate = -2147168428,
	/// <summary>The key length specified in a key/value pair is invalid.   </summary>
	InvalidKeyLength = -2147168427,
	/// <summary>The authorized time period defined in the issuance license template has expired; access is no longer permitted.</summary>
	ExpiredOfficialIssuanceLicenseTemplate = -2147168425,
	/// <summary>Rights management services are not properly configured.</summary>
	InvalidClientLicensorCertificate = -2147168424,
	/// <summary>The Hardware ID (HID) used in a machine activation attempt is invalid. Rights management services are not properly configured.</summary>
	HidInvalid = -2147168423,
	/// <summary>The user's email address cannot be verified.</summary>
	EmailNotVerified = -2147168422,
	/// <summary>Cannot open or publish content with restricted permissions because a debugger has been detected. </summary>
	DebuggerDetected = -2147168416,
	/// <summary>Rights management services are not properly configured.</summary>
	InvalidLockboxType = -2147168400,
	/// <summary>Rights management services are not properly configured.</summary>
	InvalidLockboxPath = -2147168399,
	/// <summary>The specified registry path is invalid.</summary>
	InvalidRegistryPath = -2147168398,
	/// <summary>Rights management services are not properly configured.</summary>
	NoAesCryptoProvider = -2147168397,
	/// <summary>The option specified has already been set.  </summary>
	GlobalOptionAlreadySet = -2147168396,
	/// <summary>The document does not contain an Owner License.</summary>
	OwnerLicenseNotFound = -2147168395
}
