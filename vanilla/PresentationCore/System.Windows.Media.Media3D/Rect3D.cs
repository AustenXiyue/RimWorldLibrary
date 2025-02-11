using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a 3-D rectangle: for example, a cube. </summary>
[Serializable]
[TypeConverter(typeof(Rect3DConverter))]
[ValueSerializer(typeof(Rect3DValueSerializer))]
public struct Rect3D : IFormattable
{
	internal static readonly Rect3D Infinite = CreateInfiniteRect3D();

	private static readonly Rect3D s_empty = CreateEmptyRect3D();

	internal double _x;

	internal double _y;

	internal double _z;

	internal double _sizeX;

	internal double _sizeY;

	internal double _sizeZ;

	/// <summary>Gets an empty <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>An empty <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
	public static Rect3D Empty => s_empty;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is the <see cref="P:System.Windows.Media.Media3D.Rect3D.Empty" /> <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is the empty rectangle; false otherwise.</returns>
	public bool IsEmpty => _sizeX < 0.0;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Point3D" /> that represents the origin of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Point3D" /> that represents the origin of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, typically the back bottom left corner.  The default value is (0,0,0).</returns>
	public Point3D Location
	{
		get
		{
			return new Point3D(_x, _y, _z);
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			_x = value._x;
			_y = value._y;
			_z = value._z;
		}
	}

	/// <summary>Gets or sets the area of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Size3D" /> that specifies the area of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
	public Size3D Size
	{
		get
		{
			if (IsEmpty)
			{
				return Size3D.Empty;
			}
			return new Size3D(_sizeX, _sizeY, _sizeZ);
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
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			_sizeX = value._x;
			_sizeY = value._y;
			_sizeZ = value._z;
		}
	}

