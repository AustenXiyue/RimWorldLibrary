using System;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

internal class FontSourceFactory : IFontSourceFactory
{
	public IFontSource Create(string uriString)
	{
		return new FontSource(new Uri(uriString), skipDemand: false);
	}
}
