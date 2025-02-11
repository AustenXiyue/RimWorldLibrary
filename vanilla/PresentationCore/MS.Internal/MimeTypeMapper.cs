using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MS.Internal.PresentationCore;
using MS.Win32.Compile;

namespace MS.Internal;

[FriendAccessAllowed]
internal static class MimeTypeMapper
{
	private static readonly Dictionary<string, ContentType> _fileExtensionToMimeType = new Dictionary<string, ContentType>(4);

	internal static readonly ContentType OctetMime = new ContentType("application/octet-stream");

	internal static readonly ContentType TextPlainMime = new ContentType("text/plain");

	internal const string XamlExtension = "xaml";

	internal const string BamlExtension = "baml";

	internal const string XbapExtension = "xbap";

	internal const string JpgExtension = "jpg";

	internal static readonly ContentType XamlMime = new ContentType("application/xaml+xml");

	internal static readonly ContentType BamlMime = new ContentType("application/baml+xml");

	internal static readonly ContentType JpgMime = new ContentType("image/jpg");

	internal static readonly ContentType IconMime = new ContentType("image/x-icon");

	internal static readonly ContentType FixedDocumentSequenceMime = new ContentType("application/vnd.ms-package.xps-fixeddocumentsequence+xml");

	internal static readonly ContentType FixedDocumentMime = new ContentType("application/vnd.ms-package.xps-fixeddocument+xml");

	internal static readonly ContentType FixedPageMime = new ContentType("application/vnd.ms-package.xps-fixedpage+xml");

	internal static readonly ContentType ResourceDictionaryMime = new ContentType("application/vnd.ms-package.xps-resourcedictionary+xml");

	internal static readonly ContentType HtmlMime = new ContentType("text/html");

	internal static readonly ContentType HtmMime = new ContentType("text/htm");

	internal static readonly ContentType XbapMime = new ContentType("application/x-ms-xbap");

	internal static ContentType GetMimeTypeFromUri(Uri uriSource)
	{
		ContentType value = ContentType.Empty;
		if (uriSource != null)
		{
			Uri uri = uriSource;
			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri("http://foo/bar/");
				uri = new Uri(uri, uriSource);
			}
			string fileExtension = GetFileExtension(uri);
			lock (((ICollection)_fileExtensionToMimeType).SyncRoot)
			{
				if (_fileExtensionToMimeType.Count == 0)
				{
					_fileExtensionToMimeType.Add("xaml", XamlMime);
					_fileExtensionToMimeType.Add("baml", BamlMime);
					_fileExtensionToMimeType.Add("jpg", JpgMime);
					_fileExtensionToMimeType.Add("xbap", XbapMime);
				}
				if (!_fileExtensionToMimeType.TryGetValue(fileExtension, out value))
				{
					value = GetMimeTypeFromUrlMon(uriSource);
					if (value != ContentType.Empty)
					{
						_fileExtensionToMimeType.Add(fileExtension, value);
					}
				}
			}
		}
		return value;
	}

	private static ContentType GetMimeTypeFromUrlMon(Uri uriSource)
	{
		ContentType result = ContentType.Empty;
		if (uriSource != null && UnsafeNativeMethods.FindMimeFromData(null, BindUriHelper.UriToString(uriSource), IntPtr.Zero, 0, null, 0, out var wzMimeOut, 0) == 0 && wzMimeOut != null)
		{
			result = new ContentType(wzMimeOut);
		}
		return result;
	}

	private static string GetDocument(Uri uri)
	{
		if (uri.IsFile)
		{
			return uri.LocalPath;
		}
		return uri.GetLeftPart(UriPartial.Path);
	}

	internal static string GetFileExtension(Uri uri)
	{
		string extension = Path.GetExtension(GetDocument(uri));
		string result = string.Empty;
		if (!string.IsNullOrEmpty(extension))
		{
			result = extension.Substring(1).ToLower(CultureInfo.InvariantCulture);
		}
		return result;
	}

	internal static bool IsHTMLMime(ContentType contentType)
	{
		if (!HtmlMime.AreTypeAndSubTypeEqual(contentType))
		{
			return HtmMime.AreTypeAndSubTypeEqual(contentType);
		}
		return true;
	}
}
