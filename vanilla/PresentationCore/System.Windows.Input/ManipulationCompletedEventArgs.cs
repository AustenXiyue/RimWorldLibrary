using System.Collections.Generic;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event. </summary>
public sealed class ManipulationCompletedEventArgs : InputEventArgs
{
	private IEnumerable<IManipulator> _manipulators;

	/// <summary>Gets a value that indicates whether the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs during inertia.</summary>
	/// <returns>true if the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs during inertia; false if the event occurs while the user has contact with the <see cref="T:System.Windows.UIElement" />.</returns>
	public bool IsInertial { get; private set; }

	/// <summary>Gets the container that defines the coordinates for the manipulation.</summary>
	/// <returns>The container that defines the coordinates for the manipulation.</returns>
	public IInputElement ManipulationContainer { get; private set; }

	/// <summary>Gets the point from which the manipulation originated.</summary>
	/// <returns>The point from which the manipulation originated.</returns>
	public Point ManipulationOrigin { get; private set; }

	/// <summary>Gets the total transformation that occurs during the current manipulation.</summary>
	/// <returns>The total transformation that occurs during the current manipulation.</returns>
	public ManipulationDelta TotalManipulation { get; private set; }

	/// <summary>Gets the velocities that are used for the manipulation.</summary>
	/// <returns>The velocities that are used for the manipulation.</returns>
	public ManipulationVelocities FinalVelocities { get; private set; }

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

	internal ManipulationCompletedEventArgs(ManipulationDevice manipulationDevice, int timestamp, IInputElement manipulationContainer, Point origin, ManipulationDelta total, ManipulationVelocities velocities, bool isInertial)
		: base(manipulationDevice, timestamp)
	{
		if (total == null)
		{
			throw new ArgumentNullException("total");
		}
		if (velocities == null)
		{
			throw new ArgumentNullException("velocities");
		}
		base.RoutedEvent = Manipulation.ManipulationCompletedEvent;
		ManipulationContainer = manipulationContainer;
		ManipulationOrigin = origin;
		TotalManipulation = total;
		FinalVelocities = velocities;
		IsInertial = isInertial;
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
		if (base.RoutedEvent == Manipulation.ManipulationCompletedEvent)
		{
			((EventHandler<ManipulationCompletedEventArgs>)genericHandler)(genericTarget, this);
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
		if (!IsInertial)
		{
			RequestedCancel = true;
			return true;
		}
		return false;
	}
}
