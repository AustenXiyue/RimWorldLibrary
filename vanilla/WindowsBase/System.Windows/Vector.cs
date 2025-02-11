using System.ComponentModel;
using System.Windows.Converters;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows;

/// <summary>Represents a displacement in 2-D space. </summary>
[Serializable]
[TypeConverter(typeof(VectorConverter))]
[ValueSerializer(typeof(VectorValueSerializer))]
public struct Vector : IFormattable
{
	internal double _x;

	internal double _y;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Vector.X" /> component of this vector. </summary>
	/// <returns>The <see cref="P:System.Windows.Vector.X" /> component of this vector. The default value is 0.</returns>
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

	/// <summary>Gets or sets the <see cref="P:System.Windows.Vector.Y" /> component of this vector. </summary>
	/// <returns>The <see cref="P:System.Windows.Vector.Y" /> component of this vector. The default value is 0.</returns>
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

	/// <summary>Gets the length of this vector. </summary>
	/// <returns>The length of this vector. </returns>
	public double Length => Math.Sqrt(_x * _x + _y * _y);

	/// <summary>Gets the square of the length of this vector. </summary>
	/// <returns>The square of the <see cref="P:System.Windows.Vector.Length" /> of this vector.</returns>
	public double LengthSquared => _x * _x + _y * _y;

	/// <summary> Compares two vectors for equality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> components of <paramref name="vector1" /> and <paramref name="vector2" /> are equal; otherwise, false.</returns>
	/// <param name="vector1">The first vector to compare.</param>
	/// <param name="vector2">The second vector to compare.</param>
	public static bool operator ==(Vector vector1, Vector vector2)
	{
		if (vector1.X == vector2.X)
		{
			return vector1.Y == vector2.Y;
		}
		return false;
	}

	/// <summary>Compares two vectors for inequality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> components of <paramref name="vector1" /> and <paramref name="vector2" /> are different; otherwise, false.</returns>
	/// <param name="vector1">The first vector to compare.</param>
	/// <param name="vector2">The second vector to compare.</param>
	public static bool operator !=(Vector vector1, Vector vector2)
	{
		return !(vector1 == vector2);
	}

	/// <summary>Compares the two specified vectors for equality.</summary>
	/// <returns>true if t he <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> components of <paramref name="vector1" /> and <paramref name="vector2" /> are equal; otherwise, false.</returns>
	/// <param name="vector1">The first vector to compare.</param>
	/// <param name="vector2">The second vector to compare.</param>
	public static bool Equals(Vector vector1, Vector vector2)
	{
		if (vector1.X.Equals(vector2.X))
		{
			return vector1.Y.Equals(vector2.Y);
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is a <see cref="T:System.Windows.Vector" /> structure and, if it is, whether it has the same <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values as this vector.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Windows.Vector" /> and has the same <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values as this vector; otherwise, false.</returns>
	/// <param name="o">The vector to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Vector vector))
		{
			return false;
		}
		return Equals(this, vector);
	}

	/// <summary> Compares two vectors for equality.</summary>
	/// <returns>true if <paramref name="value" /> has the same <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values as this vector; otherwise, false.</returns>
	/// <param name="value">The vector to compare with this vector.</param>
	public bool Equals(Vector value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns the hash code for this vector. </summary>
	/// <returns>The hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode();
	}

	/// <summary>Converts a string representation of a vector into the equivalent <see cref="T:System.Windows.Vector" /> structure. </summary>
	/// <returns>The equivalent <see cref="T:System.Windows.Vector" /> structure.</returns>
	/// <param name="source">The string representation of the vector.</param>
	public static Vector Parse(string source)
	{
		IFormatProvider invariantEnglishUS = TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string value = tokenizerHelper.NextTokenRequired();
		Vector result = new Vector(Convert.ToDouble(value, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Returns the string representation of this <see cref="T:System.Windows.Vector" /> structure.</summary>
	/// <returns>A string that represents the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values of this <see cref="T:System.Windows.Vector" />.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Returns the string representation of this <see cref="T:System.Windows.Vector" /> structure with the specified formatting information. </summary>
	/// <returns>A string that represents the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values of this <see cref="T:System.Windows.Vector" />.</returns>
	/// <param name="provider">The culture-specific formatting information.</param>
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
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}", numericListSeparator, _x, _y);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Vector" /> structure. </summary>
	/// <param name="x">The <see cref="P:System.Windows.Vector.X" />-offset of the new <see cref="T:System.Windows.Vector" />.</param>
	/// <param name="y">The <see cref="P:System.Windows.Vector.Y" />-offset of the new <see cref="T:System.Windows.Vector" />.</param>
	public Vector(double x, double y)
	{
		_x = x;
		_y = y;
	}

	/// <summary> Normalizes this vector. </summary>
	public void Normalize()
	{
		this /= Math.Max(Math.Abs(_x), Math.Abs(_y));
		this /= Length;
	}

	/// <summary>Calculates the cross product of two vectors. </summary>
	/// <returns>The cross product of <paramref name="vector1" /> and <paramref name="vector2" />. The following formula is used to calculate the cross product: (Vector1.X * Vector2.Y) - (Vector1.Y * Vector2.X)</returns>
	/// <param name="vector1">The first vector to evaluate.</param>
	/// <param name="vector2">The second vector to evaluate.</param>
	public static double CrossProduct(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._y - vector1._y * vector2._x;
	}

	/// <summary>Retrieves the angle, expressed in degrees, between the two specified vectors. </summary>
	/// <returns>The angle, in degrees, between <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first vector to evaluate.</param>
	/// <param name="vector2">The second vector to evaluate.</param>
	public static double AngleBetween(Vector vector1, Vector vector2)
	{
		double y = vector1._x * vector2._y - vector2._x * vector1._y;
		double x = vector1._x * vector2._x + vector1._y * vector2._y;
		return Math.Atan2(y, x) * (180.0 / Math.PI);
	}

	/// <summary>Negates the specified vector. </summary>
	/// <returns>A vector with <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values opposite of the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values of <paramref name="vector" />.</returns>
	/// <param name="vector">The vector to negate.</param>
	public static Vector operator -(Vector vector)
	{
		return new Vector(0.0 - vector._x, 0.0 - vector._y);
	}

	/// <summary>Negates this vector. The vector has the same magnitude as before, but its direction is now opposite. </summary>
	public void Negate()
	{
		_x = 0.0 - _x;
		_y = 0.0 - _y;
	}

	/// <summary>Adds two vectors and returns the result as a vector. </summary>
	/// <returns>The sum of <paramref name="vector1" /> and <paramref name="vector2" />. </returns>
	/// <param name="vector1">The first vector to add.</param>
	/// <param name="vector2">The second vector to add.</param>
	public static Vector operator +(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x + vector2._x, vector1._y + vector2._y);
	}

	/// <summary>Adds two vectors and returns the result as a <see cref="T:System.Windows.Vector" /> structure. </summary>
	/// <returns>The sum of <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first vector to add.</param>
	/// <param name="vector2">The second vector to add.</param>
	public static Vector Add(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x + vector2._x, vector1._y + vector2._y);
	}

	/// <summary>Subtracts one specified vector from another. </summary>
	/// <returns>The difference between <paramref name="vector1" /> and <paramref name="vector2" />. </returns>
	/// <param name="vector1">The vector from which <paramref name="vector2" /> is subtracted. </param>
	/// <param name="vector2">The vector to subtract from <paramref name="vector1" />.</param>
	public static Vector operator -(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x - vector2._x, vector1._y - vector2._y);
	}

