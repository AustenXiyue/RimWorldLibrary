namespace System.Windows.Automation.Peers;

internal class ItemsControlItemAutomationPeer : ItemAutomationPeer
{
	public ItemsControlItemAutomationPeer(object item, ItemsControlWrapperAutomationPeer parent)
		: base(item, parent)
	{
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.DataItem;
	}

	protected override string GetClassNameCore()
	{
		return "ItemsControlItem";
	}
}
