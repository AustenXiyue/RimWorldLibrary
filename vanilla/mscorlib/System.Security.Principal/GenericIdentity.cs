using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace System.Security.Principal;

/// <summary>Represents a generic user.</summary>
[Serializable]
[ComVisible(true)]
public class GenericIdentity : ClaimsIdentity
{
	private string m_name;

	private string m_type;

	/// <summary>Gets all claims for the user represented by this generic identity.</summary>
	/// <returns>A collection of claims for this <see cref="T:System.Security.Principal.GenericIdentity" /> object.</returns>
	public override IEnumerable<Claim> Claims => base.Claims;

	/// <summary>Gets the user's name.</summary>
	/// <returns>The name of the user on whose behalf the code is being run.</returns>
	public override string Name => m_name;

	/// <summary>Gets the type of authentication used to identify the user.</summary>
	/// <returns>The type of authentication used to identify the user.</returns>
	public override string AuthenticationType => m_type;

	/// <summary>Gets a value indicating whether the user has been authenticated.</summary>
	/// <returns>true if the user was has been authenticated; otherwise, false.</returns>
	public override bool IsAuthenticated => !m_name.Equals("");

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.GenericIdentity" /> class representing the user with the specified name.</summary>
	/// <param name="name">The name of the user on whose behalf the code is running. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	[SecuritySafeCritical]
	public GenericIdentity(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		m_name = name;
		m_type = "";
		AddNameClaim();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.GenericIdentity" /> class representing the user with the specified name and authentication type.</summary>
	/// <param name="name">The name of the user on whose behalf the code is running. </param>
	/// <param name="type">The type of authentication used to identify the user. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null.-or- The <paramref name="type" /> parameter is null. </exception>
	[SecuritySafeCritical]
	public GenericIdentity(string name, string type)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		m_name = name;
		m_type = type;
		AddNameClaim();
	}

	private GenericIdentity()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.GenericIdentity" /> class by using the specified <see cref="T:System.Security.Principal.GenericIdentity" /> object.</summary>
	/// <param name="identity">The object from which to construct the new instance of <see cref="T:System.Security.Principal.GenericIdentity" />.</param>
	protected GenericIdentity(GenericIdentity identity)
		: base(identity)
	{
		m_name = identity.m_name;
		m_type = identity.m_type;
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A copy of the current instance.</returns>
	public override ClaimsIdentity Clone()
	{
		return new GenericIdentity(this);
	}

	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext context)
	{
		bool flag = false;
		using (IEnumerator<Claim> enumerator = base.Claims.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				flag = true;
			}
		}
		if (!flag)
		{
			AddNameClaim();
		}
	}

	[SecuritySafeCritical]
	private void AddNameClaim()
	{
		if (m_name != null)
		{
			base.AddClaim(new Claim(base.NameClaimType, m_name, "http://www.w3.org/2001/XMLSchema#string", "LOCAL AUTHORITY", "LOCAL AUTHORITY", this));
		}
	}
}
