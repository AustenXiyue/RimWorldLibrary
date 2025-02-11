using System.Collections.Generic;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.ManipulationBoundaryFeedback" /> event. </summary>
public sealed class ManipulationBoundaryFeedbackEventArgs : InputEventArgs
{
	private IEnumerable<IManipulator> _manipulators;

	/// <summary>Gets the container that the <see cref="P:System.Windows.Input.ManipulationBoundaryFeedbackEventArgs.BoundaryFeedback" /> property is relative to.</summary>
	/// <returns>The container that the <see cref="P:System.Windows.Input.ManipulationBoundaryFeedbackEventArgs.BoundaryFeedback" /> property is relative to.</returns>
	public IInputElement ManipulationContainer { get; private set; }

	/// <summary>Gets the unused portion of the direct manipulation.</summary>
	/// <returns>The unused portion of the direct manipulation.</returns>
	public ManipulationDelta BoundaryFeedback { get; private set; }

	internal Func<Point, Point> CompensateForBoundaryFeedback { get; set; }

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

	internal ManipulationBoundaryFeedbackEventArgs(ManipulationDevice manipulationDevice, int timestamp, IInputElement manipulationContainer, ManipulationDelta boundaryFeedback)
		: base(manipulationDevice, timestamp)
	{
		base.RoutedEvent = Manipulation.ManipulationBoundaryFeedbackEvent;
		ManipulationContainer = manipulationContainer;
		BoundaryFeedback = boundaryFeedback;
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
		if (base.RoutedEvent == Manipulation.ManipulationBoundaryFeedbackEvent)
		{
			((EventHandler<ManipulationBoundaryFeedbackEventArgs>)genericHandler)(genericTarget, this);
		}
		else
		{
			base.InvokeEventHandler(genericHandler, genericTarget);
		}
	}
}
