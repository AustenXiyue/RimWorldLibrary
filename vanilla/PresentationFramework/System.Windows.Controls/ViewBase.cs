using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

/// <summary>Represents the base class for views that define the appearance of data in a <see cref="T:System.Windows.Controls.ListView" /> control.</summary>
public abstract class ViewBase : DependencyObject
{
	private DependencyObject _inheritanceContext;

	private bool _isUsed;

	/// <summary>Gets the object that is associated with the style for the view mode.</summary>
	/// <returns>The style to use for the view mode. The default value is the style for the <see cref="T:System.Windows.Controls.ListBox" />.</returns>
	protected internal virtual object DefaultStyleKey => typeof(ListBox);

	/// <summary>Gets the style to use for the items in the view mode.</summary>
	/// <returns>The style of a <see cref="T:System.Windows.Controls.ListViewItem" />. The default value is the style for the <see cref="T:System.Windows.Controls.ListBoxItem" /> control.</returns>
	protected internal virtual object ItemContainerDefaultStyleKey => typeof(ListBoxItem);

	internal override DependencyObject InheritanceContext => _inheritanceContext;

	internal bool IsUsed
	{
		get
		{
			return _isUsed;
		}
		set
		{
			_isUsed = value;
		}
	}

	/// <summary>Prepares an item in the view for display, by setting bindings and styles.</summary>
	/// <param name="item">The item to prepare for display.</param>
	protected internal virtual void PrepareItem(ListViewItem item)
	{
	}

	/// <summary>Removes all bindings and styling that are set for an item.</summary>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ListViewItem" /> to remove settings from.</param>
	protected internal virtual void ClearItem(ListViewItem item)
	{
	}

	internal virtual void OnThemeChanged()
	{
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (_inheritanceContext != context)
		{
			_inheritanceContext = context;
			OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (_inheritanceContext == context)
		{
			_inheritanceContext = null;
			OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	/// <summary>Is called when a <see cref="T:System.Windows.Controls.ListView" /> control creates a <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" /> for its <see cref="P:System.Windows.Controls.ListView.View" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.IViewAutomationPeer" /> interface that implements the <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" /> for a custom <see cref="P:System.Windows.Controls.ListView.View" />. </returns>
	/// <param name="parent">The <see cref="T:System.Windows.Controls.ListView" /> control to use to create the <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" />.</param>
	protected internal virtual IViewAutomationPeer GetAutomationPeer(ListView parent)
	{
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ViewBase" /> class.</summary>
	protected ViewBase()
	{
	}
}
