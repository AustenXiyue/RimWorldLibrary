namespace System;

/// <summary>A customizable parser based on the Gopher scheme.</summary>
public class GopherStyleUriParser : UriParser
{
	/// <summary>Creates a customizable parser based on the Gopher scheme.</summary>
	public GopherStyleUriParser()
		: base(UriParser.GopherUri.Flags)
	{
	}
}
