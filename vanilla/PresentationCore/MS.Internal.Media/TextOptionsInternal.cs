using System;
using System.Windows;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.Media;

[FriendAccessAllowed]
internal static class TextOptionsInternal
{
	[FriendAccessAllowed]
	internal static readonly DependencyProperty TextHintingModeProperty = DependencyProperty.RegisterAttached("TextHintingMode", typeof(TextHintingMode), typeof(TextOptionsInternal), new UIPropertyMetadata(TextHintingMode.Auto), System.Windows.Media.ValidateEnums.IsTextHintingModeValid);

	[FriendAccessAllowed]
	public static void SetTextHintingMode(DependencyObject element, TextHintingMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextHintingModeProperty, value);
	}

	[FriendAccessAllowed]
	public static TextHintingMode GetTextHintingMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (TextHintingMode)element.GetValue(TextHintingModeProperty);
	}
}
