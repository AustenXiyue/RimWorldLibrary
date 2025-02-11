using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Media3D.Converters;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Data structure that describes the size of a three-dimensional object. </summary>
[Serializable]
[TypeConverter(typeof(Size3DConverter))]
[ValueSerializer(typeof(Size3DValueSerializer))]
public struct Size3D : IFormattable
{
	private static readonly Size3D s_empty = CreateEmptySize3D();

	internal double _x;

	internal double _y;

	internal double _z;

	/// <summary>Gets a value that represents an empty <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>An empty instance of a <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.</returns>
	public static Size3D Empty => s_empty;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure is empty. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure is empty; otherwise, false.  The default is false.</returns>
	public bool IsEmpty => _x < 0.0;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Media3D.Size3D.X" /> value of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Size3D.X" /> value of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.  The default value is 0.</returns>
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
				throw new InvalidOperationException(SR.Size3D_CannotModifyEmptySize);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
			}
			_x = value;
		}
	}

	/// <summary> Gets or sets the <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> value of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> value of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.  The default value is 0.</returns>
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
				throw new InvalidOperationException(SR.Size3D_CannotModifyEmptySize);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
			}
			_y = value;
		}
	}

	/// <summary> Gets or sets the <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> value of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> value of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.  The default value is 0.</returns>
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
				throw new InvalidOperationException(SR.Size3D_CannotModifyEmptySize);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
			}
			_z = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <param name="x">The new <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure's <see cref="P:System.Windows.Media.Media3D.Size3D.X" /> value.</param>
	/// <param name="y">The new <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure's <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> value.</param>
	/// <param name="z">The new <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure's <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> value.</param>
	public Size3D(double x, double y, double z)
	{
		if (x < 0.0 || y < 0.0 || z < 0.0)
		{
			throw new ArgumentException(SR.Size3D_DimensionCannotBeNegative);
		}
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>Converts this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure into a <see cref="T:System.Windows.Media.Media3D.Vector3D" /> structure. </summary>
	/// <returns>The result of converting <paramref name="size" />.</returns>
	/// <param name="size">The size to convert.</param>
	public static explicit operator Vector3D(Size3D size)
	{
		return new Vector3D(size._x, size._y, size._z);
	}

	/// <summary>Converts this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure into a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The result of converting <paramref name="size" />.</returns>
	/// <param name="size">The size to convert.</param>
	public static explicit operator Point3D(Size3D size)
	{
		return new Point3D(size._x, size._y, size._z);
	}

	private static Size3D CreateEmptySize3D()
	{
		Size3D result = default(Size3D);
		result._x = double.NegativeInfinity;
		result._y = double.NegativeInfinity;
		result._z = double.NegativeInfinity;
		return result;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures for equality.   Two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures are equal if the values of their <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> properties are the same.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> components of <paramref name="size1" /> and <paramref name="size2" /> are equal; otherwise, false.</returns>
	/// <param name="size1">The first <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure to compare.</param>
	/// <param name="size2">The second <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure to compare.</param>
	public static bool operator ==(Size3D size1, Size3D size2)
	{
		if (size1.X == size2.X && size1.Y == size2.Y)
		{
			return size1.Z == size2.Z;
		}
		return false;
	}

	/// <summary> Compares two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures for inequality.  Two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures are not equal if the values of their <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> properties are different.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> coordinates of <paramref name="size1" /> and <paramref name="size2" /> are different; otherwise, false.</returns>
	/// <param name="size1">The first <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure to compare.</param>
	/// <param name="size2">The second <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure to compare.</param>
	public static bool operator !=(Size3D size1, Size3D size2)
	{
		return !(size1 == size2);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures for equality.   Two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures are equal if the values of their <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> properties are the same.</summary>
	/// <returns>true if instances are equal; otherwise, false.true if the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> components of <paramref name="size1" /> and <paramref name="size2" /> are equal; otherwise, false.</returns>
	/// <param name="size1">The first <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure to compare.</param>
	/// <param name="size2">The second <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure to compare.</param>
	public static bool Equals(Size3D size1, Size3D size2)
	{
		if (size1.IsEmpty)
		{
			return size2.IsEmpty;
		}
		if (size1.X.Equals(size2.X) && size1.Y.Equals(size2.Y))
		{
			return size1.Z.Equals(size2.Z);
		}
		return false;
	}

	/// <summary> Determines whether the specified object is a <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure and whether the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> properties of the specified <see cref="T:System.Object" /> are equal to the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" /> and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" />  properties of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.</summary>
	/// <returns>true if instances are equal; otherwise, false.true if <paramref name="o" /> is a <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure and is identical with this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Object" /> to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Size3D size))
		{
			return false;
		}
		return Equals(this, size);
	}

	/// <summary>Compares two <see cref="T:System.Windows.Media.Media3D.Size3D" /> structures for equality.</summary>
	/// <returns>true if instances are equal; otherwise, false.</returns>
	/// <param name="value">The instance of Size3D to compare to this instance.</param>
	public bool Equals(Size3D value)
	{
		return Equals(this, value);
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>A hash code for this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.</returns>
	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}

	/// <summary>Converts a <see cref="T:System.String" /> representation of a three-dimensional size structure into the equivalent <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>The equivalent <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.</returns>
	/// <param name="source">The <see cref="T:System.String" /> representation of the three-dimensional size structure.</param>
	public static Size3D Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Size3D result = ((!(text == "Empty")) ? new Size3D(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Empty);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> values of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure. </summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Media.Media3D.Size3D.X" />, <see cref="P:System.Windows.Media.Media3D.Size3D.Y" />, and <see cref="P:System.Windows.Media.Media3D.Size3D.Z" /> values of this <see cref="T:System.Windows.Media.Media3D.Size3D" /> structure.</returns>
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
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}", numericListSeparator, _x, _y, _z);
	}
}
