using System.Security.Permissions;
using Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents an X.509 store, which is a physical store where certificates are persisted and managed. This class cannot be inherited.</summary>
public sealed class X509Store : IDisposable
{
	private string _name;

	private StoreLocation _location;

	private X509Certificate2Collection list;

	private OpenFlags _flags;

	private Mono.Security.X509.X509Store store;

	/// <summary>Returns a collection of certificates located in an X.509 certificate store.</summary>
	/// <returns>A collection of certificates.</returns>
	public X509Certificate2Collection Certificates
	{
		get
		{
			if (list == null)
			{
				list = new X509Certificate2Collection();
			}
			else if (store == null)
			{
				list.Clear();
			}
			return list;
		}
	}

	/// <summary>Gets the location of the X.509 certificate store.</summary>
	/// <returns>The location of the certificate store.</returns>
	public StoreLocation Location => _location;

	/// <summary>Gets the name of the X.509 certificate store.</summary>
	/// <returns>The name of the certificate store.</returns>
	public string Name => _name;

	private X509Stores Factory
	{
		get
		{
			if (_location == StoreLocation.CurrentUser)
			{
				return X509StoreManager.CurrentUser;
			}
			return X509StoreManager.LocalMachine;
		}
	}

	private bool IsOpen => store != null;

	private bool IsReadOnly => (_flags & OpenFlags.ReadWrite) == 0;

	internal Mono.Security.X509.X509Store Store => store;

