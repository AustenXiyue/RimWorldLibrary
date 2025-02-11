using System.Windows.Input.Manipulations;

namespace System.Windows.Input;

/// <summary>Controls the deceleration of a rotation manipulation during inertia.</summary>
public class InertiaRotationBehavior
{
	private bool _isInitialVelocitySet;

	private double _initialVelocity = double.NaN;

	private bool _isDesiredDecelerationSet;

	private double _desiredDeceleration = double.NaN;

	private bool _isDesiredRotationSet;

	private double _desiredRotation = double.NaN;

	/// <summary>Gets or sets the initial rate of the rotation at the start of the inertia phase.</summary>
	/// <returns>The initial rate of the rotation at the start of the inertia phase.</returns>
	public double InitialVelocity
	{
		get
		{
			return _initialVelocity;
		}
		set
		{
			_isInitialVelocitySet = true;
			_initialVelocity = value;
		}
	}

	/// <summary>Gets or sets the rate the rotation slows in degrees per squared millisecond.</summary>
	/// <returns>The rate the rotation slows in degrees per squared millisecond. The default is <see cref="F:System.Double.NaN" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is set to infinity.-or-The property is set to <see cref="F:System.Double.NaN" />.</exception>
	public double DesiredDeceleration
	{
		get
		{
			return _desiredDeceleration;
		}
		set
		{
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_isDesiredDecelerationSet = true;
			_desiredDeceleration = value;
			_isDesiredRotationSet = false;
			_desiredRotation = double.NaN;
		}
	}

	/// <summary>Gets or sets the rotation, in degrees, at the end of the inertial movement.</summary>
	/// <returns>The rotation, in degrees, at the end of the inertial movement. The default is <see cref="F:System.Double.NaN" />.</returns>
	public double DesiredRotation
	{
		get
		{
			return _desiredRotation;
		}
		set
		{
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_isDesiredRotationSet = true;
			_desiredRotation = value;
			_isDesiredDecelerationSet = false;
			_desiredDeceleration = double.NaN;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InertiaRotationBehavior" /> class. </summary>
	public InertiaRotationBehavior()
	{
	}

	internal InertiaRotationBehavior(double initialVelocity)
	{
		_initialVelocity = initialVelocity;
	}

	internal bool CanUseForInertia()
	{
		if (!_isInitialVelocitySet && !_isDesiredDecelerationSet)
		{
			return _isDesiredRotationSet;
		}
		return true;
	}

	internal static void ApplyParameters(InertiaRotationBehavior behavior, InertiaProcessor2D processor, double initialVelocity)
	{
		if (behavior != null && behavior.CanUseForInertia())
		{
			InertiaRotationBehavior2D inertiaRotationBehavior2D = new InertiaRotationBehavior2D();
			if (behavior._isInitialVelocitySet)
			{
				inertiaRotationBehavior2D.InitialVelocity = (float)AngleUtil.DegreesToRadians(behavior._initialVelocity);
			}
			else
			{
				inertiaRotationBehavior2D.InitialVelocity = (float)AngleUtil.DegreesToRadians(initialVelocity);
			}
			if (behavior._isDesiredDecelerationSet)
			{
				inertiaRotationBehavior2D.DesiredDeceleration = (float)AngleUtil.DegreesToRadians(behavior._desiredDeceleration);
			}
			if (behavior._isDesiredRotationSet)
			{
				inertiaRotationBehavior2D.DesiredRotation = (float)AngleUtil.DegreesToRadians(behavior._desiredRotation);
			}
			processor.RotationBehavior = inertiaRotationBehavior2D;
		}
	}
}
