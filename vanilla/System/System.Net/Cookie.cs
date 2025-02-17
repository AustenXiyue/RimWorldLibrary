using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Net;

/// <summary>Provides a set of properties and methods that are used to manage cookies. This class cannot be inherited.</summary>
[Serializable]
public sealed class Cookie
{
	internal const int MaxSupportedVersion = 1;

	internal const string CommentAttributeName = "Comment";

	internal const string CommentUrlAttributeName = "CommentURL";

	internal const string DiscardAttributeName = "Discard";

	internal const string DomainAttributeName = "Domain";

	internal const string ExpiresAttributeName = "Expires";

	internal const string MaxAgeAttributeName = "Max-Age";

	internal const string PathAttributeName = "Path";

	internal const string PortAttributeName = "Port";

	internal const string SecureAttributeName = "Secure";

	internal const string VersionAttributeName = "Version";

	internal const string HttpOnlyAttributeName = "HttpOnly";

	internal const string SeparatorLiteral = "; ";

	internal const string EqualsLiteral = "=";

	internal const string QuotesLiteral = "\"";

	internal const string SpecialAttributeLiteral = "$";

	internal static readonly char[] PortSplitDelimiters = new char[3] { ' ', ',', '"' };

	internal static readonly char[] Reserved2Name = new char[7] { ' ', '\t', '\r', '\n', '=', ';', ',' };

	internal static readonly char[] Reserved2Value = new char[2] { ';', ',' };

	private static Comparer staticComparer = new Comparer();

	private string m_comment = string.Empty;

	private Uri m_commentUri;

	private CookieVariant m_cookieVariant = CookieVariant.Plain;

	private bool m_discard;

	private string m_domain = string.Empty;

	private bool m_domain_implicit = true;

	private DateTime m_expires = DateTime.MinValue;

	private string m_name = string.Empty;

	private string m_path = string.Empty;

	private bool m_path_implicit = true;

	private string m_port = string.Empty;

	private bool m_port_implicit = true;

	private int[] m_port_list;

	private bool m_secure;

	[OptionalField]
	private bool m_httpOnly;

	private DateTime m_timeStamp = DateTime.Now;

	private string m_value = string.Empty;

	private int m_version;

	private string m_domainKey = string.Empty;

	internal bool IsQuotedVersion;

	internal bool IsQuotedDomain;

	/// <summary>Gets or sets a comment that the server can add to a <see cref="T:System.Net.Cookie" />.</summary>
	/// <returns>An optional comment to document intended usage for this <see cref="T:System.Net.Cookie" />.</returns>
	public string Comment
	{
		get
		{
			return m_comment;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			m_comment = value;
		}
	}

	/// <summary>Gets or sets a URI comment that the server can provide with a <see cref="T:System.Net.Cookie" />.</summary>
	/// <returns>An optional comment that represents the intended usage of the URI reference for this <see cref="T:System.Net.Cookie" />. The value must conform to URI format.</returns>
	public Uri CommentUri
	{
		get
		{
			return m_commentUri;
		}
		set
		{
			m_commentUri = value;
		}
	}

	/// <summary>Determines whether a page script or other active content can access this cookie.</summary>
	/// <returns>Boolean value that determines whether a page script or other active content can access this cookie.</returns>
	public bool HttpOnly
	{
		get
		{
			return m_httpOnly;
		}
		set
		{
			m_httpOnly = value;
		}
	}

	/// <summary>Gets or sets the discard flag set by the server.</summary>
	/// <returns>true if the client is to discard the <see cref="T:System.Net.Cookie" /> at the end of the current session; otherwise, false. The default is false.</returns>
	public bool Discard
	{
		get
		{
			return m_discard;
		}
		set
		{
			m_discard = value;
		}
	}

	/// <summary>Gets or sets the URI for which the <see cref="T:System.Net.Cookie" /> is valid.</summary>
	/// <returns>The URI for which the <see cref="T:System.Net.Cookie" /> is valid.</returns>
	public string Domain
	{
		get
		{
			return m_domain;
		}
		set
		{
			m_domain = ((value == null) ? string.Empty : value);
			m_domain_implicit = false;
			m_domainKey = string.Empty;
		}
	}

