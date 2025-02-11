using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>Describes the height or width of a <see cref="T:System.Windows.Documents.Figure" />.</summary>
[TypeConverter(typeof(FigureLengthConverter))]
public struct FigureLength : IEquatable<FigureLength>
{
	private double _unitValue;

	private FigureUnitType _unitType;

	/// <summary>Gets a value that determines whether this <see cref="T:System.Windows.FigureLength" /> holds an absolute value (in pixels).</summary>
	/// <returns>true if this <see cref="T:System.Windows.FigureLength" /> holds an absolute value (in pixels); otherwise, false. The default value is false.</returns>
	public bool IsAbsolute => _unitType == FigureUnitType.Pixel;

	/// <summary>Gets a value that determines whether this <see cref="T:System.Windows.FigureLength" /> is automatic (not specified).</summary>
	/// <returns>true if this <see cref="T:System.Windows.FigureLength" /> is automatic (not specified); otherwise, false. The default value is true.</returns>
	public bool IsAuto => _unitType == FigureUnitType.Auto;

	/// <summary>Gets a value that determines whether this <see cref="T:System.Windows.FigureLength" /> has a <see cref="T:System.Windows.FigureUnitType" /> property value of <see cref="F:System.Windows.FigureUnitType.Column" />.</summary>
	/// <returns>true if this <see cref="T:System.Windows.FigureLength" /> has a <see cref="T:System.Windows.FigureUnitType" /> property value of <see cref="F:System.Windows.FigureUnitType.Column" />; otherwise, false. The default value is false.</returns>
	public bool IsColumn => _unitType == FigureUnitType.Column;

	/// <summary>Gets a value that determines whether this <see cref="T:System.Windows.FigureLength" /> has a <see cref="T:System.Windows.FigureUnitType" /> property value of <see cref="F:System.Windows.FigureUnitType.Content" />.</summary>
	/// <returns>Returns true if this <see cref="T:System.Windows.FigureLength" /> has a <see cref="T:System.Windows.FigureUnitType" /> property value of <see cref="F:System.Windows.FigureUnitType.Content" />; otherwise, false. The default value is false.</returns>
	public bool IsContent => _unitType == FigureUnitType.Content;

	/// <summary>Gets a value that determines whether this <see cref="T:System.Windows.FigureLength" /> has a <see cref="T:System.Windows.FigureUnitType" /> property value of <see cref="F:System.Windows.FigureUnitType.Page" />.</summary>
	/// <returns>true if this <see cref="T:System.Windows.FigureLength" /> has a <see cref="T:System.Windows.FigureUnitType" /> property value of <see cref="F:System.Windows.FigureUnitType.Page" />; otherwise, false. The default value is false.</returns>
	public bool IsPage => _unitType == FigureUnitType.Page;

	/// <summary>Gets the value of this <see cref="T:System.Windows.FigureLength" />. </summary>
	/// <returns>The value of this <see cref="T:System.Windows.FigureLength" />. The default value is 1.</returns>
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

