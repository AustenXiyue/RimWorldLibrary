using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input.Manipulations;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.ManipulationInertiaStarting" /> event. </summary>
public sealed class ManipulationInertiaStartingEventArgs : InputEventArgs
{
	[Flags]
	private enum Behaviors
	{
		None = 0,
		Translation = 1,
		Rotation = 2,
		Expansion = 4
	}

	private List<InertiaParameters2D> _inertiaParameters;

	private InertiaTranslationBehavior _translationBehavior;

	private InertiaRotationBehavior _rotationBehavior;

	private InertiaExpansionBehavior _expansionBehavior;

	private Behaviors _behaviors;

	private bool _isInInertia;

	private IEnumerable<IManipulator> _manipulators;

	/// <summary>Gets the container that the <see cref="P:System.Windows.Input.ManipulationStartedEventArgs.ManipulationOrigin" /> property is relative to.</summary>
	/// <returns>The container that the <see cref="P:System.Windows.Input.ManipulationStartedEventArgs.ManipulationOrigin" /> property is relative to.</returns>
	public IInputElement ManipulationContainer { get; private set; }

	/// <summary>Gets or sets the point from which the manipulation originated.</summary>
	/// <returns>The point from which the manipulation originated.</returns>
	public Point ManipulationOrigin { get; set; }

	/// <summary>Gets the rates of changes to the manipulation that occur before inertia starts.</summary>
	/// <returns>The rates of changes to the manipulation that occur before inertia starts.</returns>
	public ManipulationVelocities InitialVelocities { get; private set; }

	/// <summary>Gets and sets the rate of slowdown of linear inertial movement.</summary>
	/// <returns>The rate of slowdown of linear inertial movement.</returns>
	public InertiaTranslationBehavior TranslationBehavior
	{
		get
		{
			if (!IsBehaviorSet(Behaviors.Translation))
			{
				_translationBehavior = new InertiaTranslationBehavior(InitialVelocities.LinearVelocity);
				SetBehavior(Behaviors.Translation);
			}
			return _translationBehavior;
		}
		set
		{
			_translationBehavior = value;
		}
	}

	/// <summary>Gets or sets the rate of slowdown of rotational inertial movement.</summary>
	/// <returns>The rate of slowdown of rotational inertial movement.</returns>
	public InertiaRotationBehavior RotationBehavior
	{
		get
		{
			if (!IsBehaviorSet(Behaviors.Rotation))
			{
				_rotationBehavior = new InertiaRotationBehavior(InitialVelocities.AngularVelocity);
				SetBehavior(Behaviors.Rotation);
			}
			return _rotationBehavior;
		}
		set
		{
			_rotationBehavior = value;
		}
	}

	/// <summary>Get or sets the rate of slowdown of expansion inertial movement.</summary>
	/// <returns>The rate of slowdown of expansion inertial movement</returns>
	public InertiaExpansionBehavior ExpansionBehavior
	{
		get
		{
			if (!IsBehaviorSet(Behaviors.Expansion))
			{
				_expansionBehavior = new InertiaExpansionBehavior(InitialVelocities.ExpansionVelocity);
				SetBehavior(Behaviors.Expansion);
			}
			return _expansionBehavior;
		}
		set
		{
			_expansionBehavior = value;
		}
	}

	internal bool RequestedCancel { get; private set; }

	/// <summary>Gets a collection of objects that represents the touch contacts for the manipulation.</summary>
	/// <returns>A collection of objects that represents the touch contacts for the manipulation.</returns>
	public IEnumerable<IManipulator> Manipulators
	{
		get
		{
			if (_manipulators == null)
			{
				_manipulators = ((ManipulationDevice)base.Device).GetManipulatorsReadOnly();
			}
			return _manipulators;
		}
	}

	internal ManipulationInertiaStartingEventArgs(ManipulationDevice manipulationDevice, int timestamp, IInputElement manipulationContainer, Point origin, ManipulationVelocities initialVelocities, bool isInInertia)
		: base(manipulationDevice, timestamp)
	{
		if (initialVelocities == null)
		{
			throw new ArgumentNullException("initialVelocities");
		}
		base.RoutedEvent = Manipulation.ManipulationInertiaStartingEvent;
		ManipulationContainer = manipulationContainer;
		ManipulationOrigin = origin;
		InitialVelocities = initialVelocities;
		_isInInertia = isInInertia;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		if ((object)genericHandler == null)
		{
			throw new ArgumentNullException("genericHandler");
		}
		if (genericTarget == null)
		{
			throw new ArgumentNullException("genericTarget");
		}
		if (base.RoutedEvent == Manipulation.ManipulationInertiaStartingEvent)
		{
			((EventHandler<ManipulationInertiaStartingEventArgs>)genericHandler)(genericTarget, this);
		}
		else
		{
			base.InvokeEventHandler(genericHandler, genericTarget);
		}
	}

	/// <summary>Cancels the manipulation.</summary>
	/// <returns>true if the manipulation was successfully canceled; otherwise, false.</returns>
	public bool Cancel()
	{
		if (!_isInInertia)
		{
			RequestedCancel = true;
			return true;
		}
		return false;
	}

	/// <summary>Specifies the behavior of a manipulation during inertia.</summary>
	/// <param name="parameter">The object that specifies the behavior of a manipulation during inertia.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="parameter" /> is null.</exception>
	[Browsable(false)]
	public void SetInertiaParameter(InertiaParameters2D parameter)
	{
		if (parameter == null)
		{
			throw new ArgumentNullException("parameter");
		}
		if (_inertiaParameters == null)
		{
			_inertiaParameters = new List<InertiaParameters2D>();
		}
		_inertiaParameters.Add(parameter);
	}

	internal bool CanBeginInertia()
	{
		if (_inertiaParameters != null && _inertiaParameters.Count > 0)
		{
			return true;
		}
		if (_translationBehavior != null && _translationBehavior.CanUseForInertia())
		{
			return true;
		}
		if (_rotationBehavior != null && _rotationBehavior.CanUseForInertia())
		{
			return true;
		}
		if (_expansionBehavior != null && _expansionBehavior.CanUseForInertia())
		{
			return true;
		}
		return false;
	}

	internal void ApplyParameters(InertiaProcessor2D processor)
	{
		processor.InitialOriginX = (float)ManipulationOrigin.X;
		processor.InitialOriginY = (float)ManipulationOrigin.Y;
		ManipulationVelocities initialVelocities = InitialVelocities;
		InertiaTranslationBehavior.ApplyParameters(_translationBehavior, processor, initialVelocities.LinearVelocity);
		InertiaRotationBehavior.ApplyParameters(_rotationBehavior, processor, initialVelocities.AngularVelocity);
		InertiaExpansionBehavior.ApplyParameters(_expansionBehavior, processor, initialVelocities.ExpansionVelocity);
		if (_inertiaParameters != null)
		{
			int i = 0;
			for (int count = _inertiaParameters.Count; i < count; i++)
			{
				processor.SetParameters(_inertiaParameters[i]);
			}
		}
	}

	private bool IsBehaviorSet(Behaviors behavior)
	{
		return (_behaviors & behavior) == behavior;
	}

	private void SetBehavior(Behaviors behavior)
	{
		_behaviors |= behavior;
	}
}
