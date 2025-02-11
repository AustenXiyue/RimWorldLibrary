using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> event.</summary>
public class TextChangedEventArgs : RoutedEventArgs
{
	private UndoAction _undoAction;

	private readonly ICollection<TextChange> _changes;

	/// <summary>Gets how the undo stack is caused or affected by this text change </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.UndoAction" /> appropriate for this text change.</returns>
	public UndoAction UndoAction => _undoAction;

	/// <summary>Gets a collection of objects that contains information about the changes that occurred.</summary>
	/// <returns>A collection of objects that contains information about the changes that occurred.</returns>
	public ICollection<TextChange> Changes => _changes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TextChangedEventArgs" /> class, using the specified event ID, undo action, and text changes. </summary>
	/// <param name="id">The event identifier (ID).</param>
	/// <param name="action">The <see cref="P:System.Windows.Controls.TextChangedEventArgs.UndoAction" /> caused by the text change.</param>
	/// <param name="changes">The changes that occurred during this event. For more information, see <see cref="P:System.Windows.Controls.TextChangedEventArgs.Changes" />.</param>
	public TextChangedEventArgs(RoutedEvent id, UndoAction action, ICollection<TextChange> changes)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (action < UndoAction.None || action > UndoAction.Create)
		{
			throw new InvalidEnumArgumentException("action", (int)action, typeof(UndoAction));
		}
		base.RoutedEvent = id;
		_undoAction = action;
		_changes = changes;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TextChangedEventArgs" /> class, using the specified event ID and undo action.     </summary>
	/// <param name="id">The event identifier (ID).</param>
	/// <param name="action">The <see cref="P:System.Windows.Controls.TextChangedEventArgs.UndoAction" /> caused by the text change.</param>
	public TextChangedEventArgs(RoutedEvent id, UndoAction action)
		: this(id, action, new ReadOnlyCollection<TextChange>(new List<TextChange>()))
	{
	}

	/// <summary>Performs the proper type casting to call the type-safe <see cref="T:System.Windows.Controls.TextChangedEventHandler" />  delegate for the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> event.</summary>
	/// <param name="genericHandler">The handler to invoke.</param>
	/// <param name="genericTarget">The current object along the event's route.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((TextChangedEventHandler)genericHandler)(genericTarget, this);
	}
}
