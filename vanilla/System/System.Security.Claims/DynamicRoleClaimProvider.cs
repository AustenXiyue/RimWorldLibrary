using System.Collections.Generic;
using System.ComponentModel;
using Unity;

namespace System.Security.Claims;

/// <summary>The single method, <see cref="M:System.Security.Claims.DynamicRoleClaimProvider.AddDynamicRoleClaims(System.Security.Claims.ClaimsIdentity,System.Collections.Generic.IEnumerable{System.Security.Claims.Claim})" />, exposed by this class is obsolete. You can use a <see cref="T:System.Security.Claims.ClaimsAuthenticationManager" /> object to add claims to a <see cref="T:System.Security.Claims.ClaimsIdentity" /> object.</summary>
public static class DynamicRoleClaimProvider
{
	/// <summary>You can use a <see cref="T:System.Security.Claims.ClaimsAuthenticationManager" /> object to add claims to a <see cref="T:System.Security.Claims.ClaimsIdentity" /> object.</summary>
	/// <param name="claimsIdentity">The claims identity to which to add the claims.</param>
	/// <param name="claims">The claims to add.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ClaimsAuthenticationManager to add claims to a ClaimsIdentity", true)]
	public static void AddDynamicRoleClaims(ClaimsIdentity claimsIdentity, IEnumerable<Claim> claims)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
