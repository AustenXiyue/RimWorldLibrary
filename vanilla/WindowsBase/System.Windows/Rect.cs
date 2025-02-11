using System.ComponentModel;
using System.Windows.Converters;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Describes the width, height, and location of a rectangle. </summary>
[Serializable]
[TypeConverter(typeof(RectConverter))]
[ValueSerializer(typeof(RectValueSerializer))]
public struct Rect : IFormattable
{
	internal double _x;

	internal double _y;

	internal double _width;

	internal double _height;

	private static readonly Rect s_empty = CreateEmptyRect();

	/// <summary>Gets a special value that represents a rectangle with no position or area. </summary>
	/// <returns>The empty rectangle, which has <see cref="P:System.Windows.Rect.X" /> and <see cref="P:System.Windows.Rect.Y" /> property values of <see cref="F:System.Double.PositiveInfinity" />, and has <see cref="P:System.Windows.Rect.Width" /> and <see cref="P:System.Windows.Rect.Height" /> property values of <see cref="F:System.Double.NegativeInfinity" />.</returns>
	public static Rect Empty => s_empty;

	/// <summary>Gets a value that indicates whether the rectangle is the <see cref="P:System.Windows.Rect.Empty" /> rectangle.</summary>
	/// <returns>true if the rectangle is the <see cref="P:System.Windows.Rect.Empty" /> rectangle; otherwise, false.</returns>
	public bool IsEmpty => _width < 0.0;

