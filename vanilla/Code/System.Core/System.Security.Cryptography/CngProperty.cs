using System.Security.Permissions;

namespace System.Security.Cryptography;

/// <summary>Encapsulates a property of a Cryptography Next Generation (CNG) key or provider.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public struct CngProperty : IEquatable<CngProperty>
{
	private string m_name;

	private CngPropertyOptions m_propertyOptions;

	private byte[] m_value;

	private int? m_hashCode;

	/// <summary>Gets the property name that the current <see cref="T:System.Security.Cryptography.CngProperty" /> object specifies.</summary>
	/// <returns>The property name that is set in the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</returns>
	public string Name => m_name;

	/// <summary>Gets the property options that the current <see cref="T:System.Security.Cryptography.CngProperty" /> object specifies.</summary>
	/// <returns>An object that specifies the options that are set in the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</returns>
	public CngPropertyOptions Options => m_propertyOptions;

	internal byte[] Value => m_value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CngProperty" /> class.</summary>
	/// <param name="name">The property name to initialize.</param>
	/// <param name="value">The property value to initialize.</param>
	/// <param name="options">A bitwise combination of the enumeration values that specify how the property is stored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	public CngProperty(string name, byte[] value, CngPropertyOptions options)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_name = name;
		m_propertyOptions = options;
		m_hashCode = null;
		if (value != null)
		{
			m_value = value.Clone() as byte[];
		}
		else
		{
			m_value = null;
		}
	}

	/// <summary>Gets the property value that the current <see cref="T:System.Security.Cryptography.CngProperty" /> object specifies.</summary>
	/// <returns>An array that represents the value stored in the property.</returns>
	public byte[] GetValue()
	{
		byte[] result = null;
		if (m_value != null)
		{
			result = m_value.Clone() as byte[];
		}
		return result;
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngProperty" /> objects specify the same property name, value, and options.</summary>
	/// <returns>true if the two objects specify the same property; otherwise, false.</returns>
	/// <param name="left">An object that specifies a property of a Cryptography Next Generation (CNG) key or provider.</param>
	/// <param name="right">A second object, to be compared to the object that is identified by the <paramref name="left" /> parameter.</param>
	public static bool operator ==(CngProperty left, CngProperty right)
	{
		return left.Equals(right);
	}

	/// <summary>Determines whether two <see cref="T:System.Security.Cryptography.CngProperty" /> objects do not specify the same property name, value, and options.</summary>
	/// <returns>true if the two objects do not specify the same property; otherwise, false.</returns>
	/// <param name="left">An object that specifies a property of a Cryptography Next Generation (CNG) key or provider.</param>
	/// <param name="right">A second object, to be compared to the object that is identified by the <paramref name="left" /> parameter.</param>
	public static bool operator !=(CngProperty left, CngProperty right)
	{
		return !left.Equals(right);
	}

	/// <summary>Compares the specified object to the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Security.Cryptography.CngProperty" /> object that specifies the same property as the current object; otherwise, false.</returns>
	/// <param name="obj">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</param>
	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is CngProperty))
		{
			return false;
		}
		return Equals((CngProperty)obj);
	}

	/// <summary>Compares the specified <see cref="T:System.Security.Cryptography.CngProperty" /> object to the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</summary>
	/// <returns>true if the <paramref name="other" /> parameter represents the same property as the current object; otherwise, false.</returns>
	/// <param name="other">An object to be compared to the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</param>
	public bool Equals(CngProperty other)
	{
		if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
		{
			return false;
		}
		if (Options != other.Options)
		{
			return false;
		}
		if (m_value == null)
		{
			return other.m_value == null;
		}
		if (other.m_value == null)
		{
			return false;
		}
		if (m_value.Length != other.m_value.Length)
		{
			return false;
		}
		for (int i = 0; i < m_value.Length; i++)
		{
			if (m_value[i] != other.m_value[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Generates a hash value for the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</summary>
	/// <returns>The hash value of the current <see cref="T:System.Security.Cryptography.CngProperty" /> object.</returns>
	public override int GetHashCode()
	{
		if (!m_hashCode.HasValue)
		{
			int num = Name.GetHashCode() ^ Options.GetHashCode();
			if (m_value != null)
			{
				for (int i = 0; i < m_value.Length; i++)
				{
					int num2 = m_value[i] << i % 4 * 8;
					num ^= num2;
				}
			}
			m_hashCode = num;
		}
		return m_hashCode.Value;
	}
}
