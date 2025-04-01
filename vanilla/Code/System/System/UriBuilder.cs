namespace System;

/// <summary>Provides a custom constructor for uniform resource identifiers (URIs) and modifies URIs for the <see cref="T:System.Uri" /> class.</summary>
/// <filterpriority>2</filterpriority>
public class UriBuilder
{
	private bool _changed = true;

	private string _fragment = string.Empty;

	private string _host = "localhost";

	private string _password = string.Empty;

	private string _path = "/";

	private int _port = -1;

	private string _query = string.Empty;

	private string _scheme = "http";

	private string _schemeDelimiter = Uri.SchemeDelimiter;

	private Uri _uri;

	private string _username = string.Empty;

	private string Extra
	{
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (value.Length > 0)
			{
				if (value[0] == '#')
				{
					Fragment = value.Substring(1);
					return;
				}
				if (value[0] != '?')
				{
					throw new ArgumentException("Extra portion of URI not valid.", "value");
				}
				int num = value.IndexOf('#');
				if (num == -1)
				{
					num = value.Length;
				}
				else
				{
					Fragment = value.Substring(num + 1);
				}
				Query = value.Substring(1, num - 1);
			}
			else
			{
				Fragment = string.Empty;
				Query = string.Empty;
			}
		}
	}

	/// <summary>Gets or sets the fragment portion of the URI.</summary>
	/// <returns>The fragment portion of the URI. The fragment identifier ("#") is added to the beginning of the fragment.</returns>
	/// <filterpriority>2</filterpriority>
	public string Fragment
	{
		get
		{
			return _fragment;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (value.Length > 0 && value[0] != '#')
			{
				value = "#" + value;
			}
			_fragment = value;
			_changed = true;
		}
	}

	/// <summary>Gets or sets the Domain Name System (DNS) host name or IP address of a server.</summary>
	/// <returns>The DNS host name or IP address of the server.</returns>
	/// <filterpriority>1</filterpriority>
	public string Host
	{
		get
		{
			return _host;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			_host = value;
			if (_host.IndexOf(':') >= 0 && _host[0] != '[')
			{
				_host = "[" + _host + "]";
			}
			_changed = true;
		}
	}

	/// <summary>Gets or sets the password associated with the user that accesses the URI.</summary>
	/// <returns>The password of the user that accesses the URI.</returns>
	/// <filterpriority>1</filterpriority>
	public string Password
	{
		get
		{
			return _password;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			_password = value;
			_changed = true;
		}
	}

	/// <summary>Gets or sets the path to the resource referenced by the URI.</summary>
	/// <returns>The path to the resource referenced by the URI.</returns>
	/// <filterpriority>1</filterpriority>
	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				value = "/";
			}
			_path = Uri.InternalEscapeString(value.Replace('\\', '/'));
			_changed = true;
		}
	}

	/// <summary>Gets or sets the port number of the URI.</summary>
	/// <returns>The port number of the URI.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The port cannot be set to a value less than -1 or greater than 65,535. </exception>
	/// <filterpriority>1</filterpriority>
	public int Port
	{
		get
		{
			return _port;
		}
		set
		{
			if (value < -1 || value > 65535)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_port = value;
			_changed = true;
		}
	}

	/// <summary>Gets or sets any query information included in the URI.</summary>
	/// <returns>The query information included in the URI.</returns>
	/// <filterpriority>1</filterpriority>
	public string Query
	{
		get
		{
			return _query;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (value.Length > 0 && value[0] != '?')
			{
				value = "?" + value;
			}
			_query = value;
			_changed = true;
		}
	}

	/// <summary>Gets or sets the scheme name of the URI.</summary>
	/// <returns>The scheme of the URI.</returns>
	/// <exception cref="T:System.ArgumentException">The scheme cannot be set to an invalid scheme name. </exception>
	/// <filterpriority>1</filterpriority>
	public string Scheme
	{
		get
		{
			return _scheme;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			int num = value.IndexOf(':');
			if (num != -1)
			{
				value = value.Substring(0, num);
			}
			if (value.Length != 0)
			{
				if (!Uri.CheckSchemeName(value))
				{
					throw new ArgumentException("Invalid URI: The URI scheme is not valid.", "value");
				}
				value = value.ToLowerInvariant();
			}
			_scheme = value;
			_changed = true;
		}
	}

	/// <summary>Gets the <see cref="T:System.Uri" /> instance constructed by the specified <see cref="T:System.UriBuilder" /> instance.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI constructed by the <see cref="T:System.UriBuilder" />.</returns>
	/// <exception cref="T:System.UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The URI constructed by the <see cref="T:System.UriBuilder" /> properties is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	public Uri Uri
	{
		get
		{
			if (_changed)
			{
				_uri = new Uri(ToString());
				SetFieldsFromUri(_uri);
				_changed = false;
			}
			return _uri;
		}
	}

	/// <summary>The user name associated with the user that accesses the URI.</summary>
	/// <returns>The user name of the user that accesses the URI.</returns>
	/// <filterpriority>1</filterpriority>
	public string UserName
	{
		get
		{
			return _username;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			_username = value;
			_changed = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class.</summary>
	public UriBuilder()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified URI.</summary>
	/// <param name="uri">A URI string. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="uri" /> is null. </exception>
	/// <exception cref="T:System.UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.<paramref name="uri" /> is a zero length string or contains only spaces.-or- The parsing routine detected a scheme in an invalid form.-or- The parser detected more than two consecutive slashes in a URI that does not use the "file" scheme.-or- <paramref name="uri" /> is not a valid URI. </exception>
	public UriBuilder(string uri)
	{
		Uri uri2 = new Uri(uri, UriKind.RelativeOrAbsolute);
		if (uri2.IsAbsoluteUri)
		{
			Init(uri2);
			return;
		}
		uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
		Init(new Uri(uri));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified <see cref="T:System.Uri" /> instance.</summary>
	/// <param name="uri">An instance of the <see cref="T:System.Uri" /> class. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="uri" /> is null. </exception>
	public UriBuilder(Uri uri)
	{
		if ((object)uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		Init(uri);
	}

	private void Init(Uri uri)
	{
		_fragment = uri.Fragment;
		_query = uri.Query;
		_host = uri.Host;
		_path = uri.AbsolutePath;
		_port = uri.Port;
		_scheme = uri.Scheme;
		_schemeDelimiter = (uri.HasAuthority ? Uri.SchemeDelimiter : ":");
		string userInfo = uri.UserInfo;
		if (!string.IsNullOrEmpty(userInfo))
		{
			int num = userInfo.IndexOf(':');
			if (num != -1)
			{
				_password = userInfo.Substring(num + 1);
				_username = userInfo.Substring(0, num);
			}
			else
			{
				_username = userInfo;
			}
		}
		SetFieldsFromUri(uri);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme and host.</summary>
	/// <param name="schemeName">An Internet access protocol. </param>
	/// <param name="hostName">A DNS-style domain name or IP address. </param>
	public UriBuilder(string schemeName, string hostName)
	{
		Scheme = schemeName;
		Host = hostName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme, host, and port.</summary>
	/// <param name="scheme">An Internet access protocol. </param>
	/// <param name="host">A DNS-style domain name or IP address. </param>
	/// <param name="portNumber">An IP port number for the service. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="portNumber" /> is less than -1 or greater than 65,535. </exception>
	public UriBuilder(string scheme, string host, int portNumber)
		: this(scheme, host)
	{
		Port = portNumber;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme, host, port number, and path.</summary>
	/// <param name="scheme">An Internet access protocol. </param>
	/// <param name="host">A DNS-style domain name or IP address. </param>
	/// <param name="port">An IP port number for the service. </param>
	/// <param name="pathValue">The path to the Internet resource. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than -1 or greater than 65,535. </exception>
	public UriBuilder(string scheme, string host, int port, string pathValue)
		: this(scheme, host, port)
	{
		Path = pathValue;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme, host, port number, path and query string or fragment identifier.</summary>
	/// <param name="scheme">An Internet access protocol. </param>
	/// <param name="host">A DNS-style domain name or IP address. </param>
	/// <param name="port">An IP port number for the service. </param>
	/// <param name="path">The path to the Internet resource. </param>
	/// <param name="extraValue">A query string or fragment identifier. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="extraValue" /> is neither null nor <see cref="F:System.String.Empty" />, nor does a valid fragment identifier begin with a number sign (#), nor a valid query string begin with a question mark (?). </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than -1 or greater than 65,535. </exception>
	public UriBuilder(string scheme, string host, int port, string path, string extraValue)
		: this(scheme, host, port, path)
	{
		try
		{
			Extra = extraValue;
		}
		catch (Exception ex)
		{
			if (ex is OutOfMemoryException)
			{
				throw;
			}
			throw new ArgumentException("Extra portion of URI not valid.", "extraValue");
		}
	}

	/// <summary>Compares an existing <see cref="T:System.Uri" /> instance with the contents of the <see cref="T:System.UriBuilder" /> for equality.</summary>
	/// <returns>true if <paramref name="rparam" /> represents the same <see cref="T:System.Uri" /> as the <see cref="T:System.Uri" /> constructed by this <see cref="T:System.UriBuilder" /> instance; otherwise, false.</returns>
	/// <param name="rparam">The object to compare with the current instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object rparam)
	{
		if (rparam == null)
		{
			return false;
		}
		return Uri.Equals(rparam.ToString());
	}

	/// <summary>Returns the hash code for the URI.</summary>
	/// <returns>The hash code generated for the URI.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return Uri.GetHashCode();
	}

	private void SetFieldsFromUri(Uri uri)
	{
		_fragment = uri.Fragment;
		_query = uri.Query;
		_host = uri.Host;
		_path = uri.AbsolutePath;
		_port = uri.Port;
		_scheme = uri.Scheme;
		_schemeDelimiter = (uri.HasAuthority ? Uri.SchemeDelimiter : ":");
		string userInfo = uri.UserInfo;
		if (userInfo.Length > 0)
		{
			int num = userInfo.IndexOf(':');
			if (num != -1)
			{
				_password = userInfo.Substring(num + 1);
				_username = userInfo.Substring(0, num);
			}
			else
			{
				_username = userInfo;
			}
		}
	}

	/// <summary>Returns the display string for the specified <see cref="T:System.UriBuilder" /> instance.</summary>
	/// <returns>The string that contains the unescaped display string of the <see cref="T:System.UriBuilder" />.</returns>
	/// <exception cref="T:System.UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The <see cref="T:System.UriBuilder" /> instance has a bad password. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (_username.Length == 0 && _password.Length > 0)
		{
			throw new UriFormatException("Invalid URI: The username:password construct is badly formed.");
		}
		if (_scheme.Length != 0)
		{
			UriParser syntax = UriParser.GetSyntax(_scheme);
			if (syntax != null)
			{
				_schemeDelimiter = ((syntax.InFact(UriSyntaxFlags.MustHaveAuthority) || (_host.Length != 0 && syntax.NotAny(UriSyntaxFlags.MailToLikeUri) && syntax.InFact(UriSyntaxFlags.OptionalAuthority))) ? Uri.SchemeDelimiter : ":");
			}
			else
			{
				_schemeDelimiter = ((_host.Length != 0) ? Uri.SchemeDelimiter : ":");
			}
		}
		string text = ((_scheme.Length != 0) ? (_scheme + _schemeDelimiter) : string.Empty);
		return text + _username + ((_password.Length > 0) ? (":" + _password) : string.Empty) + ((_username.Length > 0) ? "@" : string.Empty) + _host + ((_port != -1 && _host.Length > 0) ? (":" + _port) : string.Empty) + ((_host.Length > 0 && _path.Length != 0 && _path[0] != '/') ? "/" : string.Empty) + _path + _query + _fragment;
	}
}
