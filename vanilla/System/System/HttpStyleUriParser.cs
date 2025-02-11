namespace System;

/// <summary>A customizable parser based on the HTTP scheme.</summary>
public class HttpStyleUriParser : UriParser
{
	/// <summary>Create a customizable parser based on the HTTP scheme.</summary>
	public HttpStyleUriParser()
		: base(UriParser.HttpUri.Flags)
	{
	}
}
