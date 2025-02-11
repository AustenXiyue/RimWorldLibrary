using System.Windows.Input.Manipulations;

namespace System.Windows.Input;

/// <summary>Controls deceleration on a translation manipulation during inertia.</summary>
public class InertiaTranslationBehavior
{
	private bool _isInitialVelocitySet;

	private Vector _initialVelocity = new Vector(double.NaN, double.NaN);

	private bool _isDesiredDecelerationSet;

	private double _desiredDeceleration = double.NaN;

	private bool _isDesiredDisplacementSet;

	private double _desiredDisplacement = double.NaN;

	/// <summary>Gets or sets the initial rate of linear movement at the start of the inertia phase.</summary>
	/// <returns>The initial rate of linear movement at the start of the inertia phase.</returns>
	public Vector InitialVelocity
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

	/// <summary>Gets or sets the rate the linear movement slows in device-independent units (1/96th inch per unit) per squared millisecond.</summary>
	/// <returns>The rate the linear movement slows in device-independent units (1/96th inch per unit) per squared millisecond.  The default is <see cref="F:System.Double.NaN" />.</returns>
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
			_isDesiredDisplacementSet = false;
			_desiredDisplacement = double.NaN;
		}
	}

	/// <summary>Gets or sets the linear movement of the manipulation at the end of inertia.</summary>
	/// <returns>The linear movement of the manipulation at the end of inertia. The default is <see cref="F:System.Double.NaN" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is set to infinity.-or-The property is set to <see cref="F:System.Double.NaN" />.</exception>
	public double DesiredDisplacement
	{
		get
		{
			return _desiredDisplacement;
		}
		set
		{
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_isDesiredDisplacementSet = true;
			_desiredDisplacement = value;
			_isDesiredDecelerationSet = false;
			_desiredDeceleration = double.NaN;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InertiaTranslationBehavior" /> class. </summary>
	public InertiaTranslationBehavior()
	{
	}

	internal InertiaTranslationBehavior(Vector initialVelocity)
	{
		_initialVelocity = initialVelocity;
	}

	internal bool CanUseForInertia()
	{
		if (!_isInitialVelocitySet && !_isDesiredDecelerationSet)
		{
			return _isDesiredDisplacementSet;
		}
		return true;
	}

	internal static void ApplyParameters(InertiaTranslationBehavior behavior, InertiaProcessor2D processor, Vector initialVelocity)
	{
		if (behavior != null && behavior.CanUseForInertia())
		{
			InertiaTranslationBehavior2D inertiaTranslationBehavior2D = new InertiaTranslationBehavior2D();
			if (behavior._isInitialVelocitySet)
			{
				inertiaTranslationBehavior2D.InitialVelocityX = (float)behavior._initialVelocity.X;
				inertiaTranslationBehavior2D.InitialVelocityY = (float)behavior._initialVelocity.Y;
			}
			else
			{
				inertiaTranslationBehavior2D.InitialVelocityX = (float)initialVelocity.X;
				inertiaTranslationBehavior2D.InitialVelocityY = (float)initialVelocity.Y;
			}
			if (behavior._isDesiredDecelerationSet)
			{
				inertiaTranslationBehavior2D.DesiredDeceleration = (float)behavior._desiredDeceleration;
			}
			if (behavior._isDesiredDisplacementSet)
			{
				inertiaTranslationBehavior2D.DesiredDisplacement = (float)behavior._desiredDisplacement;
			}
			processor.TranslationBehavior = inertiaTranslationBehavior2D;
		}
	}
}
