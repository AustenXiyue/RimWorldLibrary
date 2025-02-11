namespace System.Windows.Controls;

/// <summary> Defines the different roles that a <see cref="T:System.Windows.Controls.MenuItem" /> can have. </summary>
public enum MenuItemRole
{
	/// <summary> Top-level menu item that can invoke commands. </summary>
	TopLevelItem,
	/// <summary> Header for top-level menus. </summary>
	TopLevelHeader,
	/// <summary> Menu item in a submenu that can invoke commands. </summary>
	SubmenuItem,
	/// <summary> Header for a submenu. </summary>
	SubmenuHeader
}
