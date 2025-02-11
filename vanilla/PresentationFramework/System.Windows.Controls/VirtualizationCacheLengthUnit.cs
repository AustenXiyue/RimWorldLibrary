namespace System.Windows.Controls;

/// <summary>Specifies the type of unit that is used by the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> attached property.</summary>
public enum VirtualizationCacheLengthUnit
{
	/// <summary>The <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> is measured in terms of device-independent units (1/96th inch per unit).</summary>
	Pixel,
	/// <summary>The <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> is measured in terms of the items that are displayed in the panel.</summary>
	Item,
	/// <summary>The <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> is measured in terms of a page, which is equal to the size of the panel's viewport.</summary>
	Page
}
