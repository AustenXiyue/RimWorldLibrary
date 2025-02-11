namespace System.Windows;

/// <summary>Specifies whether a window can be resized and, if so, how it can be resized. Used by the <see cref="P:System.Windows.Window.ResizeMode" /> property.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public enum ResizeMode
{
	/// <summary>A window cannot be resized. The Minimize and Maximize buttons are not displayed in the title bar.</summary>
	NoResize,
	/// <summary>A window can only be minimized and restored. The Minimize and Maximize buttons are both shown, but only the Minimize button is enabled.</summary>
	CanMinimize,
	/// <summary>A window can be resized. The Minimize and Maximize buttons are both shown and enabled.</summary>
	CanResize,
	/// <summary>A window can be resized. The Minimize and Maximize buttons are both shown and enabled. A resize grip appears in the bottom-right corner of the window.</summary>
	CanResizeWithGrip
}
