using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using MS.Internal;
using MS.Internal.WindowsBase;

namespace System.Windows.Media;

/// <summary> Represents a 3x3 affine transformation matrix used for transformations in 2-D space. </summary>
[Serializable]
[TypeConverter(typeof(MatrixConverter))]
[ValueSerializer(typeof(MatrixValueSerializer))]
public struct Matrix : IFormattable
{
	private static Matrix s_identity = CreateIdentity();

	private const int c_identityHashCode = 0;

	internal double _m11;

	internal double _m12;

	internal double _m21;

	internal double _m22;

	internal double _offsetX;

	internal double _offsetY;

	internal MatrixTypes _type;

	internal int _padding;

	/// <summary> Gets an identity <see cref="T:System.Windows.Media.Matrix" />. </summary>
	/// <returns>An identity matrix.</returns>
	public static Matrix Identity => s_identity;

	/// <summary> Gets a value that indicates whether this <see cref="T:System.Windows.Media.Matrix" /> structure is an identity matrix. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Matrix" /> structure is an identity matrix; otherwise, false. The default is true.</returns>
	public bool IsIdentity
	{
		get
		{
			if (_type != 0)
			{
				if (_m11 == 1.0 && _m12 == 0.0 && _m21 == 0.0 && _m22 == 1.0 && _offsetX == 0.0)
				{
					return _offsetY == 0.0;
				}
				return false;
			}
			return true;
		}
	}

	/// <summary> Gets the determinant of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The determinant of this <see cref="T:System.Windows.Media.Matrix" />.</returns>
	public double Determinant
	{
		get
		{
			switch (_type)
			{
			case MatrixTypes.TRANSFORM_IS_IDENTITY:
			case MatrixTypes.TRANSFORM_IS_TRANSLATION:
				return 1.0;
			case MatrixTypes.TRANSFORM_IS_SCALING:
			case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
				return _m11 * _m22;
			default:
				return _m11 * _m22 - _m12 * _m21;
			}
		}
	}

	/// <summary> Gets a value that indicates whether this <see cref="T:System.Windows.Media.Matrix" /> structure is invertible. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Matrix" /> has an inverse; otherwise, false. The default is true.</returns>
	public bool HasInverse => !DoubleUtil.IsZero(Determinant);

