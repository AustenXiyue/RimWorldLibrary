using System.Windows.Interop;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

internal class HwndHostAutomationPeer : FrameworkElementAutomationPeer
{
	private InteropAutomationProvider _interopProvider;

	public HwndHostAutomationPeer(HwndHost owner)
		: base(owner)
	{
		base.IsInteropPeer = true;
	}

	protected override string GetClassNameCore()
	{
		return "HwndHost";
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Pane;
	}

	internal override InteropAutomationProvider GetInteropChild()
	{
		if (_interopProvider == null)
		{
			HostedWindowWrapper wrapper = null;
			nint criticalHandle = ((HwndHost)base.Owner).CriticalHandle;
			if (criticalHandle != IntPtr.Zero)
			{
				wrapper = HostedWindowWrapper.CreateInternal(criticalHandle);
			}
			_interopProvider = new InteropAutomationProvider(wrapper, this);
		}
		return _interopProvider;
	}
}
