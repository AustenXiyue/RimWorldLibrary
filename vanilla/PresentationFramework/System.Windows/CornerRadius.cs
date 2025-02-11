using System.ComponentModel;
using System.Globalization;
using MS.Internal;

namespace System.Windows;

/// <summary>Represents the radii of a rectangle's corners. </summary>
[TypeConverter(typeof(CornerRadiusConverter))]
public struct CornerRadius : IEquatable<CornerRadius>
{
	private double _topLeft;

	private double _topRight;

	private double _bottomLeft;

	private double _bottomRight;

	/// <summary>Gets or sets the radius of the top-left corner. </summary>
	/// <returns>The radius of the top-left corner. The default is 0.</returns>
	public double TopLeft
	{
		get
		{
			return _topLeft;
		}
		set
		{
			_topLeft = value;
		}
	}

	/// <summary>Gets or sets the radius of the top-right corner. </summary>
	/// <returns>The radius of the top-right corner. The default is 0.</returns>
	public double TopRight
	{
		get
		{
			return _topRight;
		}
		set
		{
			_topRight = value;
		}
	}

	/// <summary>Gets or sets the radius of the bottom-right corner. </summary>
	/// <returns>The radius of the bottom-right corner. The default is 0.</returns>
	public double BottomRight
	{
		get
		{
			return _bottomRight;
		}
		set
		{
			_bottomRight = value;
		}
	}

	/// <summary>Gets or sets the radius of the bottom-left corner. </summary>
	/// <returns>The radius of the bottom-left corner. The default is 0.</returns>
	public double BottomLeft
	{
		get
		{
			return _bottomLeft;
		}
		set
		{
			_bottomLeft = value;
		}
	}

