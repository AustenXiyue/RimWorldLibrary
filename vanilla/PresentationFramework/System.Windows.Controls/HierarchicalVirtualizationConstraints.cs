namespace System.Windows.Controls;

/// <summary>Specifies the sizes of a control's viewport and cache. This structure is used by the <see cref="T:System.Windows.Controls.Primitives.IHierarchicalVirtualizationAndScrollInfo" /> interface.</summary>
public struct HierarchicalVirtualizationConstraints
{
	private VirtualizationCacheLength _cacheLength;

	private VirtualizationCacheLengthUnit _cacheLengthUnit;

	private Rect _viewport;

	private long _scrollGeneration;

	/// <summary>Gets the size of the cache before and after the viewport.</summary>
	/// <returns>The size of the cache before and after the viewport.</returns>
	public VirtualizationCacheLength CacheLength => _cacheLength;

	/// <summary>Gets the type of unit that is used by the <see cref="P:System.Windows.Controls.HierarchicalVirtualizationConstraints.CacheLength" /> property.</summary>
	/// <returns>The type of unit that is used by the <see cref="P:System.Windows.Controls.HierarchicalVirtualizationConstraints.CacheLength" /> property.</returns>
	public VirtualizationCacheLengthUnit CacheLengthUnit => _cacheLengthUnit;

	/// <summary>Gets the area that displays the items of the control. </summary>
	/// <returns>The area that displays the items of the control.</returns>
	public Rect Viewport => _viewport;

	internal long ScrollGeneration
	{
		get
		{
			return _scrollGeneration;
		}
		set
		{
			_scrollGeneration = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> class.</summary>
	/// <param name="cacheLength">The size of the cache before and after the viewport.</param>
	/// <param name="cacheLengthUnit">The type of unit that is used by the <see cref="P:System.Windows.Controls.HierarchicalVirtualizationConstraints.CacheLength" /> property.</param>
	/// <param name="viewport">The size of the cache before and after the viewport.</param>
	public HierarchicalVirtualizationConstraints(VirtualizationCacheLength cacheLength, VirtualizationCacheLengthUnit cacheLengthUnit, Rect viewport)
	{
		_cacheLength = cacheLength;
		_cacheLengthUnit = cacheLengthUnit;
		_viewport = viewport;
		_scrollGeneration = 0L;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> objects are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> objects are equal; otherwise, false.</returns>
	/// <param name="constraints1">The first <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> to compare.</param>
	/// <param name="constraints2">The second <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> to compare.</param>
	public static bool operator ==(HierarchicalVirtualizationConstraints constraints1, HierarchicalVirtualizationConstraints constraints2)
	{
		if (constraints1.CacheLength == constraints2.CacheLength && constraints1.CacheLengthUnit == constraints2.CacheLengthUnit)
		{
			return constraints2.Viewport == constraints2.Viewport;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> objects are unequal.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> objects are unequal; otherwise, false.</returns>
	/// <param name="constraints1">The first <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> to compare.</param>
	/// <param name="constraints2">The second <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> to compare.</param>
	public static bool operator !=(HierarchicalVirtualizationConstraints constraints1, HierarchicalVirtualizationConstraints constraints2)
	{
		if (!(constraints1.CacheLength != constraints2.CacheLength) && constraints1.CacheLengthUnit == constraints2.CacheLengthUnit)
		{
			return constraints1.Viewport != constraints2.Viewport;
		}
		return true;
	}

	/// <summary>Returns a value that indicates whether the specified object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" />.</summary>
	/// <returns>true if the specified object is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" />; otherwise, false.</returns>
	/// <param name="oCompare">The object to compare.</param>
	public override bool Equals(object oCompare)
	{
		if (oCompare is HierarchicalVirtualizationConstraints hierarchicalVirtualizationConstraints)
		{
			return this == hierarchicalVirtualizationConstraints;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> is equal to this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" />; otherwise, false.</returns>
	/// <param name="comparisonConstraints">The <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" /> to compare.</param>
	public bool Equals(HierarchicalVirtualizationConstraints comparisonConstraints)
	{
		return this == comparisonConstraints;
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" />.</summary>
	/// <returns>A hash code for this <see cref="T:System.Windows.Controls.HierarchicalVirtualizationConstraints" />.</returns>
	public override int GetHashCode()
	{
		return _cacheLength.GetHashCode() ^ _cacheLengthUnit.GetHashCode() ^ _viewport.GetHashCode();
	}
}
