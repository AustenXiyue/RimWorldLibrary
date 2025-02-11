using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Appears as the root of the visual subtree generated for a group. </summary>
public class GroupItem : ContentControl, IHierarchicalVirtualizationAndScrollInfo, IContainItemStorage
{
	private ItemContainerGenerator _generator;

	private Panel _itemsHost;

	private FrameworkElement _header;

	private Expander _expander;

	internal static readonly UncommonField<bool> MustDisableVirtualizationField;

	internal static readonly UncommonField<bool> InBackgroundLayoutField;

	internal static readonly UncommonField<Thickness> DesiredPixelItemsSizeCorrectionFactorField;

	internal static readonly UncommonField<HierarchicalVirtualizationConstraints> HierarchicalVirtualizationConstraintsField;

	internal static readonly UncommonField<HierarchicalVirtualizationHeaderDesiredSizes> HierarchicalVirtualizationHeaderDesiredSizesField;

	internal static readonly UncommonField<HierarchicalVirtualizationItemDesiredSizes> HierarchicalVirtualizationItemDesiredSizesField;

	private static DependencyObjectType _dType;

	private const string ExpanderHeaderPartName = "HeaderSite";

	internal ItemContainerGenerator Generator
	{
		get
		{
			return _generator;
		}
		set
		{
			_generator = value;
		}
	}

