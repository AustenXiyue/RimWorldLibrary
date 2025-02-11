using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MS.Internal;

internal static class SynchronizedInputHelper
{
	internal static DependencyObject GetUIParentCore(DependencyObject o)
	{
		if (o is UIElement uIElement)
		{
			return uIElement.GetUIParentCore();
		}
		if (o is ContentElement contentElement)
		{
			return contentElement.GetUIParentCore();
		}
		if (o is UIElement3D uIElement3D)
		{
			return uIElement3D.GetUIParentCore();
		}
		return null;
	}

	internal static bool IsMappedEvent(RoutedEventArgs args)
	{
		RoutedEvent routedEvent = args.RoutedEvent;
		if (routedEvent != Keyboard.KeyUpEvent && routedEvent != Keyboard.KeyDownEvent && routedEvent != TextCompositionManager.TextInputEvent && routedEvent != Mouse.MouseDownEvent)
		{
			return routedEvent == Mouse.MouseUpEvent;
		}
		return true;
	}

	internal static SynchronizedInputType GetPairedInputType(SynchronizedInputType inputType)
	{
		SynchronizedInputType result = SynchronizedInputType.KeyDown;
		switch (inputType)
		{
		case SynchronizedInputType.KeyDown:
			result = SynchronizedInputType.KeyUp;
			break;
		case SynchronizedInputType.KeyUp:
			result = SynchronizedInputType.KeyDown;
			break;
		case SynchronizedInputType.MouseLeftButtonDown:
			result = SynchronizedInputType.MouseLeftButtonUp;
			break;
		case SynchronizedInputType.MouseLeftButtonUp:
			result = SynchronizedInputType.MouseLeftButtonDown;
			break;
		case SynchronizedInputType.MouseRightButtonDown:
			result = SynchronizedInputType.MouseRightButtonUp;
			break;
		case SynchronizedInputType.MouseRightButtonUp:
			result = SynchronizedInputType.MouseRightButtonDown;
			break;
		}
		return result;
	}

	internal static bool IsListening(RoutedEventArgs args)
	{
		if (Array.IndexOf(InputManager.SynchronizedInputEvents, args.RoutedEvent) >= 0)
		{
			return true;
		}
		return false;
	}

	internal static bool IsListening(DependencyObject o, RoutedEventArgs args)
	{
		if (InputManager.ListeningElement == o && Array.IndexOf(InputManager.SynchronizedInputEvents, args.RoutedEvent) >= 0)
		{
			return true;
		}
		return false;
	}

	internal static bool ShouldContinueListening(RoutedEventArgs args)
	{
		return args.RoutedEvent == Keyboard.KeyDownEvent;
	}

	internal static void AddParentPreOpportunityHandler(DependencyObject o, EventRoute route, RoutedEventArgs args)
	{
		DependencyObject dependencyObject = null;
		if (o is Visual || o is Visual3D)
		{
			dependencyObject = UIElementHelper.GetUIParent(o);
		}
		DependencyObject uIParentCore = GetUIParentCore(o);
		if (uIParentCore != null && uIParentCore != dependencyObject)
		{
			if (uIParentCore is UIElement uIElement)
			{
				uIElement.AddSynchronizedInputPreOpportunityHandler(route, args);
			}
			else if (uIParentCore is ContentElement contentElement)
			{
				contentElement.AddSynchronizedInputPreOpportunityHandler(route, args);
			}
			else if (uIParentCore is UIElement3D uIElement3D)
			{
				uIElement3D.AddSynchronizedInputPreOpportunityHandler(route, args);
			}
		}
	}

	internal static void AddHandlerToRoute(DependencyObject o, EventRoute route, RoutedEventHandler eventHandler, bool handledToo)
	{
		route.Add(o, eventHandler, handledToo);
	}

