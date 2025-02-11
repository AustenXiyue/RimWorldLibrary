using System.Net;
using System.Net.Cache;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.PresentationCore;

namespace System.IO.Packaging;

/// <summary>Makes a request to an entire <see cref="T:System.IO.Packaging.PackagePart" /> or to a <see cref="T:System.IO.Packaging.PackagePart" /> in a package, identified by a pack URI.</summary>
public sealed class PackWebRequest : WebRequest
{
	private Uri _uri;

	private Uri _innerUri;

	private Uri _partName;

	private WebRequest _webRequest;

	private Package _cacheEntry;

	private bool _respectCachePolicy;

	private bool _cachedPackageIsThreadSafe;

	private RequestCachePolicy _cachePolicy;

	private static RequestCachePolicy _defaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

	private static Uri _siteOfOriginUri = PackUriHelper.GetPackageUri(BaseUriHelper.SiteOfOriginBaseUri);

	private static Uri _appBaseUri = PackUriHelper.GetPackageUri(BaseUriHelper.PackAppBaseUri);

	/// <summary>Gets or sets the <see cref="T:System.Net.Cache.RequestCachePolicy" />.</summary>
	/// <returns>The <see cref="T:System.Net.Cache.RequestCachePolicy" /> to use with pack URI web request.</returns>
	/// <exception cref="T:System.Net.WebException">The specified <see cref="T:System.Net.Cache.RequestCachePolicy" /> to set is not valid.</exception>
	public override RequestCachePolicy CachePolicy
	{
		get
		{
			return _cachePolicy;
		}
		set
		{
			if (value == null)
			{
				_cachePolicy = _defaultCachePolicy;
				return;
			}
			switch (value.Level)
			{
			default:
				throw new WebException(SR.PackWebRequestCachePolicyIllegal);
			case RequestCacheLevel.BypassCache:
			case RequestCacheLevel.CacheOnly:
			case RequestCacheLevel.CacheIfAvailable:
				_cachePolicy = value;
				break;
			}
		}
	}

	/// <summary>Gets or sets the name of the connection group.</summary>
	/// <returns>The connection group name.</returns>
	public override string ConnectionGroupName
	{
		get
		{
			return GetRequest().ConnectionGroupName;
		}
		set
		{
			GetRequest().ConnectionGroupName = value;
		}
	}

