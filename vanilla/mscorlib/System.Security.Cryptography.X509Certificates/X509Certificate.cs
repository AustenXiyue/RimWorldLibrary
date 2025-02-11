using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using Mono.Security.Authenticode;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Provides methods that help you use X.509 v.3 certificates.</summary>
[Serializable]
[ComVisible(true)]
[MonoTODO("X509ContentType.SerializedCert isn't supported (anywhere in the class)")]
public class X509Certificate : IDeserializationCallback, ISerializable, IDisposable
{
	private X509CertificateImpl impl;

	private bool hideDates;

	private string issuer_name;

	private string subject_name;

	internal X509CertificateImpl Impl
	{
		get
		{
			X509Helper.ThrowIfContextInvalid(impl);
			return impl;
		}
	}

	internal bool IsValid => X509Helper.IsValid(impl);

	/// <summary>Gets the name of the certificate authority that issued the X.509v3 certificate.</summary>
	/// <returns>The name of the certificate authority that issued the X.509v3 certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate handle is invalid.</exception>
	public string Issuer
	{
		get
		{
			X509Helper.ThrowIfContextInvalid(impl);
			if (issuer_name == null)
			{
				issuer_name = impl.GetIssuerName(legacyV1Mode: false);
			}
			return issuer_name;
		}
	}

	/// <summary>Gets the subject distinguished name from the certificate.</summary>
	/// <returns>The subject distinguished name from the certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate handle is invalid.</exception>
	public string Subject
	{
		get
		{
			X509Helper.ThrowIfContextInvalid(impl);
			if (subject_name == null)
			{
				subject_name = impl.GetSubjectName(legacyV1Mode: false);
			}
			return subject_name;
		}
	}

	/// <summary>Gets a handle to a Microsoft Cryptographic API certificate context described by an unmanaged PCCERT_CONTEXT structure. </summary>
	/// <returns>An <see cref="T:System.IntPtr" /> structure that represents an unmanaged PCCERT_CONTEXT structure.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public IntPtr Handle
	{
		get
		{
			if (X509Helper.IsValid(impl))
			{
				return impl.Handle;
			}
			return IntPtr.Zero;
		}
	}

	/// <summary>Creates an X.509v3 certificate from the specified PKCS7 signed file.</summary>
	/// <returns>The newly created X.509 certificate.</returns>
	/// <param name="filename">The path of the PKCS7 signed file from which to create the X.509 certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="filename" /> parameter is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public static X509Certificate CreateFromCertFile(string filename)
	{
		return new X509Certificate(File.ReadAllBytes(filename));
	}

	/// <summary>Creates an X.509v3 certificate from the specified signed file.</summary>
	/// <returns>The newly created X.509 certificate.</returns>
	/// <param name="filename">The path of the signed file from which to create the X.509 certificate. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[MonoTODO("Incomplete - minimal validation in this version")]
	public static X509Certificate CreateFromSignedFile(string filename)
	{
		try
		{
			AuthenticodeDeformatter authenticodeDeformatter = new AuthenticodeDeformatter(filename);
			if (authenticodeDeformatter.SigningCertificate != null)
			{
				return new X509Certificate(authenticodeDeformatter.SigningCertificate.RawData);
			}
		}
		catch (SecurityException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new COMException(Locale.GetText("Couldn't extract digital signature from {0}.", filename), inner);
		}
		throw new CryptographicException(Locale.GetText("{0} isn't signed.", filename));
	}

