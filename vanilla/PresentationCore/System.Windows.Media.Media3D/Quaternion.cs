using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Structure that represents a rotation in three dimensions. </summary>
[Serializable]
[TypeConverter(typeof(QuaternionConverter))]
[ValueSerializer(typeof(QuaternionValueSerializer))]
public struct Quaternion : IFormattable
{
	internal double _x;

	internal double _y;

	internal double _z;

	internal double _w;

	private bool _isNotDistinguishedIdentity;

	private static int c_identityHashCode = GetIdentityHashCode();

	private static Quaternion s_identity = GetIdentity();

	/// <summary>Gets the Identity quaternion </summary>
	/// <returns>The Identity quaternion.</returns>
	public static Quaternion Identity => s_identity;

	/// <summary>Gets the quaternion's axis.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that represents the quaternion's axis.</returns>
	public Vector3D Axis
	{
		get
		{
			if (IsDistinguishedIdentity || (_x == 0.0 && _y == 0.0 && _z == 0.0))
			{
				return new Vector3D(0.0, 1.0, 0.0);
			}
			Vector3D result = new Vector3D(_x, _y, _z);
			result.Normalize();
			return result;
		}
	}

	/// <summary>Gets the quaternion's angle, in degrees. </summary>
	/// <returns>Double that represents the quaternion's angle, in degrees.</returns>
	public double Angle
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 0.0;
			}
			double num = Math.Sqrt(_x * _x + _y * _y + _z * _z);
			double x = _w;
			if (!(num <= double.MaxValue))
			{
				double num2 = Math.Max(Math.Abs(_x), Math.Max(Math.Abs(_y), Math.Abs(_z)));
				double num3 = _x / num2;
				double num4 = _y / num2;
				double num5 = _z / num2;
				num = Math.Sqrt(num3 * num3 + num4 * num4 + num5 * num5);
				x = _w / num2;
			}
			return Math.Atan2(num, x) * (360.0 / Math.PI);
		}
	}

	/// <summary>Gets a value that indicates whether the quaternion is normalized.</summary>
	/// <returns>true if the quaternion is normalized, false otherwise.</returns>
	public bool IsNormalized
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return true;
			}
			return DoubleUtil.IsOne(_x * _x + _y * _y + _z * _z + _w * _w);
		}
	}

	/// <summary>Gets a value that indicates whether the specified quaternion is an <see cref="P:System.Windows.Media.Media3D.Quaternion.Identity" /> quaternion. </summary>
	/// <returns>true if the quaternion is the <see cref="P:System.Windows.Media.Media3D.Quaternion.Identity" /> quaternion, false otherwise.</returns>
	public bool IsIdentity
	{
		get
		{
			if (!IsDistinguishedIdentity)
			{
				if (_x == 0.0 && _y == 0.0 && _z == 0.0)
				{
					return _w == 1.0;
				}
				return false;
			}
			return true;
		}
	}

	/// <summary>Gets the X component of the quaternion.</summary>
	/// <returns>The X component of the quaternion.</returns>
	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_x = value;
		}
	}

	/// <summary>Gets the Y component of the quaternion.</summary>
	/// <returns>The Y component of the quaternion.</returns>
	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_y = value;
		}
	}

	/// <summary>Gets the Z component of the quaternion.</summary>
	/// <returns>The Z component of the quaternion.</returns>
	public double Z
	{
		get
		{
			return _z;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_z = value;
		}
	}

	/// <summary>Gets the W component of the quaternion.</summary>
	/// <returns>The W component of the quaternion.</returns>
	public double W
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 1.0;
			}
			return _w;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_w = value;
		}
	}

	private bool IsDistinguishedIdentity
	{
		get
		{
			return !_isNotDistinguishedIdentity;
		}
		set
		{
			_isNotDistinguishedIdentity = !value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Quaternion" /> structure. </summary>
	/// <param name="x">Value of the new <see cref="T:System.Windows.Media.Media3D.Quaternion" />'s X coordinate.</param>
	/// <param name="y">Value of the new <see cref="T:System.Windows.Media.Media3D.Quaternion" />'s Y coordinate.</param>
	/// <param name="z">Value of the new <see cref="T:System.Windows.Media.Media3D.Quaternion" />'s Z coordinate.</param>
	/// <param name="w">Value of the new <see cref="T:System.Windows.Media.Media3D.Quaternion" />'s W coordinate.</param>
	public Quaternion(double x, double y, double z, double w)
	{
		_x = x;
		_y = y;
		_z = z;
		_w = w;
		_isNotDistinguishedIdentity = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Quaternion" /> structure. </summary>
	/// <param name="axisOfRotation">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that represents the axis of rotation.</param>
	/// <param name="angleInDegrees">Angle to rotate around the specified axis, in degrees.</param>
	public Quaternion(Vector3D axisOfRotation, double angleInDegrees)
	{
		angleInDegrees %= 360.0;
		double num = angleInDegrees * (Math.PI / 180.0);
		double length = axisOfRotation.Length;
		if (length == 0.0)
		{
			throw new InvalidOperationException(SR.Quaternion_ZeroAxisSpecified);
		}
		Vector3D vector3D = axisOfRotation / length * Math.Sin(0.5 * num);
		_x = vector3D.X;
		_y = vector3D.Y;
		_z = vector3D.Z;
		_w = Math.Cos(0.5 * num);
		_isNotDistinguishedIdentity = true;
	}

	/// <summary>Replaces a quaternion with its conjugate. </summary>
	public void Conjugate()
	{
		if (!IsDistinguishedIdentity)
		{
			_x = 0.0 - _x;
			_y = 0.0 - _y;
			_z = 0.0 - _z;
		}
	}

	/// <summary>Replaces the specified quaternion with its inverse </summary>
	public void Invert()
	{
		if (!IsDistinguishedIdentity)
		{
			Conjugate();
			double num = _x * _x + _y * _y + _z * _z + _w * _w;
			_x /= num;
			_y /= num;
			_z /= num;
			_w /= num;
		}
	}

	/// <summary>Returns a normalized quaternion. </summary>
	public void Normalize()
	{
		if (!IsDistinguishedIdentity)
		{
			double num = _x * _x + _y * _y + _z * _z + _w * _w;
			if (num > double.MaxValue)
			{
				double num2 = 1.0 / Max(Math.Abs(_x), Math.Abs(_y), Math.Abs(_z), Math.Abs(_w));
				_x *= num2;
				_y *= num2;
				_z *= num2;
				_w *= num2;
				num = _x * _x + _y * _y + _z * _z + _w * _w;
			}
			double num3 = 1.0 / Math.Sqrt(num);
			_x *= num3;
			_y *= num3;
			_z *= num3;
			_w *= num3;
		}
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> values.</summary>
	/// <returns>Quaternion that is the sum of the two specified  <see cref="T:System.Windows.Media.Media3D.Quaternion" /> values.</returns>
	/// <param name="left">First quaternion to add.</param>
	/// <param name="right">Second quaternion to add.</param>
	public static Quaternion operator +(Quaternion left, Quaternion right)
	{
		if (right.IsDistinguishedIdentity)
		{
			if (left.IsDistinguishedIdentity)
			{
				return new Quaternion(0.0, 0.0, 0.0, 2.0);
			}
			left._w += 1.0;
			return left;
		}
		if (left.IsDistinguishedIdentity)
		{
			right._w += 1.0;
			return right;
		}
		return new Quaternion(left._x + right._x, left._y + right._y, left._z + right._z, left._w + right._w);
	}

	/// <summary>Adds the specified quaternions. </summary>
	/// <returns>Quaternion that is the result of addition.</returns>
	/// <param name="left">First quaternion to add.</param>
	/// <param name="right">Second quaternion to add.</param>
	public static Quaternion Add(Quaternion left, Quaternion right)
	{
		return left + right;
	}

	/// <summary>Subtracts a specified quaternion from another.</summary>
	/// <returns>Quaternion that is the result of subtraction.</returns>
	/// <param name="left">Quaternion from which to subtract.</param>
	/// <param name="right">Quaternion to subtract from the first quaternion.</param>
	public static Quaternion operator -(Quaternion left, Quaternion right)
	{
		if (right.IsDistinguishedIdentity)
		{
			if (left.IsDistinguishedIdentity)
			{
				return new Quaternion(0.0, 0.0, 0.0, 0.0);
			}
			left._w -= 1.0;
			return left;
		}
		if (left.IsDistinguishedIdentity)
		{
			return new Quaternion(0.0 - right._x, 0.0 - right._y, 0.0 - right._z, 1.0 - right._w);
		}
		return new Quaternion(left._x - right._x, left._y - right._y, left._z - right._z, left._w - right._w);
	}

	/// <summary>Subtracts a Quaternion from another. </summary>
	/// <returns>Quaternion that is the result of subtraction.</returns>
	/// <param name="left">Quaternion from which to subtract.</param>
	/// <param name="right">Quaternion to subtract from the first quaternion.</param>
	public static Quaternion Subtract(Quaternion left, Quaternion right)
	{
		return left - right;
	}

	/// <summary>Multiplies the specified quaternion by another.</summary>
	/// <returns>Quaternion that is the product of multiplication.</returns>
	/// <param name="left">First quaternion.</param>
	/// <param name="right">Second quaternion.</param>
	public static Quaternion operator *(Quaternion left, Quaternion right)
	{
		if (left.IsDistinguishedIdentity)
		{
			return right;
		}
		if (right.IsDistinguishedIdentity)
		{
			return left;
		}
		double x = left._w * right._x + left._x * right._w + left._y * right._z - left._z * right._y;
		double y = left._w * right._y + left._y * right._w + left._z * right._x - left._x * right._z;
		double z = left._w * right._z + left._z * right._w + left._x * right._y - left._y * right._x;
		double w = left._w * right._w - left._x * right._x - left._y * right._y - left._z * right._z;
		return new Quaternion(x, y, z, w);
	}

	/// <summary>Multiplies the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> values. </summary>
	/// <returns>Quaternion that is the result of multiplication.</returns>
	/// <param name="left">First quaternion to multiply.</param>
	/// <param name="right">Second quaternion to multiply.</param>
	public static Quaternion Multiply(Quaternion left, Quaternion right)
	{
		return left * right;
	}

	private void Scale(double scale)
	{
		if (IsDistinguishedIdentity)
		{
			_w = scale;
			IsDistinguishedIdentity = false;
			return;
		}
		_x *= scale;
		_y *= scale;
		_z *= scale;
		_w *= scale;
	}

	private double Length()
	{
		if (IsDistinguishedIdentity)
		{
			return 1.0;
		}
		double num = _x * _x + _y * _y + _z * _z + _w * _w;
		if (!(num <= double.MaxValue))
		{
			double num2 = Math.Max(Math.Max(Math.Abs(_x), Math.Abs(_y)), Math.Max(Math.Abs(_z), Math.Abs(_w)));
			double num3 = _x / num2;
			double num4 = _y / num2;
			double num5 = _z / num2;
			double num6 = _w / num2;
			return Math.Sqrt(num3 * num3 + num4 * num4 + num5 * num5 + num6 * num6) * num2;
		}
		return Math.Sqrt(num);
	}

	/// <summary>Interpolates between two orientations using spherical linear interpolation. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the orientation resulting from the interpolation.</returns>
	/// <param name="from">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the starting orientation.</param>
	/// <param name="to">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the ending orientation.</param>
	/// <param name="t">Interpolation coefficient.</param>
	public static Quaternion Slerp(Quaternion from, Quaternion to, double t)
	{
		return Slerp(from, to, t, useShortestPath: true);
	}

	/// <summary>Interpolates between orientations, represented as <see cref="T:System.Windows.Media.Media3D.Quaternion" /> structures, using spherical linear interpolation.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the orientation resulting from the interpolation.</returns>
	/// <param name="from">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the starting orientation.</param>
	/// <param name="to">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the ending orientation.</param>
	/// <param name="t">Interpolation coefficient.</param>
	/// <param name="useShortestPath">Boolean that indicates whether to compute quaternions that constitute the shortest possible arc on a four-dimensional unit sphere.</param>
	public static Quaternion Slerp(Quaternion from, Quaternion to, double t, bool useShortestPath)
	{
		if (from.IsDistinguishedIdentity)
		{
			from._w = 1.0;
		}
		if (to.IsDistinguishedIdentity)
		{
			to._w = 1.0;
		}
		double num = from.Length();
		double num2 = to.Length();
		from.Scale(1.0 / num);
		to.Scale(1.0 / num2);
		double num3 = from._x * to._x + from._y * to._y + from._z * to._z + from._w * to._w;
		if (useShortestPath)
		{
			if (num3 < 0.0)
			{
				num3 = 0.0 - num3;
				to._x = 0.0 - to._x;
				to._y = 0.0 - to._y;
				to._z = 0.0 - to._z;
				to._w = 0.0 - to._w;
			}
		}
		else if (num3 < -1.0)
		{
			num3 = -1.0;
		}
		if (num3 > 1.0)
		{
			num3 = 1.0;
		}
		double num4;
		double num5;
		if (num3 > 0.999999)
		{
			num4 = 1.0 - t;
			num5 = t;
		}
		else if (num3 < -0.9999999999)
		{
			to = new Quaternion(0.0 - from.Y, from.X, 0.0 - from.W, from.Z);
			double num6 = t * Math.PI;
			num4 = Math.Cos(num6);
			num5 = Math.Sin(num6);
		}
		else
		{
			double num7 = Math.Acos(num3);
			double num8 = Math.Sqrt(1.0 - num3 * num3);
			num4 = Math.Sin((1.0 - t) * num7) / num8;
			num5 = Math.Sin(t * num7) / num8;
		}
		double num9 = num * Math.Pow(num2 / num, t);
		num4 *= num9;
		num5 *= num9;
		return new Quaternion(num4 * from._x + num5 * to._x, num4 * from._y + num5 * to._y, num4 * from._z + num5 * to._z, num4 * from._w + num5 * to._w);
	}

	private static double Max(double a, double b, double c, double d)
	{
		if (b > a)
		{
			a = b;
		}
		if (c > a)
		{
			a = c;
		}
		if (d > a)
		{
			a = d;
		}
		return a;
	}

	private static int GetIdentityHashCode()
	{
		return 0.0.GetHashCode() ^ 1.0.GetHashCode();
	}

	private static Quaternion GetIdentity()
	{
		Quaternion result = new Quaternion(0.0, 0.0, 0.0, 1.0);
		result.IsDistinguishedIdentity = true;
		return result;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances for exact equality.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances are exactly equal, false otherwise.</returns>
	/// <param name="quaternion1">First Quaternion to compare.</param>
	/// <param name="quaternion2">Second Quaternion to compare.</param>
	public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
	{
		if (quaternion1.IsDistinguishedIdentity || quaternion2.IsDistinguishedIdentity)
		{
			return quaternion1.IsIdentity == quaternion2.IsIdentity;
		}
		if (quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z)
		{
			return quaternion1.W == quaternion2.W;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances for exact inequality. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances are exactly equal, false otherwise.</returns>
	/// <param name="quaternion1">First quaternion to compare.</param>
	/// <param name="quaternion2">Second quaternion to compare.</param>
	public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
	{
		return !(quaternion1 == quaternion2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances for equality. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances are exactly equal, false otherwise.</returns>
	/// <param name="quaternion1">First <see cref="T:System.Windows.Media.Media3D.Quaternion" /> to compare.</param>
	/// <param name="quaternion2">Second <see cref="T:System.Windows.Media.Media3D.Quaternion" /> to compare.</param>
	public static bool Equals(Quaternion quaternion1, Quaternion quaternion2)
	{
		if (quaternion1.IsDistinguishedIdentity || quaternion2.IsDistinguishedIdentity)
		{
			return quaternion1.IsIdentity == quaternion2.IsIdentity;
		}
		if (quaternion1.X.Equals(quaternion2.X) && quaternion1.Y.Equals(quaternion2.Y) && quaternion1.Z.Equals(quaternion2.Z))
		{
			return quaternion1.W.Equals(quaternion2.W);
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances for equality. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances are exactly equal, false otherwise.</returns>
	/// <param name="o">Object with which to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Quaternion quaternion))
		{
			return false;
		}
		return Equals(this, quaternion);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances for equality. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.Media3D.Quaternion" /> instances are exactly equal, false otherwise.</returns>
	/// <param name="value">Quaternion with which to compare.</param>
	public bool Equals(Quaternion value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns the hash code for the <see cref="T:System.Windows.Media.Media3D.Quaternion" />. </summary>
	/// <returns>An integer type that represents the hash code for the <see cref="T:System.Windows.Media.Media3D.Quaternion" />.</returns>
	public override int GetHashCode()
	{
		if (IsDistinguishedIdentity)
		{
			return c_identityHashCode;
		}
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
	}

	/// <summary>Converts a string representation of a <see cref="T:System.Windows.Media.Media3D.Quaternion" /> into the equivalent <see cref="T:System.Windows.Media.Media3D.Quaternion" /> structure. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> represented by the string.</returns>
	/// <param name="source">A string representation of a <see cref="T:System.Windows.Media.Media3D.Quaternion" />.</param>
	public static Quaternion Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Quaternion result = ((!(text == "Identity")) ? new Quaternion(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Identity);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a string representation of the object. </summary>
	/// <returns>String representation of the object.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of the object. </summary>
	/// <returns>String representation of the object.</returns>
	/// <param name="provider">Culture-specific formatting information.</param>
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
		if (IsIdentity)
		{
			return "Identity";
		}
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}", numericListSeparator, _x, _y, _z, _w);
	}
}
