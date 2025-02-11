using System.Collections;
using System.Runtime.InteropServices;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal class MoveSizeWinEventHandler : WinEventHandler
{
	private ArrayList _arTextStore;

	internal int TextStoreCount => _arTextStore.Count;

	internal MoveSizeWinEventHandler()
		: base(11, 11)
	{
	}

	internal void RegisterTextStore(TextStore textstore)
	{
		if (_arTextStore == null)
		{
			_arTextStore = new ArrayList(1);
		}
		_arTextStore.Add(textstore);
	}

	internal void UnregisterTextStore(TextStore textstore)
	{
		_arTextStore.Remove(textstore);
	}

	internal override void WinEventProc(int eventId, nint hwnd)
	{
		Invariant.Assert(eventId == 11);
		if (_arTextStore == null)
		{
			return;
		}
		for (int i = 0; i < _arTextStore.Count; i++)
		{
			bool flag = false;
			TextStore textStore = (TextStore)_arTextStore[i];
			for (nint num = textStore.CriticalSourceWnd; num != IntPtr.Zero; num = MS.Win32.UnsafeNativeMethods.GetParent(new HandleRef(this, num)))
			{
				if (hwnd == num)
				{
					textStore.OnLayoutUpdated();
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				textStore.MakeLayoutChangeOnGotFocus();
			}
		}
	}
}
