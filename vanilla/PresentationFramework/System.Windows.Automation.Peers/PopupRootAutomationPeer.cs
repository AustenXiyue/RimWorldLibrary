using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

internal class PopupRootAutomationPeer : FrameworkElementAutomationPeer
{
	public PopupRootAutomationPeer(PopupRoot owner)
		: base(owner)
	{
	}

	protected override string GetClassNameCore()
	{
		return "Popup";
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Window;
	}
}
