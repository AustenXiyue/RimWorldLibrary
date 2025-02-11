using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Input;

internal class InputProcessorProfiles
{
	private MS.Internal.SecurityCriticalDataForSet<MS.Win32.UnsafeNativeMethods.ITfInputProcessorProfiles> _ipp;

	private int _cookie;

	internal short CurrentInputLanguage
	{
		set
		{
			if (_ipp.Value == null || _ipp.Value.ChangeCurrentLanguage(value) == 0)
			{
				return;
			}
			nint[] array = null;
			int keyboardLayoutList = SafeNativeMethods.GetKeyboardLayoutList(0, null);
			if (keyboardLayoutList <= 1)
			{
				return;
			}
			array = new nint[keyboardLayoutList];
			keyboardLayoutList = SafeNativeMethods.GetKeyboardLayoutList(keyboardLayoutList, array);
			for (int i = 0; i < array.Length && i < keyboardLayoutList; i++)
			{
				if (value == (short)array[i])
				{
					SafeNativeMethods.ActivateKeyboardLayout(new HandleRef(this, array[i]), 0);
					break;
				}
			}
		}
	}

	internal ArrayList InputLanguageList
	{
		get
		{
			_ipp.Value.GetLanguageList(out var langids, out var count);
			ArrayList arrayList = new ArrayList();
			int num = Marshal.SizeOf(typeof(short));
			for (int i = 0; i < count; i++)
			{
				short culture = Marshal.PtrToStructure<short>((nint)((long)langids + (long)(num * i)));
				arrayList.Add(new CultureInfo(culture));
			}
			Marshal.FreeCoTaskMem(langids);
			return arrayList;
		}
	}

	internal InputProcessorProfiles()
	{
		_ipp.Value = null;
		_cookie = -1;
	}

	internal bool Initialize(object o)
	{
		_ipp.Value = InputProcessorProfilesLoader.Load();
		if (_ipp.Value == null)
		{
			return false;
		}
		AdviseNotifySink(o);
		return true;
	}

	internal void Uninitialize()
	{
		UnadviseNotifySink();
		Marshal.ReleaseComObject(_ipp.Value);
		_ipp.Value = null;
	}

	private void AdviseNotifySink(object o)
	{
		MS.Win32.UnsafeNativeMethods.ITfSource obj = _ipp.Value as MS.Win32.UnsafeNativeMethods.ITfSource;
		Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfLanguageProfileNotifySink;
		obj.AdviseSink(ref riid, o, out _cookie);
	}

	private void UnadviseNotifySink()
	{
		(_ipp.Value as MS.Win32.UnsafeNativeMethods.ITfSource).UnadviseSink(_cookie);
		_cookie = -1;
	}
}
