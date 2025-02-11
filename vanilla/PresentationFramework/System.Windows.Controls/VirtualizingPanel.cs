using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Provides a framework for <see cref="T:System.Windows.Controls.Panel" /> elements that virtualize their child data collection. This is an abstract class.</summary>
public abstract class VirtualizingPanel : Panel
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizing" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizing" /> dependency property.</returns>
	public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.RegisterAttached("IsVirtualizing", typeof(bool), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnVirtualizationPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.VirtualizationMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.VirtualizationMode" /> dependency property.</returns>
	public static readonly DependencyProperty VirtualizationModeProperty = DependencyProperty.RegisterAttached("VirtualizationMode", typeof(VirtualizationMode), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(VirtualizationMode.Standard, FrameworkPropertyMetadataOptions.AffectsMeasure, OnVirtualizationPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizingWhenGrouping" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizingWhenGrouping" /> attached property.</returns>
	public static readonly DependencyProperty IsVirtualizingWhenGroupingProperty = DependencyProperty.RegisterAttached("IsVirtualizingWhenGrouping", typeof(bool), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure, OnVirtualizationPropertyChanged, CoerceIsVirtualizingWhenGrouping));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> attached property.</returns>
	public static readonly DependencyProperty ScrollUnitProperty = DependencyProperty.RegisterAttached("ScrollUnit", typeof(ScrollUnit), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(ScrollUnit.Item, FrameworkPropertyMetadataOptions.AffectsMeasure, OnVirtualizationPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> attached property.</returns>
	public static readonly DependencyProperty CacheLengthProperty = DependencyProperty.RegisterAttached("CacheLength", typeof(VirtualizationCacheLength), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(new VirtualizationCacheLength(1.0), FrameworkPropertyMetadataOptions.AffectsMeasure, OnVirtualizationPropertyChanged), ValidateCacheSizeBeforeOrAfterViewport);

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLengthUnit" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLengthUnit" /> attached property.</returns>
	public static readonly DependencyProperty CacheLengthUnitProperty = DependencyProperty.RegisterAttached("CacheLengthUnit", typeof(VirtualizationCacheLengthUnit), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(VirtualizationCacheLengthUnit.Page, FrameworkPropertyMetadataOptions.AffectsMeasure, OnVirtualizationPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsContainerVirtualizable" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsContainerVirtualizable" /> attached property.</returns>
	public static readonly DependencyProperty IsContainerVirtualizableProperty = DependencyProperty.RegisterAttached("IsContainerVirtualizable", typeof(bool), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(true));

	internal static readonly DependencyProperty ShouldCacheContainerSizeProperty = DependencyProperty.RegisterAttached("ShouldCacheContainerSize", typeof(bool), typeof(VirtualizingPanel), new FrameworkPropertyMetadata(true));

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> can virtualize items that are grouped or organized in a hierarchy.</summary>
	/// <returns>A value that indicates whether the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> can virtualize items that are grouped or organized in a hierarchy.</returns>
	public bool CanHierarchicallyScrollAndVirtualize => CanHierarchicallyScrollAndVirtualizeCore;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> can virtualize items that are grouped or organized in a hierarchy.</summary>
	/// <returns>false in all cases.</returns>
	protected virtual bool CanHierarchicallyScrollAndVirtualizeCore => false;

	/// <summary>Gets a value that identifies the <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> for this <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> for this <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</returns>
	public IItemContainerGenerator ItemContainerGenerator => base.Generator;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> class.</summary>
	protected VirtualizingPanel()
	{
	}

	/// <summary>Returns the position of the specified item, relative to the <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</summary>
	/// <returns>The position of the specified item, relative to the <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</returns>
	/// <param name="child">The element whose position to find.</param>
	public double GetItemOffset(UIElement child)
	{
		return GetItemOffsetCore(child);
	}

	/// <summary>Returns the position of the specified item, relative to the <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</summary>
	/// <returns>0 in all cases.</returns>
	/// <param name="child">The element whose position to find.</param>
	protected virtual double GetItemOffsetCore(UIElement child)
	{
		return 0.0;
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizing" /> attached property.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing its content; otherwise false.</returns>
	/// <param name="element">The object from which the attached property value is read.</param>
	public static bool GetIsVirtualizing(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsVirtualizingProperty);
	}

	/// <summary>Sets the value of the <see cref="F:System.Windows.Controls.VirtualizingStackPanel.IsVirtualizingProperty" /> attached property.</summary>
	/// <param name="element">The object to which the attached property value is set.</param>
	/// <param name="value">true if the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> is virtualizing; otherwise false.</param>
	public static void SetIsVirtualizing(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsVirtualizingProperty, value);
	}

	/// <summary>Returns the <see cref="P:System.Windows.Controls.VirtualizingPanel.VirtualizationMode" /> attached property for the specified object.</summary>
	/// <returns>One of the enumeration values that specifies whether the object uses container recycling.</returns>
	/// <param name="element">The object from which the <see cref="P:System.Windows.Controls.VirtualizingPanel.VirtualizationMode" /> property is read.</param>
	public static VirtualizationMode GetVirtualizationMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (VirtualizationMode)element.GetValue(VirtualizationModeProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.VirtualizingPanel.VirtualizationMode" /> attached property on the specified object.</summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.VirtualizingPanel.VirtualizationMode" /> property.</param>
	/// <param name="value">One of the enumeration values that specifies whether <paramref name="element" /> uses container recycling.</param>
	public static void SetVirtualizationMode(DependencyObject element, VirtualizationMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(VirtualizationModeProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizingWhenGrouping" /> property.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> virtualizes the grouped items in its collection; otherwise, false. </returns>
	/// <param name="element">The element to get the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizingWhenGrouping" /> attached property from.</param>
	public static bool GetIsVirtualizingWhenGrouping(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsVirtualizingWhenGroupingProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsVirtualizingWhenGrouping" /> attached property.</summary>
	/// <param name="element">The object to set the property on.</param>
	/// <param name="value">true to specify that the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> virtualizes the grouped items in its collection; otherwise, false.</param>
	public static void SetIsVirtualizingWhenGrouping(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsVirtualizingWhenGroupingProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> property.</summary>
	/// <returns>A value that indicates whether scrolling is measured as items in the collection or as pixels.</returns>
	/// <param name="element">The element to get the <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> attached property from.</param>
	public static ScrollUnit GetScrollUnit(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (ScrollUnit)element.GetValue(ScrollUnitProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.VirtualizingPanel.ScrollUnit" /> attached property.</summary>
	/// <param name="element">The object to set the property on.</param>
	/// <param name="value">A value that indicates whether scrolling is measured as items in the collection or as pixels.</param>
	public static void SetScrollUnit(DependencyObject element, ScrollUnit value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ScrollUnitProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> property.</summary>
	/// <returns>The size of the cache before and after the viewport when the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing. </returns>
	/// <param name="element">The element to get the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> attached property from.</param>
	public static VirtualizationCacheLength GetCacheLength(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (VirtualizationCacheLength)element.GetValue(CacheLengthProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> attached property.</summary>
	/// <param name="element">The object to set the property on.</param>
	/// <param name="value">The size of the cache before and after the viewport when the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> is virtualizing.</param>
	public static void SetCacheLength(DependencyObject element, VirtualizationCacheLength value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(CacheLengthProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLengthUnit" /> property.</summary>
	/// <returns>The type of unit that is used by the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> property. </returns>
	/// <param name="element">The element to get the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLengthUnit" /> attached property from.</param>
	public static VirtualizationCacheLengthUnit GetCacheLengthUnit(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (VirtualizationCacheLengthUnit)element.GetValue(CacheLengthUnitProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLengthUnit" /> attached property.</summary>
	/// <param name="element">The object to set the property on.</param>
	/// <param name="value">The type of unit that is used by the <see cref="P:System.Windows.Controls.VirtualizingPanel.CacheLength" /> property. </param>
	public static void SetCacheLengthUnit(DependencyObject element, VirtualizationCacheLengthUnit value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(CacheLengthUnitProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsContainerVirtualizable" /> property.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> should virtualize an item; otherwise, false. </returns>
	/// <param name="element">The element to get the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsContainerVirtualizable" /> attached property from.</param>
	public static bool GetIsContainerVirtualizable(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsContainerVirtualizableProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.VirtualizingPanel.IsContainerVirtualizable" /> attached property.</summary>
	/// <param name="element">The object to set the property on.</param>
	/// <param name="value">true to indicate that the <see cref="T:System.Windows.Controls.VirtualizingPanel" /> should virtualize an item; otherwise, false. </param>
	public static void SetIsContainerVirtualizable(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsContainerVirtualizableProperty, value);
	}

	internal static bool GetShouldCacheContainerSize(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (VirtualizingStackPanel.IsVSP45Compat)
		{
			return (bool)element.GetValue(ShouldCacheContainerSizeProperty);
		}
		return true;
	}

	private static bool ValidateCacheSizeBeforeOrAfterViewport(object value)
	{
		VirtualizationCacheLength virtualizationCacheLength = (VirtualizationCacheLength)value;
		if (DoubleUtil.GreaterThanOrClose(virtualizationCacheLength.CacheBeforeViewport, 0.0))
		{
			return DoubleUtil.GreaterThanOrClose(virtualizationCacheLength.CacheAfterViewport, 0.0);
		}
		return false;
	}

	private static object CoerceIsVirtualizingWhenGrouping(DependencyObject d, object baseValue)
	{
		return GetIsVirtualizing(d) && (bool)baseValue;
	}

	internal static void OnVirtualizationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!(d is ItemsControl { ItemsHost: { } itemsHost } itemsControl))
		{
			return;
		}
		itemsHost.InvalidateMeasure();
		if (VisualTreeHelper.GetParent(itemsHost) is ItemsPresenter itemsPresenter)
		{
			itemsPresenter.InvalidateMeasure();
		}
		if (d is TreeView)
		{
			DependencyProperty property = e.Property;
			if (property == VirtualizingStackPanel.IsVirtualizingProperty || property == IsVirtualizingWhenGroupingProperty || property == VirtualizingStackPanel.VirtualizationModeProperty || property == ScrollUnitProperty)
			{
				VirtualizationPropertyChangePropagationRecursive(itemsControl, itemsHost);
			}
		}
	}

	private static void VirtualizationPropertyChangePropagationRecursive(DependencyObject parent, Panel itemsHost)
	{
		UIElementCollection internalChildren = itemsHost.InternalChildren;
		int count = internalChildren.Count;
		for (int i = 0; i < count; i++)
		{
			if (internalChildren[i] is IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo)
			{
				TreeViewItem.IsVirtualizingPropagationHelper(parent, (DependencyObject)hierarchicalVirtualizationAndScrollInfo);
				Panel itemsHost2 = hierarchicalVirtualizationAndScrollInfo.ItemsHost;
				if (itemsHost2 != null)
				{
					VirtualizationPropertyChangePropagationRecursive((DependencyObject)hierarchicalVirtualizationAndScrollInfo, itemsHost2);
				}
			}
		}
	}

	internal override void GenerateChildren()
	{
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.UIElement" /> to the <see cref="P:System.Windows.Controls.Panel.InternalChildren" /> collection of a <see cref="T:System.Windows.Controls.VirtualizingPanel" /> element.</summary>
	/// <param name="child">The <see cref="T:System.Windows.UIElement" /> child to add to the collection.</param>
	protected void AddInternalChild(UIElement child)
	{
		AddInternalChild(base.InternalChildren, child);
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.UIElement" /> to the <see cref="P:System.Windows.Controls.Panel.InternalChildren" /> collection of a <see cref="T:System.Windows.Controls.VirtualizingPanel" /> element at the specified index position.</summary>
	/// <param name="index">The index position within the collection at which the child element is inserted.</param>
	/// <param name="child">The <see cref="T:System.Windows.UIElement" /> child to add to the collection.</param>
	protected void InsertInternalChild(int index, UIElement child)
	{
		InsertInternalChild(base.InternalChildren, index, child);
	}

	/// <summary>Removes child elements from the <see cref="P:System.Windows.Controls.Panel.InternalChildren" /> collection.</summary>
	/// <param name="index">The beginning index position within the collection at which the first child element is removed.</param>
	/// <param name="range">The total number of child elements to remove from the collection.</param>
	protected void RemoveInternalChildRange(int index, int range)
	{
		RemoveInternalChildRange(base.InternalChildren, index, range);
	}

	internal static void AddInternalChild(UIElementCollection children, UIElement child)
	{
		children.AddInternal(child);
	}

	internal static void InsertInternalChild(UIElementCollection children, int index, UIElement child)
	{
		children.InsertInternal(index, child);
	}

	internal static void RemoveInternalChildRange(UIElementCollection children, int index, int range)
	{
		children.RemoveRangeInternal(index, range);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> for this <see cref="T:System.Windows.Controls.Panel" /> changes.</summary>
	/// <param name="sender">The <see cref="T:System.Object" /> that raised the event.</param>
	/// <param name="args">Provides data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</param>
	protected virtual void OnItemsChanged(object sender, ItemsChangedEventArgs args)
	{
	}

	/// <summary>Returns a value that indicates whether a changed item in an <see cref="T:System.Windows.Controls.ItemsControl" /> affects the layout for this panel.</summary>
	/// <returns>true if the changed item in an <see cref="T:System.Windows.Controls.ItemsControl" /> affects the layout for this panel; otherwise, false.</returns>
	/// <param name="areItemChangesLocal">true if the changed item is a direct child of this <see cref="T:System.Windows.Controls.VirtualizingPanel" />; false if the changed item is an indirect descendant of the <see cref="T:System.Windows.Controls.VirtualizingPanel" />.  </param>
	/// <param name="args">Contains data regarding the changed item.</param>
	public bool ShouldItemsChangeAffectLayout(bool areItemChangesLocal, ItemsChangedEventArgs args)
	{
		return ShouldItemsChangeAffectLayoutCore(areItemChangesLocal, args);
	}

	/// <summary>Returns a value that indicates whether a changed item in an <see cref="T:System.Windows.Controls.ItemsControl" /> affects the layout for this panel.</summary>
	/// <returns>true if the changed item in an <see cref="T:System.Windows.Controls.ItemsControl" /> affects the layout for this panel; otherwise, false.</returns>
	/// <param name="areItemChangesLocal">true if the changed item is a direct child of this <see cref="T:System.Windows.Controls.VirtualizingPanel" />; false if the changed item is an indirect descendant of the <see cref="T:System.Windows.Controls.VirtualizingPanel" />.  </param>
	/// <param name="args">Contains data regarding the changed item.</param>
	protected virtual bool ShouldItemsChangeAffectLayoutCore(bool areItemChangesLocal, ItemsChangedEventArgs args)
	{
		return true;
	}

	/// <summary>Called when the collection of child elements is cleared by the base <see cref="T:System.Windows.Controls.Panel" /> class.</summary>
	protected virtual void OnClearChildren()
	{
	}

	/// <summary>Generates the item at the specified index location and makes it visible.</summary>
	/// <param name="index">The index position of the item that is generated and made visible.</param>
	public void BringIndexIntoViewPublic(int index)
	{
		BringIndexIntoView(index);
	}

	/// <summary>When implemented in a derived class, generates the item at the specified index location and makes it visible.</summary>
	/// <param name="index">The index position of the item that is generated and made visible.</param>
	protected internal virtual void BringIndexIntoView(int index)
	{
	}

	internal override bool OnItemsChangedInternal(object sender, ItemsChangedEventArgs args)
	{
		NotifyCollectionChangedAction action = args.Action;
		if ((uint)action > 3u)
		{
			base.OnItemsChangedInternal(sender, args);
		}
		OnItemsChanged(sender, args);
		return ShouldItemsChangeAffectLayout(areItemChangesLocal: true, args);
	}

	internal override void OnClearChildrenInternal()
	{
		OnClearChildren();
	}
}
