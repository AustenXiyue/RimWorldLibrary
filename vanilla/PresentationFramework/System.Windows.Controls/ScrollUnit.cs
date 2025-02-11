namespace System.Windows.Controls;

/// <summary>Specifies the type of unit that is used by the <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> attached property.</summary>
public enum ScrollUnit
{
	/// <summary>The <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> is measured in terms of device-independent units (1/96th inch per unit).</summary>
	Pixel,
	/// <summary>The <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> is measured in terms of the items that are displayed in the panel.</summary>
	Item
}
