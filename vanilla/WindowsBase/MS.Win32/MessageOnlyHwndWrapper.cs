namespace MS.Win32;

internal class MessageOnlyHwndWrapper : HwndWrapper
{
	public MessageOnlyHwndWrapper()
		: base(0, 0, 0, 0, 0, 0, 0, "", NativeMethods.HWND_MESSAGE, null)
	{
	}
}
