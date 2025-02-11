using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal;

[FriendAccessAllowed]
internal static class PointUtil
{
	public static Point ClientToRoot(Point point, PresentationSource presentationSource)
	{
		bool success = true;
		return TryClientToRoot(point, presentationSource, throwOnError: true, out success);
	}

	public static Point TryClientToRoot(Point point, PresentationSource presentationSource, bool throwOnError, out bool success)
	{
		if (throwOnError || (presentationSource != null && presentationSource.CompositionTarget != null && !presentationSource.CompositionTarget.IsDisposed))
		{
			point = presentationSource.CompositionTarget.TransformFromDevice.Transform(point);
			point = TryApplyVisualTransform(point, presentationSource.RootVisual, inverse: true, throwOnError, out success);
			return point;
		}
		success = false;
		return new Point(0.0, 0.0);
	}

	public static Point RootToClient(Point point, PresentationSource presentationSource)
	{
		point = ApplyVisualTransform(point, presentationSource.RootVisual, inverse: false);
		point = presentationSource.CompositionTarget.TransformToDevice.Transform(point);
		return point;
	}

	public static Point ApplyVisualTransform(Point point, Visual v, bool inverse)
	{
		bool success = true;
		return TryApplyVisualTransform(point, v, inverse, throwOnError: true, out success);
	}

	public static Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out bool success)
	{
		success = true;
		if (v != null)
		{
			Matrix visualTransform = GetVisualTransform(v);
			if (inverse)
			{
				if (!throwOnError && !visualTransform.HasInverse)
				{
					success = false;
					return new Point(0.0, 0.0);
				}
				visualTransform.Invert();
			}
			point = visualTransform.Transform(point);
		}
		return point;
	}

	internal static Matrix GetVisualTransform(Visual v)
	{
		if (v != null)
		{
			Matrix matrix = Matrix.Identity;
			Transform transform = VisualTreeHelper.GetTransform(v);
			if (transform != null)
			{
				Matrix value = transform.Value;
				matrix = Matrix.Multiply(matrix, value);
			}
			Vector offset = VisualTreeHelper.GetOffset(v);
			matrix.Translate(offset.X, offset.Y);
			return matrix;
		}
		return Matrix.Identity;
	}

	public static Point ClientToScreen(Point pointClient, PresentationSource presentationSource)
	{
		if (!(presentationSource is HwndSource hwndSource))
		{
			return pointClient;
		}
		HandleRef handleRef = new HandleRef(hwndSource, hwndSource.CriticalHandle);
		MS.Win32.NativeMethods.POINT pt = AdjustForRightToLeft(FromPoint(pointClient), handleRef);
		MS.Win32.UnsafeNativeMethods.ClientToScreen(handleRef, ref pt);
		return ToPoint(pt);
	}

	internal static Point ScreenToClient(Point pointScreen, PresentationSource presentationSource)
	{
		if (!(presentationSource is HwndSource hwndSource))
		{
			return pointScreen;
		}
		HandleRef handleRef = new HandleRef(hwndSource, hwndSource.CriticalHandle);
		MS.Win32.NativeMethods.POINT pt = FromPoint(pointScreen);
		SafeNativeMethods.ScreenToClient(handleRef, ref pt);
		pt = AdjustForRightToLeft(pt, handleRef);
		return ToPoint(pt);
	}

	internal static Rect ElementToRoot(Rect rectElement, Visual element, PresentationSource presentationSource)
	{
		return element.TransformToAncestor(presentationSource.RootVisual).TransformBounds(rectElement);
	}

	internal static Rect RootToClient(Rect rectRoot, PresentationSource presentationSource)
	{
		CompositionTarget compositionTarget = presentationSource.CompositionTarget;
		Matrix visualTransform = GetVisualTransform(compositionTarget.RootVisual);
		Rect rect = Rect.Transform(rectRoot, visualTransform);
		Matrix transformToDevice = compositionTarget.TransformToDevice;
		return Rect.Transform(rect, transformToDevice);
	}

	internal static Rect ClientToScreen(Rect rectClient, HwndSource hwndSource)
	{
		Point point = ClientToScreen(rectClient.TopLeft, hwndSource);
		Point point2 = ClientToScreen(rectClient.BottomRight, hwndSource);
		return new Rect(point, point2);
	}

	internal static MS.Win32.NativeMethods.POINT AdjustForRightToLeft(MS.Win32.NativeMethods.POINT pt, HandleRef handleRef)
	{
		if ((SafeNativeMethods.GetWindowStyle(handleRef, exStyle: true) & 0x400000) == 4194304)
		{
			MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
			SafeNativeMethods.GetClientRect(handleRef, ref rect);
			pt.x = rect.right - pt.x;
		}
		return pt;
	}

	internal static MS.Win32.NativeMethods.RECT AdjustForRightToLeft(MS.Win32.NativeMethods.RECT rc, HandleRef handleRef)
	{
		if ((SafeNativeMethods.GetWindowStyle(handleRef, exStyle: true) & 0x400000) == 4194304)
		{
			MS.Win32.NativeMethods.RECT rect = default(MS.Win32.NativeMethods.RECT);
			SafeNativeMethods.GetClientRect(handleRef, ref rect);
			int num = rc.right - rc.left;
			rc.right = rect.right - rc.left;
			rc.left = rc.right - num;
		}
		return rc;
	}

	internal static MS.Win32.NativeMethods.POINT FromPoint(Point point)
	{
		return new MS.Win32.NativeMethods.POINT(DoubleUtil.DoubleToInt(point.X), DoubleUtil.DoubleToInt(point.Y));
	}

	internal static Point ToPoint(MS.Win32.NativeMethods.POINT pt)
	{
		return new Point(pt.x, pt.y);
	}

	internal static MS.Win32.NativeMethods.RECT FromRect(Rect rect)
	{
		MS.Win32.NativeMethods.RECT result = default(MS.Win32.NativeMethods.RECT);
		result.top = DoubleUtil.DoubleToInt(rect.Y);
		result.left = DoubleUtil.DoubleToInt(rect.X);
		result.bottom = DoubleUtil.DoubleToInt(rect.Bottom);
		result.right = DoubleUtil.DoubleToInt(rect.Right);
		return result;
	}

	internal static Rect ToRect(MS.Win32.NativeMethods.RECT rc)
	{
		Rect result = default(Rect);
		result.X = rc.left;
		result.Y = rc.top;
		result.Width = rc.right - rc.left;
		result.Height = rc.bottom - rc.top;
		return result;
	}
}
