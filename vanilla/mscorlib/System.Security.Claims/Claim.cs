using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace System.Security.Claims;

/// <summary>Represents a claim.</summary>
[Serializable]
public class Claim
{
	private enum SerializationMask
	{
		None = 0,
		NameClaimType = 1,
		RoleClaimType = 2,
		StringType = 4,
		Issuer = 8,
		OriginalIssuerEqualsIssuer = 0x10,
		OriginalIssuer = 0x20,
		HasProperties = 0x40,
		UserData = 0x80
	}

	private string m_issuer;

	private string m_originalIssuer;

	private string m_type;

	private string m_value;

	private string m_valueType;

	[NonSerialized]
	private byte[] m_userSerializationData;

	private Dictionary<string, string> m_properties;

	[NonSerialized]
	private object m_propertyLock = new object();

	[NonSerialized]
	private ClaimsIdentity m_subject;

	protected virtual byte[] CustomSerializationData => m_userSerializationData;

	/// <summary>Gets the issuer of the claim.</summary>
	/// <returns>A name that refers to the issuer of the claim.</returns>
	public string Issuer => m_issuer;

	/// <summary>Gets the original issuer of the claim. </summary>
	/// <returns>A name that refers to the original issuer of the claim.</returns>
	public string OriginalIssuer => m_originalIssuer;

	/// <summary>Gets a dictionary that contains additional properties associated with this claim.</summary>
	/// <returns>A dictionary that contains additional properties associated with the claim. The properties are represented as name-value pairs.</returns>
	public IDictionary<string, string> Properties
	{
		get
		{
			if (m_properties == null)
			{
				lock (m_propertyLock)
				{
					if (m_properties == null)
					{
						m_properties = new Dictionary<string, string>();
					}
				}
			}
			return m_properties;
		}
	}

	/// <summary>Gets the subject of the claim.</summary>
	/// <returns>The subject of the claim.</returns>
	public ClaimsIdentity Subject
	{
		get
		{
			return m_subject;
		}
		internal set
		{
			m_subject = value;
		}
	}

	/// <summary>Gets the claim type of the claim.</summary>
	/// <returns>The claim type.</returns>
	public string Type => m_type;

	/// <summary>Gets the value of the claim.</summary>
	/// <returns>The claim value.</returns>
	public string Value => m_value;

	/// <summary>Gets the value type of the claim.</summary>
	/// <returns>The claim value type.</returns>
	public string ValueType => m_valueType;

	public Claim(BinaryReader reader)
		: this(reader, null)
	{
	}

