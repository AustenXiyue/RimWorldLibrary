using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;

namespace System.Windows.Media.Media3D;

/// <summary>Represents an x-, y-, and z-coordinate point in 3-D space. </summary>
[Serializable]
[TypeConverter(typeof(Point3DConverter))]
[ValueSerializer(typeof(Point3DValueSerializer))]
public struct Point3D : IFormattable
{
	internal double _x;

	internal double _y;

	internal double _z;

	/// <summary>Gets or sets the x-coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The x-coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
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

	/// <summary>Gets or sets the y-coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The y-coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
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

	/// <summary>Gets or sets the z-coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The z-coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <param name="x">The <see cref="P:System.Windows.Media.Media3D.Point3D.X" /> value of the new <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	/// <param name="y">The <see cref="P:System.Windows.Media.Media3D.Point3D.Y" /> value of the new <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	/// <param name="z">The <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> value of the new <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	public Point3D(double x, double y, double z)
	{
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>Translates the <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure by the specified amounts. </summary>
	/// <param name="offsetX">The amount to change the <see cref="P:System.Windows.Media.Media3D.Point3D.X" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	/// <param name="offsetY">The amount to change the <see cref="P:System.Windows.Media.Media3D.Point3D.Y" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	/// <param name="offsetZ">The amount to change the <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> coordinate of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	public void Offset(double offsetX, double offsetY, double offsetZ)
	{
		_x += offsetX;
		_y += offsetY;
		_z += offsetZ;
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure that is the sum of <paramref name="point" /> and <paramref name="vector" />.</returns>
	/// <param name="point">The point to add.</param>
	/// <param name="vector">The vector to add.</param>
	public static Point3D operator +(Point3D point, Vector3D vector)
	{
		return new Point3D(point._x + vector._x, point._y + vector._y, point._z + vector._z);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The sum of <paramref name="point" /> and <paramref name="vector" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to add.</param>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to add. </param>
	public static Point3D Add(Point3D point, Vector3D vector)
	{
		return new Point3D(point._x + vector._x, point._y + vector._y, point._z + vector._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The changed <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure, the result of subtracting <paramref name="vector" /> from <paramref name="point" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure from which to subtract vector.</param>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to subtract from point.</param>
	public static Point3D operator -(Point3D point, Vector3D vector)
	{
		return new Point3D(point._x - vector._x, point._y - vector._y, point._z - vector._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The difference between <paramref name="point" /> and <paramref name="vector" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure from which to subtract <paramref name="vector" />.</param>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to subtract from <paramref name="point" />.</param>
	public static Point3D Subtract(Point3D point, Vector3D vector)
	{
		return new Point3D(point._x - vector._x, point._y - vector._y, point._z - vector._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure that represents the difference between <paramref name="point1" /> and <paramref name="point2" />.</returns>
	/// <param name="point1">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure on which to perform subtraction.</param>
	/// <param name="point2">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to subtract from <paramref name="point1" />.</param>
	public static Vector3D operator -(Point3D point1, Point3D point2)
	{
		return new Vector3D(point1._x - point2._x, point1._y - point2._y, point1._z - point2._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure that represents the difference between <paramref name="point1" /> and <paramref name="point2" />.</returns>
	/// <param name="point1">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to be subtracted from.</param>
	/// <param name="point2">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to subtract from <paramref name="point1" />.</param>
	public static Vector3D Subtract(Point3D point1, Point3D point2)
	{
		Vector3D result = default(Vector3D);
		Subtract(ref point1, ref point2, out result);
		return result;
	}

	internal static void Subtract(ref Point3D p1, ref Point3D p2, out Vector3D result)
	{
		result._x = p1._x - p2._x;
		result._y = p1._y - p2._y;
		result._z = p1._z - p2._z;
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The result of transforming <paramref name="point" /> by using <paramref name="matrix" />.</returns>
	/// <param name="point">The point to transform.</param>
	/// <param name="matrix">The matrix that is used to transform <paramref name="point" />.</param>
	public static Point3D operator *(Point3D point, Matrix3D matrix)
	{
		return matrix.Transform(point);
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>A transformed <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure, the result of transforming <paramref name="point" /> by <paramref name="matrix" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to transform.</param>
	/// <param name="matrix">The <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure to use for the transformation.</param>
	public static Point3D Multiply(Point3D point, Matrix3D matrix)
	{
		return matrix.Transform(point);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure into a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The result of converting <paramref name="point" />.</returns>
	/// <param name="point">The point to convert.</param>
	public static explicit operator Vector3D(Point3D point)
	{
		return new Vector3D(point._x, point._y, point._z);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure into a <see cref="T:System.Windows.Media.Media3D.Point4D" /> structure. </summary>
	/// <returns>The result of converting <paramref name="point" />.</returns>
	/// <param name="point">The point to convert.</param>
	public static explicit operator Point4D(Point3D point)
	{
		return new Point4D(point._x, point._y, point._z, 1.0);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point3D" /> structures for equality. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> coordinates of <paramref name="point1" /> and <paramref name="point2" /> are equal; otherwise, false.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to compare.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to compare.</param>
	public static bool operator ==(Point3D point1, Point3D point2)
	{
		if (point1.X == point2.X && point1.Y == point2.Y)
		{
			return point1.Z == point2.Z;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point3D" /> structures for inequality. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> coordinates of <paramref name="point1" /> and <paramref name="point2" /> are different; otherwise, false.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to compare.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to compare.</param>
	public static bool operator !=(Point3D point1, Point3D point2)
	{
		return !(point1 == point2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point3D" /> structures for equality. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> values for <paramref name="point1" /> and <paramref name="point2" /> are equal; otherwise, false.</returns>
	/// <param name="point1">The first <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to compare.</param>
	/// <param name="point2">The second <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to compare.</param>
	public static bool Equals(Point3D point1, Point3D point2)
	{
		if (point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y))
		{
			return point1.Z.Equals(point2.Z);
		}
		return false;
	}

	/// <summary>Determines whether the specified object is a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure and if so, whether the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> properties of the specified <see cref="T:System.Object" /> are equal to the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> properties of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>true if the instances are equal; otherwise, false.true if <paramref name="o" /> is a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure and if it is also identical to this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure; otherwise, false.</returns>
	/// <param name="o">The object to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Point3D point))
		{
			return false;
		}
		return Equals(this, point);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Point3D" /> structures for equality.</summary>
	/// <returns>true if instances are equal; otherwise, false.</returns>
	/// <param name="value">The instance of <see cref="T:System.Windows.Media.Media3D.Point3D" /> to compare to this instance.</param>
	public bool Equals(Point3D value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>A hash code for this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}

	/// <summary>Converts a <see cref="T:System.String" /> representation of a 3-D point into the equivalent <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The equivalent <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the 3-D point.</param>
	public static Point3D Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string value = tokenizerHelper.NextTokenRequired();
		Point3D result = new Point3D(Convert.ToDouble(value, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> values of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Point3D.X" />, <see cref="P:System.Windows.Media.Media3D.Point3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Point3D.Z" /> values of this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
	/// <param name="provider">The culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return ConvertToString(null, provider);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
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
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}", numericListSeparator, _x, _y, _z);
	}
}
