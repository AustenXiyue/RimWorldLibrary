using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.Security.Claims;

/// <summary>Represents a claims-based identity.</summary>
[Serializable]
[ComVisible(true)]
public class ClaimsIdentity : IIdentity
{
	private enum SerializationMask
	{
		None = 0,
		AuthenticationType = 1,
		BootstrapConext = 2,
		NameClaimType = 4,
		RoleClaimType = 8,
		HasClaims = 0x10,
		HasLabel = 0x20,
		Actor = 0x40,
		UserData = 0x80
	}

	[NonSerialized]
	private byte[] m_userSerializationData;

	[NonSerialized]
	private const string PreFix = "System.Security.ClaimsIdentity.";

	[NonSerialized]
	private const string ActorKey = "System.Security.ClaimsIdentity.actor";

	[NonSerialized]
	private const string AuthenticationTypeKey = "System.Security.ClaimsIdentity.authenticationType";

	[NonSerialized]
	private const string BootstrapContextKey = "System.Security.ClaimsIdentity.bootstrapContext";

	[NonSerialized]
	private const string ClaimsKey = "System.Security.ClaimsIdentity.claims";

	[NonSerialized]
	private const string LabelKey = "System.Security.ClaimsIdentity.label";

	[NonSerialized]
	private const string NameClaimTypeKey = "System.Security.ClaimsIdentity.nameClaimType";

	[NonSerialized]
	private const string RoleClaimTypeKey = "System.Security.ClaimsIdentity.roleClaimType";

	[NonSerialized]
	private const string VersionKey = "System.Security.ClaimsIdentity.version";

	/// <summary>The default issuer; “LOCAL AUTHORITY”.</summary>
	[NonSerialized]
	public const string DefaultIssuer = "LOCAL AUTHORITY";

	/// <summary>The default name claim type; <see cref="F:System.Security.Claims.ClaimTypes.Name" />.</summary>
	[NonSerialized]
	public const string DefaultNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

	/// <summary>The default role claim type; <see cref="F:System.Security.Claims.ClaimTypes.Role" />.</summary>
	[NonSerialized]
	public const string DefaultRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

	[NonSerialized]
	private List<Claim> m_instanceClaims = new List<Claim>();

	[NonSerialized]
	private Collection<IEnumerable<Claim>> m_externalClaims = new Collection<IEnumerable<Claim>>();

	[NonSerialized]
	private string m_nameType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

	[NonSerialized]
	private string m_roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

	[OptionalField(VersionAdded = 2)]
	private string m_version = "1.0";

	[OptionalField(VersionAdded = 2)]
	private ClaimsIdentity m_actor;

	[OptionalField(VersionAdded = 2)]
	private string m_authenticationType;

	[OptionalField(VersionAdded = 2)]
	private object m_bootstrapContext;

	[OptionalField(VersionAdded = 2)]
	private string m_label;

	[OptionalField(VersionAdded = 2)]
	private string m_serializedNameType;

	[OptionalField(VersionAdded = 2)]
	private string m_serializedRoleType;

	[OptionalField(VersionAdded = 2)]
	private string m_serializedClaims;

	/// <summary>Gets the authentication type.</summary>
	/// <returns>The authentication type.</returns>
	public virtual string AuthenticationType => m_authenticationType;

	/// <summary>Gets a value that indicates whether the identity has been authenticated.</summary>
	/// <returns>true if the identity has been authenticated; otherwise, false.</returns>
	public virtual bool IsAuthenticated => !string.IsNullOrEmpty(m_authenticationType);

	/// <summary>Gets or sets the identity of the calling party that was granted delegation rights.</summary>
	/// <returns>The calling party that was granted delegation rights.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt to set the property to the current instance occurs.</exception>
	public ClaimsIdentity Actor
	{
		get
		{
			return m_actor;
		}
		set
		{
			if (value != null && IsCircular(value))
			{
				throw new InvalidOperationException(Environment.GetResourceString("Actor cannot be set so that circular directed graph will exist chaining the subjects together."));
			}
			m_actor = value;
		}
	}

	/// <summary>Gets or sets the token that was used to create this claims identity.</summary>
	/// <returns>The bootstrap context.</returns>
	public object BootstrapContext
	{
		get
		{
			return m_bootstrapContext;
		}
		[SecurityCritical]
		set
		{
			m_bootstrapContext = value;
		}
	}

