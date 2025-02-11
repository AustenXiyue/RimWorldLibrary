using System.Runtime.InteropServices.ComTypes;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows;

internal class OleDropTarget : DispatcherObject, MS.Win32.UnsafeNativeMethods.IOleDropTarget
{
	private nint _windowHandle;

	private IDataObject _dataObject;

	private DependencyObject _lastTarget;

	public OleDropTarget(nint handle)
	{
		if (handle == IntPtr.Zero)
		{
			throw new ArgumentNullException("handle");
		}
		_windowHandle = handle;
	}

	int MS.Win32.UnsafeNativeMethods.IOleDropTarget.OleDragEnter(object data, int dragDropKeyStates, long point, ref int effects)
	{
		_dataObject = GetDataObject(data);
		if (_dataObject == null || !IsDataAvailable(_dataObject))
		{
			effects = 0;
			return 1;
		}
		Point targetPoint;
		DependencyObject dependencyObject = (_lastTarget = GetCurrentTarget(point, out targetPoint));
		if (dependencyObject != null)
		{
			RaiseDragEvent(DragDrop.DragEnterEvent, dragDropKeyStates, ref effects, dependencyObject, targetPoint);
		}
		else
		{
			effects = 0;
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleDropTarget.OleDragOver(int dragDropKeyStates, long point, ref int effects)
	{
		Invariant.Assert(_dataObject != null);
		Point targetPoint;
		DependencyObject currentTarget = GetCurrentTarget(point, out targetPoint);
		if (currentTarget != null)
		{
			if (currentTarget != _lastTarget)
			{
				try
				{
					if (_lastTarget != null)
					{
						RaiseDragEvent(DragDrop.DragLeaveEvent, dragDropKeyStates, ref effects, _lastTarget, targetPoint);
					}
					RaiseDragEvent(DragDrop.DragEnterEvent, dragDropKeyStates, ref effects, currentTarget, targetPoint);
				}
				finally
				{
					_lastTarget = currentTarget;
				}
			}
			else
			{
				RaiseDragEvent(DragDrop.DragOverEvent, dragDropKeyStates, ref effects, currentTarget, targetPoint);
			}
		}
		else
		{
			try
			{
				if (_lastTarget != null)
				{
					RaiseDragEvent(DragDrop.DragLeaveEvent, dragDropKeyStates, ref effects, _lastTarget, targetPoint);
				}
			}
			finally
			{
				_lastTarget = currentTarget;
				effects = 0;
			}
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleDropTarget.OleDragLeave()
	{
		if (_lastTarget != null)
		{
			int effects = 0;
			try
			{
				RaiseDragEvent(DragDrop.DragLeaveEvent, 0, ref effects, _lastTarget, new Point(0.0, 0.0));
			}
			finally
			{
				_lastTarget = null;
				_dataObject = null;
			}
		}
		return 0;
	}

	int MS.Win32.UnsafeNativeMethods.IOleDropTarget.OleDrop(object data, int dragDropKeyStates, long point, ref int effects)
	{
		IDataObject dataObject = GetDataObject(data);
		if (dataObject == null || !IsDataAvailable(dataObject))
		{
			effects = 0;
			return 1;
		}
		_lastTarget = null;
		Point targetPoint;
		DependencyObject currentTarget = GetCurrentTarget(point, out targetPoint);
		if (currentTarget != null)
		{
			RaiseDragEvent(DragDrop.DropEvent, dragDropKeyStates, ref effects, currentTarget, targetPoint);
		}
		else
		{
			effects = 0;
		}
		return 0;
	}

	private void RaiseDragEvent(RoutedEvent dragEvent, int dragDropKeyStates, ref int effects, DependencyObject target, Point targetPoint)
	{
		Invariant.Assert(_dataObject != null);
		Invariant.Assert(target != null);
		DragEventArgs dragEventArgs = new DragEventArgs(_dataObject, (DragDropKeyStates)dragDropKeyStates, (DragDropEffects)effects, target, targetPoint);
		if (dragEvent == DragDrop.DragEnterEvent)
		{
			dragEventArgs.RoutedEvent = DragDrop.PreviewDragEnterEvent;
		}
		else if (dragEvent == DragDrop.DragOverEvent)
		{
			dragEventArgs.RoutedEvent = DragDrop.PreviewDragOverEvent;
		}
		else if (dragEvent == DragDrop.DragLeaveEvent)
		{
			dragEventArgs.RoutedEvent = DragDrop.PreviewDragLeaveEvent;
		}
		else if (dragEvent == DragDrop.DropEvent)
		{
			dragEventArgs.RoutedEvent = DragDrop.PreviewDropEvent;
		}
		if (target is UIElement)
		{
			((UIElement)target).RaiseEvent(dragEventArgs);
		}
		else if (target is ContentElement)
		{
			((ContentElement)target).RaiseEvent(dragEventArgs);
		}
		else
		{
			if (!(target is UIElement3D))
			{
				throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "scope");
			}
			((UIElement3D)target).RaiseEvent(dragEventArgs);
		}
		if (!dragEventArgs.Handled)
		{
			dragEventArgs.RoutedEvent = dragEvent;
			if (target is UIElement)
			{
				((UIElement)target).RaiseEvent(dragEventArgs);
			}
			else if (target is ContentElement)
			{
				((ContentElement)target).RaiseEvent(dragEventArgs);
			}
			else
			{
				if (!(target is UIElement3D))
				{
					throw new ArgumentException(SR.ScopeMustBeUIElementOrContent, "scope");
				}
				((UIElement3D)target).RaiseEvent(dragEventArgs);
			}
		}
		if (!dragEventArgs.Handled)
		{
			if (dragEvent == DragDrop.DragEnterEvent)
			{
				OnDefaultDragEnter(dragEventArgs);
			}
			else if (dragEvent == DragDrop.DragOverEvent)
			{
				OnDefaultDragOver(dragEventArgs);
			}
		}
		effects = (int)dragEventArgs.Effects;
	}

	private void OnDefaultDragEnter(DragEventArgs e)
	{
		if (e.Data == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if ((e.AllowedEffects & DragDropEffects.Move) != 0)
		{
			e.Effects = DragDropEffects.Move;
		}
		if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
		{
			e.Effects = DragDropEffects.Copy;
		}
	}

	private void OnDefaultDragOver(DragEventArgs e)
	{
		if (e.Data == null)
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		if ((e.AllowedEffects & DragDropEffects.Move) != 0)
		{
			e.Effects = DragDropEffects.Move;
		}
		if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
		{
			e.Effects = DragDropEffects.Copy;
		}
	}

	private Point GetClientPointFromScreenPoint(long dragPoint, PresentationSource source)
	{
		return PointUtil.ScreenToClient(new Point((int)(dragPoint & 0xFFFFFFFFu), (int)((dragPoint >> 32) & 0xFFFFFFFFu)), source);
	}

	private DependencyObject GetCurrentTarget(long dragPoint, out Point targetPoint)
	{
		DependencyObject dependencyObject = null;
		HwndSource hwndSource = HwndSource.FromHwnd(_windowHandle);
		targetPoint = GetClientPointFromScreenPoint(dragPoint, hwndSource);
		if (hwndSource != null)
		{
			dependencyObject = MouseDevice.LocalHitTest(targetPoint, hwndSource) as DependencyObject;
			if (dependencyObject is UIElement uIElement)
			{
				dependencyObject = ((!uIElement.AllowDrop) ? null : uIElement);
			}
			else if (dependencyObject is ContentElement contentElement)
			{
				dependencyObject = ((!contentElement.AllowDrop) ? null : contentElement);
			}
			else if (dependencyObject is UIElement3D uIElement3D)
			{
				dependencyObject = ((!uIElement3D.AllowDrop) ? null : uIElement3D);
			}
			if (dependencyObject != null)
			{
				targetPoint = PointUtil.ClientToRoot(targetPoint, hwndSource);
				targetPoint = InputElement.TranslatePoint(targetPoint, hwndSource.RootVisual, dependencyObject);
			}
		}
		return dependencyObject;
	}

	private IDataObject GetDataObject(object data)
	{
		IDataObject result = null;
		if (data != null)
		{
			result = ((!(data is DataObject)) ? new DataObject((System.Runtime.InteropServices.ComTypes.IDataObject)data) : ((DataObject)data));
		}
		return result;
	}

	private bool IsDataAvailable(IDataObject dataObject)
	{
		bool result = false;
		if (dataObject != null)
		{
			string[] formats = dataObject.GetFormats();
			for (int i = 0; i < formats.Length; i++)
			{
				if (dataObject.GetDataPresent(formats[i]))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
