using System.Windows.Automation.Peers;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that displays items and information in a horizontal bar in an application window.</summary>
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(StatusBarItem))]
public class StatusBar : ItemsControl
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.StatusBar.ItemContainerTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.StatusBar.ItemContainerTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty ItemContainerTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.StatusBar.UsesItemContainerTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.StatusBar.UsesItemContainerTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty UsesItemContainerTemplateProperty;

	private object _currentItem;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the custom logic for choosing a template used to display each item. </summary>
	/// <returns>A custom object that provides logic and returns an item container. </returns>
	public ItemContainerTemplateSelector ItemContainerTemplateSelector
	{
		get
		{
			return (ItemContainerTemplateSelector)GetValue(ItemContainerTemplateSelectorProperty);
		}
		set
		{
			SetValue(ItemContainerTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the menu selects different item containers, depending on the type of the item in the underlying collection or some other heuristic.</summary>
	/// <returns>true the menu selects different item containers; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool UsesItemContainerTemplate
	{
		get
		{
			return (bool)GetValue(UsesItemContainerTemplateProperty);
		}
		set
		{
			SetValue(UsesItemContainerTemplateProperty, value);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>The key that represents the style to use for <see cref="T:System.Windows.Controls.Separator" /> objects in the <see cref="T:System.Windows.Controls.Primitives.StatusBar" />.</summary>
	/// <returns>A <see cref="T:System.Windows.ResourceKey" /> that references the style to use for <see cref="T:System.Windows.Controls.Separator" /> objects.</returns>
	public static ResourceKey SeparatorStyleKey => SystemResourceKey.StatusBarSeparatorStyleKey;

	static StatusBar()
	{
		ItemContainerTemplateSelectorProperty = MenuBase.ItemContainerTemplateSelectorProperty.AddOwner(typeof(StatusBar), new FrameworkPropertyMetadata(new DefaultItemContainerTemplateSelector()));
		UsesItemContainerTemplateProperty = MenuBase.UsesItemContainerTemplateProperty.AddOwner(typeof(StatusBar));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBar), new FrameworkPropertyMetadata(typeof(StatusBar)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(StatusBar));
		Control.IsTabStopProperty.OverrideMetadata(typeof(StatusBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(DockPanel)));
		itemsPanelTemplate.Seal();
		ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(StatusBar), new FrameworkPropertyMetadata(itemsPanelTemplate));
		ControlsTraceLogger.AddControl(TelemetryControls.StatusBar);
	}

	/// <summary>Determines if the specified item is (or is eligible to be) its own container.</summary>
	/// <returns>Returns true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
	/// <param name="item">The specified object to evaluate.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		int num;
		if (!(item is StatusBarItem))
		{
			num = ((item is Separator) ? 1 : 0);
			if (num == 0)
			{
				_currentItem = item;
			}
		}
		else
		{
			num = 1;
		}
		return (byte)num != 0;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" />.</summary>
	/// <returns>The element used to display the specified item.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		object currentItem = _currentItem;
		_currentItem = null;
		if (UsesItemContainerTemplate)
		{
			DataTemplate dataTemplate = ItemContainerTemplateSelector.SelectTemplate(currentItem, this);
			if (dataTemplate != null)
			{
				object obj = dataTemplate.LoadContent();
				if (obj is StatusBarItem || obj is Separator)
				{
					return obj as DependencyObject;
				}
				throw new InvalidOperationException(SR.Format(SR.InvalidItemContainer, GetType().Name, typeof(StatusBarItem).Name, typeof(Separator).Name, obj));
			}
		}
		return new StatusBarItem();
	}

	/// <summary>Prepares an item for display in the <see cref="T:System.Windows.Controls.Primitives.StatusBar" />.</summary>
	/// <param name="element">The item to display in the <see cref="T:System.Windows.Controls.Primitives.StatusBar" />.</param>
	/// <param name="item">The content of the item to display.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		if (element is Separator separator)
		{
			if (separator.GetValueSource(FrameworkElement.StyleProperty, null, out var _) <= BaseValueSourceInternal.ImplicitReference)
			{
				separator.SetResourceReference(FrameworkElement.StyleProperty, SeparatorStyleKey);
			}
			separator.DefaultStyleKey = SeparatorStyleKey;
		}
	}

	/// <summary>Determines whether to apply the <see cref="T:System.Windows.Style" /> for an item in the <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> to an object.</summary>
	/// <returns>true if the <paramref name="item" /> is not a <see cref="T:System.Windows.Controls.Separator" />; otherwise, false.</returns>
	/// <param name="container">The container for the item.</param>
	/// <param name="item">The object to evaluate.</param>
	protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
	{
		if (item is Separator)
		{
			return false;
		}
		return base.ShouldApplyItemContainerStyle(container, item);
	}

	/// <summary>Specifies an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Primitives.StatusBar" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.StatusBarAutomationPeer" /> for this <see cref="T:System.Windows.Controls.Primitives.StatusBar" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new StatusBarAutomationPeer(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> class. </summary>
	public StatusBar()
	{
	}
}