	/// <summary>Gets the claims associated with this claims identity.</summary>
	/// <returns>The collection of claims associated with this claims identity.</returns>
	public virtual IEnumerable<Claim> Claims
	{
		get
		{
			for (int i = 0; i < m_instanceClaims.Count; i++)
			{
				yield return m_instanceClaims[i];
			}
			if (m_externalClaims == null)
			{
				yield break;
			}
			for (int j = 0; j < m_externalClaims.Count; j++)
			{
				if (m_externalClaims[j] == null)
				{
					continue;
				}
				foreach (Claim item in m_externalClaims[j])
				{
					yield return item;
				}
			}
		}
	}

	protected virtual byte[] CustomSerializationData => m_userSerializationData;

	internal Collection<IEnumerable<Claim>> ExternalClaims
	{
		[FriendAccessAllowed]
		get
		{
			return m_externalClaims;
		}
	}

	/// <summary>Gets or sets the label for this claims identity.</summary>
	/// <returns>The label.</returns>
	public string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			m_label = value;
		}
	}

	/// <summary>Gets the name of this claims identity.</summary>
	/// <returns>The name or null.</returns>
	public virtual string Name => FindFirst(m_nameType)?.Value;

	/// <summary>Gets the claim type that is used to determine which claims provide the value for the <see cref="P:System.Security.Claims.ClaimsIdentity.Name" /> property of this claims identity.</summary>
	/// <returns>The name claim type.</returns>
	public string NameClaimType => m_nameType;

	/// <summary>Gets the claim type that will be interpreted as a .NET Framework role among the claims in this claims identity.</summary>
	/// <returns>The role claim type.</returns>
	public string RoleClaimType => m_roleType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class with an empty claims collection.</summary>
	public ClaimsIdentity()
		: this((IEnumerable<Claim>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class using the name and authentication type from the specified <see cref="T:System.Security.Principal.IIdentity" />.</summary>
	/// <param name="identity">The identity from which to base the new claims identity.</param>
	public ClaimsIdentity(IIdentity identity)
		: this(identity, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class using an enumerated collection of <see cref="T:System.Security.Claims.Claim" /> objects.</summary>
	/// <param name="claims">The claims with which to populate the claims identity.</param>
	public ClaimsIdentity(IEnumerable<Claim> claims)
		: this(null, claims, null, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class with an empty claims collection and the specified authentication type.</summary>
	/// <param name="authenticationType">The type of authentication used.</param>
	public ClaimsIdentity(string authenticationType)
		: this(null, null, authenticationType, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class with the specified claims and authentication type.</summary>
	/// <param name="claims">The claims with which to populate the claims identity.</param>
	/// <param name="authenticationType">The type of authentication used.</param>
	public ClaimsIdentity(IEnumerable<Claim> claims, string authenticationType)
		: this(null, claims, authenticationType, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class using the specified claims and the specified <see cref="T:System.Security.Principal.IIdentity" />.</summary>
	/// <param name="identity">The identity from which to base the new claims identity.</param>
	/// <param name="claims">The claims with which to populate the claims identity.</param>
	public ClaimsIdentity(IIdentity identity, IEnumerable<Claim> claims)
		: this(identity, claims, null, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class with the specified authentication type, name claim type, and role claim type.</summary>
	/// <param name="authenticationType">The type of authentication used.</param>
	/// <param name="nameType">The claim type to use for name claims.</param>
	/// <param name="roleType">The claim type to use for role claims.</param>
	public ClaimsIdentity(string authenticationType, string nameType, string roleType)
		: this(null, null, authenticationType, nameType, roleType)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class with the specified, claims, authentication type, name claim type, and role claim type.</summary>
	/// <param name="claims">The claims with which to populate the claims identity.</param>
	/// <param name="authenticationType">The type of authentication used.</param>
	/// <param name="nameType">The claim type to use for name claims.</param>
	/// <param name="roleType">The claim type to use for role claims.</param>
	public ClaimsIdentity(IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType)
		: this(null, claims, authenticationType, nameType, roleType)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class from the specified <see cref="T:System.Security.Principal.IIdentity" /> using the specified claims, authentication type, name claim type, and role claim type.</summary>
	/// <param name="identity">The identity from which to base the new claims identity.</param>
	/// <param name="claims">The claims with which to populate the new claims identity.</param>
	/// <param name="authenticationType">The type of authentication used.</param>
	/// <param name="nameType">The claim type to use for name claims.</param>
	/// <param name="roleType">The claim type to use for role claims.</param>
	public ClaimsIdentity(IIdentity identity, IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType)
		: this(identity, claims, authenticationType, nameType, roleType, checkAuthType: true)
	{
	}

	internal ClaimsIdentity(IIdentity identity, IEnumerable<Claim> claims, string authenticationType, string nameType, string roleType, bool checkAuthType)
	{
		bool flag = false;
		bool flag2 = false;
		if (checkAuthType && identity != null && string.IsNullOrEmpty(authenticationType))
		{
			if (identity is WindowsIdentity)
			{
				try
				{
					m_authenticationType = identity.AuthenticationType;
				}
				catch (UnauthorizedAccessException)
				{
					m_authenticationType = null;
				}
			}
			else
			{
				m_authenticationType = identity.AuthenticationType;
			}
		}
		else
		{
			m_authenticationType = authenticationType;
		}
		if (!string.IsNullOrEmpty(nameType))
		{
			m_nameType = nameType;
			flag = true;
		}
		if (!string.IsNullOrEmpty(roleType))
		{
			m_roleType = roleType;
			flag2 = true;
		}
		if (identity is ClaimsIdentity claimsIdentity)
		{
			m_label = claimsIdentity.m_label;
			if (!flag)
			{
				m_nameType = claimsIdentity.m_nameType;
			}
			if (!flag2)
			{
				m_roleType = claimsIdentity.m_roleType;
			}
			m_bootstrapContext = claimsIdentity.m_bootstrapContext;
			if (claimsIdentity.Actor != null)
			{
				if (IsCircular(claimsIdentity.Actor))
				{
					throw new InvalidOperationException(Environment.GetResourceString("Actor cannot be set so that circular directed graph will exist chaining the subjects together."));
				}
				if (!AppContextSwitches.SetActorAsReferenceWhenCopyingClaimsIdentity)
				{
					m_actor = claimsIdentity.Actor.Clone();
				}
				else
				{
					m_actor = claimsIdentity.Actor;
				}
			}
			if (claimsIdentity is WindowsIdentity && !(this is WindowsIdentity))
			{
				SafeAddClaims(claimsIdentity.Claims);
			}
			else
			{
				SafeAddClaims(claimsIdentity.m_instanceClaims);
			}
			if (claimsIdentity.m_userSerializationData != null)
			{
				m_userSerializationData = claimsIdentity.m_userSerializationData.Clone() as byte[];
			}
		}
		else if (identity != null && !string.IsNullOrEmpty(identity.Name))
		{
			SafeAddClaim(new Claim(m_nameType, identity.Name, "http://www.w3.org/2001/XMLSchema#string", "LOCAL AUTHORITY", "LOCAL AUTHORITY", this));
		}
		if (claims != null)
		{
			SafeAddClaims(claims);
		}
	}

	public ClaimsIdentity(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Initialize(reader);
	}

	protected ClaimsIdentity(ClaimsIdentity other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other.m_actor != null)
		{
			m_actor = other.m_actor.Clone();
		}
		m_authenticationType = other.m_authenticationType;
		m_bootstrapContext = other.m_bootstrapContext;
		m_label = other.m_label;
		m_nameType = other.m_nameType;
		m_roleType = other.m_roleType;
		if (other.m_userSerializationData != null)
		{
			m_userSerializationData = other.m_userSerializationData.Clone() as byte[];
		}
		SafeAddClaims(other.m_instanceClaims);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class from a serialized stream created by using <see cref="T:System.Runtime.Serialization.ISerializable" />.</summary>
	/// <param name="info">The serialized data.</param>
	/// <param name="context">The context for serialization.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	protected ClaimsIdentity(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Deserialize(info, context, useContext: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Claims.ClaimsIdentity" /> class from a serialized stream created by using <see cref="T:System.Runtime.Serialization.ISerializable" />.</summary>
	/// <param name="info">The serialized data.</param>
	[SecurityCritical]
	protected ClaimsIdentity(SerializationInfo info)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Deserialize(info, default(StreamingContext), useContext: false);
	}

	/// <summary>Returns a new <see cref="T:System.Security.Claims.ClaimsIdentity" /> copied from this claims identity.</summary>
	/// <returns>A copy of the current instance.</returns>
	public virtual ClaimsIdentity Clone()
	{
		ClaimsIdentity claimsIdentity = new ClaimsIdentity(m_instanceClaims);
		claimsIdentity.m_authenticationType = m_authenticationType;
		claimsIdentity.m_bootstrapContext = m_bootstrapContext;
		claimsIdentity.m_label = m_label;
		claimsIdentity.m_nameType = m_nameType;
		claimsIdentity.m_roleType = m_roleType;
		if (Actor != null)
		{
			if (IsCircular(Actor))
			{
				throw new InvalidOperationException(Environment.GetResourceString("Actor cannot be set so that circular directed graph will exist chaining the subjects together."));
			}
			if (!AppContextSwitches.SetActorAsReferenceWhenCopyingClaimsIdentity)
			{
				claimsIdentity.Actor = Actor.Clone();
			}
			else
			{
				claimsIdentity.Actor = Actor;
			}
		}
		return claimsIdentity;
	}

	/// <summary>Adds a single claim to this claims identity.</summary>
	/// <param name="claim">The claim to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="claim" /> is null.</exception>
	[SecurityCritical]
	public virtual void AddClaim(Claim claim)
	{
		if (claim == null)
		{
			throw new ArgumentNullException("claim");
		}
		if (claim.Subject == this)
		{
			m_instanceClaims.Add(claim);
		}
		else
		{
			m_instanceClaims.Add(claim.Clone(this));
		}
	}

	/// <summary>Adds a list of claims to this claims identity.</summary>
	/// <param name="claims">The claims to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="claims" /> is null.</exception>
	[SecurityCritical]
	public virtual void AddClaims(IEnumerable<Claim> claims)
	{
		if (claims == null)
		{
			throw new ArgumentNullException("claims");
		}
		foreach (Claim claim in claims)
		{
			if (claim != null)
			{
				AddClaim(claim);
			}
		}
	}

	/// <summary>Attempts to remove a claim from the claims identity.</summary>
	/// <returns>true if the claim was successfully removed; otherwise, false.</returns>
	/// <param name="claim">The claim to remove.</param>
	[SecurityCritical]
	public virtual bool TryRemoveClaim(Claim claim)
	{
		bool result = false;
		for (int i = 0; i < m_instanceClaims.Count; i++)
		{
			if (m_instanceClaims[i] == claim)
			{
				m_instanceClaims.RemoveAt(i);
				result = true;
				break;
			}
		}
		return result;
	}

	/// <summary>Attempts to remove a claim from the claims identity.</summary>
	/// <param name="claim">The claim to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The claim cannot be removed.</exception>
	[SecurityCritical]
	public virtual void RemoveClaim(Claim claim)
	{
		if (!TryRemoveClaim(claim))
		{
			throw new InvalidOperationException(Environment.GetResourceString("The Claim '{0}' was not able to be removed.  It is either not part of this Identity or it is a claim that is owned by the Principal that contains this Identity. For example, the Principal will own the claim when creating a GenericPrincipal with roles. The roles will be exposed through the Identity that is passed in the constructor, but not actually owned by the Identity.  Similar logic exists for a RolePrincipal.", claim));
		}
	}

	[SecuritySafeCritical]
	private void SafeAddClaims(IEnumerable<Claim> claims)
	{
		foreach (Claim claim in claims)
		{
			if (claim.Subject == this)
			{
				m_instanceClaims.Add(claim);
			}
			else
			{
				m_instanceClaims.Add(claim.Clone(this));
			}
		}
	}

	[SecuritySafeCritical]
	private void SafeAddClaim(Claim claim)
	{
		if (claim.Subject == this)
		{
			m_instanceClaims.Add(claim);
		}
		else
		{
			m_instanceClaims.Add(claim.Clone(this));
		}
	}

	/// <summary>Retrieves all of the claims that are matched by the specified predicate.</summary>
	/// <returns>The matching claims. The list is read-only.</returns>
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
		foreach (Claim claim in Claims)
		{
			if (match(claim))
			{
				list.Add(claim);
			}
		}
		return list.AsReadOnly();
	}

	/// <summary>Retrieves all of the claims that have the specified claim type.</summary>
	/// <returns>The matching claims. The list is read-only.</returns>
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
		foreach (Claim claim in Claims)
		{
			if (claim != null && string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(claim);
			}
		}
		return list.AsReadOnly();
	}

	/// <summary>Determines whether this claims identity has a claim that is matched by the specified predicate.</summary>
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
		foreach (Claim claim in Claims)
		{
			if (match(claim))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether this claims identity has a claim with the specified claim type and value.</summary>
	/// <returns>true if a match is found; otherwise, false.</returns>
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
		foreach (Claim claim in Claims)
		{
			if (claim != null && claim != null && string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase) && string.Equals(claim.Value, value, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
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
		foreach (Claim claim in Claims)
		{
			if (match(claim))
			{
				return claim;
			}
		}
		return null;
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
		foreach (Claim claim in Claims)
		{
			if (claim != null && string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase))
			{
				return claim;
			}
		}
		return null;
	}

	[OnSerializing]
	[SecurityCritical]
	private void OnSerializingMethod(StreamingContext context)
	{
		if (!(this is ISerializable))
		{
			m_serializedClaims = SerializeClaims();
			m_serializedNameType = m_nameType;
			m_serializedRoleType = m_roleType;
		}
	}

	[SecurityCritical]
	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext context)
	{
		if (!(this is ISerializable))
		{
			if (!string.IsNullOrEmpty(m_serializedClaims))
			{
				DeserializeClaims(m_serializedClaims);
				m_serializedClaims = null;
			}
			m_nameType = (string.IsNullOrEmpty(m_serializedNameType) ? "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" : m_serializedNameType);
			m_roleType = (string.IsNullOrEmpty(m_serializedRoleType) ? "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" : m_serializedRoleType);
		}
	}

	[OnDeserializing]
	private void OnDeserializingMethod(StreamingContext context)
	{
		if (!(this is ISerializable))
		{
			m_instanceClaims = new List<Claim>();
			m_externalClaims = new Collection<IEnumerable<Claim>>();
		}
	}

	/// <summary>Populates the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with data needed to serialize the current <see cref="T:System.Security.Claims.ClaimsIdentity" /> object.</summary>
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
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		info.AddValue("System.Security.ClaimsIdentity.version", m_version);
		if (!string.IsNullOrEmpty(m_authenticationType))
		{
			info.AddValue("System.Security.ClaimsIdentity.authenticationType", m_authenticationType);
		}
		info.AddValue("System.Security.ClaimsIdentity.nameClaimType", m_nameType);
		info.AddValue("System.Security.ClaimsIdentity.roleClaimType", m_roleType);
		if (!string.IsNullOrEmpty(m_label))
		{
			info.AddValue("System.Security.ClaimsIdentity.label", m_label);
		}
		if (m_actor != null)
		{
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, m_actor, null, fCheck: false);
			info.AddValue("System.Security.ClaimsIdentity.actor", Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length));
		}
		info.AddValue("System.Security.ClaimsIdentity.claims", SerializeClaims());
		if (m_bootstrapContext != null)
		{
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream2, m_bootstrapContext, null, fCheck: false);
				info.AddValue("System.Security.ClaimsIdentity.bootstrapContext", Convert.ToBase64String(memoryStream2.GetBuffer(), 0, (int)memoryStream2.Length));
			}
		}
	}

	[SecurityCritical]
	private void DeserializeClaims(string serializedClaims)
	{
		if (!string.IsNullOrEmpty(serializedClaims))
		{
			using MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(serializedClaims));
			m_instanceClaims = (List<Claim>)new BinaryFormatter().Deserialize(serializationStream, null, fCheck: false);
			for (int i = 0; i < m_instanceClaims.Count; i++)
			{
				m_instanceClaims[i].Subject = this;
			}
		}
		if (m_instanceClaims == null)
		{
			m_instanceClaims = new List<Claim>();
		}
	}

	[SecurityCritical]
	private string SerializeClaims()
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, m_instanceClaims, null, fCheck: false);
		return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
	}

	private bool IsCircular(ClaimsIdentity subject)
	{
		if (this == subject)
		{
			return true;
		}
		ClaimsIdentity claimsIdentity = subject;
		while (claimsIdentity.Actor != null)
		{
			if (this == claimsIdentity.Actor)
			{
				return true;
			}
			claimsIdentity = claimsIdentity.Actor;
		}
		return false;
	}

	private void Initialize(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		int num = reader.ReadInt32();
		if ((num & 1) == 1)
		{
			m_authenticationType = reader.ReadString();
		}
		if ((num & 2) == 2)
		{
			m_bootstrapContext = reader.ReadString();
		}
		if ((num & 4) == 4)
		{
			m_nameType = reader.ReadString();
		}
		else
		{
			m_nameType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
		}
		if ((num & 8) == 8)
		{
			m_roleType = reader.ReadString();
		}
		else
		{
			m_roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
		}
		if ((num & 0x10) == 16)
		{
			int num2 = reader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				Claim item = new Claim(reader, this);
				m_instanceClaims.Add(item);
			}
		}
	}

	protected virtual Claim CreateClaim(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return new Claim(reader, this);
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
		if (m_authenticationType != null)
		{
			serializationMask |= SerializationMask.AuthenticationType;
			num++;
		}
		if (m_bootstrapContext != null && m_bootstrapContext is string)
		{
			serializationMask |= SerializationMask.BootstrapConext;
			num++;
		}
		if (!string.Equals(m_nameType, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", StringComparison.Ordinal))
		{
			serializationMask |= SerializationMask.NameClaimType;
			num++;
		}
		if (!string.Equals(m_roleType, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.Ordinal))
		{
			serializationMask |= SerializationMask.RoleClaimType;
			num++;
		}
		if (!string.IsNullOrWhiteSpace(m_label))
		{
			serializationMask |= SerializationMask.HasLabel;
			num++;
		}
		if (m_instanceClaims.Count > 0)
		{
			serializationMask |= SerializationMask.HasClaims;
			num++;
		}
		if (m_actor != null)
		{
			serializationMask |= SerializationMask.Actor;
			num++;
		}
		if (userData != null && userData.Length != 0)
		{
			num++;
			serializationMask |= SerializationMask.UserData;
		}
		writer.Write((int)serializationMask);
		writer.Write(num);
		if ((serializationMask & SerializationMask.AuthenticationType) == SerializationMask.AuthenticationType)
		{
			writer.Write(m_authenticationType);
		}
		if ((serializationMask & SerializationMask.BootstrapConext) == SerializationMask.BootstrapConext)
		{
			writer.Write(m_bootstrapContext as string);
		}
		if ((serializationMask & SerializationMask.NameClaimType) == SerializationMask.NameClaimType)
		{
			writer.Write(m_nameType);
		}
		if ((serializationMask & SerializationMask.RoleClaimType) == SerializationMask.RoleClaimType)
		{
			writer.Write(m_roleType);
		}
		if ((serializationMask & SerializationMask.HasLabel) == SerializationMask.HasLabel)
		{
			writer.Write(m_label);
		}
		if ((serializationMask & SerializationMask.HasClaims) == SerializationMask.HasClaims)
		{
			writer.Write(m_instanceClaims.Count);
			foreach (Claim instanceClaim in m_instanceClaims)
			{
				instanceClaim.WriteTo(writer);
			}
		}
		if ((serializationMask & SerializationMask.Actor) == SerializationMask.Actor)
		{
			m_actor.WriteTo(writer);
		}
		if ((serializationMask & SerializationMask.UserData) == SerializationMask.UserData)
		{
			writer.Write(userData.Length);
			writer.Write(userData);
		}
		writer.Flush();
	}

	[SecurityCritical]
	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	private void Deserialize(SerializationInfo info, StreamingContext context, bool useContext)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		BinaryFormatter binaryFormatter = ((!useContext) ? new BinaryFormatter() : new BinaryFormatter(null, context));
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Name)
			{
			case "System.Security.ClaimsIdentity.version":
				info.GetString("System.Security.ClaimsIdentity.version");
				break;
			case "System.Security.ClaimsIdentity.authenticationType":
				m_authenticationType = info.GetString("System.Security.ClaimsIdentity.authenticationType");
				break;
			case "System.Security.ClaimsIdentity.nameClaimType":
				m_nameType = info.GetString("System.Security.ClaimsIdentity.nameClaimType");
				break;
			case "System.Security.ClaimsIdentity.roleClaimType":
				m_roleType = info.GetString("System.Security.ClaimsIdentity.roleClaimType");
				break;
			case "System.Security.ClaimsIdentity.label":
				m_label = info.GetString("System.Security.ClaimsIdentity.label");
				break;
			case "System.Security.ClaimsIdentity.actor":
			{
				using (MemoryStream serializationStream2 = new MemoryStream(Convert.FromBase64String(info.GetString("System.Security.ClaimsIdentity.actor"))))
				{
					m_actor = (ClaimsIdentity)binaryFormatter.Deserialize(serializationStream2, null, fCheck: false);
				}
				break;
			}
			case "System.Security.ClaimsIdentity.claims":
				DeserializeClaims(info.GetString("System.Security.ClaimsIdentity.claims"));
				break;
			case "System.Security.ClaimsIdentity.bootstrapContext":
			{
				using (MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(info.GetString("System.Security.ClaimsIdentity.bootstrapContext"))))
				{
					m_bootstrapContext = binaryFormatter.Deserialize(serializationStream, null, fCheck: false);
				}
				break;
			}
			}
		}
	}
}