	/// <summary>Gets or sets the size of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />  in the X dimension. </summary>
	/// <returns>Double that specifies the size of the <see cref="T:System.Windows.Media.Media3D.Rect3D" /> in the X dimension.</returns>
	public double SizeX
	{
		get
		{
			return _sizeX;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
			}
			_sizeX = value;
		}
	}

	/// <summary>Gets or sets the size of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />  in the Y dimension. </summary>
	/// <returns>Double that specifies the size of the <see cref="T:System.Windows.Media.Media3D.Rect3D" /> in the Y dimension.</returns>
	public double SizeY
	{
		get
		{
			return _sizeY;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
			}
			_sizeY = value;
		}
	}

	/// <summary>Gets or sets the size of the Rect3D in the Z dimension. </summary>
	/// <returns>Double that specifies the size of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />  in the Z dimension.</returns>
	public double SizeZ
	{
		get
		{
			return _sizeZ;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
			}
			_sizeZ = value;
		}
	}

	/// <summary>Gets or sets the value of the X coordinate of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>Value of the X coordinate of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
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
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			_x = value;
		}
	}

	/// <summary>Gets or sets the value of the Y coordinate of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>Value of the Y coordinate of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
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
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			_y = value;
		}
	}

	/// <summary>Gets or sets the value of the Z coordinate of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>Value of the Z coordinate of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
	public double Z
	{
		get
		{
			return _z;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException(SR.Rect3D_CannotModifyEmptyRect);
			}
			_z = value;
		}
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> structure. </summary>
	/// <param name="location">Location of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	/// <param name="size">Size of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	public Rect3D(Point3D location, Size3D size)
	{
		if (size.IsEmpty)
		{
			this = s_empty;
			return;
		}
		_x = location._x;
		_y = location._y;
		_z = location._z;
		_sizeX = size._x;
		_sizeY = size._y;
		_sizeZ = size._z;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Rect3D" /> structure. </summary>
	/// <param name="x">X-axis coordinate of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	/// <param name="y">Y-axis coordinate of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	/// <param name="z">Z-axis coordinate of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	/// <param name="sizeX">Size of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" /> in the X dimension.</param>
	/// <param name="sizeY">Size of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" /> in the Y dimension.</param>
	/// <param name="sizeZ">Size of the new <see cref="T:System.Windows.Media.Media3D.Rect3D" /> in the Z dimension.</param>
	public Rect3D(double x, double y, double z, double sizeX, double sizeY, double sizeZ)
	{
		if (sizeX < 0.0 || sizeY < 0.0 || sizeZ < 0.0)
		{
			throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
		}
		_x = x;
		_y = y;
		_z = z;
		_sizeX = sizeX;
		_sizeY = sizeY;
		_sizeZ = sizeZ;
	}

	internal Rect3D(Point3D point1, Point3D point2)
	{
		_x = Math.Min(point1._x, point2._x);
		_y = Math.Min(point1._y, point2._y);
		_z = Math.Min(point1._z, point2._z);
		_sizeX = Math.Max(point1._x, point2._x) - _x;
		_sizeY = Math.Max(point1._y, point2._y) - _y;
		_sizeZ = Math.Max(point1._z, point2._z) - _z;
	}

	internal Rect3D(Point3D point, Vector3D vector)
		: this(point, point + vector)
	{
	}

	/// <summary>Gets a value that indicates whether a specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> is within the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, including its edges. </summary>
	/// <returns>True if the specified point or rectangle is within the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, including its edges; false otherwise.</returns>
	/// <param name="point">Point to be tested.</param>
	public bool Contains(Point3D point)
	{
		return Contains(point._x, point._y, point._z);
	}

	/// <summary>Gets a value that indicates whether a specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> is within the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, including its edges. </summary>
	/// <returns>true if the specified point or rectangle is within the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, including its edges; false otherwise.</returns>
	/// <param name="x">X-axis coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> to be tested.</param>
	/// <param name="y">Y-axis coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> to be tested.</param>
	/// <param name="z">Z-coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> to be tested.</param>
	public bool Contains(double x, double y, double z)
	{
		if (IsEmpty)
		{
			return false;
		}
		return ContainsInternal(x, y, z);
	}

	/// <summary>Gets a value that indicates whether a specified <see cref="T:System.Windows.Media.Media3D.Point3D" /> is within the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, including its edges. </summary>
	/// <returns>True if the specified point or rectangle is within the <see cref="T:System.Windows.Media.Media3D.Rect3D" />, including its edges; false otherwise.</returns>
	/// <param name="rect">
	///   <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to be tested. </param>
	public bool Contains(Rect3D rect)
	{
		if (IsEmpty || rect.IsEmpty)
		{
			return false;
		}
		if (_x <= rect._x && _y <= rect._y && _z <= rect._z && _x + _sizeX >= rect._x + rect._sizeX && _y + _sizeY >= rect._y + rect._sizeY)
		{
			return _z + _sizeZ >= rect._z + rect._sizeZ;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> intersects with this <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> intersects with this <see cref="T:System.Windows.Media.Media3D.Rect3D" />; false otherwise.</returns>
	/// <param name="rect">Rectangle to evaluate.</param>
	public bool IntersectsWith(Rect3D rect)
	{
		if (IsEmpty || rect.IsEmpty)
		{
			return false;
		}
		if (rect._x <= _x + _sizeX && rect._x + rect._sizeX >= _x && rect._y <= _y + _sizeY && rect._y + rect._sizeY >= _y && rect._z <= _z + _sizeZ)
		{
			return rect._z + rect._sizeZ >= _z;
		}
		return false;
	}

	/// <summary>Finds the intersection of the current <see cref="T:System.Windows.Media.Media3D.Rect3D" /> and the specified <see cref="T:System.Windows.Media.Media3D.Rect3D" />, and stores the result as the current <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</summary>
	/// <param name="rect">The <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to intersect with the current <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	public void Intersect(Rect3D rect)
	{
		if (IsEmpty || rect.IsEmpty || !IntersectsWith(rect))
		{
			this = Empty;
			return;
		}
		double num = Math.Max(_x, rect._x);
		double num2 = Math.Max(_y, rect._y);
		double num3 = Math.Max(_z, rect._z);
		_sizeX = Math.Min(_x + _sizeX, rect._x + rect._sizeX) - num;
		_sizeY = Math.Min(_y + _sizeY, rect._y + rect._sizeY) - num2;
		_sizeZ = Math.Min(_z + _sizeZ, rect._z + rect._sizeZ) - num3;
		_x = num;
		_y = num2;
		_z = num3;
	}

	/// <summary>Returns the intersection of the specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> values.</summary>
	/// <returns>Result of the intersection of <paramref name="rect1" /> and <paramref name="rect2" />.</returns>
	/// <param name="rect1">First <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	/// <param name="rect2">Second <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	public static Rect3D Intersect(Rect3D rect1, Rect3D rect2)
	{
		rect1.Intersect(rect2);
		return rect1;
	}

	/// <summary>Updates a specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to reflect the union of that <see cref="T:System.Windows.Media.Media3D.Rect3D" /> and a second specified <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <param name="rect">The <see cref="T:System.Windows.Media.Media3D.Rect3D" /> whose union with the current <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is to be evaluated.</param>
	public void Union(Rect3D rect)
	{
		if (IsEmpty)
		{
			this = rect;
		}
		else if (!rect.IsEmpty)
		{
			double num = Math.Min(_x, rect._x);
			double num2 = Math.Min(_y, rect._y);
			double num3 = Math.Min(_z, rect._z);
			_sizeX = Math.Max(_x + _sizeX, rect._x + rect._sizeX) - num;
			_sizeY = Math.Max(_y + _sizeY, rect._y + rect._sizeY) - num2;
			_sizeZ = Math.Max(_z + _sizeZ, rect._z + rect._sizeZ) - num3;
			_x = num;
			_y = num2;
			_z = num3;
		}
	}

	/// <summary>Returns a new instance of <see cref="T:System.Windows.Media.Media3D.Rect3D" /> that represents the union of two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> objects. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Rect3D" /> value that represents the result of the union of <paramref name="rect1" /> and <paramref name="rect2" />.</returns>
	/// <param name="rect1">First <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	/// <param name="rect2">Second <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	public static Rect3D Union(Rect3D rect1, Rect3D rect2)
	{
		rect1.Union(rect2);
		return rect1;
	}

	/// <summary>Updates a specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to reflect the union of that <see cref="T:System.Windows.Media.Media3D.Rect3D" /> and a specified <see cref="T:System.Windows.Media.Media3D.Point3D" />. </summary>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> whose union with the specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is to be evaluated.</param>
	public void Union(Point3D point)
	{
		Union(new Rect3D(point, point));
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Media.Media3D.Rect3D" /> that represents the union of a <see cref="T:System.Windows.Media.Media3D.Rect3D" />, and a specified <see cref="T:System.Windows.Media.Media3D.Point3D" />.</summary>
	/// <returns>Result of the union of <paramref name="rect" /> and <paramref name="point" />.</returns>
	/// <param name="rect">The <see cref="T:System.Windows.Media.Media3D.Rect3D" /> whose union with the current <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is to be evaluated.</param>
	/// <param name="point">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> whose union with the specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is to be evaluated.</param>
	public static Rect3D Union(Rect3D rect, Point3D point)
	{
		rect.Union(new Rect3D(point, point));
		return rect;
	}

	/// <summary>Sets the offset translation of the <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to the provided value, specified as a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <param name="offsetVector">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the offset translation.</param>
	public void Offset(Vector3D offsetVector)
	{
		Offset(offsetVector._x, offsetVector._y, offsetVector._z);
	}

	/// <summary>Gets or sets an offset value by which the location of a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is translated. </summary>
	/// <param name="offsetX">Offset along the X axis.</param>
	/// <param name="offsetY">Offset along the Y axis.</param>
	/// <param name="offsetZ">Offset along the Z axis.</param>
	public void Offset(double offsetX, double offsetY, double offsetZ)
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(SR.Rect3D_CannotCallMethod);
		}
		_x += offsetX;
		_y += offsetY;
		_z += offsetZ;
	}

	/// <summary>Gets or sets an offset value by which the location of a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is translated. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Rect3D" /> value that represents the result of the offset.</returns>
	/// <param name="rect">
	///   <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to be translated.</param>
	/// <param name="offsetVector">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the offset translation.</param>
	public static Rect3D Offset(Rect3D rect, Vector3D offsetVector)
	{
		rect.Offset(offsetVector._x, offsetVector._y, offsetVector._z);
		return rect;
	}

	/// <summary>Gets or sets an offset value by which the location of a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> is translated. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Rect3D" /> value that represents the result of the offset.</returns>
	/// <param name="rect">Rect3D to be translated.</param>
	/// <param name="offsetX">Offset along the X axis.</param>
	/// <param name="offsetY">Offset along the Y axis.</param>
	/// <param name="offsetZ">Offset along the Z axis.</param>
	public static Rect3D Offset(Rect3D rect, double offsetX, double offsetY, double offsetZ)
	{
		rect.Offset(offsetX, offsetY, offsetZ);
		return rect;
	}

	private bool ContainsInternal(double x, double y, double z)
	{
		if (x >= _x && x <= _x + _sizeX && y >= _y && y <= _y + _sizeY && z >= _z)
		{
			return z <= _z + _sizeZ;
		}
		return false;
	}

	private static Rect3D CreateEmptyRect3D()
	{
		Rect3D result = default(Rect3D);
		result._x = double.PositiveInfinity;
		result._y = double.PositiveInfinity;
		result._z = double.PositiveInfinity;
		result._sizeX = double.NegativeInfinity;
		result._sizeY = double.NegativeInfinity;
		result._sizeZ = double.NegativeInfinity;
		return result;
	}

	private static Rect3D CreateInfiniteRect3D()
	{
		Rect3D result = default(Rect3D);
		result._x = -3.4028234663852886E+38;
		result._y = -3.4028234663852886E+38;
		result._z = -3.4028234663852886E+38;
		result._sizeX = 6.805646932770577E+38;
		result._sizeY = 6.805646932770577E+38;
		result._sizeZ = 6.805646932770577E+38;
		return result;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances for exact equality. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances are exactly equal; false otherwise.</returns>
	/// <param name="rect1">First <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to evaluate.</param>
	/// <param name="rect2">Second <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to evaluate.</param>
	public static bool operator ==(Rect3D rect1, Rect3D rect2)
	{
		if (rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Z == rect2.Z && rect1.SizeX == rect2.SizeX && rect1.SizeY == rect2.SizeY)
		{
			return rect1.SizeZ == rect2.SizeZ;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances for exact inequality. </summary>
	/// <returns>True if the two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances are unequal, false otherwise.</returns>
	/// <param name="rect1">First <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to compare.</param>
	/// <param name="rect2">Second <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to compare.</param>
	public static bool operator !=(Rect3D rect1, Rect3D rect2)
	{
		return !(rect1 == rect2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances for equality. </summary>
	/// <returns>true if the two specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances are exactly equal; false otherwise.</returns>
	/// <param name="rect1">First <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to compare.</param>
	/// <param name="rect2">Second <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to compare.</param>
	public static bool Equals(Rect3D rect1, Rect3D rect2)
	{
		if (rect1.IsEmpty)
		{
			return rect2.IsEmpty;
		}
		if (rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Z.Equals(rect2.Z) && rect1.SizeX.Equals(rect2.SizeX) && rect1.SizeY.Equals(rect2.SizeY))
		{
			return rect1.SizeZ.Equals(rect2.SizeZ);
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances for equality. </summary>
	/// <returns>true if the two specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances are exactly equal; false otherwise.</returns>
	/// <param name="o">The object to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Rect3D rect))
		{
			return false;
		}
		return Equals(this, rect);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances for equality. </summary>
	/// <returns>true if the two specified <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instances are exactly equal; false otherwise.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.Media3D.Rect3D" /> instance to compare with the current instance.</param>
	public bool Equals(Rect3D value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns the hash code for the <see cref="T:System.Windows.Media.Media3D.Rect3D" /></summary>
	/// <returns>A hash code for this <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ SizeX.GetHashCode() ^ SizeY.GetHashCode() ^ SizeZ.GetHashCode();
	}

	/// <summary>Converts a string representation of a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> into the equivalent <see cref="T:System.Windows.Media.Media3D.Rect3D" /> structure. </summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
	/// <param name="source">String that represents a <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</param>
	public static Rect3D Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Rect3D result = ((!(text == "Empty")) ? new Rect3D(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Empty);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a string representation of the Rect3D. </summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</returns>
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
		if (IsEmpty)
		{
			return "Empty";
		}
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}{0}{5:" + format + "}{0}{6:" + format + "}", numericListSeparator, _x, _y, _z, _sizeX, _sizeY, _sizeZ);
	}
}
