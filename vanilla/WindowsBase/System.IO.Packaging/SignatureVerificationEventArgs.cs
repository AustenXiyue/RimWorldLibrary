namespace System.IO.Packaging;

/// <summary> Specifies the event args provided to the <see cref="T:System.IO.Packaging.InvalidSignatureEventHandler" />. </summary>
public class SignatureVerificationEventArgs : EventArgs
{
	private PackageDigitalSignature _signature;

	private VerifyResult _result;

	/// <summary> Gets the digital signature being verified. </summary>
	/// <returns>The digital signature being verified.</returns>
	public PackageDigitalSignature Signature => _signature;

	/// <summary> Gets the signature verification error. </summary>
	/// <returns>The signature verification error.</returns>
	public VerifyResult VerifyResult => _result;

	internal SignatureVerificationEventArgs(PackageDigitalSignature signature, VerifyResult result)
	{
		if (signature == null)
		{
			throw new ArgumentNullException("signature");
		}
		if (result < VerifyResult.Success || result > VerifyResult.NotSigned)
		{
			throw new ArgumentOutOfRangeException("result");
		}
		_signature = signature;
		_result = result;
	}
}
