namespace System.Windows.Input;

internal struct TabletDeviceSizeInfo
{
	public Size TabletSize;

	public Size ScreenSize;

	internal TabletDeviceSizeInfo(Size tabletSize, Size screenSize)
	{
		TabletSize = tabletSize;
		ScreenSize = screenSize;
	}
}
