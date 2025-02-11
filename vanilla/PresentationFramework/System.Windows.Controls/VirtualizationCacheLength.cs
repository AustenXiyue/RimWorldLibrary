using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Controls;

/// <summary>Represents the measurements for the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> attached property.</summary>
[TypeConverter(typeof(VirtualizationCacheLengthConverter))]
public struct VirtualizationCacheLength : IEquatable<VirtualizationCacheLength>
{
	private double _cacheBeforeViewport;

	private double _cacheAfterViewport;

	/// <summary>Gets the size of the cache after the viewport when the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing.</summary>
	/// <returns>The size of the cache after the viewport when the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing.</returns>
	public double CacheBeforeViewport => _cacheBeforeViewport;

	/// <summary>Gets the size of the cache before the viewport when the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing.</summary>
	/// <returns>The size of the cache after the viewport when the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing.</returns>
	public double CacheAfterViewport => _cacheAfterViewport;

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> class with a uniform cache length for each side of the viewport.</summary>
	/// <param name="cacheBeforeAndAfterViewport">The size of the cache before and after the viewport.</param>
	public VirtualizationCacheLength(double cacheBeforeAndAfterViewport)
		: this(cacheBeforeAndAfterViewport, cacheBeforeAndAfterViewport)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> class with the specified cache lengths for each side of the viewport.</summary>
	/// <param name="cacheBeforeViewport">The size of the cache before the viewport.</param>
	/// <param name="cacheAfterViewport">The size of the cache after the viewport.</param>
	public VirtualizationCacheLength(double cacheBeforeViewport, double cacheAfterViewport)
	{
		if (double.IsNaN(cacheBeforeViewport))
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterNoNaN, "cacheBeforeViewport"));
		}
		if (double.IsNaN(cacheAfterViewport))
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterNoNaN, "cacheAfterViewport"));
		}
		_cacheBeforeViewport = cacheBeforeViewport;
		_cacheAfterViewport = cacheAfterViewport;
	}

	/// <summary>Determines whether the two specified <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> objects are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> are equal; otherwise, false.</returns>
	/// <param name="cl1">The first object to compare.</param>
	/// <param name="cl2">The second object to compare.</param>
	public static bool operator ==(VirtualizationCacheLength cl1, VirtualizationCacheLength cl2)
	{
		if (cl1.CacheBeforeViewport == cl2.CacheBeforeViewport)
		{
			return cl1.CacheAfterViewport == cl2.CacheAfterViewport;
		}
		return false;
	}

	/// <summary>Determines whether the two specified <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> objects are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> are equal; otherwise, false.</returns>
	/// <param name="cl1">The first object to compare.</param>
	/// <param name="cl2">The second object to compare.</param>
	public static bool operator !=(VirtualizationCacheLength cl1, VirtualizationCacheLength cl2)
	{
		if (cl1.CacheBeforeViewport == cl2.CacheBeforeViewport)
		{
			return cl1.CacheAfterViewport != cl2.CacheAfterViewport;
		}
		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</summary>
	/// <returns>true if the specified object is equal to the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />; otherwise, false.</returns>
	/// <param name="oCompare">The object to compare with the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</param>
	public override bool Equals(object oCompare)
	{
		if (oCompare is VirtualizationCacheLength virtualizationCacheLength)
		{
			return this == virtualizationCacheLength;
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> is equal to the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</summary>
	/// <returns>true if the specified object is equal to the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />; otherwise, false.</returns>
	/// <param name="cacheLength">The object to compare with the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />. </param>
	public bool Equals(VirtualizationCacheLength cacheLength)
	{
		return this == cacheLength;
	}

	/// <summary>Gets a hash code for the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</returns>
	public override int GetHashCode()
	{
		return (int)_cacheBeforeViewport + (int)_cacheAfterViewport;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</summary>
	/// <returns>A string that represents the current <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</returns>
	public override string ToString()
	{
		return VirtualizationCacheLengthConverter.ToString(this, CultureInfo.InvariantCulture);
	}
}
