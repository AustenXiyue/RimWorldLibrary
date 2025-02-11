namespace System.Windows;

/// <summary>Contains arguments for the <see cref="E:System.Windows.DragDrop.GiveFeedback" /> event.</summary>
public sealed class GiveFeedbackEventArgs : RoutedEventArgs
{
	private DragDropEffects _effects;

	private bool _useDefaultCursors;

	/// <summary>Gets a value that indicates the effects of drag-and-drop operation.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.DragDropEffects" /> enumeration that indicates the effects of the drag-and-drop operation.</returns>
	public DragDropEffects Effects => _effects;

	/// <summary>Gets or sets a Boolean value indicating whether default cursor feedback behavior should be used for the associated drag-and-drop operation.</summary>
	/// <returns>A Boolean value that indicating whether default cursor feedback behavior should be used for the associated drag-and-drop operation. true to use default feedback cursor behavior; otherwise, false.</returns>
	public bool UseDefaultCursors
	{
		get
		{
			return _useDefaultCursors;
		}
		set
		{
			_useDefaultCursors = value;
		}
	}

	internal GiveFeedbackEventArgs(DragDropEffects effects, bool useDefaultCursors)
	{
		DragDrop.IsValidDragDropEffects(effects);
		_effects = effects;
		_useDefaultCursors = useDefaultCursors;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((GiveFeedbackEventHandler)genericHandler)(genericTarget, this);
	}
}
