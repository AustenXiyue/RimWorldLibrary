using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> types to UI Automation.</summary>
public class StatusBarAutomationPeer : FrameworkElementAutomationPeer
{
	private delegate bool IteratorCallback(AutomationPeer peer);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.StatusBarAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarAutomationPeer" />.</param>
	public StatusBarAutomationPeer(StatusBar owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "StatusBar".</returns>
	protected override string GetClassNameCore()
	{
		return "StatusBar";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.StatusBar" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.StatusBar;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>A list of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = new List<AutomationPeer>();
		if (base.Owner is ItemsControl itemsControl)
		{
			foreach (object item in (IEnumerable)itemsControl.Items)
			{
				if (item is Separator)
				{
					Separator element = item as Separator;
					list.Add(UIElementAutomationPeer.CreatePeerForElement(element));
				}
				else
				{
					if (!(itemsControl.ItemContainerGenerator.ContainerFromItem(item) is StatusBarItem statusBarItem))
					{
						continue;
					}
					if (item is string || item is TextBlock || (item is StatusBarItem && ((StatusBarItem)item).Content is string))
					{
						list.Add(UIElementAutomationPeer.CreatePeerForElement(statusBarItem));
						continue;
					}
					List<AutomationPeer> childrenAutomationPeer = GetChildrenAutomationPeer(statusBarItem);
					if (childrenAutomationPeer == null)
					{
						continue;
					}
					foreach (AutomationPeer item2 in childrenAutomationPeer)
					{
						list.Add(item2);
					}
				}
			}
		}
		return list;
	}

	private List<AutomationPeer> GetChildrenAutomationPeer(Visual parent)
	{
		Invariant.Assert(parent != null);
		List<AutomationPeer> children = null;
		iterate(parent, delegate(AutomationPeer peer)
		{
			if (children == null)
			{
				children = new List<AutomationPeer>();
			}
			children.Add(peer);
			return false;
		});
		return children;
	}

	private static bool iterate(Visual parent, IteratorCallback callback)
	{
		bool flag = false;
		AutomationPeer automationPeer = null;
		int internalVisualChildrenCount = parent.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			if (flag)
			{
				break;
			}
			Visual visual = parent.InternalGetVisualChild(i);
			flag = ((visual == null || !visual.CheckFlagsAnd(VisualFlags.IsUIElement) || (automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)visual)) == null) ? iterate(visual, callback) : callback(automationPeer));
		}
		return flag;
	}
}
