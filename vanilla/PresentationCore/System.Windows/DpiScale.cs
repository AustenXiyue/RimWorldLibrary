namespace System.Windows;

public struct DpiScale
{
	private readonly double _dpiScaleX;

	private readonly double _dpiScaleY;

	public double DpiScaleX => _dpiScaleX;

	public double DpiScaleY => _dpiScaleY;

	public double PixelsPerDip => _dpiScaleY;

	public double PixelsPerInchX => 96.0 * _dpiScaleX;

	public double PixelsPerInchY => 96.0 * _dpiScaleY;

	public DpiScale(double dpiScaleX, double dpiScaleY)
	{
		_dpiScaleX = dpiScaleX;
		_dpiScaleY = dpiScaleY;
	}

	internal bool Equals(DpiScale other)
	{
		if (_dpiScaleX == other._dpiScaleX)
		{
			return _dpiScaleY == other._dpiScaleY;
		}
		return false;
	}
}
