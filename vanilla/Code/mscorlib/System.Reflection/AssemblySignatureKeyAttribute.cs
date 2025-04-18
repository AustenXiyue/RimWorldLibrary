namespace System.Reflection;

/// <summary>Provides migration from an older, simpler strong name key to a larger key with a stronger hashing algorithm.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class AssemblySignatureKeyAttribute : Attribute
{
	private string _publicKey;

	private string _countersignature;

	/// <summary>Gets the public key for the strong name used to sign the assembly.</summary>
	/// <returns>The public key for this assembly.</returns>
	public string PublicKey => _publicKey;

	/// <summary>Gets the countersignature for the strong name for this assembly.</summary>
	/// <returns>The countersignature for this signature key.</returns>
	public string Countersignature => _countersignature;

	/// <summary>Creates a new instance of the <see cref="T:System.Reflection.AssemblySignatureKeyAttribute" /> class by using the specified public key and countersignature.</summary>
	/// <param name="publicKey">The public or identity key.</param>
	/// <param name="countersignature">The countersignature, which is the signature key portion of the strong-name key.</param>
	public AssemblySignatureKeyAttribute(string publicKey, string countersignature)
	{
		_publicKey = publicKey;
		_countersignature = countersignature;
	}
}
