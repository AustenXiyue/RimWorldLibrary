using System.ComponentModel;
using System.Windows.Converters;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Implements a structure that is used to describe the <see cref="T:System.Windows.Size" /> of an object. </summary>
[Serializable]
[TypeConverter(typeof(SizeConverter))]
[ValueSerializer(typeof(SizeValueSerializer))]
public struct Size : IFormattable
{
	internal double _width;

	internal double _height;

	private static readonly Size s_empty = CreateEmptySize();

	/// <summary>Gets a value that represents a static empty <see cref="T:System.Windows.Size" />. </summary>
	/// <returns>An empty instance of <see cref="T:System.Windows.Size" />.</returns>
	public static Size Empty => s_empty;

	/// <summary>Gets a value that indicates whether this instance of <see cref="T:System.Windows.Size" /> is <see cref="P:System.Windows.Size.Empty" />. </summary>
	/// <returns>true if this instance of size is <see cref="P:System.Windows.Size.Empty" />; otherwise false.</returns>
	public bool IsEmpty => _width < 0.0;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Size.Width" /> of this instance of <see cref="T:System.Windows.Size" />. </summary>
	/// <returns>The <see cref="P:System.Windows.Size.Width" /> of this instance of <see cref="T:System.Windows.Size" />. The default value is 0. The value cannot be negative.</returns>
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
				throw new InvalidOperationException(SR.Size_CannotModifyEmptySize);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size_WidthCannotBeNegative);
			}
			_width = value;
		}
	}

	/// <summary>Gets or sets the <see cref="P:System.Windows.Size.Height" /> of this instance of <see cref="T:System.Windows.Size" />. </summary>
	/// <returns>The <see cref="P:System.Windows.Size.Height" /> of this instance of <see cref="T:System.Windows.Size" />. The default is 0. The value cannot be negative.</returns>
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
				throw new InvalidOperationException(SR.Size_CannotModifyEmptySize);
			}
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Size_HeightCannotBeNegative);
			}
			_height = value;
		}
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.Size" /> for equality. </summary>
	/// <returns>true if the two instances of <see cref="T:System.Windows.Size" /> are equal; otherwise false.</returns>
	/// <param name="size1">The first instance of <see cref="T:System.Windows.Size" /> to compare.</param>
	/// <param name="size2">The second instance of <see cref="T:System.Windows.Size" /> to compare.</param>
	public static bool operator ==(Size size1, Size size2)
	{
		if (size1.Width == size2.Width)
		{
			return size1.Height == size2.Height;
		}
		return false;
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.Size" /> for inequality. </summary>
	/// <returns>true if the instances of <see cref="T:System.Windows.Size" /> are not equal; otherwise false.</returns>
	/// <param name="size1">The first instance of <see cref="T:System.Windows.Size" /> to compare.</param>
	/// <param name="size2">The second instance of <see cref="T:System.Windows.Size" /> to compare.</param>
	public static bool operator !=(Size size1, Size size2)
	{
		return !(size1 == size2);
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.Size" /> for equality. </summary>
	/// <returns>true if the instances of <see cref="T:System.Windows.Size" /> are equal; otherwise, false.</returns>
	/// <param name="size1">The first instance of <see cref="T:System.Windows.Size" /> to compare.</param>
	/// <param name="size2">The second instance of <see cref="T:System.Windows.Size" /> to compare.</param>
	public static bool Equals(Size size1, Size size2)
	{
		if (size1.IsEmpty)
		{
			return size2.IsEmpty;
		}
		if (size1.Width.Equals(size2.Width))
		{
			return size1.Height.Equals(size2.Height);
		}
		return false;
	}

	/// <summary>Compares an object to an instance of <see cref="T:System.Windows.Size" /> for equality. </summary>
	/// <returns>true if the sizes are equal; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Object" /> to compare.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is Size size))
		{
			return false;
		}
		return Equals(this, size);
	}

	/// <summary>Compares a value to an instance of <see cref="T:System.Windows.Size" /> for equality. </summary>
	/// <returns>true if the instances of <see cref="T:System.Windows.Size" /> are equal; otherwise, false.</returns>
	/// <param name="value">The size to compare to this current instance of <see cref="T:System.Windows.Size" />.</param>
	public bool Equals(Size value)
	{
		return Equals(this, value);
	}

	/// <summary>Gets the hash code for this instance of <see cref="T:System.Windows.Size" />. </summary>
	/// <returns>The hash code for this instance of <see cref="T:System.Windows.Size" />.</returns>
	public override int GetHashCode()
	{
		if (IsEmpty)
		{
			return 0;
		}
		return Width.GetHashCode() ^ Height.GetHashCode();
	}

	/// <summary>Returns an instance of <see cref="T:System.Windows.Size" /> from a converted <see cref="T:System.String" />. </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Size" />.</returns>
	/// <param name="source">A <see cref="T:System.String" /> value to parse to a <see cref="T:System.Windows.Size" /> value.</param>
	public static Size Parse(string source)
	{
		IFormatProvider invariantEnglishUS = TypeConverterHelper.InvariantEnglishUS;
		TokenizerHelper tokenizerHelper = new TokenizerHelper(source, invariantEnglishUS);
		string text = tokenizerHelper.NextTokenRequired();
		Size result = ((!(text == "Empty")) ? new Size(Convert.ToDouble(text, invariantEnglishUS), Convert.ToDouble(tokenizerHelper.NextTokenRequired(), invariantEnglishUS)) : Empty);
		tokenizerHelper.LastTokenRequired();
		return result;
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents this <see cref="T:System.Windows.Size" /> object. </summary>
	/// <returns>A <see cref="T:System.String" /> that specifies the width followed by the height.</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents this instance of <see cref="T:System.Windows.Size" />. </summary>
	/// <returns>A <see cref="T:System.String" /> that represents this <see cref="T:System.Windows.Size" /> object.</returns>
	/// <param name="provider">An object that provides a way to control formatting.</param>
	public string ToString(IFormatProvider provider)
	{
		return ConvertToString(null, provider);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns> The value of the current instance in the specified format.</returns>
	/// <param name="format"> The format to use.</param>
	/// <param name="provider"> The provider to use to format the value.</param>
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
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}", numericListSeparator, _width, _height);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Size" /> structure and assigns it an initial <paramref name="width" /> and <paramref name="height" />.</summary>
	/// <param name="width">The initial width of the instance of <see cref="T:System.Windows.Size" />.</param>
	/// <param name="height">The initial height of the instance of <see cref="T:System.Windows.Size" />.</param>
	public Size(double width, double height)
	{
		if (width < 0.0 || height < 0.0)
		{
			throw new ArgumentException(SR.Size_WidthAndHeightCannotBeNegative);
		}
		_width = width;
		_height = height;
	}

	/// <summary>Explicitly converts an instance of <see cref="T:System.Windows.Size" /> to an instance of <see cref="T:System.Windows.Vector" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Vector" /> equal in value to this instance of <see cref="T:System.Windows.Size" />.</returns>
	/// <param name="size">The <see cref="T:System.Windows.Size" /> value to be converted.</param>
	public static explicit operator Vector(Size size)
	{
		return new Vector(size._width, size._height);
	}

	/// <summary>Explicitly converts an instance of <see cref="T:System.Windows.Size" /> to an instance of <see cref="T:System.Windows.Point" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> equal in value to this instance of <see cref="T:System.Windows.Size" />.</returns>
	/// <param name="size">The <see cref="T:System.Windows.Size" /> value to be converted.</param>
	public static explicit operator Point(Size size)
	{
		return new Point(size._width, size._height);
	}

	private static Size CreateEmptySize()
	{
		Size result = default(Size);
		result._width = double.NegativeInfinity;
		result._height = double.NegativeInfinity;
		return result;
	}
}