	internal X509Certificate(byte[] data, bool dates)
	{
		if (data != null)
		{
			Import(data, (string)null, X509KeyStorageFlags.DefaultKeySet);
			hideDates = !dates;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class defined from a sequence of bytes representing an X.509v3 certificate.</summary>
	/// <param name="data">A byte array containing data from an X.509 certificate.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public X509Certificate(byte[] data)
		: this(data, dates: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a handle to an unmanaged PCCERT_CONTEXT structure.</summary>
	/// <param name="handle">A handle to an unmanaged PCCERT_CONTEXT structure.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The handle parameter does not represent a valid PCCERT_CONTEXT structure.</exception>
	public X509Certificate(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			throw new ArgumentException("Invalid handle.");
		}
		impl = X509Helper.InitFromHandle(handle);
	}

	internal X509Certificate(X509CertificateImpl impl)
	{
		if (impl == null)
		{
			throw new ArgumentNullException("impl");
		}
		this.impl = X509Helper.InitFromCertificate(impl);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using another <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class.</summary>
	/// <param name="cert">A <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class from which to initialize this class. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="cert" /> parameter is null.</exception>
	public X509Certificate(X509Certificate cert)
	{
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		impl = X509Helper.InitFromCertificate(cert);
		hideDates = false;
	}

	internal void ImportHandle(X509CertificateImpl impl)
	{
		Reset();
		this.impl = impl;
	}

	internal void ThrowIfContextInvalid()
	{
		X509Helper.ThrowIfContextInvalid(impl);
	}

	/// <summary>Compares two <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> objects for equality.</summary>
	/// <returns>true if the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object is equal to the object specified by the <paramref name="other" /> parameter; otherwise, false.</returns>
	/// <param name="other">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to compare to the current object.</param>
	public virtual bool Equals(X509Certificate other)
	{
		if (other == null)
		{
			return false;
		}
		if (!X509Helper.IsValid(other.impl))
		{
			if (!X509Helper.IsValid(impl))
			{
				return true;
			}
			throw new CryptographicException(Locale.GetText("Certificate instance is empty."));
		}
		return object.Equals(impl, other.impl);
	}

	/// <summary>Returns the hash value for the X.509v3 certificate as an array of bytes.</summary>
	/// <returns>The hash value for the X.509 certificate.</returns>
	public virtual byte[] GetCertHash()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetCertHash();
	}

	/// <summary>Returns the SHA1 hash value for the X.509v3 certificate as a hexadecimal string.</summary>
	/// <returns>The hexadecimal string representation of the X.509 certificate hash value.</returns>
	public virtual string GetCertHashString()
	{
		return X509Helper.ToHexString(GetCertHash());
	}

	/// <summary>Returns the effective date of this X.509v3 certificate.</summary>
	/// <returns>The effective date for this X.509 certificate.</returns>
	public virtual string GetEffectiveDateString()
	{
		if (hideDates)
		{
			return null;
		}
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetValidFrom().ToLocalTime().ToString();
	}

	/// <summary>Returns the expiration date of this X.509v3 certificate.</summary>
	/// <returns>The expiration date for this X.509 certificate.</returns>
	public virtual string GetExpirationDateString()
	{
		if (hideDates)
		{
			return null;
		}
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetValidUntil().ToLocalTime().ToString();
	}

	/// <summary>Returns the name of the format of this X.509v3 certificate.</summary>
	/// <returns>The format of this X.509 certificate.</returns>
	public virtual string GetFormat()
	{
		return "X509";
	}

	/// <summary>Returns the hash code for the X.509v3 certificate as an integer.</summary>
	/// <returns>The hash code for the X.509 certificate as an integer.</returns>
	public override int GetHashCode()
	{
		if (!X509Helper.IsValid(impl))
		{
			return 0;
		}
		return impl.GetHashCode();
	}

	/// <summary>Returns the name of the certification authority that issued the X.509v3 certificate.</summary>
	/// <returns>The name of the certification authority that issued the X.509 certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	[Obsolete("Use the Issuer property.")]
	public virtual string GetIssuerName()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetIssuerName(legacyV1Mode: true);
	}

	/// <summary>Returns the key algorithm information for this X.509v3 certificate.</summary>
	/// <returns>The key algorithm information for this X.509 certificate as a string.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual string GetKeyAlgorithm()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetKeyAlgorithm();
	}

