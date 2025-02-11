using System;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using MS.Internal.WindowsBase;

namespace MS.Internal.Security.RightsManagement;

internal static class Errors
{
	internal static string GetLocalizedFailureCodeMessageWithDefault(RightsManagementFailureCode failureCode)
	{
		string localizedFailureCodeMessage = GetLocalizedFailureCodeMessage(failureCode);
		if (localizedFailureCodeMessage != null)
		{
			return localizedFailureCodeMessage;
		}
		return SR.RmExceptionGenericMessage;
	}

	internal static void ThrowOnErrorCode(int hr)
	{
		if (hr >= 0)
		{
			return;
		}
		string localizedFailureCodeMessage = GetLocalizedFailureCodeMessage((RightsManagementFailureCode)hr);
		if (localizedFailureCodeMessage != null)
		{
			throw new RightsManagementException((RightsManagementFailureCode)hr, localizedFailureCodeMessage);
		}
		try
		{
			Marshal.ThrowExceptionForHR(hr);
		}
		catch (Exception innerException)
		{
			throw new RightsManagementException(SR.RmExceptionGenericMessage, innerException);
		}
	}

	private static string GetLocalizedFailureCodeMessage(RightsManagementFailureCode failureCode)
	{
		string name;
		switch (failureCode)
		{
		case RightsManagementFailureCode.InvalidLicense:
			name = "RmExceptionInvalidLicense";
			break;
		case RightsManagementFailureCode.InfoNotInLicense:
			name = "RmExceptionInfoNotInLicense";
			break;
		case RightsManagementFailureCode.InvalidLicenseSignature:
			name = "RmExceptionInvalidLicenseSignature";
			break;
		case RightsManagementFailureCode.EncryptionNotPermitted:
			name = "RmExceptionEncryptionNotPermitted";
			break;
		case RightsManagementFailureCode.RightNotGranted:
			name = "RmExceptionRightNotGranted";
			break;
		case RightsManagementFailureCode.InvalidVersion:
			name = "RmExceptionInvalidVersion";
			break;
		case RightsManagementFailureCode.InvalidEncodingType:
			name = "RmExceptionInvalidEncodingType";
			break;
		case RightsManagementFailureCode.InvalidNumericalValue:
			name = "RmExceptionInvalidNumericalValue";
			break;
		case RightsManagementFailureCode.InvalidAlgorithmType:
			name = "RmExceptionInvalidAlgorithmType";
			break;
		case RightsManagementFailureCode.EnvironmentNotLoaded:
			name = "RmExceptionEnvironmentNotLoaded";
			break;
		case RightsManagementFailureCode.EnvironmentCannotLoad:
			name = "RmExceptionEnvironmentCannotLoad";
			break;
		case RightsManagementFailureCode.TooManyLoadedEnvironments:
			name = "RmExceptionTooManyLoadedEnvironments";
			break;
		case RightsManagementFailureCode.IncompatibleObjects:
			name = "RmExceptionIncompatibleObjects";
			break;
		case RightsManagementFailureCode.LibraryFail:
			name = "RmExceptionLibraryFail";
			break;
		case RightsManagementFailureCode.EnablingPrincipalFailure:
			name = "RmExceptionEnablingPrincipalFailure";
			break;
		case RightsManagementFailureCode.InfoNotPresent:
			name = "RmExceptionInfoNotPresent";
			break;
		case RightsManagementFailureCode.BadGetInfoQuery:
			name = "RmExceptionBadGetInfoQuery";
			break;
		case RightsManagementFailureCode.KeyTypeUnsupported:
			name = "RmExceptionKeyTypeUnsupported";
			break;
		case RightsManagementFailureCode.CryptoOperationUnsupported:
			name = "RmExceptionCryptoOperationUnsupported";
			break;
		case RightsManagementFailureCode.ClockRollbackDetected:
			name = "RmExceptionClockRollbackDetected";
			break;
		case RightsManagementFailureCode.QueryReportsNoResults:
			name = "RmExceptionQueryReportsNoResults";
			break;
		case RightsManagementFailureCode.UnexpectedException:
			name = "RmExceptionUnexpectedException";
			break;
		case RightsManagementFailureCode.BindValidityTimeViolated:
			name = "RmExceptionBindValidityTimeViolated";
			break;
		case RightsManagementFailureCode.BrokenCertChain:
			name = "RmExceptionBrokenCertChain";
			break;
		case RightsManagementFailureCode.BindPolicyViolation:
			name = "RmExceptionBindPolicyViolation";
			break;
		case RightsManagementFailureCode.ManifestPolicyViolation:
			name = "RmExceptionManifestPolicyViolation";
			break;
		case RightsManagementFailureCode.BindRevokedLicense:
			name = "RmExceptionBindRevokedLicense";
			break;
		case RightsManagementFailureCode.BindRevokedIssuer:
			name = "RmExceptionBindRevokedIssuer";
			break;
		case RightsManagementFailureCode.BindRevokedPrincipal:
			name = "RmExceptionBindRevokedPrincipal";
			break;
		case RightsManagementFailureCode.BindRevokedResource:
			name = "RmExceptionBindRevokedResource";
			break;
		case RightsManagementFailureCode.BindRevokedModule:
			name = "RmExceptionBindRevokedModule";
			break;
		case RightsManagementFailureCode.BindContentNotInEndUseLicense:
			name = "RmExceptionBindContentNotInEndUseLicense";
			break;
		case RightsManagementFailureCode.BindAccessPrincipalNotEnabling:
			name = "RmExceptionBindAccessPrincipalNotEnabling";
			break;
		case RightsManagementFailureCode.BindAccessUnsatisfied:
			name = "RmExceptionBindAccessUnsatisfied";
			break;
		case RightsManagementFailureCode.BindIndicatedPrincipalMissing:
			name = "RmExceptionBindIndicatedPrincipalMissing";
			break;
		case RightsManagementFailureCode.BindMachineNotFoundInGroupIdentity:
			name = "RmExceptionBindMachineNotFoundInGroupIdentity";
			break;
		case RightsManagementFailureCode.LibraryUnsupportedPlugIn:
			name = "RmExceptionLibraryUnsupportedPlugIn";
			break;
		case RightsManagementFailureCode.BindRevocationListStale:
			name = "RmExceptionBindRevocationListStale";
			break;
		case RightsManagementFailureCode.BindNoApplicableRevocationList:
			name = "RmExceptionBindNoApplicableRevocationList";
			break;
		case RightsManagementFailureCode.InvalidHandle:
			name = "RmExceptionInvalidHandle";
			break;
		case RightsManagementFailureCode.BindIntervalTimeViolated:
			name = "RmExceptionBindIntervalTimeViolated";
			break;
		case RightsManagementFailureCode.BindNoSatisfiedRightsGroup:
			name = "RmExceptionBindNoSatisfiedRightsGroup";
			break;
		case RightsManagementFailureCode.BindSpecifiedWorkMissing:
			name = "RmExceptionBindSpecifiedWorkMissing";
			break;
		case RightsManagementFailureCode.NoMoreData:
			name = "RmExceptionNoMoreData";
			break;
		case RightsManagementFailureCode.LicenseAcquisitionFailed:
			name = "RmExceptionLicenseAcquisitionFailed";
			break;
		case RightsManagementFailureCode.IdMismatch:
			name = "RmExceptionIdMismatch";
			break;
		case RightsManagementFailureCode.TooManyCertificates:
			name = "RmExceptionTooManyCertificates";
			break;
		case RightsManagementFailureCode.NoDistributionPointUrlFound:
			name = "RmExceptionNoDistributionPointUrlFound";
			break;
		case RightsManagementFailureCode.AlreadyInProgress:
			name = "RmExceptionAlreadyInProgress";
			break;
		case RightsManagementFailureCode.GroupIdentityNotSet:
			name = "RmExceptionGroupIdentityNotSet";
			break;
		case RightsManagementFailureCode.RecordNotFound:
			name = "RmExceptionRecordNotFound";
			break;
		case RightsManagementFailureCode.NoConnect:
			name = "RmExceptionNoConnect";
			break;
		case RightsManagementFailureCode.NoLicense:
			name = "RmExceptionNoLicense";
			break;
		case RightsManagementFailureCode.NeedsMachineActivation:
			name = "RmExceptionNeedsMachineActivation";
			break;
		case RightsManagementFailureCode.NeedsGroupIdentityActivation:
			name = "RmExceptionNeedsGroupIdentityActivation";
			break;
		case RightsManagementFailureCode.ActivationFailed:
			name = "RmExceptionActivationFailed";
			break;
		case RightsManagementFailureCode.Aborted:
			name = "RmExceptionAborted";
			break;
		case RightsManagementFailureCode.OutOfQuota:
			name = "RmExceptionOutOfQuota";
			break;
		case RightsManagementFailureCode.AuthenticationFailed:
			name = "RmExceptionAuthenticationFailed";
			break;
		case RightsManagementFailureCode.ServerError:
			name = "RmExceptionServerError";
			break;
		case RightsManagementFailureCode.InstallationFailed:
			name = "RmExceptionInstallationFailed";
			break;
		case RightsManagementFailureCode.HidCorrupted:
			name = "RmExceptionHidCorrupted";
			break;
		case RightsManagementFailureCode.InvalidServerResponse:
			name = "RmExceptionInvalidServerResponse";
			break;
		case RightsManagementFailureCode.ServiceNotFound:
			name = "RmExceptionServiceNotFound";
			break;
		case RightsManagementFailureCode.UseDefault:
			name = "RmExceptionUseDefault";
			break;
		case RightsManagementFailureCode.ServerNotFound:
			name = "RmExceptionServerNotFound";
			break;
		case RightsManagementFailureCode.InvalidEmail:
			name = "RmExceptionInvalidEmail";
			break;
		case RightsManagementFailureCode.ValidityTimeViolation:
			name = "RmExceptionValidityTimeViolation";
			break;
		case RightsManagementFailureCode.OutdatedModule:
			name = "RmExceptionOutdatedModule";
			break;
		case RightsManagementFailureCode.ServiceMoved:
			name = "RmExceptionServiceMoved";
			break;
		case RightsManagementFailureCode.ServiceGone:
			name = "RmExceptionServiceGone";
			break;
		case RightsManagementFailureCode.AdEntryNotFound:
			name = "RmExceptionAdEntryNotFound";
			break;
		case RightsManagementFailureCode.NotAChain:
			name = "RmExceptionNotAChain";
			break;
		case RightsManagementFailureCode.RequestDenied:
			name = "RmExceptionRequestDenied";
			break;
		case RightsManagementFailureCode.NotSet:
			name = "RmExceptionNotSet";
			break;
		case RightsManagementFailureCode.MetadataNotSet:
			name = "RmExceptionMetadataNotSet";
			break;
		case RightsManagementFailureCode.RevocationInfoNotSet:
			name = "RmExceptionRevocationInfoNotSet";
			break;
		case RightsManagementFailureCode.InvalidTimeInfo:
			name = "RmExceptionInvalidTimeInfo";
			break;
		case RightsManagementFailureCode.RightNotSet:
			name = "RmExceptionRightNotSet";
			break;
		case RightsManagementFailureCode.LicenseBindingToWindowsIdentityFailed:
			name = "RmExceptionLicenseBindingToWindowsIdentityFailed";
			break;
		case RightsManagementFailureCode.InvalidIssuanceLicenseTemplate:
			name = "RmExceptionInvalidIssuanceLicenseTemplate";
			break;
		case RightsManagementFailureCode.InvalidKeyLength:
			name = "RmExceptionInvalidKeyLength";
			break;
		case RightsManagementFailureCode.ExpiredOfficialIssuanceLicenseTemplate:
			name = "RmExceptionExpiredOfficialIssuanceLicenseTemplate";
			break;
		case RightsManagementFailureCode.InvalidClientLicensorCertificate:
			name = "RmExceptionInvalidClientLicensorCertificate";
			break;
		case RightsManagementFailureCode.HidInvalid:
			name = "RmExceptionHidInvalid";
			break;
		case RightsManagementFailureCode.EmailNotVerified:
			name = "RmExceptionEmailNotVerified";
			break;
		case RightsManagementFailureCode.DebuggerDetected:
			name = "RmExceptionDebuggerDetected";
			break;
		case RightsManagementFailureCode.InvalidLockboxType:
			name = "RmExceptionInvalidLockboxType";
			break;
		case RightsManagementFailureCode.InvalidLockboxPath:
			name = "RmExceptionInvalidLockboxPath";
			break;
		case RightsManagementFailureCode.InvalidRegistryPath:
			name = "RmExceptionInvalidRegistryPath";
			break;
		case RightsManagementFailureCode.NoAesCryptoProvider:
			name = "RmExceptionNoAesCryptoProvider";
			break;
		case RightsManagementFailureCode.GlobalOptionAlreadySet:
			name = "RmExceptionGlobalOptionAlreadySet";
			break;
		case RightsManagementFailureCode.OwnerLicenseNotFound:
			name = "RmExceptionOwnerLicenseNotFound";
			break;
		default:
			return null;
		}
		return SR.Get(name);
	}
}
