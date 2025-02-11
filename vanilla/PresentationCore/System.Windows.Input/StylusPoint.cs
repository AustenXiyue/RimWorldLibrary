using System.Collections.ObjectModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Represents a single data point collected from the digitizer and stylus.</summary>
public struct StylusPoint : IEquatable<StylusPoint>
{
	internal const float DefaultPressure = 0.5f;

	private double _x;

	private double _y;

	private float _pressureFactor;

	private int[] _additionalValues;

	private StylusPointDescription _stylusPointDescription;

	/// <summary>Specifies the largest valid value for a pair of (x, y) coordinates.</summary>
	public static readonly double MaxXY = 81164736.2834643;

	/// <summary>Specifies the smallest valid value for a pair of (x, y) coordinates.</summary>
	public static readonly double MinXY = -81164736.3212596;

	/// <summary>Gets or sets the value for the x-coordinate of the <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
	/// <returns>The x-coordinate of the <see cref="T:System.Windows.Input.StylusPoint" />.</returns>
	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			if (double.IsNaN(value))
			{
				throw new ArgumentOutOfRangeException("X", SR.InvalidStylusPointXYNaN);
			}
			_x = GetClampedXYValue(value);
		}
	}

	/// <summary>Gets or sets the y-coordinate of the <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
	/// <returns>The y-coordinate of the <see cref="T:System.Windows.Input.StylusPoint" />.</returns>
	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			if (double.IsNaN(value))
			{
				throw new ArgumentOutOfRangeException("Y", SR.InvalidStylusPointXYNaN);
			}
			_y = GetClampedXYValue(value);
		}
	}

	/// <summary>Gets or sets a value between 0 and 1 that reflects the amount of pressure the stylus applies to the digitizer's surface when the <see cref="T:System.Windows.Input.StylusPoint" /> is created.</summary>
	/// <returns>Value between 0 and 1 indicating the amount of pressure the stylus applies to the digitizer's surface as the <see cref="T:System.Windows.Input.StylusPoint" /> is created.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <see cref="P:System.Windows.Input.StylusPoint.PressureFactor" /> property is set to a value less than 0 or greater than 1.</exception>
	public float PressureFactor
	{
		get
		{
			if (_pressureFactor > 1f)
			{
				return 1f;
			}
			if (_pressureFactor < 0f)
			{
				return 0f;
			}
			return _pressureFactor;
		}
		set
		{
			if (value < 0f || value > 1f)
			{
				throw new ArgumentOutOfRangeException("PressureFactor", SR.InvalidPressureValue);
			}
			_pressureFactor = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.StylusPointDescription" /> that specifies the properties stored in the <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusPointDescription" /> specifies the properties stored in the <see cref="T:System.Windows.Input.StylusPoint" />.</returns>
	public StylusPointDescription Description
	{
		get
		{
			if (_stylusPointDescription == null)
			{
				_stylusPointDescription = new StylusPointDescription();
			}
			return _stylusPointDescription;
		}
		internal set
		{
			_stylusPointDescription = value;
		}
	}

	internal bool HasDefaultPressure => _pressureFactor == 0.5f;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPoint" /> class using specified (x, y) coordinates.</summary>
	public StylusPoint(double x, double y)
		: this(x, y, 0.5f, null, null, validateAdditionalData: false, validatePressureFactor: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPoint" /> class using specified (x, y) coordinates and pressure.</summary>
	/// <param name="x">The x-coordinate of the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="y">The y-coordinate of the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="pressureFactor">The amount of pressure applied to the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pressureFactor" /> is less than 0 or greater than 1.</exception>
	public StylusPoint(double x, double y, float pressureFactor)
		: this(x, y, pressureFactor, null, null, validateAdditionalData: false, validatePressureFactor: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPoint" /> class using specified (x, y) coordinates, a <paramref name="pressureFactor" />, and additional parameters specified in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <param name="pressureFactor">The amount of pressure applied to the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="stylusPointDescription">A <see cref="T:System.Windows.Input.StylusPointDescription" /> that specifies the additional properties stored in the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	/// <param name="additionalValues">An array of 32-bit signed integers that contains the values of the properties defined in <paramref name="stylusPointDescription" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pressureFactor" /> is less than 0 or greater than 1.-or-The values in <paramref name="additionalValues" /> that correspond to button properties are not 0 or 1.</exception>
	/// <exception cref="T:System.ArgumentException">The number of values in <paramref name="additionalValues" /> does not match the number of properties in <paramref name="stylusPointDescription" /> minus 3.</exception>
	public StylusPoint(double x, double y, float pressureFactor, StylusPointDescription stylusPointDescription, int[] additionalValues)
		: this(x, y, pressureFactor, stylusPointDescription, additionalValues, validateAdditionalData: true, validatePressureFactor: true)
	{
	}

	internal StylusPoint(double x, double y, float pressureFactor, StylusPointDescription stylusPointDescription, int[] additionalValues, bool validateAdditionalData, bool validatePressureFactor)
	{
		if (double.IsNaN(x))
		{
			throw new ArgumentOutOfRangeException("x", SR.InvalidStylusPointXYNaN);
		}
		if (double.IsNaN(y))
		{
			throw new ArgumentOutOfRangeException("y", SR.InvalidStylusPointXYNaN);
		}
		if (validatePressureFactor && (pressureFactor == float.NaN || pressureFactor < 0f || pressureFactor > 1f))
		{
			throw new ArgumentOutOfRangeException("pressureFactor", SR.InvalidPressureValue);
		}
		_x = GetClampedXYValue(x);
		_y = GetClampedXYValue(y);
		_stylusPointDescription = stylusPointDescription;
		_additionalValues = additionalValues;
		_pressureFactor = pressureFactor;
		if (!validateAdditionalData)
		{
			return;
		}
		if (stylusPointDescription == null)
		{
			throw new ArgumentNullException("stylusPointDescription");
		}
		if (stylusPointDescription.PropertyCount > 3 && additionalValues == null)
		{
			throw new ArgumentNullException("additionalValues");
		}
		if (additionalValues != null)
		{
			ReadOnlyCollection<StylusPointPropertyInfo> stylusPointProperties = stylusPointDescription.GetStylusPointProperties();
			int num = stylusPointProperties.Count - 3;
			if (additionalValues.Length != num)
			{
				throw new ArgumentException(SR.InvalidAdditionalDataForStylusPoint, "additionalValues");
			}
			int[] additionalValues2 = new int[stylusPointDescription.GetExpectedAdditionalDataCount()];
			_additionalValues = additionalValues2;
			int num2 = 3;
			int num3 = 0;
			while (num2 < stylusPointProperties.Count)
			{
				SetPropertyValue(stylusPointProperties[num2], additionalValues[num3], copyBeforeWrite: false);
				num2++;
				num3++;
			}
		}
	}

	/// <summary>Returns whether the current <see cref="T:System.Windows.Input.StylusPoint" /> contains the specified property.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Input.StylusPointProperty" /> is in the current <see cref="T:System.Windows.Input.StylusPoint" />; otherwise, false.</returns>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> to check for in the <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	public bool HasProperty(StylusPointProperty stylusPointProperty)
	{
		return Description.HasProperty(stylusPointProperty);
	}

	/// <summary>Returns the value of the specified property.</summary>
	/// <returns>The value of the specified <see cref="T:System.Windows.Input.StylusPointProperty" />.</returns>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> that specifies which property value to get.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stylusPointProperty" /> is not one of the properties in <see cref="P:System.Windows.Input.StylusPoint.Description" />.</exception>
	public int GetPropertyValue(StylusPointProperty stylusPointProperty)
	{
		if (stylusPointProperty == null)
		{
			throw new ArgumentNullException("stylusPointProperty");
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.X)
		{
			return (int)_x;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.Y)
		{
			return (int)_y;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.NormalPressure)
		{
			int maximum = Description.GetPropertyInfo(StylusPointProperties.NormalPressure).Maximum;
			return (int)(_pressureFactor * (float)maximum);
		}
		int propertyIndex = Description.GetPropertyIndex(stylusPointProperty.Id);
		if (-1 == propertyIndex)
		{
			throw new ArgumentException(SR.InvalidStylusPointProperty, "stylusPointProperty");
		}
		if (stylusPointProperty.IsButton)
		{
			int num = _additionalValues[_additionalValues.Length - 1];
			int buttonBitPosition = Description.GetButtonBitPosition(stylusPointProperty);
			int num2 = 1 << buttonBitPosition;
			if ((num & num2) != 0)
			{
				return 1;
			}
			return 0;
		}
		return _additionalValues[propertyIndex - 3];
	}

	/// <summary>Sets the specified property to the specified value.</summary>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> that specifies which property value to set.</param>
	/// <param name="value">The value of the property.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stylusPointProperty" /> is not one of the properties in <see cref="P:System.Windows.Input.StylusPoint.Description" />.</exception>
	public void SetPropertyValue(StylusPointProperty stylusPointProperty, int value)
	{
		SetPropertyValue(stylusPointProperty, value, copyBeforeWrite: true);
	}

	internal void SetPropertyValue(StylusPointProperty stylusPointProperty, int value, bool copyBeforeWrite)
	{
		if (stylusPointProperty == null)
		{
			throw new ArgumentNullException("stylusPointProperty");
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.X)
		{
			double xyValue = value;
			_x = GetClampedXYValue(xyValue);
			return;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.Y)
		{
			double xyValue2 = value;
			_y = GetClampedXYValue(xyValue2);
			return;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.NormalPressure)
		{
			StylusPointPropertyInfo propertyInfo = Description.GetPropertyInfo(StylusPointProperties.NormalPressure);
			int minimum = propertyInfo.Minimum;
			int maximum = propertyInfo.Maximum;
			if (maximum == 0)
			{
				_pressureFactor = 0f;
			}
			else
			{
				_pressureFactor = Convert.ToSingle(minimum + value) / Convert.ToSingle(maximum);
			}
			return;
		}
		int propertyIndex = Description.GetPropertyIndex(stylusPointProperty.Id);
		if (-1 == propertyIndex)
		{
			throw new ArgumentException(SR.InvalidStylusPointProperty, "propertyId");
		}
		if (stylusPointProperty.IsButton)
		{
			if (value < 0 || value > 1)
			{
				throw new ArgumentOutOfRangeException("value", SR.InvalidMinMaxForButton);
			}
			if (copyBeforeWrite)
			{
				CopyAdditionalData();
			}
			int num = _additionalValues[_additionalValues.Length - 1];
			int buttonBitPosition = Description.GetButtonBitPosition(stylusPointProperty);
			int num2 = 1 << buttonBitPosition;
			num = ((value != 0) ? (num | num2) : (num & ~num2));
			_additionalValues[_additionalValues.Length - 1] = num;
		}
		else
		{
			if (copyBeforeWrite)
			{
				CopyAdditionalData();
			}
			_additionalValues[propertyIndex - 3] = value;
		}
	}

	/// <summary>Casts the specified <see cref="T:System.Windows.Input.StylusPoint" /> to a <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> that contains the same (x, y) coordinates as <paramref name="stylusPoint" />.</returns>
	/// <param name="stylusPoint">The <see cref="T:System.Windows.Input.StylusPoint" /> to cast to a <see cref="T:System.Windows.Point" />.</param>
	public static explicit operator Point(StylusPoint stylusPoint)
	{
		return new Point(stylusPoint.X, stylusPoint.Y);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Input.StylusPoint" /> to a <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> structure.</returns>
	public Point ToPoint()
	{
		return new Point(X, Y);
	}

	/// <summary>Compares two specified <see cref="T:System.Windows.Input.StylusPoint" /> objects and determines whether they are equal.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPoint" /> objects are equal; otherwise, false.</returns>
	/// <param name="stylusPoint1">The first <see cref="T:System.Windows.Input.StylusPoint" /> to compare.</param>
	/// <param name="stylusPoint2">The second <see cref="T:System.Windows.Input.StylusPoint" /> to compare.</param>
	public static bool operator ==(StylusPoint stylusPoint1, StylusPoint stylusPoint2)
	{
		return Equals(stylusPoint1, stylusPoint2);
	}

	/// <summary>Returns a Boolean value which indicates whether the specified <see cref="T:System.Windows.Input.StylusPoint" /> objects are unequal.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.Input.StylusPoint" /> objects are unequal; otherwise, false.</returns>
	/// <param name="stylusPoint1">The first <see cref="T:System.Windows.Input.StylusPoint" /> to compare.</param>
	/// <param name="stylusPoint2">The second <see cref="T:System.Windows.Input.StylusPoint" /> to compare.</param>
	public static bool operator !=(StylusPoint stylusPoint1, StylusPoint stylusPoint2)
	{
		return !Equals(stylusPoint1, stylusPoint2);
	}

	/// <summary>Returns a Boolean value that indicates whether the two specified <see cref="T:System.Windows.Input.StylusPoint" /> objects are equal.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPoint" /> objects are equal; otherwise, false.</returns>
	/// <param name="stylusPoint1">The first <see cref="T:System.Windows.Input.StylusPoint" /> to compare.</param>
	/// <param name="stylusPoint2">The second <see cref="T:System.Windows.Input.StylusPoint" /> to compare.</param>
	public static bool Equals(StylusPoint stylusPoint1, StylusPoint stylusPoint2)
	{
		if (stylusPoint1._x != stylusPoint2._x || stylusPoint1._y != stylusPoint2._y || stylusPoint1._pressureFactor != stylusPoint2._pressureFactor)
		{
			return false;
		}
		if (stylusPoint1._additionalValues == null && stylusPoint2._additionalValues == null)
		{
			return true;
		}
		if (stylusPoint1.Description == stylusPoint2.Description || StylusPointDescription.AreCompatible(stylusPoint1.Description, stylusPoint2.Description))
		{
			for (int i = 0; i < stylusPoint1._additionalValues.Length; i++)
			{
				if (stylusPoint1._additionalValues[i] != stylusPoint2._additionalValues[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	/// <summary>Returns a value indicating whether the specified object is equal to the <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
	/// <returns>true if the objects are equal; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Windows.Input.StylusPoint" /> to compare to the current <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	public override bool Equals(object o)
	{
		if (o == null || !(o is StylusPoint stylusPoint))
		{
			return false;
		}
		return Equals(this, stylusPoint);
	}

	/// <summary>Returns a Boolean value that indicates whether the specified <see cref="T:System.Windows.Input.StylusPoint" /> is equal to the current <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPoint" /> objects are equal; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Input.StylusPoint" /> to compare to the current <see cref="T:System.Windows.Input.StylusPoint" />.</param>
	public bool Equals(StylusPoint value)
	{
		return Equals(this, value);
	}

	public override int GetHashCode()
	{
		int num = _x.GetHashCode() ^ _y.GetHashCode() ^ _pressureFactor.GetHashCode();
		if (_stylusPointDescription != null)
		{
			num ^= _stylusPointDescription.GetHashCode();
		}
		if (_additionalValues != null)
		{
			for (int i = 0; i < _additionalValues.Length; i++)
			{
				num ^= _additionalValues[i];
			}
		}
		return num;
	}

	internal int[] GetAdditionalData()
	{
		return _additionalValues;
	}

	internal float GetUntruncatedPressureFactor()
	{
		return _pressureFactor;
	}

	internal int[] GetPacketData()
	{
		int num = 2;
		if (_additionalValues != null)
		{
			num += _additionalValues.Length;
		}
		if (Description.ContainsTruePressure)
		{
			num++;
		}
		int[] array = new int[num];
		array[0] = (int)_x;
		array[1] = (int)_y;
		int num2 = 2;
		if (Description.ContainsTruePressure)
		{
			num2 = 3;
			array[2] = GetPropertyValue(StylusPointProperties.NormalPressure);
		}
		if (_additionalValues != null)
		{
			for (int i = 0; i < _additionalValues.Length; i++)
			{
				array[i + num2] = _additionalValues[i];
			}
		}
		return array;
	}

	private void CopyAdditionalData()
	{
		_additionalValues = (int[])_additionalValues?.Clone();
	}

	private static double GetClampedXYValue(double xyValue)
	{
		if (xyValue > MaxXY)
		{
			return MaxXY;
		}
		if (xyValue < MinXY)
		{
			return MinXY;
		}
		return xyValue;
	}
}
