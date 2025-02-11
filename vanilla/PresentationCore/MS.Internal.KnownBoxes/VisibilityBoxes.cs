using System.Windows;
using MS.Internal.PresentationCore;

namespace MS.Internal.KnownBoxes;

[FriendAccessAllowed]
internal static class VisibilityBoxes
{
	internal static object VisibleBox = Visibility.Visible;

	internal static object HiddenBox = Visibility.Hidden;

	internal static object CollapsedBox = Visibility.Collapsed;

	internal static object Box(Visibility value)
	{
		return value switch
		{
			Visibility.Visible => VisibleBox, 
			Visibility.Hidden => HiddenBox, 
			_ => CollapsedBox, 
		};
	}
}