	/// <summary>Gets or sets the position of the top-left corner of the rectangle.</summary>
	/// <returns>The position of the top-left corner of the rectangle. The default is (0, 0). </returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Rect.Location" /> is set on an <see cref="P:System.Windows.Rect.Empty" /> rectangle. </exception>
	public Point Location
	{
		get
		{
			return new Point(_x, _y);
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect_CannotModifyEmptyRect);
			}
			_x = value._x;
			_y = value._y;
		}
	}

	/// <summary>Gets or sets the width and height of the rectangle. </summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> structure that specifies the width and height of the rectangle.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Rect.Size" /> is set on an <see cref="P:System.Windows.Rect.Empty" /> rectangle. </exception>
	public Size Size
	{
		get
		{
			if (IsEmpty)
			{
				return Size.Empty;
			}
			return new Size(_width, _height);
		}
		set
		{
			if (value.IsEmpty)
			{
				this = s_empty;
				return;
			}
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect_CannotModifyEmptyRect);
			}
			_width = value._width;
			_height = value._height;
		}
	}

	/// <summary>Gets or sets the x-axis value of the left side of the rectangle. </summary>
	/// <returns>The x-axis value of the left side of the rectangle.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Rect.X" /> is set on an <see cref="P:System.Windows.Rect.Empty" /> rectangle. </exception>
	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect_CannotModifyEmptyRect);
			}
			_x = value;
		}
	}

	/// <summary>Gets or sets the y-axis value of the top side of the rectangle. </summary>
	/// <returns>The y-axis value of the top side of the rectangle.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Rect.Y" /> is set on an <see cref="P:System.Windows.Rect.Empty" /> rectangle. </exception>
	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect_CannotModifyEmptyRect);
			}
			_y = value;
		}
	}

	/// <summary>Gets or sets the width of the rectangle.  </summary>
	/// <returns>A positive number that represents the width of the rectangle. The default is 0.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Rect.Width" /> is set to a negative value.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Rect.Width" /> is set on an <see cref="P:System.Windows.Rect.Empty" /> rectangle. </exception>
	public double Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect_CannotModifyEmptyRect);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size_WidthCannotBeNegative);
			}
			_width = value;
		}
	}

	/// <summary>Gets or sets the height of the rectangle. </summary>
	/// <returns>A positive number that represents the height of the rectangle. The default is 0.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Rect.Height" /> is set to a negative value.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Rect.Height" /> is set on an <see cref="P:System.Windows.Rect.Empty" /> rectangle. </exception>
	public double Height
	{
		get
		{
			return _height;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect_CannotModifyEmptyRect);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size_HeightCannotBeNegative);
			}
			_height = value;
		}
	}

	/// <summary>Gets the x-axis value of the left side of the rectangle. </summary>
	/// <returns>The x-axis value of the left side of the rectangle.</returns>
	public double Left => _x;

	/// <summary>Gets the y-axis position of the top of the rectangle. </summary>
	/// <returns>The y-axis position of the top of the rectangle.</returns>
	public double Top => _y;

	/// <summary>Gets the x-axis value of the right side of the rectangle.  </summary>
	/// <returns>The x-axis value of the right side of the rectangle.</returns>
	public double Right
	{
		get
		{
			if (IsEmpty)
			{
				return double.NegativeInfinity;
			}
			return _x + _width;
		}
	}

	/// <summary>Gets the y-axis value of the bottom of the rectangle. </summary>
	/// <returns>The y-axis value of the bottom of the rectangle. If the rectangle is empty, the value is <see cref="F:System.Double.NegativeInfinity" /> .</returns>
	public double Bottom
	{
		get
		{
			if (IsEmpty)
			{
				return double.NegativeInfinity;
			}
			return _y + _height;
		}
	}

	/// <summary>Gets the position of the top-left corner of the rectangle. </summary>
	/// <returns>The position of the top-left corner of the rectangle.</returns>
	public Point TopLeft => new Point(Left, Top);

	/// <summary>Gets the position of the top-right corner of the rectangle. </summary>
	/// <returns>The position of the top-right corner of the rectangle.</returns>
	public Point TopRight => new Point(Right, Top);

	/// <summary>Gets the position of the bottom-left corner of the rectangle </summary>
	/// <returns>The position of the bottom-left corner of the rectangle.</returns>
	public Point BottomLeft => new Point(Left, Bottom);

	/// <summary>Gets the position of the bottom-right corner of the rectangle. </summary>
	/// <returns>The position of the bottom-right corner of the rectangle.</returns>
	public Point BottomRight => new Point(Right, Bottom);

	/// <summary>Compares two rectangles for exact equality.</summary>
	/// <returns>true if the rectangles have the same <see cref="P:System.Windows.Rect.Location" /> and <see cref="P:System.Windows.Rect.Size" /> values; otherwise, false.</returns>
	/// <param name="rect1">The first rectangle to compare.</param>
	/// <param name="rect2">The second rectangle to compare.</param>
	public static bool operator ==(Rect rect1, Rect rect2)
	{
		if (rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Width == rect2.Width)
		{
			return rect1.Height == rect2.Height;
		}
		return false;
	}

	/// <summary>Compares two rectangles for inequality.  </summary>
	/// <returns>true if the rectangles do not have the same <see cref="P:System.Windows.Rect.Location" /> and <see cref="P:System.Windows.Rect.Size" /> values; otherwise, false.</returns>
	/// <param name="rect1">The first rectangle to compare.</param>
	/// <param name="rect2">The second rectangle to compare.</param>
	public static bool operator !=(Rect rect1, Rect rect2)
	{
		return !(rect1 == rect2);
	}

	/// <summary>Indicates whether the specified rectangles are equal. </summary>
	/// <returns>true if the rectangles have the same <see cref="P:System.Windows.Rect.Location" /> and <see cref="P:System.Windows.Rect.Size" /> values; otherwise, false.</returns>
	/// <param name="rect1">The first rectangle to compare.</param>
	/// <param name="rect2">The second rectangle to compare.</param>
	public static bool Equals(Rect rect1, Rect rect2)
	{
		if (rect1.IsEmpty)
		{
			return rect2.IsEmpty;
		}
		if (rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width))
		{
			return rect1.Height.Equals(rect2.Height);
		}
		return false;
	}

	/// <summary>Indicates whether the specified object is equal to the current rectangle.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Windows.Rect" /> and has the same <see cref="P:System.Windows.Rect.Location" /> and <see cref="P:System.Windows.Rect.Size" /> values as the current rectangle; otherwise, false.</returns>
	/// <param name="o">The object to compare to the current rectangle.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Rect rect))
		{
			return false;
		}
		return Equals(this, rect);
	}

	/// <summary>Indicates whether the specified rectangle is equal to the current rectangle. </summary>
	/// <returns>true if the specified rectangle has the same <see cref="P:System.Windows.Rect.Location" /> and <see cref="P:System.Windows.Rect.Size" /> values as the current rectangle; otherwise, false.</returns>
	/// <param name="value">The rectangle to compare to the current rectangle.</param>
	public bool Equals(Rect value)
	{
		return Equals(this, value);
	}

	/// <summary>Creates a hash code for the rectangle. </summary>
	/// <returns>A hash code for the current <see cref="T:System.Windows.Rect" /> structure.</returns>
	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
	}

	/// <summary>Creates a new rectangle from the specified string representation. </summary>
	/// <returns>The resulting rectangle.</returns>
	/// <param name="source">The string representation of the rectangle, in the form "x, y, width, height".</param>
	public static Rect Parse(string source)
	{
		IFormatProvider invariantEnglishUS = TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Rect result = ((!(text == "Empty")) ? new Rect(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Empty);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Returns a string representation of the rectangle. </summary>
	/// <returns>A string representation of the current rectangle. The string has the following form: "<see cref="P:System.Windows.Rect.X" />,<see cref="P:System.Windows.Rect.Y" />,<see cref="P:System.Windows.Rect.Width" />,<see cref="P:System.Windows.Rect.Height" />".</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Returns a string representation of the rectangle by using the specified format provider. </summary>
	/// <returns>A string representation of the current rectangle that is determined by the specified format provider.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>A string representation of the rectangle.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ConvertToString(format, provider);
	}

	internal string ConvertToString(string format, IFormatProvider provider)
	{
		if (IsEmpty)
		{
			return "Empty";
		}
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}", numericListSeparator, _x, _y, _width, _height);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Rect" /> structure that has the specified top-left corner location and the specified width and height. </summary>
	/// <param name="location">A point that specifies the location of the top-left corner of the rectangle.</param>
	/// <param name="size">A <see cref="T:System.Windows.Size" /> structure that specifies the width and height of the rectangle.</param>
	public Rect(Point location, Size size)
	{
		if (size.IsEmpty)
		{
			this = s_empty;
			return;
		}
		_x = location._x;
		_y = location._y;
		_width = size._width;
		_height = size._height;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Rect" /> structure that has the specified x-coordinate, y-coordinate, width, and height. </summary>
	/// <param name="x">The x-coordinate of the top-left corner of the rectangle.</param>
	/// <param name="y">The y-coordinate of the top-left corner of the rectangle.</param>
	/// <param name="width">The width of the rectangle.</param>
	/// <param name="height">The height of the rectangle.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="width" /> is a negative value.-or-<paramref name="height" /> is a negative value.</exception>
	public Rect(double x, double y, double width, double height)
	{
		if (width < 0.0 || height < 0.0)
		{
			throw new ArgumentException(SR.Size_WidthAndHeightCannotBeNegative);
		}
		_x = x;
		_y = y;
		_width = width;
		_height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Rect" /> structure that is exactly large enough to contain the two specified points. </summary>
	/// <param name="point1">The first point that the new rectangle must contain.</param>
	/// <param name="point2">The second point that the new rectangle must contain.</param>
	public Rect(Point point1, Point point2)
	{
		_x = Math.Min(point1._x, point2._x);
		_y = Math.Min(point1._y, point2._y);
		_width = Math.Max(Math.Max(point1._x, point2._x) - _x, 0.0);
		_height = Math.Max(Math.Max(point1._y, point2._y) - _y, 0.0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Rect" /> structure that is exactly large enough to contain the specified point and the sum of the specified point and the specified vector. </summary>
	/// <param name="point">The first point the rectangle must contain.</param>
	/// <param name="vector">The amount to offset the specified point. The resulting rectangle will be exactly large enough to contain both points.</param>
	public Rect(Point point, Vector vector)
		: this(point, point + vector)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Rect" /> structure that is of the specified size and is located at (0,0).  </summary>
	/// <param name="size">A <see cref="T:System.Windows.Size" /> structure that specifies the width and height of the rectangle.</param>
	public Rect(Size size)
	{
		if (size.IsEmpty)
		{
			this = s_empty;
			return;
		}
		_x = (_y = 0.0);
		_width = size.Width;
		_height = size.Height;
	}

	/// <summary>Indicates whether the rectangle contains the specified point.</summary>
	/// <returns>true if the rectangle contains the specified point; otherwise, false.</returns>
	/// <param name="point">The point to check.</param>
	public bool Contains(Point point)
	{
		return Contains(point._x, point._y);
	}

	/// <summary>Indicates whether the rectangle contains the specified x-coordinate and y-coordinate. </summary>
	/// <returns>true if (<paramref name="x" />, <paramref name="y" />) is contained by the rectangle; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the point to check.</param>
	/// <param name="y">The y-coordinate of the point to check.</param>
	public bool Contains(double x, double y)
	{
		if (IsEmpty)
		{
			return false;
		}
		return ContainsInternal(x, y);
	}

	/// <summary>Indicates whether the rectangle contains the specified rectangle. </summary>
	/// <returns>true if <paramref name="rect" /> is entirely contained by the rectangle; otherwise, false.</returns>
	/// <param name="rect">The rectangle to check.</param>
	public bool Contains(Rect rect)
	{
		if (IsEmpty || rect.IsEmpty)
		{
			return false;
		}
		if (_x <= rect._x && _y <= rect._y && _x + _width >= rect._x + rect._width)
		{
			return _y + _height >= rect._y + rect._height;
		}
		return false;
	}

	/// <summary>Indicates whether the specified rectangle intersects with the current rectangle. </summary>
	/// <returns>true if the specified rectangle intersects with the current rectangle; otherwise, false.</returns>
	/// <param name="rect">The rectangle to check.</param>
	public bool IntersectsWith(Rect rect)
	{
		if (IsEmpty || rect.IsEmpty)
		{
			return false;
		}
		if (rect.Left <= Right && rect.Right >= Left && rect.Top <= Bottom)
		{
			return rect.Bottom >= Top;
		}
		return false;
	}

	/// <summary>Finds the intersection of the current rectangle and the specified rectangle, and stores the result as the current rectangle. </summary>
	/// <param name="rect">The rectangle to intersect with the current rectangle.</param>
	public void Intersect(Rect rect)
	{
		if (!IntersectsWith(rect))
		{
			this = Empty;
			return;
		}
		double num = Math.Max(Left, rect.Left);
		double num2 = Math.Max(Top, rect.Top);
		_width = Math.Max(Math.Min(Right, rect.Right) - num, 0.0);
		_height = Math.Max(Math.Min(Bottom, rect.Bottom) - num2, 0.0);
		_x = num;
		_y = num2;
	}

	/// <summary>Returns the intersection of the specified rectangles. </summary>
	/// <returns>The intersection of the two rectangles, or <see cref="P:System.Windows.Rect.Empty" /> if no intersection exists.</returns>
	/// <param name="rect1">The first rectangle to compare.</param>
	/// <param name="rect2">The second rectangle to compare.</param>
	public static Rect Intersect(Rect rect1, Rect rect2)
	{
		rect1.Intersect(rect2);
		return rect1;
	}

	/// <summary>Expands the current rectangle exactly enough to contain the specified rectangle. </summary>
	/// <param name="rect">The rectangle to include.</param>
	public void Union(Rect rect)
	{
		if (IsEmpty)
		{
			this = rect;
		}
		else if (!rect.IsEmpty)
		{
			double num = Math.Min(Left, rect.Left);
			double num2 = Math.Min(Top, rect.Top);
			if (rect.Width == double.PositiveInfinity || Width == double.PositiveInfinity)
			{
				_width = double.PositiveInfinity;
			}
			else
			{
				double num3 = Math.Max(Right, rect.Right);
				_width = Math.Max(num3 - num, 0.0);
			}
			if (rect.Height == double.PositiveInfinity || Height == double.PositiveInfinity)
			{
				_height = double.PositiveInfinity;
			}
			else
			{
				double num4 = Math.Max(Bottom, rect.Bottom);
				_height = Math.Max(num4 - num2, 0.0);
			}
			_x = num;
			_y = num2;
		}
	}

	/// <summary>Creates a rectangle that is exactly large enough to contain the two specified rectangles. </summary>
	/// <returns>The resulting rectangle.</returns>
	/// <param name="rect1">The first rectangle to include.</param>
	/// <param name="rect2">The second rectangle to include.</param>
	public static Rect Union(Rect rect1, Rect rect2)
	{
		rect1.Union(rect2);
		return rect1;
	}

	/// <summary>Expands the current rectangle exactly enough to contain the specified point. </summary>
	/// <param name="point">The point to include.</param>
	public void Union(Point point)
	{
		Union(new Rect(point, point));
	}

	/// <summary>Creates a rectangle that is exactly large enough to include the specified rectangle and the specified point. </summary>
	/// <returns>A rectangle that is exactly large enough to contain the specified rectangle and the specified point.</returns>
	/// <param name="rect">The rectangle to include.</param>
	/// <param name="point">The point to include.</param>
	public static Rect Union(Rect rect, Point point)
	{
		rect.Union(new Rect(point, point));
		return rect;
	}

	/// <summary>Moves the rectangle by the specified vector. </summary>
	/// <param name="offsetVector">A vector that specifies the horizontal and vertical amounts to move the rectangle.</param>
	/// <exception cref="T:System.InvalidOperationException">This method is called on the <see cref="P:System.Windows.Rect.Empty" /> rectangle.</exception>
	public void Offset(Vector offsetVector)
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(SR.Rect_CannotCallMethod);
		}
		_x += offsetVector._x;
		_y += offsetVector._y;
	}

	/// <summary>Moves the rectangle by the specified horizontal and vertical amounts. </summary>
	/// <param name="offsetX">The amount to move the rectangle horizontally.</param>
	/// <param name="offsetY">The amount to move the rectangle vertically.</param>
	/// <exception cref="T:System.InvalidOperationException">This method is called on the <see cref="P:System.Windows.Rect.Empty" /> rectangle.</exception>
	public void Offset(double offsetX, double offsetY)
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(SR.Rect_CannotCallMethod);
		}
		_x += offsetX;
		_y += offsetY;
	}

	/// <summary>Returns a rectangle that is offset from the specified rectangle by using the specified vector. </summary>
	/// <returns>The resulting rectangle.</returns>
	/// <param name="rect">The original rectangle.</param>
	/// <param name="offsetVector">A vector that specifies the horizontal and vertical offsets for the new rectangle.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="rect" /> is <see cref="P:System.Windows.Rect.Empty" />.</exception>
	public static Rect Offset(Rect rect, Vector offsetVector)
	{
		rect.Offset(offsetVector.X, offsetVector.Y);
		return rect;
	}

	/// <summary>Returns a rectangle that is offset from the specified rectangle by using the specified horizontal and vertical amounts. </summary>
	/// <returns>The resulting rectangle.</returns>
	/// <param name="rect">The rectangle to move.</param>
	/// <param name="offsetX">The horizontal offset for the new rectangle.</param>
	/// <param name="offsetY">The vertical offset for the new rectangle.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="rect" /> is <see cref="P:System.Windows.Rect.Empty" />.</exception>
	public static Rect Offset(Rect rect, double offsetX, double offsetY)
	{
		rect.Offset(offsetX, offsetY);
		return rect;
	}

	/// <summary>Expands the rectangle by using the specified <see cref="T:System.Windows.Size" />, in all directions. </summary>
	/// <param name="size">Specifies the amount to expand the rectangle. The <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Width" /> property specifies the amount to increase the rectangle's <see cref="P:System.Windows.Rect.Left" /> and <see cref="P:System.Windows.Rect.Right" /> properties. The <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Height" /> property specifies the amount to increase the rectangle's <see cref="P:System.Windows.Rect.Top" /> and <see cref="P:System.Windows.Rect.Bottom" /> properties. </param>
	/// <exception cref="T:System.InvalidOperationException">This method is called on the <see cref="P:System.Windows.Rect.Empty" /> rectangle.</exception>
	public void Inflate(Size size)
	{
		Inflate(size._width, size._height);
	}

	/// <summary>Expands or shrinks the rectangle by using the specified width and height amounts, in all directions. </summary>
	/// <param name="width">The amount by which to expand or shrink the left and right sides of the rectangle.</param>
	/// <param name="height">The amount by which to expand or shrink the top and bottom sides of the rectangle.</param>
	/// <exception cref="T:System.InvalidOperationException">This method is called on the <see cref="P:System.Windows.Rect.Empty" /> rectangle.</exception>
	public void Inflate(double width, double height)
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(SR.Rect_CannotCallMethod);
		}
		_x -= width;
		_y -= height;
		_width += width;
		_width += width;
		_height += height;
		_height += height;
		if (!(_width >= 0.0) || !(_height >= 0.0))
		{
			this = s_empty;
		}
	}

	/// <summary>Returns the rectangle that results from expanding the specified rectangle by the specified <see cref="T:System.Windows.Size" />, in all directions. </summary>
	/// <returns>The resulting rectangle.</returns>
	/// <param name="rect">The <see cref="T:System.Windows.Rect" /> structure to modify.</param>
	/// <param name="size">Specifies the amount to expand the rectangle. The <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Width" /> property specifies the amount to increase the rectangle's <see cref="P:System.Windows.Rect.Left" /> and <see cref="P:System.Windows.Rect.Right" /> properties. The <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Height" /> property specifies the amount to increase the rectangle's <see cref="P:System.Windows.Rect.Top" /> and <see cref="P:System.Windows.Rect.Bottom" /> properties.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="rect" /> is an <see cref="P:System.Windows.Rect.Empty" /> rectangle.</exception>
	public static Rect Inflate(Rect rect, Size size)
	{
		rect.Inflate(size._width, size._height);
		return rect;
	}

	/// <summary>Creates a rectangle that results from expanding or shrinking the specified rectangle by the specified width and height amounts, in all directions. </summary>
	/// <returns>The resulting rectangle. </returns>
	/// <param name="rect">The <see cref="T:System.Windows.Rect" /> structure to modify.</param>
	/// <param name="width">The amount by which to expand or shrink the left and right sides of the rectangle.</param>
	/// <param name="height">The amount by which to expand or shrink the top and bottom sides of the rectangle.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="rect" /> is an <see cref="P:System.Windows.Rect.Empty" /> rectangle.</exception>
	public static Rect Inflate(Rect rect, double width, double height)
	{
		rect.Inflate(width, height);
		return rect;
	}

	/// <summary>Returns the rectangle that results from applying the specified matrix to the specified rectangle. </summary>
	/// <returns>The rectangle that results from the operation.</returns>
	/// <param name="rect">A rectangle that is the basis for the transformation.</param>
	/// <param name="matrix">A matrix that specifies the transformation to apply.</param>
	public static Rect Transform(Rect rect, Matrix matrix)
	{
		MatrixUtil.TransformRect(ref rect, ref matrix);
		return rect;
	}

	/// <summary>Transforms the rectangle by applying the specified matrix. </summary>
	/// <param name="matrix">A matrix that specifies the transformation to apply.</param>
	public void Transform(Matrix matrix)
	{
		MatrixUtil.TransformRect(ref this, ref matrix);
	}

	/// <summary>Multiplies the size of the current rectangle by the specified x and y values.</summary>
	/// <param name="scaleX">The scale factor in the x-direction.</param>
	/// <param name="scaleY">The scale factor in the y-direction.</param>
	public void Scale(double scaleX, double scaleY)
	{
		if (!IsEmpty)
		{
			_x *= scaleX;
			_y *= scaleY;
			_width *= scaleX;
			_height *= scaleY;
			if (scaleX < 0.0)
			{
				_x += _width;
				_width *= -1.0;
			}
			if (scaleY < 0.0)
			{
				_y += _height;
				_height *= -1.0;
			}
		}
	}

	private bool ContainsInternal(double x, double y)
	{
		if (x >= _x && x - _width <= _x && y >= _y)
		{
			return y - _height <= _y;
		}
		return false;
	}

	private static Rect CreateEmptyRect()
	{
		Rect result = default(Rect);
		result._x = double.PositiveInfinity;
		result._y = double.PositiveInfinity;
		result._width = double.NegativeInfinity;
		result._height = double.NegativeInfinity;
		return result;
	}
}