	/// <summary>Gets an <see cref="T:System.IntPtr" /> handle to an HCERTSTORE store.  </summary>
	/// <returns>A handle to an HCERTSTORE store.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The store is not open. </exception>
	[System.MonoTODO("Mono's stores are fully managed. Always returns IntPtr.Zero.")]
	public IntPtr StoreHandle => IntPtr.Zero;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using the personal certificates of the current user store.</summary>
	public X509Store()
		: this("MY", StoreLocation.CurrentUser)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using the specified store name.</summary>
	/// <param name="storeName">A string value that represents the store name. See <see cref="T:System.Security.Cryptography.X509Certificates.StoreName" />  for more information. </param>
	public X509Store(string storeName)
		: this(storeName, StoreLocation.CurrentUser)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using the specified <see cref="T:System.Security.Cryptography.X509Certificates.StoreName" /> value.</summary>
	/// <param name="storeName">One of the enumeration values that specifies the name of the X.509 certificate store. </param>
	public X509Store(StoreName storeName)
		: this(storeName, StoreLocation.CurrentUser)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using the specified <see cref="T:System.Security.Cryptography.X509Certificates.StoreLocation" /> value.</summary>
	/// <param name="storeLocation">One of the enumeration values that specifies the location of the X.509 certificate store. </param>
	public X509Store(StoreLocation storeLocation)
		: this("MY", storeLocation)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using the specified <see cref="T:System.Security.Cryptography.X509Certificates.StoreName" /> and <see cref="T:System.Security.Cryptography.X509Certificates.StoreLocation" /> values.</summary>
	/// <param name="storeName">One of the enumeration values that specifies the name of the X.509 certificate store. </param>
	/// <param name="storeLocation">One of the enumeration values that specifies the location of the X.509 certificate store. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="storeLocation" /> is not a valid location or <paramref name="storeName" /> is not a valid name. </exception>
	public X509Store(StoreName storeName, StoreLocation storeLocation)
	{
		if (storeName < StoreName.AddressBook || storeName > StoreName.TrustedPublisher)
		{
			throw new ArgumentException("storeName");
		}
		if (storeLocation < StoreLocation.CurrentUser || storeLocation > StoreLocation.LocalMachine)
		{
			throw new ArgumentException("storeLocation");
		}
		if (storeName == StoreName.CertificateAuthority)
		{
			_name = "CA";
		}
		else
		{
			_name = storeName.ToString();
		}
		_location = storeLocation;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using an Intptr handle to an HCERTSTORE store.</summary>
	/// <param name="storeHandle">A handle to an HCERTSTORE store.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="storeHandle" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="storeHandle" /> parameter points to an invalid context.</exception>
	[System.MonoTODO("Mono's stores are fully managed. All handles are invalid.")]
	[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
	public X509Store(IntPtr storeHandle)
	{
		if (storeHandle == IntPtr.Zero)
		{
			throw new ArgumentNullException("storeHandle");
		}
		throw new CryptographicException("Invalid handle.");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> class using a string that represents a value from the <see cref="T:System.Security.Cryptography.X509Certificates.StoreName" /> enumeration and a value from the <see cref="T:System.Security.Cryptography.X509Certificates.StoreLocation" /> enumeration.</summary>
	/// <param name="storeName">A string that represents a value from the <see cref="T:System.Security.Cryptography.X509Certificates.StoreName" /> enumeration. </param>
	/// <param name="storeLocation">One of the enumeration values that specifies the location of the X.509 certificate store. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="storeLocation" /> contains invalid values. </exception>
	public X509Store(string storeName, StoreLocation storeLocation)
	{
		if (storeLocation < StoreLocation.CurrentUser || storeLocation > StoreLocation.LocalMachine)
		{
			throw new ArgumentException("storeLocation");
		}
		_name = storeName;
		_location = storeLocation;
	}

	/// <summary>Adds a certificate to an X.509 certificate store.</summary>
	/// <param name="certificate">The certificate to add. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate could not be added to the store.</exception>
	public void Add(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (!IsOpen)
		{
			throw new CryptographicException(global::Locale.GetText("Store isn't opened."));
		}
		if (IsReadOnly)
		{
			throw new CryptographicException(global::Locale.GetText("Store is read-only."));
		}
		if (!Exists(certificate))
		{
			try
			{
				store.Import(new Mono.Security.X509.X509Certificate(certificate.RawData));
			}
			finally
			{
				Certificates.Add(certificate);
			}
		}
	}

	/// <summary>Adds a collection of certificates to an X.509 certificate store.</summary>
	/// <param name="certificates">The collection of certificates to add. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificates" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[System.MonoTODO("Method isn't transactional (like documented)")]
	public void AddRange(X509Certificate2Collection certificates)
	{
		if (certificates == null)
		{
			throw new ArgumentNullException("certificates");
		}
		if (certificates.Count == 0)
		{
			return;
		}
		if (!IsOpen)
		{
			throw new CryptographicException(global::Locale.GetText("Store isn't opened."));
		}
		if (IsReadOnly)
		{
			throw new CryptographicException(global::Locale.GetText("Store is read-only."));
		}
		X509Certificate2Enumerator enumerator = certificates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			X509Certificate2 current = enumerator.Current;
			if (!Exists(current))
			{
				try
				{
					store.Import(new Mono.Security.X509.X509Certificate(current.RawData));
				}
				finally
				{
					Certificates.Add(current);
				}
			}
		}
	}

	/// <summary>Closes an X.509 certificate store.</summary>
	public void Close()
	{
		store = null;
		if (list != null)
		{
			list.Clear();
		}
	}

	public void Dispose()
	{
		Close();
	}

	/// <summary>Opens an X.509 certificate store or creates a new store, depending on <see cref="T:System.Security.Cryptography.X509Certificates.OpenFlags" /> flag settings.</summary>
	/// <param name="flags">A bitwise combination of enumeration values that specifies the way to open the X.509 certificate store. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The store is unreadable. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">The store contains invalid values.</exception>
	public void Open(OpenFlags flags)
	{
		if (string.IsNullOrEmpty(_name))
		{
			throw new CryptographicException(global::Locale.GetText("Invalid store name (null or empty)."));
		}
		string name = _name;
		string storeName = ((!(name == "Root")) ? _name : "Trust");
		bool create = (flags & OpenFlags.OpenExistingOnly) != OpenFlags.OpenExistingOnly;
		store = Factory.Open(storeName, create);
		if (store == null)
		{
			throw new CryptographicException(global::Locale.GetText("Store {0} doesn't exists.", _name));
		}
		_flags = flags;
		foreach (Mono.Security.X509.X509Certificate certificate in store.Certificates)
		{
			X509Certificate2 x509Certificate = new X509Certificate2(certificate.RawData);
			x509Certificate.PrivateKey = certificate.RSA;
			Certificates.Add(x509Certificate);
		}
	}

	/// <summary>Removes a certificate from an X.509 certificate store.</summary>
	/// <param name="certificate">The certificate to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public void Remove(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (!IsOpen)
		{
			throw new CryptographicException(global::Locale.GetText("Store isn't opened."));
		}
		if (!Exists(certificate))
		{
			return;
		}
		if (IsReadOnly)
		{
			throw new CryptographicException(global::Locale.GetText("Store is read-only."));
		}
		try
		{
			store.Remove(new Mono.Security.X509.X509Certificate(certificate.RawData));
		}
		finally
		{
			Certificates.Remove(certificate);
		}
	}

	/// <summary>Removes a range of certificates from an X.509 certificate store.</summary>
	/// <param name="certificates">A range of certificates to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificates" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[System.MonoTODO("Method isn't transactional (like documented)")]
	public void RemoveRange(X509Certificate2Collection certificates)
	{
		if (certificates == null)
		{
			throw new ArgumentNullException("certificates");
		}
		if (certificates.Count == 0)
		{
			return;
		}
		if (!IsOpen)
		{
			throw new CryptographicException(global::Locale.GetText("Store isn't opened."));
		}
		bool flag = false;
		X509Certificate2Enumerator enumerator = certificates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			X509Certificate2 current = enumerator.Current;
			if (Exists(current))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		if (IsReadOnly)
		{
			throw new CryptographicException(global::Locale.GetText("Store is read-only."));
		}
		try
		{
			enumerator = certificates.GetEnumerator();
			while (enumerator.MoveNext())
			{
				X509Certificate2 current2 = enumerator.Current;
				store.Remove(new Mono.Security.X509.X509Certificate(current2.RawData));
			}
		}
		finally
		{
			Certificates.RemoveRange(certificates);
		}
	}

	private bool Exists(X509Certificate2 certificate)
	{
		if (store == null || list == null || certificate == null)
		{
			return false;
		}
		X509Certificate2Enumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			X509Certificate2 current = enumerator.Current;
			if (certificate.Equals(current))
			{
				return true;
			}
		}
		return false;
	}
}
