using System.Collections.Generic;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.ManipulationStarted" /> event. </summary>
public sealed class ManipulationStartedEventArgs : InputEventArgs
{
	private IEnumerable<IManipulator> _manipulators;

	/// <summary>Gets the container that the <see cref="P:System.Windows.Input.ManipulationStartedEventArgs.ManipulationOrigin" /> property is relative to.</summary>
	/// <returns>The container that the <see cref="P:System.Windows.Input.ManipulationStartedEventArgs.ManipulationOrigin" /> property is relative to.</returns>
	public IInputElement ManipulationContainer { get; private set; }

	/// <summary>Gets the point from which the manipulation originated.</summary>
	/// <returns>The point from which the manipulation originated.</returns>
	public Point ManipulationOrigin { get; private set; }

	internal bool RequestedComplete { get; private set; }

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

	internal ManipulationStartedEventArgs(ManipulationDevice manipulationDevice, int timestamp, IInputElement manipulationContainer, Point origin)
		: base(manipulationDevice, timestamp)
	{
		base.RoutedEvent = Manipulation.ManipulationStartedEvent;
		ManipulationContainer = manipulationContainer;
		ManipulationOrigin = origin;
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
		if (base.RoutedEvent == Manipulation.ManipulationStartedEvent)
		{
			((EventHandler<ManipulationStartedEventArgs>)genericHandler)(genericTarget, this);
		}
		else
		{
			base.InvokeEventHandler(genericHandler, genericTarget);
		}
	}

	/// <summary>Completes the manipulation without inertia.</summary>
	public void Complete()
	{
		RequestedComplete = true;
		RequestedCancel = false;
	}

	/// <summary>Cancels the manipulation.</summary>
	/// <returns>true if manipulation was successfully, otherwise, false.</returns>
	public bool Cancel()
	{
		RequestedCancel = true;
		RequestedComplete = false;
		return true;
	}
}
