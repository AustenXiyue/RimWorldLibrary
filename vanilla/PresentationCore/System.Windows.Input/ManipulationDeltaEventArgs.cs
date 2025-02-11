using System.Collections.Generic;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event. </summary>
public sealed class ManipulationDeltaEventArgs : InputEventArgs
{
	private IEnumerable<IManipulator> _manipulators;

	/// <summary>Gets a value that indicates whether the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event occurs during inertia.</summary>
	/// <returns>true if the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event occurs during inertia; false if the event occurs while the user has contact with the <see cref="T:System.Windows.UIElement" />.</returns>
	public bool IsInertial { get; private set; }

	/// <summary>Gets the container that defines the coordinates for the manipulation.</summary>
	/// <returns>The container that defines the coordinates for the manipulation.</returns>
	public IInputElement ManipulationContainer { get; private set; }

	/// <summary>Gets the point from which the manipulation originated.</summary>
	/// <returns>The point from which the manipulation originated.</returns>
	public Point ManipulationOrigin { get; private set; }

	/// <summary>Gets the cumulated changes of the current manipulation.</summary>
	/// <returns>The cumulated changes of the current manipulation.</returns>
	public ManipulationDelta CumulativeManipulation { get; private set; }

	/// <summary>Gets the most recent changes of the current manipulation.</summary>
	/// <returns>The most recent changes of the current manipulation.</returns>
	public ManipulationDelta DeltaManipulation { get; private set; }

	/// <summary>Gets the rates of the most recent changes to the manipulation.</summary>
	/// <returns>The rates of the most recent changes to the manipulation.</returns>
	public ManipulationVelocities Velocities { get; private set; }

	internal ManipulationDelta UnusedManipulation { get; private set; }

	internal bool RequestedComplete { get; private set; }

	internal bool RequestedInertia { get; private set; }

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

	internal ManipulationDeltaEventArgs(ManipulationDevice manipulationDevice, int timestamp, IInputElement manipulationContainer, Point origin, ManipulationDelta delta, ManipulationDelta cumulative, ManipulationVelocities velocities, bool isInertial)
		: base(manipulationDevice, timestamp)
	{
		if (delta == null)
		{
			throw new ArgumentNullException("delta");
		}
		if (cumulative == null)
		{
			throw new ArgumentNullException("cumulative");
		}
		if (velocities == null)
		{
			throw new ArgumentNullException("velocities");
		}
		base.RoutedEvent = Manipulation.ManipulationDeltaEvent;
		ManipulationContainer = manipulationContainer;
		ManipulationOrigin = origin;
		DeltaManipulation = delta;
		CumulativeManipulation = cumulative;
		Velocities = velocities;
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
		if (base.RoutedEvent == Manipulation.ManipulationDeltaEvent)
		{
			((EventHandler<ManipulationDeltaEventArgs>)genericHandler)(genericTarget, this);
		}
		else
		{
			base.InvokeEventHandler(genericHandler, genericTarget);
		}
	}

	/// <summary>Specifies that the manipulation has gone beyond certain boundaries.</summary>
	/// <param name="unusedManipulation">The portion of the manipulation that represents moving beyond the boundary.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="unusedManipulation" /> is null.</exception>
	public void ReportBoundaryFeedback(ManipulationDelta unusedManipulation)
	{
		if (unusedManipulation == null)
		{
			throw new ArgumentNullException("unusedManipulation");
		}
		UnusedManipulation = unusedManipulation;
	}

	/// <summary>Completes the manipulation without inertia.</summary>
	public void Complete()
	{
		RequestedComplete = true;
		RequestedInertia = false;
		RequestedCancel = false;
	}

	/// <summary>Starts inertia on the manipulation by ignoring subsequent contact movements and raising the <see cref="E:System.Windows.UIElement.ManipulationInertiaStarting" /> event.</summary>
	public void StartInertia()
	{
		RequestedComplete = true;
		RequestedInertia = true;
		RequestedCancel = false;
	}

	/// <summary>Cancels the manipulation.</summary>
	/// <returns>true if the manipulation was successfully canceled; otherwise, false.</returns>
	public bool Cancel()
	{
		if (!IsInertial)
		{
			RequestedCancel = true;
			RequestedComplete = false;
			RequestedInertia = false;
			return true;
		}
		return false;
	}
}
