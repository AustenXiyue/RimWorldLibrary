using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a displacement in 3-D space. </summary>
[Serializable]
[TypeConverter(typeof(Vector3DConverter))]
[ValueSerializer(typeof(Vector3DValueSerializer))]
public struct Vector3D : IFormattable
{
	internal double _x;

	internal double _y;

	internal double _z;

	/// <summary>Gets the length of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The length of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </returns>
	public double Length => Math.Sqrt(_x * _x + _y * _y + _z * _z);

	/// <summary>Gets the square of the length of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The square of the length of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</returns>
	public double LengthSquared => _x * _x + _y * _y + _z * _z;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" /> component of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Vector3D.X" /> component of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. The default value is 0.</returns>
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

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" /> component of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" /> component of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. The default value is 0.</returns>
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

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> component of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> component of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. The default value is 0.</returns>
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

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <param name="x">The new <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure's <see cref="P:System.Windows.Media.Media3D.Vector3D.X" /> value.</param>
	/// <param name="y">The new <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure's <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" /> value.</param>
	/// <param name="z">The new <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure's <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> value.</param>
	public Vector3D(double x, double y, double z)
	{
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>Normalizes the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	public void Normalize()
	{
		double num = Math.Abs(_x);
		double num2 = Math.Abs(_y);
		double num3 = Math.Abs(_z);
		if (num2 > num)
		{
			num = num2;
		}
		if (num3 > num)
		{
			num = num3;
		}
		_x /= num;
		_y /= num;
		_z /= num;
		double num4 = Math.Sqrt(_x * _x + _y * _y + _z * _z);
		this /= num4;
	}

	/// <summary>Retrieves the angle required to rotate the first specified  <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure into the second specified  <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The angle in degrees needed to rotate <paramref name="vector1" /> into <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to evaluate.</param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to evaluate.</param>
	public static double AngleBetween(Vector3D vector1, Vector3D vector2)
	{
		vector1.Normalize();
		vector2.Normalize();
		double radians = ((!(DotProduct(vector1, vector2) < 0.0)) ? (2.0 * Math.Asin((vector1 - vector2).Length / 2.0)) : (Math.PI - 2.0 * Math.Asin((-vector1 - vector2).Length / 2.0)));
		return M3DUtil.RadiansToDegrees(radians);
	}

	/// <summary>Negates a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure with <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> values opposite of the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> values of <paramref name="vector" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to negate.</param>
	public static Vector3D operator -(Vector3D vector)
	{
		return new Vector3D(0.0 - vector._x, 0.0 - vector._y, 0.0 - vector._z);
	}

	/// <summary>Negates a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	public void Negate()
	{
		_x = 0.0 - _x;
		_y = 0.0 - _y;
		_z = 0.0 - _z;
	}

	/// <summary>Adds two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The sum of <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to add.</param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to add.</param>
	public static Vector3D operator +(Vector3D vector1, Vector3D vector2)
	{
		return new Vector3D(vector1._x + vector2._x, vector1._y + vector2._y, vector1._z + vector2._z);
	}

	/// <summary>Adds two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The sum of <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to add.</param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to add.</param>
	public static Vector3D Add(Vector3D vector1, Vector3D vector2)
	{
		return new Vector3D(vector1._x + vector2._x, vector1._y + vector2._y, vector1._z + vector2._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The result of subtracting <paramref name="vector2" /> from <paramref name="vector1" />.</returns>
	/// <param name="vector1">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to be subtracted from.</param>
	/// <param name="vector2">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to subtract from <paramref name="vector1" />.</param>
	public static Vector3D operator -(Vector3D vector1, Vector3D vector2)
	{
		return new Vector3D(vector1._x - vector2._x, vector1._y - vector2._y, vector1._z - vector2._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The result of subtracting <paramref name="vector2" /> from <paramref name="vector1" />.</returns>
	/// <param name="vector1">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to be subtracted from.</param>
	/// <param name="vector2">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to subtract from <paramref name="vector1" />.</param>
	public static Vector3D Subtract(Vector3D vector1, Vector3D vector2)
	{
		return new Vector3D(vector1._x - vector2._x, vector1._y - vector2._y, vector1._z - vector2._z);
	}

	/// <summary>Translates the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The result of translating <paramref name="point" /> by <paramref name="vector" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure used to translate the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to be translated.</param>
	public static Point3D operator +(Vector3D vector, Point3D point)
	{
		return new Point3D(vector._x + point._x, vector._y + point._y, vector._z + point._z);
	}

	/// <summary>Translates the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The result of translating <paramref name="point" /> by <paramref name="vector" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure used to translate the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</param>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to be translated.</param>
	public static Point3D Add(Vector3D vector, Point3D point)
	{
		return new Point3D(vector._x + point._x, vector._y + point._y, vector._z + point._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The result of subtracting <paramref name="point" /> from <paramref name="vector" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to be subtracted from.</param>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to subtract from <paramref name="vector" />.</param>
	public static Point3D operator -(Vector3D vector, Point3D point)
	{
		return new Point3D(vector._x - point._x, vector._y - point._y, vector._z - point._z);
	}

	/// <summary>Subtracts a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure from a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The result of subtracting <paramref name="point" /> from <paramref name="vector" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to be subtracted from.</param>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to subtract from <paramref name="vector" />.</param>
	public static Point3D Subtract(Vector3D vector, Point3D point)
	{
		return new Point3D(vector._x - point._x, vector._y - point._y, vector._z - point._z);
	}

	/// <summary>Multiplies the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure by the specified scalar and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>The result of multiplying <paramref name="vector" /> and <paramref name="scalar" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to multiply.</param>
	/// <param name="scalar">The scalar to multiply.</param>
	public static Vector3D operator *(Vector3D vector, double scalar)
	{
		return new Vector3D(vector._x * scalar, vector._y * scalar, vector._z * scalar);
	}

	/// <summary>Multiplies the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure by the specified scalar and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>The result of multiplying <paramref name="vector" /> and <paramref name="scalar" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to multiply.</param>
	/// <param name="scalar">The scalar to multiply.</param>
	public static Vector3D Multiply(Vector3D vector, double scalar)
	{
		return new Vector3D(vector._x * scalar, vector._y * scalar, vector._z * scalar);
	}

	/// <summary>Multiplies the specified scalar by the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>The result of multiplying <paramref name="scalar" /> and <paramref name="vector" />.</returns>
	/// <param name="scalar">The scalar to multiply.</param>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to multiply.</param>
	public static Vector3D operator *(double scalar, Vector3D vector)
	{
		return new Vector3D(vector._x * scalar, vector._y * scalar, vector._z * scalar);
	}

	/// <summary>Multiplies the specified scalar by the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>The result of multiplying <paramref name="scalar" /> and <paramref name="vector" />.</returns>
	/// <param name="scalar">The scalar to multiply.</param>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to multiply.</param>
	public static Vector3D Multiply(double scalar, Vector3D vector)
	{
		return new Vector3D(vector._x * scalar, vector._y * scalar, vector._z * scalar);
	}

	/// <summary> Divides the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure by the specified scalar and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>The result of dividing <paramref name="vector" /> by <paramref name="scalar" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to be divided.</param>
	/// <param name="scalar">The scalar to divide <paramref name="vector" /> by.</param>
	public static Vector3D operator /(Vector3D vector, double scalar)
	{
		return vector * (1.0 / scalar);
	}

	/// <summary>Divides the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure by the specified scalar and returns the result as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>The result of dividing <paramref name="vector" /> by <paramref name="scalar" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to be divided.</param>
	/// <param name="scalar">The scalar to divide <paramref name="vector" /> by.</param>
	public static Vector3D Divide(Vector3D vector, double scalar)
	{
		return vector * (1.0 / scalar);
	}

	/// <summary>Transforms the coordinate space of the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure using the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The result of transforming <paramref name="vector" /> by <paramref name="matrix" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to transform.</param>
	/// <param name="matrix">The transformation to apply to the <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</param>
	public static Vector3D operator *(Vector3D vector, Matrix3D matrix)
	{
		return matrix.Transform(vector);
	}

	/// <summary>Transforms the coordinate space of the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure using the specified <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>Returns the result of transforming <paramref name="vector" /> by <paramref name="matrix3D" />.</returns>
	/// <param name="vector">The <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to transform.</param>
	/// <param name="matrix">The transformation to apply to the <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</param>
	public static Vector3D Multiply(Vector3D vector, Matrix3D matrix)
	{
		return matrix.Transform(vector);
	}

	/// <summary>Calculates the dot product of two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures. </summary>
	/// <returns>The dot product of <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to evaluate.  </param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to evaluate.</param>
	public static double DotProduct(Vector3D vector1, Vector3D vector2)
	{
		return DotProduct(ref vector1, ref vector2);
	}

	internal static double DotProduct(ref Vector3D vector1, ref Vector3D vector2)
	{
		return vector1._x * vector2._x + vector1._y * vector2._y + vector1._z * vector2._z;
	}

	/// <summary>Calculates the cross product of two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures. </summary>
	/// <returns>The cross product of <paramref name="vector1" /> and <paramref name="vector2" />.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to evaluate.</param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to evaluate.</param>
	public static Vector3D CrossProduct(Vector3D vector1, Vector3D vector2)
	{
		CrossProduct(ref vector1, ref vector2, out var result);
		return result;
	}

	internal static void CrossProduct(ref Vector3D vector1, ref Vector3D vector2, out Vector3D result)
	{
		result._x = vector1._y * vector2._z - vector1._z * vector2._y;
		result._y = vector1._z * vector2._x - vector1._x * vector2._z;
		result._z = vector1._x * vector2._y - vector1._y * vector2._x;
	}

	/// <summary>Converts a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure into a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The result of converting <paramref name="vector" />.</returns>
	/// <param name="vector">The vector to convert.</param>
	public static explicit operator Point3D(Vector3D vector)
	{
		return new Point3D(vector._x, vector._y, vector._z);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure into a <see cref="T:System.Windows.Media.Media3D.Size3D" />.</summary>
	/// <returns>The result of converting <paramref name="vector" />.</returns>
	/// <param name="vector">The vector to convert.</param>
	public static explicit operator Size3D(Vector3D vector)
	{
		return new Size3D(Math.Abs(vector._x), Math.Abs(vector._y), Math.Abs(vector._z));
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures for equality.   </summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> components of <paramref name="vector1" /> and <paramref name="vector2" /> are equal; false otherwise.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to compare.</param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to compare.</param>
	public static bool operator ==(Vector3D vector1, Vector3D vector2)
	{
		if (vector1.X == vector2.X && vector1.Y == vector2.Y)
		{
			return vector1.Z == vector2.Z;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures for inequality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> components of <paramref name="vector3D1" /> and <paramref name="vector3D2" /> are different; false otherwise.</returns>
	/// <param name="vector1">The first <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to compare.</param>
	/// <param name="vector2">The second <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure to compare.</param>
	public static bool operator !=(Vector3D vector1, Vector3D vector2)
	{
		return !(vector1 == vector2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures for equality.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> components of <paramref name="vector1" /> and <paramref name="vector2" /> are equal; false otherwise.</returns>
	/// <param name="vector1">First <see cref="T:System.Windows.Media.Media3D.Vector3D" />  to compare.</param>
	/// <param name="vector2">Second <see cref="T:System.Windows.Media.Media3D.Vector3D" />  to compare.</param>
	public static bool Equals(Vector3D vector1, Vector3D vector2)
	{
		if (vector1.X.Equals(vector2.X) && vector1.Y.Equals(vector2.Y))
		{
			return vector1.Z.Equals(vector2.Z);
		}
		return false;
	}

	/// <summary>Determines whether the specified object is a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure and whether the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> properties of the specified <see cref="T:System.Object" /> are equal to the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> properties of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure and is identical with this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure; false otherwise.</returns>
	/// <param name="o">The object to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Vector3D vector))
		{
			return false;
		}
		return Equals(this, vector);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structures for equality. </summary>
	/// <returns>true if instances are equal; otherwise, false.</returns>
	/// <param name="value">The instance of Vector to compare against this instance.</param>
	public bool Equals(Vector3D value)
	{
		return Equals(this, value);
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>A hash code for this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</returns>
	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}

	/// <summary>Converts a <see cref="T:System.String" /> representation of a 3-D vector into the equivalent <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The equivalent <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the 3-D vector.</param>
	public static Vector3D Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string value = tokenizerHelper.NextTokenRequired();
		Vector3D result = new Vector3D(Convert.ToDouble(value, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS));
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>A string containing the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> values of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Vector3D.X" />, <see cref="P:System.Windows.Media.Media3D.Vector3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Vector3D.Z" /> values of this <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return ConvertToString(null, provider);
	}

	/// <summary>This member is part of the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly by your code. For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
	/// <returns>String representation of this object.</returns>
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
