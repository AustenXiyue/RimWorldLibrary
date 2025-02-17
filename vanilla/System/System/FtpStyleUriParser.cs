namespace System;

/// <summary>A customizable parser based on the File Transfer Protocol (FTP) scheme.</summary>
public class FtpStyleUriParser : UriParser
{
	/// <summary>Creates a customizable parser based on the File Transfer Protocol (FTP) scheme.</summary>
	public FtpStyleUriParser()
		: base(UriParser.FtpUri.Flags)
	{
	}
}
