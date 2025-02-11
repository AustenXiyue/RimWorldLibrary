namespace MS.Internal.Shaping;

internal struct DeviceTable
{
	private const int offsetStartSize = 0;

	private const int offsetEndSize = 2;

	private const int offsetDeltaFormat = 4;

	private const int offsetDeltaValueArray = 6;

	private const int sizeDeltaValue = 2;

	private int offset;

	private ushort StartSize(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private ushort EndSize(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	private ushort DeltaFormat(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	private ushort DeltaValue(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 6 + Index * 2);
	}

	public int Value(FontTable Table, ushort PixelsPerEm)
	{
		if (IsNull())
		{
			return 0;
		}
		ushort num = StartSize(Table);
		ushort num2 = EndSize(Table);
		if (PixelsPerEm < num || PixelsPerEm > num2)
		{
			return 0;
		}
		ushort num3 = (ushort)(PixelsPerEm - num);
		ushort index;
		ushort num4;
		ushort num5;
		switch (DeltaFormat(Table))
		{
		case 1:
			index = (ushort)(num3 >> 3);
			num4 = (ushort)(16 + 2 * (num3 & 7));
			num5 = 30;
			break;
		case 2:
			index = (ushort)(num3 >> 2);
			num4 = (ushort)(16 + 4 * (num3 & 3));
			num5 = 28;
			break;
		case 3:
			index = (ushort)(num3 >> 1);
			num4 = (ushort)(16 + 8 * (num3 & 1));
			num5 = 24;
			break;
		default:
			return 0;
		}
		return DeltaValue(Table, index) << (int)num4 >> (int)num5;
	}

	public DeviceTable(int Offset)
	{
		offset = Offset;
	}

	private bool IsNull()
	{
		return offset == 0;
	}
}
