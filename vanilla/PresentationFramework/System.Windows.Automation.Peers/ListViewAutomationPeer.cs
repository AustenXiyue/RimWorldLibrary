using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MS.Internal;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ListView" /> types to UI Automation.</summary>
public class ListViewAutomationPeer : ListBoxAutomationPeer
{
	private bool _refreshItemPeers;

	private IViewAutomationPeer _viewAutomationPeer;

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.IViewAutomationPeer" /> for this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />. </summary>
	/// <returns>The interface instance that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />.</returns>
	protected internal IViewAutomationPeer ViewAutomationPeer
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return _viewAutomationPeer;
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		set
		{
			if (_viewAutomationPeer != value)
			{
				_refreshItemPeers = true;
			}
			_viewAutomationPeer = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ListView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />.</param>
	public ListViewAutomationPeer(ListView owner)
		: base(owner)
	{
		Invariant.Assert(owner != null);
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ListView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.List" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		if (_viewAutomationPeer != null)
		{
			return _viewAutomationPeer.GetAutomationControlType();
		}
		return base.GetAutomationControlTypeCore();
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ListView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ListView".</returns>
	protected override string GetClassNameCore()
	{
		return "ListView";
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.ListView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the view that this <see cref="T:System.Windows.Controls.ListView" /> is using. Default <see cref="T:System.Windows.Controls.ListView" /> implementation uses the <see cref="T:System.Windows.Controls.GridView" />, and this method returns <see cref="T:System.Windows.Automation.Peers.GridViewAutomationPeer" />.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object obj = null;
		if (_viewAutomationPeer != null)
		{
			obj = _viewAutomationPeer.GetPattern(patternInterface);
			if (obj != null)
			{
				return obj;
			}
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.ListView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		if (_refreshItemPeers)
		{
			_refreshItemPeers = false;
			base.ItemPeers.Clear();
		}
		List<AutomationPeer> list = base.GetChildrenCore();
		if (_viewAutomationPeer != null)
		{
			list = _viewAutomationPeer.GetChildren(list);
		}
		return list;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> instance that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ListViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />.</param>
	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		if (_viewAutomationPeer != null)
		{
			return _viewAutomationPeer.CreateItemAutomationPeer(item);
		}
		return base.CreateItemAutomationPeer(item);
	}
}
