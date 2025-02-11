using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that displays a list of data items.</summary>
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListViewItem))]
public class ListView : ListBox
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ListView.View" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ListView.View" /> dependency property.</returns>
	public static readonly DependencyProperty ViewProperty;

	private ViewBase _previousView;

	/// <summary>Gets or sets an object that defines how the data is styled and organized in a <see cref="T:System.Windows.Controls.ListView" /> control.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ViewBase" /> object that specifies how to display information in the <see cref="T:System.Windows.Controls.ListView" />.</returns>
	public ViewBase View
	{
		get
		{
			return (ViewBase)GetValue(ViewProperty);
		}
		set
		{
			SetValue(ViewProperty, value);
		}
	}

	static ListView()
	{
		ViewProperty = DependencyProperty.Register("View", typeof(ViewBase), typeof(ListView), new PropertyMetadata(OnViewChanged));
		ListBox.SelectionModeProperty.OverrideMetadata(typeof(ListView), new FrameworkPropertyMetadata(SelectionMode.Extended));
		ControlsTraceLogger.AddControl(TelemetryControls.ListView);
	}

	private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ListView listView = (ListView)d;
		ViewBase viewBase = (ViewBase)e.OldValue;
		ViewBase viewBase2 = (ViewBase)e.NewValue;
		if (viewBase2 != null)
		{
			if (viewBase2.IsUsed)
			{
				throw new InvalidOperationException(SR.ListView_ViewCannotBeShared);
			}
			viewBase2.IsUsed = true;
		}
		listView._previousView = viewBase;
		listView.ApplyNewView();
		listView._previousView = viewBase2;
		if (UIElementAutomationPeer.FromElement(listView) is ListViewAutomationPeer listViewAutomationPeer)
		{
			if (listViewAutomationPeer.ViewAutomationPeer != null)
			{
				listViewAutomationPeer.ViewAutomationPeer.ViewDetached();
			}
			if (viewBase2 != null)
			{
				listViewAutomationPeer.ViewAutomationPeer = viewBase2.GetAutomationPeer(listView);
			}
			else
			{
				listViewAutomationPeer.ViewAutomationPeer = null;
			}
			listViewAutomationPeer.InvalidatePeer();
		}
		if (viewBase != null)
		{
			viewBase.IsUsed = false;
		}
	}

	/// <summary>Sets the styles, templates, and bindings for a <see cref="T:System.Windows.Controls.ListViewItem" />.</summary>
	/// <param name="element">An object that is a <see cref="T:System.Windows.Controls.ListViewItem" /> or that can be converted into one.</param>
	/// <param name="item">The object to use to create the <see cref="T:System.Windows.Controls.ListViewItem" />.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		if (element is ListViewItem listViewItem)
		{
			ViewBase view = View;
			if (view != null)
			{
				listViewItem.SetDefaultStyleKey(view.ItemContainerDefaultStyleKey);
				view.PrepareItem(listViewItem);
			}
			else
			{
				listViewItem.ClearDefaultStyleKey();
			}
		}
	}

	/// <summary>Removes all templates, styles, and bindings for the object that is displayed as a <see cref="T:System.Windows.Controls.ListViewItem" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.Controls.ListViewItem" /> container to clear.</param>
	/// <param name="item">The object that the <see cref="T:System.Windows.Controls.ListViewItem" /> contains.</param>
	protected override void ClearContainerForItemOverride(DependencyObject element, object item)
	{
		base.ClearContainerForItemOverride(element, item);
	}

	/// <summary>Determines whether an object is a <see cref="T:System.Windows.Controls.ListViewItem" />.</summary>
	/// <returns>true if the <paramref name="item" /> is a <see cref="T:System.Windows.Controls.ListViewItem" />; otherwise, false.</returns>
	/// <param name="item">The object to evaluate.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is ListViewItem;
	}

	/// <summary>Creates and returns a new <see cref="T:System.Windows.Controls.ListViewItem" /> container.</summary>
	/// <returns>A new <see cref="T:System.Windows.Controls.ListViewItem" /> control.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new ListViewItem();
	}

	/// <summary>Responds to an <see cref="M:System.Windows.Controls.ItemsControl.OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs)" />. </summary>
	/// <param name="e">The event arguments.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		base.OnItemsChanged(e);
		if (UIElementAutomationPeer.FromElement(this) is ListViewAutomationPeer { ViewAutomationPeer: not null } listViewAutomationPeer)
		{
			listViewAutomationPeer.ViewAutomationPeer.ItemsChanged(e);
		}
	}

	/// <summary>Defines an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.ListView" /> control.</summary>
	/// <returns>Returns a <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" /> object for the <see cref="T:System.Windows.Controls.ListView" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		ListViewAutomationPeer listViewAutomationPeer = new ListViewAutomationPeer(this);
		if (listViewAutomationPeer != null && View != null)
		{
			listViewAutomationPeer.ViewAutomationPeer = View.GetAutomationPeer(this);
		}
		return listViewAutomationPeer;
	}

	private void ApplyNewView()
	{
		ViewBase view = View;
		if (view != null)
		{
			base.DefaultStyleKey = view.DefaultStyleKey;
		}
		else
		{
			ClearValue(FrameworkElement.DefaultStyleKeyProperty);
		}
		if (base.IsLoaded)
		{
			base.ItemContainerGenerator.Refresh();
		}
	}

	internal override void OnThemeChanged()
	{
		if (!base.HasTemplateGeneratedSubTree && View != null)
		{
			View.OnThemeChanged();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ListView" /> class.</summary>
	public ListView()
	{
	}
}
