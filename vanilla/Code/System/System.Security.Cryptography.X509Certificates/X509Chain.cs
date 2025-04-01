using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents a chain-building engine for <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> certificates.</summary>
public class X509Chain : IDisposable
{
	private X509ChainImpl impl;

	internal X509ChainImpl Impl
	{
		get
		{
			X509Helper2.ThrowIfContextInvalid(impl);
			return impl;
		}
	}

	internal bool IsValid => X509Helper2.IsValid(impl);

	/// <summary>Gets a handle to an X.509 chain.</summary>
	/// <returns>An <see cref="T:System.IntPtr" /> handle to an X.509 chain.</returns>
	[System.MonoTODO("Mono's X509Chain is fully managed. Always returns IntPtr.Zero.")]
	public IntPtr ChainContext
	{
		get
		{
			if (impl != null && impl.IsValid)
			{
				return impl.Handle;
			}
			return IntPtr.Zero;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElement" /> objects.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainElementCollection" /> object.</returns>
	public X509ChainElementCollection ChainElements => Impl.ChainElements;

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainPolicy" /> to use when building an X.509 certificate chain.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainPolicy" /> object associated with this X.509 chain.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value being set for this property is null.</exception>
	public X509ChainPolicy ChainPolicy
	{
		get
		{
			return Impl.ChainPolicy;
		}
		set
		{
			Impl.ChainPolicy = value;
		}
	}

	/// <summary>Gets the status of each element in an <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> object.</summary>
	/// <returns>An array of <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainStatus" /> objects.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public X509ChainStatus[] ChainStatus => Impl.ChainStatus;

	public SafeX509ChainHandle SafeHandle
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal void ThrowIfContextInvalid()
	{
		X509Helper2.ThrowIfContextInvalid(impl);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> class.</summary>
	public X509Chain()
		: this(useMachineContext: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> class specifying a value that indicates whether the machine context should be used.</summary>
	/// <param name="useMachineContext">true to use the machine context; false to use the current user context. </param>
	public X509Chain(bool useMachineContext)
	{
		impl = X509Helper2.CreateChainImpl(useMachineContext);
	}

	internal X509Chain(X509ChainImpl impl)
	{
		X509Helper2.ThrowIfContextInvalid(impl);
		this.impl = impl;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> class using an <see cref="T:System.IntPtr" /> handle to an X.509 chain.</summary>
	/// <param name="chainContext">An <see cref="T:System.IntPtr" /> handle to an X.509 chain.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="chainContext" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="chainContext" /> parameter points to an invalid context.</exception>
	[System.MonoTODO("Mono's X509Chain is fully managed. All handles are invalid.")]
	public X509Chain(IntPtr chainContext)
	{
		throw new NotSupportedException();
	}

	/// <summary>Builds an X.509 chain using the policy specified in <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainPolicy" />.</summary>
	/// <returns>true if the X.509 certificate is valid; otherwise, false.</returns>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="certificate" /> is not a valid certificate or is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="certificate" /> is unreadable. </exception>
	[System.MonoTODO("Not totally RFC3280 compliant, but neither is MS implementation...")]
	public bool Build(X509Certificate2 certificate)
	{
		return Impl.Build(certificate);
	}

	/// <summary>Clears the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> object.</summary>
	public void Reset()
	{
		Impl.Reset();
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> object after querying for the mapping defined in the CryptoConfig file, and maps the chain to that mapping.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Chain" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static X509Chain Create()
	{
		return (X509Chain)CryptoConfig.CreateFromName("X509Chain");
	}

	[SecuritySafeCritical]
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (impl != null)
		{
			impl.Dispose();
			impl = null;
		}
	}

	~X509Chain()
	{
		Dispose(disposing: false);
	}
}
