using System;
using System.Globalization;

namespace MS.Internal;

internal static class ResourceIDHelper
{
	internal static string GetResourceIDFromRelativePath(string relPath)
	{
		Uri baseUri = new Uri("http://foo/");
		Uri sourceUri = new Uri(baseUri, relPath.Replace("#", "%23"));
		return GetResourceIDFromUri(baseUri, sourceUri);
	}

	private static string GetResourceIDFromUri(Uri baseUri, Uri sourceUri)
	{
		string result = string.Empty;
		if (!baseUri.IsAbsoluteUri || !sourceUri.IsAbsoluteUri)
		{
			return result;
		}
		if (baseUri.Scheme == sourceUri.Scheme && baseUri.Host == sourceUri.Host)
		{
			string components = baseUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
			string components2 = sourceUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
			components = components.ToLower(CultureInfo.InvariantCulture);
			components2 = components2.ToLower(CultureInfo.InvariantCulture);
			if (components2.StartsWith(components, StringComparison.OrdinalIgnoreCase))
			{
				result = components2.Substring(components.Length);
			}
		}
		return result;
	}
}
