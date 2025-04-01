namespace System;

/// <summary>A customizable parser based on the File scheme.</summary>
public class FileStyleUriParser : UriParser
{
	/// <summary>Creates a customizable parser based on the File scheme.</summary>
	public FileStyleUriParser()
		: base(UriParser.FileUri.Flags)
	{
	}
}
