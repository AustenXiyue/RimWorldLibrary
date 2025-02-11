namespace System.Windows;

/// <summary>Specifies the position that a <see cref="T:System.Windows.Window" /> will be shown in when it is first opened. Used by the <see cref="P:System.Windows.Window.WindowStartupLocation" /> property.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public enum WindowStartupLocation
{
	/// <summary>The startup location of a <see cref="T:System.Windows.Window" /> is set from code, or defers to the default Windows location.</summary>
	Manual,
	/// <summary>The startup location of a <see cref="T:System.Windows.Window" /> is the center of the screen that contains the mouse cursor.</summary>
	CenterScreen,
	/// <summary>The startup location of a <see cref="T:System.Windows.Window" /> is the center of the <see cref="T:System.Windows.Window" /> that owns it, as specified by the <see cref="P:System.Windows.Window.Owner" /> property.</summary>
	CenterOwner
}
