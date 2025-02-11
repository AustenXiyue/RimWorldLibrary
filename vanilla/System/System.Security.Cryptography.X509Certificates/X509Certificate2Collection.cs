using System.Globalization;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents a collection of <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects. This class cannot be inherited.</summary>
public class X509Certificate2Collection : X509CertificateCollection
{
	private static string[] newline_split = new string[1] { Environment.NewLine };

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is equal to or greater than the <see cref="P:System.Collections.CollectionBase.Count" /> property. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="index" /> is null. </exception>
	public new X509Certificate2 this[int index]
	{
		get
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("negative index");
			}
			if (index >= base.InnerList.Count)
			{
				throw new ArgumentOutOfRangeException("index >= Count");
			}
			return (X509Certificate2)base.InnerList[index];
		}
		set
		{
			base.InnerList[index] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> class without any <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> information.</summary>
	public X509Certificate2Collection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> class using the specified certificate collection.</summary>
	/// <param name="certificates">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object. </param>
	public X509Certificate2Collection(X509Certificate2Collection certificates)
	{
		AddRange(certificates);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> class using an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object.</summary>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object to start the collection from.</param>
	public X509Certificate2Collection(X509Certificate2 certificate)
	{
		Add(certificate);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> class using an array of <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects.</summary>
	/// <param name="certificates">An array of <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects. </param>
	public X509Certificate2Collection(X509Certificate2[] certificates)
	{
		AddRange(certificates);
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" />.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> index at which the <paramref name="certificate" /> has been added.</returns>
	/// <param name="certificate">An X.509 certificate represented as an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null. </exception>
	public int Add(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		return base.InnerList.Add(certificate);
	}

	/// <summary>Adds multiple <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects in an array to the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="certificates">An array of <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificates" /> is null. </exception>
	[System.MonoTODO("Method isn't transactional (like documented)")]
	public void AddRange(X509Certificate2[] certificates)
	{
		if (certificates == null)
		{
			throw new ArgumentNullException("certificates");
		}
		for (int i = 0; i < certificates.Length; i++)
		{
			base.InnerList.Add(certificates[i]);
		}
	}

	/// <summary>Adds multiple <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects in an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object to another <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="certificates">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificates" /> is null. </exception>
	[System.MonoTODO("Method isn't transactional (like documented)")]
	public void AddRange(X509Certificate2Collection certificates)
	{
		if (certificates == null)
		{
			throw new ArgumentNullException("certificates");
		}
		base.InnerList.AddRange(certificates);
	}

	/// <summary>Determines whether the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object contains a specific certificate.</summary>
	/// <returns>true if the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> contains the specified <paramref name="certificate" />; otherwise, false.</returns>
	/// <param name="certificate">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object to locate in the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null. </exception>
	public bool Contains(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		foreach (X509Certificate2 inner in base.InnerList)
		{
			if (inner.Equals(certificate))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Exports X.509 certificate information into a byte array.</summary>
	/// <returns>X.509 certificate information in a byte array.</returns>
	/// <param name="contentType">A supported <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> object. </param>
	[System.MonoTODO("only support X509ContentType.Cert")]
	public byte[] Export(X509ContentType contentType)
	{
		return Export(contentType, null);
	}

	/// <summary>Exports X.509 certificate information into a byte array using a password.</summary>
	/// <returns>X.509 certificate information in a byte array.</returns>
	/// <param name="contentType">A supported <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> object. </param>
	/// <param name="password">A string used to protect the byte array. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable, the content is invalid or, in the case of a certificate requiring a password, the private key could not be exported because the password provided was incorrect. </exception>
	[System.MonoTODO("only support X509ContentType.Cert")]
	public byte[] Export(X509ContentType contentType, string password)
	{
		switch (contentType)
		{
		case X509ContentType.Cert:
		case X509ContentType.SerializedCert:
		case X509ContentType.Pfx:
			if (base.Count > 0)
			{
				return this[base.Count - 1].Export(contentType, password);
			}
			break;
		default:
			throw new CryptographicException(global::Locale.GetText("Cannot export certificate(s) to the '{0}' format", contentType));
		case X509ContentType.SerializedStore:
		case X509ContentType.Pkcs7:
			break;
		}
		return null;
	}

	/// <summary>Searches an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object using the search criteria specified by the <see cref="T:System.Security.Cryptography.X509Certificates.X509FindType" /> enumeration and the <paramref name="findValue" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</returns>
	/// <param name="findType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509FindType" />  values. </param>
	/// <param name="findValue">The search criteria as an object. </param>
	/// <param name="validOnly">true to allow only valid certificates to be returned from the search; otherwise, false. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <paramref name="findType" /> is invalid. </exception>
	[System.MonoTODO("Does not support X509FindType.FindByTemplateName, FindByApplicationPolicy and FindByCertificatePolicy")]
	public X509Certificate2Collection Find(X509FindType findType, object findValue, bool validOnly)
	{
		if (findValue == null)
		{
			throw new ArgumentNullException("findValue");
		}
		string text = string.Empty;
		string text2 = string.Empty;
		X509KeyUsageFlags x509KeyUsageFlags = X509KeyUsageFlags.None;
		DateTime dateTime = DateTime.MinValue;
		switch (findType)
		{
		case X509FindType.FindByThumbprint:
		case X509FindType.FindBySubjectName:
		case X509FindType.FindBySubjectDistinguishedName:
		case X509FindType.FindByIssuerName:
		case X509FindType.FindByIssuerDistinguishedName:
		case X509FindType.FindBySerialNumber:
		case X509FindType.FindByTemplateName:
		case X509FindType.FindBySubjectKeyIdentifier:
			try
			{
				text = (string)findValue;
			}
			catch (Exception inner4)
			{
				throw new CryptographicException(global::Locale.GetText("Invalid find value type '{0}', expected '{1}'.", findValue.GetType(), "string"), inner4);
			}
			break;
		case X509FindType.FindByApplicationPolicy:
		case X509FindType.FindByCertificatePolicy:
		case X509FindType.FindByExtension:
			try
			{
				text2 = (string)findValue;
			}
			catch (Exception inner3)
			{
				throw new CryptographicException(global::Locale.GetText("Invalid find value type '{0}', expected '{1}'.", findValue.GetType(), "X509KeyUsageFlags"), inner3);
			}
			try
			{
				CryptoConfig.EncodeOID(text2);
			}
			catch (CryptographicUnexpectedOperationException)
			{
				string text3 = global::Locale.GetText("Invalid OID value '{0}'.", text2);
				throw new ArgumentException("findValue", text3);
			}
			break;
		case X509FindType.FindByKeyUsage:
			try
			{
				x509KeyUsageFlags = (X509KeyUsageFlags)findValue;
			}
			catch (Exception inner2)
			{
				throw new CryptographicException(global::Locale.GetText("Invalid find value type '{0}', expected '{1}'.", findValue.GetType(), "X509KeyUsageFlags"), inner2);
			}
			break;
		case X509FindType.FindByTimeValid:
		case X509FindType.FindByTimeNotYetValid:
		case X509FindType.FindByTimeExpired:
			try
			{
				dateTime = (DateTime)findValue;
			}
			catch (Exception inner)
			{
				throw new CryptographicException(global::Locale.GetText("Invalid find value type '{0}', expected '{1}'.", findValue.GetType(), "X509DateTime"), inner);
			}
			break;
		default:
			throw new CryptographicException(global::Locale.GetText("Invalid find type '{0}'.", findType));
		}
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
		foreach (X509Certificate2 inner5 in base.InnerList)
		{
			bool flag = false;
			switch (findType)
			{
			case X509FindType.FindByThumbprint:
				flag = string.Compare(text, inner5.Thumbprint, ignoreCase: true, invariantCulture) == 0 || string.Compare(text, inner5.GetCertHashString(), ignoreCase: true, invariantCulture) == 0;
				break;
			case X509FindType.FindBySubjectName:
			{
				string[] array = inner5.SubjectName.Format(multiLine: true).Split(newline_split, StringSplitOptions.RemoveEmptyEntries);
				foreach (string obj in array)
				{
					int startIndex = obj.IndexOf('=');
					flag = obj.IndexOf(text, startIndex, StringComparison.InvariantCultureIgnoreCase) >= 0;
					if (flag)
					{
						break;
					}
				}
				break;
			}
			case X509FindType.FindBySubjectDistinguishedName:
				flag = string.Compare(text, inner5.Subject, ignoreCase: true, invariantCulture) == 0;
				break;
			case X509FindType.FindByIssuerName:
				flag = inner5.GetNameInfo(X509NameType.SimpleName, forIssuer: true).IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0;
				break;
			case X509FindType.FindByIssuerDistinguishedName:
				flag = string.Compare(text, inner5.Issuer, ignoreCase: true, invariantCulture) == 0;
				break;
			case X509FindType.FindBySerialNumber:
				flag = string.Compare(text, inner5.SerialNumber, ignoreCase: true, invariantCulture) == 0;
				break;
			case X509FindType.FindBySubjectKeyIdentifier:
				if (inner5.Extensions["2.5.29.14"] is X509SubjectKeyIdentifierExtension x509SubjectKeyIdentifierExtension)
				{
					flag = string.Compare(text, x509SubjectKeyIdentifierExtension.SubjectKeyIdentifier, ignoreCase: true, invariantCulture) == 0;
				}
				break;
			case X509FindType.FindByApplicationPolicy:
				flag = inner5.Extensions.Count == 0;
				break;
			case X509FindType.FindByExtension:
				flag = inner5.Extensions[text2] != null;
				break;
			case X509FindType.FindByKeyUsage:
				flag = !(inner5.Extensions["2.5.29.15"] is X509KeyUsageExtension x509KeyUsageExtension) || (x509KeyUsageExtension.KeyUsages & x509KeyUsageFlags) == x509KeyUsageFlags;
				break;
			case X509FindType.FindByTimeValid:
				flag = dateTime >= inner5.NotBefore && dateTime <= inner5.NotAfter;
				break;
			case X509FindType.FindByTimeNotYetValid:
				flag = dateTime < inner5.NotBefore;
				break;
			case X509FindType.FindByTimeExpired:
				flag = dateTime > inner5.NotAfter;
				break;
			}
			if (!flag)
			{
				continue;
			}
			if (validOnly)
			{
				try
				{
					if (inner5.Verify())
					{
						x509Certificate2Collection.Add(inner5);
					}
				}
				catch
				{
				}
			}
			else
			{
				x509Certificate2Collection.Add(inner5);
			}
		}
		return x509Certificate2Collection;
	}

	/// <summary>Returns an enumerator that can iterate through a <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Enumerator" /> object that can iterate through the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</returns>
	public new X509Certificate2Enumerator GetEnumerator()
	{
		return new X509Certificate2Enumerator(this);
	}

	/// <summary>Imports a certificate in the form of a byte array into a <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	[System.MonoTODO("same limitations as X509Certificate2.Import")]
	public void Import(byte[] rawData)
	{
		X509Certificate2 x509Certificate = new X509Certificate2();
		x509Certificate.Import(rawData);
		Add(x509Certificate);
	}

	/// <summary>Imports a certificate, in the form of a byte array that requires a password to access the certificate, into a <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="rawData">A byte array containing data from an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object. </param>
	/// <param name="password">The password required to access the certificate information. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control how and where the certificate is imported. </param>
	[System.MonoTODO("same limitations as X509Certificate2.Import")]
	public void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		X509Certificate2 x509Certificate = new X509Certificate2();
		x509Certificate.Import(rawData, password, keyStorageFlags);
		Add(x509Certificate);
	}

	/// <summary>Imports a certificate file into a <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="fileName">The name of the file containing the certificate information. </param>
	[System.MonoTODO("same limitations as X509Certificate2.Import")]
	public void Import(string fileName)
	{
		X509Certificate2 x509Certificate = new X509Certificate2();
		x509Certificate.Import(fileName);
		Add(x509Certificate);
	}

	/// <summary>Imports a certificate file that requires a password into a <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="fileName">The name of the file containing the certificate information. </param>
	/// <param name="password">The password required to access the certificate information. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control how and where the certificate is imported. </param>
	[System.MonoTODO("same limitations as X509Certificate2.Import")]
	public void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		X509Certificate2 x509Certificate = new X509Certificate2();
		x509Certificate.Import(fileName, password, keyStorageFlags);
		Add(x509Certificate);
	}

	/// <summary>Inserts an object into the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object at the specified index.</summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="certificate" />. </param>
	/// <param name="certificate">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object to insert. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is greater than the <see cref="P:System.Collections.CollectionBase.Count" /> property. </exception>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.-or- The collection has a fixed size. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null. </exception>
	public void Insert(int index, X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("negative index");
		}
		if (index >= base.InnerList.Count)
		{
			throw new ArgumentOutOfRangeException("index >= Count");
		}
		base.InnerList.Insert(index, certificate);
	}

	/// <summary>Removes the first occurrence of a certificate from the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="certificate">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object to be removed from the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificate" /> is null. </exception>
	public void Remove(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			if (((X509Certificate)base.InnerList[i]).Equals(certificate))
			{
				base.InnerList.RemoveAt(i);
				break;
			}
		}
	}

	/// <summary>Removes multiple <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects in an array from an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="certificates">An array of <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificates" /> is null. </exception>
	[System.MonoTODO("Method isn't transactional (like documented)")]
	public void RemoveRange(X509Certificate2[] certificates)
	{
		if (certificates == null)
		{
			throw new ArgumentNullException("certificate");
		}
		foreach (X509Certificate2 certificate in certificates)
		{
			Remove(certificate);
		}
	}

	/// <summary>Removes multiple <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> objects in an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object from another <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object.</summary>
	/// <param name="certificates">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="certificates" /> is null. </exception>
	[System.MonoTODO("Method isn't transactional (like documented)")]
	public void RemoveRange(X509Certificate2Collection certificates)
	{
		if (certificates == null)
		{
			throw new ArgumentNullException("certificate");
		}
		X509Certificate2Enumerator enumerator = certificates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			X509Certificate2 current = enumerator.Current;
			Remove(current);
		}
	}
}
