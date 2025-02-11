using System.Runtime.InteropServices;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Provides helper methods and fields for initiating drag-and-drop operations, including a method to begin a drag-and-drop operation, and facilities for adding and removing drag-and-drop related event handlers.</summary>
public static class DragDrop
{
	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.PreviewQueryContinueDrag" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.PreviewQueryContinueDrag" /> attached event.</returns>
	public static readonly RoutedEvent PreviewQueryContinueDragEvent = EventManager.RegisterRoutedEvent("PreviewQueryContinueDrag", RoutingStrategy.Tunnel, typeof(QueryContinueDragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.QueryContinueDrag" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> attached event.</returns>
	public static readonly RoutedEvent QueryContinueDragEvent = EventManager.RegisterRoutedEvent("QueryContinueDrag", RoutingStrategy.Bubble, typeof(QueryContinueDragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.PreviewGiveFeedback" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.PreviewGiveFeedback" /> attached event.</returns>
	public static readonly RoutedEvent PreviewGiveFeedbackEvent = EventManager.RegisterRoutedEvent("PreviewGiveFeedback", RoutingStrategy.Tunnel, typeof(GiveFeedbackEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.GiveFeedback" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.GiveFeedback" /> attached event.</returns>
	public static readonly RoutedEvent GiveFeedbackEvent = EventManager.RegisterRoutedEvent("GiveFeedback", RoutingStrategy.Bubble, typeof(GiveFeedbackEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.PreviewDragEnter" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.PreviewDragEnter" /> attached event.</returns>
	public static readonly RoutedEvent PreviewDragEnterEvent = EventManager.RegisterRoutedEvent("PreviewDragEnter", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.DragEnter" />  attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.DragEnter" /> attached event.</returns>
	public static readonly RoutedEvent DragEnterEvent = EventManager.RegisterRoutedEvent("DragEnter", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.PreviewDragOver" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.PreviewDragOver" /> attached event.</returns>
	public static readonly RoutedEvent PreviewDragOverEvent = EventManager.RegisterRoutedEvent("PreviewDragOver", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.DragOver" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.DragOver" /> attached event.</returns>
	public static readonly RoutedEvent DragOverEvent = EventManager.RegisterRoutedEvent("DragOver", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.PreviewDragLeave" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.PreviewDragLeave" /> attached event.</returns>
	public static readonly RoutedEvent PreviewDragLeaveEvent = EventManager.RegisterRoutedEvent("PreviewDragLeave", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.DragLeave" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.DragLeave" /> attached event.</returns>
	public static readonly RoutedEvent DragLeaveEvent = EventManager.RegisterRoutedEvent("DragLeave", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.PreviewDrop" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.PreviewDrop" /> attached event.</returns>
	public static readonly RoutedEvent PreviewDropEvent = EventManager.RegisterRoutedEvent("PreviewDrop", RoutingStrategy.Tunnel, typeof(DragEventHandler), typeof(DragDrop));

	/// <summary>Identifies the <see cref="E:System.Windows.DragDrop.Drop" />  attached event</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DragDrop.Drop" /> attached event.</returns>
	public static readonly RoutedEvent DropEvent = EventManager.RegisterRoutedEvent("Drop", RoutingStrategy.Bubble, typeof(DragEventHandler), typeof(DragDrop));

	internal static readonly RoutedEvent DragDropStartedEvent = EventManager.RegisterRoutedEvent("DragDropStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DragDrop));

	internal static readonly RoutedEvent DragDropCompletedEvent = EventManager.RegisterRoutedEvent("DragDropCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DragDrop));

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.PreviewQueryContinueDrag" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddPreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewQueryContinueDragEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.PreviewQueryContinueDrag" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemovePreviewQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewQueryContinueDragEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
	{
		UIElement.AddHandler(element, QueryContinueDragEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemoveQueryContinueDragHandler(DependencyObject element, QueryContinueDragEventHandler handler)
	{
		UIElement.RemoveHandler(element, QueryContinueDragEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.PreviewGiveFeedback" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddPreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewGiveFeedbackEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.PreviewGiveFeedback" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemovePreviewGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewGiveFeedbackEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.GiveFeedback" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
	{
		UIElement.AddHandler(element, GiveFeedbackEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.GiveFeedback" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemoveGiveFeedbackHandler(DependencyObject element, GiveFeedbackEventHandler handler)
	{
		UIElement.RemoveHandler(element, GiveFeedbackEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.PreviewDragEnter" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddPreviewDragEnterHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewDragEnterEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.PreviewDragEnter" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemovePreviewDragEnterHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewDragEnterEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.DragEnter" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddDragEnterHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, DragEnterEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.DragEnter" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemoveDragEnterHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, DragEnterEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.PreviewDragOver" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddPreviewDragOverHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewDragOverEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.PreviewDragOver" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemovePreviewDragOverHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewDragOverEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.DragOver" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddDragOverHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, DragOverEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.DragOver" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemoveDragOverHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, DragOverEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.PreviewDragLeave" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddPreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewDragLeaveEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.PreviewDragLeave" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemovePreviewDragLeaveHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewDragLeaveEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.DragLeave" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddDragLeaveHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, DragLeaveEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.DragLeave" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemoveDragLeaveHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, DragLeaveEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.PreviewDrop" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddPreviewDropHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewDropEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.PreviewDrop" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemovePreviewDropHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewDropEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DragDrop.Drop" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be added.</param>
	public static void AddDropHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.AddHandler(element, DropEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DragDrop.Drop" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to be removed.</param>
	public static void RemoveDropHandler(DependencyObject element, DragEventHandler handler)
	{
		UIElement.RemoveHandler(element, DropEvent, handler);
	}

	/// <summary>Initiates a drag-and-drop operation.</summary>
	/// <returns>One of the <see cref="T:System.Windows.DragDropEffects" /> values that specifies the final effect that was performed during the drag-and-drop operation.</returns>
	/// <param name="dragSource">A reference to the dependency object that is the source of the data being dragged.</param>
	/// <param name="data">A data object that contains the data being dragged.</param>
	/// <param name="allowedEffects">One of the <see cref="T:System.Windows.DragDropEffects" /> values that specifies permitted effects of the drag-and-drop operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dragSource" /> or <paramref name="data" /> is null.</exception>
	public static DragDropEffects DoDragDrop(DependencyObject dragSource, object data, DragDropEffects allowedEffects)
	{
		if (dragSource == null)
		{
			throw new ArgumentNullException("dragSource");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		RoutedEventArgs e = new RoutedEventArgs(DragDropStartedEvent, dragSource);
		if (dragSource is UIElement)
		{
			((UIElement)dragSource).RaiseEvent(e);
		}
		else if (dragSource is ContentElement)
		{
			((ContentElement)dragSource).RaiseEvent(e);
		}
		else
		{
			if (!(dragSource is UIElement3D))
			{
				throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "dragSource");
			}
			((UIElement3D)dragSource).RaiseEvent(e);
		}
		DataObject dataObject = data as DataObject;
		if (dataObject == null)
		{
			dataObject = new DataObject(data);
		}
		DragDropEffects result = OleDoDragDrop(dragSource, dataObject, allowedEffects);
		e = new RoutedEventArgs(DragDropCompletedEvent, dragSource);
		if (dragSource is UIElement)
		{
			((UIElement)dragSource).RaiseEvent(e);
			return result;
		}
		if (dragSource is ContentElement)
		{
			((ContentElement)dragSource).RaiseEvent(e);
			return result;
		}
		if (dragSource is UIElement3D)
		{
			((UIElement3D)dragSource).RaiseEvent(e);
			return result;
		}
		throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "dragSource");
	}

	internal static void RegisterDropTarget(nint windowHandle)
	{
		if (windowHandle != IntPtr.Zero)
		{
			OleDropTarget dropTarget = new OleDropTarget(windowHandle);
			OleServicesContext.CurrentOleServicesContext.OleRegisterDragDrop(new HandleRef(null, windowHandle), dropTarget);
		}
	}

	internal static void RevokeDropTarget(nint windowHandle)
	{
		if (windowHandle != IntPtr.Zero)
		{
			OleServicesContext.CurrentOleServicesContext.OleRevokeDragDrop(new HandleRef(null, windowHandle));
		}
	}

	internal static bool IsValidDragDropEffects(DragDropEffects dragDropEffects)
	{
		int num = -2147483641;
		if (((uint)dragDropEffects & (uint)(~num)) != 0)
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidDragAction(DragAction dragAction)
	{
		if (dragAction == DragAction.Continue || dragAction == DragAction.Drop || dragAction == DragAction.Cancel)
		{
			return true;
		}
		return false;
	}

	internal static bool IsValidDragDropKeyStates(DragDropKeyStates dragDropKeyStates)
	{
		int num = 63;
		if (((uint)dragDropKeyStates & (uint)(~num)) != 0)
		{
			return false;
		}
		return true;
	}

	private static DragDropEffects OleDoDragDrop(DependencyObject dragSource, DataObject dataObject, DragDropEffects allowedEffects)
	{
		int[] array = new int[1];
		OleDragSource dropSource = new OleDragSource(dragSource);
		OleServicesContext.CurrentOleServicesContext.OleDoDragDrop(dataObject, dropSource, (int)allowedEffects, array);
		return (DragDropEffects)array[0];
	}
}
