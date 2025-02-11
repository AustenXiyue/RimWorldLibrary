namespace System.Windows.Controls;

/// <summary>Represents the desired size of the control's header, in pixels and in logical units. This structure is used by the <see cref="T:System.Windows.Controls.Primitives.IHierarchicalVirtualizationAndScrollInfo" /> interface.</summary>
public struct HierarchicalVirtualizationHeaderDesiredSizes
{
	private Size _logicalSize;

	private Size _pixelSize;

	/// <summary>Gets the size of the header, in logical units.</summary>
	/// <returns>The size of the header, in logical units.</returns>
	public Size LogicalSize => _logicalSize;

	/// <summary>Gets the size of the header, in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The size of the header, in device-independent units (1/96th inch per unit).</returns>
	public Size PixelSize => _pixelSize;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" /> class.</summary>
	/// <param name="logicalSize">The size of the header, in logical units.</param>
	/// <param name="pixelSize">The size of the header, in device-independent units (1/96th inch per unit).</param>
	public HierarchicalVirtualizationHeaderDesiredSizes(Size logicalSize, Size pixelSize)
	{
		_logicalSize = logicalSize;
		_pixelSize = pixelSize;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" /> objects are equal.</summary>
	/// <returns>true if the specified objects are equal; otherwise, false.</returns>
	/// <param name="headerDesiredSizes1">The first object to compare.</param>
	/// <param name="headerDesiredSizes2">The second object to compare.</param>
	public static bool operator ==(HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes1, HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes2)
	{
		if (headerDesiredSizes1.LogicalSize == headerDesiredSizes2.LogicalSize)
		{
			return headerDesiredSizes1.PixelSize == headerDesiredSizes2.PixelSize;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" /> objects are unequal.</summary>
	/// <returns>true if the specified objects are unequal; otherwise, false.</returns>
	/// <param name="headerDesiredSizes1">The first object to compare.</param>
	/// <param name="headerDesiredSizes2">The second object to compare.</param>
	public static bool operator !=(HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes1, HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes2)
	{
		if (!(headerDesiredSizes1.LogicalSize != headerDesiredSizes2.LogicalSize))
		{
			return headerDesiredSizes1.PixelSize != headerDesiredSizes2.PixelSize;
		}
		return true;
	}

	/// <summary>Returns a value that indicates whether the specified object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" /> object.</summary>
	/// <returns>true if the specified object is equal to this object; otherwise, false.</returns>
	/// <param name="oCompare">The object to compare.</param>
	public override bool Equals(object oCompare)
	{
		if (oCompare is HierarchicalVirtualizationHeaderDesiredSizes hierarchicalVirtualizationHeaderDesiredSizes)
		{
			return this == hierarchicalVirtualizationHeaderDesiredSizes;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" /> object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" /> object.</summary>
	/// <returns>true if the specified object is equal to this object; otherwise, false.</returns>
	/// <param name="comparisonHeaderSizes">The object to compare.</param>
	public bool Equals(HierarchicalVirtualizationHeaderDesiredSizes comparisonHeaderSizes)
	{
		return this == comparisonHeaderSizes;
	}

	/// <summary>Gets a hash code for the <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" />.</summary>
	/// <returns>A hash code for the <see cref="T:System.Windows.Controls.HierarchicalVirtualizationHeaderDesiredSizes" />.</returns>
	public override int GetHashCode()
	{
		return _logicalSize.GetHashCode() ^ _pixelSize.GetHashCode();
	}
}
