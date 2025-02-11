using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

internal class ItemsControlWrapperAutomationPeer : ItemsControlAutomationPeer
{
	public ItemsControlWrapperAutomationPeer(ItemsControl owner)
		: base(owner)
	{
	}

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new ItemsControlItemAutomationPeer(item, this);
	}

	protected override string GetClassNameCore()
	{
		return "ItemsControl";
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.List;
	}
}