	/// <summary>Subtracts the specified vector from another specified vector. </summary>
	/// <returns>The difference between <paramref name="vector1" /> and <paramref name="vector2" />. </returns>
	/// <param name="vector1">The vector from which <paramref name="vector2" /> is subtracted.</param>
	/// <param name="vector2">The vector to subtract from <paramref name="vector1" />.</param>
	public static Vector Subtract(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x - vector2._x, vector1._y - vector2._y);
	}

	/// <summary> Translates a point by the specified vector and returns the resulting point. </summary>
	/// <returns>The result of translating <paramref name="point" /> by <paramref name="vector" />.</returns>
	/// <param name="vector">The vector used to translate <paramref name="point" />.</param>
	/// <param name="point">The point to translate.</param>
	public static Point operator +(Vector vector, Point point)
	{
		return new Point(point._x + vector._x, point._y + vector._y);
	}

	/// <summary>Translates the specified point by the specified vector and returns the resulting point.</summary>
	/// <returns>The result of translating <paramref name="point" /> by <paramref name="vector" />.</returns>
	/// <param name="vector">The amount to translate the specified point.</param>
	/// <param name="point">The point to translate.</param>
	public static Point Add(Vector vector, Point point)
	{
		return new Point(point._x + vector._x, point._y + vector._y);
	}

	/// <summary>Multiplies the specified vector by the specified scalar and returns the resulting vector. </summary>
	/// <returns>The result of multiplying <paramref name="vector" /> and <paramref name="scalar" />.</returns>
	/// <param name="vector">The vector to multiply.</param>
	/// <param name="scalar">The scalar to multiply.</param>
	public static Vector operator *(Vector vector, double scalar)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	/// <summary> Multiplies the specified vector by the specified scalar and returns the resulting <see cref="T:System.Windows.Vector" />. </summary>
	/// <returns>The result of multiplying <paramref name="vector" /> and <paramref name="scalar" />.</returns>
	/// <param name="vector">The vector to multiply.</param>
	/// <param name="scalar">The scalar to multiply.</param>
	public static Vector Multiply(Vector vector, double scalar)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	/// <summary> Multiplies the specified scalar by the specified vector and returns the resulting vector. </summary>
	/// <returns>The result of multiplying <paramref name="scalar" /> and <paramref name="vector" />.</returns>
	/// <param name="scalar">The scalar to multiply.</param>
	/// <param name="vector">The vector to multiply.</param>
	public static Vector operator *(double scalar, Vector vector)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	/// <summary> Multiplies the specified scalar by the specified vector and returns the resulting <see cref="T:System.Windows.Vector" />. </summary>
	/// <returns>The result of multiplying <paramref name="scalar" /> and <paramref name="vector" />.</returns>
	/// <param name="scalar">The scalar to multiply.</param>
	/// <param name="vector">The vector to multiply.</param>
	public static Vector Multiply(double scalar, Vector vector)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	/// <summary> Divides the specified vector by the specified scalar and returns the resulting vector.</summary>
	/// <returns>The result of dividing <paramref name="vector" /> by <paramref name="scalar" />.</returns>
	/// <param name="vector">The vector to divide.</param>
	/// <param name="scalar">The scalar by which <paramref name="vector" /> will be divided.</param>
	public static Vector operator /(Vector vector, double scalar)
	{
		return vector * (1.0 / scalar);
	}

	/// <summary>Divides the specified vector by the specified scalar and returns the result as a <see cref="T:System.Windows.Vector" />.</summary>
	/// <returns>The result of dividing <paramref name="vector" /> by <paramref name="scalar" />.</returns>
	/// <param name="vector">The vector structure to divide.</param>
	/// <param name="scalar">The amount by which <paramref name="vector" /> is divided.</param>
	public static Vector Divide(Vector vector, double scalar)
	{
		return vector * (1.0 / scalar);
	}

	/// <summary> Transforms the coordinate space of the specified vector using the specified <see cref="T:System.Windows.Media.Matrix" />. </summary>
	/// <returns>The result of transforming <paramref name="vector" /> by <paramref name="matrix" />.</returns>
	/// <param name="vector">The vector to transform.</param>
	/// <param name="matrix">The transformation to apply to <paramref name="vector" />.</param>
	public static Vector operator *(Vector vector, Matrix matrix)
	{
		return matrix.Transform(vector);
	}

	/// <summary>Transforms the coordinate space of the specified vector using the specified <see cref="T:System.Windows.Media.Matrix" />.</summary>
	/// <returns>The result of transforming <paramref name="vector" /> by <paramref name="matrix" />.</returns>
	/// <param name="vector">The vector structure to transform.</param>
	/// <param name="matrix">The transformation to apply to <paramref name="vector" />.</param>
	public static Vector Multiply(Vector vector, Matrix matrix)
	{
		return matrix.Transform(vector);
	}

	/// <summary> Calculates the dot product of the two specified vector structures and returns the result as a <see cref="T:System.Double" />.</summary>
	/// <returns>Returns a <see cref="T:System.Double" /> containing the scalar dot product of <paramref name="vector1" /> and <paramref name="vector2" />, which is calculated using the following formula:vector1.X * vector2.X + vector1.Y * vector2.Y</returns>
	/// <param name="vector1">The first vector to multiply.</param>
	/// <param name="vector2">The second vector to multiply.</param>
	public static double operator *(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._x + vector1._y * vector2._y;
	}

	/// <summary>Calculates the dot product of the two specified vectors and returns the result as a <see cref="T:System.Double" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> containing the scalar dot product of <paramref name="vector1" /> and <paramref name="vector2" />, which is calculated using the following formula: (vector1.X * vector2.X) + (vector1.Y * vector2.Y) </returns>
	/// <param name="vector1">The first vector to multiply.</param>
	/// <param name="vector2">The second vector structure to multiply.</param>
	public static double Multiply(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._x + vector1._y * vector2._y;
	}

	/// <summary>Calculates the determinant of two vectors.</summary>
	/// <returns>The determinant of <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first vector to evaluate.</param>
	/// <param name="vector2">The second vector to evaluate.</param>
	public static double Determinant(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._y - vector1._y * vector2._x;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Size" /> from the offsets of this vector.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> with a <see cref="P:System.Windows.Size.Width" /> equal to the absolute value of this vector's <see cref="P:System.Windows.Vector.X" /> property and a <see cref="P:System.Windows.Size.Height" /> equal to the absolute value of this vector's <see cref="P:System.Windows.Vector.Y" /> property.</returns>
	/// <param name="vector">The vector to convert.</param>
	public static explicit operator Size(Vector vector)
	{
		return new Size(Math.Abs(vector._x), Math.Abs(vector._y));
	}

	/// <summary>Creates a <see cref="T:System.Windows.Point" /> with the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> values of this vector. </summary>
	/// <returns>A point with <see cref="P:System.Windows.Point.X" />- and <see cref="P:System.Windows.Point.Y" />-coordinate values equal to the <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> offset values of <paramref name="vector" />.</returns>
	/// <param name="vector">The vector to convert.</param>
	public static explicit operator Point(Vector vector)
	{
		return new Point(vector._x, vector._y);
	}
}
