using System.Windows.Input;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Contains arguments relevant to all drag-and-drop events (<see cref="E:System.Windows.DragDrop.DragEnter" />, <see cref="E:System.Windows.DragDrop.DragLeave" />, <see cref="E:System.Windows.DragDrop.DragOver" />, and <see cref="E:System.Windows.DragDrop.Drop" />).</summary>
public sealed class DragEventArgs : RoutedEventArgs
{
	private IDataObject _data;

	private DragDropKeyStates _dragDropKeyStates;

	private DragDropEffects _allowedEffects;

	private DragDropEffects _effects;

	private DependencyObject _target;

	private Point _dropPoint;

	/// <summary>Gets a data object that contains the data associated with the corresponding drag event.</summary>
	/// <returns>A data object that contains the data associated with the corresponding drag event..</returns>
	public IDataObject Data => _data;

	/// <summary>Gets a flag enumeration indicating the current state of the SHIFT, CTRL, and ALT keys, as well as the state of the mouse buttons.</summary>
	/// <returns>One or more members of the <see cref="T:System.Windows.DragDropKeyStates" /> flag enumeration.</returns>
	public DragDropKeyStates KeyStates => _dragDropKeyStates;

	/// <summary>Gets a member of the <see cref="T:System.Windows.DragDropEffects" /> enumeration that specifies which operations are allowed by the originator of the drag event.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.DragDropEffects" /> enumeration that specifies which operations are allowed by the originator of the drag event.</returns>
	public DragDropEffects AllowedEffects => _allowedEffects;

	/// <summary>Gets or sets the target drag-and-drop operation.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.DragDropEffects" /> enumeration specifying the target drag-and-drop operation.</returns>
	public DragDropEffects Effects
	{
		get
		{
			return _effects;
		}
		set
		{
			if (!DragDrop.IsValidDragDropEffects(value))
			{
				throw new ArgumentException(SR.Format(SR.DragDrop_DragDropEffectsInvalid, "value"));
			}
			_effects = value;
		}
	}

	internal DragEventArgs(IDataObject data, DragDropKeyStates dragDropKeyStates, DragDropEffects allowedEffects, DependencyObject target, Point point)
	{
		DragDrop.IsValidDragDropKeyStates(dragDropKeyStates);
		DragDrop.IsValidDragDropEffects(allowedEffects);
		_data = data;
		_dragDropKeyStates = dragDropKeyStates;
		_allowedEffects = allowedEffects;
		_target = target;
		_dropPoint = point;
		_effects = allowedEffects;
	}

	/// <summary>Returns a drop point that is relative to a specified <see cref="T:System.Windows.IInputElement" />.</summary>
	/// <returns>A drop point that is relative to the element specified in <paramref name="relativeTo" />.</returns>
	/// <param name="relativeTo">An <see cref="T:System.Windows.IInputElement" /> object for which to get a relative drop point.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="relativeTo" /> is null.</exception>
	public Point GetPosition(IInputElement relativeTo)
	{
		if (relativeTo == null)
		{
			throw new ArgumentNullException("relativeTo");
		}
		Point result = new Point(0.0, 0.0);
		if (_target != null)
		{
			return InputElement.TranslatePoint(_dropPoint, _target, (DependencyObject)relativeTo);
		}
		return result;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DragEventHandler)genericHandler)(genericTarget, this);
	}
}
