using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.Security.Claims;

/// <summary>An <see cref="T:System.Security.Principal.IPrincipal" /> implementation that supports multiple claims-based identities.</summary>
[Serializable]
[ComVisible(true)]
public class ClaimsPrincipal : IPrincipal
{
	private enum SerializationMask
	{
		None,
		HasIdentities,
		UserData
	}

	[NonSerialized]
	private byte[] m_userSerializationData;

	[NonSerialized]
	private const string PreFix = "System.Security.ClaimsPrincipal.";

	[NonSerialized]
	private const string IdentitiesKey = "System.Security.ClaimsPrincipal.Identities";

	[NonSerialized]
	private const string VersionKey = "System.Security.ClaimsPrincipal.Version";

	[OptionalField(VersionAdded = 2)]
	private string m_version = "1.0";

	[OptionalField(VersionAdded = 2)]
	private string m_serializedClaimsIdentities;

	[NonSerialized]
	private List<ClaimsIdentity> m_identities = new List<ClaimsIdentity>();

	[NonSerialized]
	private static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> s_identitySelector = SelectPrimaryIdentity;

	[NonSerialized]
	private static Func<ClaimsPrincipal> s_principalSelector = ClaimsPrincipalSelector;

	/// <summary>Gets and sets the delegate used to select the claims identity returned by the <see cref="P:System.Security.Claims.ClaimsPrincipal.Identity" /> property.</summary>
	/// <returns>The delegate. The default is null.</returns>
	public static Func<IEnumerable<ClaimsIdentity>, ClaimsIdentity> PrimaryIdentitySelector
	{
		get
		{
			return s_identitySelector;
		}
		[SecurityCritical]
		set
		{
			s_identitySelector = value;
		}
	}

	/// <summary>Gets and sets the delegate used to select the claims principal returned by the <see cref="P:System.Security.Claims.ClaimsPrincipal.Current" /> property.</summary>
	/// <returns>The delegate. The default is null.</returns>
	public static Func<ClaimsPrincipal> ClaimsPrincipalSelector
	{
		get
		{
			return s_principalSelector;
		}
		[SecurityCritical]
		set
		{
			s_principalSelector = value;
		}
	}

	protected virtual byte[] CustomSerializationData => m_userSerializationData;

	/// <summary>Gets a collection that contains all of the claims from all of the claims identities associated with this claims principal.</summary>
	/// <returns>The claims associated with this principal.</returns>
	public virtual IEnumerable<Claim> Claims
	{
		get
		{
			foreach (ClaimsIdentity identity in Identities)
			{
				foreach (Claim claim in identity.Claims)
				{
					yield return claim;
				}
			}
		}
	}

	/// <summary>Gets the current claims principal.</summary>
	/// <returns>The current claims principal.</returns>
	public static ClaimsPrincipal Current
	{
		get
		{
			if (s_principalSelector != null)
			{
				return s_principalSelector();
			}
			return SelectClaimsPrincipal();
		}
	}

	/// <summary>Gets a collection that contains all of the claims identities associated with this claims principal.</summary>
	/// <returns>The collection of claims identities.</returns>
	public virtual IEnumerable<ClaimsIdentity> Identities => m_identities.AsReadOnly();

	/// <summary>Gets the primary claims identity associated with this claims principal.</summary>
	/// <returns>The primary claims identity associated with this claims principal.</returns>
	public virtual IIdentity Identity
	{
		get
		{
			if (s_identitySelector != null)
			{
				return s_identitySelector(m_identities);
			}
			return SelectPrimaryIdentity(m_identities);
		}
	}

	private static ClaimsIdentity SelectPrimaryIdentity(IEnumerable<ClaimsIdentity> identities)
	{
		if (identities == null)
		{
			throw new ArgumentNullException("identities");
		}
		ClaimsIdentity claimsIdentity = null;
		foreach (ClaimsIdentity identity in identities)
		{
			if (identity is WindowsIdentity)
			{
				claimsIdentity = identity;
				break;
			}
			if (claimsIdentity == null)
			{
				claimsIdentity = identity;
			}
		}
		return claimsIdentity;
	}

