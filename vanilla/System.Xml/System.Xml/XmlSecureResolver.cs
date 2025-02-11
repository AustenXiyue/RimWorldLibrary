using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;

namespace System.Xml;

/// <summary>Helps to secure another implementation of <see cref="T:System.Xml.XmlResolver" /> by wrapping the <see cref="T:System.Xml.XmlResolver" /> object and restricting the resources that the underlying <see cref="T:System.Xml.XmlResolver" /> has access to.</summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class XmlSecureResolver : XmlResolver
{
	private XmlResolver resolver;

	/// <summary>Sets credentials used to authenticate Web requests.</summary>
	/// <returns>The credentials used to authenticate Web requests. The <see cref="T:System.Xml.XmlSecureResolver" /> sets the given credentials on the underlying <see cref="T:System.Xml.XmlResolver" />. If this property is not set, the value defaults to null; that is, the <see cref="T:System.Xml.XmlSecureResolver" /> has no user credentials.</returns>
	public override ICredentials Credentials
	{
		set
		{
			resolver.Credentials = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlSecureResolver" /> class with the <see cref="T:System.Xml.XmlResolver" /> and URL provided.</summary>
	/// <param name="resolver">The <see cref="T:System.Xml.XmlResolver" /> wrapped by the <see cref="T:System.Xml.XmlSecureResolver" />.</param>
	/// <param name="securityUrl">The URL used to create the <see cref="T:System.Security.PermissionSet" /> that will be applied to the underlying <see cref="T:System.Xml.XmlResolver" />. The <see cref="T:System.Xml.XmlSecureResolver" /> calls <see cref="M:System.Security.PermissionSet.PermitOnly" /> on the created <see cref="T:System.Security.PermissionSet" /> before calling <see cref="M:System.Xml.XmlSecureResolver.GetEntity(System.Uri,System.String,System.Type)" /> on the underlying <see cref="T:System.Xml.XmlResolver" />.</param>
	public XmlSecureResolver(XmlResolver resolver, string securityUrl)
		: this(resolver, (PermissionSet)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlSecureResolver" /> class with the <see cref="T:System.Xml.XmlResolver" /> and <see cref="T:System.Security.Policy.Evidence" /> specified.</summary>
	/// <param name="resolver">The <see cref="T:System.Xml.XmlResolver" /> wrapped by the <see cref="T:System.Xml.XmlSecureResolver" />.</param>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> used to create the <see cref="T:System.Security.PermissionSet" /> that will be applied to the underlying <see cref="T:System.Xml.XmlResolver" />. The <see cref="T:System.Xml.XmlSecureResolver" /> calls <see cref="M:System.Security.PermissionSet.PermitOnly" /> on the created <see cref="T:System.Security.PermissionSet" /> before calling <see cref="M:System.Xml.XmlSecureResolver.GetEntity(System.Uri,System.String,System.Type)" /> on the underlying <see cref="T:System.Xml.XmlResolver" />.</param>
	public XmlSecureResolver(XmlResolver resolver, Evidence evidence)
		: this(resolver, (PermissionSet)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlSecureResolver" /> class with the <see cref="T:System.Xml.XmlResolver" /> and <see cref="T:System.Security.PermissionSet" /> specified.</summary>
	/// <param name="resolver">The <see cref="T:System.Xml.XmlResolver" /> wrapped by the <see cref="T:System.Xml.XmlSecureResolver" />.</param>
	/// <param name="permissionSet">The <see cref="T:System.Security.PermissionSet" /> to apply to the underlying <see cref="T:System.Xml.XmlResolver" />. The <see cref="T:System.Xml.XmlSecureResolver" /> calls <see cref="M:System.Security.PermissionSet.PermitOnly" /> on the <see cref="T:System.Security.PermissionSet" /> before calling <see cref="M:System.Xml.XmlSecureResolver.GetEntity(System.Uri,System.String,System.Type)" /> on the underlying <see cref="T:System.Xml.XmlResolver" />.</param>
	public XmlSecureResolver(XmlResolver resolver, PermissionSet permissionSet)
	{
		this.resolver = resolver;
	}

	/// <summary>Maps a URI to an object containing the actual resource. This method temporarily sets the <see cref="T:System.Security.PermissionSet" /> created in the constructor by calling <see cref="M:System.Security.PermissionSet.PermitOnly" /> before calling GetEntity on the underlying <see cref="T:System.Xml.XmlResolver" /> to open the resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object returned by calling GetEntity on the underlying <see cref="T:System.Xml.XmlResolver" />. If a type other than stream is specified, null is returned.</returns>
	/// <param name="absoluteUri">The URI returned from <see cref="M:System.Xml.XmlSecureResolver.ResolveUri(System.Uri,System.String)" />.</param>
	/// <param name="role">The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink:role and used as an implementation-specific argument in other scenarios.</param>
	/// <param name="ofObjectToReturn">The type of object to return. The current version only returns <see cref="T:System.IO.Stream" /> objects.</param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="ofObjectToReturn" /> is neither null nor a <see cref="T:System.IO.Stream" /> type.</exception>
	/// <exception cref="T:System.UriFormatException">The specified URI is not an absolute URI.</exception>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="absoluteUri" /> is null.</exception>
	/// <exception cref="T:System.Exception">There is a runtime error (for example, an interrupted server connection).</exception>
	public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
	{
		return resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
	}

	/// <summary>Resolves the absolute URI from the base and relative URIs by calling ResolveUri on the underlying <see cref="T:System.Xml.XmlResolver" />.</summary>
	/// <returns>A <see cref="T:System.Uri" /> representing the absolute URI or null if the relative URI cannot be resolved (returned by calling ResolveUri on the underlying <see cref="T:System.Xml.XmlResolver" />).</returns>
	/// <param name="baseUri">The base URI used to resolve the relative URI.</param>
	/// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative. If absolute, this value effectively replaces the <paramref name="baseUri" /> value. If relative, it combines with the <paramref name="baseUri" /> to make an absolute URI.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="relativeUri" /> is null.</exception>
	public override Uri ResolveUri(Uri baseUri, string relativeUri)
	{
		return resolver.ResolveUri(baseUri, relativeUri);
	}

	/// <summary>Creates evidence using the supplied URL.</summary>
	/// <returns>The evidence generated from the supplied URL as defined by the default policy.</returns>
	/// <param name="securityUrl">The URL used to create the evidence.</param>
	public static Evidence CreateEvidenceForUrl(string securityUrl)
	{
		return null;
	}

	/// <summary>Asynchronously maps a URI to an object containing the actual resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object or null if a type other than stream is specified.</returns>
	/// <param name="absoluteUri">The URI returned from <see cref="M:System.Xml.XmlSecureResolver.ResolveUri(System.Uri,System.String)" />.</param>
	/// <param name="role">The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink:role and used as an implementation-specific argument in other scenarios.</param>
	/// <param name="ofObjectToReturn">The type of object to return. The current version only returns <see cref="T:System.IO.Stream" /> objects.</param>
	public override Task<object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn)
	{
		return resolver.GetEntityAsync(absoluteUri, role, ofObjectToReturn);
	}
}
