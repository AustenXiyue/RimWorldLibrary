namespace Microsoft.Win32.SafeHandles;

/// <summary>Provides a safe handle that represents a secret agreement value (NCRYPT_SECRET_HANDLE).</summary>
public sealed class SafeNCryptSecretHandle : SafeNCryptHandle
{
	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeNCryptSecretHandle" /> class.</summary>
	public SafeNCryptSecretHandle()
	{
	}

	protected override bool ReleaseNativeHandle()
	{
		return false;
	}
}
