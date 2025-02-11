namespace MS.Internal.Shaping;

internal struct ValueRecordTable
{
	private const ushort XPlacmentFlag = 1;

	private const ushort YPlacmentFlag = 2;

	private const ushort XAdvanceFlag = 4;

	private const ushort YAdvanceFlag = 8;

	private const ushort XPlacementDeviceFlag = 16;

	private const ushort YPlacementDeviceFlag = 32;

	private const ushort XAdvanceDeviceFlag = 64;

	private const ushort YAdvanceDeviceFlag = 128;

	private static ushort[] BitCount = new ushort[16]
	{
		0, 2, 2, 4, 2, 4, 4, 6, 2, 4,
		4, 6, 4, 6, 6, 8
	};

	private ushort format;

	private int baseTableOffset;

	private int offset;

	public static ushort Size(ushort Format)
	{
		return (ushort)(BitCount[Format & 0xF] + BitCount[(Format >> 4) & 0xF]);
	}

	public void AdjustPos(FontTable Table, LayoutMetrics Metrics, ref LayoutOffset GlyphOffset, ref int GlyphAdvance)
	{
		int num = offset;
		if ((format & 1) != 0)
		{
			GlyphOffset.dx += Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, Table.GetShort(num));
			num += 2;
		}
		if ((format & 2) != 0)
		{
			GlyphOffset.dy += Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmHeight, Table.GetShort(num));
			num += 2;
		}
		if ((format & 4) != 0)
		{
			GlyphAdvance += Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, Table.GetShort(num));
			num += 2;
		}
		if ((format & 8) != 0)
		{
			GlyphAdvance += Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmHeight, Table.GetShort(num));
			num += 2;
		}
		if ((format & 0x10) != 0)
		{
			int num2 = Table.GetOffset(num);
			if (num2 != 0)
			{
				DeviceTable deviceTable = new DeviceTable(baseTableOffset + num2);
				GlyphOffset.dx += deviceTable.Value(Table, Metrics.PixelsEmWidth);
			}
			num += 2;
		}
		if ((format & 0x20) != 0)
		{
			int num3 = Table.GetOffset(num);
			if (num3 != 0)
			{
				DeviceTable deviceTable2 = new DeviceTable(baseTableOffset + num3);
				GlyphOffset.dy += deviceTable2.Value(Table, Metrics.PixelsEmHeight);
			}
			num += 2;
		}
		if ((format & 0x40) != 0)
		{
			if (Metrics.Direction == TextFlowDirection.LTR || Metrics.Direction == TextFlowDirection.RTL)
			{
				int num4 = Table.GetOffset(num);
				if (num4 != 0)
				{
					GlyphAdvance += new DeviceTable(baseTableOffset + num4).Value(Table, Metrics.PixelsEmWidth);
				}
			}
			num += 2;
		}
		if ((format & 0x80) == 0)
		{
			return;
		}
		if (Metrics.Direction == TextFlowDirection.TTB || Metrics.Direction == TextFlowDirection.BTT)
		{
			int num5 = Table.GetOffset(num);
			if (num5 != 0)
			{
				GlyphAdvance += new DeviceTable(baseTableOffset + num5).Value(Table, Metrics.PixelsEmHeight);
			}
		}
		num += 2;
	}

	public ValueRecordTable(int Offset, int BaseTableOffset, ushort Format)
	{
		offset = Offset;
		baseTableOffset = BaseTableOffset;
		format = Format;
	}
}