	private static ClaimsPrincipal SelectClaimsPrincipal()
	{
		if (Thread.CurrentPrincipal is ClaimsPrincipal result)
		{
			return result;
		}
		return new ClaimsPrincipal(Thread.CurrentPrincipal);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsPrincipal" /> class.</summary>
	public ClaimsPrincipal()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsPrincipal" /> class using the specified claims identities.</summary>
	/// <param name="identities">The identities from which to initialize the new claims principal.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identities" /> is null.</exception>
	public ClaimsPrincipal(IEnumerable<ClaimsIdentity> identities)
	{
		if (identities == null)
		{
			throw new ArgumentNullException("identities");
		}
		m_identities.AddRange(identities);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsPrincipal" /> class from the specified identity.</summary>
	/// <param name="identity">The identity from which to initialize the new claims principal.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identity" /> is null.</exception>
	public ClaimsPrincipal(IIdentity identity)
	{
		if (identity == null)
		{
			throw new ArgumentNullException("identity");
		}
		if (identity is ClaimsIdentity item)
		{
			m_identities.Add(item);
		}
		else
		{
			m_identities.Add(new ClaimsIdentity(identity));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsPrincipal" /> class from the specified principal.</summary>
	/// <param name="principal">The principal from which to initialize the new claims principal.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="principal" /> is null.</exception>
	public ClaimsPrincipal(IPrincipal principal)
	{
		if (principal == null)
		{
			throw new ArgumentNullException("principal");
		}
		if (!(principal is ClaimsPrincipal claimsPrincipal))
		{
			m_identities.Add(new ClaimsIdentity(principal.Identity));
		}
		else if (claimsPrincipal.Identities != null)
		{
			m_identities.AddRange(claimsPrincipal.Identities);
		}
	}

	public ClaimsPrincipal(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Initialize(reader);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsPrincipal" /> class from a serialized stream created by using <see cref="T:System.Runtime.Serialization.ISerializable" />.</summary>
	/// <param name="info">The serialized data.</param>
	/// <param name="context">The context for serialization.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	protected ClaimsPrincipal(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Deserialize(info, context);
	}

	public virtual ClaimsPrincipal Clone()
	{
		return new ClaimsPrincipal(this);
	}

	protected virtual ClaimsIdentity CreateClaimsIdentity(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return new ClaimsIdentity(reader);
	}

	[SecurityCritical]
	[OnSerializing]
	private void OnSerializingMethod(StreamingContext context)
	{
		if (!(this is ISerializable))
		{
			m_serializedClaimsIdentities = SerializeIdentities();
		}
	}

	[OnDeserialized]
	[SecurityCritical]
	private void OnDeserializedMethod(StreamingContext context)
	{
		if (!(this is ISerializable))
		{
			DeserializeIdentities(m_serializedClaimsIdentities);
			m_serializedClaimsIdentities = null;
		}
	}

	/// <summary>Populates the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with data needed to serialize the current <see cref="T:System.Security.Claims.ClaimsPrincipal" /> object.</summary>
	/// <param name="info">The object to populate with data.</param>
	/// <param name="context">The destination for this serialization. Can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("System.Security.ClaimsPrincipal.Identities", SerializeIdentities());
		info.AddValue("System.Security.ClaimsPrincipal.Version", m_version);
	}

	[SecurityCritical]
	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	private void Deserialize(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Name;
			if (!(name == "System.Security.ClaimsPrincipal.Identities"))
			{
				if (name == "System.Security.ClaimsPrincipal.Version")
				{
					m_version = info.GetString("System.Security.ClaimsPrincipal.Version");
				}
			}
			else
			{
				DeserializeIdentities(info.GetString("System.Security.ClaimsPrincipal.Identities"));
			}
		}
	}

	[SecurityCritical]
	private void DeserializeIdentities(string identities)
	{
		m_identities = new List<ClaimsIdentity>();
		if (string.IsNullOrEmpty(identities))
		{
			return;
		}
		List<string> list = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(identities));
		list = (List<string>)binaryFormatter.Deserialize(serializationStream, null, fCheck: false);
		for (int i = 0; i < list.Count; i += 2)
		{
			ClaimsIdentity claimsIdentity = null;
			using (MemoryStream serializationStream2 = new MemoryStream(Convert.FromBase64String(list[i + 1])))
			{
				claimsIdentity = (ClaimsIdentity)binaryFormatter.Deserialize(serializationStream2, null, fCheck: false);
			}
			if (!string.IsNullOrEmpty(list[i]))
			{
				if (!long.TryParse(list[i], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
				{
					throw new SerializationException(Environment.GetResourceString("Invalid BinaryFormatter stream."));
				}
				claimsIdentity = new WindowsIdentity(claimsIdentity, new IntPtr(result));
			}
			m_identities.Add(claimsIdentity);
		}
	}

	[SecurityCritical]
	private string SerializeIdentities()
	{
		List<string> list = new List<string>();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		foreach (ClaimsIdentity identity in m_identities)
		{
			if (identity.GetType() == typeof(WindowsIdentity))
			{
				WindowsIdentity windowsIdentity = identity as WindowsIdentity;
				list.Add(windowsIdentity.GetTokenInternal().ToInt64().ToString(NumberFormatInfo.InvariantInfo));
				using MemoryStream memoryStream = new MemoryStream();
				binaryFormatter.Serialize(memoryStream, windowsIdentity.CloneAsBase(), null, fCheck: false);
				list.Add(Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length));
			}
			else
			{
				using MemoryStream memoryStream2 = new MemoryStream();
				list.Add("");
				binaryFormatter.Serialize(memoryStream2, identity, null, fCheck: false);
				list.Add(Convert.ToBase64String(memoryStream2.GetBuffer(), 0, (int)memoryStream2.Length));
			}
		}
		using MemoryStream memoryStream3 = new MemoryStream();
		binaryFormatter.Serialize(memoryStream3, list, null, fCheck: false);
		return Convert.ToBase64String(memoryStream3.GetBuffer(), 0, (int)memoryStream3.Length);
	}

	/// <summary>Adds the specified claims identity to this claims principal.</summary>
	/// <param name="identity">The claims identity to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identity" /> is null.</exception>
	[SecurityCritical]
	public virtual void AddIdentity(ClaimsIdentity identity)
	{
		if (identity == null)
		{
			throw new ArgumentNullException("identity");
		}
		m_identities.Add(identity);
	}

	/// <summary>Adds the specified claims identities to this claims principal.</summary>
	/// <param name="identities">The claims identities to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identities" /> is null.</exception>
	[SecurityCritical]
	public virtual void AddIdentities(IEnumerable<ClaimsIdentity> identities)
	{
		if (identities == null)
		{
			throw new ArgumentNullException("identities");
		}
		m_identities.AddRange(identities);
	}

	/// <summary>Retrieves all of the claims that are matched by the specified predicate.</summary>
	/// <returns>The matching claims.</returns>
	/// <param name="match">The function that performs the matching logic.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public virtual IEnumerable<Claim> FindAll(Predicate<Claim> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		List<Claim> list = new List<Claim>();
		foreach (ClaimsIdentity identity in Identities)
		{
			if (identity == null)
			{
				continue;
			}
			foreach (Claim item in identity.FindAll(match))
			{
				list.Add(item);
			}
		}
		return list.AsReadOnly();
	}

	/// <summary>Retrieves all or the claims that have the specified claim type.</summary>
	/// <returns>The matching claims.</returns>
	/// <param name="type">The claim type against which to match claims.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public virtual IEnumerable<Claim> FindAll(string type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		List<Claim> list = new List<Claim>();
		foreach (ClaimsIdentity identity in Identities)
		{
			if (identity == null)
			{
				continue;
			}
			foreach (Claim item in identity.FindAll(type))
			{
				list.Add(item);
			}
		}
		return list.AsReadOnly();
	}

