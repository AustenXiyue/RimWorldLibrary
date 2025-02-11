using System.Collections;
using System.Globalization;
using MS.Win32;

namespace System.Windows.Input;

internal sealed class InputLanguageSource : IInputLanguageSource, IDisposable
{
	private short _langid;

	private int _dispatcherThreadId;

	private InputLanguageManager _inputlanguagemanager;

	private InputProcessorProfiles _ipp;

	public CultureInfo CurrentInputLanguage
	{
		get
		{
			return new CultureInfo(_CurrentInputLanguage);
		}
		set
		{
			_CurrentInputLanguage = (short)value.LCID;
		}
	}

	public IEnumerable InputLanguageList
	{
		get
		{
			EnsureInputProcessorProfile();
			if (_ipp == null)
			{
				return new ArrayList { CurrentInputLanguage };
			}
			return _ipp.InputLanguageList;
		}
	}

	private short _CurrentInputLanguage
	{
		get
		{
			return (short)MS.Win32.NativeMethods.IntPtrToInt32(SafeNativeMethods.GetKeyboardLayout(_dispatcherThreadId));
		}
		set
		{
			EnsureInputProcessorProfile();
			if (_ipp != null)
			{
				_ipp.CurrentInputLanguage = value;
			}
		}
	}

	internal InputLanguageSource(InputLanguageManager inputlanguagemanager)
	{
		_inputlanguagemanager = inputlanguagemanager;
		_langid = (short)MS.Win32.NativeMethods.IntPtrToInt32(SafeNativeMethods.GetKeyboardLayout(0));
		_dispatcherThreadId = SafeNativeMethods.GetCurrentThreadId();
		_inputlanguagemanager.RegisterInputLanguageSource(this);
	}

	public void Dispose()
	{
		if (_ipp != null)
		{
			Uninitialize();
		}
	}

	public void Initialize()
	{
		EnsureInputProcessorProfile();
	}

	public void Uninitialize()
	{
		if (_ipp != null)
		{
			_ipp.Uninitialize();
			_ipp = null;
		}
	}

	internal bool OnLanguageChange(short langid)
	{
		if (_langid != langid && InputLanguageManager.Current.Source == this)
		{
			return InputLanguageManager.Current.ReportInputLanguageChanging(new CultureInfo(langid), new CultureInfo(_langid));
		}
		return true;
	}

	internal void OnLanguageChanged()
	{
		short currentInputLanguage = _CurrentInputLanguage;
		if (_langid != currentInputLanguage)
		{
			short langid = _langid;
			_langid = currentInputLanguage;
			if (InputLanguageManager.Current.Source == this)
			{
				InputLanguageManager.Current.ReportInputLanguageChanged(new CultureInfo(currentInputLanguage), new CultureInfo(langid));
			}
		}
	}

	private void EnsureInputProcessorProfile()
	{
		if (_ipp == null && SafeNativeMethods.GetKeyboardLayoutList(0, null) > 1)
		{
			InputLanguageProfileNotifySink o = new InputLanguageProfileNotifySink(this);
			_ipp = new InputProcessorProfiles();
			if (!_ipp.Initialize(o))
			{
				_ipp = null;
			}
		}
	}
}
