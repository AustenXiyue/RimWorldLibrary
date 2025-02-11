using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Controls;

/// <summary>Represents the lengths of elements within the <see cref="T:System.Windows.Controls.DataGrid" /> control. </summary>
[TypeConverter(typeof(DataGridLengthConverter))]
public struct DataGridLength : IEquatable<DataGridLength>
{
	private double _unitValue;

	private DataGridLengthUnitType _unitType;

	private double _desiredValue;

	private double _displayValue;

	private const double AutoValue = 1.0;

	private static readonly DataGridLength _auto = new DataGridLength(1.0, DataGridLengthUnitType.Auto, 0.0, 0.0);

	private static readonly DataGridLength _sizeToCells = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells, 0.0, 0.0);

	private static readonly DataGridLength _sizeToHeader = new DataGridLength(1.0, DataGridLengthUnitType.SizeToHeader, 0.0, 0.0);

	/// <summary>Gets a value that indicates whether this instance sizes elements based on a fixed pixel value.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.DataGridLength.UnitType" /> property is set to <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Pixel" />; otherwise, false.</returns>
	public bool IsAbsolute => _unitType == DataGridLengthUnitType.Pixel;

	/// <summary>Gets a value that indicates whether this instance automatically sizes elements based on both the content of cells and the column headers.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.DataGridLength.UnitType" /> property is set to <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Auto" />; otherwise, false.</returns>
	public bool IsAuto => _unitType == DataGridLengthUnitType.Auto;

	/// <summary>Gets a value that indicates whether this instance automatically sizes elements based on a weighted proportion of available space.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.DataGridLength.UnitType" /> property is set to <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Star" />; otherwise, false.</returns>
	public bool IsStar => _unitType == DataGridLengthUnitType.Star;

	/// <summary>Gets a value that indicates whether this instance automatically sizes elements based on the content of the cells.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.DataGridLength.UnitType" /> property is set to <see cref="F:System.Windows.Controls.DataGridLengthUnitType.SizeToCells" />; otherwise, false.</returns>
	public bool IsSizeToCells => _unitType == DataGridLengthUnitType.SizeToCells;

	/// <summary>Gets a value that indicates whether this instance automatically sizes elements based on the header.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.DataGridLength.UnitType" /> property is set to <see cref="F:System.Windows.Controls.DataGridLengthUnitType.SizeToHeader" />; otherwise, false.</returns>
	public bool IsSizeToHeader => _unitType == DataGridLengthUnitType.SizeToHeader;

	/// <summary>Gets the absolute value of the <see cref="T:System.Windows.Controls.DataGridLength" /> in pixels.</summary>
	/// <returns>The absolute value of the <see cref="T:System.Windows.Controls.DataGridLength" /> in pixels, or 1.0 if the <see cref="P:System.Windows.Controls.DataGridLength.UnitType" /> property is set to <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Auto" />.</returns>
	public double Value
	{
		get
		{
			if (_unitType != 0)
			{
				return _unitValue;
			}
			return 1.0;
		}
	}

	/// <summary>Gets the type that is used to determine how the size of the element is calculated.</summary>
	/// <returns>A type that represents how size is determined.</returns>
	public DataGridLengthUnitType UnitType => _unitType;

	/// <summary>Gets the calculated pixel value needed for the element.</summary>
	/// <returns>The number of pixels calculated for the size of the element.</returns>
	public double DesiredValue => _desiredValue;

	/// <summary>Gets the pixel value allocated for the size of the element.</summary>
	/// <returns>The number of pixels allocated for the element.</returns>
	public double DisplayValue => _displayValue;

	/// <summary>Gets a <see cref="T:System.Windows.Controls.DataGridLength" /> structure that represents the standard automatic sizing mode.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.DataGridLength" /> structure that represents the standard automatic sizing mode.</returns>
	public static DataGridLength Auto => _auto;

	/// <summary>Gets a <see cref="T:System.Windows.Controls.DataGridLength" /> structure that represents the cell-based automatic sizing mode.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.DataGridLength" /> structure that represents the cell-based automatic sizing mode.</returns>
	public static DataGridLength SizeToCells => _sizeToCells;

	/// <summary>Gets a <see cref="T:System.Windows.Controls.DataGridLength" /> structure that represents the header-based automatic sizing mode.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.DataGridLength" /> structure that represents the header-based automatic sizing mode.</returns>
	public static DataGridLength SizeToHeader => _sizeToHeader;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class with an absolute value in pixels.</summary>
	/// <param name="pixels">The absolute pixel value (96 pixels-per-inch) to initialize the length to.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pixels" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />.</exception>
	public DataGridLength(double pixels)
		: this(pixels, DataGridLengthUnitType.Pixel)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class with a specified value and unit.</summary>
	/// <param name="value">The requested size of the element.</param>
	/// <param name="type">The type that is used to determine how the size of the element is calculated.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="type" /> is not <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Auto" />, <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Pixel" />, <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Star" />, <see cref="F:System.Windows.Controls.DataGridLengthUnitType.SizeToCells" />, or <see cref="F:System.Windows.Controls.DataGridLengthUnitType.SizeToHeader" />.</exception>
	public DataGridLength(double value, DataGridLengthUnitType type)
		: this(value, type, (type == DataGridLengthUnitType.Pixel) ? value : double.NaN, (type == DataGridLengthUnitType.Pixel) ? value : double.NaN)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class with the specified value, unit, desired value, and display value.</summary>
	/// <param name="value">The requested size of the element.</param>
	/// <param name="type">The type that is used to determine how the size of the element is calculated.</param>
	/// <param name="desiredValue">The calculated size needed for the element.</param>
	/// <param name="displayValue">The allocated size for the element.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="type" /> is not <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Auto" />, <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Pixel" />, <see cref="F:System.Windows.Controls.DataGridLengthUnitType.Star" />, <see cref="F:System.Windows.Controls.DataGridLengthUnitType.SizeToCells" />, or <see cref="F:System.Windows.Controls.DataGridLengthUnitType.SizeToHeader" />.-or-<paramref name="desiredValue" /> is <see cref="F:System.Double.NegativeInfinity" /> or <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="displayValue" /> is <see cref="F:System.Double.NegativeInfinity" /> or <see cref="F:System.Double.PositiveInfinity" />.</exception>
	public DataGridLength(double value, DataGridLengthUnitType type, double desiredValue, double displayValue)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			throw new ArgumentException(SR.DataGridLength_Infinity, "value");
		}
		if (type != 0 && type != DataGridLengthUnitType.Pixel && type != DataGridLengthUnitType.Star && type != DataGridLengthUnitType.SizeToCells && type != DataGridLengthUnitType.SizeToHeader)
		{
			throw new ArgumentException(SR.DataGridLength_InvalidType, "type");
		}
		if (double.IsInfinity(desiredValue))
		{
			throw new ArgumentException(SR.DataGridLength_Infinity, "desiredValue");
		}
		if (double.IsInfinity(displayValue))
		{
			throw new ArgumentException(SR.DataGridLength_Infinity, "displayValue");
		}
		_unitValue = ((type == DataGridLengthUnitType.Auto) ? 1.0 : value);
		_unitType = type;
		_desiredValue = desiredValue;
		_displayValue = displayValue;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.DataGridLength" /> structures for equality.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Controls.DataGridLength" /> instances have the same value or sizing mode; otherwise, false.</returns>
	/// <param name="gl1">The first <see cref="T:System.Windows.Controls.DataGridLength" /> instance to compare.</param>
	/// <param name="gl2">The second <see cref="T:System.Windows.Controls.DataGridLength" /> instance to compare.</param>
	public static bool operator ==(DataGridLength gl1, DataGridLength gl2)
	{
		if (gl1.UnitType == gl2.UnitType && gl1.Value == gl2.Value && (gl1.DesiredValue == gl2.DesiredValue || (double.IsNaN(gl1.DesiredValue) && double.IsNaN(gl2.DesiredValue))))
		{
			if (gl1.DisplayValue != gl2.DisplayValue)
			{
				if (double.IsNaN(gl1.DisplayValue))
				{
					return double.IsNaN(gl2.DisplayValue);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.Controls.DataGridLength" /> structures to determine whether they are not equal.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Controls.DataGridLength" /> instances do not have the same value or sizing mode; otherwise, false.</returns>
	/// <param name="gl1">The first <see cref="T:System.Windows.Controls.DataGridLength" /> instance to compare.</param>
	/// <param name="gl2">The second <see cref="T:System.Windows.Controls.DataGridLength" /> instance to compare.</param>
	public static bool operator !=(DataGridLength gl1, DataGridLength gl2)
	{
		if (gl1.UnitType == gl2.UnitType && gl1.Value == gl2.Value && (gl1.DesiredValue == gl2.DesiredValue || (double.IsNaN(gl1.DesiredValue) && double.IsNaN(gl2.DesiredValue))))
		{
			if (gl1.DisplayValue != gl2.DisplayValue)
			{
				if (double.IsNaN(gl1.DisplayValue))
				{
					return !double.IsNaN(gl2.DisplayValue);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Controls.DataGridLength" />.</summary>
	/// <returns>true if the specified object is a <see cref="T:System.Windows.Controls.DataGridLength" /> with the same value or sizing mode as the current <see cref="T:System.Windows.Controls.DataGridLength" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare to the current instance.</param>
	public override bool Equals(object obj)
	{
		if (obj is DataGridLength dataGridLength)
		{
			return this == dataGridLength;
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Controls.DataGridLength" /> is equal to the current <see cref="T:System.Windows.Controls.DataGridLength" />.</summary>
	/// <returns>true if the specified object is a <see cref="T:System.Windows.Controls.DataGridLength" /> with the same value or sizing mode as the current <see cref="T:System.Windows.Controls.DataGridLength" />; otherwise, false.</returns>
	/// <param name="other">The <see cref="T:System.Windows.Controls.DataGridLength" /> to compare to the current instance.</param>
	public bool Equals(DataGridLength other)
	{
		return this == other;
	}

	/// <summary>Gets a hash code for the <see cref="T:System.Windows.Controls.DataGridLength" />.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Windows.Controls.DataGridLength" />.</returns>
	public override int GetHashCode()
	{
		return (int)((int)_unitValue + _unitType + (int)_desiredValue + (int)_displayValue);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represent the current object.</returns>
	public override string ToString()
	{
		return DataGridLengthConverter.ConvertToString(this, CultureInfo.InvariantCulture);
	}

	/// <summary>Converts a <see cref="T:System.Double" /> to an instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class.</summary>
	/// <returns>An object that represents the specified length.</returns>
	/// <param name="value">The absolute pixel value (96 pixels-per-inch) to initialize the length to.</param>
	public static implicit operator DataGridLength(double value)
	{
		return new DataGridLength(value);
	}
}
