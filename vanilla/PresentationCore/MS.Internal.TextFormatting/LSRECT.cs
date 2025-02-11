namespace MS.Internal.TextFormatting;

internal struct LSRECT
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	internal LSRECT(int x1, int y1, int x2, int y2)
	{
		left = x1;
		top = y1;
		right = x2;
		bottom = y2;
	}
}
