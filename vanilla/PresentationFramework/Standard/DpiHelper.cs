using System;
using System.Windows;
using System.Windows.Media;

namespace Standard;

internal static class DpiHelper
{
	[ThreadStatic]
	private static Matrix _transformToDevice;

	[ThreadStatic]
	private static Matrix _transformToDip;

	public static Point LogicalPixelsToDevice(Point logicalPoint, double dpiScaleX, double dpiScaleY)
	{
		_transformToDevice = Matrix.Identity;
		_transformToDevice.Scale(dpiScaleX, dpiScaleY);
		return _transformToDevice.Transform(logicalPoint);
	}

	public static Point DevicePixelsToLogical(Point devicePoint, double dpiScaleX, double dpiScaleY)
	{
		_transformToDip = Matrix.Identity;
		_transformToDip.Scale(1.0 / dpiScaleX, 1.0 / dpiScaleY);
		return _transformToDip.Transform(devicePoint);
	}

	public static Rect LogicalRectToDevice(Rect logicalRectangle, double dpiScaleX, double dpiScaleY)
	{
		Point point = LogicalPixelsToDevice(new Point(logicalRectangle.Left, logicalRectangle.Top), dpiScaleX, dpiScaleY);
		Point point2 = LogicalPixelsToDevice(new Point(logicalRectangle.Right, logicalRectangle.Bottom), dpiScaleX, dpiScaleY);
		return new Rect(point, point2);
	}

	public static Rect DeviceRectToLogical(Rect deviceRectangle, double dpiScaleX, double dpiScaleY)
	{
		Point point = DevicePixelsToLogical(new Point(deviceRectangle.Left, deviceRectangle.Top), dpiScaleX, dpiScaleY);
		Point point2 = DevicePixelsToLogical(new Point(deviceRectangle.Right, deviceRectangle.Bottom), dpiScaleX, dpiScaleY);
		return new Rect(point, point2);
	}

	public static Size LogicalSizeToDevice(Size logicalSize, double dpiScaleX, double dpiScaleY)
	{
		Point point = LogicalPixelsToDevice(new Point(logicalSize.Width, logicalSize.Height), dpiScaleX, dpiScaleY);
		Size result = default(Size);
		result.Width = point.X;
		result.Height = point.Y;
		return result;
	}

	public static Size DeviceSizeToLogical(Size deviceSize, double dpiScaleX, double dpiScaleY)
	{
		Point point = DevicePixelsToLogical(new Point(deviceSize.Width, deviceSize.Height), dpiScaleX, dpiScaleY);
		return new Size(point.X, point.Y);
	}

	public static Thickness LogicalThicknessToDevice(Thickness logicalThickness, double dpiScaleX, double dpiScaleY)
	{
		Point point = LogicalPixelsToDevice(new Point(logicalThickness.Left, logicalThickness.Top), dpiScaleX, dpiScaleY);
		Point point2 = LogicalPixelsToDevice(new Point(logicalThickness.Right, logicalThickness.Bottom), dpiScaleX, dpiScaleY);
		return new Thickness(point.X, point.Y, point2.X, point2.Y);
	}
}