	internal bool IsZero
	{
		get
		{
			if (DoubleUtil.IsZero(_topLeft) && DoubleUtil.IsZero(_topRight) && DoubleUtil.IsZero(_bottomRight))
			{
				return DoubleUtil.IsZero(_bottomLeft);
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.CornerRadius" /> class with a specified uniform radius value for every corner or the rectangle. </summary>
	/// <param name="uniformRadius">The radius value applied to every corner of the rectangle.</param>
	public CornerRadius(double uniformRadius)
	{
		_topLeft = (_topRight = (_bottomLeft = (_bottomRight = uniformRadius)));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.CornerRadius" /> class with the specified radius values for each corner of the rectangle. </summary>
	/// <param name="topLeft">The radius of the top-left corner.</param>
	/// <param name="topRight">The radius of the top-right corner.</param>
	/// <param name="bottomRight">The radius of the bottom-right corner.</param>
	/// <param name="bottomLeft">The radius of the bottom-left corner.</param>
	public CornerRadius(double topLeft, double topRight, double bottomRight, double bottomLeft)
	{
		_topLeft = topLeft;
		_topRight = topRight;
		_bottomRight = bottomRight;
		_bottomLeft = bottomLeft;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is a <see cref="T:System.Windows.CornerRadius" /> and whether it contains the same corner radius values as this <see cref="T:System.Windows.CornerRadius" />. </summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.Windows.CornerRadius" /> and contains the same corner radius values as this <see cref="T:System.Windows.CornerRadius" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj is CornerRadius cornerRadius)
		{
			return this == cornerRadius;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.CornerRadius" /> structures for equality.</summary>
	/// <returns>true if <paramref name="cornerRadius" /> contains the same corner radius values as this <see cref="T:System.Windows.CornerRadius" />; otherwise, false.</returns>
	/// <param name="cornerRadius">The <see cref="T:System.Windows.CornerRadius" /> to compare to this <see cref="T:System.Windows.CornerRadius" />.</param>
	public bool Equals(CornerRadius cornerRadius)
	{
		return this == cornerRadius;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.CornerRadius" />. </summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.CornerRadius" /> structure.</returns>
	public override int GetHashCode()
	{
		return _topLeft.GetHashCode() ^ _topRight.GetHashCode() ^ _bottomLeft.GetHashCode() ^ _bottomRight.GetHashCode();
	}

	/// <summary>Returns the string representation of the <see cref="T:System.Windows.CornerRadius" />. </summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.CornerRadius" />.</returns>
	public override string ToString()
	{
		return CornerRadiusConverter.ToString(this, CultureInfo.InvariantCulture);
	}

	/// <summary>Compares two <see cref="T:System.Windows.CornerRadius" /> structures for equality.</summary>
	/// <returns>true if <paramref name="cr1" /> and <paramref name="cr2" /> have equal values for all corners (same values for <see cref="P:System.Windows.CornerRadius.TopLeft" />, <see cref="P:System.Windows.CornerRadius.TopRight" />, <see cref="P:System.Windows.CornerRadius.BottomLeft" />, <see cref="P:System.Windows.CornerRadius.BottomRight" />); false if <paramref name="cr1" /> and <paramref name="cr2" /> have different values for one or more corners.</returns>
	/// <param name="cr1">The first <see cref="T:System.Windows.CornerRadius" /> to compare.</param>
	/// <param name="cr2">The second <see cref="T:System.Windows.CornerRadius" /> to compare.</param>
	public static bool operator ==(CornerRadius cr1, CornerRadius cr2)
	{
		if ((cr1._topLeft == cr2._topLeft || (double.IsNaN(cr1._topLeft) && double.IsNaN(cr2._topLeft))) && (cr1._topRight == cr2._topRight || (double.IsNaN(cr1._topRight) && double.IsNaN(cr2._topRight))) && (cr1._bottomRight == cr2._bottomRight || (double.IsNaN(cr1._bottomRight) && double.IsNaN(cr2._bottomRight))))
		{
			if (cr1._bottomLeft != cr2._bottomLeft)
			{
				if (double.IsNaN(cr1._bottomLeft))
				{
					return double.IsNaN(cr2._bottomLeft);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.CornerRadius" /> structures for inequality. </summary>
	/// <returns>true if <paramref name="cr1" /> and <paramref name="cr2" /> have different values for one or more corners (different values for <see cref="P:System.Windows.CornerRadius.TopLeft" />, <see cref="P:System.Windows.CornerRadius.TopRight" />, <see cref="P:System.Windows.CornerRadius.BottomLeft" />, <see cref="P:System.Windows.CornerRadius.BottomRight" />); false if <paramref name="cr1" /> and <paramref name="cr2" /> have identical corners.</returns>
	/// <param name="cr1">The first <see cref="T:System.Windows.CornerRadius" /> to compare.</param>
	/// <param name="cr2">The second <see cref="T:System.Windows.CornerRadius" /> to compare.</param>
	public static bool operator !=(CornerRadius cr1, CornerRadius cr2)
	{
		return !(cr1 == cr2);
	}

	internal bool IsValid(bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
	{
		if (!allowNegative && (_topLeft < 0.0 || _topRight < 0.0 || _bottomLeft < 0.0 || _bottomRight < 0.0))
		{
			return false;
		}
		if (!allowNaN && (double.IsNaN(_topLeft) || double.IsNaN(_topRight) || double.IsNaN(_bottomLeft) || double.IsNaN(_bottomRight)))
		{
			return false;
		}
		if (!allowPositiveInfinity && (double.IsPositiveInfinity(_topLeft) || double.IsPositiveInfinity(_topRight) || double.IsPositiveInfinity(_bottomLeft) || double.IsPositiveInfinity(_bottomRight)))
		{
			return false;
		}
		if (!allowNegativeInfinity && (double.IsNegativeInfinity(_topLeft) || double.IsNegativeInfinity(_topRight) || double.IsNegativeInfinity(_bottomLeft) || double.IsNegativeInfinity(_bottomRight)))
		{
			return false;
		}
		return true;
	}
}
