namespace MS.Internal.Shaping;

internal struct LayoutMetrics
{
	public TextFlowDirection Direction;

	public ushort DesignEmHeight;

	public ushort PixelsEmWidth;

	public ushort PixelsEmHeight;

	public LayoutMetrics(TextFlowDirection Direction, ushort DesignEmHeight, ushort PixelsEmWidth, ushort PixelsEmHeight)
	{
		this.Direction = Direction;
		this.DesignEmHeight = DesignEmHeight;
		this.PixelsEmWidth = PixelsEmWidth;
		this.PixelsEmHeight = PixelsEmHeight;
	}
}
