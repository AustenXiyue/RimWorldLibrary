using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using MS.Internal.KnownBoxes;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.MenuItem" /> types to UI Automation.</summary>
public class MenuItemAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IInvokeProvider, IToggleProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The state (expanded or collapsed) of the control.</returns>
	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
	{
		get
		{
			ExpandCollapseState result = ExpandCollapseState.Collapsed;
			MenuItem menuItem = (MenuItem)base.Owner;
			MenuItemRole role = menuItem.Role;
			if (role == MenuItemRole.TopLevelItem || role == MenuItemRole.SubmenuItem || !menuItem.HasItems)
			{
				result = ExpandCollapseState.LeafNode;
			}
			else if (menuItem.IsSubmenuOpen)
			{
				result = ExpandCollapseState.Expanded;
			}
			return result;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The toggle state of the control.</returns>
	ToggleState IToggleProvider.ToggleState
	{
		get
		{
			if (!((MenuItem)base.Owner).IsChecked)
			{
				return ToggleState.Off;
			}
			return ToggleState.On;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />.</param>
	public MenuItemAutomationPeer(MenuItem owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "MenuItem".</returns>
	protected override string GetClassNameCore()
	{
		return "MenuItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.MenuItem" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.MenuItem;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />.</summary>
	/// <returns>An object that supports the control pattern if <paramref name="patternInterface" /> is a supported value; otherwise, null. </returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object result = null;
		MenuItem menuItem = (MenuItem)base.Owner;
		switch (patternInterface)
		{
		case PatternInterface.ExpandCollapse:
		{
			MenuItemRole role2 = menuItem.Role;
			if ((role2 == MenuItemRole.TopLevelHeader || role2 == MenuItemRole.SubmenuHeader) && menuItem.HasItems)
			{
				result = this;
			}
			break;
		}
		case PatternInterface.Toggle:
			if (menuItem.IsCheckable)
			{
				result = this;
			}
			break;
		case PatternInterface.Invoke:
		{
			MenuItemRole role = menuItem.Role;
			if ((role == MenuItemRole.TopLevelItem || role == MenuItemRole.SubmenuItem) && !menuItem.HasItems)
			{
				result = this;
			}
			break;
		}
		case PatternInterface.SynchronizedInput:
			result = base.GetPattern(patternInterface);
			break;
		}
		return result;
	}

	protected override int GetSizeOfSetCore()
	{
		int num = base.GetSizeOfSetCore();
		if (num == -1)
		{
			MenuItem menuItem = (MenuItem)base.Owner;
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(menuItem);
			num = ItemAutomationPeer.GetSizeOfSetFromItemsControl(itemsControl, menuItem);
			foreach (object item in (IEnumerable)itemsControl.Items)
			{
				if (item is Separator)
				{
					num--;
				}
			}
		}
		return num;
	}

	protected override int GetPositionInSetCore()
	{
		int num = base.GetPositionInSetCore();
		if (num == -1)
		{
			MenuItem menuItem = (MenuItem)base.Owner;
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(menuItem);
			num = ItemAutomationPeer.GetPositionInSetFromItemsControl(itemsControl, menuItem);
			foreach (object item in (IEnumerable)itemsControl.Items)
			{
				if (item == menuItem)
				{
					break;
				}
				if (item is Separator)
				{
					num--;
				}
			}
		}
		return num;
	}

	/// <summary>Gets the access key for the <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAccessKey" />.</summary>
	/// <returns>The access key for the <see cref="T:System.Windows.Controls.MenuItem" />.</returns>
	protected override string GetAccessKeyCore()
	{
		string text = base.GetAccessKeyCore();
		if (!string.IsNullOrEmpty(text))
		{
			MenuItemRole role = ((MenuItem)base.Owner).Role;
			if (role == MenuItemRole.TopLevelHeader || role == MenuItemRole.TopLevelItem)
			{
				text = "Alt+" + text;
			}
		}
		return text;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		if (ExpandCollapseState.Expanded == ((IExpandCollapseProvider)this).ExpandCollapseState)
		{
			ItemsControl itemsControl = (ItemsControl)base.Owner;
			ItemCollection items = itemsControl.Items;
			if (items.Count > 0)
			{
				list = new List<AutomationPeer>(items.Count);
				for (int i = 0; i < items.Count; i++)
				{
					if (itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is UIElement element)
					{
						AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(element);
						if (automationPeer == null)
						{
							automationPeer = UIElementAutomationPeer.CreatePeerForElement(element);
						}
						if (automationPeer != null)
						{
							list.Add(automationPeer);
						}
					}
				}
			}
		}
		return list;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		MenuItem menuItem = (MenuItem)base.Owner;
		MenuItemRole role = menuItem.Role;
		if ((role != MenuItemRole.TopLevelHeader && role != MenuItemRole.SubmenuHeader) || !menuItem.HasItems)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		menuItem.OpenMenu();
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		MenuItem menuItem = (MenuItem)base.Owner;
		MenuItemRole role = menuItem.Role;
		if ((role != MenuItemRole.TopLevelHeader && role != MenuItemRole.SubmenuHeader) || !menuItem.HasItems)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		menuItem.SetCurrentValueInternal(MenuItem.IsSubmenuOpenProperty, BooleanBoxes.FalseBox);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		MenuItem menuItem = (MenuItem)base.Owner;
		switch (menuItem.Role)
		{
		case MenuItemRole.TopLevelItem:
		case MenuItemRole.SubmenuItem:
			menuItem.ClickItem();
			break;
		case MenuItemRole.TopLevelHeader:
		case MenuItemRole.SubmenuHeader:
			menuItem.ClickHeader();
			break;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IToggleProvider.Toggle()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		MenuItem menuItem = (MenuItem)base.Owner;
		if (!menuItem.IsCheckable)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		menuItem.SetCurrentValueInternal(MenuItem.IsCheckedProperty, BooleanBoxes.Box(!menuItem.IsChecked));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
	{
		RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed, newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseToggleStatePropertyChangedEvent(bool oldValue, bool newValue)
	{
		RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, oldValue ? ConvertToToggleState(oldValue) : ConvertToToggleState(newValue), newValue ? ConvertToToggleState(oldValue) : ConvertToToggleState(newValue));
	}

	private static ToggleState ConvertToToggleState(bool value)
	{
		if (!value)
		{
			return ToggleState.Off;
		}
		return ToggleState.On;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.Controls.MenuItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MenuItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected override string GetNameCore()
	{
		string nameCore = base.GetNameCore();
		if (!string.IsNullOrEmpty(nameCore) && ((MenuItem)base.Owner).Header is string)
		{
			return AccessText.RemoveAccessKeyMarker(nameCore);
		}
		return nameCore;
	}
}
