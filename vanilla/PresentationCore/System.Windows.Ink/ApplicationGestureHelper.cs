namespace System.Windows.Ink;

internal static class ApplicationGestureHelper
{
	internal const int CountOfValues = 44;

	internal static bool IsDefined(ApplicationGesture applicationGesture)
	{
		switch (applicationGesture)
		{
		case ApplicationGesture.AllGestures:
		case ApplicationGesture.NoGesture:
		case ApplicationGesture.ScratchOut:
		case ApplicationGesture.Triangle:
		case ApplicationGesture.Square:
		case ApplicationGesture.Star:
		case ApplicationGesture.Check:
		case ApplicationGesture.Curlicue:
		case ApplicationGesture.DoubleCurlicue:
		case ApplicationGesture.Circle:
		case ApplicationGesture.DoubleCircle:
		case ApplicationGesture.SemicircleLeft:
		case ApplicationGesture.SemicircleRight:
		case ApplicationGesture.ChevronUp:
		case ApplicationGesture.ChevronDown:
		case ApplicationGesture.ChevronLeft:
		case ApplicationGesture.ChevronRight:
		case ApplicationGesture.ArrowUp:
		case ApplicationGesture.ArrowDown:
		case ApplicationGesture.ArrowLeft:
		case ApplicationGesture.ArrowRight:
		case ApplicationGesture.Up:
		case ApplicationGesture.Down:
		case ApplicationGesture.Left:
		case ApplicationGesture.Right:
		case ApplicationGesture.UpDown:
		case ApplicationGesture.DownUp:
		case ApplicationGesture.LeftRight:
		case ApplicationGesture.RightLeft:
		case ApplicationGesture.UpLeftLong:
		case ApplicationGesture.UpRightLong:
		case ApplicationGesture.DownLeftLong:
		case ApplicationGesture.DownRightLong:
		case ApplicationGesture.UpLeft:
		case ApplicationGesture.UpRight:
		case ApplicationGesture.DownLeft:
		case ApplicationGesture.DownRight:
		case ApplicationGesture.LeftUp:
		case ApplicationGesture.LeftDown:
		case ApplicationGesture.RightUp:
		case ApplicationGesture.RightDown:
		case ApplicationGesture.Exclamation:
		case ApplicationGesture.Tap:
		case ApplicationGesture.DoubleTap:
			return true;
		default:
			return false;
		}
	}
}