	/// <summary>Gets or sets the Content-length HTTP header. </summary>
	/// <returns>The content length, in bytes.</returns>
	/// <exception cref="T:System.NotSupportedException">Set is not supported, <see cref="T:System.IO.Packaging.PackWebRequest" /> is read-only.</exception>
	public override long ContentLength
	{
		get
		{
			return GetRequest().ContentLength;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets or sets the Content-type HTTP header. </summary>
	/// <returns>The contents of the header.</returns>
	public override string ContentType
	{
		get
		{
			string contentType = GetRequest().ContentType;
			if (contentType == null)
			{
				return contentType;
			}
			return new ContentType(contentType).ToString();
		}
		set
		{
			GetRequest().ContentType = value;
		}
	}

	/// <summary>Gets or sets the authentication credentials.</summary>
	/// <returns>The authentication credentials to use with the request.</returns>
	public override ICredentials Credentials
	{
		get
		{
			return GetRequest().Credentials;
		}
		set
		{
			GetRequest().Credentials = value;
		}
	}

	/// <summary>Gets or sets the collection of header name/value pairs associated with the request. </summary>
	/// <returns>A header collection object.</returns>
	public override WebHeaderCollection Headers
	{
		get
		{
			return GetRequest().Headers;
		}
		set
		{
			GetRequest().Headers = value;
		}
	}

	/// <summary>Gets or sets the protocol method to use with the pack URI request.</summary>
	/// <returns>The protocol method name that performs this request.</returns>
	public override string Method
	{
		get
		{
			return GetRequest().Method;
		}
		set
		{
			GetRequest().Method = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to preauthenticate the request.</summary>
	/// <returns>true to send a WWW-authenticate HTTP header with the initial request; otherwise, false. </returns>
	public override bool PreAuthenticate
	{
		get
		{
			return GetRequest().PreAuthenticate;
		}
		set
		{
			GetRequest().PreAuthenticate = value;
		}
	}

	/// <summary>Gets or sets the network proxy for Internet access.</summary>
	/// <returns>The <see cref="T:System.Net.WebProxy" /> to use for Internet access.</returns>
	public override IWebProxy Proxy
	{
		get
		{
			return GetRequest().Proxy;
		}
		set
		{
			GetRequest().Proxy = value;
		}
	}

	/// <summary>Gets the URI of the resource associated with the request.</summary>
	/// <returns>The uniform resource identifier (URI) of the resource associated with the request.</returns>
	public override Uri RequestUri => _uri;

	/// <summary>Gets or sets the length of time before the request times out.</summary>
	/// <returns>The number of milliseconds to wait before the request times out.</returns>
	public override int Timeout
	{
		get
		{
			return GetRequest().Timeout;
		}
		set
		{
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			GetRequest().Timeout = value;
		}
	}

	/// <summary>Gets or sets the default authentication credentials.</summary>
	/// <returns>The default authentication credentials to use with the pack URI request.</returns>
	public override bool UseDefaultCredentials
	{
		get
		{
			return GetRequest().UseDefaultCredentials;
		}
		set
		{
			GetRequest().UseDefaultCredentials = value;
		}
	}

	private bool IsCachedPackage => _cacheEntry != null;

	private bool IsPreloadedPackage
	{
		get
		{
			if (_cacheEntry != null)
			{
				return !_respectCachePolicy;
			}
			return false;
		}
	}

	internal PackWebRequest(Uri uri, Uri packageUri, Uri partUri)
		: this(uri, packageUri, partUri, null, respectCachePolicy: false, cachedPackageIsThreadSafe: false)
	{
	}

	internal PackWebRequest(Uri uri, Uri packageUri, Uri partUri, Package cacheEntry, bool respectCachePolicy, bool cachedPackageIsThreadSafe)
	{
		_uri = uri;
		_innerUri = packageUri;
		_partName = partUri;
		_cacheEntry = cacheEntry;
		_respectCachePolicy = respectCachePolicy;
		_cachedPackageIsThreadSafe = cachedPackageIsThreadSafe;
		_cachePolicy = _defaultCachePolicy;
	}

	/// <summary>Do not use—<see cref="M:System.IO.Packaging.PackWebRequest.GetRequestStream" /> is not supported by <see cref="T:System.IO.Packaging.PackWebRequest" />.</summary>
	/// <returns>If <see cref="M:System.IO.Packaging.PackWebRequest.GetRequestStream" /> is called, a <see cref="T:System.NotSupportedException" /> is thrown.</returns>
	/// <exception cref="T:System.NotSupportedException">Occurs on any call to <see cref="M:System.IO.Packaging.PackWebRequest.GetRequestStream" />.  The pack URI protocol does not support writing.</exception>
	public override Stream GetRequestStream()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns the response stream for the request.</summary>
	/// <returns>The response stream for the request.</returns>
	public override WebResponse GetResponse()
	{
		bool flag = IsCachedPackage;
		if (!flag || (flag && _respectCachePolicy))
		{
			RequestCacheLevel level = _cachePolicy.Level;
			if (level == RequestCacheLevel.Default)
			{
				level = _defaultCachePolicy.Level;
			}
			switch (level)
			{
			case RequestCacheLevel.BypassCache:
				flag = false;
				break;
			case RequestCacheLevel.CacheOnly:
				if (!flag)
				{
					throw new WebException(SR.ResourceNotFoundUnderCacheOnlyPolicy);
				}
				break;
			default:
				throw new WebException(SR.PackWebRequestCachePolicyIllegal);
			case RequestCacheLevel.CacheIfAvailable:
				break;
			}
		}
		if (flag)
		{
			return new PackWebResponse(_uri, _innerUri, _partName, _cacheEntry, _cachedPackageIsThreadSafe);
		}
		WebRequest request = GetRequest(allowPseudoRequest: false);
		if (_webRequest == null || _webRequest is PseudoWebRequest)
		{
			throw new InvalidOperationException(SR.SchemaInvalidForTransport);
		}
		return new PackWebResponse(_uri, _innerUri, _partName, request);
	}

	/// <summary>Gets the inner <see cref="T:System.Net.WebRequest" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebRequest" /> created from the inner-URI if the request resolves to a valid transport protocol such http or ftp; or a <see cref="T:System.Net.WebRequest" /> created with a null-URI if the request resolves from the <see cref="T:System.IO.Packaging.PackageStore" /> cache.</returns>
	/// <exception cref="T:System.NotSupportedException">The inner URI does not resolve to a valid transport protocol such as http for ftp, nor can the request be resolved from the <see cref="T:System.IO.Packaging.PackageStore" />.</exception>
	public WebRequest GetInnerRequest()
	{
		WebRequest request = GetRequest(allowPseudoRequest: false);
		if (request == null || request is PseudoWebRequest)
		{
			return null;
		}
		return request;
	}

	private WebRequest GetRequest()
	{
		return GetRequest(allowPseudoRequest: true);
	}

	private WebRequest GetRequest(bool allowPseudoRequest)
	{
		if (_webRequest == null)
		{
			if (!IsPreloadedPackage)
			{
				try
				{
					_webRequest = WpfWebRequestHelper.CreateRequest(_innerUri);
					if (_webRequest is FtpWebRequest ftpWebRequest)
					{
						ftpWebRequest.UsePassive = false;
					}
				}
				catch (NotSupportedException)
				{
					if (!IsCachedPackage)
					{
						throw;
					}
				}
			}
			if (_webRequest == null && allowPseudoRequest)
			{
				_webRequest = new PseudoWebRequest(_uri, _innerUri, _partName, _cacheEntry);
			}
		}
		return _webRequest;
	}
}