	internal static void PreOpportunityHandler(object sender, RoutedEventArgs args)
	{
		if (args is KeyboardEventArgs)
		{
			InputManager.SynchronizedInputState = SynchronizedInputStates.HadOpportunity;
		}
		else if (args is TextCompositionEventArgs)
		{
			InputManager.SynchronizedInputState = SynchronizedInputStates.HadOpportunity;
		}
		else
		{
			if (!(args is MouseButtonEventArgs { ChangedButton: var changedButton }))
			{
				return;
			}
			switch (changedButton)
			{
			case MouseButton.Left:
				if (InputManager.SynchronizeInputType == SynchronizedInputType.MouseLeftButtonDown || InputManager.SynchronizeInputType == SynchronizedInputType.MouseLeftButtonUp)
				{
					InputManager.SynchronizedInputState = SynchronizedInputStates.HadOpportunity;
				}
				break;
			case MouseButton.Right:
				if (InputManager.SynchronizeInputType == SynchronizedInputType.MouseRightButtonDown || InputManager.SynchronizeInputType == SynchronizedInputType.MouseRightButtonUp)
				{
					InputManager.SynchronizedInputState = SynchronizedInputStates.HadOpportunity;
				}
				break;
			}
		}
	}

	internal static void PostOpportunityHandler(object sender, RoutedEventArgs args)
	{
		if (args is KeyboardEventArgs)
		{
			InputManager.SynchronizedInputState = SynchronizedInputStates.Handled;
		}
		else if (args is TextCompositionEventArgs)
		{
			InputManager.SynchronizedInputState = SynchronizedInputStates.Handled;
		}
		else
		{
			if (!(args is MouseButtonEventArgs { ChangedButton: var changedButton }))
			{
				return;
			}
			switch (changedButton)
			{
			case MouseButton.Left:
				if (InputManager.SynchronizeInputType == SynchronizedInputType.MouseLeftButtonDown || InputManager.SynchronizeInputType == SynchronizedInputType.MouseLeftButtonUp)
				{
					InputManager.SynchronizedInputState = SynchronizedInputStates.Handled;
				}
				break;
			case MouseButton.Right:
				if (InputManager.SynchronizeInputType == SynchronizedInputType.MouseRightButtonDown || InputManager.SynchronizeInputType == SynchronizedInputType.MouseRightButtonUp)
				{
					InputManager.SynchronizedInputState = SynchronizedInputStates.Handled;
				}
				break;
			}
		}
	}

	internal static RoutedEvent[] MapInputTypeToRoutedEvents(SynchronizedInputType inputType)
	{
		RoutedEvent[] array = null;
		switch (inputType)
		{
		case SynchronizedInputType.KeyUp:
			return new RoutedEvent[1] { Keyboard.KeyUpEvent };
		case SynchronizedInputType.KeyDown:
			return new RoutedEvent[2]
			{
				Keyboard.KeyDownEvent,
				TextCompositionManager.TextInputEvent
			};
		case SynchronizedInputType.MouseLeftButtonDown:
		case SynchronizedInputType.MouseRightButtonDown:
			return new RoutedEvent[1] { Mouse.MouseDownEvent };
		case SynchronizedInputType.MouseLeftButtonUp:
		case SynchronizedInputType.MouseRightButtonUp:
			return new RoutedEvent[1] { Mouse.MouseUpEvent };
		default:
			return null;
		}
	}

	internal static void RaiseAutomationEvents()
	{
		if (InputManager.ListeningElement is UIElement uIElement)
		{
			RaiseAutomationEvent(uIElement.GetAutomationPeer());
		}
		else if (InputManager.ListeningElement is ContentElement contentElement)
		{
			RaiseAutomationEvent(contentElement.GetAutomationPeer());
		}
		else if (InputManager.ListeningElement is UIElement3D uIElement3D)
		{
			RaiseAutomationEvent(uIElement3D.GetAutomationPeer());
		}
	}

	internal static void RaiseAutomationEvent(AutomationPeer peer)
	{
		if (peer != null)
		{
			switch (InputManager.SynchronizedInputState)
			{
			case SynchronizedInputStates.Handled:
				peer.RaiseAutomationEvent(AutomationEvents.InputReachedTarget);
				break;
			case SynchronizedInputStates.Discarded:
				peer.RaiseAutomationEvent(AutomationEvents.InputDiscarded);
				break;
			default:
				peer.RaiseAutomationEvent(AutomationEvents.InputReachedOtherElement);
				break;
			}
		}
	}
}
