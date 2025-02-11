using System.Collections.Generic;
using MS.Internal;
using MS.Internal.Security.RightsManagement;

namespace System.Security.RightsManagement;

/// <summary>Represents a user or user-group for granting access to rights managed content.     </summary>
public class ContentUser
{
	internal sealed class ContentUserComparer : IEqualityComparer<ContentUser>
	{
		bool IEqualityComparer<ContentUser>.Equals(ContentUser user1, ContentUser user2)
		{
			Invariant.Assert(user1 != null, "user1 should not be null");
			return user1.GenericEquals(user2);
		}

		int IEqualityComparer<ContentUser>.GetHashCode(ContentUser user)
		{
			Invariant.Assert(user != null, "user should not be null");
			return user.GetHashCode();
		}
	}

	internal static readonly ContentUserComparer _contentUserComparer = new ContentUserComparer();

	private const string WindowsAuthProvider = "WindowsAuthProvider";

	private const string PassportAuthProvider = "PassportAuthProvider";

	private const string OwnerUserName = "Owner";

	private static ContentUser _ownerUser;

	private const string AnyoneUserName = "Anyone";

	private static ContentUser _anyoneUser;

	private string _name;

	private AuthenticationType _authenticationType;

	private int hashValue;

	private bool hashCalcIsDone;

	/// <summary>Gets the <see cref="T:System.Security.RightsManagement.AuthenticationType" /> specified to the <see cref="M:System.Security.RightsManagement.ContentUser.#ctor(System.String,System.Security.RightsManagement.AuthenticationType)" /> constructor.</summary>
	/// <returns>The <see cref="T:System.Security.RightsManagement.AuthenticationType" /> specified to the <see cref="M:System.Security.RightsManagement.ContentUser.#ctor(System.String,System.Security.RightsManagement.AuthenticationType)" /> constructor.</returns>
	public AuthenticationType AuthenticationType => _authenticationType;

	/// <summary>Gets the user or group name specified to the <see cref="M:System.Security.RightsManagement.ContentUser.#ctor(System.String,System.Security.RightsManagement.AuthenticationType)" /> constructor.</summary>
	/// <returns>The user or group name specified to the <see cref="M:System.Security.RightsManagement.ContentUser.#ctor(System.String,System.Security.RightsManagement.AuthenticationType)" /> constructor.</returns>
	public string Name => _name;

	/// <summary>Gets an instance of the "Anyone" <see cref="T:System.Security.RightsManagement.ContentUser" /> persona.</summary>
	/// <returns>An instance of the "Anyone" <see cref="T:System.Security.RightsManagement.ContentUser" /> persona.</returns>
	public static ContentUser AnyoneUser
	{
		get
		{
			if (_anyoneUser == null)
			{
				_anyoneUser = new ContentUser("Anyone", AuthenticationType.Internal);
			}
			return _anyoneUser;
		}
	}

	/// <summary>Gets an instance of the "Owner" <see cref="T:System.Security.RightsManagement.ContentUser" /> persona.</summary>
	/// <returns>An instance of the "Owner" <see cref="T:System.Security.RightsManagement.ContentUser" /> persona.</returns>
	public static ContentUser OwnerUser
	{
		get
		{
			if (_ownerUser == null)
			{
				_ownerUser = new ContentUser("Owner", AuthenticationType.Internal);
			}
			return _ownerUser;
		}
	}

	internal string AuthenticationProviderType
	{
		get
		{
			if (_authenticationType == AuthenticationType.Windows)
			{
				return "WindowsAuthProvider";
			}
			if (_authenticationType == AuthenticationType.Passport)
			{
				return "PassportAuthProvider";
			}
			Invariant.Assert(condition: false, "AuthenticationProviderType can only be queried for Windows or Passport authentication");
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.ContentUser" /> class.</summary>
	/// <param name="name">The user or group name.</param>
	/// <param name="authenticationType">The method for authentication.</param>
	public ContentUser(string name, AuthenticationType authenticationType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Trim().Length == 0)
		{
			throw new ArgumentOutOfRangeException("name");
		}
		if (authenticationType != 0 && authenticationType != AuthenticationType.Passport && authenticationType != AuthenticationType.WindowsPassport && authenticationType != AuthenticationType.Internal)
		{
			throw new ArgumentOutOfRangeException("authenticationType");
		}
		if (authenticationType == AuthenticationType.Internal && !CompareToAnyone(name) && !CompareToOwner(name))
		{
			throw new ArgumentOutOfRangeException("name");
		}
		_name = name;
		_authenticationType = authenticationType;
	}

	/// <summary>Returns a value that indicates whether the user is currently authenticated.</summary>
	/// <returns>true if the user is currently authenticated; otherwise, false.  The default is false until authenticated.</returns>
	public bool IsAuthenticated()
	{
		if (_authenticationType != 0 && _authenticationType != AuthenticationType.Passport)
		{
			return false;
		}
		using ClientSession clientSession = new ClientSession(this);
		return clientSession.IsMachineActivated() && clientSession.IsUserActivated();
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Security.RightsManagement.ContentUser" /> is equivalent to another given instance.</summary>
	/// <returns>true if <see cref="P:System.Security.RightsManagement.ContentUser.Name" /> and <see cref="P:System.Security.RightsManagement.ContentUser.AuthenticationType" /> are the same for both this user and the given user; otherwise, false.</returns>
	/// <param name="obj">The user instance to compare for equality.</param>
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (GetType() != obj.GetType())
		{
			return false;
		}
		ContentUser contentUser = (ContentUser)obj;
		if (string.CompareOrdinal(_name.ToUpperInvariant(), contentUser._name.ToUpperInvariant()) == 0)
		{
			return _authenticationType.Equals(contentUser._authenticationType);
		}
		return false;
	}

	/// <summary>Returns a computed hash code based on the user <see cref="P:System.Security.RightsManagement.ContentUser.Name" /> and <see cref="P:System.Security.RightsManagement.ContentUser.AuthenticationType" />.</summary>
	/// <returns>A hash code computed from the user <see cref="P:System.Security.RightsManagement.ContentUser.Name" /> and <see cref="P:System.Security.RightsManagement.ContentUser.AuthenticationType" />.</returns>
	public override int GetHashCode()
	{
		if (!hashCalcIsDone)
		{
			hashValue = (_name.ToUpperInvariant() + _authenticationType).GetHashCode();
			hashCalcIsDone = true;
		}
		return hashValue;
	}

	internal bool GenericEquals(ContentUser userObj)
	{
		if (userObj == null)
		{
			return false;
		}
		if (string.CompareOrdinal(_name.ToUpperInvariant(), userObj._name.ToUpperInvariant()) == 0)
		{
			return _authenticationType.Equals(userObj._authenticationType);
		}
		return false;
	}

	internal static bool CompareToAnyone(string name)
	{
		return string.CompareOrdinal("Anyone".ToUpperInvariant(), name.ToUpperInvariant()) == 0;
	}

	internal static bool CompareToOwner(string name)
	{
		return string.CompareOrdinal("Owner".ToUpperInvariant(), name.ToUpperInvariant()) == 0;
	}
}
