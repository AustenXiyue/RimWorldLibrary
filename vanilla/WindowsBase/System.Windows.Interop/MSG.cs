using MS.Internal.WindowsBase;

namespace System.Windows.Interop;

/// <summary>Contains message information from a thread's message queue. </summary>
[Serializable]
public struct MSG
{
	private nint _hwnd;

	private int _message;

	private nint _wParam;

	private nint _lParam;

	private int _time;

	private int _pt_x;

	private int _pt_y;

	/// <summary>Gets or sets the window handle (HWND) to the window whose window procedure receives the message. </summary>
	/// <returns>The window handle (HWND).</returns>
	public nint hwnd
	{
		get
		{
			return _hwnd;
		}
		set
		{
			_hwnd = value;
		}
	}

	/// <summary>Gets or sets the message identifier. </summary>
	/// <returns>The message identifier.</returns>
	public int message
	{
		get
		{
			return _message;
		}
		set
		{
			_message = value;
		}
	}

	/// <summary>Gets or sets the <paramref name="wParam" /> value for the message, which specifies additional information about the message. The exact meaning depends on the value of the message. </summary>
	/// <returns>The <paramref name="wParam" /> value for the message.</returns>
	public nint wParam
	{
		get
		{
			return _wParam;
		}
		set
		{
			_wParam = value;
		}
	}

	/// <summary>Gets or sets the <paramref name="lParam" /> value that specifies additional information about the message. The exact meaning depends on the value of the <see cref="P:System.Windows.Interop.MSG.message" /> member.</summary>
	/// <returns>The <paramref name="lParam" /> value for the message.</returns>
	public nint lParam
	{
		get
		{
			return _lParam;
		}
		set
		{
			_lParam = value;
		}
	}

	/// <summary>Gets or sets the time at which the message was posted.</summary>
	/// <returns>The time that the message was posted.</returns>
	public int time
	{
		get
		{
			return _time;
		}
		set
		{
			_time = value;
		}
	}

	/// <summary>Gets or sets the x coordinate of the cursor position on the screen, when the message was posted. </summary>
	/// <returns>The x coordinate of the cursor position.</returns>
	public int pt_x
	{
		get
		{
			return _pt_x;
		}
		set
		{
			_pt_x = value;
		}
	}

	/// <summary>Gets or sets the y coordinate of the cursor position on the screen, when the message was posted. </summary>
	/// <returns>The y coordinate of the cursor position.</returns>
	public int pt_y
	{
		get
		{
			return _pt_y;
		}
		set
		{
			_pt_y = value;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal MSG(nint hwnd, int message, nint wParam, nint lParam, int time, int pt_x, int pt_y)
	{
		_hwnd = hwnd;
		_message = message;
		_wParam = wParam;
		_lParam = lParam;
		_time = time;
		_pt_x = pt_x;
		_pt_y = pt_y;
	}
}
