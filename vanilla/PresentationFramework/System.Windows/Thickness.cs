using System.ComponentModel;
using System.Globalization;
using MS.Internal;

namespace System.Windows;

/// <summary>Describes the thickness of a frame around a rectangle. Four <see cref="T:System.Double" /> values describe the <see cref="P:System.Windows.Thickness.Left" />, <see cref="P:System.Windows.Thickness.Top" />, <see cref="P:System.Windows.Thickness.Right" />, and <see cref="P:System.Windows.Thickness.Bottom" /> sides of the rectangle, respectively. </summary>
[TypeConverter(typeof(ThicknessConverter))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public struct Thickness : IEquatable<Thickness>
{
	private double _Left;

	private double _Top;

	private double _Right;

	private double _Bottom;

	internal bool IsZero
	{
		get
		{
			if (DoubleUtil.IsZero(Left) && DoubleUtil.IsZero(Top) && DoubleUtil.IsZero(Right))
			{
				return DoubleUtil.IsZero(Bottom);
			}
			return false;
		}
	}

	internal bool IsUniform
	{
		get
		{
			if (DoubleUtil.AreClose(Left, Top) && DoubleUtil.AreClose(Left, Right))
			{
				return DoubleUtil.AreClose(Left, Bottom);
			}
			return false;
		}
	}

	/// <summary>Gets or sets the width, in pixels, of the left side of the bounding rectangle. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width, in pixels, of the left side of the bounding rectangle for this instance of <see cref="T:System.Windows.Thickness" />. a pixel is equal to 1/96 on an inch. The default is 0.</returns>
	public double Left
	{
		get
		{
			return _Left;
		}
		set
		{
			_Left = value;
		}
	}

	/// <summary>Gets or sets the width, in pixels, of the upper side of the bounding rectangle.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width, in pixels, of the upper side of the bounding rectangle for this instance of <see cref="T:System.Windows.Thickness" />. A pixel is equal to 1/96 of an inch. The default is 0.</returns>
	public double Top
	{
		get
		{
			return _Top;
		}
		set
		{
			_Top = value;
		}
	}

	/// <summary>Gets or sets the width, in pixels, of the right side of the bounding rectangle. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width, in pixels, of the right side of the bounding rectangle for this instance of <see cref="T:System.Windows.Thickness" />. A pixel is equal to 1/96 of an inch. The default is 0.</returns>
	public double Right
	{
		get
		{
			return _Right;
		}
		set
		{
			_Right = value;
		}
	}

	/// <summary>Gets or sets the width, in pixels, of the lower side of the bounding rectangle.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the width, in pixels, of the lower side of the bounding rectangle for this instance of <see cref="T:System.Windows.Thickness" />. A pixel is equal to 1/96 of an inch. The default is 0.</returns>
	public double Bottom
	{
		get
		{
			return _Bottom;
		}
		set
		{
			_Bottom = value;
		}
	}

	internal Size Size => new Size(_Left + _Right, _Top + _Bottom);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Thickness" /> structure that has the specified uniform length on each side. </summary>
	/// <param name="uniformLength">The uniform length applied to all four sides of the bounding rectangle.</param>
	public Thickness(double uniformLength)
	{
		_Left = (_Top = (_Right = (_Bottom = uniformLength)));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Thickness" /> structure that has specific lengths (supplied as a <see cref="T:System.Double" />) applied to each side of the rectangle. </summary>
	/// <param name="left">The thickness for the left side of the rectangle.</param>
	/// <param name="top">The thickness for the upper side of the rectangle.</param>
	/// <param name="right">The thickness for the right side of the rectangle</param>
	/// <param name="bottom">The thickness for the lower side of the rectangle.</param>
	public Thickness(double left, double top, double right, double bottom)
	{
		_Left = left;
		_Top = top;
		_Right = right;
		_Bottom = bottom;
	}

	/// <summary>Compares this <see cref="T:System.Windows.Thickness" /> structure to another <see cref="T:System.Object" /> for equality.</summary>
	/// <returns>true if the two objects are equal; otherwise, false.</returns>
	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj is Thickness thickness)
		{
			return this == thickness;
		}
		return false;
	}

	/// <summary>Compares this <see cref="T:System.Windows.Thickness" /> structure to another <see cref="T:System.Windows.Thickness" /> structure for equality.</summary>
	/// <returns>true if the two instances of <see cref="T:System.Windows.Thickness" /> are equal; otherwise, false.</returns>
	/// <param name="thickness">An instance of <see cref="T:System.Windows.Thickness" /> to compare for equality.</param>
	public bool Equals(Thickness thickness)
	{
		return this == thickness;
	}

	/// <summary>Returns the hash code of the structure.</summary>
	/// <returns>A hash code for this instance of <see cref="T:System.Windows.Thickness" />.</returns>
	public override int GetHashCode()
	{
		return _Left.GetHashCode() ^ _Top.GetHashCode() ^ _Right.GetHashCode() ^ _Bottom.GetHashCode();
	}

	/// <summary>Returns the string representation of the <see cref="T:System.Windows.Thickness" /> structure.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the <see cref="T:System.Windows.Thickness" /> value.</returns>
	public override string ToString()
	{
		return ThicknessConverter.ToString(this, CultureInfo.InvariantCulture);
	}

	internal string ToString(CultureInfo cultureInfo)
	{
		return ThicknessConverter.ToString(this, cultureInfo);
	}

	internal bool IsValid(bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
	{
		if (!allowNegative && (Left < 0.0 || Right < 0.0 || Top < 0.0 || Bottom < 0.0))
		{
			return false;
		}
		if (!allowNaN && (double.IsNaN(Left) || double.IsNaN(Right) || double.IsNaN(Top) || double.IsNaN(Bottom)))
		{
			return false;
		}
		if (!allowPositiveInfinity && (double.IsPositiveInfinity(Left) || double.IsPositiveInfinity(Right) || double.IsPositiveInfinity(Top) || double.IsPositiveInfinity(Bottom)))
		{
			return false;
		}
		if (!allowNegativeInfinity && (double.IsNegativeInfinity(Left) || double.IsNegativeInfinity(Right) || double.IsNegativeInfinity(Top) || double.IsNegativeInfinity(Bottom)))
		{
			return false;
		}
		return true;
	}

	internal bool IsClose(Thickness thickness)
	{
		if (DoubleUtil.AreClose(Left, thickness.Left) && DoubleUtil.AreClose(Top, thickness.Top) && DoubleUtil.AreClose(Right, thickness.Right))
		{
			return DoubleUtil.AreClose(Bottom, thickness.Bottom);
		}
		return false;
	}

	internal static bool AreClose(Thickness thickness0, Thickness thickness1)
	{
		return thickness0.IsClose(thickness1);
	}

	/// <summary>Compares the value of two <see cref="T:System.Windows.Thickness" /> structures for equality.</summary>
	/// <returns>true if the two instances of <see cref="T:System.Windows.Thickness" /> are equal; otherwise, false.</returns>
	/// <param name="t1">The first structure to compare.</param>
	/// <param name="t2">The other structure to compare.</param>
	public static bool operator ==(Thickness t1, Thickness t2)
	{
		if ((t1._Left == t2._Left || (double.IsNaN(t1._Left) && double.IsNaN(t2._Left))) && (t1._Top == t2._Top || (double.IsNaN(t1._Top) && double.IsNaN(t2._Top))) && (t1._Right == t2._Right || (double.IsNaN(t1._Right) && double.IsNaN(t2._Right))))
		{
			if (t1._Bottom != t2._Bottom)
			{
				if (double.IsNaN(t1._Bottom))
				{
					return double.IsNaN(t2._Bottom);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Thickness" /> structures for inequality. </summary>
	/// <returns>true if the two instances of <see cref="T:System.Windows.Thickness" /> are not equal; otherwise, false.</returns>
	/// <param name="t1">The first structure to compare.</param>
	/// <param name="t2">The other structure to compare.</param>
	public static bool operator !=(Thickness t1, Thickness t2)
	{
		return !(t1 == t2);
	}
}
