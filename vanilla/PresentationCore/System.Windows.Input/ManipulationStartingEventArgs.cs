using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input.Manipulations;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.ManipulationStarting" />, event. </summary>
public sealed class ManipulationStartingEventArgs : InputEventArgs
{
	private List<ManipulationParameters2D> _parameters;

	private ManipulationModes _mode;

	private IEnumerable<IManipulator> _manipulators;

	/// <summary>Gets or sets which types of manipulations are possible.</summary>
	/// <returns>One of the enumeration values. The default is <see cref="F:System.Windows.Input.ManipulationModes.All" />.</returns>
	/// <exception cref="T:System.ArgumentException">The property is set to a value other than one or more of the <see cref="T:System.Windows.Input.ManipulationModes" /> enumerations value</exception>
	public ManipulationModes Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			if ((value & ~ManipulationModes.All) != 0)
			{
				throw new ArgumentException(SR.Manipulation_InvalidManipulationMode, "value");
			}
			_mode = value;
		}
	}

	/// <summary>Gets or sets the container that all manipulation events and calculations are relative to.</summary>
	/// <returns>The container that all manipulation events and calculations are relative to. The default is the element on which the event occurred.</returns>
	public IInputElement ManipulationContainer { get; set; }

	/// <summary>Gets or sets an object that describes the pivot for a single-point manipulation.</summary>
	/// <returns>An object that describes the pivot for a single-point manipulation.</returns>
	public ManipulationPivot Pivot { get; set; }

	/// <summary>Gets or sets whether one finger can start a manipulation.</summary>
	/// <returns>true one finger can start a manipulation; otherwise, false. The default is true.</returns>
	public bool IsSingleTouchEnabled { get; set; }

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

	internal IList<ManipulationParameters2D> Parameters => _parameters;

	internal ManipulationStartingEventArgs(ManipulationDevice manipulationDevice, int timestamp)
		: base(manipulationDevice, timestamp)
	{
		base.RoutedEvent = Manipulation.ManipulationStartingEvent;
		Mode = ManipulationModes.All;
		IsSingleTouchEnabled = true;
	}

	/// <summary>Cancels the manipulation and promotes touch to mouse events.</summary>
	/// <returns>true if touch was successfully promoted to mouse events, otherwise, false.</returns>
	public bool Cancel()
	{
		RequestedCancel = true;
		return true;
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
		if (base.RoutedEvent == Manipulation.ManipulationStartingEvent)
		{
			((EventHandler<ManipulationStartingEventArgs>)genericHandler)(genericTarget, this);
		}
		else
		{
			base.InvokeEventHandler(genericHandler, genericTarget);
		}
	}

	/// <summary>Adds parameters to the current manipulation of the specified element.</summary>
	/// <param name="parameter">The parameter to add.</param>
	[Browsable(false)]
	public void SetManipulationParameter(ManipulationParameters2D parameter)
	{
		if (_parameters == null)
		{
			_parameters = new List<ManipulationParameters2D>(1);
		}
		_parameters.Add(parameter);
	}
}
