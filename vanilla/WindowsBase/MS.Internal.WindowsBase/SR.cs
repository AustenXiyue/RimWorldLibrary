using System;
using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.WindowsBase;

namespace MS.Internal.WindowsBase;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(FxResources.WindowsBase.SR)));

	internal static string WPF_UILanguage => GetResourceString("WPF_UILanguage");

	internal static string Rect_CannotModifyEmptyRect => GetResourceString("Rect_CannotModifyEmptyRect");

	internal static string Rect_CannotCallMethod => GetResourceString("Rect_CannotCallMethod");

	internal static string Size_WidthAndHeightCannotBeNegative => GetResourceString("Size_WidthAndHeightCannotBeNegative");

	internal static string Size_WidthCannotBeNegative => GetResourceString("Size_WidthCannotBeNegative");

	internal static string Size_HeightCannotBeNegative => GetResourceString("Size_HeightCannotBeNegative");

	internal static string Size_CannotModifyEmptySize => GetResourceString("Size_CannotModifyEmptySize");

	internal static string Transform_NotInvertible => GetResourceString("Transform_NotInvertible");

	internal static string General_Expected_Type => GetResourceString("General_Expected_Type");

	internal static string ReferenceIsNull => GetResourceString("ReferenceIsNull");

	internal static string ParameterMustBeBetween => GetResourceString("ParameterMustBeBetween");

	internal static string Freezable_UnregisteredHandler => GetResourceString("Freezable_UnregisteredHandler");

	internal static string Freezable_AttemptToUseInnerValueWithDifferentThread => GetResourceString("Freezable_AttemptToUseInnerValueWithDifferentThread");

	internal static string Freezable_CantFreeze => GetResourceString("Freezable_CantFreeze");

	internal static string Freezable_NotAContext => GetResourceString("Freezable_NotAContext");

	internal static string FrugalList_TargetMapCannotHoldAllData => GetResourceString("FrugalList_TargetMapCannotHoldAllData");

	internal static string FrugalList_CannotPromoteBeyondArray => GetResourceString("FrugalList_CannotPromoteBeyondArray");

	internal static string FrugalMap_TargetMapCannotHoldAllData => GetResourceString("FrugalMap_TargetMapCannotHoldAllData");

	internal static string FrugalMap_CannotPromoteBeyondHashtable => GetResourceString("FrugalMap_CannotPromoteBeyondHashtable");

	internal static string Unsupported_Key => GetResourceString("Unsupported_Key");

	internal static string InvalidPriority => GetResourceString("InvalidPriority");

	internal static string InvalidPriorityRangeOrder => GetResourceString("InvalidPriorityRangeOrder");

	internal static string DispatcherHasShutdown => GetResourceString("DispatcherHasShutdown");

	internal static string ThreadMayNotWaitOnOperationsAlreadyExecutingOnTheSameThread => GetResourceString("ThreadMayNotWaitOnOperationsAlreadyExecutingOnTheSameThread");

	internal static string VerifyAccess => GetResourceString("VerifyAccess");

	internal static string MismatchedDispatchers => GetResourceString("MismatchedDispatchers");

	internal static string DispatcherProcessingDisabledButStillPumping => GetResourceString("DispatcherProcessingDisabledButStillPumping");

	internal static string DispatcherProcessingDisabled => GetResourceString("DispatcherProcessingDisabled");

	internal static string DispatcherPriorityAwaiterInvalid => GetResourceString("DispatcherPriorityAwaiterInvalid");

	internal static string DispatcherYieldNoAvailableDispatcher => GetResourceString("DispatcherYieldNoAvailableDispatcher");

	internal static string DispatcherRequestProcessingFailed => GetResourceString("DispatcherRequestProcessingFailed");

	internal static string ExceptionFilterCodeNotPresent => GetResourceString("ExceptionFilterCodeNotPresent");

	internal static string Unsupported_Modifier => GetResourceString("Unsupported_Modifier");

	internal static string TimeSpanPeriodOutOfRange_TooSmall => GetResourceString("TimeSpanPeriodOutOfRange_TooSmall");

	internal static string TimeSpanPeriodOutOfRange_TooLarge => GetResourceString("TimeSpanPeriodOutOfRange_TooLarge");

	internal static string ClearOnReadOnlyObjectNotAllowed => GetResourceString("ClearOnReadOnlyObjectNotAllowed");

	internal static string DefaultValueAutoAssignFailed => GetResourceString("DefaultValueAutoAssignFailed");

	internal static string DefaultValueMayNotBeExpression => GetResourceString("DefaultValueMayNotBeExpression");

	internal static string DefaultValueMayNotBeUnset => GetResourceString("DefaultValueMayNotBeUnset");

	internal static string DefaultValueMustBeFreeThreaded => GetResourceString("DefaultValueMustBeFreeThreaded");

	internal static string DefaultValuePropertyTypeMismatch => GetResourceString("DefaultValuePropertyTypeMismatch");

	internal static string DefaultValueInvalid => GetResourceString("DefaultValueInvalid");

	internal static string DTypeNotSupportForSystemType => GetResourceString("DTypeNotSupportForSystemType");

	internal static string InvalidPropertyValue => GetResourceString("InvalidPropertyValue");

	internal static string LocalValueEnumerationOutOfBounds => GetResourceString("LocalValueEnumerationOutOfBounds");

	internal static string LocalValueEnumerationReset => GetResourceString("LocalValueEnumerationReset");

	internal static string LocalValueEnumerationInvalidated => GetResourceString("LocalValueEnumerationInvalidated");

	internal static string MissingCreateDefaultValue => GetResourceString("MissingCreateDefaultValue");

	internal static string OverridingMetadataDoesNotMatchBaseMetadataType => GetResourceString("OverridingMetadataDoesNotMatchBaseMetadataType");

	internal static string PropertyAlreadyRegistered => GetResourceString("PropertyAlreadyRegistered");

	internal static string PropertyNotReadOnly => GetResourceString("PropertyNotReadOnly");

	internal static string ReadOnlyChangeNotAllowed => GetResourceString("ReadOnlyChangeNotAllowed");

	internal static string ReadOnlyKeyNotAuthorized => GetResourceString("ReadOnlyKeyNotAuthorized");

	internal static string ReadOnlyOverrideNotAllowed => GetResourceString("ReadOnlyOverrideNotAllowed");

	internal static string ReadOnlyOverrideKeyNotAuthorized => GetResourceString("ReadOnlyOverrideKeyNotAuthorized");

	internal static string ReadOnlyDesignerCoersionNotAllowed => GetResourceString("ReadOnlyDesignerCoersionNotAllowed");

	internal static string SetOnReadOnlyObjectNotAllowed => GetResourceString("SetOnReadOnlyObjectNotAllowed");

	internal static string ShareableExpressionsCannotChangeSources => GetResourceString("ShareableExpressionsCannotChangeSources");

	internal static string SharingNonSharableExpression => GetResourceString("SharingNonSharableExpression");

	internal static string SpecialMethodMustBePublic => GetResourceString("SpecialMethodMustBePublic");

	internal static string SourcesMustBeInSameThread => GetResourceString("SourcesMustBeInSameThread");

	internal static string SourceChangeExpressionMismatch => GetResourceString("SourceChangeExpressionMismatch");

	internal static string TooManyDependencyProperties => GetResourceString("TooManyDependencyProperties");

	internal static string TypeMetadataAlreadyInUse => GetResourceString("TypeMetadataAlreadyInUse");

	internal static string TypeMetadataAlreadyRegistered => GetResourceString("TypeMetadataAlreadyRegistered");

	internal static string TypeMustBeDependencyObjectDerived => GetResourceString("TypeMustBeDependencyObjectDerived");

	internal static string UnknownExpressionMode => GetResourceString("UnknownExpressionMode");

	internal static string BufferTooSmall => GetResourceString("BufferTooSmall");

	internal static string BufferOffsetNegative => GetResourceString("BufferOffsetNegative");

	internal static string CompoundFilePathNullEmpty => GetResourceString("CompoundFilePathNullEmpty");

	internal static string CanNotCreateContainerOnReadOnlyStream => GetResourceString("CanNotCreateContainerOnReadOnlyStream");

	internal static string CanNotCreateAsReadOnly => GetResourceString("CanNotCreateAsReadOnly");

	internal static string CanNotCreateInReadOnly => GetResourceString("CanNotCreateInReadOnly");

	internal static string CanNotCreateStorageRootOnNonReadableStream => GetResourceString("CanNotCreateStorageRootOnNonReadableStream");

	internal static string CanNotDelete => GetResourceString("CanNotDelete");

	internal static string CanNotDeleteAccessDenied => GetResourceString("CanNotDeleteAccessDenied");

	internal static string CanNotCreateAccessDenied => GetResourceString("CanNotCreateAccessDenied");

	internal static string CanNotDeleteInReadOnly => GetResourceString("CanNotDeleteInReadOnly");

	internal static string CanNotDeleteNonEmptyStorage => GetResourceString("CanNotDeleteNonEmptyStorage");

	internal static string CanNotDeleteRoot => GetResourceString("CanNotDeleteRoot");

	internal static string CanNotOnNonExistStorage => GetResourceString("CanNotOnNonExistStorage");

	internal static string CanNotOpenStorage => GetResourceString("CanNotOpenStorage");

	internal static string ContainerNotFound => GetResourceString("ContainerNotFound");

	internal static string ContainerCanNotOpen => GetResourceString("ContainerCanNotOpen");

	internal static string CreateModeMustBeCreateOrOpen => GetResourceString("CreateModeMustBeCreateOrOpen");

	internal static string CFAPIFailure => GetResourceString("CFAPIFailure");

	internal static string DataSpaceLabelInUse => GetResourceString("DataSpaceLabelInUse");

	internal static string DataSpaceLabelInvalidEmpty => GetResourceString("DataSpaceLabelInvalidEmpty");

	internal static string DataSpaceLabelUndefined => GetResourceString("DataSpaceLabelUndefined");

	internal static string DataSpaceManagerDisposed => GetResourceString("DataSpaceManagerDisposed");

	internal static string DataSpaceMapEntryInvalid => GetResourceString("DataSpaceMapEntryInvalid");

	internal static string FileAccessInvalid => GetResourceString("FileAccessInvalid");

	internal static string FileAlreadyExists => GetResourceString("FileAlreadyExists");

	internal static string FileModeUnsupported => GetResourceString("FileModeUnsupported");

	internal static string FileModeInvalid => GetResourceString("FileModeInvalid");

	internal static string FileShareUnsupported => GetResourceString("FileShareUnsupported");

	internal static string FileShareInvalid => GetResourceString("FileShareInvalid");

	internal static string ILockBytesStreamMustSeek => GetResourceString("ILockBytesStreamMustSeek");

	internal static string InvalidArgumentValue => GetResourceString("InvalidArgumentValue");

	internal static string InvalidCondition01 => GetResourceString("InvalidCondition01");

	internal static string InvalidStringFormat => GetResourceString("InvalidStringFormat");

	internal static string InvalidTableType => GetResourceString("InvalidTableType");

	internal static string MoveToDestNotExist => GetResourceString("MoveToDestNotExist");

	internal static string MoveToNYI => GetResourceString("MoveToNYI");

	internal static string NameAlreadyInUse => GetResourceString("NameAlreadyInUse");

	internal static string NameCanNotHaveDelimiter => GetResourceString("NameCanNotHaveDelimiter");

	internal static string NamedAPIFailure => GetResourceString("NamedAPIFailure");

	internal static string NameTableCorruptStg => GetResourceString("NameTableCorruptStg");

	internal static string NameTableCorruptMem => GetResourceString("NameTableCorruptMem");

	internal static string NameTableVersionMismatchRead => GetResourceString("NameTableVersionMismatchRead");

	internal static string NameTableVersionMismatchWrite => GetResourceString("NameTableVersionMismatchWrite");

	internal static string NYIDefault => GetResourceString("NYIDefault");

	internal static string PathHasEmptyElement => GetResourceString("PathHasEmptyElement");

	internal static string ReadCountNegative => GetResourceString("ReadCountNegative");

	internal static string SeekFailed => GetResourceString("SeekFailed");

	internal static string SeekNegative => GetResourceString("SeekNegative");

	internal static string SeekOriginInvalid => GetResourceString("SeekOriginInvalid");

	internal static string StorageFlagsUnsupported => GetResourceString("StorageFlagsUnsupported");

	internal static string StorageAlreadyExist => GetResourceString("StorageAlreadyExist");

	internal static string StreamAlreadyExist => GetResourceString("StreamAlreadyExist");

	internal static string StorageInfoDisposed => GetResourceString("StorageInfoDisposed");

	internal static string StorageNotExist => GetResourceString("StorageNotExist");

	internal static string StorageRootDisposed => GetResourceString("StorageRootDisposed");

	internal static string StreamInfoDisposed => GetResourceString("StreamInfoDisposed");

	internal static string StreamLengthNegative => GetResourceString("StreamLengthNegative");

	internal static string StreamNotExist => GetResourceString("StreamNotExist");

	internal static string StreamNameNotValid => GetResourceString("StreamNameNotValid");

	internal static string StreamTimeStampNotImplemented => GetResourceString("StreamTimeStampNotImplemented");

	internal static string StringCanNotBeReservedName => GetResourceString("StringCanNotBeReservedName");

	internal static string TimeStampNotAvailable => GetResourceString("TimeStampNotAvailable");

	internal static string TransformLabelInUse => GetResourceString("TransformLabelInUse");

	internal static string TransformLabelUndefined => GetResourceString("TransformLabelUndefined");

	internal static string TransformObjectConstructorParam => GetResourceString("TransformObjectConstructorParam");

	internal static string TransformObjectImplementIDataTransform => GetResourceString("TransformObjectImplementIDataTransform");

	internal static string TransformObjectInitFailed => GetResourceString("TransformObjectInitFailed");

	internal static string TransformTypeUnsupported => GetResourceString("TransformTypeUnsupported");

	internal static string TransformStackValid => GetResourceString("TransformStackValid");

	internal static string UnableToCreateOnStream => GetResourceString("UnableToCreateOnStream");

	internal static string UnableToCreateStorage => GetResourceString("UnableToCreateStorage");

	internal static string UnableToCreateStream => GetResourceString("UnableToCreateStream");

	internal static string UnableToOpenStream => GetResourceString("UnableToOpenStream");

	internal static string UnsupportedTypeEncounteredWhenBuildingStgEnum => GetResourceString("UnsupportedTypeEncounteredWhenBuildingStgEnum");

	internal static string WriteFailure => GetResourceString("WriteFailure");

	internal static string WriteOnlyUnsupported => GetResourceString("WriteOnlyUnsupported");

	internal static string WriteSizeNegative => GetResourceString("WriteSizeNegative");

	internal static string CFM_CorruptMetadataStream => GetResourceString("CFM_CorruptMetadataStream");

	internal static string CFM_CorruptMetadataStream_Root => GetResourceString("CFM_CorruptMetadataStream_Root");

	internal static string CFM_CorruptMetadataStream_DuplicateKey => GetResourceString("CFM_CorruptMetadataStream_DuplicateKey");

	internal static string CFM_ObjectMustBeCompoundFileMetadataKey => GetResourceString("CFM_ObjectMustBeCompoundFileMetadataKey");

	internal static string CFM_ReadOnlyContainer => GetResourceString("CFM_ReadOnlyContainer");

	internal static string CFM_TypeTableFormat => GetResourceString("CFM_TypeTableFormat");

	internal static string CFM_UnicodeCharInvalid => GetResourceString("CFM_UnicodeCharInvalid");

	internal static string CFM_ValueMustBeString => GetResourceString("CFM_ValueMustBeString");

	internal static string CFM_XMLCharInvalid => GetResourceString("CFM_XMLCharInvalid");

	internal static string CanNotCompareDiffTypes => GetResourceString("CanNotCompareDiffTypes");

	internal static string CFRCorrupt => GetResourceString("CFRCorrupt");

	internal static string CFRCorruptMultiStream => GetResourceString("CFRCorruptMultiStream");

	internal static string CFRCorruptStgFollowStm => GetResourceString("CFRCorruptStgFollowStm");

	internal static string DelimiterLeading => GetResourceString("DelimiterLeading");

	internal static string DelimiterTrailing => GetResourceString("DelimiterTrailing");

	internal static string OffsetNegative => GetResourceString("OffsetNegative");

	internal static string UnknownReferenceComponentType => GetResourceString("UnknownReferenceComponentType");

	internal static string UnknownReferenceSerialize => GetResourceString("UnknownReferenceSerialize");

	internal static string MalformedCompoundFilePath => GetResourceString("MalformedCompoundFilePath");

	internal static string CannotMakeStreamLengthNegative => GetResourceString("CannotMakeStreamLengthNegative");

	internal static string CorruptStream => GetResourceString("CorruptStream");

	internal static string LengthNotSupported => GetResourceString("LengthNotSupported");

	internal static string ReadBufferTooSmall => GetResourceString("ReadBufferTooSmall");

	internal static string ReadNotSupported => GetResourceString("ReadNotSupported");

	internal static string SeekNotSupported => GetResourceString("SeekNotSupported");

	internal static string SetLengthNotSupported => GetResourceString("SetLengthNotSupported");

	internal static string SetPositionNotSupported => GetResourceString("SetPositionNotSupported");

	internal static string StreamPositionNegative => GetResourceString("StreamPositionNegative");

	internal static string TransformParametersFixed => GetResourceString("TransformParametersFixed");

	internal static string WriteBufferTooSmall => GetResourceString("WriteBufferTooSmall");

	internal static string WriteCountNegative => GetResourceString("WriteCountNegative");

	internal static string WriteNotSupported => GetResourceString("WriteNotSupported");

	internal static string ZLibVersionError => GetResourceString("ZLibVersionError");

	internal static string ExpectedVersionPairObject => GetResourceString("ExpectedVersionPairObject");

	internal static string VersionNumberComponentNegative => GetResourceString("VersionNumberComponentNegative");

	internal static string ZeroLengthFeatureID => GetResourceString("ZeroLengthFeatureID");

	internal static string VersionStreamMissing => GetResourceString("VersionStreamMissing");

	internal static string VersionUpdateFailure => GetResourceString("VersionUpdateFailure");

	internal static string CannotRemoveSignatureFromReadOnlyFile => GetResourceString("CannotRemoveSignatureFromReadOnlyFile");

	internal static string CannotSignReadOnlyFile => GetResourceString("CannotSignReadOnlyFile");

	internal static string DigSigCannotLocateCertificate => GetResourceString("DigSigCannotLocateCertificate");

	internal static string DigSigDuplicateCertificate => GetResourceString("DigSigDuplicateCertificate");

	internal static string CertSelectionDialogTitle => GetResourceString("CertSelectionDialogTitle");

	internal static string CertSelectionDialogMessage => GetResourceString("CertSelectionDialogMessage");

	internal static string DuplicateSignature => GetResourceString("DuplicateSignature");

	internal static string XmlSignatureParseError => GetResourceString("XmlSignatureParseError");

	internal static string RequiredXmlAttributeMissing => GetResourceString("RequiredXmlAttributeMissing");

	internal static string UnexpectedXmlTag => GetResourceString("UnexpectedXmlTag");

	internal static string RequiredTagNotFound => GetResourceString("RequiredTagNotFound");

	internal static string PackageSignatureObjectTagRequired => GetResourceString("PackageSignatureObjectTagRequired");

	internal static string PackageSignatureReferenceTagRequired => GetResourceString("PackageSignatureReferenceTagRequired");

	internal static string MoreThanOnePackageSpecificReference => GetResourceString("MoreThanOnePackageSpecificReference");

	internal static string InvalidUriAttribute => GetResourceString("InvalidUriAttribute");

	internal static string NoCounterSignUnsignedContainer => GetResourceString("NoCounterSignUnsignedContainer");

	internal static string BadSignatureTimeFormatString => GetResourceString("BadSignatureTimeFormatString");

	internal static string PackageSignatureCorruption => GetResourceString("PackageSignatureCorruption");

	internal static string UnsupportedHashAlgorithm => GetResourceString("UnsupportedHashAlgorithm");

	internal static string RelationshipTransformNotFollowedByCanonicalizationTransform => GetResourceString("RelationshipTransformNotFollowedByCanonicalizationTransform");

	internal static string MultipleRelationshipTransformsFound => GetResourceString("MultipleRelationshipTransformsFound");

	internal static string UnsupportedTransformAlgorithm => GetResourceString("UnsupportedTransformAlgorithm");

	internal static string UnsupportedCanonicalizationMethod => GetResourceString("UnsupportedCanonicalizationMethod");

	internal static string HashAlgorithmMustBeReusable => GetResourceString("HashAlgorithmMustBeReusable");

	internal static string PartReferenceUriMalformed => GetResourceString("PartReferenceUriMalformed");

	internal static string SignatureOriginNotFound => GetResourceString("SignatureOriginNotFound");

	internal static string MultipleSignatureOrigins => GetResourceString("MultipleSignatureOrigins");

	internal static string NothingToSign => GetResourceString("NothingToSign");

	internal static string EmptySignatureId => GetResourceString("EmptySignatureId");

	internal static string SignatureDeleted => GetResourceString("SignatureDeleted");

	internal static string SignaturePackageObjectTagMustBeUnique => GetResourceString("SignaturePackageObjectTagMustBeUnique");

	internal static string PackageSpecificReferenceTagMustBeUnique => GetResourceString("PackageSpecificReferenceTagMustBeUnique");

	internal static string SignatureObjectIdMustBeUnique => GetResourceString("SignatureObjectIdMustBeUnique");

	internal static string CanOnlyCounterSignSignatureParts => GetResourceString("CanOnlyCounterSignSignatureParts");

	internal static string CertificatePartContentTypeMismatch => GetResourceString("CertificatePartContentTypeMismatch");

	internal static string CertificateKeyTypeNotSupported => GetResourceString("CertificateKeyTypeNotSupported");

	internal static string PartToSignMissing => GetResourceString("PartToSignMissing");

	internal static string DuplicateObjectId => GetResourceString("DuplicateObjectId");

	internal static string CallbackParameterInvalid => GetResourceString("CallbackParameterInvalid");

	internal static string CannotChangePublishLicense => GetResourceString("CannotChangePublishLicense");

	internal static string CannotChangeCryptoProvider => GetResourceString("CannotChangeCryptoProvider");

	internal static string ExcessiveLengthPrefix => GetResourceString("ExcessiveLengthPrefix");

	internal static string GetOlePropertyFailed => GetResourceString("GetOlePropertyFailed");

	internal static string InvalidAuthenticationTypeString => GetResourceString("InvalidAuthenticationTypeString");

	internal static string InvalidDocumentPropertyType => GetResourceString("InvalidDocumentPropertyType");

	internal static string InvalidDocumentPropertyVariantType => GetResourceString("InvalidDocumentPropertyVariantType");

	internal static string InvalidTypePrefixedUserName => GetResourceString("InvalidTypePrefixedUserName");

	internal static string InvalidTransformFeatureName => GetResourceString("InvalidTransformFeatureName");

	internal static string PackageNotFound => GetResourceString("PackageNotFound");

	internal static string NoPublishLicenseStream => GetResourceString("NoPublishLicenseStream");

	internal static string NoUseLicenseStorage => GetResourceString("NoUseLicenseStorage");

	internal static string ReaderVersionError => GetResourceString("ReaderVersionError");

	internal static string PublishLicenseStreamCorrupt => GetResourceString("PublishLicenseStreamCorrupt");

	internal static string PublishLicenseNotFound => GetResourceString("PublishLicenseNotFound");

	internal static string RightsManagementEncryptionTransformNotFound => GetResourceString("RightsManagementEncryptionTransformNotFound");

	internal static string MultipleRightsManagementEncryptionTransformFound => GetResourceString("MultipleRightsManagementEncryptionTransformFound");

	internal static string StreamNeedsReadWriteAccess => GetResourceString("StreamNeedsReadWriteAccess");

	internal static string CryptoProviderCanNotDecrypt => GetResourceString("CryptoProviderCanNotDecrypt");

	internal static string CryptoProviderCanNotMergeBlocks => GetResourceString("CryptoProviderCanNotMergeBlocks");

	internal static string EncryptedPackageEnvelopeDisposed => GetResourceString("EncryptedPackageEnvelopeDisposed");

	internal static string CryptoProviderDisposed => GetResourceString("CryptoProviderDisposed");

	internal static string UpdaterVersionError => GetResourceString("UpdaterVersionError");

	internal static string DictionaryIsReadOnly => GetResourceString("DictionaryIsReadOnly");

	internal static string CryptoProviderIsNotReady => GetResourceString("CryptoProviderIsNotReady");

	internal static string UseLicenseStreamCorrupt => GetResourceString("UseLicenseStreamCorrupt");

	internal static string EncryptedDataStreamCorrupt => GetResourceString("EncryptedDataStreamCorrupt");

	internal static string UnknownDocumentProperty => GetResourceString("UnknownDocumentProperty");

	internal static string WrongDocumentPropertyVariantType => GetResourceString("WrongDocumentPropertyVariantType");

	internal static string UserIsNotActivated => GetResourceString("UserIsNotActivated");

	internal static string UserHasNoClientLicensorCert => GetResourceString("UserHasNoClientLicensorCert");

	internal static string EncryptionRightIsNotGranted => GetResourceString("EncryptionRightIsNotGranted");

	internal static string DecryptionRightIsNotGranted => GetResourceString("DecryptionRightIsNotGranted");

	internal static string NoPrivilegesForPublishLicenseDecryption => GetResourceString("NoPrivilegesForPublishLicenseDecryption");

	internal static string InvalidPublishLicense => GetResourceString("InvalidPublishLicense");

	internal static string PublishLicenseStreamHeaderTooLong => GetResourceString("PublishLicenseStreamHeaderTooLong");

	internal static string OnlyPassportOrWindowsAuthenticatedUsersAreAllowed => GetResourceString("OnlyPassportOrWindowsAuthenticatedUsersAreAllowed");

	internal static string RmExceptionGenericMessage => GetResourceString("RmExceptionGenericMessage");

	internal static string RmExceptionInvalidLicense => GetResourceString("RmExceptionInvalidLicense");

	internal static string RmExceptionInfoNotInLicense => GetResourceString("RmExceptionInfoNotInLicense");

	internal static string RmExceptionInvalidLicenseSignature => GetResourceString("RmExceptionInvalidLicenseSignature");

	internal static string RmExceptionEncryptionNotPermitted => GetResourceString("RmExceptionEncryptionNotPermitted");

	internal static string RmExceptionRightNotGranted => GetResourceString("RmExceptionRightNotGranted");

	internal static string RmExceptionInvalidVersion => GetResourceString("RmExceptionInvalidVersion");

	internal static string RmExceptionInvalidEncodingType => GetResourceString("RmExceptionInvalidEncodingType");

	internal static string RmExceptionInvalidNumericalValue => GetResourceString("RmExceptionInvalidNumericalValue");

	internal static string RmExceptionInvalidAlgorithmType => GetResourceString("RmExceptionInvalidAlgorithmType");

	internal static string RmExceptionEnvironmentNotLoaded => GetResourceString("RmExceptionEnvironmentNotLoaded");

	internal static string RmExceptionEnvironmentCannotLoad => GetResourceString("RmExceptionEnvironmentCannotLoad");

	internal static string RmExceptionTooManyLoadedEnvironments => GetResourceString("RmExceptionTooManyLoadedEnvironments");

	internal static string RmExceptionIncompatibleObjects => GetResourceString("RmExceptionIncompatibleObjects");

	internal static string RmExceptionLibraryFail => GetResourceString("RmExceptionLibraryFail");

	internal static string RmExceptionEnablingPrincipalFailure => GetResourceString("RmExceptionEnablingPrincipalFailure");

	internal static string RmExceptionInfoNotPresent => GetResourceString("RmExceptionInfoNotPresent");

	internal static string RmExceptionBadGetInfoQuery => GetResourceString("RmExceptionBadGetInfoQuery");

	internal static string RmExceptionKeyTypeUnsupported => GetResourceString("RmExceptionKeyTypeUnsupported");

	internal static string RmExceptionCryptoOperationUnsupported => GetResourceString("RmExceptionCryptoOperationUnsupported");

	internal static string RmExceptionClockRollbackDetected => GetResourceString("RmExceptionClockRollbackDetected");

	internal static string RmExceptionQueryReportsNoResults => GetResourceString("RmExceptionQueryReportsNoResults");

	internal static string RmExceptionUnexpectedException => GetResourceString("RmExceptionUnexpectedException");

	internal static string RmExceptionBindValidityTimeViolated => GetResourceString("RmExceptionBindValidityTimeViolated");

	internal static string RmExceptionBrokenCertChain => GetResourceString("RmExceptionBrokenCertChain");

	internal static string RmExceptionBindPolicyViolation => GetResourceString("RmExceptionBindPolicyViolation");

	internal static string RmExceptionManifestPolicyViolation => GetResourceString("RmExceptionManifestPolicyViolation");

	internal static string RmExceptionBindRevokedLicense => GetResourceString("RmExceptionBindRevokedLicense");

	internal static string RmExceptionBindRevokedIssuer => GetResourceString("RmExceptionBindRevokedIssuer");

	internal static string RmExceptionBindRevokedPrincipal => GetResourceString("RmExceptionBindRevokedPrincipal");

	internal static string RmExceptionBindRevokedResource => GetResourceString("RmExceptionBindRevokedResource");

	internal static string RmExceptionBindRevokedModule => GetResourceString("RmExceptionBindRevokedModule");

	internal static string RmExceptionBindContentNotInEndUseLicense => GetResourceString("RmExceptionBindContentNotInEndUseLicense");

	internal static string RmExceptionBindAccessPrincipalNotEnabling => GetResourceString("RmExceptionBindAccessPrincipalNotEnabling");

	internal static string RmExceptionBindAccessUnsatisfied => GetResourceString("RmExceptionBindAccessUnsatisfied");

	internal static string RmExceptionBindIndicatedPrincipalMissing => GetResourceString("RmExceptionBindIndicatedPrincipalMissing");

	internal static string RmExceptionBindMachineNotFoundInGroupIdentity => GetResourceString("RmExceptionBindMachineNotFoundInGroupIdentity");

	internal static string RmExceptionLibraryUnsupportedPlugIn => GetResourceString("RmExceptionLibraryUnsupportedPlugIn");

	internal static string RmExceptionBindRevocationListStale => GetResourceString("RmExceptionBindRevocationListStale");

	internal static string RmExceptionBindNoApplicableRevocationList => GetResourceString("RmExceptionBindNoApplicableRevocationList");

	internal static string RmExceptionInvalidHandle => GetResourceString("RmExceptionInvalidHandle");

	internal static string RmExceptionBindIntervalTimeViolated => GetResourceString("RmExceptionBindIntervalTimeViolated");

	internal static string RmExceptionBindNoSatisfiedRightsGroup => GetResourceString("RmExceptionBindNoSatisfiedRightsGroup");

	internal static string RmExceptionBindSpecifiedWorkMissing => GetResourceString("RmExceptionBindSpecifiedWorkMissing");

	internal static string RmExceptionNoMoreData => GetResourceString("RmExceptionNoMoreData");

	internal static string RmExceptionLicenseAcquisitionFailed => GetResourceString("RmExceptionLicenseAcquisitionFailed");

	internal static string RmExceptionIdMismatch => GetResourceString("RmExceptionIdMismatch");

	internal static string RmExceptionTooManyCertificates => GetResourceString("RmExceptionTooManyCertificates");

	internal static string RmExceptionNoDistributionPointUrlFound => GetResourceString("RmExceptionNoDistributionPointUrlFound");

	internal static string RmExceptionAlreadyInProgress => GetResourceString("RmExceptionAlreadyInProgress");

	internal static string RmExceptionGroupIdentityNotSet => GetResourceString("RmExceptionGroupIdentityNotSet");

	internal static string RmExceptionRecordNotFound => GetResourceString("RmExceptionRecordNotFound");

	internal static string RmExceptionNoConnect => GetResourceString("RmExceptionNoConnect");

	internal static string RmExceptionNoLicense => GetResourceString("RmExceptionNoLicense");

	internal static string RmExceptionNeedsMachineActivation => GetResourceString("RmExceptionNeedsMachineActivation");

	internal static string RmExceptionNeedsGroupIdentityActivation => GetResourceString("RmExceptionNeedsGroupIdentityActivation");

	internal static string RmExceptionActivationFailed => GetResourceString("RmExceptionActivationFailed");

	internal static string RmExceptionAborted => GetResourceString("RmExceptionAborted");

	internal static string RmExceptionOutOfQuota => GetResourceString("RmExceptionOutOfQuota");

	internal static string RmExceptionAuthenticationFailed => GetResourceString("RmExceptionAuthenticationFailed");

	internal static string RmExceptionServerError => GetResourceString("RmExceptionServerError");

	internal static string RmExceptionInstallationFailed => GetResourceString("RmExceptionInstallationFailed");

	internal static string RmExceptionHidCorrupted => GetResourceString("RmExceptionHidCorrupted");

	internal static string RmExceptionInvalidServerResponse => GetResourceString("RmExceptionInvalidServerResponse");

	internal static string RmExceptionServiceNotFound => GetResourceString("RmExceptionServiceNotFound");

	internal static string RmExceptionUseDefault => GetResourceString("RmExceptionUseDefault");

	internal static string RmExceptionServerNotFound => GetResourceString("RmExceptionServerNotFound");

	internal static string RmExceptionInvalidEmail => GetResourceString("RmExceptionInvalidEmail");

	internal static string RmExceptionValidityTimeViolation => GetResourceString("RmExceptionValidityTimeViolation");

	internal static string RmExceptionOutdatedModule => GetResourceString("RmExceptionOutdatedModule");

	internal static string RmExceptionServiceMoved => GetResourceString("RmExceptionServiceMoved");

	internal static string RmExceptionServiceGone => GetResourceString("RmExceptionServiceGone");

	internal static string RmExceptionAdEntryNotFound => GetResourceString("RmExceptionAdEntryNotFound");

	internal static string RmExceptionNotAChain => GetResourceString("RmExceptionNotAChain");

	internal static string RmExceptionRequestDenied => GetResourceString("RmExceptionRequestDenied");

	internal static string RmExceptionNotSet => GetResourceString("RmExceptionNotSet");

	internal static string RmExceptionMetadataNotSet => GetResourceString("RmExceptionMetadataNotSet");

	internal static string RmExceptionRevocationInfoNotSet => GetResourceString("RmExceptionRevocationInfoNotSet");

	internal static string RmExceptionInvalidTimeInfo => GetResourceString("RmExceptionInvalidTimeInfo");

	internal static string RmExceptionRightNotSet => GetResourceString("RmExceptionRightNotSet");

	internal static string RmExceptionLicenseBindingToWindowsIdentityFailed => GetResourceString("RmExceptionLicenseBindingToWindowsIdentityFailed");

	internal static string RmExceptionInvalidIssuanceLicenseTemplate => GetResourceString("RmExceptionInvalidIssuanceLicenseTemplate");

	internal static string RmExceptionInvalidKeyLength => GetResourceString("RmExceptionInvalidKeyLength");

	internal static string RmExceptionExpiredOfficialIssuanceLicenseTemplate => GetResourceString("RmExceptionExpiredOfficialIssuanceLicenseTemplate");

	internal static string RmExceptionInvalidClientLicensorCertificate => GetResourceString("RmExceptionInvalidClientLicensorCertificate");

	internal static string RmExceptionHidInvalid => GetResourceString("RmExceptionHidInvalid");

	internal static string RmExceptionEmailNotVerified => GetResourceString("RmExceptionEmailNotVerified");

	internal static string RmExceptionDebuggerDetected => GetResourceString("RmExceptionDebuggerDetected");

	internal static string RmExceptionInvalidLockboxType => GetResourceString("RmExceptionInvalidLockboxType");

	internal static string RmExceptionInvalidLockboxPath => GetResourceString("RmExceptionInvalidLockboxPath");

	internal static string RmExceptionInvalidRegistryPath => GetResourceString("RmExceptionInvalidRegistryPath");

	internal static string RmExceptionNoAesCryptoProvider => GetResourceString("RmExceptionNoAesCryptoProvider");

	internal static string RmExceptionGlobalOptionAlreadySet => GetResourceString("RmExceptionGlobalOptionAlreadySet");

	internal static string RmExceptionOwnerLicenseNotFound => GetResourceString("RmExceptionOwnerLicenseNotFound");

	internal static string ZipZeroSizeFileIsNotValidArchive => GetResourceString("ZipZeroSizeFileIsNotValidArchive");

	internal static string CanNotWriteInReadOnlyMode => GetResourceString("CanNotWriteInReadOnlyMode");

	internal static string CanNotReadInWriteOnlyMode => GetResourceString("CanNotReadInWriteOnlyMode");

	internal static string CanNotReadWriteInReadOnlyWriteOnlyMode => GetResourceString("CanNotReadWriteInReadOnlyWriteOnlyMode");

	internal static string AttemptedToCreateDuplicateFileName => GetResourceString("AttemptedToCreateDuplicateFileName");

	internal static string FileDoesNotExists => GetResourceString("FileDoesNotExists");

	internal static string TruncateAppendModesNotSupported => GetResourceString("TruncateAppendModesNotSupported");

	internal static string OnlyFileShareReadAndFileShareNoneSupported => GetResourceString("OnlyFileShareReadAndFileShareNoneSupported");

	internal static string CanNotReadDataFromStreamWhichDoesNotSupportReading => GetResourceString("CanNotReadDataFromStreamWhichDoesNotSupportReading");

	internal static string CanNotWriteDataToStreamWhichDoesNotSupportWriting => GetResourceString("CanNotWriteDataToStreamWhichDoesNotSupportWriting");

	internal static string CanNotOperateOnStreamWhichDoesNotSupportSeeking => GetResourceString("CanNotOperateOnStreamWhichDoesNotSupportSeeking");

	internal static string UnsupportedCombinationOfModeAccessShareStreaming => GetResourceString("UnsupportedCombinationOfModeAccessShareStreaming");

	internal static string CorruptedData => GetResourceString("CorruptedData");

	internal static string NotSupportedMultiDisk => GetResourceString("NotSupportedMultiDisk");

	internal static string ZipArchiveDisposed => GetResourceString("ZipArchiveDisposed");

	internal static string ZipFileItemDisposed => GetResourceString("ZipFileItemDisposed");

	internal static string NotSupportedVersionNeededToExtract => GetResourceString("NotSupportedVersionNeededToExtract");

	internal static string Zip64StructuresTooLarge => GetResourceString("Zip64StructuresTooLarge");

	internal static string ZipNotSupportedEncryptedArchive => GetResourceString("ZipNotSupportedEncryptedArchive");

	internal static string ZipNotSupportedSignedArchive => GetResourceString("ZipNotSupportedSignedArchive");

	internal static string ZipNotSupportedCompressionMethod => GetResourceString("ZipNotSupportedCompressionMethod");

	internal static string CompressLengthMismatch => GetResourceString("CompressLengthMismatch");

	internal static string CreateNewOnNonEmptyStream => GetResourceString("CreateNewOnNonEmptyStream");

	internal static string PartDoesNotExist => GetResourceString("PartDoesNotExist");

	internal static string PartAlreadyExists => GetResourceString("PartAlreadyExists");

	internal static string PartNamePrefixExists => GetResourceString("PartNamePrefixExists");

	internal static string IncompatibleModeOrAccess => GetResourceString("IncompatibleModeOrAccess");

	internal static string URIShouldNotBeAbsolute => GetResourceString("URIShouldNotBeAbsolute");

	internal static string UriShouldBeAbsolute => GetResourceString("UriShouldBeAbsolute");

	internal static string ContainerAndPartModeIncompatible => GetResourceString("ContainerAndPartModeIncompatible");

	internal static string UnsupportedCombinationOfModeAccess => GetResourceString("UnsupportedCombinationOfModeAccess");

	internal static string NullStreamReturned => GetResourceString("NullStreamReturned");

	internal static string ObjectDisposed => GetResourceString("ObjectDisposed");

	internal static string ReadOnlyStream => GetResourceString("ReadOnlyStream");

	internal static string WriteOnlyStream => GetResourceString("WriteOnlyStream");

	internal static string ParentContainerClosed => GetResourceString("ParentContainerClosed");

	internal static string PackagePartDeleted => GetResourceString("PackagePartDeleted");

	internal static string RelationshipToRelationshipIllegal => GetResourceString("RelationshipToRelationshipIllegal");

	internal static string RelationshipPartsCannotHaveRelationships => GetResourceString("RelationshipPartsCannotHaveRelationships");

	internal static string RelationshipPartIncorrectContentType => GetResourceString("RelationshipPartIncorrectContentType");

	internal static string PackageRelationshipDoesNotExist => GetResourceString("PackageRelationshipDoesNotExist");

	internal static string PackagePartRelationshipDoesNotExist => GetResourceString("PackagePartRelationshipDoesNotExist");

	internal static string RelationshipTargetMustBeRelative => GetResourceString("RelationshipTargetMustBeRelative");

	internal static string RequiredRelationshipAttributeMissing => GetResourceString("RequiredRelationshipAttributeMissing");

	internal static string RelationshipTagDoesntMatchSchema => GetResourceString("RelationshipTagDoesntMatchSchema");

	internal static string RelationshipsTagHasExtraAttributes => GetResourceString("RelationshipsTagHasExtraAttributes");

	internal static string UnknownTagEncountered => GetResourceString("UnknownTagEncountered");

	internal static string ExpectedRelationshipsElementTag => GetResourceString("ExpectedRelationshipsElementTag");

	internal static string InvalidXmlBaseAttributePresent => GetResourceString("InvalidXmlBaseAttributePresent");

	internal static string NotAUniqueRelationshipId => GetResourceString("NotAUniqueRelationshipId");

	internal static string NotAValidXmlIdString => GetResourceString("NotAValidXmlIdString");

	internal static string InvalidValueForTheAttribute => GetResourceString("InvalidValueForTheAttribute");

	internal static string InvalidRelationshipType => GetResourceString("InvalidRelationshipType");

	internal static string PartUriShouldStartWithForwardSlash => GetResourceString("PartUriShouldStartWithForwardSlash");

	internal static string PartUriShouldNotEndWithForwardSlash => GetResourceString("PartUriShouldNotEndWithForwardSlash");

	internal static string UriShouldBePackScheme => GetResourceString("UriShouldBePackScheme");

	internal static string PartUriIsEmpty => GetResourceString("PartUriIsEmpty");

	internal static string InvalidPartUri => GetResourceString("InvalidPartUri");

	internal static string RelationshipPartUriNotExpected => GetResourceString("RelationshipPartUriNotExpected");

	internal static string RelationshipPartUriExpected => GetResourceString("RelationshipPartUriExpected");

	internal static string NotAValidRelationshipPartUri => GetResourceString("NotAValidRelationshipPartUri");

	internal static string FragmentMustStartWithHash => GetResourceString("FragmentMustStartWithHash");

	internal static string PartUriCannotHaveAFragment => GetResourceString("PartUriCannotHaveAFragment");

	internal static string PartUriShouldNotStartWithTwoForwardSlashes => GetResourceString("PartUriShouldNotStartWithTwoForwardSlashes");

	internal static string InnerPackageUriHasFragment => GetResourceString("InnerPackageUriHasFragment");

	internal static string StreamObjectDisposed => GetResourceString("StreamObjectDisposed");

	internal static string NullContentTypeProvided => GetResourceString("NullContentTypeProvided");

	internal static string GetContentTypeCoreNotImplemented => GetResourceString("GetContentTypeCoreNotImplemented");

	internal static string RequiredAttributeMissing => GetResourceString("RequiredAttributeMissing");

	internal static string RequiredAttributeEmpty => GetResourceString("RequiredAttributeEmpty");

	internal static string TypesTagHasExtraAttributes => GetResourceString("TypesTagHasExtraAttributes");

	internal static string TypesElementExpected => GetResourceString("TypesElementExpected");

	internal static string TypesXmlDoesNotMatchSchema => GetResourceString("TypesXmlDoesNotMatchSchema");

	internal static string DefaultTagDoesNotMatchSchema => GetResourceString("DefaultTagDoesNotMatchSchema");

	internal static string OverrideTagDoesNotMatchSchema => GetResourceString("OverrideTagDoesNotMatchSchema");

	internal static string ElementIsNotEmptyElement => GetResourceString("ElementIsNotEmptyElement");

	internal static string BadPackageFormat => GetResourceString("BadPackageFormat");

	internal static string StreamingModeNotSupportedForConsumption => GetResourceString("StreamingModeNotSupportedForConsumption");

	internal static string StreamingPackageProductionImpliesWriteOnlyAccess => GetResourceString("StreamingPackageProductionImpliesWriteOnlyAccess");

	internal static string StreamingPackageProductionRequiresSingleWriter => GetResourceString("StreamingPackageProductionRequiresSingleWriter");

	internal static string MethodAvailableOnlyInStreamingCreation => GetResourceString("MethodAvailableOnlyInStreamingCreation");

	internal static string OperationIsNotSupportedInStreamingProduction => GetResourceString("OperationIsNotSupportedInStreamingProduction");

	internal static string OnlyWriteOperationsAreSupportedInStreamingCreation => GetResourceString("OnlyWriteOperationsAreSupportedInStreamingCreation");

	internal static string OperationViolatesWriteOnceSemantics => GetResourceString("OperationViolatesWriteOnceSemantics");

	internal static string OnlyStreamingProductionIsSupported => GetResourceString("OnlyStreamingProductionIsSupported");

	internal static string IOBufferOverflow => GetResourceString("IOBufferOverflow");

	internal static string StreamDoesNotSupportWrite => GetResourceString("StreamDoesNotSupportWrite");

	internal static string MoreThanOneMetadataRelationships => GetResourceString("MoreThanOneMetadataRelationships");

	internal static string NoExternalTargetForMetadataRelationship => GetResourceString("NoExternalTargetForMetadataRelationship");

	internal static string CorePropertiesElementExpected => GetResourceString("CorePropertiesElementExpected");

	internal static string NoStructuredContentInsideProperties => GetResourceString("NoStructuredContentInsideProperties");

	internal static string UnknownNamespaceInCorePropertiesPart => GetResourceString("UnknownNamespaceInCorePropertiesPart");

	internal static string InvalidPropertyNameInCorePropertiesPart => GetResourceString("InvalidPropertyNameInCorePropertiesPart");

	internal static string PropertyStartTagExpected => GetResourceString("PropertyStartTagExpected");

	internal static string XsdDateTimeExpected => GetResourceString("XsdDateTimeExpected");

	internal static string DanglingMetadataRelationship => GetResourceString("DanglingMetadataRelationship");

	internal static string WrongContentTypeForPropertyPart => GetResourceString("WrongContentTypeForPropertyPart");

	internal static string PropertyWrongNumbOfAttribsDefinedOn => GetResourceString("PropertyWrongNumbOfAttribsDefinedOn");

	internal static string UnknownDCDateTimeXsiType => GetResourceString("UnknownDCDateTimeXsiType");

	internal static string DuplicateCorePropertyName => GetResourceString("DuplicateCorePropertyName");

	internal static string StorageBasedPackagePropertiesDiposed => GetResourceString("StorageBasedPackagePropertiesDiposed");

	internal static string EncodingNotSupported => GetResourceString("EncodingNotSupported");

	internal static string DuplicatePiecesFound => GetResourceString("DuplicatePiecesFound");

	internal static string PieceDoesNotExist => GetResourceString("PieceDoesNotExist");

	internal static string ServiceTypeAlreadyAdded => GetResourceString("ServiceTypeAlreadyAdded");

	internal static string ParserAttributeArgsHigh => GetResourceString("ParserAttributeArgsHigh");

	internal static string ParserAttributeArgsLow => GetResourceString("ParserAttributeArgsLow");

	internal static string ParserAssemblyLoadVersionMismatch => GetResourceString("ParserAssemblyLoadVersionMismatch");

	internal static string ToStringNull => GetResourceString("ToStringNull");

	internal static string ConvertToException => GetResourceString("ConvertToException");

	internal static string ConvertFromException => GetResourceString("ConvertFromException");

	internal static string SortDescriptionPropertyNameCannotBeEmpty => GetResourceString("SortDescriptionPropertyNameCannotBeEmpty");

	internal static string CannotChangeAfterSealed => GetResourceString("CannotChangeAfterSealed");

	internal static string BadPropertyForGroup => GetResourceString("BadPropertyForGroup");

	internal static string CurrentChangingCannotBeCanceled => GetResourceString("CurrentChangingCannotBeCanceled");

	internal static string NotSupported_ReadOnlyCollection => GetResourceString("NotSupported_ReadOnlyCollection");

	internal static string Arg_RankMultiDimNotSupported => GetResourceString("Arg_RankMultiDimNotSupported");

	internal static string Arg_NonZeroLowerBound => GetResourceString("Arg_NonZeroLowerBound");

	internal static string ArgumentOutOfRange_NeedNonNegNum => GetResourceString("ArgumentOutOfRange_NeedNonNegNum");

	internal static string Arg_ArrayPlusOffTooSmall => GetResourceString("Arg_ArrayPlusOffTooSmall");

	internal static string Argument_InvalidArrayType => GetResourceString("Argument_InvalidArrayType");

	internal static string ReachOutOfRange => GetResourceString("ReachOutOfRange");

	internal static string InvalidPermissionState => GetResourceString("InvalidPermissionState");

	internal static string TargetNotWebBrowserPermissionLevel => GetResourceString("TargetNotWebBrowserPermissionLevel");

	internal static string TargetNotMediaPermissionLevel => GetResourceString("TargetNotMediaPermissionLevel");

	internal static string BadXml => GetResourceString("BadXml");

	internal static string InvalidPermissionLevel => GetResourceString("InvalidPermissionLevel");

	internal static string XCRChoiceOnlyInAC => GetResourceString("XCRChoiceOnlyInAC");

	internal static string XCRChoiceAfterFallback => GetResourceString("XCRChoiceAfterFallback");

	internal static string XCRRequiresAttribNotFound => GetResourceString("XCRRequiresAttribNotFound");

	internal static string XCRInvalidRequiresAttribute => GetResourceString("XCRInvalidRequiresAttribute");

	internal static string XCRFallbackOnlyInAC => GetResourceString("XCRFallbackOnlyInAC");

	internal static string XCRChoiceNotFound => GetResourceString("XCRChoiceNotFound");

	internal static string XCRMultipleFallbackFound => GetResourceString("XCRMultipleFallbackFound");

	internal static string XCRInvalidAttribInElement => GetResourceString("XCRInvalidAttribInElement");

	internal static string XCRUnknownCompatElement => GetResourceString("XCRUnknownCompatElement");

	internal static string XCRInvalidACChild => GetResourceString("XCRInvalidACChild");

	internal static string XCRInvalidFormat => GetResourceString("XCRInvalidFormat");

	internal static string XCRUndefinedPrefix => GetResourceString("XCRUndefinedPrefix");

	internal static string XCRUnknownCompatAttrib => GetResourceString("XCRUnknownCompatAttrib");

	internal static string XCRNSProcessContentNotIgnorable => GetResourceString("XCRNSProcessContentNotIgnorable");

	internal static string XCRDuplicateProcessContent => GetResourceString("XCRDuplicateProcessContent");

	internal static string XCRInvalidProcessContent => GetResourceString("XCRInvalidProcessContent");

	internal static string XCRDuplicateWildcardProcessContent => GetResourceString("XCRDuplicateWildcardProcessContent");

	internal static string XCRMustUnderstandFailed => GetResourceString("XCRMustUnderstandFailed");

	internal static string XCRNSPreserveNotIgnorable => GetResourceString("XCRNSPreserveNotIgnorable");

	internal static string XCRDuplicatePreserve => GetResourceString("XCRDuplicatePreserve");

	internal static string XCRInvalidPreserve => GetResourceString("XCRInvalidPreserve");

	internal static string XCRDuplicateWildcardPreserve => GetResourceString("XCRDuplicateWildcardPreserve");

	internal static string XCRInvalidXMLName => GetResourceString("XCRInvalidXMLName");

	internal static string XCRCompatCycle => GetResourceString("XCRCompatCycle");

	internal static string EventNotFound => GetResourceString("EventNotFound");

	internal static string ListenerDidNotHandleEvent => GetResourceString("ListenerDidNotHandleEvent");

	internal static string ListenerDidNotHandleEventDetail => GetResourceString("ListenerDidNotHandleEventDetail");

	internal static string NoMulticastHandlers => GetResourceString("NoMulticastHandlers");

	internal static string InvariantFailure => GetResourceString("InvariantFailure");

	internal static string ContentTypeCannotHaveLeadingTrailingLWS => GetResourceString("ContentTypeCannotHaveLeadingTrailingLWS");

	internal static string InvalidTypeSubType => GetResourceString("InvalidTypeSubType");

	internal static string ExpectingParameterValuePairs => GetResourceString("ExpectingParameterValuePairs");

	internal static string InvalidParameterValuePair => GetResourceString("InvalidParameterValuePair");

	internal static string InvalidToken => GetResourceString("InvalidToken");

	internal static string InvalidParameterValue => GetResourceString("InvalidParameterValue");

	internal static string InvalidLinearWhiteSpaceCharacter => GetResourceString("InvalidLinearWhiteSpaceCharacter");

	internal static string ExpectingSemicolon => GetResourceString("ExpectingSemicolon");

	internal static string HwndSubclassMultipleAttach => GetResourceString("HwndSubclassMultipleAttach");

	internal static string UnableToLocateResource => GetResourceString("UnableToLocateResource");

	internal static string SplashScreenIsLoading => GetResourceString("SplashScreenIsLoading");

	internal static string NameScopeNameNotEmptyString => GetResourceString("NameScopeNameNotEmptyString");

	internal static string NameScopeNameNotFound => GetResourceString("NameScopeNameNotFound");

	internal static string NameScopeDuplicateNamesNotAllowed => GetResourceString("NameScopeDuplicateNamesNotAllowed");

	internal static string NameScopeNotFound => GetResourceString("NameScopeNotFound");

	internal static string NameScopeInvalidIdentifierName => GetResourceString("NameScopeInvalidIdentifierName");

	internal static string NoDependencyProperty => GetResourceString("NoDependencyProperty");

	internal static string MarkupExtensionArrayType => GetResourceString("MarkupExtensionArrayType");

	internal static string MarkupExtensionArrayBadType => GetResourceString("MarkupExtensionArrayBadType");

	internal static string MarkupExtensionNoContext => GetResourceString("MarkupExtensionNoContext");

	internal static string MarkupExtensionBadStatic => GetResourceString("MarkupExtensionBadStatic");

	internal static string MarkupExtensionStaticMember => GetResourceString("MarkupExtensionStaticMember");

	internal static string MarkupExtensionTypeName => GetResourceString("MarkupExtensionTypeName");

	internal static string MarkupExtensionTypeNameBad => GetResourceString("MarkupExtensionTypeNameBad");

	internal static string MustBeOfType => GetResourceString("MustBeOfType");

	internal static string Verify_ApartmentState => GetResourceString("Verify_ApartmentState");

	internal static string Verify_NeitherNullNorEmpty => GetResourceString("Verify_NeitherNullNorEmpty");

	internal static string Verify_AreNotEqual => GetResourceString("Verify_AreNotEqual");

	internal static string Verify_FileExists => GetResourceString("Verify_FileExists");

	internal static string InvalidEvent => GetResourceString("InvalidEvent");

	internal static string CompatibilityPreferencesSealed => GetResourceString("CompatibilityPreferencesSealed");

	internal static string CombinationOfAccessibilitySwitchesNotSupported => GetResourceString("CombinationOfAccessibilitySwitchesNotSupported");

	internal static string AccessibilitySwitchDependencyNotSatisfied => GetResourceString("AccessibilitySwitchDependencyNotSatisfied");

	internal static string TokenizerHelperExtraDataEncountered => GetResourceString("TokenizerHelperExtraDataEncountered");

	internal static string TokenizerHelperPrematureStringTermination => GetResourceString("TokenizerHelperPrematureStringTermination");

	internal static string TokenizerHelperMissingEndQuote => GetResourceString("TokenizerHelperMissingEndQuote");

	internal static string TokenizerHelperEmptyToken => GetResourceString("TokenizerHelperEmptyToken");

	internal static string Enumerator_VerifyContext => GetResourceString("Enumerator_VerifyContext");

	internal static string InvalidPermissionStateValue => GetResourceString("InvalidPermissionStateValue");

	internal static string InvalidPermissionType => GetResourceString("InvalidPermissionType");

	internal static string StringEmpty => GetResourceString("StringEmpty");

	internal static string ParameterCannotBeNegative => GetResourceString("ParameterCannotBeNegative");

	internal static string Freezable_CantBeFrozen => GetResourceString("Freezable_CantBeFrozen");

	internal static string TypeMetadataCannotChangeAfterUse => GetResourceString("TypeMetadataCannotChangeAfterUse");

	internal static string Enum_Invalid => GetResourceString("Enum_Invalid");

	internal static string CannotConvertStringToType => GetResourceString("CannotConvertStringToType");

	internal static string CannotModifyReadOnlyContainer => GetResourceString("CannotModifyReadOnlyContainer");

	internal static string CannotRetrievePartsOfWriteOnlyContainer => GetResourceString("CannotRetrievePartsOfWriteOnlyContainer");

	internal static string FileFormatExceptionWithFileName => GetResourceString("FileFormatExceptionWithFileName");

	internal static string FileFormatException => GetResourceString("FileFormatException");

	internal static string Cryptography_InvalidHandle => GetResourceString("Cryptography_InvalidHandle");

	internal static string WpfDllConsistencyErrorData => GetResourceString("WpfDllConsistencyErrorData");

	internal static string WpfDllConsistencyErrorHeader => GetResourceString("WpfDllConsistencyErrorHeader");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey)
	{
		string result = null;
		try
		{
			result = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		return result;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string resourceString = GetResourceString(resourceKey);
		if (defaultString != null && resourceKey.Equals(resourceString, StringComparison.Ordinal))
		{
			return defaultString;
		}
		return resourceString;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}

	public static string Get(string name)
	{
		return GetResourceString(name, null);
	}

	public static string Get(string name, params object[] args)
	{
		return Format(GetResourceString(name, null), args);
	}
}
