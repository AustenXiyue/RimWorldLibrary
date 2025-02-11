namespace MS.Internal.Shaping;

internal struct ClassDefTable
{
	private const int offsetFormat = 0;

	private const int offsetFormat1StartGlyph = 2;

	private const int offsetFormat1GlyphCount = 4;

	private const int offsetFormat1ClassValueArray = 6;

	private const int sizeFormat1ClassValue = 2;

	private const int offsetFormat2RangeCount = 2;

	private const int offsetFormat2RangeRecordArray = 4;

	private const int sizeFormat2RangeRecord = 6;

	private const int offsetFormat2RangeRecordStart = 0;

	private const int offsetFormat2RangeRecordEnd = 2;

	private const int offsetFormat2RangeRecordClass = 4;

	private int offset;

	public static ClassDefTable InvalidClassDef => new ClassDefTable(-1);

	public bool IsInvalid => offset == -1;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private ushort Format1StartGlyph(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	private ushort Format1GlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	private ushort Format1ClassValue(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 6 + Index * 2);
	}

	private ushort Format2RangeCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	private ushort Format2RangeStartGlyph(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 6);
	}

	private ushort Format2RangeEndGlyph(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 6 + 2);
	}

	private ushort Format2RangeClassValue(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 6 + 4);
	}

	public ushort GetClass(FontTable Table, ushort glyph)
	{
		switch (Format(Table))
		{
		case 1:
		{
			ushort num4 = Format1StartGlyph(Table);
			ushort num5 = Format1GlyphCount(Table);
			if (glyph >= num4 && glyph - num4 < num5)
			{
				return Format1ClassValue(Table, (ushort)(glyph - num4));
			}
			return 0;
		}
		case 2:
		{
			ushort num = 0;
			ushort num2 = Format2RangeCount(Table);
			while (num < num2)
			{
				ushort num3 = (ushort)(num + num2 >> 1);
				if (glyph < Format2RangeStartGlyph(Table, num3))
				{
					num2 = num3;
					continue;
				}
				if (glyph > Format2RangeEndGlyph(Table, num3))
				{
					num = (ushort)(num3 + 1);
					continue;
				}
				return Format2RangeClassValue(Table, num3);
			}
			return 0;
		}
		default:
			return 0;
		}
	}

	public ClassDefTable(int Offset)
	{
		offset = Offset;
	}
}
