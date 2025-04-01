namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents the chain policy to be applied when building an X509 certificate chain. This class cannot be inherited.</summary>
public sealed class X509ChainPolicy
{
	private OidCollection apps;

	private OidCollection cert;

	private X509CertificateCollection store;

	private X509Certificate2Collection store2;

	private X509RevocationFlag rflag;

	private X509RevocationMode mode;

	private TimeSpan timeout;

	private X509VerificationFlags vflags;

	private DateTime vtime;

	/// <summary>Gets a collection of object identifiers (OIDs) specifying which application policies or enhanced key usages (EKUs) the certificate supports.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.OidCollection" />  object.</returns>
	public OidCollection ApplicationPolicy => apps;

	/// <summary>Gets a collection of object identifiers (OIDs) specifying which certificate policies the certificate supports.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.OidCollection" /> object.</returns>
	public OidCollection CertificatePolicy => cert;

	/// <summary>Represents an additional collection of certificates that can be searched by the chaining engine when validating a certificate chain.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</returns>
	public X509Certificate2Collection ExtraStore
	{
		get
		{
			if (store2 != null)
			{
				return store2;
			}
			store2 = new X509Certificate2Collection();
			if (store != null)
			{
				foreach (X509Certificate item in store)
				{
					store2.Add(new X509Certificate2(item));
				}
			}
			return store2;
		}
		internal set
		{
			store2 = value;
		}
	}

	/// <summary>Gets or sets values for X509 revocation flags.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509RevocationFlag" /> object.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Security.Cryptography.X509Certificates.X509RevocationFlag" /> value supplied is not a valid flag. </exception>
	public X509RevocationFlag RevocationFlag
	{
		get
		{
			return rflag;
		}
		set
		{
			if (value < X509RevocationFlag.EndCertificateOnly || value > X509RevocationFlag.ExcludeRoot)
			{
				throw new ArgumentException("RevocationFlag");
			}
			rflag = value;
		}
	}

	/// <summary>Gets or sets values for X509 certificate revocation mode.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509RevocationMode" /> object.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Security.Cryptography.X509Certificates.X509RevocationMode" /> value supplied is not a valid flag. </exception>
	public X509RevocationMode RevocationMode
	{
		get
		{
			return mode;
		}
		set
		{
			if (value < X509RevocationMode.NoCheck || value > X509RevocationMode.Offline)
			{
				throw new ArgumentException("RevocationMode");
			}
			mode = value;
		}
	}

	/// <summary>Gets the time span that elapsed during online revocation verification or downloading the certificate revocation list (CRL).</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> object.</returns>
	public TimeSpan UrlRetrievalTimeout
	{
		get
		{
			return timeout;
		}
		set
		{
			timeout = value;
		}
	}

	/// <summary>Gets verification flags for the certificate.</summary>
	/// <returns>A value from the <see cref="T:System.Security.Cryptography.X509Certificates.X509VerificationFlags" /> enumeration.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Security.Cryptography.X509Certificates.X509VerificationFlags" /> value supplied is not a valid flag. <see cref="F:System.Security.Cryptography.X509Certificates.X509VerificationFlags.NoFlag" /> is the default value. </exception>
	public X509VerificationFlags VerificationFlags
	{
		get
		{
			return vflags;
		}
		set
		{
			if ((value | X509VerificationFlags.AllFlags) != X509VerificationFlags.AllFlags)
			{
				throw new ArgumentException("VerificationFlags");
			}
			vflags = value;
		}
	}

	/// <summary>The time that the certificate was verified expressed in local time.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object.</returns>
	public DateTime VerificationTime
	{
		get
		{
			return vtime;
		}
		set
		{
			vtime = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainPolicy" /> class. </summary>
	public X509ChainPolicy()
	{
		Reset();
	}

	internal X509ChainPolicy(X509CertificateCollection store)
	{
		this.store = store;
		Reset();
	}

	/// <summary>Resets the <see cref="T:System.Security.Cryptography.X509Certificates.X509ChainPolicy" /> members to their default values.</summary>
	public void Reset()
	{
		apps = new OidCollection();
		cert = new OidCollection();
		store2 = null;
		rflag = X509RevocationFlag.ExcludeRoot;
		mode = X509RevocationMode.Online;
		timeout = TimeSpan.Zero;
		vflags = X509VerificationFlags.NoFlag;
		vtime = DateTime.Now;
	}
}