	/// <summary>Returns the key algorithm parameters for the X.509v3 certificate.</summary>
	/// <returns>The key algorithm parameters for the X.509 certificate as an array of bytes.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual byte[] GetKeyAlgorithmParameters()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetKeyAlgorithmParameters() ?? throw new CryptographicException(Locale.GetText("Parameters not part of the certificate"));
	}

	/// <summary>Returns the key algorithm parameters for the X.509v3 certificate.</summary>
	/// <returns>The key algorithm parameters for the X.509 certificate as a hexadecimal string.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual string GetKeyAlgorithmParametersString()
	{
		return X509Helper.ToHexString(GetKeyAlgorithmParameters());
	}

	/// <summary>Returns the name of the principal to which the certificate was issued.</summary>
	/// <returns>The name of the principal to which the certificate was issued.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	[Obsolete("Use the Subject property.")]
	public virtual string GetName()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetSubjectName(legacyV1Mode: true);
	}

	/// <summary>Returns the public key for the X.509v3 certificate.</summary>
	/// <returns>The public key for the X.509 certificate as an array of bytes.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual byte[] GetPublicKey()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetPublicKey();
	}

	/// <summary>Returns the public key for the X.509v3 certificate.</summary>
	/// <returns>The public key for the X.509 certificate as a hexadecimal string.</returns>
	public virtual string GetPublicKeyString()
	{
		return X509Helper.ToHexString(GetPublicKey());
	}

	/// <summary>Returns the raw data for the entire X.509v3 certificate.</summary>
	/// <returns>A byte array containing the X.509 certificate data.</returns>
	public virtual byte[] GetRawCertData()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetRawCertData();
	}

	/// <summary>Returns the raw data for the entire X.509v3 certificate.</summary>
	/// <returns>The X.509 certificate data as a hexadecimal string.</returns>
	public virtual string GetRawCertDataString()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return X509Helper.ToHexString(impl.GetRawCertData());
	}

	/// <summary>Returns the serial number of the X.509v3 certificate.</summary>
	/// <returns>The serial number of the X.509 certificate as an array of bytes.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual byte[] GetSerialNumber()
	{
		X509Helper.ThrowIfContextInvalid(impl);
		return impl.GetSerialNumber();
	}

	/// <summary>Returns the serial number of the X.509v3 certificate.</summary>
	/// <returns>The serial number of the X.509 certificate as a hexadecimal string.</returns>
	public virtual string GetSerialNumberString()
	{
		byte[] serialNumber = GetSerialNumber();
		Array.Reverse(serialNumber);
		return X509Helper.ToHexString(serialNumber);
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</summary>
	/// <returns>A string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	public override string ToString()
	{
		return base.ToString();
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object, with extra information, if specified.</summary>
	/// <returns>A string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="fVerbose">true to produce the verbose form of the string representation; otherwise, false. </param>
	public virtual string ToString(bool fVerbose)
	{
		if (!fVerbose || !X509Helper.IsValid(impl))
		{
			return base.ToString();
		}
		return impl.ToString(full: true);
	}

	/// <summary>Converts the specified date and time to a string.</summary>
	/// <returns>A string representation of the value of the <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="date">The date and time to convert.</param>
	protected static string FormatDate(DateTime date)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class. </summary>
	public X509Certificate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public X509Certificate(byte[] rawData, string password)
	{
		Import(rawData, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	[MonoTODO("SecureString support is incomplete")]
	public X509Certificate(byte[] rawData, SecureString password)
	{
		Import(rawData, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public X509Certificate(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	[MonoTODO("SecureString support is incomplete")]
	public X509Certificate(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using the name of a PKCS7 signed file. </summary>
	/// <param name="fileName">The name of a PKCS7 signed file.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public X509Certificate(string fileName)
	{
		Import(fileName, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using the name of a PKCS7 signed file and a password to access the certificate.</summary>
	/// <param name="fileName">The name of a PKCS7 signed file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public X509Certificate(string fileName, string password)
	{
		Import(fileName, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a certificate file name and a password.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	[MonoTODO("SecureString support is incomplete")]
	public X509Certificate(string fileName, SecureString password)
	{
		Import(fileName, password, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using the name of a PKCS7 signed file, a password to access the certificate, and a key storage flag. </summary>
	/// <param name="fileName">The name of a PKCS7 signed file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public X509Certificate(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(fileName, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a certificate file name, a password, and a key storage flag. </summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	[MonoTODO("SecureString support is incomplete")]
	public X509Certificate(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(fileName, password, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object and a <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that describes serialization information.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that describes how serialization should be performed.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate(SerializationInfo info, StreamingContext context)
	{
		byte[] rawData = (byte[])info.GetValue("RawData", typeof(byte[]));
		Import(rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Compares two <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> objects for equality.</summary>
	/// <returns>true if the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object is equal to the object specified by the <paramref name="other" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to compare to the current object. </param>
	[ComVisible(false)]
	public override bool Equals(object obj)
	{
		if (obj is X509Certificate other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <summary>Exports the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to a byte array in a format described by one of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values. </summary>
	/// <returns>An array of bytes that represents the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="contentType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values that describes how to format the output data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A value other than <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Cert" />, <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.SerializedCert" />, or <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12" /> was passed to the <paramref name="contentType" /> parameter.-or-The certificate could not be exported.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Open, Export" />
	/// </PermissionSet>
	[ComVisible(false)]
	[MonoTODO("X509ContentType.Pfx/Pkcs12 and SerializedCert are not supported")]
	public virtual byte[] Export(X509ContentType contentType)
	{
		return Export(contentType, (byte[])null);
	}

	/// <summary>Exports the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to a byte array in a format described by one of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values, and using the specified password.</summary>
	/// <returns>An array of bytes that represents the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="contentType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values that describes how to format the output data.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A value other than <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Cert" />, <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.SerializedCert" />, or <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12" /> was passed to the <paramref name="contentType" /> parameter.-or-The certificate could not be exported.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Open, Export" />
	/// </PermissionSet>
	[MonoTODO("X509ContentType.Pfx/Pkcs12 and SerializedCert are not supported")]
	[ComVisible(false)]
	public virtual byte[] Export(X509ContentType contentType, string password)
	{
		byte[] password2 = ((password == null) ? null : Encoding.UTF8.GetBytes(password));
		return Export(contentType, password2);
	}

	/// <summary>Exports the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to a byte array using the specified format and a password.</summary>
	/// <returns>A byte array that represents the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="contentType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values that describes how to format the output data.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A value other than <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Cert" />, <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.SerializedCert" />, or <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12" /> was passed to the <paramref name="contentType" /> parameter.-or-The certificate could not be exported.</exception>
	[MonoTODO("X509ContentType.Pfx/Pkcs12 and SerializedCert are not supported. SecureString support is incomplete.")]
	public virtual byte[] Export(X509ContentType contentType, SecureString password)
	{
		byte[] password2 = password?.GetBuffer();
		return Export(contentType, password2);
	}

	internal byte[] Export(X509ContentType contentType, byte[] password)
	{
		try
		{
			X509Helper.ThrowIfContextInvalid(impl);
			return impl.Export(contentType, password);
		}
		finally
		{
			if (password != null)
			{
				Array.Clear(password, 0, password.Length);
			}
		}
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with data from a byte array.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void Import(byte[] rawData)
	{
		Import(rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object using data from a byte array, a password, and flags for determining how the private key is imported.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[MonoTODO("missing KeyStorageFlags support")]
	[ComVisible(false)]
	public virtual void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		Reset();
		impl = X509Helper.Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object using data from a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	[MonoTODO("SecureString support is incomplete")]
	public virtual void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		Import(rawData, (string)null, keyStorageFlags);
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with information from a certificate file.</summary>
	/// <param name="fileName">The name of a certificate file represented as a string. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void Import(string fileName)
	{
		byte[] rawData = File.ReadAllBytes(fileName);
		Import(rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with information from a certificate file, a password, and a <see cref="T:System.Security.Cryptography.X509Certificates.X509KeyStorageFlags" /> value.</summary>
	/// <param name="fileName">The name of a certificate file represented as a string. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	[MonoTODO("missing KeyStorageFlags support")]
	public virtual void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		byte[] rawData = File.ReadAllBytes(fileName);
		Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with information from a certificate file, a password, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	[MonoTODO("SecureString support is incomplete, missing KeyStorageFlags support")]
	public virtual void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		byte[] rawData = File.ReadAllBytes(fileName);
		Import(rawData, (string)null, keyStorageFlags);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and is called back by the deserialization event when deserialization is complete.  </summary>
	/// <param name="sender">The source of the deserialization event.</param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
	}

	/// <summary>Gets serialization information with all the data needed to recreate an instance of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</summary>
	/// <param name="info">The object to populate with serialization information.</param>
	/// <param name="context">The destination context of the serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (!X509Helper.IsValid(impl))
		{
			throw new NullReferenceException();
		}
		info.AddValue("RawData", impl.GetRawCertData());
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Reset();
		}
	}

	/// <summary>Resets the state of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object.</summary>
	[ComVisible(false)]
	public virtual void Reset()
	{
		if (impl != null)
		{
			impl.Dispose();
			impl = null;
		}
		issuer_name = null;
		subject_name = null;
		hideDates = false;
	}
}
