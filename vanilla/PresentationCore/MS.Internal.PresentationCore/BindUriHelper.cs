using System;
using System.Text;
using System.Windows.Navigation;
using MS.Internal.AppModel;

namespace MS.Internal.PresentationCore;

internal static class BindUriHelper
{
	private const int MAX_PATH_LENGTH = 2048;

	private const int MAX_SCHEME_LENGTH = 32;

	public const int MAX_URL_LENGTH = 2083;

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
}
