using System;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace Microsoft.Win32;

/// <summary>Defines a credential policy to be used for resource requests that are made using <see cref="T:System.Net.WebRequest" /> and its derived classes.</summary>
public class IntranetZoneCredentialPolicy : ICredentialPolicy
{
	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.Win32.IntranetZoneCredentialPolicy" /> class.</summary>
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
	public IntranetZoneCredentialPolicy()
	{
	}

	/// <summary>Returns a <see cref="T:System.Boolean" /> that indicates whether the client's credentials are sent with a request for a resource that was made using <see cref="T:System.Net.WebRequest" />.</summary>
	/// <returns>true if the requested resource is in the same domain as the client making the request; otherwise, false.</returns>
	/// <param name="challengeUri">The <see cref="T:System.Uri" /> that will receive the request.</param>
	/// <param name="request">The <see cref="T:System.Net.WebRequest" /> that represents the resource being requested.</param>
	/// <param name="credential">The <see cref="T:System.Net.NetworkCredential" /> that will be sent with the request if this method returns true.</param>
	/// <param name="authModule">The <see cref="T:System.Net.IAuthenticationModule" /> that will conduct the authentication, if authentication is required.</param>
	public virtual bool ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authModule)
	{
		return Zone.CreateFromUrl(challengeUri.AbsoluteUri).SecurityZone == SecurityZone.Intranet;
	}
}
