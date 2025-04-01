using System.Collections.Generic;
using System.Globalization;

namespace System;

/// <summary>Parses a new URI scheme. This is an abstract class.</summary>
public abstract class UriParser
{
	private enum UriQuirksVersion
	{
		V2 = 2,
		V3
	}

	private class BuiltInUriParser : UriParser
	{
		internal BuiltInUriParser(string lwrCaseScheme, int defaultPort, UriSyntaxFlags syntaxFlags)
			: base(syntaxFlags | UriSyntaxFlags.SimpleUserSyntax | UriSyntaxFlags.BuiltInSyntax)
		{
			m_Scheme = lwrCaseScheme;
			m_Port = defaultPort;
		}
	}

	private const UriSyntaxFlags SchemeOnlyFlags = UriSyntaxFlags.MayHavePath;

	private static readonly Dictionary<string, UriParser> m_Table;

	private static Dictionary<string, UriParser> m_TempTable;

	private UriSyntaxFlags m_Flags;

	private volatile UriSyntaxFlags m_UpdatableFlags;

	private volatile bool m_UpdatableFlagsUsed;

	private const UriSyntaxFlags c_UpdatableFlags = UriSyntaxFlags.UnEscapeDotsAndSlashes;

	private int m_Port;

	private string m_Scheme;

	internal const int NoDefaultPort = -1;

	private const int c_InitialTableSize = 25;

	internal static UriParser HttpUri;

	internal static UriParser HttpsUri;

	internal static UriParser WsUri;

	internal static UriParser WssUri;

	internal static UriParser FtpUri;

	internal static UriParser FileUri;

	internal static UriParser GopherUri;

	internal static UriParser NntpUri;

	internal static UriParser NewsUri;

	internal static UriParser MailToUri;

	internal static UriParser UuidUri;

	internal static UriParser TelnetUri;

	internal static UriParser LdapUri;

	internal static UriParser NetTcpUri;

	internal static UriParser NetPipeUri;

	internal static UriParser VsMacrosUri;

	private static readonly UriQuirksVersion s_QuirksVersion;

	private const int c_MaxCapacity = 512;

	private const UriSyntaxFlags UnknownV1SyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.OptionalAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.V1_UnknownUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private static readonly UriSyntaxFlags HttpSyntaxFlags;

	private const UriSyntaxFlags FtpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private static readonly UriSyntaxFlags FileSyntaxFlags;

	private const UriSyntaxFlags VsmacrosSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags GopherSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags NewsSyntaxFlags = UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags NntpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags TelnetSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags LdapSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags MailtoSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MailToLikeUri | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags NetPipeSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	private const UriSyntaxFlags NetTcpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

	internal string SchemeName => m_Scheme;

	internal int DefaultPort => m_Port;

	internal static bool ShouldUseLegacyV2Quirks => s_QuirksVersion <= UriQuirksVersion.V2;

	internal UriSyntaxFlags Flags => m_Flags;

	internal bool IsSimple => InFact(UriSyntaxFlags.SimpleUserSyntax);

	/// <summary>Constructs a default URI parser.</summary>
	protected UriParser()
		: this(UriSyntaxFlags.MayHavePath)
	{
	}

	/// <summary>Invoked by a <see cref="T:System.Uri" /> constructor to get a <see cref="T:System.UriParser" /> instance</summary>
	/// <returns>A <see cref="T:System.UriParser" /> for the constructed <see cref="T:System.Uri" />.</returns>
	protected virtual UriParser OnNewUri()
	{
		return this;
	}

	/// <summary>Invoked by the Framework when a <see cref="T:System.UriParser" /> method is registered.</summary>
	/// <param name="schemeName">The scheme that is associated with this <see cref="T:System.UriParser" />.</param>
	/// <param name="defaultPort">The port number of the scheme.</param>
	protected virtual void OnRegister(string schemeName, int defaultPort)
	{
	}

	/// <summary>Initialize the state of the parser and validate the URI.</summary>
	/// <param name="uri">The T:System.Uri to validate.</param>
	/// <param name="parsingError">Validation errors, if any.</param>
	protected virtual void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
	{
		parsingError = uri.ParseMinimal();
	}

