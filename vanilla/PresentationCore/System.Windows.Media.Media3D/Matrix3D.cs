using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary> Represents a 4 x 4 matrix used for transformations in 3-D space. </summary>
[Serializable]
[TypeConverter(typeof(Matrix3DConverter))]
[ValueSerializer(typeof(Matrix3DValueSerializer))]
public struct Matrix3D : IFormattable
{
	private double _m11;

	private double _m12;

	private double _m13;

	private double _m14;

	private double _m21;

	private double _m22;

	private double _m23;

	private double _m24;

	private double _m31;

	private double _m32;

	private double _m33;

	private double _m34;

	private double _offsetX;

	private double _offsetY;

	private double _offsetZ;

	private double _m44;

	private bool _isNotKnownToBeIdentity;

	private static readonly Matrix3D s_identity = CreateIdentity();

	private const int c_identityHashCode = 0;

	/// <summary>Changes a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure into an identity <see cref="T:System.Windows.Media.Media3D.Matrix3D" />. </summary>
	/// <returns>The identity Matrix3D.</returns>
	public static Matrix3D Identity => s_identity;

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure is an identity Matrix3D.</summary>
	/// <returns>true if the Matrix3D structure is an identity Matrix3D; otherwise, false. The default value is true.</returns>
	public bool IsIdentity
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return true;
			}
			if (_m11 == 1.0 && _m12 == 0.0 && _m13 == 0.0 && _m14 == 0.0 && _m21 == 0.0 && _m22 == 1.0 && _m23 == 0.0 && _m24 == 0.0 && _m31 == 0.0 && _m32 == 0.0 && _m33 == 1.0 && _m34 == 0.0 && _offsetX == 0.0 && _offsetY == 0.0 && _offsetZ == 0.0 && _m44 == 1.0)
			{
				IsDistinguishedIdentity = true;
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure is affine. </summary>
	/// <returns>true if the Matrix3D structure is affine; otherwise, false. </returns>
	public bool IsAffine
	{
		get
		{
			if (!IsDistinguishedIdentity)
			{
				if (_m14 == 0.0 && _m24 == 0.0 && _m34 == 0.0)
				{
					return _m44 == 1.0;
				}
				return false;
			}
			return true;
		}
	}

	/// <summary>Retrieves the determinant of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The determinant of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double Determinant
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 1.0;
			}
			if (IsAffine)
			{
				return GetNormalizedAffineDeterminant();
			}
			double num = _m13 * _m24 - _m23 * _m14;
			double num2 = _m13 * _m34 - _m33 * _m14;
			double num3 = _m13 * _m44 - _offsetZ * _m14;
			double num4 = _m23 * _m34 - _m33 * _m24;
			double num5 = _m23 * _m44 - _offsetZ * _m24;
			double num6 = _m33 * _m44 - _offsetZ * _m34;
			double num7 = _m22 * num2 - _m32 * num - _m12 * num4;
			double num8 = _m12 * num5 - _m22 * num3 + _offsetY * num;
			double num9 = _m32 * num3 - _offsetY * num2 - _m12 * num6;
			double num10 = _m22 * num6 - _m32 * num5 + _offsetY * num4;
			return _offsetX * num7 + _m31 * num8 + _m21 * num9 + _m11 * num10;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> is invertible. </summary>
	/// <returns>true if the Matrix3D structure has an inverse; otherwise, false. The default value is true.</returns>
	public bool HasInverse => !DoubleUtil.IsZero(Determinant);

	/// <summary>Gets or sets the value of the first row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the first row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M11
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 1.0;
			}
			return _m11;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m11 = value;
		}
	}

	/// <summary>Gets or sets the value of the first row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the first row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M12
	{
		get
		{
			return _m12;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m12 = value;
		}
	}

	/// <summary>Gets or sets the value of the first row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the first row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M13
	{
		get
		{
			return _m13;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m13 = value;
		}
	}

	/// <summary>Gets or sets the value of the first row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</summary>
	/// <returns>The value of the first row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M14
	{
		get
		{
			return _m14;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m14 = value;
		}
	}

	/// <summary>Gets or sets the value of the second row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the second row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M21
	{
		get
		{
			return _m21;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m21 = value;
		}
	}

	/// <summary>Gets or sets the value of the second row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the second row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M22
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 1.0;
			}
			return _m22;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m22 = value;
		}
	}

	/// <summary>Gets or sets the value of the second row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the second row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M23
	{
		get
		{
			return _m23;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m23 = value;
		}
	}

	/// <summary>Gets or sets the value of the second row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the second row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M24
	{
		get
		{
			return _m24;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m24 = value;
		}
	}

	/// <summary>Gets or sets the value of the third row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the third row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M31
	{
		get
		{
			return _m31;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m31 = value;
		}
	}

	/// <summary>Gets or sets the value of the third row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the third row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M32
	{
		get
		{
			return _m32;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m32 = value;
		}
	}

	/// <summary>Gets or sets the value of the third row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the third row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M33
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 1.0;
			}
			return _m33;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m33 = value;
		}
	}

	/// <summary>Gets or sets the value of the third row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the third row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M34
	{
		get
		{
			return _m34;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m34 = value;
		}
	}

	/// <summary>Gets or sets the value of the fourth row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the fourth row and first column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double OffsetX
	{
		get
		{
			return _offsetX;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_offsetX = value;
		}
	}

	/// <summary>Gets or sets the value of the fourth row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the fourth row and second column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double OffsetY
	{
		get
		{
			return _offsetY;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_offsetY = value;
		}
	}

	/// <summary>Gets or sets the value of the fourth row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the fourth row and third column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double OffsetZ
	{
		get
		{
			return _offsetZ;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_offsetZ = value;
		}
	}

	/// <summary>Gets or sets the value of the fourth row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>The value of the fourth row and fourth column of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	public double M44
	{
		get
		{
			if (IsDistinguishedIdentity)
			{
				return 1.0;
			}
			return _m44;
		}
		set
		{
			if (IsDistinguishedIdentity)
			{
				this = s_identity;
				IsDistinguishedIdentity = false;
			}
			_m44 = value;
		}
	}

	private bool IsDistinguishedIdentity
	{
		get
		{
			return !_isNotKnownToBeIdentity;
		}
		set
		{
			_isNotKnownToBeIdentity = !value;
		}
	}

	/// <summary> Constructor that sets matrix's initial values. </summary>
	/// <param name="m11">Value of the (1,1) field of the new matrix.</param>
	/// <param name="m12">Value of the (1,2) field of the new matrix.</param>
	/// <param name="m13">Value of the (1,3) field of the new matrix.</param>
	/// <param name="m14">Value of the (1,4) field of the new matrix.</param>
	/// <param name="m21">Value of the (2,1) field of the new matrix.</param>
	/// <param name="m22">Value of the (2,2) field of the new matrix.</param>
	/// <param name="m23">Value of the (2,3) field of the new matrix.</param>
	/// <param name="m24">Value of the (2,4) field of the new matrix.</param>
	/// <param name="m31">Value of the (3,1) field of the new matrix.</param>
	/// <param name="m32">Value of the (3,2) field of the new matrix.</param>
	/// <param name="m33">Value of the (3,3) field of the new matrix.</param>
	/// <param name="m34">Value of the (3,4) field of the new matrix.</param>
	/// <param name="offsetX">Value of the X offset field of the new matrix.</param>
	/// <param name="offsetY">Value of the Y offset field of the new matrix.</param>
	/// <param name="offsetZ">Value of the Z offset field of the new matrix.</param>
	/// <param name="m44">Value of the (4,4) field of the new matrix.</param>
	public Matrix3D(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24, double m31, double m32, double m33, double m34, double offsetX, double offsetY, double offsetZ, double m44)
	{
		_m11 = m11;
		_m12 = m12;
		_m13 = m13;
		_m14 = m14;
		_m21 = m21;
		_m22 = m22;
		_m23 = m23;
		_m24 = m24;
		_m31 = m31;
		_m32 = m32;
		_m33 = m33;
		_m34 = m34;
		_offsetX = offsetX;
		_offsetY = offsetY;
		_offsetZ = offsetZ;
		_m44 = m44;
		_isNotKnownToBeIdentity = true;
	}

	/// <summary>Changes this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure into an identity matrix. </summary>
	public void SetIdentity()
	{
		this = s_identity;
	}

	/// <summary> Prepends a specified matrix to the current matrix. </summary>
	/// <param name="matrix">Matrix to prepend.</param>
	public void Prepend(Matrix3D matrix)
	{
		this = matrix * this;
	}

	/// <summary> Appends a specified matrix to the current matrix. </summary>
	/// <param name="matrix">Matrix to append.</param>
	public void Append(Matrix3D matrix)
	{
		this *= matrix;
	}

	/// <summary>Appends a rotation transform to the current <see cref="T:System.Windows.Media.Media3D.Matrix3D" />. </summary>
	/// <param name="quaternion">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the rotation.</param>
	public void Rotate(Quaternion quaternion)
	{
		Point3D center = default(Point3D);
		this *= CreateRotationMatrix(ref quaternion, ref center);
	}

	/// <summary>Prepends a rotation specified by a <see cref="T:System.Windows.Media.Media3D.Quaternion" /> to this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="quaternion">Quaternion representing rotation.</param>
	public void RotatePrepend(Quaternion quaternion)
	{
		Point3D center = default(Point3D);
		this = CreateRotationMatrix(ref quaternion, ref center) * this;
	}

	/// <summary>Rotates this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> about the specified <see cref="T:System.Windows.Media.Media3D.Point3D" />.</summary>
	/// <param name="quaternion">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the rotation.</param>
	/// <param name="center">Center <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.</param>
	public void RotateAt(Quaternion quaternion, Point3D center)
	{
		this *= CreateRotationMatrix(ref quaternion, ref center);
	}

	/// <summary>Prepends a rotation about a specified center <see cref="T:System.Windows.Media.Media3D.Point3D" /> to this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="quaternion">
	///   <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that represents the rotation.</param>
	/// <param name="center">Center <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.</param>
	public void RotateAtPrepend(Quaternion quaternion, Point3D center)
	{
		this = CreateRotationMatrix(ref quaternion, ref center) * this;
	}

	/// <summary>Appends the specified scale <see cref="T:System.Windows.Media.Media3D.Vector3D" /> to this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="scale">Vector3D by which to scale this Matrix3D structure.</param>
	public void Scale(Vector3D scale)
	{
		if (IsDistinguishedIdentity)
		{
			SetScaleMatrix(ref scale);
			return;
		}
		_m11 *= scale.X;
		_m12 *= scale.Y;
		_m13 *= scale.Z;
		_m21 *= scale.X;
		_m22 *= scale.Y;
		_m23 *= scale.Z;
		_m31 *= scale.X;
		_m32 *= scale.Y;
		_m33 *= scale.Z;
		_offsetX *= scale.X;
		_offsetY *= scale.Y;
		_offsetZ *= scale.Z;
	}

	/// <summary>Prepends the specifed scale <see cref="T:System.Windows.Media.Media3D.Vector3D" /> to the current <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="scale">Vector3D by which to scale this Matrix3D structure.</param>
	public void ScalePrepend(Vector3D scale)
	{
		if (IsDistinguishedIdentity)
		{
			SetScaleMatrix(ref scale);
			return;
		}
		_m11 *= scale.X;
		_m12 *= scale.X;
		_m13 *= scale.X;
		_m14 *= scale.X;
		_m21 *= scale.Y;
		_m22 *= scale.Y;
		_m23 *= scale.Y;
		_m24 *= scale.Y;
		_m31 *= scale.Z;
		_m32 *= scale.Z;
		_m33 *= scale.Z;
		_m34 *= scale.Z;
	}

	/// <summary>Scales this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure by the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> about the specified <see cref="T:System.Windows.Media.Media3D.Point3D" />. </summary>
	/// <param name="scale">Vector3D by which to scale this Matrix3D structure.</param>
	/// <param name="center">Point3D about which to scale.</param>
	public void ScaleAt(Vector3D scale, Point3D center)
	{
		if (IsDistinguishedIdentity)
		{
			SetScaleMatrix(ref scale, ref center);
			return;
		}
		double num = _m14 * center.X;
		_m11 = num + scale.X * (_m11 - num);
		num = _m14 * center.Y;
		_m12 = num + scale.Y * (_m12 - num);
		num = _m14 * center.Z;
		_m13 = num + scale.Z * (_m13 - num);
		num = _m24 * center.X;
		_m21 = num + scale.X * (_m21 - num);
		num = _m24 * center.Y;
		_m22 = num + scale.Y * (_m22 - num);
		num = _m24 * center.Z;
		_m23 = num + scale.Z * (_m23 - num);
		num = _m34 * center.X;
		_m31 = num + scale.X * (_m31 - num);
		num = _m34 * center.Y;
		_m32 = num + scale.Y * (_m32 - num);
		num = _m34 * center.Z;
		_m33 = num + scale.Z * (_m33 - num);
		num = _m44 * center.X;
		_offsetX = num + scale.X * (_offsetX - num);
		num = _m44 * center.Y;
		_offsetY = num + scale.Y * (_offsetY - num);
		num = _m44 * center.Z;
		_offsetZ = num + scale.Z * (_offsetZ - num);
	}

	/// <summary>Prepends the specified scale transformation about the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> to this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="scale">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> by which to scale this Matrix3D structure.</param>
	/// <param name="center">Point3D about which to scale.</param>
	public void ScaleAtPrepend(Vector3D scale, Point3D center)
	{
		if (IsDistinguishedIdentity)
		{
			SetScaleMatrix(ref scale, ref center);
			return;
		}
		double num = center.X - center.X * scale.X;
		double num2 = center.Y - center.Y * scale.Y;
		double num3 = center.Z - center.Z * scale.Z;
		_offsetX += _m11 * num + _m21 * num2 + _m31 * num3;
		_offsetY += _m12 * num + _m22 * num2 + _m32 * num3;
		_offsetZ += _m13 * num + _m23 * num2 + _m33 * num3;
		_m44 += _m14 * num + _m24 * num2 + _m34 * num3;
		_m11 *= scale.X;
		_m12 *= scale.X;
		_m13 *= scale.X;
		_m14 *= scale.X;
		_m21 *= scale.Y;
		_m22 *= scale.Y;
		_m23 *= scale.Y;
		_m24 *= scale.Y;
		_m31 *= scale.Z;
		_m32 *= scale.Z;
		_m33 *= scale.Z;
		_m34 *= scale.Z;
	}

	/// <summary>Appends a translation of the specified offset to the current <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="offset">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the offset for transformation.</param>
	public void Translate(Vector3D offset)
	{
		if (IsDistinguishedIdentity)
		{
			SetTranslationMatrix(ref offset);
			return;
		}
		_m11 += _m14 * offset.X;
		_m12 += _m14 * offset.Y;
		_m13 += _m14 * offset.Z;
		_m21 += _m24 * offset.X;
		_m22 += _m24 * offset.Y;
		_m23 += _m24 * offset.Z;
		_m31 += _m34 * offset.X;
		_m32 += _m34 * offset.Y;
		_m33 += _m34 * offset.Z;
		_offsetX += _m44 * offset.X;
		_offsetY += _m44 * offset.Y;
		_offsetZ += _m44 * offset.Z;
	}

	/// <summary>Prepends a translation of the specified offset to this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <param name="offset">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the offset for transformation.</param>
	public void TranslatePrepend(Vector3D offset)
	{
		if (IsDistinguishedIdentity)
		{
			SetTranslationMatrix(ref offset);
			return;
		}
		_offsetX += _m11 * offset.X + _m21 * offset.Y + _m31 * offset.Z;
		_offsetY += _m12 * offset.X + _m22 * offset.Y + _m32 * offset.Z;
		_offsetZ += _m13 * offset.X + _m23 * offset.Y + _m33 * offset.Z;
		_m44 += _m14 * offset.X + _m24 * offset.Y + _m34 * offset.Z;
	}

	/// <summary> Multiplies the specified matrices. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that is the result of multiplication.</returns>
	/// <param name="matrix1">Matrix to multiply.</param>
	/// <param name="matrix2">Matrix by which the first matrix is multiplied.</param>
	public static Matrix3D operator *(Matrix3D matrix1, Matrix3D matrix2)
	{
		if (matrix1.IsDistinguishedIdentity)
		{
			return matrix2;
		}
		if (matrix2.IsDistinguishedIdentity)
		{
			return matrix1;
		}
		return new Matrix3D(matrix1._m11 * matrix2._m11 + matrix1._m12 * matrix2._m21 + matrix1._m13 * matrix2._m31 + matrix1._m14 * matrix2._offsetX, matrix1._m11 * matrix2._m12 + matrix1._m12 * matrix2._m22 + matrix1._m13 * matrix2._m32 + matrix1._m14 * matrix2._offsetY, matrix1._m11 * matrix2._m13 + matrix1._m12 * matrix2._m23 + matrix1._m13 * matrix2._m33 + matrix1._m14 * matrix2._offsetZ, matrix1._m11 * matrix2._m14 + matrix1._m12 * matrix2._m24 + matrix1._m13 * matrix2._m34 + matrix1._m14 * matrix2._m44, matrix1._m21 * matrix2._m11 + matrix1._m22 * matrix2._m21 + matrix1._m23 * matrix2._m31 + matrix1._m24 * matrix2._offsetX, matrix1._m21 * matrix2._m12 + matrix1._m22 * matrix2._m22 + matrix1._m23 * matrix2._m32 + matrix1._m24 * matrix2._offsetY, matrix1._m21 * matrix2._m13 + matrix1._m22 * matrix2._m23 + matrix1._m23 * matrix2._m33 + matrix1._m24 * matrix2._offsetZ, matrix1._m21 * matrix2._m14 + matrix1._m22 * matrix2._m24 + matrix1._m23 * matrix2._m34 + matrix1._m24 * matrix2._m44, matrix1._m31 * matrix2._m11 + matrix1._m32 * matrix2._m21 + matrix1._m33 * matrix2._m31 + matrix1._m34 * matrix2._offsetX, matrix1._m31 * matrix2._m12 + matrix1._m32 * matrix2._m22 + matrix1._m33 * matrix2._m32 + matrix1._m34 * matrix2._offsetY, matrix1._m31 * matrix2._m13 + matrix1._m32 * matrix2._m23 + matrix1._m33 * matrix2._m33 + matrix1._m34 * matrix2._offsetZ, matrix1._m31 * matrix2._m14 + matrix1._m32 * matrix2._m24 + matrix1._m33 * matrix2._m34 + matrix1._m34 * matrix2._m44, matrix1._offsetX * matrix2._m11 + matrix1._offsetY * matrix2._m21 + matrix1._offsetZ * matrix2._m31 + matrix1._m44 * matrix2._offsetX, matrix1._offsetX * matrix2._m12 + matrix1._offsetY * matrix2._m22 + matrix1._offsetZ * matrix2._m32 + matrix1._m44 * matrix2._offsetY, matrix1._offsetX * matrix2._m13 + matrix1._offsetY * matrix2._m23 + matrix1._offsetZ * matrix2._m33 + matrix1._m44 * matrix2._offsetZ, matrix1._offsetX * matrix2._m14 + matrix1._offsetY * matrix2._m24 + matrix1._offsetZ * matrix2._m34 + matrix1._m44 * matrix2._m44);
	}

	/// <summary>Multiplies the specified matrices. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that is the result of multiplication.</returns>
	/// <param name="matrix1">Matrix to multiply.</param>
	/// <param name="matrix2">Matrix by which the first matrix is multiplied.</param>
	public static Matrix3D Multiply(Matrix3D matrix1, Matrix3D matrix2)
	{
		return matrix1 * matrix2;
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> by the <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> and returns the result. </summary>
	/// <returns>The result of transforming <paramref name="point" /> by this Matrix3D.</returns>
	/// <param name="point">Point3D to transform.</param>
	/// <exception cref="T:System.InvalidOperationException">Throws InvalidOperationException if the transform is not affine.</exception>
	public Point3D Transform(Point3D point)
	{
		MultiplyPoint(ref point);
		return point;
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects in the array by the <see cref="T:System.Windows.Media.Media3D.Matrix3D" />. </summary>
	/// <param name="points">Point3D objects to transform. The original points in the array are replaced by their transformed values.</param>
	/// <exception cref="T:System.InvalidOperationException">Throws InvalidOperationException if the transform is not affine.</exception>
	public void Transform(Point3D[] points)
	{
		if (points != null)
		{
			for (int i = 0; i < points.Length; i++)
			{
				MultiplyPoint(ref points[i]);
			}
		}
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point4D" /> by the <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> and returns the result.. </summary>
	/// <returns>The result of transforming <paramref name="point" /> by this Matrix3D.</returns>
	/// <param name="point">
	///   <see cref="T:System.Windows.Media.Media3D.Point4D" /> to transform.</param>
	public Point4D Transform(Point4D point)
	{
		MultiplyPoint(ref point);
		return point;
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point4D" /> objects in the array by the <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> and returns the result. </summary>
	/// <param name="points">
	///   <see cref="T:System.Windows.Media.Media3D.Point4D" /> objects to transform. The original points in the array are replaced by their transformed values.</param>
	public void Transform(Point4D[] points)
	{
		if (points != null)
		{
			for (int i = 0; i < points.Length; i++)
			{
				MultiplyPoint(ref points[i]);
			}
		}
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> by this <see cref="T:System.Windows.Media.Media3D.Matrix3D" />. </summary>
	/// <returns>The result of transforming <paramref name="vector" /> by this Matrix3D.</returns>
	/// <param name="vector">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> to transform.</param>
	public Vector3D Transform(Vector3D vector)
	{
		MultiplyVector(ref vector);
		return vector;
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> objects in the array by this <see cref="T:System.Windows.Media.Media3D.Matrix3D" />. </summary>
	/// <param name="vectors">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> objects to transform. The original Vector3D objects in the array are replaced by their transformed values.</param>
	public void Transform(Vector3D[] vectors)
	{
		if (vectors != null)
		{
			for (int i = 0; i < vectors.Length; i++)
			{
				MultiplyVector(ref vectors[i]);
			}
		}
	}

	/// <summary>Inverts this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</summary>
	/// <exception cref="T:System.InvalidOperationException">Throws InvalidOperationException if the matrix is not invertible.</exception>
	public void Invert()
	{
		if (!InvertCore())
		{
			throw new InvalidOperationException(SR.Format(SR.Matrix3D_NotInvertible, null));
		}
	}

	internal void SetScaleMatrix(ref Vector3D scale)
	{
		_m11 = scale.X;
		_m22 = scale.Y;
		_m33 = scale.Z;
		_m44 = 1.0;
		IsDistinguishedIdentity = false;
	}

	internal void SetScaleMatrix(ref Vector3D scale, ref Point3D center)
	{
		_m11 = scale.X;
		_m22 = scale.Y;
		_m33 = scale.Z;
		_m44 = 1.0;
		_offsetX = center.X - center.X * scale.X;
		_offsetY = center.Y - center.Y * scale.Y;
		_offsetZ = center.Z - center.Z * scale.Z;
		IsDistinguishedIdentity = false;
	}

	internal void SetTranslationMatrix(ref Vector3D offset)
	{
		_m11 = (_m22 = (_m33 = (_m44 = 1.0)));
		_offsetX = offset.X;
		_offsetY = offset.Y;
		_offsetZ = offset.Z;
		IsDistinguishedIdentity = false;
	}

	internal static Matrix3D CreateRotationMatrix(ref Quaternion quaternion, ref Point3D center)
	{
		Matrix3D result = s_identity;
		result.IsDistinguishedIdentity = false;
		double num = quaternion.X + quaternion.X;
		double num2 = quaternion.Y + quaternion.Y;
		double num3 = quaternion.Z + quaternion.Z;
		double num4 = quaternion.X * num;
		double num5 = quaternion.X * num2;
		double num6 = quaternion.X * num3;
		double num7 = quaternion.Y * num2;
		double num8 = quaternion.Y * num3;
		double num9 = quaternion.Z * num3;
		double num10 = quaternion.W * num;
		double num11 = quaternion.W * num2;
		double num12 = quaternion.W * num3;
		result._m11 = 1.0 - (num7 + num9);
		result._m12 = num5 + num12;
		result._m13 = num6 - num11;
		result._m21 = num5 - num12;
		result._m22 = 1.0 - (num4 + num9);
		result._m23 = num8 + num10;
		result._m31 = num6 + num11;
		result._m32 = num8 - num10;
		result._m33 = 1.0 - (num4 + num7);
		if (center.X != 0.0 || center.Y != 0.0 || center.Z != 0.0)
		{
			result._offsetX = (0.0 - center.X) * result._m11 - center.Y * result._m21 - center.Z * result._m31 + center.X;
			result._offsetY = (0.0 - center.X) * result._m12 - center.Y * result._m22 - center.Z * result._m32 + center.Y;
			result._offsetZ = (0.0 - center.X) * result._m13 - center.Y * result._m23 - center.Z * result._m33 + center.Z;
		}
		return result;
	}

	internal void MultiplyPoint(ref Point3D point)
	{
		if (!IsDistinguishedIdentity)
		{
			double x = point.X;
			double y = point.Y;
			double z = point.Z;
			point.X = x * _m11 + y * _m21 + z * _m31 + _offsetX;
			point.Y = x * _m12 + y * _m22 + z * _m32 + _offsetY;
			point.Z = x * _m13 + y * _m23 + z * _m33 + _offsetZ;
			if (!IsAffine)
			{
				double num = x * _m14 + y * _m24 + z * _m34 + _m44;
				point.X /= num;
				point.Y /= num;
				point.Z /= num;
			}
		}
	}

	internal void MultiplyPoint(ref Point4D point)
	{
		if (!IsDistinguishedIdentity)
		{
			double x = point.X;
			double y = point.Y;
			double z = point.Z;
			double w = point.W;
			point.X = x * _m11 + y * _m21 + z * _m31 + w * _offsetX;
			point.Y = x * _m12 + y * _m22 + z * _m32 + w * _offsetY;
			point.Z = x * _m13 + y * _m23 + z * _m33 + w * _offsetZ;
			point.W = x * _m14 + y * _m24 + z * _m34 + w * _m44;
		}
	}

	internal void MultiplyVector(ref Vector3D vector)
	{
		if (!IsDistinguishedIdentity)
		{
			double x = vector.X;
			double y = vector.Y;
			double z = vector.Z;
			vector.X = x * _m11 + y * _m21 + z * _m31;
			vector.Y = x * _m12 + y * _m22 + z * _m32;
			vector.Z = x * _m13 + y * _m23 + z * _m33;
		}
	}

	internal double GetNormalizedAffineDeterminant()
	{
		double num = _m12 * _m23 - _m22 * _m13;
		double num2 = _m32 * _m13 - _m12 * _m33;
		double num3 = _m22 * _m33 - _m32 * _m23;
		return _m31 * num + _m21 * num2 + _m11 * num3;
	}

	internal bool NormalizedAffineInvert()
	{
		double num = _m12 * _m23 - _m22 * _m13;
		double num2 = _m32 * _m13 - _m12 * _m33;
		double num3 = _m22 * _m33 - _m32 * _m23;
		double num4 = _m31 * num + _m21 * num2 + _m11 * num3;
		if (DoubleUtil.IsZero(num4))
		{
			return false;
		}
		double num5 = _m21 * _m13 - _m11 * _m23;
		double num6 = _m11 * _m33 - _m31 * _m13;
		double num7 = _m31 * _m23 - _m21 * _m33;
		double num8 = _m11 * _m22 - _m21 * _m12;
		double num9 = _m11 * _m32 - _m31 * _m12;
		double num10 = _m11 * _offsetY - _offsetX * _m12;
		double num11 = _m21 * _m32 - _m31 * _m22;
		double num12 = _m21 * _offsetY - _offsetX * _m22;
		double num13 = _m31 * _offsetY - _offsetX * _m32;
		double num14 = _m23 * num10 - _offsetZ * num8 - _m13 * num12;
		double num15 = _m13 * num13 - _m33 * num10 + _offsetZ * num9;
		double num16 = _m33 * num12 - _offsetZ * num11 - _m23 * num13;
		double num17 = num8;
		double num18 = 0.0 - num9;
		double num19 = num11;
		double num20 = 1.0 / num4;
		_m11 = num3 * num20;
		_m12 = num2 * num20;
		_m13 = num * num20;
		_m21 = num7 * num20;
		_m22 = num6 * num20;
		_m23 = num5 * num20;
		_m31 = num19 * num20;
		_m32 = num18 * num20;
		_m33 = num17 * num20;
		_offsetX = num16 * num20;
		_offsetY = num15 * num20;
		_offsetZ = num14 * num20;
		return true;
	}

	internal bool InvertCore()
	{
		if (IsDistinguishedIdentity)
		{
			return true;
		}
		if (IsAffine)
		{
			return NormalizedAffineInvert();
		}
		double num = _m13 * _m24 - _m23 * _m14;
		double num2 = _m13 * _m34 - _m33 * _m14;
		double num3 = _m13 * _m44 - _offsetZ * _m14;
		double num4 = _m23 * _m34 - _m33 * _m24;
		double num5 = _m23 * _m44 - _offsetZ * _m24;
		double num6 = _m33 * _m44 - _offsetZ * _m34;
		double num7 = _m22 * num2 - _m32 * num - _m12 * num4;
		double num8 = _m12 * num5 - _m22 * num3 + _offsetY * num;
		double num9 = _m32 * num3 - _offsetY * num2 - _m12 * num6;
		double num10 = _m22 * num6 - _m32 * num5 + _offsetY * num4;
		double num11 = _offsetX * num7 + _m31 * num8 + _m21 * num9 + _m11 * num10;
		if (DoubleUtil.IsZero(num11))
		{
			return false;
		}
		double num12 = _m11 * num4 - _m21 * num2 + _m31 * num;
		double num13 = _m21 * num3 - _offsetX * num - _m11 * num5;
		double num14 = _m11 * num6 - _m31 * num3 + _offsetX * num2;
		double num15 = _m31 * num5 - _offsetX * num4 - _m21 * num6;
		num = _m11 * _m22 - _m21 * _m12;
		num2 = _m11 * _m32 - _m31 * _m12;
		num3 = _m11 * _offsetY - _offsetX * _m12;
		num4 = _m21 * _m32 - _m31 * _m22;
		num5 = _m21 * _offsetY - _offsetX * _m22;
		num6 = _m31 * _offsetY - _offsetX * _m32;
		double num16 = _m13 * num4 - _m23 * num2 + _m33 * num;
		double num17 = _m23 * num3 - _offsetZ * num - _m13 * num5;
		double num18 = _m13 * num6 - _m33 * num3 + _offsetZ * num2;
		double num19 = _m33 * num5 - _offsetZ * num4 - _m23 * num6;
		double num20 = _m24 * num2 - _m34 * num - _m14 * num4;
		double num21 = _m14 * num5 - _m24 * num3 + _m44 * num;
		double num22 = _m34 * num3 - _m44 * num2 - _m14 * num6;
		double num23 = _m24 * num6 - _m34 * num5 + _m44 * num4;
		double num24 = 1.0 / num11;
		_m11 = num10 * num24;
		_m12 = num9 * num24;
		_m13 = num8 * num24;
		_m14 = num7 * num24;
		_m21 = num15 * num24;
		_m22 = num14 * num24;
		_m23 = num13 * num24;
		_m24 = num12 * num24;
		_m31 = num23 * num24;
		_m32 = num22 * num24;
		_m33 = num21 * num24;
		_m34 = num20 * num24;
		_offsetX = num19 * num24;
		_offsetY = num18 * num24;
		_offsetZ = num17 * num24;
		_m44 = num16 * num24;
		return true;
	}

	private static Matrix3D CreateIdentity()
	{
		Matrix3D result = new Matrix3D(1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0);
		result.IsDistinguishedIdentity = true;
		return result;
	}

	/// <summary> Compares two <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> instances for exact equality. </summary>
	/// <returns>
	///   <see cref="T:System.Boolean" /> that indicates whether the two matrix instances are equal.</returns>
	/// <param name="matrix1">The first matrix to compare.</param>
	/// <param name="matrix2">The second matrix to compare.</param>
	public static bool operator ==(Matrix3D matrix1, Matrix3D matrix2)
	{
		if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
		{
			return matrix1.IsIdentity == matrix2.IsIdentity;
		}
		if (matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M13 == matrix2.M13 && matrix1.M14 == matrix2.M14 && matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 && matrix1.M23 == matrix2.M23 && matrix1.M24 == matrix2.M24 && matrix1.M31 == matrix2.M31 && matrix1.M32 == matrix2.M32 && matrix1.M33 == matrix2.M33 && matrix1.M34 == matrix2.M34 && matrix1.OffsetX == matrix2.OffsetX && matrix1.OffsetY == matrix2.OffsetY && matrix1.OffsetZ == matrix2.OffsetZ)
		{
			return matrix1.M44 == matrix2.M44;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> instances for exact inequality. </summary>
	/// <returns>
	///   <see cref="T:System.Boolean" /> that indicates whether the two matrix instances are inequal.</returns>
	/// <param name="matrix1">The first matrix to compare.</param>
	/// <param name="matrix2">The second matrix to compare.</param>
	public static bool operator !=(Matrix3D matrix1, Matrix3D matrix2)
	{
		return !(matrix1 == matrix2);
	}

	/// <summary> Tests equality between two matrices. </summary>
	/// <returns>
	///   <see cref="T:System.Boolean" /> that indicates whether the matrices are equal.</returns>
	/// <param name="matrix1">The first Matrix3D to compare.</param>
	/// <param name="matrix2">The second Matrix3D to compare.</param>
	public static bool Equals(Matrix3D matrix1, Matrix3D matrix2)
	{
		if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
		{
			return matrix1.IsIdentity == matrix2.IsIdentity;
		}
		if (matrix1.M11.Equals(matrix2.M11) && matrix1.M12.Equals(matrix2.M12) && matrix1.M13.Equals(matrix2.M13) && matrix1.M14.Equals(matrix2.M14) && matrix1.M21.Equals(matrix2.M21) && matrix1.M22.Equals(matrix2.M22) && matrix1.M23.Equals(matrix2.M23) && matrix1.M24.Equals(matrix2.M24) && matrix1.M31.Equals(matrix2.M31) && matrix1.M32.Equals(matrix2.M32) && matrix1.M33.Equals(matrix2.M33) && matrix1.M34.Equals(matrix2.M34) && matrix1.OffsetX.Equals(matrix2.OffsetX) && matrix1.OffsetY.Equals(matrix2.OffsetY) && matrix1.OffsetZ.Equals(matrix2.OffsetZ))
		{
			return matrix1.M44.Equals(matrix2.M44);
		}
		return false;
	}

	/// <summary> Tests equality between two matrices. </summary>
	/// <returns>
	///   <see cref="T:System.Boolean" /> that indicates whether the matrices are equal.</returns>
	/// <param name="o">Object to test for equality.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Matrix3D matrix))
		{
			return false;
		}
		return Equals(this, matrix);
	}

	/// <summary>Tests equality between two matrices. </summary>
	/// <returns>
	///   <see cref="T:System.Boolean" /> that indicates whether the matrices are equal.</returns>
	/// <param name="value">The Matrix3D to which to compare.</param>
	public bool Equals(Matrix3D value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns the hash code for this matrix </summary>
	/// <returns>Integer that specifies the hash code for this matrix.</returns>
	public override int GetHashCode()
	{
		if (IsDistinguishedIdentity)
		{
			return 0;
		}
		return M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^ M14.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^ M24.GetHashCode() ^ M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode() ^ M34.GetHashCode() ^ OffsetX.GetHashCode() ^ OffsetY.GetHashCode() ^ OffsetZ.GetHashCode() ^ M44.GetHashCode();
	}

	/// <summary>Converts a string representation of a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure into the equivalent Matrix3D structure.</summary>
	/// <returns>Matrix3D structure represented by the string.</returns>
	/// <param name="source">String representation of the Matrix3D.</param>
	public static Matrix3D Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Matrix3D result = ((!(text == "Identity")) ? new Matrix3D(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Identity);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a string representation of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>String representation of this Matrix3D structure.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure. </summary>
	/// <returns>String representation of this <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> structure.</returns>
	/// <param name="provider">Culture-specified formatting information.</param>
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
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}{0}{5:" + format + "}{0}{6:" + format + "}{0}{7:" + format + "}{0}{8:" + format + "}{0}{9:" + format + "}{0}{10:" + format + "}{0}{11:" + format + "}{0}{12:" + format + "}{0}{13:" + format + "}{0}{14:" + format + "}{0}{15:" + format + "}{0}{16:" + format + "}", numericListSeparator, _m11, _m12, _m13, _m14, _m21, _m22, _m23, _m24, _m31, _m32, _m33, _m34, _offsetX, _offsetY, _offsetZ, _m44);
	}
}
