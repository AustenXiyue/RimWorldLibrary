using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;

namespace System.Windows.Media.Media3D;

/// <summary>Represents an x-, y-, z-, and w-coordinate point in world space used in performing transformations with non-affine 3-D matrices. </summary>
[Serializable]
[TypeConverter(typeof(Point4DConverter))]
[ValueSerializer(typeof(Point4DValueSerializer))]
public struct Point4D : IFormattable
{
	internal double _x;

	internal double _y;

	internal double _z;

	internal double _w;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Point4D.X" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Point4D.X" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.  The default value is 0.</returns>
	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Point4D.Y" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Point4D.Y" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.  The default value is 0.</returns>
	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Point4D.Z" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Point4D.Z" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.  The default value is 0.</returns>
	public double Z
	{
		get
		{
			return _z;
		}
		set
		{
			_z = value;
		}
	}

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> component of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.  The default value is 0.</returns>
	public double W
	{
		get
		{
			return _w;
		}
		set
		{
			_w = value;
		}
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <param name="x">The x-coordinate of the new <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	/// <param name="y">The y-coordinate of the new <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	/// <param name="z">The z-coordinate of the new <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	/// <param name="w">The w-coordinate of the new <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	public Point4D(double x, double y, double z, double w)
	{
		_x = x;
		_y = y;
		_z = z;
		_w = w;
	}

	/// <summary>Translates the <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure by the specified amounts. </summary>
	/// <param name="deltaX">The amount to offset the <see cref="P:System.Windows.Media.Media3D.Point4D.X" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	/// <param name="deltaY">The amount to offset the <see cref="P:System.Windows.Media.Media3D.Point4D.Y" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	/// <param name="deltaZ">The amount to offset the <see cref="P:System.Windows.Media.Media3D.Point4D.Z" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	/// <param name="deltaW">The amount to offset the <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</param>
	public void Offset(double deltaX, double deltaY, double deltaZ, double deltaW)
	{
		_x += deltaX;
		_y += deltaY;
		_z += deltaZ;
		_w += deltaW;
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to a <see cref="T:System.Windows.Media.Media3D.Point4D" />.</summary>
	/// <returns>Returns the sum of <paramref name="point1" /> and <paramref name="point2" />.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to add.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to add.</param>
	public static Point4D operator +(Point4D point1, Point4D point2)
	{
		return new Point4D(point1._x + point2._x, point1._y + point2._y, point1._z + point2._z, point1._w + point2._w);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to a <see cref="T:System.Windows.Media.Media3D.Point4D" />.</summary>
	/// <returns>Returns the sum of <paramref name="point1" /> and <paramref name="point2" />.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to add.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to add.</param>
	public static Point4D Add(Point4D point1, Point4D point2)
	{
		return new Point4D(point1._x + point2._x, point1._y + point2._y, point1._z + point2._z, point1._w + point2._w);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>Returns the difference between <paramref name="point1" /> and <paramref name="point2" />.</returns>
	/// <param name="point1">The <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to be subtracted from.</param>
	/// <param name="point2">The <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to subtract from <paramref name="point1" />.</param>
	public static Point4D operator -(Point4D point1, Point4D point2)
	{
		return new Point4D(point1._x - point2._x, point1._y - point2._y, point1._z - point2._z, point1._w - point2._w);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>Returns the difference between <paramref name="point1" /> and <paramref name="point2" />.</returns>
	/// <param name="point1">The <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to be subtracted from.</param>
	/// <param name="point2">The <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to subtract from <paramref name="point1" />.</param>
	public static Point4D Subtract(Point4D point1, Point4D point2)
	{
		return new Point4D(point1._x - point2._x, point1._y - point2._y, point1._z - point2._z, point1._w - point2._w);
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>Returns the result of transforming <paramref name="point" /> and <paramref name="matrix" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to transform.</param>
	/// <param name="matrix">The transformation <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</param>
	public static Point4D operator *(Point4D point, Matrix3D matrix)
	{
		return matrix.Transform(point);
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>Returns the result of transforming <paramref name="point" /> and <paramref name="matrix" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to transform.</param>
	/// <param name="matrix">The transformation <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</param>
	public static Point4D Multiply(Point4D point, Matrix3D matrix)
	{
		return matrix.Transform(point);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point4D" /> structures for equality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.Z" /> coordinates of <paramref name="point4D1" /> and <paramref name="point4D2" /> are equal; otherwise, false.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to compare.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to compare.</param>
	public static bool operator ==(Point4D point1, Point4D point2)
	{
		if (point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z)
		{
			return point1.W == point2.W;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point4D" /> structures for inequality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Z" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> coordinates of <paramref name="point4D1" /> and <paramref name="point4D2" /> are different; otherwise, false.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to compare.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to compare.</param>
	public static bool operator !=(Point4D point1, Point4D point2)
	{
		return !(point1 == point2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point4D" /> structures for equality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.Z" /> components of <paramref name="point3D1" /> and <paramref name="point3D2" /> are equal; false otherwise.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to compare.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure to compare.</param>
	public static bool Equals(Point4D point1, Point4D point2)
	{
		if (point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y) && point1.Z.Equals(point2.Z))
		{
			return point1.W.Equals(point2.W);
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure and if the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Z" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> properties of the specified <see cref="T:System.Object" /> are equal to the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Z" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.W" />  properties of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</summary>
	/// <returns>true if instances are equal; otherwise, false.true if <paramref name="o" /> (the passed <see cref="T:System.Object" />) is a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure and is identical with this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure; false otherwise.</returns>
	/// <param name="o">The object to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Point4D point))
		{
			return false;
		}
		return Equals(this, point);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point4D" /> structures for equality.</summary>
	/// <returns>true if instances are equal; otherwise, false.</returns>
	/// <param name="value">The instance of Point4D to compare to this instance.</param>
	public bool Equals(Point4D value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>Returns a hash code for this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</returns>
	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
	}

	/// <summary>Converts a <see cref="T:System.String" /> representation of a point4D structure into the equivalent <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>Returns the equivalent <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the point4D structure.</param>
	public static Point4D Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string value = tokenizerHelper.NextTokenRequired();
		Point4D result = new Point4D(Convert.ToDouble(value, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Z" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> values of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Point4D.X" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Y" />, <see cref="P:System.Windows.Media.Media3D.Point4D.Z" />, and <see cref="P:System.Windows.Media.Media3D.Point4D.W" /> values of this <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return ConvertToString(null, provider);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
	/// <returns>A string containing the value of the current instance in the specified format. </returns>
	/// <param name="format">The string specifying the format to use. -or- null to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
	/// <param name="provider">The IFormatProvider to use to format the value. -or- null to obtain the numeric format information from the current locale setting of the operating system.</param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ConvertToString(format, provider);
	}

	internal string ConvertToString(string format, IFormatProvider provider)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}", numericListSeparator, _x, _y, _z, _w);
	}
}
