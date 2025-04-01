using System.Runtime.InteropServices;
using System.Security.Util;
using System.Text;

namespace System;

/// <summary>Contains information used to uniquely identify a manifest-based application. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class ApplicationId
{
	private string m_name;

	private Version m_version;

	private string m_processorArchitecture;

	private string m_culture;

	internal byte[] m_publicKeyToken;

	/// <summary>Gets the public key token for the application.</summary>
	/// <returns>A byte array containing the public key token for the application.</returns>
	/// <filterpriority>2</filterpriority>
	public byte[] PublicKeyToken
	{
		get
		{
			byte[] array = new byte[m_publicKeyToken.Length];
			Array.Copy(m_publicKeyToken, 0, array, 0, m_publicKeyToken.Length);
			return array;
		}
	}

	/// <summary>Gets the name of the application.</summary>
	/// <returns>The name of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name => m_name;

	/// <summary>Gets the version of the application.</summary>
	/// <returns>A <see cref="T:System.Version" /> that specifies the version of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public Version Version => m_version;

	/// <summary>Gets the target processor architecture for the application.</summary>
	/// <returns>The processor architecture of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string ProcessorArchitecture => m_processorArchitecture;

	/// <summary>Gets a string representing the culture information for the application.</summary>
	/// <returns>The culture information for the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string Culture => m_culture;

	internal ApplicationId()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationId" /> class.</summary>
	/// <param name="publicKeyToken">The array of bytes representing the raw public key data. </param>
	/// <param name="name">The name of the application. </param>
	/// <param name="version">A <see cref="T:System.Version" /> object that specifies the version of the application. </param>
	/// <param name="processorArchitecture">The processor architecture of the application. </param>
	/// <param name="culture">The culture of the application. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name " />is null.-or-<paramref name="version " />is null.-or-<paramref name="publicKeyToken " />is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name " />is an empty string.</exception>
	public ApplicationId(byte[] publicKeyToken, string name, Version version, string processorArchitecture, string culture)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("ApplicationId cannot have an empty string for the name."));
		}
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		if (publicKeyToken == null)
		{
			throw new ArgumentNullException("publicKeyToken");
		}
		m_publicKeyToken = new byte[publicKeyToken.Length];
		Array.Copy(publicKeyToken, 0, m_publicKeyToken, 0, publicKeyToken.Length);
		m_name = name;
		m_version = version;
		m_processorArchitecture = processorArchitecture;
		m_culture = culture;
	}

	/// <summary>Creates and returns an identical copy of the current application identity.</summary>
	/// <returns>An <see cref="T:System.ApplicationId" /> object that represents an exact copy of the original.</returns>
	/// <filterpriority>2</filterpriority>
	public ApplicationId Copy()
	{
		return new ApplicationId(m_publicKeyToken, m_name, m_version, m_processorArchitecture, m_culture);
	}

	/// <summary>Creates and returns a string representation of the application identity.</summary>
	/// <returns>A string representation of the application identity.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(m_name);
		if (m_culture != null)
		{
			stringBuilder.Append(", culture=\"");
			stringBuilder.Append(m_culture);
			stringBuilder.Append("\"");
		}
		stringBuilder.Append(", version=\"");
		stringBuilder.Append(m_version.ToString());
		stringBuilder.Append("\"");
		if (m_publicKeyToken != null)
		{
			stringBuilder.Append(", publicKeyToken=\"");
			stringBuilder.Append(Hex.EncodeHexString(m_publicKeyToken));
			stringBuilder.Append("\"");
		}
		if (m_processorArchitecture != null)
		{
			stringBuilder.Append(", processorArchitecture =\"");
			stringBuilder.Append(m_processorArchitecture);
			stringBuilder.Append("\"");
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Determines whether the specified <see cref="T:System.ApplicationId" /> object is equivalent to the current <see cref="T:System.ApplicationId" />.</summary>
	/// <returns>true if the specified <see cref="T:System.ApplicationId" /> object is equivalent to the current <see cref="T:System.ApplicationId" />; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.ApplicationId" /> object to compare to the current <see cref="T:System.ApplicationId" />. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object o)
	{
		if (!(o is ApplicationId applicationId))
		{
			return false;
		}
		if (!object.Equals(m_name, applicationId.m_name) || !object.Equals(m_version, applicationId.m_version) || !object.Equals(m_processorArchitecture, applicationId.m_processorArchitecture) || !object.Equals(m_culture, applicationId.m_culture))
		{
			return false;
		}
		if (m_publicKeyToken.Length != applicationId.m_publicKeyToken.Length)
		{
			return false;
		}
		for (int i = 0; i < m_publicKeyToken.Length; i++)
		{
			if (m_publicKeyToken[i] != applicationId.m_publicKeyToken[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets the hash code for the current application identity.</summary>
	/// <returns>The hash code for the current application identity.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return m_name.GetHashCode() ^ m_version.GetHashCode();
	}
}
