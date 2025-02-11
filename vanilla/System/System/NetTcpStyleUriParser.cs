namespace System;

/// <summary>A parser based on the NetTcp scheme for the "Indigo" system.</summary>
public class NetTcpStyleUriParser : UriParser
{
	/// <summary>Create a parser based on the NetTcp scheme for the "Indigo" system.</summary>
	public NetTcpStyleUriParser()
		: base(UriParser.NetTcpUri.Flags)
	{
	}
}