	/// <summary>Gets or sets an object that represents the sizes of the control's viewport and cache.</summary>
	/// <returns>An object that represents the sizes of the control's viewport and cache.</returns>
	HierarchicalVirtualizationConstraints IHierarchicalVirtualizationAndScrollInfo.Constraints
	{
		get
		{
			return HierarchicalVirtualizationConstraintsField.GetValue(this);
		}
		set
		{
			if (value.CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
			{
				throw new InvalidOperationException(SR.PageCacheSizeNotAllowed);
			}
			HierarchicalVirtualizationConstraintsField.SetValue(this, value);
		}
	}

	/// <summary>Gets an object that represents the desired size of the control's header.</summary>
	/// <returns>An object that represents the desired size of the control's header.</returns>
	HierarchicalVirtualizationHeaderDesiredSizes IHierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes
	{
		get
		{
			FrameworkElement headerElement = HeaderElement;
			Size headerSize = default(Size);
			if (base.IsVisible && headerElement != null)
			{
				headerSize = headerElement.DesiredSize;
				Helper.ApplyCorrectionFactorToPixelHeaderSize(ParentItemsControl, this, _itemsHost, ref headerSize);
			}
			return new HierarchicalVirtualizationHeaderDesiredSizes(new Size(DoubleUtil.GreaterThanZero(headerSize.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(headerSize.Height) ? 1 : 0), headerSize);
		}
	}

	/// <summary>Gets or sets an object that represents the desired size of the control's items.</summary>
	/// <returns>An object that represents the desired size of the control's items.</returns>
	HierarchicalVirtualizationItemDesiredSizes IHierarchicalVirtualizationAndScrollInfo.ItemDesiredSizes
	{
		get
		{
			return Helper.ApplyCorrectionFactorToItemDesiredSizes(this, _itemsHost);
		}
		set
		{
			HierarchicalVirtualizationItemDesiredSizesField.SetValue(this, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Panel" /> that displays the items of the control.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Panel" /> that displays the items of the control.</returns>
	Panel IHierarchicalVirtualizationAndScrollInfo.ItemsHost => _itemsHost;

	/// <summary>Gets or sets a value that indicates whether the owning <see cref="T:System.Windows.Controls.ItemsControl" /> should virtualize its items.</summary>
	/// <returns>true if the owning <see cref="T:System.Windows.Controls.ItemsControl" /> should virtualize its items; otherwise, false.</returns>
	bool IHierarchicalVirtualizationAndScrollInfo.MustDisableVirtualization
	{
		get
		{
			return MustDisableVirtualizationField.GetValue(this);
		}
		set
		{
			MustDisableVirtualizationField.SetValue(this, value);
		}
	}

	/// <summary>Gets a value that indicates whether the control's layout pass occurs at a lower priority.</summary>
	/// <returns>true if the control's layout pass occurs at a lower priority; otherwise, false.</returns>
	bool IHierarchicalVirtualizationAndScrollInfo.InBackgroundLayout
	{
		get
		{
			return InBackgroundLayoutField.GetValue(this);
		}
		set
		{
			InBackgroundLayoutField.SetValue(this, value);
		}
	}

	private ItemsControl ParentItemsControl
	{
		get
		{
			DependencyObject dependencyObject = this;
			do
			{
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				if (dependencyObject is ItemsControl result)
				{
					return result;
				}
			}
			while (dependencyObject != null);
			return null;
		}
	}

	internal IContainItemStorage ParentItemStorageProvider
	{
		get
		{
			DependencyObject parent = VisualTreeHelper.GetParent(this);
			if (parent != null)
			{
				return ItemsControl.GetItemsOwnerInternal(parent) as IContainItemStorage;
			}
			return null;
		}
	}

	internal Panel ItemsHost
	{
		get
		{
			return _itemsHost;
		}
		set
		{
			_itemsHost = value;
		}
	}

	private ItemsPresenter ItemsHostPresenter
	{
		get
		{
			if (_expander != null)
			{
				return Helper.FindTemplatedDescendant<ItemsPresenter>(_expander, _expander);
			}
			return Helper.FindTemplatedDescendant<ItemsPresenter>(this, this);
		}
	}

	internal Expander Expander => _expander;

	private FrameworkElement ExpanderHeader
	{
		get
		{
			if (_expander != null)
			{
				return _expander.GetTemplateChild("HeaderSite") as FrameworkElement;
			}
			return null;
		}
	}

	private FrameworkElement HeaderElement
	{
		get
		{
			FrameworkElement result = null;
			if (_header != null)
			{
				result = _header;
			}
			else if (_expander != null)
			{
				result = ExpanderHeader;
			}
			return result;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static GroupItem()
	{
		MustDisableVirtualizationField = new UncommonField<bool>();
		InBackgroundLayoutField = new UncommonField<bool>();
		DesiredPixelItemsSizeCorrectionFactorField = new UncommonField<Thickness>();
		HierarchicalVirtualizationConstraintsField = new UncommonField<HierarchicalVirtualizationConstraints>();
		HierarchicalVirtualizationHeaderDesiredSizesField = new UncommonField<HierarchicalVirtualizationHeaderDesiredSizes>();
		HierarchicalVirtualizationItemDesiredSizesField = new UncommonField<HierarchicalVirtualizationItemDesiredSizes>();
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupItem), new FrameworkPropertyMetadata(typeof(GroupItem)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(GroupItem));
		UIElement.FocusableProperty.OverrideMetadata(typeof(GroupItem), new FrameworkPropertyMetadata(false));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(GroupItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.GroupItem" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.GroupItem" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new GroupItemAutomationPeer(this);
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.GroupItem" /> when a new template is applied. </summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		_header = GetTemplateChild("PART_Header") as FrameworkElement;
		_expander = Helper.FindTemplatedDescendant<Expander>(this, this);
		if (_expander != null)
		{
			ItemsControl parentItemsControl = ParentItemsControl;
			if (parentItemsControl != null && VirtualizingPanel.GetIsVirtualizingWhenGrouping(parentItemsControl))
			{
				Helper.SetItemValuesOnContainer(parentItemsControl, _expander, parentItemsControl.ItemContainerGenerator.ItemFromContainer(this));
			}
			_expander.Expanded += OnExpanded;
		}
	}

	private static void OnExpanded(object sender, RoutedEventArgs e)
	{
		if (!(sender is GroupItem { _expander: not null } groupItem) || !groupItem._expander.IsExpanded)
		{
			return;
		}
		ItemsControl parentItemsControl = groupItem.ParentItemsControl;
		if (parentItemsControl != null && VirtualizingPanel.GetIsVirtualizing(parentItemsControl) && VirtualizingPanel.GetVirtualizationMode(parentItemsControl) == VirtualizationMode.Recycling)
		{
			ItemsPresenter itemsHostPresenter = groupItem.ItemsHostPresenter;
			if (itemsHostPresenter != null)
			{
				groupItem.InvalidateMeasure();
				Helper.InvalidateMeasureOnPath(itemsHostPresenter, groupItem, duringMeasure: false);
			}
		}
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		base.OnTemplateChangedInternal(oldTemplate, newTemplate);
		if (_expander != null)
		{
			_expander.Expanded -= OnExpanded;
			_expander = null;
		}
		_itemsHost = null;
	}

	/// <summary>Arranges the content of the <see cref="T:System.Windows.Controls.GroupItem" />.</summary>
	/// <returns>The actual sized used by the <see cref="T:System.Windows.Controls.GroupItem" />.</returns>
	/// <param name="arrangeSize">The final area within the parent that the <see cref="T:System.Windows.Controls.GroupItem" /> should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		arrangeSize = base.ArrangeOverride(arrangeSize);
		Helper.ComputeCorrectionFactor(ParentItemsControl, this, ItemsHost, HeaderElement);
		return arrangeSize;
	}

	internal override string GetPlainText()
	{
		if (base.Content is CollectionViewGroup { Name: not null } collectionViewGroup)
		{
			return collectionViewGroup.Name.ToString();
		}
		return base.GetPlainText();
	}

	internal void PrepareItemContainer(object item, ItemsControl parentItemsControl)
	{
		if (Generator == null)
		{
			return;
		}
		if (_itemsHost != null)
		{
			_itemsHost.IsItemsHost = true;
		}
		bool flag = parentItemsControl != null && VirtualizingPanel.GetIsVirtualizingWhenGrouping(parentItemsControl);
		if (Generator != null)
		{
			if (!flag)
			{
				Generator.Release();
			}
			else
			{
				Generator.RemoveAllInternal(saveRecycleQueue: true);
			}
		}
		GroupStyle groupStyle = Generator.Parent.GroupStyle;
		Style style = groupStyle.ContainerStyle;
		if (style == null && groupStyle.ContainerStyleSelector != null)
		{
			style = groupStyle.ContainerStyleSelector.SelectStyle(item, this);
		}
		if (style != null)
		{
			if (!style.TargetType.IsInstanceOfType(this))
			{
				throw new InvalidOperationException(SR.Format(SR.StyleForWrongType, style.TargetType.Name, GetType().Name));
			}
			base.Style = style;
			WriteInternalFlag2(InternalFlags2.IsStyleSetFromGenerator, set: true);
		}
		if (base.ContentIsItem || !HasNonDefaultValue(ContentControl.ContentProperty))
		{
			base.Content = item;
			base.ContentIsItem = true;
		}
		if (!HasNonDefaultValue(ContentControl.ContentTemplateProperty))
		{
			base.ContentTemplate = groupStyle.HeaderTemplate;
		}
		if (!HasNonDefaultValue(ContentControl.ContentTemplateSelectorProperty))
		{
			base.ContentTemplateSelector = groupStyle.HeaderTemplateSelector;
		}
		if (!HasNonDefaultValue(ContentControl.ContentStringFormatProperty))
		{
			base.ContentStringFormat = groupStyle.HeaderStringFormat;
		}
		Helper.ClearVirtualizingElement(this);
		if (flag)
		{
			Helper.SetItemValuesOnContainer(parentItemsControl, this, item);
			if (_expander != null)
			{
				Helper.SetItemValuesOnContainer(parentItemsControl, _expander, item);
			}
		}
	}

	internal void ClearItemContainer(object item, ItemsControl parentItemsControl)
	{
		if (Generator == null)
		{
			return;
		}
		if (parentItemsControl != null && VirtualizingPanel.GetIsVirtualizingWhenGrouping(parentItemsControl))
		{
			Helper.StoreItemValues(parentItemsControl, this, item);
			if (_expander != null)
			{
				Helper.StoreItemValues(parentItemsControl, _expander, item);
			}
			if (_itemsHost is VirtualizingPanel virtualizingPanel)
			{
				virtualizingPanel.OnClearChildrenInternal();
			}
			Generator.RemoveAllInternal(saveRecycleQueue: true);
		}
		else
		{
			Generator.Release();
		}
		ClearContentControl(item);
	}

	/// <summary>Returns the value of the specified property that is associated with the specified item.</summary>
	/// <returns>The value of the specified property that is associated with the specified item.</returns>
	/// <param name="item">The item that has the specified property associated with it.</param>
	/// <param name="dp">The property whose value to return.</param>
	object IContainItemStorage.ReadItemValue(object item, DependencyProperty dp)
	{
		return Helper.ReadItemValue(this, item, dp.GlobalIndex);
	}

	/// <summary>Stores the specified property and value and associates them with the specified item.</summary>
	/// <param name="item">The item to associate the value and property with.</param>
	/// <param name="dp">The property that is associated with the specified item.</param>
	/// <param name="value">The value of the associated property.</param>
	void IContainItemStorage.StoreItemValue(object item, DependencyProperty dp, object value)
	{
		Helper.StoreItemValue(this, item, dp.GlobalIndex, value);
	}

	/// <summary>Removes the association between the specified item and property.</summary>
	/// <param name="item">The associated item.</param>
	/// <param name="dp">The associated property.</param>
	void IContainItemStorage.ClearItemValue(object item, DependencyProperty dp)
	{
		Helper.ClearItemValue(this, item, dp.GlobalIndex);
	}

	/// <summary>Removes the specified property from all property lists.</summary>
	/// <param name="dp">The property to remove.</param>
	void IContainItemStorage.ClearValue(DependencyProperty dp)
	{
		Helper.ClearItemValueStorage(this, new int[1] { dp.GlobalIndex });
	}

	/// <summary>Clears all property associations.</summary>
	void IContainItemStorage.Clear()
	{
		Helper.ClearItemValueStorage(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GroupItem" /> class. </summary>
	public GroupItem()
	{
	}
}
