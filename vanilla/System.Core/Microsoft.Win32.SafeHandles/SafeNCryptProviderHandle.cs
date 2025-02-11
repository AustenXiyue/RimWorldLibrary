namespace Microsoft.Win32.SafeHandles;

/// <summary>Provides a safe handle that represents a key storage provider (NCRYPT_PROV_HANDLE).</summary>
public sealed class SafeNCryptProviderHandle : SafeNCryptHandle
{
	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.SafeHandles.SafeNCryptProviderHandle" /> class.</summary>
	public SafeNCryptProviderHandle()
	{
	}

	protected override bool ReleaseNativeHandle()
	{
		return false;
	}
}
