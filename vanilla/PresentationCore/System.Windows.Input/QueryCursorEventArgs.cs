namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> event. </summary>
public class QueryCursorEventArgs : MouseEventArgs
{
	private Cursor _cursor;

	/// <summary>Gets or sets the cursor associated with this event. </summary>
	/// <returns>The cursor.</returns>
	public Cursor Cursor
	{
		get
		{
			return _cursor;
		}
		set
		{
			_cursor = ((value == null) ? Cursors.None : value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.QueryCursorEventArgs" /> class, by using the specified mouse device and the specified timestamp.</summary>
	/// <param name="mouse">The logical mouse device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	public QueryCursorEventArgs(MouseDevice mouse, int timestamp)
		: base(mouse, timestamp)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.QueryCursorEventArgs" /> class, by using the specified mouse device, timestamp, and stylus device.</summary>
	/// <param name="mouse">The logical mouse device associated with this event.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="stylusDevice">The stylus pointer associated with this event.</param>
	public QueryCursorEventArgs(MouseDevice mouse, int timestamp, StylusDevice stylusDevice)
		: base(mouse, timestamp, stylusDevice)
	{
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((QueryCursorEventHandler)genericHandler)(genericTarget, this);
	}
}
