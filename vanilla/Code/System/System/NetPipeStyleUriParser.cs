namespace System;

/// <summary>A parser based on the NetPipe scheme for the "Indigo" system.</summary>
public class NetPipeStyleUriParser : UriParser
{
	/// <summary>Create a parser based on the NetPipe scheme for the "Indigo" system.</summary>
	public NetPipeStyleUriParser()
		: base(UriParser.NetPipeUri.Flags)
	{
	}
}
