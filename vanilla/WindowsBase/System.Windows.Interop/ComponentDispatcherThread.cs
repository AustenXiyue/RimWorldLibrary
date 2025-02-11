namespace System.Windows.Interop;

internal class ComponentDispatcherThread
{
	private int _modalCount;

	private MSG _currentKeyboardMSG;

	public bool IsThreadModal => _modalCount > 0;

	public MSG CurrentKeyboardMessage
	{
		get
		{
			return _currentKeyboardMSG;
		}
		set
		{
			_currentKeyboardMSG = value;
		}
	}

	public event EventHandler ThreadIdle
	{
		add
		{
			_threadIdle += value;
		}
		remove
		{
			_threadIdle -= value;
		}
	}

	private event EventHandler _threadIdle;

	public event ThreadMessageEventHandler ThreadFilterMessage
	{
		add
		{
			_threadFilterMessage += value;
		}
		remove
		{
			_threadFilterMessage -= value;
		}
	}

	private event ThreadMessageEventHandler _threadFilterMessage;

	public event ThreadMessageEventHandler ThreadPreprocessMessage
	{
		add
		{
			_threadPreprocessMessage += value;
		}
		remove
		{
			_threadPreprocessMessage -= value;
		}
	}

	private event ThreadMessageEventHandler _threadPreprocessMessage;

	public event EventHandler EnterThreadModal
	{
		add
		{
			_enterThreadModal += value;
		}
		remove
		{
			_enterThreadModal -= value;
		}
	}

	private event EventHandler _enterThreadModal;

	public event EventHandler LeaveThreadModal
	{
		add
		{
			_leaveThreadModal += value;
		}
		remove
		{
			_leaveThreadModal -= value;
		}
	}

	private event EventHandler _leaveThreadModal;

	public void PushModal()
	{
		_modalCount++;
		if (1 == _modalCount && this._enterThreadModal != null)
		{
			this._enterThreadModal(null, EventArgs.Empty);
		}
	}

	public void PopModal()
	{
		_modalCount--;
		if (_modalCount == 0 && this._leaveThreadModal != null)
		{
			this._leaveThreadModal(null, EventArgs.Empty);
		}
		if (_modalCount < 0)
		{
			_modalCount = 0;
		}
	}

	public void RaiseIdle()
	{
		if (this._threadIdle != null)
		{
			this._threadIdle(null, EventArgs.Empty);
		}
	}

	public bool RaiseThreadMessage(ref MSG msg)
	{
		bool handled = false;
		if (this._threadFilterMessage != null)
		{
			this._threadFilterMessage(ref msg, ref handled);
		}
		if (handled)
		{
			return handled;
		}
		if (this._threadPreprocessMessage != null)
		{
			this._threadPreprocessMessage(ref msg, ref handled);
		}
		return handled;
	}

	public void AddThreadPreprocessMessageHandlerFirst(ThreadMessageEventHandler handler)
	{
		this._threadPreprocessMessage = (ThreadMessageEventHandler)Delegate.Combine(handler, this._threadPreprocessMessage);
	}

	public void RemoveThreadPreprocessMessageHandlerFirst(ThreadMessageEventHandler handler)
	{
		if (this._threadPreprocessMessage == null)
		{
			return;
		}
		ThreadMessageEventHandler threadMessageEventHandler = null;
		Delegate[] invocationList = this._threadPreprocessMessage.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			ThreadMessageEventHandler threadMessageEventHandler2 = (ThreadMessageEventHandler)invocationList[i];
			if (threadMessageEventHandler2 == handler)
			{
				handler = null;
			}
			else
			{
				threadMessageEventHandler = (ThreadMessageEventHandler)Delegate.Combine(threadMessageEventHandler, threadMessageEventHandler2);
			}
		}
		this._threadPreprocessMessage = threadMessageEventHandler;
	}
}