	/// <summary>Called by <see cref="T:System.Uri" /> constructors and <see cref="Overload:System.Uri.TryCreate" /> to resolve a relative URI.</summary>
	/// <returns>The string of the resolved relative <see cref="T:System.Uri" />.</returns>
	/// <param name="baseUri">A base URI.</param>
	/// <param name="relativeUri">A relative URI.</param>
	/// <param name="parsingError">Errors during the resolve process, if any.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="baseUri" /> parameter is not an absolute <see cref="T:System.Uri" />- or -<paramref name="baseUri" /> parameter requires user-driven parsing.</exception>
	protected virtual string Resolve(Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
	{
		if (baseUri.UserDrivenParsing)
		{
			throw new InvalidOperationException(global::SR.GetString("A derived type '{0}' is responsible for parsing this Uri instance. The base implementation must not be used.", GetType().FullName));
		}
		if (!baseUri.IsAbsoluteUri)
		{
			throw new InvalidOperationException(global::SR.GetString("This operation is not supported for a relative URI."));
		}
		string newUriString = null;
		bool userEscaped = false;
		Uri uri = Uri.ResolveHelper(baseUri, relativeUri, ref newUriString, ref userEscaped, out parsingError);
		if (parsingError != null)
		{
			return null;
		}
		if (uri != null)
		{
			return uri.OriginalString;
		}
		return newUriString;
	}

	/// <summary>Determines whether <paramref name="baseUri" /> is a base URI for <paramref name="relativeUri" />.</summary>
	/// <returns>true if <paramref name="baseUri" /> is a base URI for <paramref name="relativeUri" />; otherwise, false.</returns>
	/// <param name="baseUri">The base URI.</param>
	/// <param name="relativeUri">The URI to test.</param>
	protected virtual bool IsBaseOf(Uri baseUri, Uri relativeUri)
	{
		return baseUri.IsBaseOfHelper(relativeUri);
	}

	/// <summary>Gets the components from a URI.</summary>
	/// <returns>A string that contains the components.</returns>
	/// <param name="uri">The URI to parse.</param>
	/// <param name="components">The <see cref="T:System.UriComponents" /> to retrieve from <paramref name="uri" />.</param>
	/// <param name="format">One of the <see cref="T:System.UriFormat" /> values that controls how special characters are escaped.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="uriFormat" /> is invalid.- or -<paramref name="uriComponents" /> is not a combination of valid <see cref="T:System.UriComponents" /> values. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="uri" /> requires user-driven parsing- or -<paramref name="uri" /> is not an absolute URI. Relative URIs cannot be used with this method.</exception>
	protected virtual string GetComponents(Uri uri, UriComponents components, UriFormat format)
	{
		if ((components & UriComponents.SerializationInfoString) != 0 && components != UriComponents.SerializationInfoString)
		{
			throw new ArgumentOutOfRangeException("components", components, global::SR.GetString("UriComponents.SerializationInfoString must not be combined with other UriComponents."));
		}
		if ((format & (UriFormat)(-4)) != 0)
		{
			throw new ArgumentOutOfRangeException("format");
		}
		if (uri.UserDrivenParsing)
		{
			throw new InvalidOperationException(global::SR.GetString("A derived type '{0}' is responsible for parsing this Uri instance. The base implementation must not be used.", GetType().FullName));
		}
		if (!uri.IsAbsoluteUri)
		{
			throw new InvalidOperationException(global::SR.GetString("This operation is not supported for a relative URI."));
		}
		return uri.GetComponentsHelper(components, format);
	}

	/// <summary>Indicates whether a URI is well-formed.</summary>
	/// <returns>true if <paramref name="uri" /> is well-formed; otherwise, false.</returns>
	/// <param name="uri">The URI to check.</param>
	protected virtual bool IsWellFormedOriginalString(Uri uri)
	{
		return uri.InternalIsWellFormedOriginalString();
	}

	/// <summary>Associates a scheme and port number with a <see cref="T:System.UriParser" />.</summary>
	/// <param name="uriParser">The URI parser to register.</param>
	/// <param name="schemeName">The name of the scheme that is associated with this parser.</param>
	/// <param name="defaultPort">The default port number for the specified scheme.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="uriParser" /> parameter is null- or -<paramref name="schemeName" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="schemeName" /> parameter is not valid- or -<paramref name="defaultPort" /> parameter is not valid. The <paramref name="defaultPort" /> parameter is less than -1 or greater than 65,534.</exception>
	public static void Register(UriParser uriParser, string schemeName, int defaultPort)
	{
		if (uriParser == null)
		{
			throw new ArgumentNullException("uriParser");
		}
		if (schemeName == null)
		{
			throw new ArgumentNullException("schemeName");
		}
		if (schemeName.Length == 1)
		{
			throw new ArgumentOutOfRangeException("schemeName");
		}
		if (!Uri.CheckSchemeName(schemeName))
		{
			throw new ArgumentOutOfRangeException("schemeName");
		}
		if ((defaultPort >= 65535 || defaultPort < 0) && defaultPort != -1)
		{
			throw new ArgumentOutOfRangeException("defaultPort");
		}
		schemeName = schemeName.ToLower(CultureInfo.InvariantCulture);
		FetchSyntax(uriParser, schemeName, defaultPort);
	}

	/// <summary>Indicates whether the parser for a scheme is registered.</summary>
	/// <returns>true if <paramref name="schemeName" /> has been registered; otherwise, false.</returns>
	/// <param name="schemeName">The scheme name to check.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemeName" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="schemeName" /> parameter is not valid. </exception>
	public static bool IsKnownScheme(string schemeName)
	{
		if (schemeName == null)
		{
			throw new ArgumentNullException("schemeName");
		}
		if (!Uri.CheckSchemeName(schemeName))
		{
			throw new ArgumentOutOfRangeException("schemeName");
		}
		return GetSyntax(schemeName.ToLower(CultureInfo.InvariantCulture))?.NotAny(UriSyntaxFlags.V1_UnknownUri) ?? false;
	}

	static UriParser()
	{
		s_QuirksVersion = UriQuirksVersion.V3;
		HttpSyntaxFlags = (UriSyntaxFlags)(0x1E00F7D | (ShouldUseLegacyV2Quirks ? 33554432 : 0) | 0x4000000 | 0x10000000);
		FileSyntaxFlags = (UriSyntaxFlags)(0xFD1 | ((!ShouldUseLegacyV2Quirks) ? 32 : 0) | 0x2000 | 0x200000 | 0x100000 | 0x400000 | 0x800000 | 0x1000000 | 0x2000000 | 0x4000000 | 0x10000000);
		m_Table = new Dictionary<string, UriParser>(25);
		m_TempTable = new Dictionary<string, UriParser>(25);
		HttpUri = new BuiltInUriParser("http", 80, HttpSyntaxFlags);
		m_Table[HttpUri.SchemeName] = HttpUri;
		HttpsUri = new BuiltInUriParser("https", 443, HttpUri.m_Flags);
		m_Table[HttpsUri.SchemeName] = HttpsUri;
		WsUri = new BuiltInUriParser("ws", 80, HttpSyntaxFlags);
		m_Table[WsUri.SchemeName] = WsUri;
		WssUri = new BuiltInUriParser("wss", 443, HttpSyntaxFlags);
		m_Table[WssUri.SchemeName] = WssUri;
		FtpUri = new BuiltInUriParser("ftp", 21, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[FtpUri.SchemeName] = FtpUri;
		FileUri = new BuiltInUriParser("file", -1, FileSyntaxFlags);
		m_Table[FileUri.SchemeName] = FileUri;
		GopherUri = new BuiltInUriParser("gopher", 70, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[GopherUri.SchemeName] = GopherUri;
		NntpUri = new BuiltInUriParser("nntp", 119, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[NntpUri.SchemeName] = NntpUri;
		NewsUri = new BuiltInUriParser("news", -1, UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowIriParsing);
		m_Table[NewsUri.SchemeName] = NewsUri;
		MailToUri = new BuiltInUriParser("mailto", 25, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MailToLikeUri | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[MailToUri.SchemeName] = MailToUri;
		UuidUri = new BuiltInUriParser("uuid", -1, NewsUri.m_Flags);
		m_Table[UuidUri.SchemeName] = UuidUri;
		TelnetUri = new BuiltInUriParser("telnet", 23, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[TelnetUri.SchemeName] = TelnetUri;
		LdapUri = new BuiltInUriParser("ldap", 389, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[LdapUri.SchemeName] = LdapUri;
		NetTcpUri = new BuiltInUriParser("net.tcp", 808, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[NetTcpUri.SchemeName] = NetTcpUri;
		NetPipeUri = new BuiltInUriParser("net.pipe", -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[NetPipeUri.SchemeName] = NetPipeUri;
		VsMacrosUri = new BuiltInUriParser("vsmacros", -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
		m_Table[VsMacrosUri.SchemeName] = VsMacrosUri;
	}

	internal bool NotAny(UriSyntaxFlags flags)
	{
		return IsFullMatch(flags, UriSyntaxFlags.None);
	}

	internal bool InFact(UriSyntaxFlags flags)
	{
		return !IsFullMatch(flags, UriSyntaxFlags.None);
	}

	internal bool IsAllSet(UriSyntaxFlags flags)
	{
		return IsFullMatch(flags, flags);
	}

	private bool IsFullMatch(UriSyntaxFlags flags, UriSyntaxFlags expected)
	{
		UriSyntaxFlags uriSyntaxFlags = (((flags & UriSyntaxFlags.UnEscapeDotsAndSlashes) != 0 && m_UpdatableFlagsUsed) ? ((m_Flags & ~UriSyntaxFlags.UnEscapeDotsAndSlashes) | m_UpdatableFlags) : m_Flags);
		return (uriSyntaxFlags & flags) == expected;
	}

	internal UriParser(UriSyntaxFlags flags)
	{
		m_Flags = flags;
		m_Scheme = string.Empty;
	}

	private static void FetchSyntax(UriParser syntax, string lwrCaseSchemeName, int defaultPort)
	{
		if (syntax.SchemeName.Length != 0)
		{
			throw new InvalidOperationException(global::SR.GetString("The URI parser instance passed into 'uriParser' parameter is already registered with the scheme name '{0}'.", syntax.SchemeName));
		}
		lock (m_Table)
		{
			syntax.m_Flags &= ~UriSyntaxFlags.V1_UnknownUri;
			UriParser value = null;
			m_Table.TryGetValue(lwrCaseSchemeName, out value);
			if (value != null)
			{
				throw new InvalidOperationException(global::SR.GetString("A URI scheme name '{0}' already has a registered custom parser.", value.SchemeName));
			}
			m_TempTable.TryGetValue(syntax.SchemeName, out value);
			if (value != null)
			{
				lwrCaseSchemeName = value.m_Scheme;
				m_TempTable.Remove(lwrCaseSchemeName);
			}
			syntax.OnRegister(lwrCaseSchemeName, defaultPort);
			syntax.m_Scheme = lwrCaseSchemeName;
			syntax.CheckSetIsSimpleFlag();
			syntax.m_Port = defaultPort;
			m_Table[syntax.SchemeName] = syntax;
		}
	}

	internal static UriParser FindOrFetchAsUnknownV1Syntax(string lwrCaseScheme)
	{
		UriParser value = null;
		m_Table.TryGetValue(lwrCaseScheme, out value);
		if (value != null)
		{
			return value;
		}
		m_TempTable.TryGetValue(lwrCaseScheme, out value);
		if (value != null)
		{
			return value;
		}
		lock (m_Table)
		{
			if (m_TempTable.Count >= 512)
			{
				m_TempTable = new Dictionary<string, UriParser>(25);
			}
			value = new BuiltInUriParser(lwrCaseScheme, -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.OptionalAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.V1_UnknownUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_TempTable[lwrCaseScheme] = value;
			return value;
		}
	}

	internal static UriParser GetSyntax(string lwrCaseScheme)
	{
		UriParser value = null;
		m_Table.TryGetValue(lwrCaseScheme, out value);
		if (value == null)
		{
			m_TempTable.TryGetValue(lwrCaseScheme, out value);
		}
		return value;
	}

	internal void CheckSetIsSimpleFlag()
	{
		Type type = GetType();
		if (type == typeof(GenericUriParser) || type == typeof(HttpStyleUriParser) || type == typeof(FtpStyleUriParser) || type == typeof(FileStyleUriParser) || type == typeof(NewsStyleUriParser) || type == typeof(GopherStyleUriParser) || type == typeof(NetPipeStyleUriParser) || type == typeof(NetTcpStyleUriParser) || type == typeof(LdapStyleUriParser))
		{
			m_Flags |= UriSyntaxFlags.SimpleUserSyntax;
		}
	}

	internal void SetUpdatableFlags(UriSyntaxFlags flags)
	{
		m_UpdatableFlags = flags;
		m_UpdatableFlagsUsed = true;
	}

	internal UriParser InternalOnNewUri()
	{
		UriParser uriParser = OnNewUri();
		if (this != uriParser)
		{
			uriParser.m_Scheme = m_Scheme;
			uriParser.m_Port = m_Port;
			uriParser.m_Flags = m_Flags;
		}
		return uriParser;
	}

	internal void InternalValidate(Uri thisUri, out UriFormatException parsingError)
	{
		InitializeAndValidate(thisUri, out parsingError);
	}

	internal string InternalResolve(Uri thisBaseUri, Uri uriLink, out UriFormatException parsingError)
	{
		return Resolve(thisBaseUri, uriLink, out parsingError);
	}

	internal bool InternalIsBaseOf(Uri thisBaseUri, Uri uriLink)
	{
		return IsBaseOf(thisBaseUri, uriLink);
	}

	internal string InternalGetComponents(Uri thisUri, UriComponents uriComponents, UriFormat uriFormat)
	{
		return GetComponents(thisUri, uriComponents, uriFormat);
	}

	internal bool InternalIsWellFormedOriginalString(Uri thisUri)
	{
		return IsWellFormedOriginalString(thisUri);
	}
}
