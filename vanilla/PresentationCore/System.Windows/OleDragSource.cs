using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows;

internal class OleDragSource : MS.Win32.UnsafeNativeMethods.IOleDropSource
{
	private DependencyObject _dragSource;

	public OleDragSource(DependencyObject dragSource)
	{
		_dragSource = dragSource;
	}

	int MS.Win32.UnsafeNativeMethods.IOleDropSource.OleQueryContinueDrag(int escapeKey, int grfkeyState)
	{
		bool escapePressed = false;
		if (escapeKey != 0)
		{
			escapePressed = true;
		}
		QueryContinueDragEventArgs queryContinueDragEventArgs = new QueryContinueDragEventArgs(escapePressed, (DragDropKeyStates)grfkeyState);
		RaiseQueryContinueDragEvent(queryContinueDragEventArgs);
		if (queryContinueDragEventArgs.Action == DragAction.Continue)
		{
			return 0;
		}
		if (queryContinueDragEventArgs.Action == DragAction.Drop)
		{
			return 262400;
		}
		if (queryContinueDragEventArgs.Action == DragAction.Cancel)
		{
			return 262401;
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleDropSource.OleGiveFeedback(int effect)
	{
		GiveFeedbackEventArgs giveFeedbackEventArgs = new GiveFeedbackEventArgs((DragDropEffects)effect, useDefaultCursors: false);
		RaiseGiveFeedbackEvent(giveFeedbackEventArgs);
		if (giveFeedbackEventArgs.UseDefaultCursors)
		{
			return 262402;
		}
		return 0;
	}

	private void RaiseQueryContinueDragEvent(QueryContinueDragEventArgs args)
	{
		args.RoutedEvent = DragDrop.PreviewQueryContinueDragEvent;
		if (_dragSource is UIElement)
		{
			((UIElement)_dragSource).RaiseEvent(args);
		}
		else if (_dragSource is ContentElement)
		{
			((ContentElement)_dragSource).RaiseEvent(args);
		}
		else
		{
			if (!(_dragSource is UIElement3D))
			{
				throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "scope");
			}
			((UIElement3D)_dragSource).RaiseEvent(args);
		}
		args.RoutedEvent = DragDrop.QueryContinueDragEvent;
		if (!args.Handled)
		{
			if (_dragSource is UIElement)
			{
				((UIElement)_dragSource).RaiseEvent(args);
			}
			else if (_dragSource is ContentElement)
			{
				((ContentElement)_dragSource).RaiseEvent(args);
			}
			else
			{
				if (!(_dragSource is UIElement3D))
				{
					throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "scope");
				}
				((UIElement3D)_dragSource).RaiseEvent(args);
			}
		}
		if (!args.Handled)
		{
			OnDefaultQueryContinueDrag(args);
		}
	}

	private void RaiseGiveFeedbackEvent(GiveFeedbackEventArgs args)
	{
		args.RoutedEvent = DragDrop.PreviewGiveFeedbackEvent;
		if (_dragSource is UIElement)
		{
			((UIElement)_dragSource).RaiseEvent(args);
		}
		else if (_dragSource is ContentElement)
		{
			((ContentElement)_dragSource).RaiseEvent(args);
		}
		else
		{
			if (!(_dragSource is UIElement3D))
			{
				throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "scope");
			}
			((UIElement3D)_dragSource).RaiseEvent(args);
		}
		args.RoutedEvent = DragDrop.GiveFeedbackEvent;
		if (!args.Handled)
		{
			if (_dragSource is UIElement)
			{
				((UIElement)_dragSource).RaiseEvent(args);
			}
			else if (_dragSource is ContentElement)
			{
				((ContentElement)_dragSource).RaiseEvent(args);
			}
			else
			{
				if (!(_dragSource is UIElement3D))
				{
					throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "scope");
				}
				((UIElement3D)_dragSource).RaiseEvent(args);
			}
		}
		if (!args.Handled)
		{
			OnDefaultGiveFeedback(args);
		}
	}

	private void OnDefaultQueryContinueDrag(QueryContinueDragEventArgs e)
	{
		int num = 0;
		if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) == DragDropKeyStates.LeftMouseButton)
		{
			num++;
		}
		if ((e.KeyStates & DragDropKeyStates.MiddleMouseButton) == DragDropKeyStates.MiddleMouseButton)
		{
			num++;
		}
		if ((e.KeyStates & DragDropKeyStates.RightMouseButton) == DragDropKeyStates.RightMouseButton)
		{
			num++;
		}
		e.Action = DragAction.Continue;
		if (e.EscapePressed || num >= 2)
		{
			e.Action = DragAction.Cancel;
		}
		else if (num == 0)
		{
			e.Action = DragAction.Drop;
		}
	}

	private void OnDefaultGiveFeedback(GiveFeedbackEventArgs e)
	{
		e.UseDefaultCursors = true;
	}
}
