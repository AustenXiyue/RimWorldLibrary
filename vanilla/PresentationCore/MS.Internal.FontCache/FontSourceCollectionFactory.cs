using System;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

internal class FontSourceCollectionFactory : IFontSourceCollectionFactory
{
	public IFontSourceCollection Create(string uriString)
	{
		return new FontSourceCollection(new Uri(uriString), isWindowsFonts: false);
	}
}