	/// <summary>Gets or sets the value of the first row and first column of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The value of the first row and first column of this <see cref="T:System.Windows.Media.Matrix" />. The default value is 1.</returns>
	public double M11
	{
		get
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				return 1.0;
			}
			return _m11;
		}
		set
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				SetMatrix(value, 0.0, 0.0, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_SCALING);
				return;
			}
			_m11 = value;
			if (_type != MatrixTypes.TRANSFORM_IS_UNKNOWN)
			{
				_type |= MatrixTypes.TRANSFORM_IS_SCALING;
			}
		}
	}

	/// <summary> Gets or sets the value of the first row and second column of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The value of the first row and second column of this <see cref="T:System.Windows.Media.Matrix" />. The default value is 0.</returns>
	public double M12
	{
		get
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				return 0.0;
			}
			return _m12;
		}
		set
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				SetMatrix(1.0, value, 0.0, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_UNKNOWN);
				return;
			}
			_m12 = value;
			_type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
		}
	}

	/// <summary> Gets or sets the value of the second row and first column of this <see cref="T:System.Windows.Media.Matrix" /> structure.</summary>
	/// <returns>The value of the second row and first column of this <see cref="T:System.Windows.Media.Matrix" />. The default value is 0.</returns>
	public double M21
	{
		get
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				return 0.0;
			}
			return _m21;
		}
		set
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				SetMatrix(1.0, 0.0, value, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_UNKNOWN);
				return;
			}
			_m21 = value;
			_type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
		}
	}

	/// <summary>Gets or sets the value of the second row and second column of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The value of the second row and second column of this <see cref="T:System.Windows.Media.Matrix" /> structure. The default value is 1.</returns>
	public double M22
	{
		get
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				return 1.0;
			}
			return _m22;
		}
		set
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				SetMatrix(1.0, 0.0, 0.0, value, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_SCALING);
				return;
			}
			_m22 = value;
			if (_type != MatrixTypes.TRANSFORM_IS_UNKNOWN)
			{
				_type |= MatrixTypes.TRANSFORM_IS_SCALING;
			}
		}
	}

	/// <summary>Gets or sets the value of the third row and first column of this <see cref="T:System.Windows.Media.Matrix" /> structure.  </summary>
	/// <returns>The value of the third row and first column of this <see cref="T:System.Windows.Media.Matrix" /> structure. The default value is 0.</returns>
	public double OffsetX
	{
		get
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				return 0.0;
			}
			return _offsetX;
		}
		set
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				SetMatrix(1.0, 0.0, 0.0, 1.0, value, 0.0, MatrixTypes.TRANSFORM_IS_TRANSLATION);
				return;
			}
			_offsetX = value;
			if (_type != MatrixTypes.TRANSFORM_IS_UNKNOWN)
			{
				_type |= MatrixTypes.TRANSFORM_IS_TRANSLATION;
			}
		}
	}

	/// <summary>Gets or sets the value of the third row and second column of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The value of the third row and second column of this <see cref="T:System.Windows.Media.Matrix" /> structure. The default value is 0.</returns>
	public double OffsetY
	{
		get
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				return 0.0;
			}
			return _offsetY;
		}
		set
		{
			if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
			{
				SetMatrix(1.0, 0.0, 0.0, 1.0, 0.0, value, MatrixTypes.TRANSFORM_IS_TRANSLATION);
				return;
			}
			_offsetY = value;
			if (_type != MatrixTypes.TRANSFORM_IS_UNKNOWN)
			{
				_type |= MatrixTypes.TRANSFORM_IS_TRANSLATION;
			}
		}
	}

	private bool IsDistinguishedIdentity => _type == MatrixTypes.TRANSFORM_IS_IDENTITY;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="m11">The new <see cref="T:System.Windows.Media.Matrix" /> structure's <see cref="P:System.Windows.Media.Matrix.M11" /> coefficient.</param>
	/// <param name="m12">The new <see cref="T:System.Windows.Media.Matrix" /> structure's <see cref="P:System.Windows.Media.Matrix.M12" /> coefficient.</param>
	/// <param name="m21">The new <see cref="T:System.Windows.Media.Matrix" /> structure's <see cref="P:System.Windows.Media.Matrix.M21" /> coefficient.</param>
	/// <param name="m22">The new <see cref="T:System.Windows.Media.Matrix" /> structure's <see cref="P:System.Windows.Media.Matrix.M22" /> coefficient.</param>
	/// <param name="offsetX">The new <see cref="T:System.Windows.Media.Matrix" /> structure's <see cref="P:System.Windows.Media.Matrix.OffsetX" /> coefficient.</param>
	/// <param name="offsetY">The new <see cref="T:System.Windows.Media.Matrix" /> structure's <see cref="P:System.Windows.Media.Matrix.OffsetY" /> coefficient.</param>
	public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
	{
		_m11 = m11;
		_m12 = m12;
		_m21 = m21;
		_m22 = m22;
		_offsetX = offsetX;
		_offsetY = offsetY;
		_type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
		_padding = 0;
		DeriveMatrixType();
	}

	/// <summary> Changes this <see cref="T:System.Windows.Media.Matrix" /> structure into an identity matrix. </summary>
	public void SetIdentity()
	{
		_type = MatrixTypes.TRANSFORM_IS_IDENTITY;
	}

	/// <summary> Multiplies a <see cref="T:System.Windows.Media.Matrix" /> structure by another <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The result of multiplying <paramref name="trans1" /> by <paramref name="trans2" />.</returns>
	/// <param name="trans1">The first <see cref="T:System.Windows.Media.Matrix" /> structure to multiply.</param>
	/// <param name="trans2">The second <see cref="T:System.Windows.Media.Matrix" /> structure to multiply.</param>
	public static Matrix operator *(Matrix trans1, Matrix trans2)
	{
		MatrixUtil.MultiplyMatrix(ref trans1, ref trans2);
		return trans1;
	}

	/// <summary> Multiplies a <see cref="T:System.Windows.Media.Matrix" /> structure by another <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The result of multiplying <paramref name="trans1" /> by <paramref name="trans2" />.</returns>
	/// <param name="trans1">The first <see cref="T:System.Windows.Media.Matrix" /> structure to multiply.</param>
	/// <param name="trans2">The second <see cref="T:System.Windows.Media.Matrix" /> structure to multiply.</param>
	public static Matrix Multiply(Matrix trans1, Matrix trans2)
	{
		MatrixUtil.MultiplyMatrix(ref trans1, ref trans2);
		return trans1;
	}

	/// <summary> Appends the specified <see cref="T:System.Windows.Media.Matrix" /> structure to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="matrix">The <see cref="T:System.Windows.Media.Matrix" /> structure to append to this <see cref="T:System.Windows.Media.Matrix" /> structure.</param>
	public void Append(Matrix matrix)
	{
		this *= matrix;
	}

	/// <summary> Prepends the specified <see cref="T:System.Windows.Media.Matrix" /> structure onto this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="matrix">The <see cref="T:System.Windows.Media.Matrix" /> structure to prepend to this <see cref="T:System.Windows.Media.Matrix" /> structure.</param>
	public void Prepend(Matrix matrix)
	{
		this = matrix * this;
	}

	/// <summary> Applies a rotation of the specified angle about the origin of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="angle">The angle of rotation.</param>
	public void Rotate(double angle)
	{
		angle %= 360.0;
		this *= CreateRotationRadians(angle * (Math.PI / 180.0));
	}

	/// <summary> Prepends a rotation of the specified angle to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="angle">The angle of rotation to prepend.</param>
	public void RotatePrepend(double angle)
	{
		angle %= 360.0;
		this = CreateRotationRadians(angle * (Math.PI / 180.0)) * this;
	}

	/// <summary>Rotates this matrix about the specified point.</summary>
	/// <param name="angle">The angle, in degrees, by which to rotate this matrix. </param>
	/// <param name="centerX">The x-coordinate of the point about which to rotate this matrix.</param>
	/// <param name="centerY">The y-coordinate of the point about which to rotate this matrix.</param>
	public void RotateAt(double angle, double centerX, double centerY)
	{
		angle %= 360.0;
		this *= CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY);
	}

	/// <summary>Prepends a rotation of the specified angle at the specified point to this <see cref="T:System.Windows.Media.Matrix" /> structure.</summary>
	/// <param name="angle">The rotation angle, in degrees.</param>
	/// <param name="centerX">The x-coordinate of the rotation center.</param>
	/// <param name="centerY">The y-coordinate of the rotation center.</param>
	public void RotateAtPrepend(double angle, double centerX, double centerY)
	{
		angle %= 360.0;
		this = CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY) * this;
	}

	/// <summary> Appends the specified scale vector to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="scaleX">The value by which to scale this <see cref="T:System.Windows.Media.Matrix" /> along the x-axis.</param>
	/// <param name="scaleY">The value by which to scale this <see cref="T:System.Windows.Media.Matrix" /> along the y-axis.</param>
	public void Scale(double scaleX, double scaleY)
	{
		this *= CreateScaling(scaleX, scaleY);
	}

	/// <summary> Prepends the specified scale vector to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="scaleX">The value by which to scale this <see cref="T:System.Windows.Media.Matrix" /> structure along the x-axis.</param>
	/// <param name="scaleY">The value by which to scale this <see cref="T:System.Windows.Media.Matrix" /> structure along the y-axis.</param>
	public void ScalePrepend(double scaleX, double scaleY)
	{
		this = CreateScaling(scaleX, scaleY) * this;
	}

	/// <summary>Scales this <see cref="T:System.Windows.Media.Matrix" /> by the specified amount about the specified point.</summary>
	/// <param name="scaleX">The amount by which to scale this <see cref="T:System.Windows.Media.Matrix" /> along the x-axis. </param>
	/// <param name="scaleY">The amount by which to scale this <see cref="T:System.Windows.Media.Matrix" /> along the y-axis.</param>
	/// <param name="centerX">The x-coordinate of the scale operation's center point.</param>
	/// <param name="centerY">The y-coordinate of the scale operation's center point.</param>
	public void ScaleAt(double scaleX, double scaleY, double centerX, double centerY)
	{
		this *= CreateScaling(scaleX, scaleY, centerX, centerY);
	}

	/// <summary>Prepends the specified scale about the specified point of this <see cref="T:System.Windows.Media.Matrix" />.</summary>
	/// <param name="scaleX">The x-axis scale factor.</param>
	/// <param name="scaleY">The y-axis scale factor.</param>
	/// <param name="centerX">The x-coordinate of the point about which the scale operation is performed.</param>
	/// <param name="centerY">The y-coordinate of the point about which the scale operation is performed.</param>
	public void ScaleAtPrepend(double scaleX, double scaleY, double centerX, double centerY)
	{
		this = CreateScaling(scaleX, scaleY, centerX, centerY) * this;
	}

	/// <summary> Appends a skew of the specified degrees in the x and y dimensions to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="skewX">The angle in the x dimension by which to skew this <see cref="T:System.Windows.Media.Matrix" />.</param>
	/// <param name="skewY">The angle in the y dimension by which to skew this <see cref="T:System.Windows.Media.Matrix" />.</param>
	public void Skew(double skewX, double skewY)
	{
		skewX %= 360.0;
		skewY %= 360.0;
		this *= CreateSkewRadians(skewX * (Math.PI / 180.0), skewY * (Math.PI / 180.0));
	}

	/// <summary> Prepends a skew of the specified degrees in the x and y dimensions to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="skewX">The angle in the x dimension by which to skew this <see cref="T:System.Windows.Media.Matrix" />.</param>
	/// <param name="skewY">The angle in the y dimension by which to skew this <see cref="T:System.Windows.Media.Matrix" />.</param>
	public void SkewPrepend(double skewX, double skewY)
	{
		skewX %= 360.0;
		skewY %= 360.0;
		this = CreateSkewRadians(skewX * (Math.PI / 180.0), skewY * (Math.PI / 180.0)) * this;
	}

	/// <summary> Appends a translation of the specified offsets to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="offsetX">The amount to offset this <see cref="T:System.Windows.Media.Matrix" /> along the x-axis.</param>
	/// <param name="offsetY">The amount to offset this <see cref="T:System.Windows.Media.Matrix" /> along the y-axis.</param>
	public void Translate(double offsetX, double offsetY)
	{
		if (_type == MatrixTypes.TRANSFORM_IS_IDENTITY)
		{
			SetMatrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY, MatrixTypes.TRANSFORM_IS_TRANSLATION);
		}
		else if (_type == MatrixTypes.TRANSFORM_IS_UNKNOWN)
		{
			_offsetX += offsetX;
			_offsetY += offsetY;
		}
		else
		{
			_offsetX += offsetX;
			_offsetY += offsetY;
			_type |= MatrixTypes.TRANSFORM_IS_TRANSLATION;
		}
	}

	/// <summary> Prepends a translation of the specified offsets to this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <param name="offsetX">The amount to offset this <see cref="T:System.Windows.Media.Matrix" /> along the x-axis.</param>
	/// <param name="offsetY">The amount to offset this <see cref="T:System.Windows.Media.Matrix" /> along the y-axis.</param>
	public void TranslatePrepend(double offsetX, double offsetY)
	{
		this = CreateTranslation(offsetX, offsetY) * this;
	}

	/// <summary>Transforms the specified point by the <see cref="T:System.Windows.Media.Matrix" /> and returns the result.</summary>
	/// <returns>The result of transforming <paramref name="point" /> by this <see cref="T:System.Windows.Media.Matrix" />.</returns>
	/// <param name="point">The point to transform.</param>
	public Point Transform(Point point)
	{
		Point result = point;
		MultiplyPoint(ref result._x, ref result._y);
		return result;
	}

	/// <summary>Transforms the specified points by this <see cref="T:System.Windows.Media.Matrix" />. </summary>
	/// <param name="points">The points to transform. The original points in the array are replaced by their transformed values.</param>
	public void Transform(Point[] points)
	{
		if (points != null)
		{
			for (int i = 0; i < points.Length; i++)
			{
				MultiplyPoint(ref points[i]._x, ref points[i]._y);
			}
		}
	}

	/// <summary>Transforms the specified vector by this <see cref="T:System.Windows.Media.Matrix" />.</summary>
	/// <returns>The result of transforming <paramref name="vector" /> by this <see cref="T:System.Windows.Media.Matrix" />.</returns>
	/// <param name="vector">The vector to transform.</param>
	public Vector Transform(Vector vector)
	{
		Vector result = vector;
		MultiplyVector(ref result._x, ref result._y);
		return result;
	}

	/// <summary>Transforms the specified vectors by this <see cref="T:System.Windows.Media.Matrix" />.</summary>
	/// <param name="vectors">The vectors to transform. The original vectors in the array are replaced by their transformed values.</param>
	public void Transform(Vector[] vectors)
	{
		if (vectors != null)
		{
			for (int i = 0; i < vectors.Length; i++)
			{
				MultiplyVector(ref vectors[i]._x, ref vectors[i]._y);
			}
		}
	}

	/// <summary> Inverts this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Matrix" /> structure is not invertible.</exception>
	public void Invert()
	{
		double determinant = Determinant;
		if (DoubleUtil.IsZero(determinant))
		{
			throw new InvalidOperationException(SR.Transform_NotInvertible);
		}
		switch (_type)
		{
		case MatrixTypes.TRANSFORM_IS_SCALING:
			_m11 = 1.0 / _m11;
			_m22 = 1.0 / _m22;
			break;
		case MatrixTypes.TRANSFORM_IS_TRANSLATION:
			_offsetX = 0.0 - _offsetX;
			_offsetY = 0.0 - _offsetY;
			break;
		case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
			_m11 = 1.0 / _m11;
			_m22 = 1.0 / _m22;
			_offsetX = (0.0 - _offsetX) * _m11;
			_offsetY = (0.0 - _offsetY) * _m22;
			break;
		default:
		{
			double num = 1.0 / determinant;
			SetMatrix(_m22 * num, (0.0 - _m12) * num, (0.0 - _m21) * num, _m11 * num, (_m21 * _offsetY - _offsetX * _m22) * num, (_offsetX * _m12 - _m11 * _offsetY) * num, MatrixTypes.TRANSFORM_IS_UNKNOWN);
			break;
		}
		case MatrixTypes.TRANSFORM_IS_IDENTITY:
			break;
		}
	}

	internal void MultiplyVector(ref double x, ref double y)
	{
		switch (_type)
		{
		case MatrixTypes.TRANSFORM_IS_IDENTITY:
		case MatrixTypes.TRANSFORM_IS_TRANSLATION:
			return;
		case MatrixTypes.TRANSFORM_IS_SCALING:
		case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
			x *= _m11;
			y *= _m22;
			return;
		}
		double num = y * _m21;
		double num2 = x * _m12;
		x *= _m11;
		x += num;
		y *= _m22;
		y += num2;
	}

	internal void MultiplyPoint(ref double x, ref double y)
	{
		switch (_type)
		{
		case MatrixTypes.TRANSFORM_IS_IDENTITY:
			break;
		case MatrixTypes.TRANSFORM_IS_TRANSLATION:
			x += _offsetX;
			y += _offsetY;
			break;
		case MatrixTypes.TRANSFORM_IS_SCALING:
			x *= _m11;
			y *= _m22;
			break;
		case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
			x *= _m11;
			x += _offsetX;
			y *= _m22;
			y += _offsetY;
			break;
		default:
		{
			double num = y * _m21 + _offsetX;
			double num2 = x * _m12 + _offsetY;
			x *= _m11;
			x += num;
			y *= _m22;
			y += num2;
			break;
		}
		}
	}

	internal static Matrix CreateRotationRadians(double angle)
	{
		return CreateRotationRadians(angle, 0.0, 0.0);
	}

	internal static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
	{
		Matrix result = default(Matrix);
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		double offsetX = centerX * (1.0 - num2) + centerY * num;
		double offsetY = centerY * (1.0 - num2) - centerX * num;
		result.SetMatrix(num2, num, 0.0 - num, num2, offsetX, offsetY, MatrixTypes.TRANSFORM_IS_UNKNOWN);
		return result;
	}

	internal static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
	{
		Matrix result = default(Matrix);
		result.SetMatrix(scaleX, 0.0, 0.0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY, MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING);
		return result;
	}

	internal static Matrix CreateScaling(double scaleX, double scaleY)
	{
		Matrix result = default(Matrix);
		result.SetMatrix(scaleX, 0.0, 0.0, scaleY, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_SCALING);
		return result;
	}

	internal static Matrix CreateSkewRadians(double skewX, double skewY)
	{
		Matrix result = default(Matrix);
		result.SetMatrix(1.0, Math.Tan(skewY), Math.Tan(skewX), 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_UNKNOWN);
		return result;
	}

	internal static Matrix CreateTranslation(double offsetX, double offsetY)
	{
		Matrix result = default(Matrix);
		result.SetMatrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY, MatrixTypes.TRANSFORM_IS_TRANSLATION);
		return result;
	}

	private static Matrix CreateIdentity()
	{
		Matrix result = default(Matrix);
		result.SetMatrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_IDENTITY);
		return result;
	}

	private void SetMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY, MatrixTypes type)
	{
		_m11 = m11;
		_m12 = m12;
		_m21 = m21;
		_m22 = m22;
		_offsetX = offsetX;
		_offsetY = offsetY;
		_type = type;
	}

	private void DeriveMatrixType()
	{
		_type = MatrixTypes.TRANSFORM_IS_IDENTITY;
		if (_m21 != 0.0 || _m12 != 0.0)
		{
			_type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
			return;
		}
		if (_m11 != 1.0 || _m22 != 1.0)
		{
			_type = MatrixTypes.TRANSFORM_IS_SCALING;
		}
		if (_offsetX != 0.0 || _offsetY != 0.0)
		{
			_type |= MatrixTypes.TRANSFORM_IS_TRANSLATION;
		}
		if ((_type & (MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING)) == 0)
		{
			_type = MatrixTypes.TRANSFORM_IS_IDENTITY;
		}
	}

	[Conditional("DEBUG")]
	private void Debug_CheckType()
	{
		switch (_type)
		{
		case MatrixTypes.TRANSFORM_IS_IDENTITY:
			break;
		case MatrixTypes.TRANSFORM_IS_UNKNOWN:
			break;
		case MatrixTypes.TRANSFORM_IS_SCALING:
			break;
		case MatrixTypes.TRANSFORM_IS_TRANSLATION:
			break;
		case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
			break;
		}
	}

	/// <summary> Determines whether the two specified <see cref="T:System.Windows.Media.Matrix" /> structures are identical.</summary>
	/// <returns>true if <paramref name="matrix1" /> and <paramref name="matrix2" /> are identical; otherwise, false.</returns>
	/// <param name="matrix1">The first <see cref="T:System.Windows.Media.Matrix" /> structure to compare.</param>
	/// <param name="matrix2">The second <see cref="T:System.Windows.Media.Matrix" /> structure to compare.</param>
	public static bool operator ==(Matrix matrix1, Matrix matrix2)
	{
		if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
		{
			return matrix1.IsIdentity == matrix2.IsIdentity;
		}
		if (matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 && matrix1.OffsetX == matrix2.OffsetX)
		{
			return matrix1.OffsetY == matrix2.OffsetY;
		}
		return false;
	}

	/// <summary> Determines whether the two specified <see cref="T:System.Windows.Media.Matrix" /> structures are not identical.</summary>
	/// <returns>true if <paramref name="matrix1" /> and <paramref name="matrix2" /> are not identical; otherwise, false.</returns>
	/// <param name="matrix1">The first <see cref="T:System.Windows.Media.Matrix" /> structure to compare.</param>
	/// <param name="matrix2">The second <see cref="T:System.Windows.Media.Matrix" /> structure to compare.</param>
	public static bool operator !=(Matrix matrix1, Matrix matrix2)
	{
		return !(matrix1 == matrix2);
	}

	/// <summary> Determines whether the two specified <see cref="T:System.Windows.Media.Matrix" /> structures are identical.</summary>
	/// <returns>true if <paramref name="matrix1" /> and <paramref name="matrix2" /> are identical; otherwise, false.</returns>
	/// <param name="matrix1">The first <see cref="T:System.Windows.Media.Matrix" /> structure to compare.</param>
	/// <param name="matrix2">The second <see cref="T:System.Windows.Media.Matrix" /> structure to compare.</param>
	public static bool Equals(Matrix matrix1, Matrix matrix2)
	{
		if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
		{
			return matrix1.IsIdentity == matrix2.IsIdentity;
		}
		if (matrix1.M11.Equals(matrix2.M11) && matrix1.M12.Equals(matrix2.M12) && matrix1.M21.Equals(matrix2.M21) && matrix1.M22.Equals(matrix2.M22) && matrix1.OffsetX.Equals(matrix2.OffsetX))
		{
			return matrix1.OffsetY.Equals(matrix2.OffsetY);
		}
		return false;
	}

	/// <summary> Determines whether the specified <see cref="T:System.Object" /> is a <see cref="T:System.Windows.Media.Matrix" /> structure that is identical to this <see cref="T:System.Windows.Media.Matrix" />. </summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Windows.Media.Matrix" /> structure that is identical to this <see cref="T:System.Windows.Media.Matrix" /> structure; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Object" /> to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Matrix matrix))
		{
			return false;
		}
		return Equals(this, matrix);
	}

	/// <summary> Determines whether the specified <see cref="T:System.Windows.Media.Matrix" /> structure is identical to this instance. </summary>
	/// <returns>true if instances are equal; otherwise, false. </returns>
	/// <param name="value">The instance of <see cref="T:System.Windows.Media.Matrix" /> to compare to this instance.</param>
	public bool Equals(Matrix value)
	{
		return Equals(this, value);
	}

	/// <summary> Returns the hash code for this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The hash code for this instance.</returns>
	public override int GetHashCode()
	{
		if (IsDistinguishedIdentity)
		{
			return 0;
		}
		return M11.GetHashCode() ^ M12.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode() ^ OffsetX.GetHashCode() ^ OffsetY.GetHashCode();
	}

	/// <summary> Converts a <see cref="T:System.String" /> representation of a matrix into the equivalent <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>The equivalent <see cref="T:System.Windows.Media.Matrix" /> structure.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the matrix.</param>
	public static Matrix Parse(string source)
	{
		IFormatProvider invariantEnglishUS = TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Matrix result = ((!(text == "Identity")) ? new Matrix(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Identity);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary> Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Matrix" /> structure. </summary>
	/// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Matrix.M11" />, <see cref="P:System.Windows.Media.Matrix.M12" />, <see cref="P:System.Windows.Media.Matrix.M21" />, <see cref="P:System.Windows.Media.Matrix.M22" />, <see cref="P:System.Windows.Media.Matrix.OffsetX" />, and <see cref="P:System.Windows.Media.Matrix.OffsetY" /> values of this <see cref="T:System.Windows.Media.Matrix" />.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary> Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Matrix" /> structure with culture-specific formatting information. </summary>
	/// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Matrix.M11" />, <see cref="P:System.Windows.Media.Matrix.M12" />, <see cref="P:System.Windows.Media.Matrix.M21" />, <see cref="P:System.Windows.Media.Matrix.M22" />, <see cref="P:System.Windows.Media.Matrix.OffsetX" />, and <see cref="P:System.Windows.Media.Matrix.OffsetY" /> values of this <see cref="T:System.Windows.Media.Matrix" />.</returns>
	/// <param name="provider">The culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
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
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}{0}{5:" + format + "}{0}{6:" + format + "}", numericListSeparator, _m11, _m12, _m21, _m22, _offsetX, _offsetY);
	}
}
