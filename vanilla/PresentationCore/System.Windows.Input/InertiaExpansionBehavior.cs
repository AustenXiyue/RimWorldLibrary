using System.Windows.Input.Manipulations;

namespace System.Windows.Input;

/// <summary>Controls the deceleration of a resizing manipulation during inertia.</summary>
public class InertiaExpansionBehavior
{
	private bool _isInitialVelocitySet;

	private Vector _initialVelocity = new Vector(double.NaN, double.NaN);

	private bool _isDesiredDecelerationSet;

	private double _desiredDeceleration = double.NaN;

	private bool _isDesiredExpansionSet;

	private Vector _desiredExpansion = new Vector(double.NaN, double.NaN);

	private bool _isInitialRadiusSet;

	private double _initialRadius = 1.0;

	/// <summary>Gets or sets the initial rate the element resizes at the start of inertia.</summary>
	/// <returns>The initial rate the element resizes at the start of inertia.</returns>
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

	/// <summary>Gets or sets the rate that resizing slows in device-independent units (1/96th inch per unit) per square milliseconds.</summary>
	/// <returns>The rate that resizing slows in device-independent units (1/96th inch per unit) per square milliseconds. The default is <see cref="F:System.Double.NaN" />.</returns>
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
			_isDesiredExpansionSet = false;
			_desiredExpansion = new Vector(double.NaN, double.NaN);
		}
	}

	/// <summary>Gets or sets the amount the element resizes at the end of inertia.</summary>
	/// <returns>The amount the element resizes at the end of inertia. The default is a <see cref="T:System.Windows.Vector" /> that has its <see cref="P:System.Windows.Vector.X" /> and <see cref="P:System.Windows.Vector.Y" /> properties set to <see cref="F:System.Double.NaN" />.</returns>
	public Vector DesiredExpansion
	{
		get
		{
			return _desiredExpansion;
		}
		set
		{
			_isDesiredExpansionSet = true;
			_desiredExpansion = value;
			_isDesiredDecelerationSet = false;
			_desiredDeceleration = double.NaN;
		}
	}

	/// <summary>Gets or sets the initial average radius.</summary>
	/// <returns>The initial average radius.</returns>
	public double InitialRadius
	{
		get
		{
			return _initialRadius;
		}
		set
		{
			_isInitialRadiusSet = true;
			_initialRadius = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InertiaExpansionBehavior" /> class. </summary>
	public InertiaExpansionBehavior()
	{
	}

	internal InertiaExpansionBehavior(Vector initialVelocity)
	{
		_initialVelocity = initialVelocity;
	}

	internal bool CanUseForInertia()
	{
		if (!_isInitialVelocitySet && !_isInitialRadiusSet && !_isDesiredDecelerationSet)
		{
			return _isDesiredExpansionSet;
		}
		return true;
	}

	internal static void ApplyParameters(InertiaExpansionBehavior behavior, InertiaProcessor2D processor, Vector initialVelocity)
	{
		if (behavior != null && behavior.CanUseForInertia())
		{
			InertiaExpansionBehavior2D inertiaExpansionBehavior2D = new InertiaExpansionBehavior2D();
			if (behavior._isInitialVelocitySet)
			{
				inertiaExpansionBehavior2D.InitialVelocityX = (float)behavior._initialVelocity.X;
				inertiaExpansionBehavior2D.InitialVelocityY = (float)behavior._initialVelocity.Y;
			}
			else
			{
				inertiaExpansionBehavior2D.InitialVelocityX = (float)initialVelocity.X;
				inertiaExpansionBehavior2D.InitialVelocityY = (float)initialVelocity.Y;
			}
			if (behavior._isDesiredDecelerationSet)
			{
				inertiaExpansionBehavior2D.DesiredDeceleration = (float)behavior._desiredDeceleration;
			}
			if (behavior._isDesiredExpansionSet)
			{
				inertiaExpansionBehavior2D.DesiredExpansionX = (float)behavior._desiredExpansion.X;
				inertiaExpansionBehavior2D.DesiredExpansionY = (float)behavior._desiredExpansion.Y;
			}
			if (behavior._isInitialRadiusSet)
			{
				inertiaExpansionBehavior2D.InitialRadius = (float)behavior._initialRadius;
			}
			processor.ExpansionBehavior = inertiaExpansionBehavior2D;
		}
	}
}
