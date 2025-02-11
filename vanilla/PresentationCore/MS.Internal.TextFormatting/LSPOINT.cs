namespace MS.Internal.TextFormatting;

internal struct LSPOINT
{
	public int x;

	public int y;

	public LSPOINT(int horizontalPosition, int verticalPosition)
	{
		x = horizontalPosition;
		y = verticalPosition;
	}
}
