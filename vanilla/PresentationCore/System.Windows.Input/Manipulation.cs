using System.ComponentModel;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Contains methods to get and update information about a manipulation.</summary>
public static class Manipulation
{
	internal static readonly RoutedEvent ManipulationStartingEvent = EventManager.RegisterRoutedEvent("ManipulationStarting", RoutingStrategy.Bubble, typeof(EventHandler<ManipulationStartingEventArgs>), typeof(ManipulationDevice));

	internal static readonly RoutedEvent ManipulationStartedEvent = EventManager.RegisterRoutedEvent("ManipulationStarted", RoutingStrategy.Bubble, typeof(EventHandler<ManipulationStartedEventArgs>), typeof(ManipulationDevice));

	internal static readonly RoutedEvent ManipulationDeltaEvent = EventManager.RegisterRoutedEvent("ManipulationDelta", RoutingStrategy.Bubble, typeof(EventHandler<ManipulationDeltaEventArgs>), typeof(ManipulationDevice));

	internal static readonly RoutedEvent ManipulationInertiaStartingEvent = EventManager.RegisterRoutedEvent("ManipulationInertiaStarting", RoutingStrategy.Bubble, typeof(EventHandler<ManipulationInertiaStartingEventArgs>), typeof(ManipulationDevice));

	internal static readonly RoutedEvent ManipulationBoundaryFeedbackEvent = EventManager.RegisterRoutedEvent("ManipulationBoundaryFeedback", RoutingStrategy.Bubble, typeof(EventHandler<ManipulationBoundaryFeedbackEventArgs>), typeof(ManipulationDevice));

	internal static readonly RoutedEvent ManipulationCompletedEvent = EventManager.RegisterRoutedEvent("ManipulationCompleted", RoutingStrategy.Bubble, typeof(EventHandler<ManipulationCompletedEventArgs>), typeof(ManipulationDevice));

	/// <summary>Gets a value that indicates whether a manipulation is associated with the specified element.</summary>
	/// <returns>true if a manipulation is associated with the specified element; otherwise, false.</returns>
	/// <param name="element">The element to check.</param>
	public static bool IsManipulationActive(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return GetActiveManipulationDevice(element) != null;
	}

	private static ManipulationDevice GetActiveManipulationDevice(UIElement element)
	{
		ManipulationDevice manipulationDevice = ManipulationDevice.GetManipulationDevice(element);
		if (manipulationDevice != null && manipulationDevice.IsManipulationActive)
		{
			return manipulationDevice;
		}
		return null;
	}

	/// <summary>Stops the manipulation and begins inertia on the specified element.</summary>
	/// <param name="element">The element on which to begin inertia.</param>
	public static void StartInertia(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		ManipulationDevice.GetManipulationDevice(element)?.CompleteManipulation(withInertia: true);
	}

