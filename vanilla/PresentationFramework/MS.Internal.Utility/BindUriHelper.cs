using System;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using MS.Internal.AppModel;

namespace MS.Internal.Utility;

internal static class BindUriHelper
{
	private const int MAX_PATH_LENGTH = 2048;

	private const int MAX_SCHEME_LENGTH = 32;

	public const int MAX_URL_LENGTH = 2083;

	private const string PLACEBOURI = "http://microsoft.com/";

	private static Uri placeboBase = new Uri("http://microsoft.com/");

	private const string FRAGMENTMARKER = "#";

	internal static Uri BaseUri
	{
		get
		{
			return BaseUriHelper.BaseUri;
		}
		set
		{
			BaseUriHelper.BaseUri = BaseUriHelper.FixFileUri(value);
		}
	}

	internal static string UriToString(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		return new StringBuilder(uri.GetComponents(uri.IsAbsoluteUri ? UriComponents.AbsoluteUri : UriComponents.SerializationInfoString, UriFormat.SafeUnescaped), 2083).ToString();
	}

	internal static bool DoSchemeAndHostMatch(Uri first, Uri second)
	{
		if (SecurityHelper.AreStringTypesEqual(first.Scheme, second.Scheme))
		{
			return first.Host.Equals(second.Host);
		}
		return false;
	}

	internal static Uri GetResolvedUri(Uri baseUri, Uri orgUri)
	{
		if (orgUri == null)
		{
			return null;
		}
		if (!orgUri.IsAbsoluteUri)
		{
			return new Uri((baseUri == null) ? BaseUri : baseUri, orgUri);
		}
		return BaseUriHelper.FixFileUri(orgUri);
	}

	internal static string GetReferer(Uri destinationUri)
	{
		string result = null;
		Uri browserSource = SiteOfOriginContainer.BrowserSource;
		if (browserSource != null)
		{
			int num = CustomCredentialPolicy.MapUrlToZone(browserSource);
			int num2 = CustomCredentialPolicy.MapUrlToZone(destinationUri);
			if (num == num2 && SecurityHelper.AreStringTypesEqual(browserSource.Scheme, destinationUri.Scheme))
			{
				result = browserSource.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
			}
		}
		return result;
	}

	internal static Uri GetResolvedUri(Uri originalUri)
	{
		return GetResolvedUri(null, originalUri);
	}

	internal static Uri GetUriToNavigate(DependencyObject element, Uri baseUri, Uri inputUri)
	{
		Uri result = inputUri;
		if (inputUri == null || inputUri.IsAbsoluteUri)
		{
			return result;
		}
		if (StartWithFragment(inputUri))
		{
			baseUri = null;
		}
		if (baseUri != null)
		{
			if (!baseUri.IsAbsoluteUri)
			{
				return GetResolvedUri(GetResolvedUri(null, baseUri), inputUri);
			}
			return GetResolvedUri(baseUri, inputUri);
		}
		Uri uri = null;
		if (element != null)
		{
			if (element is INavigator navigator)
			{
				uri = navigator.CurrentSource;
			}
			else
			{
				NavigationService navigationService = null;
				uri = ((!(element.GetValue(NavigationService.NavigationServiceProperty) is NavigationService navigationService2)) ? null : navigationService2.CurrentSource);
			}
		}
		if (uri != null)
		{
			if (uri.IsAbsoluteUri)
			{
				return GetResolvedUri(uri, inputUri);
			}
			return GetResolvedUri(GetResolvedUri(null, uri), inputUri);
		}
		return GetResolvedUri(null, inputUri);
	}

	internal static bool StartWithFragment(Uri uri)
	{
		return uri.OriginalString.StartsWith("#", StringComparison.Ordinal);
	}

	internal static string GetFragment(Uri uri)
	{
		Uri uri2 = uri;
		string result = string.Empty;
		if (!uri.IsAbsoluteUri)
		{
			uri2 = new Uri(placeboBase, uri);
		}
		string fragment = uri2.Fragment;
		if (fragment != null && fragment.Length > 0)
		{
			result = fragment.Substring(1);
		}
		return result;
	}

	internal static Uri GetUriRelativeToPackAppBase(Uri original)
	{
		if (original == null)
		{
			return null;
		}
		Uri resolvedUri = GetResolvedUri(original);
		return BaseUriHelper.PackAppBaseUri.MakeRelativeUri(resolvedUri);
	}

	internal static bool IsXamlMimeType(ContentType mimeType)
	{
		if (MimeTypeMapper.XamlMime.AreTypeAndSubTypeEqual(mimeType) || MimeTypeMapper.FixedDocumentSequenceMime.AreTypeAndSubTypeEqual(mimeType) || MimeTypeMapper.FixedDocumentMime.AreTypeAndSubTypeEqual(mimeType) || MimeTypeMapper.FixedPageMime.AreTypeAndSubTypeEqual(mimeType))
		{
			return true;
		}
		return false;
	}
}