	/// <summary>Retrieves the first claim that is matched by the specified predicate.</summary>
	/// <returns>The first matching claim or null if no match is found.</returns>
	/// <param name="match">The function that performs the matching logic.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public virtual Claim FindFirst(Predicate<Claim> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		Claim claim = null;
		foreach (ClaimsIdentity identity in Identities)
		{
			if (identity != null)
			{
				claim = identity.FindFirst(match);
				if (claim != null)
				{
					return claim;
				}
			}
		}
		return claim;
	}

	/// <summary>Retrieves the first claim with the specified claim type.</summary>
	/// <returns>The first matching claim or null if no match is found.</returns>
	/// <param name="type">The claim type to match.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public virtual Claim FindFirst(string type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		Claim claim = null;
		for (int i = 0; i < m_identities.Count; i++)
		{
			if (m_identities[i] != null)
			{
				claim = m_identities[i].FindFirst(type);
				if (claim != null)
				{
					return claim;
				}
			}
		}
		return claim;
	}

	/// <summary>Determines whether any of the claims identities associated with this claims principal contains a claim that is matched by the specified predicate.</summary>
	/// <returns>true if a matching claim exists; otherwise, false.</returns>
	/// <param name="match">The function that performs the matching logic.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="match" /> is null.</exception>
	public virtual bool HasClaim(Predicate<Claim> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		for (int i = 0; i < m_identities.Count; i++)
		{
			if (m_identities[i] != null && m_identities[i].HasClaim(match))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether any of the claims identities associated with this claims principal contains a claim with the specified claim type and value.</summary>
	/// <returns>true if a matching claim exists; otherwise, false.</returns>
	/// <param name="type">The type of the claim to match.</param>
	/// <param name="value">The value of the claim to match.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.-or-<paramref name="value" /> is null.</exception>
	public virtual bool HasClaim(string type, string value)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		for (int i = 0; i < m_identities.Count; i++)
		{
			if (m_identities[i] != null && m_identities[i].HasClaim(type, value))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the entity (user) represented by this claims principal is in the specified role.</summary>
	/// <returns>true if claims principal is in the specified role; otherwise, false.</returns>
	/// <param name="role">The role for which to check.</param>
	public virtual bool IsInRole(string role)
	{
		for (int i = 0; i < m_identities.Count; i++)
		{
			if (m_identities[i] != null && m_identities[i].HasClaim(m_identities[i].RoleClaimType, role))
			{
				return true;
			}
		}
		return false;
	}

	private void Initialize(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		SerializationMask serializationMask = (SerializationMask)reader.ReadInt32();
		int num = reader.ReadInt32();
		int num2 = 0;
		if ((serializationMask & SerializationMask.HasIdentities) == SerializationMask.HasIdentities)
		{
			num2++;
			int num3 = reader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				m_identities.Add(CreateClaimsIdentity(reader));
			}
		}
		if ((serializationMask & SerializationMask.UserData) == SerializationMask.UserData)
		{
			int count = reader.ReadInt32();
			m_userSerializationData = reader.ReadBytes(count);
			num2++;
		}
		for (int j = num2; j < num; j++)
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
		int num = 0;
		SerializationMask serializationMask = SerializationMask.None;
		if (m_identities.Count > 0)
		{
			serializationMask |= SerializationMask.HasIdentities;
			num++;
		}
		if (userData != null && userData.Length != 0)
		{
			num++;
			serializationMask |= SerializationMask.UserData;
		}
		writer.Write((int)serializationMask);
		writer.Write(num);
		if ((serializationMask & SerializationMask.HasIdentities) == SerializationMask.HasIdentities)
		{
			writer.Write(m_identities.Count);
			foreach (ClaimsIdentity identity in m_identities)
			{
				identity.WriteTo(writer);
			}
		}
		if ((serializationMask & SerializationMask.UserData) == SerializationMask.UserData)
		{
			writer.Write(userData.Length);
			writer.Write(userData);
		}
		writer.Flush();
	}
}
