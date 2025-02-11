using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TabItem" /> types to UI Automation.</summary>
public class TabItemAutomationPeer : SelectorItemAutomationPeer, ISelectionItemProvider
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.TabItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />.</param>
	/// <param name="tabControlAutomationPeer">The <see cref="T:System.Windows.Automation.Peers.TabControlAutomationPeer" /> that is the parent of this <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />.</param>
	public TabItemAutomationPeer(object owner, TabControlAutomationPeer tabControlAutomationPeer)
		: base(owner, tabControlAutomationPeer)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.TabItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "TabItem".</returns>
	protected override string GetClassNameCore()
	{
		return "TabItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.TabItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.TabItem" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.TabItem;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.Controls.TabItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label. If set, this method returns the value of the <see cref="P:System.Windows.Automation.AutomationProperties.Name" /> property; otherwise this method will return the value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property.</returns>
	protected override string GetNameCore()
	{
		string nameCore = base.GetNameCore();
		if (!string.IsNullOrEmpty(nameCore) && GetWrapper() is TabItem tabItem && tabItem.Header is string)
		{
			return AccessText.RemoveAccessKeyMarker(nameCore);
		}
		return nameCore;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.TabItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		if (GetWrapper() is TabItem { IsSelected: not false } && base.ItemsControlAutomationPeer.Owner is TabControl { SelectedContentPresenter: { } selectedContentPresenter })
		{
			List<AutomationPeer> children = new FrameworkElementAutomationPeer(selectedContentPresenter).GetChildren();
			if (children != null)
			{
				if (list == null)
				{
					list = children;
				}
				else
				{
					list.AddRange(children);
				}
			}
		}
		return list;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		if (GetWrapper() is TabItem { IsSelected: not false })
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
	}

	internal override void RealizeCore()
	{
		Selector selector = (Selector)base.ItemsControlAutomationPeer.Owner;
		if (selector != null && this != null)
		{
			if (selector.CanSelectMultiple)
			{
				((ISelectionItemProvider)this).AddToSelection();
			}
			else
			{
				((ISelectionItemProvider)this).Select();
			}
		}
	}
}
