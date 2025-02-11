namespace System.Windows.Controls;

/// <summary> Specifies how <see cref="T:System.Windows.Controls.ToolBar" /> items are placed in the main toolbar panel and in the overflow panel. </summary>
public enum OverflowMode
{
	/// <summary> Item moves between the main panel and overflow panel, depending on the available space. </summary>
	AsNeeded,
	/// <summary> Item is permanently placed in the overflow panel. </summary>
	Always,
	/// <summary> Item is never allowed to overflow. </summary>
	Never
}
