using System;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Net.Cache;

namespace MS.Internal.IO.Packaging;

internal class PseudoWebRequest : WebRequest
{
	private Uri _uri;

	private Uri _innerUri;

	private Uri _partName;

	private Package _cacheEntry;

	private string _connectionGroupName;

	private string _contentType;

	private int _contentLength;

	private string _method;

	private ICredentials _credentials;

	private WebHeaderCollection _headers;

	private bool _preAuthenticate;

	private IWebProxy _proxy;

	private int _timeout;

	private bool _useDefaultCredentials;

	public override RequestCachePolicy CachePolicy
	{
		get
		{
			Invariant.Assert(condition: false, "PackWebRequest must handle this method.");
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "PackWebRequest must handle this method.");
		}
	}

	public override string ConnectionGroupName
	{
		get
		{
			return _connectionGroupName;
		}
		set
		{
			_connectionGroupName = value;
		}
	}

	public override long ContentLength
	{
		get
		{
			return _contentLength;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override string ContentType
	{
		get
		{
			return _contentType;
		}
		set
		{
			_contentType = value;
		}
	}

	public override ICredentials Credentials
	{
		get
		{
			return _credentials;
		}
		set
		{
			_credentials = value;
		}
	}

	public override WebHeaderCollection Headers
	{
		get
		{
			if (_headers == null)
			{
				_headers = new WebHeaderCollection();
			}
			return _headers;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_headers = value;
		}
	}

	public override string Method
	{
		get
		{
			return _method;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_method = value;
		}
	}

	public override bool PreAuthenticate
	{
		get
		{
			return _preAuthenticate;
		}
		set
		{
			_preAuthenticate = value;
		}
	}

	public override IWebProxy Proxy
	{
		get
		{
			if (_proxy == null)
			{
				_proxy = WebRequest.DefaultWebProxy;
			}
			return _proxy;
		}
		set
		{
			_proxy = value;
		}
	}

	public override int Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_timeout = value;
		}
	}

	public override bool UseDefaultCredentials
	{
		get
		{
			if (IsScheme(Uri.UriSchemeFtp))
			{
				throw new NotSupportedException();
			}
			return _useDefaultCredentials;
		}
		set
		{
			if (IsScheme(Uri.UriSchemeFtp))
			{
				throw new NotSupportedException();
			}
			_useDefaultCredentials = value;
		}
	}

	internal PseudoWebRequest(Uri uri, Uri packageUri, Uri partUri, Package cacheEntry)
	{
		_uri = uri;
		_innerUri = packageUri;
		_partName = partUri;
		_cacheEntry = cacheEntry;
		SetDefaults();
	}

	public override Stream GetRequestStream()
	{
		throw new NotSupportedException();
	}

	public override WebResponse GetResponse()
	{
		Invariant.Assert(condition: false, "PackWebRequest must handle this method.");
		return null;
	}

	private bool IsScheme(string schemeName)
	{
		return string.Equals(_innerUri.Scheme, schemeName, StringComparison.Ordinal);
	}

	private void SetDefaults()
	{
		_connectionGroupName = string.Empty;
		_contentType = null;
		_credentials = null;
		_headers = null;
		_preAuthenticate = false;
		_proxy = null;
		if (IsScheme(Uri.UriSchemeHttp))
		{
			_timeout = 100000;
			_method = "GET";
		}
		else
		{
			_timeout = -1;
		}
		if (IsScheme(Uri.UriSchemeFtp))
		{
			_method = "RETR";
		}
		_useDefaultCredentials = false;
		_contentLength = -1;
	}
}
