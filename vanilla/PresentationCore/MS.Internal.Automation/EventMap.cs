using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using MS.Internal.PresentationCore;

namespace MS.Internal.Automation;

internal static class EventMap
{
	private class EventInfo
	{
		internal int NumberOfListeners;

		internal EventInfo()
		{
			NumberOfListeners = 1;
		}
	}

	private static Dictionary<int, EventInfo> _eventsTable;

	private static readonly object _lock = new object();

	internal static bool HasListeners => _eventsTable != null;

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool IsKnownLegacyEvent(int id)
	{
		if (id == AutomationElementIdentifiers.ToolTipOpenedEvent.Id || id == AutomationElementIdentifiers.ToolTipClosedEvent.Id || id == AutomationElementIdentifiers.MenuOpenedEvent.Id || id == AutomationElementIdentifiers.MenuClosedEvent.Id || id == AutomationElementIdentifiers.AutomationFocusChangedEvent.Id || id == InvokePatternIdentifiers.InvokedEvent.Id || id == SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent.Id || id == SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent.Id || id == SelectionItemPatternIdentifiers.ElementSelectedEvent.Id || id == SelectionPatternIdentifiers.InvalidatedEvent.Id || id == TextPatternIdentifiers.TextSelectionChangedEvent.Id || id == TextPatternIdentifiers.TextChangedEvent.Id || id == AutomationElementIdentifiers.AsyncContentLoadedEvent.Id || id == AutomationElementIdentifiers.AutomationPropertyChangedEvent.Id || id == AutomationElementIdentifiers.StructureChangedEvent.Id || id == SynchronizedInputPatternIdentifiers.InputReachedTargetEvent?.Id || id == SynchronizedInputPatternIdentifiers.InputReachedOtherElementEvent?.Id || id == SynchronizedInputPatternIdentifiers.InputDiscardedEvent?.Id)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool IsKnownNewEvent(int id)
	{
		if (id == AutomationElementIdentifiers.LiveRegionChangedEvent?.Id || id == AutomationElementIdentifiers.NotificationEvent?.Id || id == AutomationElementIdentifiers.ActiveTextPositionChangedEvent?.Id)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool IsKnownEvent(int id)
	{
		if (IsKnownLegacyEvent(id) || (!AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures && IsKnownNewEvent(id)))
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static AutomationEvent GetRegisteredEventObjectHelper(AutomationEvents eventId)
	{
		AutomationEvent automationEvent = null;
		automationEvent = eventId switch
		{
			AutomationEvents.ToolTipOpened => AutomationElementIdentifiers.ToolTipOpenedEvent, 
			AutomationEvents.ToolTipClosed => AutomationElementIdentifiers.ToolTipClosedEvent, 
			AutomationEvents.MenuOpened => AutomationElementIdentifiers.MenuOpenedEvent, 
			AutomationEvents.MenuClosed => AutomationElementIdentifiers.MenuClosedEvent, 
			AutomationEvents.AutomationFocusChanged => AutomationElementIdentifiers.AutomationFocusChangedEvent, 
			AutomationEvents.InvokePatternOnInvoked => InvokePatternIdentifiers.InvokedEvent, 
			AutomationEvents.SelectionItemPatternOnElementAddedToSelection => SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent, 
			AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection => SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent, 
			AutomationEvents.SelectionItemPatternOnElementSelected => SelectionItemPatternIdentifiers.ElementSelectedEvent, 
			AutomationEvents.SelectionPatternOnInvalidated => SelectionPatternIdentifiers.InvalidatedEvent, 
			AutomationEvents.TextPatternOnTextSelectionChanged => TextPatternIdentifiers.TextSelectionChangedEvent, 
			AutomationEvents.TextPatternOnTextChanged => TextPatternIdentifiers.TextChangedEvent, 
			AutomationEvents.AsyncContentLoaded => AutomationElementIdentifiers.AsyncContentLoadedEvent, 
			AutomationEvents.PropertyChanged => AutomationElementIdentifiers.AutomationPropertyChangedEvent, 
			AutomationEvents.StructureChanged => AutomationElementIdentifiers.StructureChangedEvent, 
			AutomationEvents.InputReachedTarget => SynchronizedInputPatternIdentifiers.InputReachedTargetEvent, 
			AutomationEvents.InputReachedOtherElement => SynchronizedInputPatternIdentifiers.InputReachedOtherElementEvent, 
			AutomationEvents.InputDiscarded => SynchronizedInputPatternIdentifiers.InputDiscardedEvent, 
			AutomationEvents.LiveRegionChanged => AutomationElementIdentifiers.LiveRegionChangedEvent, 
			AutomationEvents.Notification => AutomationElementIdentifiers.NotificationEvent, 
			AutomationEvents.ActiveTextPositionChanged => AutomationElementIdentifiers.ActiveTextPositionChangedEvent, 
			_ => throw new ArgumentException(SR.Automation_InvalidEventId, "eventId"), 
		};
		if (automationEvent != null && !_eventsTable.ContainsKey(automationEvent.Id))
		{
			automationEvent = null;
		}
		return automationEvent;
	}

	internal static void AddEvent(int idEvent)
	{
		if (!IsKnownEvent(idEvent))
		{
			return;
		}
		bool flag = false;
		lock (_lock)
		{
			if (_eventsTable == null)
			{
				_eventsTable = new Dictionary<int, EventInfo>(20);
				flag = true;
			}
			if (_eventsTable.TryGetValue(idEvent, out var value))
			{
				value.NumberOfListeners++;
			}
			else
			{
				_eventsTable[idEvent] = new EventInfo();
			}
		}
		if (flag)
		{
			NotifySources();
		}
	}

	internal static void RemoveEvent(int idEvent)
	{
		lock (_lock)
		{
			if (_eventsTable == null || !_eventsTable.TryGetValue(idEvent, out var value))
			{
				return;
			}
			value.NumberOfListeners--;
			if (value.NumberOfListeners <= 0)
			{
				_eventsTable.Remove(idEvent);
				if (_eventsTable.Count == 0)
				{
					_eventsTable = null;
				}
			}
		}
	}

	internal static bool HasRegisteredEvent(AutomationEvents eventId)
	{
		lock (_lock)
		{
			if (_eventsTable != null && _eventsTable.Count != 0)
			{
				return GetRegisteredEventObjectHelper(eventId) != null;
			}
		}
		return false;
	}

	internal static AutomationEvent GetRegisteredEvent(AutomationEvents eventId)
	{
		lock (_lock)
		{
			if (_eventsTable != null && _eventsTable.Count != 0)
			{
				return GetRegisteredEventObjectHelper(eventId);
			}
		}
		return null;
	}

	private static void NotifySources()
	{
		WeakReferenceListEnumerator enumerator = PresentationSource.CriticalCurrentSources.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PresentationSource presentationSource = (PresentationSource)enumerator.Current;
			if (presentationSource.IsDisposed)
			{
				continue;
			}
			presentationSource.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate(object state)
			{
				PresentationSource presentationSource2 = (PresentationSource)state;
				if (presentationSource2 != null && !presentationSource2.IsDisposed)
				{
					presentationSource2.RootVisual = presentationSource2.RootVisual;
				}
				return (object)null;
			}, presentationSource);
		}
	}
}
