namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Interop.HwndHost" /> types to UI Automation.</summary>
public sealed class HostedWindowWrapper
{
	private nint _hwnd;

	internal nint Handle => _hwnd;

	public HostedWindowWrapper(nint hwnd)
	{
		_hwnd = hwnd;
	}

	private HostedWindowWrapper()
	{
		_hwnd = IntPtr.Zero;
	}

	internal static HostedWindowWrapper CreateInternal(nint hwnd)
	{
		return new HostedWindowWrapper
		{
			_hwnd = hwnd
		};
	}
}