	private string _Domain
	{
		get
		{
			if (!Plain && !m_domain_implicit && m_domain.Length != 0)
			{
				return "$Domain=" + (IsQuotedDomain ? "\"" : string.Empty) + m_domain + (IsQuotedDomain ? "\"" : string.Empty);
			}
			return string.Empty;
		}
	}

	internal bool DomainImplicit
	{
		get
		{
			return m_domain_implicit;
		}
		set
		{
			m_domain_implicit = value;
		}
	}

	/// <summary>Gets or sets the current state of the <see cref="T:System.Net.Cookie" />.</summary>
	/// <returns>true if the <see cref="T:System.Net.Cookie" /> has expired; otherwise, false. The default is false.</returns>
	public bool Expired
	{
		get
		{
			if (m_expires != DateTime.MinValue)
			{
				return m_expires.ToLocalTime() <= DateTime.Now;
			}
			return false;
		}
		set
		{
			if (value)
			{
				m_expires = DateTime.Now;
			}
		}
	}

	/// <summary>Gets or sets the expiration date and time for the <see cref="T:System.Net.Cookie" /> as a <see cref="T:System.DateTime" />.</summary>
	/// <returns>The expiration date and time for the <see cref="T:System.Net.Cookie" /> as a <see cref="T:System.DateTime" /> instance.</returns>
	public DateTime Expires
	{
		get
		{
			return m_expires;
		}
		set
		{
			m_expires = value;
		}
	}

