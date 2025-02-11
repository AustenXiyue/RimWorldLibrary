using MS.Win32;

namespace System.Windows.Input;

internal class InputLanguageProfileNotifySink : MS.Win32.UnsafeNativeMethods.ITfLanguageProfileNotifySink
{
	private InputLanguageSource _target;

	internal InputLanguageProfileNotifySink(InputLanguageSource target)
	{
		_target = target;
	}

	public void OnLanguageChange(short langid, out bool accept)
	{
		accept = _target.OnLanguageChange(langid);
	}

	public void OnLanguageChanged()
	{
		_target.OnLanguageChanged();
	}
}
