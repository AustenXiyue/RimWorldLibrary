using System.Net;
using System.Threading.Tasks;

namespace System.Xml;

[Obsolete("XmlSecureResolver is obsolete. Use XmlResolver.ThrowingResolver instead when attempting to forbid XML external entity resolution.", DiagnosticId = "SYSLIB0047", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
public class XmlSecureResolver : XmlResolver
{
	public override ICredentials Credentials
	{
		set
		{
		}
	}

	public XmlSecureResolver(XmlResolver resolver, string? securityUrl)
	{
	}

	public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
	{
		return XmlResolver.ThrowingResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
	}

	public override Task<object> GetEntityAsync(Uri absoluteUri, string? role, Type? ofObjectToReturn)
	{
		return XmlResolver.ThrowingResolver.GetEntityAsync(absoluteUri, role, ofObjectToReturn);
	}

	public override Uri ResolveUri(Uri? baseUri, string? relativeUri)
	{
		return base.ResolveUri(baseUri, relativeUri);
	}
}
