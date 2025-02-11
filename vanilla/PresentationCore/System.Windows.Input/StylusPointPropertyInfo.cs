using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Specifies the constraints of a property in a <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
public class StylusPointPropertyInfo : StylusPointProperty
{
	private int _min;

	private int _max;

	private float _resolution;

	private StylusPointPropertyUnit _unit;

	/// <summary>Gets the minimum value accepted for the <see cref="T:System.Windows.Input.StylusPoint" /> property.</summary>
	/// <returns>The minimum value accepted for the <see cref="T:System.Windows.Input.StylusPoint" /> property.</returns>
	public int Minimum => _min;

	/// <summary>Gets the maximum value accepted for the <see cref="T:System.Windows.Input.StylusPoint" /> property.</summary>
	/// <returns>The maximum value accepted for the <see cref="T:System.Windows.Input.StylusPoint" /> property.</returns>
	public int Maximum => _max;

	/// <summary>Gets the scale that converts a <see cref="T:System.Windows.Input.StylusPoint" /> property value into units.</summary>
	/// <returns>The scale that converts a <see cref="T:System.Windows.Input.StylusPoint" /> property value into units.</returns>
	public float Resolution
	{
		get
		{
			return _resolution;
		}
		internal set
		{
			_resolution = value;
		}
	}

	/// <summary>Gets the type of measurement that is used by <see cref="T:System.Windows.Input.StylusPoint" /> property. </summary>
	/// <returns>One of the <see cref="T:System.Windows.Input.StylusPointPropertyUnit" /> values.</returns>
	public StylusPointPropertyUnit Unit => _unit;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> class. </summary>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> to base the new <see cref="T:System.Windows.Input.StylusPointProperty" /> on.</param>
	public StylusPointPropertyInfo(StylusPointProperty stylusPointProperty)
		: base(stylusPointProperty)
	{
		StylusPointPropertyInfo stylusPointPropertyInfoDefault = StylusPointPropertyInfoDefaults.GetStylusPointPropertyInfoDefault(stylusPointProperty);
		_min = stylusPointPropertyInfoDefault.Minimum;
		_max = stylusPointPropertyInfoDefault.Maximum;
		_resolution = stylusPointPropertyInfoDefault.Resolution;
		_unit = stylusPointPropertyInfoDefault.Unit;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> class using the specified values.</summary>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> to base the new <see cref="T:System.Windows.Input.StylusPointProperty" /> on.</param>
	/// <param name="minimum">The minimum value accepted for the <see cref="T:System.Windows.Input.StylusPoint" /> property.</param>
	/// <param name="maximum">The maximum value accepted for the <see cref="T:System.Windows.Input.StylusPoint" /> property.</param>
	/// <param name="resolution">The scale that converts a <see cref="T:System.Windows.Input.StylusPoint" /> property value into its units.</param>
	public StylusPointPropertyInfo(StylusPointProperty stylusPointProperty, int minimum, int maximum, StylusPointPropertyUnit unit, float resolution)
		: base(stylusPointProperty)
	{
		if (!StylusPointPropertyUnitHelper.IsDefined(unit))
		{
			throw new InvalidEnumArgumentException("unit", (int)unit, typeof(StylusPointPropertyUnit));
		}
		if (maximum < minimum)
		{
			throw new ArgumentException(SR.Stylus_InvalidMax, "maximum");
		}
		if (resolution < 0f)
		{
			throw new ArgumentException(SR.InvalidStylusPointPropertyInfoResolution, "resolution");
		}
		_min = minimum;
		_max = maximum;
		_resolution = resolution;
		_unit = unit;
	}

	internal static bool AreCompatible(StylusPointPropertyInfo stylusPointPropertyInfo1, StylusPointPropertyInfo stylusPointPropertyInfo2)
	{
		if (stylusPointPropertyInfo1 == null || stylusPointPropertyInfo2 == null)
		{
			throw new ArgumentNullException("stylusPointPropertyInfo");
		}
		if (stylusPointPropertyInfo1.Id == stylusPointPropertyInfo2.Id)
		{
			return stylusPointPropertyInfo1.IsButton == stylusPointPropertyInfo2.IsButton;
		}
		return false;
	}
}
