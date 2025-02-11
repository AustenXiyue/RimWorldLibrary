using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Encapsulates the name of a key storage provider (KSP) for use with Cryptography Next Generation (CNG) objects.</summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CngProvider : IEquatable<CngProvider>
{
	private static volatile CngProvider s_msSmartCardKsp;

	private static volatile CngProvider s_msSoftwareKsp;

	private string m_provider;

	/// <summary>Gets the name of the key storage provider (KSP) that the current <see cref="T:System.Security.Cryptography.CngProvider" /> object specifies.</summary>
	/// <returns>The embedded KSP name.</returns>
	public string Provider => m_provider;

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngProvider" /> object that specifies the Microsoft Smart Card Key Storage Provider. </summary>
	/// <returns>An object that specifies the Microsoft Smart Card Key Storage Provider.</returns>
	public static CngProvider MicrosoftSmartCardKeyStorageProvider
	{
		get
		{
			if (s_msSmartCardKsp == null)
			{
				s_msSmartCardKsp = new CngProvider("Microsoft Smart Card Key Storage Provider");
			}
			return s_msSmartCardKsp;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.CngProvider" /> object that specifies the Microsoft Software Key Storage Provider.</summary>
	/// <returns>An object that specifies the Microsoft Software Key Storage Provider.</returns>
	public static CngProvider MicrosoftSoftwareKeyStorageProvider
	{
		get
		{
			if (s_msSoftwareKsp == null)
			{
				s_msSoftwareKsp = new CngProvider("Microsoft Software Key Storage Provider");
			}
			return s_msSoftwareKsp;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngProvider" /> class.</summary>
	/// <param name="provider">The name of the key storage provider (KSP) to initialize.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="provider" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="provider" /> parameter length is 0 (zero).</exception>
	public CngProvider(string provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (provider.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The provider name '{0}' is invalid.", provider), "provider");
		}
		m_provider = provider;
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngProvider" /> objects specify the same key storage provider (KSP).</summary>
	/// <returns>true if the two objects represent the same KSP; otherwise, false.</returns>
	/// <param name="left">An object that specifies a KSP.</param>
	/// <param name="right">A second object, to be compared to the object that is identified by the <paramref name="left" /> parameter.</param>
	public static bool operator ==(CngProvider left, CngProvider right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngProvider" /> objects do not represent the same key storage provider (KSP).</summary>
	/// <returns>true if the two objects do not represent the same KSP; otherwise, false.</returns>
	/// <param name="left">An object that specifies a KSP.</param>
	/// <param name="right">A second object, to be compared to the object that is identified by the <paramref name="left" /> parameter.</param>
	public static bool operator !=(CngProvider left, CngProvider right)
	{
		if ((object)left == null)
		{
			return (object)right != null;
		}
		return !left.Equals(right);
	}

	/// <summary>Compares the specified object to the current <see cref="T:System.Security.Cryptography.CngProvider" /> object.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Security.Cryptography.CngProvider" /> that specifies the same key storage provider(KSP) as the current object; otherwise, false.</returns>
	/// <param name="obj">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngProvider" /> object.</param>
	public override bool Equals(object obj)
	{
		return Equals(obj as CngProvider);
	}

	/// <summary>Compares the specified <see cref="T:System.Security.Cryptography.CngProvider" /> object to the current <see cref="T:System.Security.Cryptography.CngProvider" /> object.</summary>
	/// <returns>true if the <paramref name="other" /> parameter specifies the same key storage provider (KSP) as the current object; otherwise, false.</returns>
	/// <param name="other">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngProvider" /> object.</param>
	public bool Equals(CngProvider other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return m_provider.Equals(other.Provider);
	}

	/// <summary>Generates a hash value for the name of the key storage provider (KSP) that is embedded in the current <see cref="T:System.Security.Cryptography.CngProvider" /> object.</summary>
	/// <returns>The hash value of the embedded KSP name.</returns>
	public override int GetHashCode()
	{
		return m_provider.GetHashCode();
	}

	/// <summary>Gets the name of the key storage provider (KSP) that the current <see cref="T:System.Security.Cryptography.CngProvider" /> object specifies.</summary>
	/// <returns>The embedded KSP name.</returns>
	public override string ToString()
	{
		return m_provider.ToString();
	}
}
