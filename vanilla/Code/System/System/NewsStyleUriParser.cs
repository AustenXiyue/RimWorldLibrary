namespace System;

/// <summary>A customizable parser based on the news scheme using the Network News Transfer Protocol (NNTP).</summary>
public class NewsStyleUriParser : UriParser
{
	/// <summary>Create a customizable parser based on the news scheme using the Network News Transfer Protocol (NNTP).</summary>
	public NewsStyleUriParser()
		: base(UriParser.NewsUri.Flags)
	{
	}
}
