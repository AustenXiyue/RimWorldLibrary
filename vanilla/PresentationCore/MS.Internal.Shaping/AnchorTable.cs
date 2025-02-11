namespace MS.Internal.Shaping;

internal struct AnchorTable
{
	private const int offsetFormat = 0;

	private const int offsetXCoordinate = 2;

	private const int offsetYCoordinate = 4;

	private const int offsetFormat2AnchorPoint = 6;

	private const int offsetFormat3XDeviceTable = 6;

	private const int offsetFormat3YDeviceTable = 8;

	private int offset;

	private ushort format;

	private short XCoordinate(FontTable Table)
	{
		return Table.GetShort(offset + 2);
	}

	private short YCoordinate(FontTable Table)
	{
		return Table.GetShort(offset + 4);
	}

	private ushort Format2AnchorPoint(FontTable Table)
	{
		Invariant.Assert(format == 2);
		return Table.GetUShort(offset + 6);
	}

	private DeviceTable Format3XDeviceTable(FontTable Table)
	{
		Invariant.Assert(format == 3);
		int uShort = Table.GetUShort(offset + 6);
		if (uShort != 0)
		{
			return new DeviceTable(offset + uShort);
		}
		return new DeviceTable(0);
	}

	private DeviceTable Format3YDeviceTable(FontTable Table)
	{
		Invariant.Assert(format == 3);
		int uShort = Table.GetUShort(offset + 8);
		if (uShort != 0)
		{
			return new DeviceTable(offset + uShort);
		}
		return new DeviceTable(0);
	}

	public bool NeedContourPoint(FontTable Table)
	{
		return format == 2;
	}

	public ushort ContourPointIndex(FontTable Table)
	{
		Invariant.Assert(NeedContourPoint(Table));
		return Format2AnchorPoint(Table);
	}

	public LayoutOffset AnchorCoordinates(FontTable Table, LayoutMetrics Metrics, LayoutOffset ContourPoint)
	{
		LayoutOffset result = default(LayoutOffset);
		switch (format)
		{
		case 1:
			result.dx = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, XCoordinate(Table));
			result.dy = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmHeight, YCoordinate(Table));
			break;
		case 2:
			if (ContourPoint.dx == int.MinValue)
			{
				result.dx = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, XCoordinate(Table));
				result.dy = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmHeight, YCoordinate(Table));
			}
			else
			{
				result.dx = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, ContourPoint.dx);
				result.dy = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, ContourPoint.dy);
			}
			break;
		case 3:
			result.dx = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmWidth, XCoordinate(Table)) + Format3XDeviceTable(Table).Value(Table, Metrics.PixelsEmWidth);
			result.dy = Positioning.DesignToPixels(Metrics.DesignEmHeight, Metrics.PixelsEmHeight, YCoordinate(Table)) + Format3YDeviceTable(Table).Value(Table, Metrics.PixelsEmHeight);
			break;
		default:
			result.dx = 0;
			result.dx = 0;
			break;
		}
		return result;
	}

	public AnchorTable(FontTable Table, int Offset)
	{
		offset = Offset;
		if (offset != 0)
		{
			format = Table.GetUShort(offset);
		}
		else
		{
			format = 0;
		}
	}

	public bool IsNull()
	{
		return offset == 0;
	}
}