	public Claim(BinaryReader reader, ClaimsIdentity subject)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Initialize(reader, subject);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.Claim" /> class with the specified claim type, and value.</summary>
	/// <param name="type">The claim type.</param>
	/// <param name="value">The claim value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="value" /> is null.</exception>
	public Claim(string type, string value)
		: this(type, value, "http://www.w3.org/2001/XMLSchema#string", "LOCAL AUTHORITY", "LOCAL AUTHORITY", null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.Claim" /> class with the specified claim type, value, and value type.</summary>
	/// <param name="type">The claim type.</param>
	/// <param name="value">The claim value.</param>
	/// <param name="valueType">The claim value type. If this parameter is null, then <see cref="F:System.Security.Claims.ClaimValueTypes.String" /> is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="value" /> is null.</exception>
	public Claim(string type, string value, string valueType)
		: this(type, value, valueType, "LOCAL AUTHORITY", "LOCAL AUTHORITY", null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.Claim" /> class with the specified claim type, value, value type, and issuer.</summary>
	/// <param name="type">The claim type.</param>
	/// <param name="value">The claim value.</param>
	/// <param name="valueType">The claim value type. If this parameter is null, then <see cref="F:System.Security.Claims.ClaimValueTypes.String" /> is used.</param>
	/// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="F:System.Security.Claims.ClaimsIdentity.DefaultIssuer" /> is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="value" /> is null.</exception>
	public Claim(string type, string value, string valueType, string issuer)
		: this(type, value, valueType, issuer, issuer, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.Claim" /> class with the specified claim type, value, value type, issuer,  and original issuer.</summary>
	/// <param name="type">The claim type.</param>
	/// <param name="value">The claim value.</param>
	/// <param name="valueType">The claim value type. If this parameter is null, then <see cref="F:System.Security.Claims.ClaimValueTypes.String" /> is used.</param>
	/// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="F:System.Security.Claims.ClaimsIdentity.DefaultIssuer" /> is used.</param>
	/// <param name="originalIssuer">The original issuer of the claim. If this parameter is empty or null, then the <see cref="P:System.Security.Claims.Claim.OriginalIssuer" /> property is set to the value of the <see cref="P:System.Security.Claims.Claim.Issuer" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="value" /> is null.</exception>
	public Claim(string type, string value, string valueType, string issuer, string originalIssuer)
		: this(type, value, valueType, issuer, originalIssuer, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.Claim" /> class with the specified claim type, value, value type, issuer, original issuer and subject.</summary>
	/// <param name="type">The claim type.</param>
	/// <param name="value">The claim value.</param>
	/// <param name="valueType">The claim value type. If this parameter is null, then <see cref="F:System.Security.Claims.ClaimValueTypes.String" /> is used.</param>
	/// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="F:System.Security.Claims.ClaimsIdentity.DefaultIssuer" /> is used.</param>
	/// <param name="originalIssuer">The original issuer of the claim. If this parameter is empty or null, then the <see cref="P:System.Security.Claims.Claim.OriginalIssuer" /> property is set to the value of the <see cref="P:System.Security.Claims.Claim.Issuer" /> property.</param>
	/// <param name="subject">The subject that this claim describes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="value" /> is null.</exception>
	public Claim(string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject)
		: this(type, value, valueType, issuer, originalIssuer, subject, null, null)
	{
	}

	internal Claim(string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject, string propertyKey, string propertyValue)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_type = type;
		m_value = value;
		if (string.IsNullOrEmpty(valueType))
		{
			m_valueType = "http://www.w3.org/2001/XMLSchema#string";
		}
		else
		{
			m_valueType = valueType;
		}
		if (string.IsNullOrEmpty(issuer))
		{
			m_issuer = "LOCAL AUTHORITY";
		}
		else
		{
			m_issuer = issuer;
		}
		if (string.IsNullOrEmpty(originalIssuer))
		{
			m_originalIssuer = m_issuer;
		}
		else
		{
			m_originalIssuer = originalIssuer;
		}
		m_subject = subject;
		if (propertyKey != null)
		{
			Properties.Add(propertyKey, propertyValue);
		}
	}

	protected Claim(Claim other)
		: this(other, other?.m_subject)
	{
	}

	protected Claim(Claim other, ClaimsIdentity subject)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		m_issuer = other.m_issuer;
		m_originalIssuer = other.m_originalIssuer;
		m_subject = subject;
		m_type = other.m_type;
		m_value = other.m_value;
		m_valueType = other.m_valueType;
		if (other.m_properties != null)
		{
			m_properties = new Dictionary<string, string>();
			foreach (string key in other.m_properties.Keys)
			{
				m_properties.Add(key, other.m_properties[key]);
			}
		}
		if (other.m_userSerializationData != null)
		{
			m_userSerializationData = other.m_userSerializationData.Clone() as byte[];
		}
	}

	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext context)
	{
		m_propertyLock = new object();
	}

	/// <summary>Returns a new <see cref="T:System.Security.Claims.Claim" /> object copied from this object. The new claim does not have a subject.</summary>
	/// <returns>The new claim object.</returns>
	public virtual Claim Clone()
	{
		return Clone(null);
	}

	/// <summary>Returns a new <see cref="T:System.Security.Claims.Claim" /> object copied from this object. The subject of the new claim is set to the specified ClaimsIdentity.</summary>
	/// <returns>The new claim object.</returns>
	/// <param name="identity">The intended subject of the new claim.</param>
	public virtual Claim Clone(ClaimsIdentity identity)
	{
		return new Claim(this, identity);
	}

	private void Initialize(BinaryReader reader, ClaimsIdentity subject)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		m_subject = subject;
		SerializationMask serializationMask = (SerializationMask)reader.ReadInt32();
		int num = 1;
		int num2 = reader.ReadInt32();
		m_value = reader.ReadString();
		if ((serializationMask & SerializationMask.NameClaimType) == SerializationMask.NameClaimType)
		{
			m_type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
		}
		else if ((serializationMask & SerializationMask.RoleClaimType) == SerializationMask.RoleClaimType)
		{
			m_type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
		}
		else
		{
			m_type = reader.ReadString();
			num++;
		}
		if ((serializationMask & SerializationMask.StringType) == SerializationMask.StringType)
		{
			m_valueType = reader.ReadString();
			num++;
		}
		else
		{
			m_valueType = "http://www.w3.org/2001/XMLSchema#string";
		}
		if ((serializationMask & SerializationMask.Issuer) == SerializationMask.Issuer)
		{
			m_issuer = reader.ReadString();
			num++;
		}
		else
		{
			m_issuer = "LOCAL AUTHORITY";
		}
		if ((serializationMask & SerializationMask.OriginalIssuerEqualsIssuer) == SerializationMask.OriginalIssuerEqualsIssuer)
		{
			m_originalIssuer = m_issuer;
		}
		else if ((serializationMask & SerializationMask.OriginalIssuer) == SerializationMask.OriginalIssuer)
		{
			m_originalIssuer = reader.ReadString();
			num++;
		}
		else
		{
			m_originalIssuer = "LOCAL AUTHORITY";
		}
		if ((serializationMask & SerializationMask.HasProperties) == SerializationMask.HasProperties)
		{
			int num3 = reader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				Properties.Add(reader.ReadString(), reader.ReadString());
			}
		}
		if ((serializationMask & SerializationMask.UserData) == SerializationMask.UserData)
		{
			int count = reader.ReadInt32();
			m_userSerializationData = reader.ReadBytes(count);
			num++;
		}
		for (int j = num; j < num2; j++)
		{
			reader.ReadString();
		}
	}

	public virtual void WriteTo(BinaryWriter writer)
	{
		WriteTo(writer, null);
	}

	protected virtual void WriteTo(BinaryWriter writer, byte[] userData)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int num = 1;
		SerializationMask serializationMask = SerializationMask.None;
		if (string.Equals(m_type, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"))
		{
			serializationMask |= SerializationMask.NameClaimType;
		}
		else if (string.Equals(m_type, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
		{
			serializationMask |= SerializationMask.RoleClaimType;
		}
		else
		{
			num++;
		}
		if (!string.Equals(m_valueType, "http://www.w3.org/2001/XMLSchema#string", StringComparison.Ordinal))
		{
			num++;
			serializationMask |= SerializationMask.StringType;
		}
		if (!string.Equals(m_issuer, "LOCAL AUTHORITY", StringComparison.Ordinal))
		{
			num++;
			serializationMask |= SerializationMask.Issuer;
		}
		if (string.Equals(m_originalIssuer, m_issuer, StringComparison.Ordinal))
		{
			serializationMask |= SerializationMask.OriginalIssuerEqualsIssuer;
		}
		else if (!string.Equals(m_originalIssuer, "LOCAL AUTHORITY", StringComparison.Ordinal))
		{
			num++;
			serializationMask |= SerializationMask.OriginalIssuer;
		}
		if (Properties.Count > 0)
		{
			num++;
			serializationMask |= SerializationMask.HasProperties;
		}
		if (userData != null && userData.Length != 0)
		{
			num++;
			serializationMask |= SerializationMask.UserData;
		}
		writer.Write((int)serializationMask);
		writer.Write(num);
		writer.Write(m_value);
		if ((serializationMask & SerializationMask.NameClaimType) != SerializationMask.NameClaimType && (serializationMask & SerializationMask.RoleClaimType) != SerializationMask.RoleClaimType)
		{
			writer.Write(m_type);
		}
		if ((serializationMask & SerializationMask.StringType) == SerializationMask.StringType)
		{
			writer.Write(m_valueType);
		}
		if ((serializationMask & SerializationMask.Issuer) == SerializationMask.Issuer)
		{
			writer.Write(m_issuer);
		}
		if ((serializationMask & SerializationMask.OriginalIssuer) == SerializationMask.OriginalIssuer)
		{
			writer.Write(m_originalIssuer);
		}
		if ((serializationMask & SerializationMask.HasProperties) == SerializationMask.HasProperties)
		{
			writer.Write(Properties.Count);
			foreach (string key in Properties.Keys)
			{
				writer.Write(key);
				writer.Write(Properties[key]);
			}
		}
		if ((serializationMask & SerializationMask.UserData) == SerializationMask.UserData)
		{
			writer.Write(userData.Length);
			writer.Write(userData);
		}
		writer.Flush();
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Security.Claims.Claim" /> object.</summary>
	/// <returns>The string representation of this <see cref="T:System.Security.Claims.Claim" /> object.</returns>
	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", m_type, m_value);
	}
}