	/// <summary>Gets or sets the name for the <see cref="T:System.Net.Cookie" />.</summary>
	/// <returns>The name for the <see cref="T:System.Net.Cookie" />.</returns>
	/// <exception cref="T:System.Net.CookieException">The value specified for a set operation is null or the empty string- or -The value specified for a set operation contained an illegal character. The following characters must not be used inside the <see cref="P:System.Net.Cookie.Name" /> property: equal sign, semicolon, comma, newline (\n), return (\r), tab (\t), and space character. The dollar sign character ("$") cannot be the first character.</exception>
	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (ValidationHelper.IsBlankString(value) || !InternalSetName(value))
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Name", (value == null) ? "<null>" : value));
			}
		}
	}

	/// <summary>Gets or sets the URIs to which the <see cref="T:System.Net.Cookie" /> applies.</summary>
	/// <returns>The URIs to which the <see cref="T:System.Net.Cookie" /> applies.</returns>
	public string Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = ((value == null) ? string.Empty : value);
			m_path_implicit = false;
		}
	}

	private string _Path
	{
		get
		{
			if (!Plain && !m_path_implicit && m_path.Length != 0)
			{
				return "$Path=" + m_path;
			}
			return string.Empty;
		}
	}

	internal bool Plain => Variant == CookieVariant.Plain;

	/// <summary>Gets or sets a list of TCP ports that the <see cref="T:System.Net.Cookie" /> applies to.</summary>
	/// <returns>The list of TCP ports that the <see cref="T:System.Net.Cookie" /> applies to.</returns>
	/// <exception cref="T:System.Net.CookieException">The value specified for a set operation could not be parsed or is not enclosed in double quotes. </exception>
	public string Port
	{
		get
		{
			return m_port;
		}
		set
		{
			m_port_implicit = false;
			if (value == null || value.Length == 0)
			{
				m_port = string.Empty;
				return;
			}
			if (value[0] != '"' || value[value.Length - 1] != '"')
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Port", value));
			}
			string[] array = value.Split(PortSplitDelimiters);
			List<int> list = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != string.Empty)
				{
					if (!int.TryParse(array[i], out var result))
					{
						throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Port", value));
					}
					if (result < 0 || result > 65535)
					{
						throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Port", value));
					}
					list.Add(result);
				}
			}
			m_port_list = list.ToArray();
			m_port = value;
			m_version = 1;
			m_cookieVariant = CookieVariant.Rfc2965;
		}
	}

	internal int[] PortList => m_port_list;

	private string _Port
	{
		get
		{
			if (!m_port_implicit)
			{
				return "$Port" + ((m_port.Length == 0) ? string.Empty : ("=" + m_port));
			}
			return string.Empty;
		}
	}

	/// <summary>Gets or sets the security level of a <see cref="T:System.Net.Cookie" />.</summary>
	/// <returns>true if the client is only to return the cookie in subsequent requests if those requests use Secure Hypertext Transfer Protocol (HTTPS); otherwise, false. The default is false.</returns>
	public bool Secure
	{
		get
		{
			return m_secure;
		}
		set
		{
			m_secure = value;
		}
	}

	/// <summary>Gets the time when the cookie was issued as a <see cref="T:System.DateTime" />.</summary>
	/// <returns>The time when the cookie was issued as a <see cref="T:System.DateTime" />.</returns>
	public DateTime TimeStamp => m_timeStamp;

	/// <summary>Gets or sets the <see cref="P:System.Net.Cookie.Value" /> for the <see cref="T:System.Net.Cookie" />.</summary>
	/// <returns>The <see cref="P:System.Net.Cookie.Value" /> for the <see cref="T:System.Net.Cookie" />.</returns>
	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = ((value == null) ? string.Empty : value);
		}
	}

	internal CookieVariant Variant
	{
		get
		{
			return m_cookieVariant;
		}
		set
		{
			m_cookieVariant = value;
		}
	}

	internal string DomainKey
	{
		get
		{
			if (!m_domain_implicit)
			{
				return m_domainKey;
			}
			return Domain;
		}
	}

	/// <summary>Gets or sets the version of HTTP state maintenance to which the cookie conforms.</summary>
	/// <returns>The version of HTTP state maintenance to which the cookie conforms.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a version is not allowed. </exception>
	public int Version
	{
		get
		{
			return m_version;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			m_version = value;
			if (value > 0 && m_cookieVariant < CookieVariant.Rfc2109)
			{
				m_cookieVariant = CookieVariant.Rfc2109;
			}
		}
	}

	private string _Version
	{
		get
		{
			if (Version != 0)
			{
				return "$Version=" + (IsQuotedVersion ? "\"" : string.Empty) + m_version.ToString(NumberFormatInfo.InvariantInfo) + (IsQuotedVersion ? "\"" : string.Empty);
			}
			return string.Empty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cookie" /> class.</summary>
	public Cookie()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cookie" /> class with a specified <see cref="P:System.Net.Cookie.Name" /> and <see cref="P:System.Net.Cookie.Value" />.</summary>
	/// <param name="name">The name of a <see cref="T:System.Net.Cookie" />. The following characters must not be used inside <paramref name="name" />: equal sign, semicolon, comma, newline (\n), return (\r), tab (\t), and space character. The dollar sign character ("$") cannot be the first character. </param>
	/// <param name="value">The value of a <see cref="T:System.Net.Cookie" />. The following characters must not be used inside <paramref name="value" />: semicolon, comma. </param>
	/// <exception cref="T:System.Net.CookieException">The <paramref name="name" /> parameter is null. -or- The <paramref name="name" /> parameter is of zero length. -or- The <paramref name="name" /> parameter contains an invalid character.-or- The <paramref name="value" /> parameter is null .-or - The <paramref name="value" /> parameter contains a string not enclosed in quotes that contains an invalid character. </exception>
	public Cookie(string name, string value)
	{
		Name = name;
		m_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cookie" /> class with a specified <see cref="P:System.Net.Cookie.Name" />, <see cref="P:System.Net.Cookie.Value" />, and <see cref="P:System.Net.Cookie.Path" />.</summary>
	/// <param name="name">The name of a <see cref="T:System.Net.Cookie" />. The following characters must not be used inside <paramref name="name" />: equal sign, semicolon, comma, newline (\n), return (\r), tab (\t), and space character. The dollar sign character ("$") cannot be the first character. </param>
	/// <param name="value">The value of a <see cref="T:System.Net.Cookie" />. The following characters must not be used inside <paramref name="value" />: semicolon, comma. </param>
	/// <param name="path">The subset of URIs on the origin server to which this <see cref="T:System.Net.Cookie" /> applies. The default value is "/". </param>
	/// <exception cref="T:System.Net.CookieException">The <paramref name="name" /> parameter is null. -or- The <paramref name="name" /> parameter is of zero length. -or- The <paramref name="name" /> parameter contains an invalid character.-or- The <paramref name="value" /> parameter is null .-or - The <paramref name="value" /> parameter contains a string not enclosed in quotes that contains an invalid character.</exception>
	public Cookie(string name, string value, string path)
		: this(name, value)
	{
		Path = path;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cookie" /> class with a specified <see cref="P:System.Net.Cookie.Name" />, <see cref="P:System.Net.Cookie.Value" />, <see cref="P:System.Net.Cookie.Path" />, and <see cref="P:System.Net.Cookie.Domain" />.</summary>
	/// <param name="name">The name of a <see cref="T:System.Net.Cookie" />. The following characters must not be used inside <paramref name="name" />: equal sign, semicolon, comma, newline (\n), return (\r), tab (\t), and space character. The dollar sign character ("$") cannot be the first character. </param>
	/// <param name="value">The value of a <see cref="T:System.Net.Cookie" /> object. The following characters must not be used inside <paramref name="value" />: semicolon, comma. </param>
	/// <param name="path">The subset of URIs on the origin server to which this <see cref="T:System.Net.Cookie" /> applies. The default value is "/". </param>
	/// <param name="domain">The optional internet domain for which this <see cref="T:System.Net.Cookie" /> is valid. The default value is the host this <see cref="T:System.Net.Cookie" /> has been received from. </param>
	/// <exception cref="T:System.Net.CookieException">The <paramref name="name" /> parameter is null. -or- The <paramref name="name" /> parameter is of zero length. -or- The <paramref name="name" /> parameter contains an invalid character.-or- The <paramref name="value" /> parameter is null .-or - The <paramref name="value" /> parameter contains a string not enclosed in quotes that contains an invalid character.</exception>
	public Cookie(string name, string value, string path, string domain)
		: this(name, value, path)
	{
		Domain = domain;
	}

	internal bool InternalSetName(string value)
	{
		if (ValidationHelper.IsBlankString(value) || value[0] == '$' || value.IndexOfAny(Reserved2Name) != -1)
		{
			m_name = string.Empty;
			return false;
		}
		m_name = value;
		return true;
	}

	internal Cookie Clone()
	{
		Cookie cookie = new Cookie(m_name, m_value);
		if (!m_port_implicit)
		{
			cookie.Port = m_port;
		}
		if (!m_path_implicit)
		{
			cookie.Path = m_path;
		}
		cookie.Domain = m_domain;
		cookie.DomainImplicit = m_domain_implicit;
		cookie.m_timeStamp = m_timeStamp;
		cookie.Comment = m_comment;
		cookie.CommentUri = m_commentUri;
		cookie.HttpOnly = m_httpOnly;
		cookie.Discard = m_discard;
		cookie.Expires = m_expires;
		cookie.Version = m_version;
		cookie.Secure = m_secure;
		cookie.m_cookieVariant = m_cookieVariant;
		return cookie;
	}

	private static bool IsDomainEqualToHost(string domain, string host)
	{
		if (host.Length + 1 == domain.Length && string.Compare(host, 0, domain, 1, host.Length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		return false;
	}

	internal bool VerifySetDefaults(CookieVariant variant, Uri uri, bool isLocalDomain, string localDomain, bool set_default, bool isThrow)
	{
		string host = uri.Host;
		int port = uri.Port;
		string absolutePath = uri.AbsolutePath;
		bool flag = true;
		if (set_default)
		{
			if (Version == 0)
			{
				variant = CookieVariant.Plain;
			}
			else if (Version == 1 && variant == CookieVariant.Unknown)
			{
				variant = CookieVariant.Rfc2109;
			}
			m_cookieVariant = variant;
		}
		if (m_name == null || m_name.Length == 0 || m_name[0] == '$' || m_name.IndexOfAny(Reserved2Name) != -1)
		{
			if (isThrow)
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Name", (m_name == null) ? "<null>" : m_name));
			}
			return false;
		}
		if (m_value == null || ((m_value.Length <= 2 || m_value[0] != '"' || m_value[m_value.Length - 1] != '"') && m_value.IndexOfAny(Reserved2Value) != -1))
		{
			if (isThrow)
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Value", (m_value == null) ? "<null>" : m_value));
			}
			return false;
		}
		if (Comment != null && (Comment.Length <= 2 || Comment[0] != '"' || Comment[Comment.Length - 1] != '"') && Comment.IndexOfAny(Reserved2Value) != -1)
		{
			if (isThrow)
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Comment", Comment));
			}
			return false;
		}
		if (Path != null && (Path.Length <= 2 || Path[0] != '"' || Path[Path.Length - 1] != '"') && Path.IndexOfAny(Reserved2Value) != -1)
		{
			if (isThrow)
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Path", Path));
			}
			return false;
		}
		if (set_default && m_domain_implicit)
		{
			m_domain = host;
		}
		else
		{
			if (!m_domain_implicit)
			{
				string text = m_domain;
				if (!DomainCharsTest(text))
				{
					if (isThrow)
					{
						throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Domain", (text == null) ? "<null>" : text));
					}
					return false;
				}
				if (text[0] != '.')
				{
					if (variant != CookieVariant.Rfc2965 && variant != CookieVariant.Plain)
					{
						if (isThrow)
						{
							throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Domain", m_domain));
						}
						return false;
					}
					text = "." + text;
				}
				int num = host.IndexOf('.');
				if (isLocalDomain && string.Compare(localDomain, text, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
				}
				else if (text.IndexOf('.', 1, text.Length - 2) == -1)
				{
					if (!IsDomainEqualToHost(text, host))
					{
						flag = false;
					}
				}
				else if (variant == CookieVariant.Plain)
				{
					if (!IsDomainEqualToHost(text, host) && (host.Length <= text.Length || string.Compare(host, host.Length - text.Length, text, 0, text.Length, StringComparison.OrdinalIgnoreCase) != 0))
					{
						flag = false;
					}
				}
				else if ((num == -1 || text.Length != host.Length - num || string.Compare(host, num, text, 0, text.Length, StringComparison.OrdinalIgnoreCase) != 0) && !IsDomainEqualToHost(text, host))
				{
					flag = false;
				}
				if (flag)
				{
					m_domainKey = text.ToLower(CultureInfo.InvariantCulture);
				}
			}
			else if (string.Compare(host, m_domain, StringComparison.OrdinalIgnoreCase) != 0)
			{
				flag = false;
			}
			if (!flag)
			{
				if (isThrow)
				{
					throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Domain", m_domain));
				}
				return false;
			}
		}
		if (set_default && m_path_implicit)
		{
			switch (m_cookieVariant)
			{
			case CookieVariant.Plain:
				m_path = absolutePath;
				break;
			case CookieVariant.Rfc2109:
				m_path = absolutePath.Substring(0, absolutePath.LastIndexOf('/'));
				break;
			default:
				m_path = absolutePath.Substring(0, absolutePath.LastIndexOf('/') + 1);
				break;
			}
		}
		else if (!absolutePath.StartsWith(CookieParser.CheckQuoted(m_path)))
		{
			if (isThrow)
			{
				throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Path", m_path));
			}
			return false;
		}
		if (set_default && !m_port_implicit && m_port.Length == 0)
		{
			m_port_list = new int[1] { port };
		}
		if (!m_port_implicit)
		{
			flag = false;
			int[] port_list = m_port_list;
			for (int i = 0; i < port_list.Length; i++)
			{
				if (port_list[i] == port)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (isThrow)
				{
					throw new CookieException(global::SR.GetString("The '{0}'='{1}' part of the cookie is invalid.", "Port", m_port));
				}
				return false;
			}
		}
		return true;
	}

	private static bool DomainCharsTest(string name)
	{
		if (name == null || name.Length == 0)
		{
			return false;
		}
		foreach (char c in name)
		{
			if (c >= '0' && c <= '9')
			{
				continue;
			}
			switch (c)
			{
			case '-':
			case '.':
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				continue;
			}
			if ((c < 'A' || c > 'Z') && c != '_')
			{
				return false;
			}
		}
		return true;
	}

	internal static IComparer GetComparer()
	{
		return staticComparer;
	}

	/// <summary>Overrides the <see cref="M:System.Object.Equals(System.Object)" /> method.</summary>
	/// <returns>Returns true if the <see cref="T:System.Net.Cookie" /> is equal to <paramref name="comparand" />. Two <see cref="T:System.Net.Cookie" /> instances are equal if their <see cref="P:System.Net.Cookie.Name" />, <see cref="P:System.Net.Cookie.Value" />, <see cref="P:System.Net.Cookie.Path" />, <see cref="P:System.Net.Cookie.Domain" />, and <see cref="P:System.Net.Cookie.Version" /> properties are equal. <see cref="P:System.Net.Cookie.Name" /> and <see cref="P:System.Net.Cookie.Domain" /> string comparisons are case-insensitive.</returns>
	/// <param name="comparand">A reference to a <see cref="T:System.Net.Cookie" />. </param>
	public override bool Equals(object comparand)
	{
		if (!(comparand is Cookie))
		{
			return false;
		}
		Cookie cookie = (Cookie)comparand;
		if (string.Compare(Name, cookie.Name, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(Value, cookie.Value, StringComparison.Ordinal) == 0 && string.Compare(Path, cookie.Path, StringComparison.Ordinal) == 0 && string.Compare(Domain, cookie.Domain, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return Version == cookie.Version;
		}
		return false;
	}

	/// <summary>Overrides the <see cref="M:System.Object.GetHashCode" /> method.</summary>
	/// <returns>The 32-bit signed integer hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return (Name + "=" + Value + ";" + Path + "; " + Domain + "; " + Version).GetHashCode();
	}

	/// <summary>Overrides the <see cref="M:System.Object.ToString" /> method.</summary>
	/// <returns>Returns a string representation of this <see cref="T:System.Net.Cookie" /> object that is suitable for including in a HTTP Cookie: request header.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		string domain = _Domain;
		string path = _Path;
		string port = _Port;
		string version = _Version;
		string text = ((version.Length == 0) ? string.Empty : (version + "; ")) + Name + "=" + Value + ((path.Length == 0) ? string.Empty : ("; " + path)) + ((domain.Length == 0) ? string.Empty : ("; " + domain)) + ((port.Length == 0) ? string.Empty : ("; " + port));
		if (text == "=")
		{
			return string.Empty;
		}
		return text;
	}

	internal string ToServerString()
	{
		string text = Name + "=" + Value;
		if (m_comment != null && m_comment.Length > 0)
		{
			text = text + "; Comment=" + m_comment;
		}
		if (m_commentUri != null)
		{
			text = text + "; CommentURL=\"" + m_commentUri.ToString() + "\"";
		}
		if (m_discard)
		{
			text += "; Discard";
		}
		if (!m_domain_implicit && m_domain != null && m_domain.Length > 0)
		{
			text = text + "; Domain=" + m_domain;
		}
		if (Expires != DateTime.MinValue)
		{
			int num = (int)(Expires.ToLocalTime() - DateTime.Now).TotalSeconds;
			if (num < 0)
			{
				num = 0;
			}
			text = text + "; Max-Age=" + num.ToString(NumberFormatInfo.InvariantInfo);
		}
		if (!m_path_implicit && m_path != null && m_path.Length > 0)
		{
			text = text + "; Path=" + m_path;
		}
		if (!Plain && !m_port_implicit && m_port != null && m_port.Length > 0)
		{
			text = text + "; Port=" + m_port;
		}
		if (m_version > 0)
		{
			text = text + "; Version=" + m_version.ToString(NumberFormatInfo.InvariantInfo);
		}
		if (!(text == "="))
		{
			return text;
		}
		return null;
	}
}