	/// <summary>Gets the unit type of the <see cref="P:System.Windows.FigureLength.Value" />.</summary>
	/// <returns>The unit type of this <see cref="P:System.Windows.FigureLength.Value" />. The default value is <see cref="F:System.Windows.FigureUnitType.Auto" />.</returns>
	public FigureUnitType FigureUnitType => _unitType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FigureLength" /> class with the specified number of pixels in length.</summary>
	/// <param name="pixels">The number of device-independent pixels (96 pixels-per-inch) that make up the length.</param>
	public FigureLength(double pixels)
		: this(pixels, FigureUnitType.Pixel)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FigureLength" /> class with the specified <see cref="P:System.Windows.FigureLength.Value" /> and <see cref="P:System.Windows.FigureLength.FigureUnitType" />.</summary>
	/// <param name="value">The <see cref="P:System.Windows.FigureLength.Value" /> of the <see cref="T:System.Windows.FigureLength" /> class.</param>
	/// <param name="type">The <see cref="P:System.Windows.FigureLength.Value" /> of the <see cref="P:System.Windows.FigureLength.FigureUnitType" /> class.</param>
	public FigureLength(double value, FigureUnitType type)
	{
		double num = 1000.0;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(value))
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterNoNaN, "value"));
		}
		if (double.IsInfinity(value))
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterNoInfinity, "value"));
		}
		if (value < 0.0)
		{
			throw new ArgumentOutOfRangeException(SR.Format(SR.InvalidCtorParameterNoNegative, "value"));
		}
		if (type != 0 && type != FigureUnitType.Pixel && type != FigureUnitType.Column && type != FigureUnitType.Content && type != FigureUnitType.Page)
		{
			throw new ArgumentException(SR.Format(SR.InvalidCtorParameterUnknownFigureUnitType, "type"));
		}
		if (value > 1.0 && (type == FigureUnitType.Content || type == FigureUnitType.Page))
		{
			throw new ArgumentOutOfRangeException("value");
		}
		if (value > num && type == FigureUnitType.Column)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		if (value > num2 && type == FigureUnitType.Pixel)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		_unitValue = ((type == FigureUnitType.Auto) ? 0.0 : value);
		_unitType = type;
	}

	/// <summary>Compares two <see cref="T:System.Windows.FigureLength" /> structures for equality.</summary>
	/// <returns>true if <paramref name="fl1" /> and <paramref name="fl2" /> are equal; otherwise, false.</returns>
	/// <param name="fl1">The first <see cref="T:System.Windows.FigureLength" /> structure to compare.</param>
	/// <param name="fl2">The second <see cref="T:System.Windows.FigureLength" /> structure to compare.</param>
	public static bool operator ==(FigureLength fl1, FigureLength fl2)
	{
		if (fl1.FigureUnitType == fl2.FigureUnitType)
		{
			return fl1.Value == fl2.Value;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.FigureLength" /> structures for inequality.</summary>
	/// <returns>true if <paramref name="fl1" /> and <paramref name="fl2" /> are not equal; otherwise, false.</returns>
	/// <param name="fl1">The first <see cref="T:System.Windows.FigureLength" /> structure to compare.</param>
	/// <param name="fl2">The second <see cref="T:System.Windows.FigureLength" /> structure to compare.</param>
	public static bool operator !=(FigureLength fl1, FigureLength fl2)
	{
		if (fl1.FigureUnitType == fl2.FigureUnitType)
		{
			return fl1.Value != fl2.Value;
		}
		return true;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is a <see cref="T:System.Windows.FigureLength" /> and whether it is identical to this <see cref="T:System.Windows.FigureLength" />.</summary>
	/// <returns>true if <paramref name="oCompare" /> is a <see cref="T:System.Windows.FigureLength" /> and is identical to this <see cref="T:System.Windows.FigureLength" />; otherwise, false.</returns>
	/// <param name="oCompare">The <see cref="T:System.Object" /> to compare to this instance.</param>
	public override bool Equals(object oCompare)
	{
		if (oCompare is FigureLength figureLength)
		{
			return this == figureLength;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Windows.FigureLength" /> structures for equality.</summary>
	/// <returns>true if <paramref name="figureLength" /> is identical to this <see cref="T:System.Windows.FigureLength" />; otherwise, false.</returns>
	/// <param name="figureLength">The <see cref="T:System.Windows.FigureLength" /> to compare to this instance.</param>
	public bool Equals(FigureLength figureLength)
	{
		return this == figureLength;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.FigureLength" />.</summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.FigureLength" /> structure.</returns>
	public override int GetHashCode()
	{
		return (int)((int)_unitValue + _unitType);
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.FigureLength" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.FigureLength" />.</returns>
	public override string ToString()
	{
		return FigureLengthConverter.ToString(this, CultureInfo.InvariantCulture);
	}
}
