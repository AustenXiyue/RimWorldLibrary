using System;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Net.Cache;
using System.Windows.Navigation;
using MS.Internal.AppModel;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal;

internal static class WpfWebRequestHelper
{
	private static HttpRequestCachePolicy _httpRequestCachePolicy;

	private static HttpRequestCachePolicy _httpRequestCachePolicyRefresh;

	private static string _defaultUserAgent;

	internal static string DefaultUserAgent
	{
		get
		{
			if (_defaultUserAgent == null)
			{
				_defaultUserAgent = MS.Win32.UnsafeNativeMethods.ObtainUserAgentString();
			}
			return _defaultUserAgent;
		}
		set
		{
			_defaultUserAgent = value;
		}
	}

	[FriendAccessAllowed]
	internal static WebRequest CreateRequest(Uri uri)
	{
		if (string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.Ordinal))
		{
			return PackWebRequestFactory.CreateWebRequest(uri);
		}
		if (uri.IsFile)
		{
			uri = new Uri(uri.GetLeftPart(UriPartial.Path));
		}
		WebRequest obj = WebRequest.Create(uri) ?? throw new WebException(BaseUriHelper.PackAppBaseUri.MakeRelativeUri(uri).ToString(), WebExceptionStatus.RequestCanceled);
		if (obj is HttpWebRequest httpWebRequest)
		{
			if (string.IsNullOrEmpty(httpWebRequest.UserAgent))
			{
				httpWebRequest.UserAgent = DefaultUserAgent;
			}
			CookieHandler.HandleWebRequest(httpWebRequest);
			if (string.IsNullOrEmpty(httpWebRequest.Referer))
			{
				httpWebRequest.Referer = BindUriHelper.GetReferer(uri);
			}
			CustomCredentialPolicy.EnsureCustomCredentialPolicy();
			httpWebRequest.UseDefaultCredentials = true;
		}
		return obj;
	}

	[FriendAccessAllowed]
	internal static void ConfigCachePolicy(WebRequest request, bool isRefresh)
	{
		if (!(request is HttpWebRequest) || (request.CachePolicy != null && request.CachePolicy.Level == RequestCacheLevel.Default))
		{
			return;
		}
		if (isRefresh)
		{
			if (_httpRequestCachePolicyRefresh == null)
			{
				_httpRequestCachePolicyRefresh = new HttpRequestCachePolicy(HttpRequestCacheLevel.Refresh);
			}
			request.CachePolicy = _httpRequestCachePolicyRefresh;
		}
		else
		{
			if (_httpRequestCachePolicy == null)
			{
				_httpRequestCachePolicy = new HttpRequestCachePolicy();
			}
			request.CachePolicy = _httpRequestCachePolicy;
		}
	}

	[FriendAccessAllowed]
	internal static void HandleWebResponse(WebResponse response)
	{
		CookieHandler.HandleWebResponse(response);
	}

	[FriendAccessAllowed]
	internal static Stream CreateRequestAndGetResponseStream(Uri uri)
	{
		return GetResponseStream(CreateRequest(uri));
	}

	[FriendAccessAllowed]
	internal static Stream CreateRequestAndGetResponseStream(Uri uri, out ContentType contentType)
	{
		return GetResponseStream(CreateRequest(uri), out contentType);
	}

	[FriendAccessAllowed]
	internal static WebResponse CreateRequestAndGetResponse(Uri uri)
	{
		return GetResponse(CreateRequest(uri));
	}

	[FriendAccessAllowed]
	internal static WebResponse GetResponse(WebRequest request)
	{
		WebResponse response = request.GetResponse();
		if (response is HttpWebResponse && !(request is HttpWebRequest))
		{
			throw new ArgumentException();
		}
		if (response == null)
		{
			throw new IOException(SR.Format(p1: BaseUriHelper.PackAppBaseUri.MakeRelativeUri(request.RequestUri).ToString(), resourceFormat: SR.GetResponseFailed));
		}
		HandleWebResponse(response);
		return response;
	}

	[FriendAccessAllowed]
	internal static WebResponse EndGetResponse(WebRequest request, IAsyncResult ar)
	{
		WebResponse webResponse = request.EndGetResponse(ar);
		if (webResponse is HttpWebResponse && !(request is HttpWebRequest))
		{
			throw new ArgumentException();
		}
		if (webResponse == null)
		{
			throw new IOException(SR.Format(p1: BaseUriHelper.PackAppBaseUri.MakeRelativeUri(request.RequestUri).ToString(), resourceFormat: SR.GetResponseFailed));
		}
		HandleWebResponse(webResponse);
		return webResponse;
	}

	[FriendAccessAllowed]
	internal static Stream GetResponseStream(WebRequest request)
	{
		return GetResponse(request).GetResponseStream();
	}

	[FriendAccessAllowed]
	internal static Stream GetResponseStream(WebRequest request, out ContentType contentType)
	{
		WebResponse response = GetResponse(request);
		contentType = GetContentType(response);
		return response.GetResponseStream();
	}

	[FriendAccessAllowed]
	internal static ContentType GetContentType(WebResponse response)
	{
		ContentType contentType = ContentType.Empty;
		if (!(response is FileWebResponse))
		{
			try
			{
				contentType = new ContentType(response.ContentType);
				if (MimeTypeMapper.OctetMime.AreTypeAndSubTypeEqual(contentType, allowParameterValuePairs: true) || MimeTypeMapper.TextPlainMime.AreTypeAndSubTypeEqual(contentType, allowParameterValuePairs: true))
				{
					string fileExtension = MimeTypeMapper.GetFileExtension(response.ResponseUri);
					if (string.Equals(fileExtension, "xaml", StringComparison.OrdinalIgnoreCase) || string.Equals(fileExtension, "xbap", StringComparison.OrdinalIgnoreCase))
					{
						contentType = ContentType.Empty;
					}
				}
			}
			catch (NotImplementedException)
			{
			}
			catch (NotSupportedException)
			{
			}
		}
		if (contentType.TypeComponent == ContentType.Empty.TypeComponent && contentType.OriginalString == ContentType.Empty.OriginalString && contentType.SubTypeComponent == ContentType.Empty.SubTypeComponent)
		{
			contentType = MimeTypeMapper.GetMimeTypeFromUri(response.ResponseUri);
		}
		return contentType;
	}
}
