using System.ComponentModel;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>This class is used by a spline key frame to define animation progress. </summary>
[TypeConverter(typeof(KeySplineConverter))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class KeySpline : Freezable, IFormattable
{
	private Point _controlPoint1;

	private Point _controlPoint2;

	private bool _isSpecified;

	private bool _isDirty;

	private double _parameter;

	private double _Bx;

	private double _Cx;

	private double _Cx_Bx;

	private double _three_Cx;

	private double _By;

	private double _Cy;

	private const double accuracy = 0.001;

	private const double fuzz = 1E-06;

	/// <summary>The first control point used to define a Bezier curve that describes a <see cref="T:System.Windows.Media.Animation.KeySpline" />.</summary>
	/// <returns>The Bezier curve's first control point. The point's <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values must each be between 0 and 1, inclusive. The default value is (0,0). </returns>
	public Point ControlPoint1
	{
		get
		{
			ReadPreamble();
			return _controlPoint1;
		}
		set
		{
			WritePreamble();
			if (value != _controlPoint1)
			{
				if (!IsValidControlPoint(value))
				{
					throw new ArgumentException(SR.Format(SR.Animation_KeySpline_InvalidValue, "ControlPoint1", value));
				}
				_controlPoint1 = value;
				WritePostscript();
			}
		}
	}

	/// <summary>The second control point used to define a Bezier curve that describes a <see cref="T:System.Windows.Media.Animation.KeySpline" />.</summary>
	/// <returns>The Bezier curve's second control point. The point's <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values must each be between 0 and 1, inclusive. The default value is (1,1). </returns>
	public Point ControlPoint2
	{
		get
		{
			ReadPreamble();
			return _controlPoint2;
		}
		set
		{
			WritePreamble();
			if (value != _controlPoint2)
			{
				if (!IsValidControlPoint(value))
				{
					throw new ArgumentException(SR.Format(SR.Animation_KeySpline_InvalidValue, "ControlPoint2", value));
				}
				_controlPoint2 = value;
				WritePostscript();
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.KeySpline" /> class. </summary>
	public KeySpline()
	{
		_controlPoint1 = new Point(0.0, 0.0);
		_controlPoint2 = new Point(1.0, 1.0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.KeySpline" /> class with the specified coordinates for the control points. </summary>
	/// <param name="x1">The x-coordinate for the <see cref="P:System.Windows.Media.Animation.KeySpline.ControlPoint1" /> of the <see cref="T:System.Windows.Media.Animation.KeySpline" />.</param>
	/// <param name="y1">The y-coordinate for the <see cref="P:System.Windows.Media.Animation.KeySpline.ControlPoint1" /> of the <see cref="T:System.Windows.Media.Animation.KeySpline" />.</param>
	/// <param name="x2">The x-coordinate for the <see cref="P:System.Windows.Media.Animation.KeySpline.ControlPoint2" /> of the <see cref="T:System.Windows.Media.Animation.KeySpline" />.</param>
	/// <param name="y2">The y-coordinate for the <see cref="P:System.Windows.Media.Animation.KeySpline.ControlPoint2" /> of the <see cref="T:System.Windows.Media.Animation.KeySpline" />.</param>
	public KeySpline(double x1, double y1, double x2, double y2)
		: this(new Point(x1, y1), new Point(x2, y2))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.KeySpline" /> class with the specified control points. </summary>
	/// <param name="controlPoint1">The control point for the <see cref="P:System.Windows.Media.Animation.KeySpline.ControlPoint1" /> of the <see cref="T:System.Windows.Media.Animation.KeySpline" />.</param>
	/// <param name="controlPoint2">The control point for the <see cref="P:System.Windows.Media.Animation.KeySpline.ControlPoint2" /> of the <see cref="T:System.Windows.Media.Animation.KeySpline" />.</param>
	public KeySpline(Point controlPoint1, Point controlPoint2)
	{
		if (!IsValidControlPoint(controlPoint1))
		{
			throw new ArgumentException(SR.Format(SR.Animation_KeySpline_InvalidValue, "controlPoint1", controlPoint1));
		}
		if (!IsValidControlPoint(controlPoint2))
		{
			throw new ArgumentException(SR.Format(SR.Animation_KeySpline_InvalidValue, "controlPoint2", controlPoint2));
		}
		_controlPoint1 = controlPoint1;
		_controlPoint2 = controlPoint2;
		_isDirty = true;
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.KeySpline" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Animation.KeySpline" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new KeySpline();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.Animation.KeySpline" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.KeySpline" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		KeySpline sourceKeySpline = (KeySpline)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CloneCommon(sourceKeySpline);
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.Animation.KeySpline" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.KeySpline" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		KeySpline sourceKeySpline = (KeySpline)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CloneCommon(sourceKeySpline);
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Animation.KeySpline" /> object.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.KeySpline" /> object to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		KeySpline sourceKeySpline = (KeySpline)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CloneCommon(sourceKeySpline);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Animation.KeySpline" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.KeySpline" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		KeySpline sourceKeySpline = (KeySpline)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CloneCommon(sourceKeySpline);
	}

	/// <summary>Called when the current <see cref="T:System.Windows.Media.Animation.KeySpline" /> object is modified.</summary>
	protected override void OnChanged()
	{
		_isDirty = true;
		base.OnChanged();
	}

	/// <summary>Calculates spline progress from a supplied linear progress. </summary>
	/// <returns>The calculated spline progress.</returns>
	/// <param name="linearProgress">The linear progress to evaluate.</param>
	public double GetSplineProgress(double linearProgress)
	{
		ReadPreamble();
		if (_isDirty)
		{
			Build();
		}
		if (!_isSpecified)
		{
			return linearProgress;
		}
		SetParameterFromX(linearProgress);
		return GetBezierValue(_By, _Cy, _parameter);
	}

	private bool IsValidControlPoint(Point point)
	{
		if (point.X >= 0.0)
		{
			return point.X <= 1.0;
		}
		return false;
	}

	private void Build()
	{
		if (_controlPoint1 == new Point(0.0, 0.0) && _controlPoint2 == new Point(1.0, 1.0))
		{
			_isSpecified = false;
		}
		else
		{
			_isSpecified = true;
			_parameter = 0.0;
			_Bx = 3.0 * _controlPoint1.X;
			_Cx = 3.0 * _controlPoint2.X;
			_Cx_Bx = 2.0 * (_Cx - _Bx);
			_three_Cx = 3.0 - _Cx;
			_By = 3.0 * _controlPoint1.Y;
			_Cy = 3.0 * _controlPoint2.Y;
		}
		_isDirty = false;
	}

	private static double GetBezierValue(double b, double c, double t)
	{
		double num = 1.0 - t;
		double num2 = t * t;
		return b * t * num * num + c * num2 * num + num2 * t;
	}

	private void GetXAndDx(double t, out double x, out double dx)
	{
		double num = 1.0 - t;
		double num2 = t * t;
		double num3 = num * num;
		x = _Bx * t * num3 + _Cx * num2 * num + num2 * t;
		dx = _Bx * num3 + _Cx_Bx * num * t + _three_Cx * num2;
	}

	private void SetParameterFromX(double time)
	{
		double num = 0.0;
		double num2 = 1.0;
		if (time == 0.0)
		{
			_parameter = 0.0;
			return;
		}
		if (time == 1.0)
		{
			_parameter = 1.0;
			return;
		}
		while (num2 - num > 1E-06)
		{
			GetXAndDx(_parameter, out var x, out var dx);
			double num3 = Math.Abs(dx);
			if (x > time)
			{
				num2 = _parameter;
			}
			else
			{
				num = _parameter;
			}
			if (Math.Abs(x - time) < 0.001 * num3)
			{
				break;
			}
			if (num3 > 1E-06)
			{
				double num4 = _parameter - (x - time) / dx;
				if (num4 >= num2)
				{
					_parameter = (_parameter + num2) / 2.0;
				}
				else if (num4 <= num)
				{
					_parameter = (_parameter + num) / 2.0;
				}
				else
				{
					_parameter = num4;
				}
			}
			else
			{
				_parameter = (num + num2) / 2.0;
			}
		}
	}

	private void CloneCommon(KeySpline sourceKeySpline)
	{
		_controlPoint1 = sourceKeySpline._controlPoint1;
		_controlPoint2 = sourceKeySpline._controlPoint2;
		_isDirty = true;
	}

	/// <summary>Creates a string representation of this instance of <see cref="T:System.Windows.Media.Animation.KeySpline" /> based on the current culture. </summary>
	/// <returns>A string representation of this <see cref="T:System.Windows.Media.Animation.KeySpline" />.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return InternalConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this <see cref="T:System.Windows.Media.Animation.KeySpline" /> based on the supplied <see cref="T:System.IFormatProvider" />. </summary>
	/// <returns>A string representation of this instance of <see cref="T:System.Windows.Media.Animation.KeySpline" />.</returns>
	/// <param name="formatProvider">The format provider to use. If provider is null, the current culture is used.</param>
	public string ToString(IFormatProvider formatProvider)
	{
		ReadPreamble();
		return InternalConvertToString(null, formatProvider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		ReadPreamble();
		return InternalConvertToString(format, formatProvider);
	}

	internal string InternalConvertToString(string format, IFormatProvider formatProvider)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(formatProvider);
		return string.Format(formatProvider, "{1}{0}{2}", numericListSeparator, _controlPoint1, _controlPoint2);
	}
}
