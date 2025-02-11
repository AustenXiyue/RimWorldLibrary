namespace System.Windows.Controls;

/// <summary>Represents the desired size of the control's items, in device-independent units (1/96th inch per unit) and in logical units.</summary>
public struct HierarchicalVirtualizationItemDesiredSizes
{
	private Size _logicalSize;

	private Size _logicalSizeInViewport;

	private Size _logicalSizeBeforeViewport;

	private Size _logicalSizeAfterViewport;

	private Size _pixelSize;

	private Size _pixelSizeInViewport;

	private Size _pixelSizeBeforeViewport;

	private Size _pixelSizeAfterViewport;

	/// <summary>Gets the size of the control's child items, in logical units.</summary>
	/// <returns>The size of the control's child items, in logical units.</returns>
	public Size LogicalSize => _logicalSize;

	/// <summary>Gets the control's child items that are in the viewport, in logical units.</summary>
	/// <returns>The control's child items that are in the viewport, in logical units.</returns>
	public Size LogicalSizeInViewport => _logicalSizeInViewport;

	/// <summary>Gets the control's child items that are in the cache before the viewport, in logical units.</summary>
	/// <returns>The control's child items that are in the cache before the viewport, in logical units.</returns>
	public Size LogicalSizeBeforeViewport => _logicalSizeBeforeViewport;

	/// <summary>Gets the size of the control's child items that are in the cache after the viewport, in logical units.</summary>
	/// <returns>The control's child items that are in the cache after the viewport, in logical units.</returns>
	public Size LogicalSizeAfterViewport => _logicalSizeAfterViewport;

	/// <summary>Gets the size of the control's child items, in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The size of the control's child items, in device-independent units (1/96th inch per unit).</returns>
	public Size PixelSize => _pixelSize;

	/// <summary>Gets the size of the control's child items that are in the viewport, in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The size of the control's child items that are in the viewport, in device-independent units (1/96th inch per unit).</returns>
	public Size PixelSizeInViewport => _pixelSizeInViewport;

	/// <summary>Gets the size of the control's child items that are in the cache before the viewport, in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The size of the control's child items that are in the cache before the viewport, in device-independent units (1/96th inch per unit).</returns>
	public Size PixelSizeBeforeViewport => _pixelSizeBeforeViewport;

	/// <summary>Gets the size of the control's child items that are in the cache after the viewport, in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The size of the control's child items that are in the cache after the viewport, in device-independent units (1/96th inch per unit).</returns>
	public Size PixelSizeAfterViewport => _pixelSizeAfterViewport;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> class.</summary>
	/// <param name="logicalSize">The size of the control's child items, in logical units.</param>
	/// <param name="logicalSizeInViewport">The size of the control's child items that are in the viewport, in logical units.</param>
	/// <param name="logicalSizeBeforeViewport">The size of the control's child items that are in the cache before the viewport, in logical units.</param>
	/// <param name="logicalSizeAfterViewport">The size of the control's child items that are in the cache after the viewport, in logical units.</param>
	/// <param name="pixelSize">The size of the control's child items, in device-independent units (1/96th inch per unit).</param>
	/// <param name="pixelSizeInViewport">The size of the control's child items that are in viewport, in device-independent units (1/96th inch per unit).</param>
	/// <param name="pixelSizeBeforeViewport">The size of the control's child items that are in the cache before the viewport, in device-independent units (1/96th inch per unit).</param>
	/// <param name="pixelSizeAfterViewport">The size of the control's child items that are in the cache after the viewport, in device-independent units (1/96th inch per unit).</param>
	public HierarchicalVirtualizationItemDesiredSizes(Size logicalSize, Size logicalSizeInViewport, Size logicalSizeBeforeViewport, Size logicalSizeAfterViewport, Size pixelSize, Size pixelSizeInViewport, Size pixelSizeBeforeViewport, Size pixelSizeAfterViewport)
	{
		_logicalSize = logicalSize;
		_logicalSizeInViewport = logicalSizeInViewport;
		_logicalSizeBeforeViewport = logicalSizeBeforeViewport;
		_logicalSizeAfterViewport = logicalSizeAfterViewport;
		_pixelSize = pixelSize;
		_pixelSizeInViewport = pixelSizeInViewport;
		_pixelSizeBeforeViewport = pixelSizeBeforeViewport;
		_pixelSizeAfterViewport = pixelSizeAfterViewport;
	}

