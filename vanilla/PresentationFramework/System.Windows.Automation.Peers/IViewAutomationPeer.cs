using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Windows.Automation.Peers;

/// <summary>Allows a customized view of a <see cref="T:System.Windows.Controls.ListView" /> that derives from <see cref="T:System.Windows.Controls.ViewBase" /> to implement automation peer features that are specific to the custom view.</summary>
public interface IViewAutomationPeer
{
	/// <summary>Gets the control type for the element that is associated with this <see cref="T:System.Windows.Automation.Peers.IViewAutomationPeer" />.</summary>
	/// <returns>A value in the <see cref="T:System.Windows.Automation.Peers.AutomationControlType" /> enumeration.</returns>
	AutomationControlType GetAutomationControlType();

	/// <summary>Gets the control pattern that is associated with the specified <paramref name="patternInterface" />.</summary>
	/// <returns>Return the object that implements the control pattern. If this method returns null, the return value from <see cref="M:System.Windows.Automation.Peers.IViewAutomationPeer.GetPattern(System.Windows.Automation.Peers.PatternInterface)" /> is used.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	object GetPattern(PatternInterface patternInterface);

	/// <summary>Gets the collection of immediate child elements of the specified UI Automation peer.</summary>
	/// <returns>The automation peers for all items in the control. If the view contains interactive or informational elements in addition to the list items, automation peers for these elements must be added to the list.</returns>
	/// <param name="children">The automation peers for the list items.</param>
	List<AutomationPeer> GetChildren(List<AutomationPeer> children);

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</summary>
	/// <returns>The new <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> instance.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ListViewItem" /> that is associated with the <see cref="T:System.Windows.Controls.ListView" /> that is used by this <see cref="T:System.Windows.Automation.Peers.IViewAutomationPeer" />. </param>
	ItemAutomationPeer CreateItemAutomationPeer(object item);

	/// <summary>Called by <see cref="T:System.Windows.Controls.ListView" /> when the collection of items changes.</summary>
	/// <param name="e">A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> that contains the event data.</param>
	void ItemsChanged(NotifyCollectionChangedEventArgs e);

	/// <summary>Called when the custom view is no longer applied to the <see cref="T:System.Windows.Controls.ListView" />.</summary>
	void ViewDetached();
}
