namespace System;

/// <summary>A customizable parser based on the Lightweight Directory Access Protocol (LDAP) scheme.</summary>
public class LdapStyleUriParser : UriParser
{
	/// <summary>Creates a customizable parser based on the Lightweight Directory Access Protocol (LDAP) scheme.</summary>
	public LdapStyleUriParser()
		: base(UriParser.LdapUri.Flags)
	{
	}
}