	/// <summary>Returns a value that indicates whether the specified object <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> are equal.</summary>
	/// <returns>true if the specified object <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> are equal; otherwise, false.</returns>
	/// <param name="itemDesiredSizes1">The first object to compare.</param>
	/// <param name="itemDesiredSizes2">The second object to compare.</param>
	public static bool operator ==(HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes1, HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes2)
	{
		if (itemDesiredSizes1.LogicalSize == itemDesiredSizes2.LogicalSize && itemDesiredSizes1.LogicalSizeInViewport == itemDesiredSizes2.LogicalSizeInViewport && itemDesiredSizes1.LogicalSizeBeforeViewport == itemDesiredSizes2.LogicalSizeBeforeViewport && itemDesiredSizes1.LogicalSizeAfterViewport == itemDesiredSizes2.LogicalSizeAfterViewport && itemDesiredSizes1.PixelSize == itemDesiredSizes2.PixelSize && itemDesiredSizes1.PixelSizeInViewport == itemDesiredSizes2.PixelSizeInViewport && itemDesiredSizes1.PixelSizeBeforeViewport == itemDesiredSizes2.PixelSizeBeforeViewport)
		{
			return itemDesiredSizes1.PixelSizeAfterViewport == itemDesiredSizes2.PixelSizeAfterViewport;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified object <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> are unequal.</summary>
	/// <returns>true if the specified object <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> are unequal; otherwise, false.</returns>
	/// <param name="itemDesiredSizes1">The first object to compare.</param>
	/// <param name="itemDesiredSizes2">The second object to compare.</param>
	public static bool operator !=(HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes1, HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes2)
	{
		if (!(itemDesiredSizes1.LogicalSize != itemDesiredSizes2.LogicalSize) && !(itemDesiredSizes1.LogicalSizeInViewport != itemDesiredSizes2.LogicalSizeInViewport) && !(itemDesiredSizes1.LogicalSizeBeforeViewport != itemDesiredSizes2.LogicalSizeBeforeViewport) && !(itemDesiredSizes1.LogicalSizeAfterViewport != itemDesiredSizes2.LogicalSizeAfterViewport) && !(itemDesiredSizes1.PixelSize != itemDesiredSizes2.PixelSize) && !(itemDesiredSizes1.PixelSizeInViewport != itemDesiredSizes2.PixelSizeInViewport) && !(itemDesiredSizes1.PixelSizeBeforeViewport != itemDesiredSizes2.PixelSizeBeforeViewport))
		{
			return itemDesiredSizes1.PixelSizeAfterViewport != itemDesiredSizes2.PixelSizeAfterViewport;
		}
		return true;
	}

	/// <summary>Returns a value that indicates whether the specified object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object.</summary>
	/// <returns>true if the specified object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object; otherwise, false.</returns>
	/// <param name="oCompare">The object to compare to this object.</param>
	public override bool Equals(object oCompare)
	{
		if (oCompare is HierarchicalVirtualizationItemDesiredSizes hierarchicalVirtualizationItemDesiredSizes)
		{
			return this == hierarchicalVirtualizationItemDesiredSizes;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object.</summary>
	/// <returns>true if the specified object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object; otherwise, false.</returns>
	/// <param name="comparisonItemSizes">The object to compare to this object.</param>
	public bool Equals(HierarchicalVirtualizationItemDesiredSizes comparisonItemSizes)
	{
		return this == comparisonItemSizes;
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object.</summary>
	/// <returns>A hash code for this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationItemDesiredSizes" /> object.</returns>
	public override int GetHashCode()
	{
		return _logicalSize.GetHashCode() ^ _logicalSizeInViewport.GetHashCode() ^ _logicalSizeBeforeViewport.GetHashCode() ^ _logicalSizeAfterViewport.GetHashCode() ^ _pixelSize.GetHashCode() ^ _pixelSizeInViewport.GetHashCode() ^ _pixelSizeBeforeViewport.GetHashCode() ^ _pixelSizeAfterViewport.GetHashCode();
	}
}
