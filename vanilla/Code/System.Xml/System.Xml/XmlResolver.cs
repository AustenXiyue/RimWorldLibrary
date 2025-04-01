using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace System.Xml;

/// <summary>Resolves external XML resources named by a Uniform Resource Identifier (URI).</summary>
public abstract class XmlResolver
{
	/// <summary>When overridden in a derived class, sets the credentials used to authenticate Web requests.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> object. If this property is not set, the value defaults to null; that is, the XmlResolver has no user credentials.</returns>
	public virtual ICredentials Credentials
	{
		set
		{
		}
	}

	/// <summary>When overridden in a derived class, maps a URI to an object containing the actual resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object or null if a type other than stream is specified.</returns>
	/// <param name="absoluteUri">The URI returned from <see cref="M:System.Xml.XmlResolver.ResolveUri(System.Uri,System.String)" />.</param>
	/// <param name="role">The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink: role and used as an implementation specific argument in other scenarios.</param>
	/// <param name="ofObjectToReturn">The type of object to return. The current version only returns System.IO.Stream objects.</param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="ofObjectToReturn" /> is not a Stream type.</exception>
	/// <exception cref="T:System.UriFormatException">The specified URI is not an absolute URI.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="absoluteUri" /> is null.</exception>
	/// <exception cref="T:System.Exception">There is a runtime error (for example, an interrupted server connection).</exception>
	public abstract object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn);

	/// <summary>When overridden in a derived class, resolves the absolute URI from the base and relative URIs.</summary>
	/// <returns>A <see cref="T:System.Uri" /> representing the absolute URI or null if the relative URI cannot be resolved.</returns>
	/// <param name="baseUri">The base URI used to resolve the relative URI.</param>
	/// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative. If absolute, this value effectively replaces the <paramref name="baseUri" /> value. If relative, it combines with the <paramref name="baseUri" /> to make an absolute URI.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="relativeUri" /> is null.</exception>
	public virtual Uri ResolveUri(Uri baseUri, string relativeUri)
	{
		if (baseUri == null || (!baseUri.IsAbsoluteUri && baseUri.OriginalString.Length == 0))
		{
			Uri uri = new Uri(relativeUri, UriKind.RelativeOrAbsolute);
			if (!uri.IsAbsoluteUri && uri.OriginalString.Length > 0)
			{
				uri = new Uri(Path.GetFullPath(relativeUri));
			}
			return uri;
		}
		if (relativeUri == null || relativeUri.Length == 0)
		{
			return baseUri;
		}
		if (!baseUri.IsAbsoluteUri)
		{
			throw new NotSupportedException(Res.GetString("Relative URIs are not supported."));
		}
		return new Uri(baseUri, relativeUri);
	}

	/// <summary>Adds the ability for the resolver to return other types than just <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>true if the <paramref name="type" /> is supported; otherwise, false.</returns>
	/// <param name="absoluteUri">The URI.</param>
	/// <param name="type">The type to return.</param>
	public virtual bool SupportsType(Uri absoluteUri, Type type)
	{
		if (absoluteUri == null)
		{
			throw new ArgumentNullException("absoluteUri");
		}
		if (type == null || type == typeof(Stream))
		{
			return true;
		}
		return false;
	}

	/// <summary>Asynchronously maps a URI to an object containing the actual resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object or null if a type other than stream is specified.</returns>
	/// <param name="absoluteUri">The URI returned from <see cref="M:System.Xml.XmlResolver.ResolveUri(System.Uri,System.String)" />.</param>
	/// <param name="role">The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink: role and used as an implementation specific argument in other scenarios.</param>
	/// <param name="ofObjectToReturn">The type of object to return. The current version only returns <see cref="T:System.IO.Stream" /> objects.</param>
	public virtual Task<object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlResolver" /> class.</summary>
	protected XmlResolver()
	{
	}
}
