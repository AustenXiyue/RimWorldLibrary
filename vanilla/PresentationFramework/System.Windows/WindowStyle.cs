namespace System.Windows;

/// <summary>Specifies the type of border that a <see cref="T:System.Windows.Window" /> has. Used by the <see cref="P:System.Windows.Window.WindowStyle" /> property.</summary>
public enum WindowStyle
{
	/// <summary>Only the client area is visible - the title bar and border are not shown. A <see cref="T:System.Windows.Navigation.NavigationWindow" /> with a <see cref="P:System.Windows.Window.WindowStyle" /> of <see cref="F:System.Windows.WindowStyle.None" /> will still display the navigation user interface (UI).</summary>
	None,
	/// <summary>A window with a single border. This is the default value.</summary>
	SingleBorderWindow,
	/// <summary>A window with a 3-D border.</summary>
	ThreeDBorderWindow,
	/// <summary>A fixed tool window.</summary>
	ToolWindow
}
