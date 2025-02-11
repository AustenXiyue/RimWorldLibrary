using System.Threading;
using MS.Internal.WindowsBase;

namespace System.Windows.Interop;

/// <summary>Enables shared control of the message pump between Win32 and WPF in interoperation scenarios. </summary>
public static class ComponentDispatcher
{
	private static LocalDataStoreSlot _threadSlot;

	private static ComponentDispatcherThread CurrentThreadData
	{
		get
		{
			object data = Thread.GetData(_threadSlot);
			ComponentDispatcherThread componentDispatcherThread;
			if (data == null)
			{
				componentDispatcherThread = new ComponentDispatcherThread();
				Thread.SetData(_threadSlot, componentDispatcherThread);
			}
			else
			{
				componentDispatcherThread = (ComponentDispatcherThread)data;
			}
			return componentDispatcherThread;
		}
	}

	/// <summary>Gets a value that indicates whether the thread is modal. </summary>
	/// <returns>true if the thread is modal; otherwise, false.</returns>
	public static bool IsThreadModal => CurrentThreadData.IsThreadModal;

	/// <summary>Gets the last message that was raised. </summary>
	/// <returns>The last message.</returns>
	public static MSG CurrentKeyboardMessage => CurrentThreadData.CurrentKeyboardMessage;

	internal static MSG UnsecureCurrentKeyboardMessage
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return CurrentThreadData.CurrentKeyboardMessage;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			CurrentThreadData.CurrentKeyboardMessage = value;
		}
	}

	/// <summary>Occurs when the thread is idle. </summary>
	public static event EventHandler ThreadIdle
	{
		add
		{
			CurrentThreadData.ThreadIdle += value;
		}
		remove
		{
			CurrentThreadData.ThreadIdle -= value;
		}
	}

	/// <summary>Occurs when the message pump receives a keyboard message.  </summary>
	public static event ThreadMessageEventHandler ThreadFilterMessage
	{
		add
		{
			CurrentThreadData.ThreadFilterMessage += value;
		}
		remove
		{
			CurrentThreadData.ThreadFilterMessage -= value;
		}
	}

	/// <summary>Occurs when the message pump receives a keyboard message.</summary>
	public static event ThreadMessageEventHandler ThreadPreprocessMessage
	{
		add
		{
			CurrentThreadData.ThreadPreprocessMessage += value;
		}
		remove
		{
			CurrentThreadData.ThreadPreprocessMessage -= value;
		}
	}

	/// <summary>Occurs when a thread becomes modal. </summary>
	public static event EventHandler EnterThreadModal
	{
		add
		{
			CurrentThreadData.EnterThreadModal += value;
		}
		remove
		{
			CurrentThreadData.EnterThreadModal -= value;
		}
	}

	/// <summary>Occurs when a modal thread becomes nonmodal. </summary>
	public static event EventHandler LeaveThreadModal
	{
		add
		{
			CurrentThreadData.LeaveThreadModal += value;
		}
		remove
		{
			CurrentThreadData.LeaveThreadModal -= value;
		}
	}

	static ComponentDispatcher()
	{
		_threadSlot = Thread.AllocateDataSlot();
	}

	/// <summary>Called to indicate that the thread is modal. </summary>
	public static void PushModal()
	{
		CriticalPushModal();
	}

	internal static void CriticalPushModal()
	{
		CurrentThreadData.PushModal();
	}

	/// <summary>Called to indicate that a modal thread is no longer modal.</summary>
	public static void PopModal()
	{
		CriticalPopModal();
	}

	internal static void CriticalPopModal()
	{
		CurrentThreadData.PopModal();
	}

	/// <summary>Called to indicate that a thread is idle. </summary>
	public static void RaiseIdle()
	{
		CurrentThreadData.RaiseIdle();
	}

	/// <summary>Indicates that a new message is available for possible handling. </summary>
	/// <returns>true, if one of the modules listening to the message loop processed the message. The owner of the message loop should ignore the message. false, if the message was not processed. In this case, the owner of the message pump should call the Win32 function TranslateMessage followed by DispatchMessage. </returns>
	/// <param name="msg">The message and its associated data.</param>
	public static bool RaiseThreadMessage(ref MSG msg)
	{
		return CurrentThreadData.RaiseThreadMessage(ref msg);
	}

	internal static void CriticalAddThreadPreprocessMessageHandlerFirst(ThreadMessageEventHandler handler)
	{
		CurrentThreadData.AddThreadPreprocessMessageHandlerFirst(handler);
	}

	internal static void CriticalRemoveThreadPreprocessMessageHandlerFirst(ThreadMessageEventHandler handler)
	{
		CurrentThreadData.RemoveThreadPreprocessMessageHandlerFirst(handler);
	}
}
