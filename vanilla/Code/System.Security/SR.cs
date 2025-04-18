internal class SR
{
	public const string ArgumentOutOfRange_Index = "Index was out of range.  Must be non-negative and less than the size of the collection.";

	public const string Arg_EmptyOrNullString = "String cannot be empty or null.";

	public const string Cryptography_Partial_Chain = "A certificate chain could not be built to a trusted root authority.";

	public const string Cryptography_Xml_BadWrappedKeySize = "Bad wrapped key size.";

	public const string Cryptography_Xml_CipherValueElementRequired = "A Cipher Data element should have either a CipherValue or a CipherReference element.";

	public const string Cryptography_Xml_CreateHashAlgorithmFailed = "Could not create hash algorithm object.";

	public const string Cryptography_Xml_CreateTransformFailed = "Could not create the XML transformation identified by the URI {0}.";

	public const string Cryptography_Xml_CreatedKeyFailed = "Failed to create signing key.";

	public const string Cryptography_Xml_DigestMethodRequired = "A DigestMethod must be specified on a Reference prior to generating XML.";

	public const string Cryptography_Xml_DigestValueRequired = "A Reference must contain a DigestValue.";

	public const string Cryptography_Xml_EnvelopedSignatureRequiresContext = "An XmlDocument context is required for enveloped transforms.";

	public const string Cryptography_Xml_InvalidElement = "Malformed element {0}.";

	public const string Cryptography_Xml_InvalidEncryptionProperty = "Malformed encryption property element.";

	public const string Cryptography_Xml_InvalidKeySize = "The key size should be a non negative integer.";

	public const string Cryptography_Xml_InvalidReference = "Malformed reference element.";

	public const string Cryptography_Xml_InvalidSignatureLength = "The length of the signature with a MAC should be less than the hash output length.";

	public const string Cryptography_Xml_InvalidSignatureLength2 = "The length in bits of the signature with a MAC should be a multiple of 8.";

	public const string Cryptography_Xml_InvalidX509IssuerSerialNumber = "X509 issuer serial number is invalid.";

	public const string Cryptography_Xml_KeyInfoRequired = "A KeyInfo element is required to check the signature.";

	public const string Cryptography_Xml_KW_BadKeySize = "The length of the encrypted data in Key Wrap is either 32, 40 or 48 bytes.";

	public const string Cryptography_Xml_LoadKeyFailed = "Signing key is not loaded.";

	public const string Cryptography_Xml_MissingAlgorithm = "Symmetric algorithm is not specified.";

	public const string Cryptography_Xml_MissingCipherData = "Cipher data is not specified.";

	public const string Cryptography_Xml_MissingDecryptionKey = "Unable to retrieve the decryption key.";

	public const string Cryptography_Xml_MissingEncryptionKey = "Unable to retrieve the encryption key.";

	public const string Cryptography_Xml_NotSupportedCryptographicTransform = "The specified cryptographic transform is not supported.";

	public const string Cryptography_Xml_ReferenceElementRequired = "At least one Reference element is required.";

	public const string Cryptography_Xml_ReferenceTypeRequired = "The Reference type must be set in an EncryptedReference object.";

	public const string Cryptography_Xml_SelfReferenceRequiresContext = "An XmlDocument context is required to resolve the Reference Uri {0}.";

	public const string Cryptography_Xml_SignatureDescriptionNotCreated = "SignatureDescription could not be created for the signature algorithm supplied.";

	public const string Cryptography_Xml_SignatureMethodKeyMismatch = "The key does not fit the SignatureMethod.";

	public const string Cryptography_Xml_SignatureMethodRequired = "A signature method is required.";

	public const string Cryptography_Xml_SignatureValueRequired = "Signature requires a SignatureValue.";

	public const string Cryptography_Xml_SignedInfoRequired = "Signature requires a SignedInfo.";

	public const string Cryptography_Xml_TransformIncorrectInputType = "The input type was invalid for this transform.";

	public const string Cryptography_Xml_IncorrectObjectType = "Type of input object is invalid.";

	public const string Cryptography_Xml_UnknownTransform = "Unknown transform has been encountered.";

	public const string Cryptography_Xml_UriNotResolved = "Unable to resolve Uri {0}.";

	public const string Cryptography_Xml_UriNotSupported = " The specified Uri is not supported.";

	public const string Cryptography_Xml_UriRequired = "A Uri attribute is required for a CipherReference element.";

	public const string Cryptography_Xml_XrmlMissingContext = "Null Context property encountered.";

	public const string Cryptography_Xml_XrmlMissingIRelDecryptor = "IRelDecryptor is required.";

	public const string Cryptography_Xml_XrmlMissingIssuer = "Issuer node is required.";

	public const string Cryptography_Xml_XrmlMissingLicence = "License node is required.";

	public const string Cryptography_Xml_XrmlUnableToDecryptGrant = "Unable to decrypt grant content.";

	public const string NotSupported_KeyAlgorithm = "The certificate key algorithm is not supported.";

	public const string Log_ActualHashValue = "Actual hash value: {0}";

	public const string Log_BeginCanonicalization = "Beginning canonicalization using \"{0}\" ({1}).";

	public const string Log_BeginSignatureComputation = "Beginning signature computation.";

	public const string Log_BeginSignatureVerification = "Beginning signature verification.";

	public const string Log_BuildX509Chain = "Building and verifying the X509 chain for certificate {0}.";

	public const string Log_CanonicalizationSettings = "Canonicalization transform is using resolver {0} and base URI \"{1}\".";

	public const string Log_CanonicalizedOutput = "Output of canonicalization transform: {0}";

	public const string Log_CertificateChain = "Certificate chain:";

	public const string Log_CheckSignatureFormat = "Checking signature format using format validator \"[{0}] {1}.{2}\".";

	public const string Log_CheckSignedInfo = "Checking signature on SignedInfo with id \"{0}\".";

	public const string Log_FormatValidationSuccessful = "Signature format validation was successful.";

	public const string Log_FormatValidationNotSuccessful = "Signature format validation failed.";

	public const string Log_KeyUsages = "Found key usages \"{0}\" in extension {1} on certificate {2}.";

	public const string Log_NoNamespacesPropagated = "No namespaces are being propagated.";

	public const string Log_PropagatingNamespace = "Propagating namespace {0}=\"{1}\".";

	public const string Log_RawSignatureValue = "Raw signature: {0}";

	public const string Log_ReferenceHash = "Reference {0} hashed with \"{1}\" ({2}) has hash value {3}, expected hash value {4}.";

	public const string Log_RevocationMode = "Revocation mode for chain building: {0}.";

	public const string Log_RevocationFlag = "Revocation flag for chain building: {0}.";

	public const string Log_SigningAsymmetric = "Calculating signature with key {0} using signature description {1}, hash algorithm {2}, and asymmetric signature formatter {3}.";

	public const string Log_SigningHmac = "Calculating signature using keyed hash algorithm {0}.";

	public const string Log_SigningReference = "Hashing reference {0}, Uri \"{1}\", Id \"{2}\", Type \"{3}\" with hash algorithm \"{4}\" ({5}).";

	public const string Log_TransformedReferenceContents = "Transformed reference contents: {0}";

	public const string Log_UnsafeCanonicalizationMethod = "Canonicalization method \"{0}\" is not on the safe list. Safe canonicalization methods are: {1}.";

	public const string Log_UrlTimeout = "URL retrieval timeout for chain building: {0}.";

	public const string Log_VerificationFailed = "Verification failed checking {0}.";

	public const string Log_VerificationFailed_References = "references";

	public const string Log_VerificationFailed_SignedInfo = "SignedInfo";

	public const string Log_VerificationFailed_X509Chain = "X509 chain verification";

	public const string Log_VerificationFailed_X509KeyUsage = "X509 key usage verification";

	public const string Log_VerificationFlag = "Verification flags for chain building: {0}.";

	public const string Log_VerificationTime = "Verification time for chain building: {0}.";

	public const string Log_VerificationWithKeySuccessful = "Verification with key {0} was successful.";

	public const string Log_VerificationWithKeyNotSuccessful = "Verification with key {0} was not successful.";

	public const string Log_VerifyReference = "Processing reference {0}, Uri \"{1}\", Id \"{2}\", Type \"{3}\".";

	public const string Log_VerifySignedInfoAsymmetric = "Verifying SignedInfo using key {0}, signature description {1}, hash algorithm {2}, and asymmetric signature deformatter {3}.";

	public const string Log_VerifySignedInfoHmac = "Verifying SignedInfo using keyed hash algorithm {0}.";

	public const string Log_X509ChainError = "Error building X509 chain: {0}: {1}.";

	public const string Log_XmlContext = "Using context: {0}";

	public const string Log_SignedXmlRecursionLimit = "Signed xml recursion limit hit while trying to decrypt the key. Reference {0} hashed with \"{1}\" and ({2}).";

	public const string Log_UnsafeTransformMethod = "Transform method \"{0}\" is not on the safe list. Safe transform methods are: {1}.";
}
