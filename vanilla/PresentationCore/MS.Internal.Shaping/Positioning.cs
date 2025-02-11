namespace MS.Internal.Shaping;

internal static class Positioning
{
	public static int DesignToPixels(ushort DesignUnitsPerEm, ushort PixelsPerEm, int Value)
	{
		if (DesignUnitsPerEm == 0)
		{
			return Value;
		}
		int num = DesignUnitsPerEm / 2;
		num = ((Value < 0) ? (-(DesignUnitsPerEm >> 1) + 1) : (DesignUnitsPerEm / 2));
		return (Value * PixelsPerEm + num) / DesignUnitsPerEm;
	}

	public unsafe static void AlignAnchors(IOpenTypeFont Font, FontTable Table, LayoutMetrics Metrics, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, int StaticGlyph, int MobileGlyph, AnchorTable StaticAnchor, AnchorTable MobileAnchor, bool UseAdvances)
	{
		Invariant.Assert(StaticGlyph >= 0 && StaticGlyph < GlyphInfo.Length);
		Invariant.Assert(MobileGlyph >= 0 && MobileGlyph < GlyphInfo.Length);
		Invariant.Assert(!StaticAnchor.IsNull());
		Invariant.Assert(!MobileAnchor.IsNull());
		LayoutOffset contourPoint = default(LayoutOffset);
		if (StaticAnchor.NeedContourPoint(Table))
		{
			contourPoint = Font.GetGlyphPointCoord(GlyphInfo.Glyphs[MobileGlyph], StaticAnchor.ContourPointIndex(Table));
		}
		LayoutOffset layoutOffset = StaticAnchor.AnchorCoordinates(Table, Metrics, contourPoint);
		if (MobileAnchor.NeedContourPoint(Table))
		{
			contourPoint = Font.GetGlyphPointCoord(GlyphInfo.Glyphs[MobileGlyph], MobileAnchor.ContourPointIndex(Table));
		}
		LayoutOffset layoutOffset2 = MobileAnchor.AnchorCoordinates(Table, Metrics, contourPoint);
		int num = 0;
		if (StaticGlyph < MobileGlyph)
		{
			for (int i = StaticGlyph + 1; i < MobileGlyph; i++)
			{
				num += Advances[i];
			}
		}
		else
		{
			for (int j = MobileGlyph + 1; j < StaticGlyph; j++)
			{
				num += Advances[j];
			}
		}
		if (Metrics.Direction != 0 && Metrics.Direction != TextFlowDirection.RTL)
		{
			return;
		}
		Offsets[MobileGlyph].dy = Offsets[StaticGlyph].dy + layoutOffset.dy - layoutOffset2.dy;
		if (Metrics.Direction == TextFlowDirection.LTR == StaticGlyph < MobileGlyph)
		{
			int num2 = Offsets[StaticGlyph].dx - Advances[StaticGlyph] + layoutOffset.dx - num - layoutOffset2.dx;
			if (UseAdvances)
			{
				Advances[StaticGlyph] += num2;
			}
			else
			{
				Offsets[MobileGlyph].dx = num2;
			}
		}
		else
		{
			int num3 = Offsets[StaticGlyph].dx + Advances[MobileGlyph] + layoutOffset.dx + num - layoutOffset2.dx;
			if (UseAdvances)
			{
				Advances[MobileGlyph] -= num3;
			}
			else
			{
				Offsets[MobileGlyph].dx = num3;
			}
		}
	}
}