	/// <summary>Completes the active manipulation on the specified element. When called, manipulation input is no longer tracked and inertia on the specified element stops.</summary>
	/// <param name="element">The element on which to complete manipulation.</param>
	public static void CompleteManipulation(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!TryCompleteManipulation(element))
		{
			throw new InvalidOperationException(SR.Manipulation_ManipulationNotActive);
		}
	}

	internal static bool TryCompleteManipulation(UIElement element)
	{
		ManipulationDevice manipulationDevice = ManipulationDevice.GetManipulationDevice(element);
		if (manipulationDevice != null)
		{
			manipulationDevice.CompleteManipulation(withInertia: false);
			return true;
		}
		return false;
	}

	/// <summary>Sets the mode of manipulation for the specified element.</summary>
	/// <param name="element">The element on which to set the manipulation mode.</param>
	/// <param name="mode">The new manipulation mode.</param>
	public static void SetManipulationMode(UIElement element, ManipulationModes mode)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		ManipulationDevice activeManipulationDevice = GetActiveManipulationDevice(element);
		if (activeManipulationDevice != null)
		{
			activeManipulationDevice.ManipulationMode = mode;
			return;
		}
		throw new InvalidOperationException(SR.Manipulation_ManipulationNotActive);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.ManipulationModes" /> for the specified element.</summary>
	/// <returns>One of the enumeration values.</returns>
	/// <param name="element">The element for which to get the manipulation mode.</param>
	public static ManipulationModes GetManipulationMode(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return ManipulationDevice.GetManipulationDevice(element)?.ManipulationMode ?? ManipulationModes.None;
	}

	/// <summary>Sets the element that defines the coordinates for the manipulation of the specified element.</summary>
	/// <param name="element">The element with which the manipulation is associated.</param>
	/// <param name="container">The container that defines the coordinate space.</param>
	public static void SetManipulationContainer(UIElement element, IInputElement container)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		ManipulationDevice activeManipulationDevice = GetActiveManipulationDevice(element);
		if (activeManipulationDevice != null)
		{
			activeManipulationDevice.ManipulationContainer = container;
			return;
		}
		throw new InvalidOperationException(SR.Manipulation_ManipulationNotActive);
	}

	/// <summary>Gets the container that defines the coordinates for the manipulation.</summary>
	/// <returns>The container that defines the coordinate space.</returns>
	/// <param name="element">The element on which there is an active manipulation.</param>
	public static IInputElement GetManipulationContainer(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return ManipulationDevice.GetManipulationDevice(element)?.ManipulationContainer;
	}

	/// <summary>Sets the pivot of the single-point manipulation of the specified element.</summary>
	/// <param name="element">The element that has an active manipulation.</param>
	/// <param name="pivot">An object that describes the pivot.</param>
	public static void SetManipulationPivot(UIElement element, ManipulationPivot pivot)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		ManipulationDevice activeManipulationDevice = GetActiveManipulationDevice(element);
		if (activeManipulationDevice != null)
		{
			activeManipulationDevice.ManipulationPivot = pivot;
			return;
		}
		throw new InvalidOperationException(SR.Manipulation_ManipulationNotActive);
	}

	/// <summary>Returns an object that describes how a rotation occurs with one point of user input.</summary>
	/// <returns>An object that describes how a rotation occurs with one point of user input.</returns>
	/// <param name="element">The element on which there is a manipulation.</param>
	public static ManipulationPivot GetManipulationPivot(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return ManipulationDevice.GetManipulationDevice(element)?.ManipulationPivot;
	}

	/// <summary>Associates a <see cref="T:System.Windows.Input.IManipulator" /> object with the specified element.</summary>
	/// <param name="element">The element to associate the manipulator with.</param>
	/// <param name="manipulator">The object that provides the position of the input that creates or is added to a manipulation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.-or-<paramref name="manipulator" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.UIElement.IsManipulationEnabled" /> property on element is false.</exception>
	public static void AddManipulator(UIElement element, IManipulator manipulator)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (manipulator == null)
		{
			throw new ArgumentNullException("manipulator");
		}
		if (!element.IsManipulationEnabled)
		{
			throw new InvalidOperationException(SR.Manipulation_ManipulationNotEnabled);
		}
		ManipulationDevice.AddManipulationDevice(element).AddManipulator(manipulator);
	}

	/// <summary>Removes the association between the specified <see cref="T:System.Windows.Input.IManipulator" /> object and the element.</summary>
	/// <param name="element">The element to remove the associated manipulator from.</param>
	/// <param name="manipulator">The object that provides the position of the input.</param>
	public static void RemoveManipulator(UIElement element, IManipulator manipulator)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (manipulator == null)
		{
			throw new ArgumentNullException("manipulator");
		}
		if (!TryRemoveManipulator(element, manipulator))
		{
			throw new InvalidOperationException(SR.Manipulation_ManipulationNotActive);
		}
	}

	internal static bool TryRemoveManipulator(UIElement element, IManipulator manipulator)
	{
		ManipulationDevice manipulationDevice = ManipulationDevice.GetManipulationDevice(element);
		if (manipulationDevice != null)
		{
			manipulationDevice.RemoveManipulator(manipulator);
			return true;
		}
		return false;
	}

	/// <summary>Adds parameters to the manipulation of the specified element.</summary>
	/// <param name="element">The element that has the manipulation that the parameter is added to.</param>
	/// <param name="parameter">The parameter to add.</param>
	[Browsable(false)]
	public static void SetManipulationParameter(UIElement element, ManipulationParameters2D parameter)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (parameter == null)
		{
			throw new ArgumentNullException("parameter");
		}
		ManipulationDevice activeManipulationDevice = GetActiveManipulationDevice(element);
		if (activeManipulationDevice != null)
		{
			activeManipulationDevice.SetManipulationParameters(parameter);
			return;
		}
		throw new InvalidOperationException(SR.Manipulation_ManipulationNotActive);
	}

	internal static UIElement FindManipulationParent(Visual visual)
	{
		while (visual != null)
		{
			if (visual is UIElement { IsManipulationEnabled: not false } uIElement)
			{
				return uIElement;
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		return null;
	}
}
