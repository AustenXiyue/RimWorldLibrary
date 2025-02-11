using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>Represents the length of elements that explicitly support <see cref="F:System.Windows.GridUnitType.Star" /> unit types. </summary>
[TypeConverter(typeof(GridLengthConverter))]
public struct GridLength : IEquatable<GridLength>
{
	private double _unitValue;

	private GridUnitType _unitType;

	private static readonly GridLength s_auto = new GridLength(1.0, GridUnitType.Auto);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.GridLength" /> holds a value that is expressed in pixels. </summary>
	/// <returns>true if the <see cref="P:System.Windows.GridLength.GridUnitType" /> property is <see cref="F:System.Windows.GridUnitType.Pixel" />; otherwise, false.</returns>
	public bool IsAbsolute => _unitType == GridUnitType.Pixel;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.GridLength" /> holds a value whose size is determined by the size properties of the content object. </summary>
	/// <returns>true if the <see cref="P:System.Windows.GridLength.GridUnitType" /> property is <see cref="F:System.Windows.GridUnitType.Auto" />; otherwise, false. </returns>
	public bool IsAuto => _unitType == GridUnitType.Auto;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.GridLength" /> holds a value that is expressed as a weighted proportion of available space. </summary>
	/// <returns>true if the <see cref="P:System.Windows.GridLength.GridUnitType" /> property is <see cref="F:System.Windows.GridUnitType.Star" />; otherwise, false. </returns>
	public bool IsStar => _unitType == GridUnitType.Star;

	/// <summary>Gets a <see cref="T:System.Double" /> that represents the value of the <see cref="T:System.Windows.GridLength" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the value of the current instance. </returns>
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

	/// <summary>Gets the associated <see cref="T:System.Windows.GridUnitType" /> for the <see cref="T:System.Windows.GridLength" />. </summary>
	/// <returns>One of the <see cref="T:System.Windows.GridUnitType" /> values. The default is <see cref="F:System.Windows.GridUnitType.Auto" />.</returns>
	public GridUnitType GridUnitType => _unitType;

	/// <summary>Gets an instance of <see cref="T:System.Windows.GridLength" /> that holds a value whose size is determined by the size properties of the content object.</summary>
	/// <returns>A instance of <see cref="T:System.Windows.GridLength" /> whose <see cref="P:System.Windows.GridLength.GridUnitType" /> property is set to <see cref="F:System.Windows.GridUnitType.Auto" />. </returns>
	public static GridLength Auto => s_auto;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.GridLength" /> structure using the specified absolute value in pixels. </summary>
	/// <param name="pixels">The number of device-independent pixels (96 pixels-per-inch).</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Pixels" /> is equal to <see cref="F:System.Double.NegativeInfinity" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NaN" />.</exception>
	public GridLength(double pixels)
		: this(pixels, GridUnitType.Pixel)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.GridLength" /> structure and specifies what kind of value it holds. </summary>
	/// <param name="value">The initial value of this instance of <see cref="T:System.Windows.GridLength" />.</param>
	/// <param name="type">The <see cref="T:System.Windows.GridUnitType" /> held by this instance of <see cref="T:System.Windows.GridLength" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is equal to <see cref="F:System.Double.NegativeInfinity" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NaN" />.</exception>
	public GridLength(double value, GridUnitType type)
	{
		if (double.IsNaN(value))
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterNoNaN, "value"));
		}
		if (double.IsInfinity(value))
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterNoInfinity, "value"));
		}
		if (type != 0 && type != GridUnitType.Pixel && type != GridUnitType.Star)
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterUnknownGridUnitType, "type"));
		}
		_unitValue = ((type == GridUnitType.Auto) ? 0.0 : value);
		_unitType = type;
	}

	/// <summary>Compares two <see cref="T:System.Windows.GridLength" /> structures for equality.</summary>
	/// <returns>true if the two instances of <see cref="T:System.Windows.GridLength" /> have the same value and <see cref="T:System.Windows.GridUnitType" />; otherwise, false.</returns>
	/// <param name="gl1">The first instance of <see cref="T:System.Windows.GridLength" /> to compare.</param>
	/// <param name="gl2">The second instance of <see cref="T:System.Windows.GridLength" /> to compare.</param>
	public static bool operator ==(GridLength gl1, GridLength gl2)
	{
		if (gl1.GridUnitType == gl2.GridUnitType)
		{
			return gl1.Value == gl2.Value;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.GridLength" /> structures to determine if they are not equal.</summary>
	/// <returns>true if the two instances of <see cref="T:System.Windows.GridLength" /> do not have the same value and <see cref="T:System.Windows.GridUnitType" />; otherwise, false.</returns>
	/// <param name="gl1">The first instance of <see cref="T:System.Windows.GridLength" /> to compare.</param>
	/// <param name="gl2">The second instance of <see cref="T:System.Windows.GridLength" /> to compare.</param>
	public static bool operator !=(GridLength gl1, GridLength gl2)
	{
		if (gl1.GridUnitType == gl2.GridUnitType)
		{
			return gl1.Value != gl2.Value;
		}
		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.GridLength" /> instance. </summary>
	/// <returns>true if the specified object has the same value and <see cref="T:System.Windows.GridUnitType" /> as the current instance; otherwise, false.</returns>
	/// <param name="oCompare">The object to compare with the current instance.</param>
	public override bool Equals(object oCompare)
	{
		if (oCompare is GridLength gridLength)
		{
			return this == gridLength;
		}
		return false;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.GridLength" /> is equal to the current <see cref="T:System.Windows.GridLength" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.GridLength" /> has the same value and <see cref="P:System.Windows.GridLength.GridUnitType" /> as the current instance; otherwise, false.</returns>
	/// <param name="gridLength">The <see cref="T:System.Windows.GridLength" /> structure to compare with the current instance.</param>
	public bool Equals(GridLength gridLength)
	{
		return this == gridLength;
	}

	/// <summary>Gets a hash code for the <see cref="T:System.Windows.GridLength" />. </summary>
	/// <returns>A hash code for the current <see cref="T:System.Windows.GridLength" /> structure.</returns>
	public override int GetHashCode()
	{
		return (int)((int)_unitValue + _unitType);
	}

	/// <summary>Returns a <see cref="T:System.String" /> representation of the <see cref="T:System.Windows.GridLength" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the current <see cref="T:System.Windows.GridLength" /> structure.</returns>
	public override string ToString()
	{
		return GridLengthConverter.ToString(this, CultureInfo.InvariantCulture);
	}
}
