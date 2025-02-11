using System;
using MS.Internal.Shaping;

namespace MS.Internal.FontCache;

internal sealed class GsubGposTables : IOpenTypeFont
{
	private FontTable _gsubTable;

	private FontTable _gposTable;

	private FontFaceLayoutInfo _layout;

	internal GsubGposTables(FontFaceLayoutInfo layout)
	{
		_layout = layout;
		_gsubTable = new FontTable(_layout.Gsub());
		_gposTable = new FontTable(_layout.Gpos());
	}

	public FontTable GetFontTable(OpenTypeTags TableTag)
	{
		return TableTag switch
		{
			OpenTypeTags.GSUB => _gsubTable, 
			OpenTypeTags.GPOS => _gposTable, 
			_ => throw new NotSupportedException(), 
		};
	}

	public LayoutOffset GetGlyphPointCoord(ushort Glyph, ushort PointIndex)
	{
		throw new NotSupportedException();
	}

	public byte[] GetTableCache(OpenTypeTags tableTag)
	{
		return _layout.GetTableCache(tableTag);
	}

	public byte[] AllocateTableCache(OpenTypeTags tableTag, int size)
	{
		return _layout.AllocateTableCache(tableTag, size);
	}
}
