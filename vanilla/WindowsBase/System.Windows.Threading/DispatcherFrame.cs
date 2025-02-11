namespace System.Windows.Threading;

/// <summary>Represents an execution loop in the <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
public class DispatcherFrame : DispatcherObject
{
	private bool _exitWhenRequested;

	private bool _continue;

	/// <summary>Gets or sets a value that indicates whether this <see cref="T:System.Windows.Threading.DispatcherFrame" /> should continue.</summary>
	/// <returns>true if the frame should continue; otherwise, false.  The default value is true.</returns>
	public bool Continue
	{
		get
		{
			bool flag = _continue;
			if (flag && _exitWhenRequested)
			{
				Dispatcher dispatcher = base.Dispatcher;
				if (dispatcher._exitAllFrames || dispatcher._hasShutdownStarted)
				{
					flag = false;
				}
			}
			return flag;
		}
		set
		{
			_continue = value;
			base.Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)((object unused) => (object)null), null);
		}
	}

	static DispatcherFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherFrame" /> class.</summary>
	public DispatcherFrame()
		: this(exitWhenRequested: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherFrame" /> class, by using the specified exit request flag.</summary>
	/// <param name="exitWhenRequested">Indicates whether this frame will exit when all frames are requested to exit.</param>
	public DispatcherFrame(bool exitWhenRequested)
	{
		_exitWhenRequested = exitWhenRequested;
		_continue = true;
	}
}
