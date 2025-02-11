using System;
using System.IO;
using System.IO.Packaging;
using System.Net;

namespace MS.Internal.AppModel;

internal class SiteOfOriginPart : PackagePart
{
	private Uri _absoluteLocation;

	private ContentType _contentType = MS.Internal.ContentType.Empty;

	private Stream _cacheStream;

	private object _globalLock = new object();

	internal SiteOfOriginPart(Package container, Uri uri)
		: base(container, uri)
	{
	}

	protected override Stream GetStreamCore(FileMode mode, FileAccess access)
	{
		return GetStreamAndSetContentType(onlyNeedContentType: false);
	}

	protected override string GetContentTypeCore()
	{
		GetStreamAndSetContentType(onlyNeedContentType: true);
		return _contentType.ToString();
	}

	private Stream GetStreamAndSetContentType(bool onlyNeedContentType)
	{
		lock (_globalLock)
		{
			if (onlyNeedContentType && _contentType != MS.Internal.ContentType.Empty)
			{
				return null;
			}
			if (_cacheStream != null)
			{
				Stream cacheStream = _cacheStream;
				_cacheStream = null;
				return cacheStream;
			}
			if (_absoluteLocation == null)
			{
				string text = base.Uri.ToString();
				Invariant.Assert(text[0] == '/');
				string relativeUri = text.Substring(1);
				_absoluteLocation = new Uri(SiteOfOriginContainer.SiteOfOrigin, relativeUri);
			}
			return (!SecurityHelper.AreStringTypesEqual(_absoluteLocation.Scheme, System.Uri.UriSchemeFile)) ? HandleWebSource(onlyNeedContentType) : HandleFileSource(onlyNeedContentType);
		}
	}

	private Stream HandleFileSource(bool onlyNeedContentType)
	{
		if (_contentType == MS.Internal.ContentType.Empty)
		{
			_contentType = MimeTypeMapper.GetMimeTypeFromUri(base.Uri);
		}
		if (!onlyNeedContentType)
		{
			return File.OpenRead(_absoluteLocation.LocalPath);
		}
		return null;
	}

	private Stream HandleWebSource(bool onlyNeedContentType)
	{
		WebResponse webResponse = WpfWebRequestHelper.CreateRequestAndGetResponse(_absoluteLocation);
		Stream responseStream = webResponse.GetResponseStream();
		if (_contentType == MS.Internal.ContentType.Empty)
		{
			_contentType = WpfWebRequestHelper.GetContentType(webResponse);
		}
		if (onlyNeedContentType)
		{
			_cacheStream = responseStream;
		}
		return responseStream;
	}
}
