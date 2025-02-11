using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Data;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Win32;

namespace System.Windows.Controls;

/// <summary>Represents a control that can be used to present a collection of items.</summary>
[DefaultEvent("OnItemsChanged")]
[DefaultProperty("Items")]
[ContentProperty("Items")]
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(FrameworkElement))]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class ItemsControl : Control, IAddChild, IGeneratorHost, IContainItemStorage
{
	internal class ItemNavigateArgs
	{
		private InputDevice _deviceUsed;

		private ModifierKeys _modifierKeys;

		private static ItemNavigateArgs _empty;

		public InputDevice DeviceUsed => _deviceUsed;

		public static ItemNavigateArgs Empty
		{
			get
			{
				if (_empty == null)
				{
					_empty = new ItemNavigateArgs(null, ModifierKeys.None);
				}
				return _empty;
			}
		}

		public ItemNavigateArgs(InputDevice deviceUsed, ModifierKeys modifierKeys)
		{
			_deviceUsed = deviceUsed;
			_modifierKeys = modifierKeys;
		}
	}

	[DebuggerDisplay("Index: {Index}  Item: {Item}")]
	internal class ItemInfo
	{
		internal static readonly DependencyObject SentinelContainer;

		internal static readonly DependencyObject UnresolvedContainer;

		internal static readonly DependencyObject KeyContainer;

		internal static readonly DependencyObject RemovedContainer;

		internal object Item { get; private set; }

		internal DependencyObject Container { get; set; }

		internal int Index { get; set; }

		internal bool IsResolved => Container != UnresolvedContainer;

		internal bool IsKey => Container == KeyContainer;

		internal bool IsRemoved => Container == RemovedContainer;

		static ItemInfo()
		{
			SentinelContainer = new DependencyObject();
			UnresolvedContainer = new DependencyObject();
			KeyContainer = new DependencyObject();
			RemovedContainer = new DependencyObject();
			SentinelContainer.MakeSentinel();
			UnresolvedContainer.MakeSentinel();
			KeyContainer.MakeSentinel();
			RemovedContainer.MakeSentinel();
		}

		public ItemInfo(object item, DependencyObject container = null, int index = -1)
		{
			Item = item;
			Container = container;
			Index = index;
		}

		internal ItemInfo Clone()
		{
			return new ItemInfo(Item, Container, Index);
		}

		internal static ItemInfo Key(ItemInfo info)
		{
			if (info.Container != UnresolvedContainer)
			{
				return info;
			}
			return new ItemInfo(info.Item, KeyContainer);
		}

		public override int GetHashCode()
		{
			if (Item == null)
			{
				return 314159;
			}
			return Item.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o == this)
			{
				return true;
			}
			ItemInfo itemInfo = o as ItemInfo;
			if (itemInfo == null)
			{
				return false;
			}
			return Equals(itemInfo, matchUnresolved: false);
		}

		internal bool Equals(ItemInfo that, bool matchUnresolved)
		{
			if (IsRemoved || that.IsRemoved)
			{
				return false;
			}
			if (!EqualsEx(Item, that.Item))
			{
				return false;
			}
			if (Container == KeyContainer)
			{
				if (!matchUnresolved)
				{
					return that.Container != UnresolvedContainer;
				}
				return true;
			}
			if (that.Container == KeyContainer)
			{
				if (!matchUnresolved)
				{
					return Container != UnresolvedContainer;
				}
				return true;
			}
			if (Container == UnresolvedContainer || that.Container == UnresolvedContainer)
			{
				return false;
			}
			if (Container != that.Container)
			{
				if (Container != SentinelContainer && that.Container != SentinelContainer)
				{
					if (Container == null || that.Container == null)
					{
						if (Index >= 0 && that.Index >= 0)
						{
							return Index == that.Index;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			if (Container != SentinelContainer)
			{
				if (Index >= 0 && that.Index >= 0)
				{
					return Index == that.Index;
				}
				return true;
			}
			return Index == that.Index;
		}

		public static bool operator ==(ItemInfo info1, ItemInfo info2)
		{
			return object.Equals(info1, info2);
		}

		public static bool operator !=(ItemInfo info1, ItemInfo info2)
		{
			return !object.Equals(info1, info2);
		}

		internal ItemInfo Refresh(ItemContainerGenerator generator)
		{
			if (Container == null && Index < 0)
			{
				Container = generator.ContainerFromItem(Item);
			}
			if (Index < 0 && Container != null)
			{
				Index = generator.IndexFromContainer(Container);
			}
			if (Container == null && Index >= 0)
			{
				Container = generator.ContainerFromIndex(Index);
			}
			if (Container == SentinelContainer && Index >= 0)
			{
				Container = null;
			}
			return this;
		}

		internal void Reset(object item)
		{
			Item = item;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemsSourceProperty;

	internal static readonly DependencyPropertyKey HasItemsPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.HasItems" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.HasItems" /> dependency property.</returns>
	public static readonly DependencyProperty HasItemsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.DisplayMemberPath" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.DisplayMemberPath" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayMemberPathProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemStringFormat" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemStringFormatProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemBindingGroup" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemBindingGroup" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemBindingGroupProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemContainerStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyleSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyleSelector" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemContainerStyleSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.ItemsPanel" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.ItemsPanel" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ItemsPanelProperty;

	private static readonly DependencyPropertyKey IsGroupingPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.IsGrouping" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.IsGrouping" /> dependency property.</returns>
	public static readonly DependencyProperty IsGroupingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyleSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyleSelector" /> dependency property.</returns>
	public static readonly DependencyProperty GroupStyleSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.AlternationCount" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.AlternationCount" /> dependency property.</returns>
	public static readonly DependencyProperty AlternationCountProperty;

	private static readonly DependencyPropertyKey AlternationIndexPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.AlternationIndex" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.AlternationIndex" /> dependency property.</returns>
	public static readonly DependencyProperty AlternationIndexProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.IsTextSearchEnabled" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.IsTextSearchEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsTextSearchEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ItemsControl.IsTextSearchCaseSensitive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ItemsControl.IsTextSearchCaseSensitive" /> dependency property.</returns>
	public static readonly DependencyProperty IsTextSearchCaseSensitiveProperty;

	private ItemInfo _focusedInfo;

	private ItemCollection _items;

	private ItemContainerGenerator _itemContainerGenerator;

	private Panel _itemsHost;

	private ScrollViewer _scrollHost;

	private ObservableCollection<GroupStyle> _groupStyle = new ObservableCollection<GroupStyle>();

	private static readonly UncommonField<bool> ShouldCoerceScrollUnitField;

	private static readonly UncommonField<bool> ShouldCoerceCacheSizeField;

	private static DependencyObjectType _dType;

	/// <summary>Gets the collection used to generate the content of the <see cref="T:System.Windows.Controls.ItemsControl" />.</summary>
	/// <returns>The collection that is used to generate the content of the <see cref="T:System.Windows.Controls.ItemsControl" />. The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Bindable(true)]
	[CustomCategory("Content")]
	public ItemCollection Items
	{
		get
		{
			if (_items == null)
			{
				CreateItemCollectionAndGenerator();
			}
			return _items;
		}
	}

	/// <summary>Gets or sets a collection used to generate the content of the <see cref="T:System.Windows.Controls.ItemsControl" />.  </summary>
	/// <returns>A collection that is used to generate the content of the <see cref="T:System.Windows.Controls.ItemsControl" />. The default is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IEnumerable ItemsSource
	{
		get
		{
			return Items.ItemsSource;
		}
		set
		{
			if (value == null)
			{
				ClearValue(ItemsSourceProperty);
			}
			else
			{
				SetValue(ItemsSourceProperty, value);
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> that is associated with the control. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> that is associated with the control. The default is null.</returns>
	[Bindable(false)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public ItemContainerGenerator ItemContainerGenerator
	{
		get
		{
			if (_itemContainerGenerator == null)
			{
				CreateItemCollectionAndGenerator();
			}
			return _itemContainerGenerator;
		}
	}

	/// <summary>Gets an enumerator for the logical child objects of the <see cref="T:System.Windows.Controls.ItemsControl" /> object.</summary>
	/// <returns>An enumerator for the logical child objects of the <see cref="T:System.Windows.Controls.ItemsControl" /> object. The default is null.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (!HasItems)
			{
				return EmptyEnumerator.Instance;
			}
			return Items.LogicalChildren;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.ItemsControl" /> contains items.  </summary>
	/// <returns>true if the items count is greater than 0; otherwise, false.The default is false.</returns>
	[Bindable(false)]
	[Browsable(false)]
	public bool HasItems => (bool)GetValue(HasItemsProperty);

	/// <summary>Gets or sets a path to a value on the source object to serve as the visual representation of the object.  </summary>
	/// <returns>The path to a value on the source object. This can be any path, or an XPath such as "@Name". The default is an empty string ("").</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public string DisplayMemberPath
	{
		get
		{
			return (string)GetValue(DisplayMemberPathProperty);
		}
		set
		{
			SetValue(DisplayMemberPathProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.DataTemplate" /> used to display each item.  </summary>
	/// <returns>A <see cref="T:System.Windows.DataTemplate" /> that specifies the visualization of the data objects. The default is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public DataTemplate ItemTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ItemTemplateProperty);
		}
		set
		{
			SetValue(ItemTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the custom logic for choosing a template used to display each item.  </summary>
	/// <returns>A custom <see cref="T:System.Windows.Controls.DataTemplateSelector" /> object that provides logic and returns a <see cref="T:System.Windows.DataTemplate" />. The default is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector ItemTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
		}
		set
		{
			SetValue(ItemTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the items in the <see cref="T:System.Windows.Controls.ItemsControl" /> if they are displayed as strings.</summary>
	/// <returns>A composite string that specifies how to format the items in the <see cref="T:System.Windows.Controls.ItemsControl" /> if they are displayed as strings.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public string ItemStringFormat
	{
		get
		{
			return (string)GetValue(ItemStringFormatProperty);
		}
		set
		{
			SetValue(ItemStringFormatProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Data.BindingGroup" /> that is copied to each item in the <see cref="T:System.Windows.Controls.ItemsControl" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingGroup" /> that is copied to each item in the <see cref="T:System.Windows.Controls.ItemsControl" />.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public BindingGroup ItemBindingGroup
	{
		get
		{
			return (BindingGroup)GetValue(ItemBindingGroupProperty);
		}
		set
		{
			SetValue(ItemBindingGroupProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Style" /> that is applied to the container element generated for each item.  </summary>
	/// <returns>The <see cref="T:System.Windows.Style" /> that is applied to the container element generated for each item. The default is null.</returns>
	[Bindable(true)]
	[Category("Content")]
	public Style ItemContainerStyle
	{
		get
		{
			return (Style)GetValue(ItemContainerStyleProperty);
		}
		set
		{
			SetValue(ItemContainerStyleProperty, value);
		}
	}

	/// <summary>Gets or sets custom style-selection logic for a style that can be applied to each generated container element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.StyleSelector" /> object that contains logic that chooses the style to use as the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" />. The default is null.</returns>
	[Bindable(true)]
	[Category("Content")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleSelector ItemContainerStyleSelector
	{
		get
		{
			return (StyleSelector)GetValue(ItemContainerStyleSelectorProperty);
		}
		set
		{
			SetValue(ItemContainerStyleSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets the template that defines the panel that controls the layout of items.  </summary>
	/// <returns>An <see cref="T:System.Windows.Controls.ItemsPanelTemplate" /> that defines the panel to use for the layout of the items. The default value for the <see cref="T:System.Windows.Controls.ItemsControl" /> is an <see cref="T:System.Windows.Controls.ItemsPanelTemplate" /> that specifies a <see cref="T:System.Windows.Controls.StackPanel" />.</returns>
	[Bindable(false)]
	public ItemsPanelTemplate ItemsPanel
	{
		get
		{
			return (ItemsPanelTemplate)GetValue(ItemsPanelProperty);
		}
		set
		{
			SetValue(ItemsPanelProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the control is using grouping.  </summary>
	/// <returns>true if a control is using grouping; otherwise, false.</returns>
	[Bindable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsGrouping => (bool)GetValue(IsGroupingProperty);

	/// <summary>Gets a collection of <see cref="T:System.Windows.Controls.GroupStyle" /> objects that define the appearance of each level of groups.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.GroupStyle" /> objects that define the appearance of each level of groups.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ObservableCollection<GroupStyle> GroupStyle => _groupStyle;

	/// <summary>Gets or sets a method that enables you to provide custom selection logic for a <see cref="T:System.Windows.Controls.GroupStyle" /> to apply to each group in a collection.  </summary>
	/// <returns>A method that enables you to provide custom selection logic for a <see cref="T:System.Windows.Controls.GroupStyle" /> to apply to each group in a collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Bindable(true)]
	[CustomCategory("Content")]
	public GroupStyleSelector GroupStyleSelector
	{
		get
		{
			return (GroupStyleSelector)GetValue(GroupStyleSelectorProperty);
		}
		set
		{
			SetValue(GroupStyleSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets the number of alternating item containers in the <see cref="T:System.Windows.Controls.ItemsControl" />, which enables alternating containers to have a unique appearance. </summary>
	/// <returns>The number of alternating item containers in the <see cref="T:System.Windows.Controls.ItemsControl" />. </returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public int AlternationCount
	{
		get
		{
			return (int)GetValue(AlternationCountProperty);
		}
		set
		{
			SetValue(AlternationCountProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Controls.TextSearch" /> is enabled on the <see cref="T:System.Windows.Controls.ItemsControl" /> instance.  </summary>
	/// <returns>true if <see cref="T:System.Windows.Controls.TextSearch" /> is enabled; otherwise, false. The default is false.</returns>
	public bool IsTextSearchEnabled
	{
		get
		{
			return (bool)GetValue(IsTextSearchEnabledProperty);
		}
		set
		{
			SetValue(IsTextSearchEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether case is a condition when searching for items.</summary>
	/// <returns>true if text searches are case-sensitive; otherwise, false.</returns>
	public bool IsTextSearchCaseSensitive
	{
		get
		{
			return (bool)GetValue(IsTextSearchCaseSensitiveProperty);
		}
		set
		{
			SetValue(IsTextSearchCaseSensitiveProperty, BooleanBoxes.Box(value));
		}
	}

	ItemCollection IGeneratorHost.View => Items;

	int IGeneratorHost.AlternationCount => AlternationCount;

	private bool IsInitPending => ReadInternalFlag(InternalFlags.InitPending);

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

	internal ItemInfo FocusedInfo => _focusedInfo;

	internal bool IsLogicalVertical
	{
		get
		{
			if (ItemsHost != null && ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Vertical && ScrollHost != null && ScrollHost.CanContentScroll)
			{
				return VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
			}
			return false;
		}
	}

	internal bool IsLogicalHorizontal
	{
		get
		{
			if (ItemsHost != null && ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal && ScrollHost != null && ScrollHost.CanContentScroll)
			{
				return VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
			}
			return false;
		}
	}

	internal ScrollViewer ScrollHost
	{
		get
		{
			if (!ReadControlFlag(ControlBoolFlags.ScrollHostValid))
			{
				if (_itemsHost == null)
				{
					return null;
				}
				DependencyObject dependencyObject = _itemsHost;
				while (dependencyObject != this && dependencyObject != null)
				{
					if (dependencyObject is ScrollViewer scrollHost)
					{
						_scrollHost = scrollHost;
						break;
					}
					dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				}
				WriteControlFlag(ControlBoolFlags.ScrollHostValid, set: true);
			}
			return _scrollHost;
		}
	}

	internal static TimeSpan AutoScrollTimeout => TimeSpan.FromMilliseconds((double)SafeNativeMethods.GetDoubleClickTime() * 0.8);

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ItemsControl" /> class.</summary>
	public ItemsControl()
	{
		ShouldCoerceCacheSizeField.SetValue(this, value: true);
		CoerceValue(VirtualizingPanel.CacheLengthUnitProperty);
	}

	static ItemsControl()
	{
		ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));
		HasItemsPropertyKey = DependencyProperty.RegisterReadOnly("HasItems", typeof(bool), typeof(ItemsControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, Control.OnVisualStatePropertyChanged));
		HasItemsProperty = HasItemsPropertyKey.DependencyProperty;
		DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(ItemsControl), new FrameworkPropertyMetadata(string.Empty, OnDisplayMemberPathChanged));
		ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemTemplateChanged));
		ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemTemplateSelectorChanged));
		ItemStringFormatProperty = DependencyProperty.Register("ItemStringFormat", typeof(string), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemStringFormatChanged));
		ItemBindingGroupProperty = DependencyProperty.Register("ItemBindingGroup", typeof(BindingGroup), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemBindingGroupChanged));
		ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemContainerStyleChanged));
		ItemContainerStyleSelectorProperty = DependencyProperty.Register("ItemContainerStyleSelector", typeof(StyleSelector), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnItemContainerStyleSelectorChanged));
		ItemsPanelProperty = DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(ItemsControl), new FrameworkPropertyMetadata(GetDefaultItemsPanelTemplate(), OnItemsPanelChanged));
		IsGroupingPropertyKey = DependencyProperty.RegisterReadOnly("IsGrouping", typeof(bool), typeof(ItemsControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsGroupingChanged));
		IsGroupingProperty = IsGroupingPropertyKey.DependencyProperty;
		GroupStyleSelectorProperty = DependencyProperty.Register("GroupStyleSelector", typeof(GroupStyleSelector), typeof(ItemsControl), new FrameworkPropertyMetadata(null, OnGroupStyleSelectorChanged));
		AlternationCountProperty = DependencyProperty.Register("AlternationCount", typeof(int), typeof(ItemsControl), new FrameworkPropertyMetadata(0, OnAlternationCountChanged));
		AlternationIndexPropertyKey = DependencyProperty.RegisterAttachedReadOnly("AlternationIndex", typeof(int), typeof(ItemsControl), new FrameworkPropertyMetadata(0));
		AlternationIndexProperty = AlternationIndexPropertyKey.DependencyProperty;
		IsTextSearchEnabledProperty = DependencyProperty.Register("IsTextSearchEnabled", typeof(bool), typeof(ItemsControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsTextSearchCaseSensitiveProperty = DependencyProperty.Register("IsTextSearchCaseSensitive", typeof(bool), typeof(ItemsControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ShouldCoerceScrollUnitField = new UncommonField<bool>();
		ShouldCoerceCacheSizeField = new UncommonField<bool>();
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsControl), new FrameworkPropertyMetadata(typeof(ItemsControl)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ItemsControl));
		EventManager.RegisterClassHandler(typeof(ItemsControl), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotFocus));
		VirtualizingPanel.ScrollUnitProperty.OverrideMetadata(typeof(ItemsControl), new FrameworkPropertyMetadata(OnScrollingModeChanged, CoerceScrollingMode));
		VirtualizingPanel.CacheLengthProperty.OverrideMetadata(typeof(ItemsControl), new FrameworkPropertyMetadata(OnCacheSizeChanged));
		VirtualizingPanel.CacheLengthUnitProperty.OverrideMetadata(typeof(ItemsControl), new FrameworkPropertyMetadata(OnCacheSizeChanged, CoerceVirtualizationCacheLengthUnit));
	}

	private static void OnScrollingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ShouldCoerceScrollUnitField.SetValue(d, value: true);
		d.CoerceValue(VirtualizingPanel.ScrollUnitProperty);
	}

	private static object CoerceScrollingMode(DependencyObject d, object baseValue)
	{
		if (ShouldCoerceScrollUnitField.GetValue(d))
		{
			ShouldCoerceScrollUnitField.SetValue(d, value: false);
			BaseValueSource baseValueSource = DependencyPropertyHelper.GetValueSource(d, VirtualizingPanel.ScrollUnitProperty).BaseValueSource;
			if (((ItemsControl)d).IsGrouping && baseValueSource == BaseValueSource.Default)
			{
				return ScrollUnit.Pixel;
			}
		}
		return baseValue;
	}

	private static void OnCacheSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ShouldCoerceCacheSizeField.SetValue(d, value: true);
		d.CoerceValue(e.Property);
	}

	private static object CoerceVirtualizationCacheLengthUnit(DependencyObject d, object baseValue)
	{
		if (ShouldCoerceCacheSizeField.GetValue(d))
		{
			ShouldCoerceCacheSizeField.SetValue(d, value: false);
			BaseValueSource baseValueSource = DependencyPropertyHelper.GetValueSource(d, VirtualizingPanel.CacheLengthUnitProperty).BaseValueSource;
			if (!((ItemsControl)d).IsGrouping && !(d is TreeView) && baseValueSource == BaseValueSource.Default)
			{
				return VirtualizationCacheLengthUnit.Item;
			}
		}
		return baseValue;
	}

	private void CreateItemCollectionAndGenerator()
	{
		_items = new ItemCollection(this);
		((INotifyCollectionChanged)_items).CollectionChanged += OnItemCollectionChanged1;
		_itemContainerGenerator = new ItemContainerGenerator(this);
		_itemContainerGenerator.ChangeAlternationCount();
		((INotifyCollectionChanged)_items).CollectionChanged += OnItemCollectionChanged2;
		if (IsInitPending)
		{
			_items.BeginInit();
		}
		else if (base.IsInitialized)
		{
			_items.BeginInit();
			_items.EndInit();
		}
		((INotifyCollectionChanged)_groupStyle).CollectionChanged += OnGroupStyleChanged;
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeItems()
	{
		return HasItems;
	}

	private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ItemsControl itemsControl = (ItemsControl)d;
		IEnumerable oldValue = (IEnumerable)e.OldValue;
		IEnumerable enumerable = (IEnumerable)e.NewValue;
		((IContainItemStorage)itemsControl).Clear();
		BindingExpressionBase beb = BindingOperations.GetBindingExpressionBase(d, ItemsSourceProperty);
		if (beb != null)
		{
			itemsControl.Items.SetItemsSource(enumerable, (object x) => beb.GetSourceItem(x));
		}
		else if (e.NewValue != null)
		{
			itemsControl.Items.SetItemsSource(enumerable);
		}
		else
		{
			itemsControl.Items.ClearItemsSource();
		}
		itemsControl.OnItemsSourceChanged(oldValue, enumerable);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> property changes.</summary>
	/// <param name="oldValue">Old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> property.</param>
	/// <param name="newValue">New value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> property.</param>
	protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
	{
	}

	private void OnItemCollectionChanged1(object sender, NotifyCollectionChangedEventArgs e)
	{
		AdjustItemInfoOverride(e);
	}

	private void OnItemCollectionChanged2(object sender, NotifyCollectionChangedEventArgs e)
	{
		SetValue(HasItemsPropertyKey, _items != null && !_items.IsEmpty);
		if (_focusedInfo != null && _focusedInfo.Index < 0)
		{
			_focusedInfo = null;
		}
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			((IContainItemStorage)this).Clear();
		}
		OnItemsChanged(e);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes.</summary>
	/// <param name="e">Information about the change.</param>
	protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
	}

	internal virtual void AdjustItemInfoOverride(NotifyCollectionChangedEventArgs e)
	{
		AdjustItemInfo(e, _focusedInfo);
	}

	private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ItemsControl obj = (ItemsControl)d;
		obj.OnDisplayMemberPathChanged((string)e.OldValue, (string)e.NewValue);
		obj.UpdateDisplayMemberTemplateSelector();
	}

	private void UpdateDisplayMemberTemplateSelector()
	{
		string displayMemberPath = DisplayMemberPath;
		string itemStringFormat = ItemStringFormat;
		if (!string.IsNullOrEmpty(displayMemberPath) || !string.IsNullOrEmpty(itemStringFormat))
		{
			DataTemplateSelector itemTemplateSelector = ItemTemplateSelector;
			if (itemTemplateSelector != null && !(itemTemplateSelector is DisplayMemberTemplateSelector) && (ReadLocalValue(ItemTemplateSelectorProperty) != DependencyProperty.UnsetValue || ReadLocalValue(DisplayMemberPathProperty) == DependencyProperty.UnsetValue))
			{
				throw new InvalidOperationException(SR.DisplayMemberPathAndItemTemplateSelectorDefined);
			}
			ItemTemplateSelector = new DisplayMemberTemplateSelector(DisplayMemberPath, ItemStringFormat);
		}
		else if (ItemTemplateSelector is DisplayMemberTemplateSelector)
		{
			ClearValue(ItemTemplateSelectorProperty);
		}
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.DisplayMemberPath" /> property changes.</summary>
	/// <param name="oldDisplayMemberPath">The old value of the <see cref="P:System.Windows.Controls.ItemsControl.DisplayMemberPath" /> property.</param>
	/// <param name="newDisplayMemberPath">New value of the <see cref="P:System.Windows.Controls.ItemsControl.DisplayMemberPath" /> property.</param>
	protected virtual void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
	{
	}

	private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> property changes.</summary>
	/// <param name="oldItemTemplate">The old <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> property value.</param>
	/// <param name="newItemTemplate">The new <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> property value.</param>
	protected virtual void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
	{
		CheckTemplateSource();
		if (_itemContainerGenerator != null)
		{
			_itemContainerGenerator.Refresh();
		}
	}

	private static void OnItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> property changes.</summary>
	/// <param name="oldItemTemplateSelector">Old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> property.</param>
	/// <param name="newItemTemplateSelector">New value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> property.</param>
	protected virtual void OnItemTemplateSelectorChanged(DataTemplateSelector oldItemTemplateSelector, DataTemplateSelector newItemTemplateSelector)
	{
		CheckTemplateSource();
		if (_itemContainerGenerator != null && ItemTemplate == null)
		{
			_itemContainerGenerator.Refresh();
		}
	}

	private static void OnItemStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ItemsControl obj = (ItemsControl)d;
		obj.OnItemStringFormatChanged((string)e.OldValue, (string)e.NewValue);
		obj.UpdateDisplayMemberTemplateSelector();
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemStringFormat" /> property changes.</summary>
	/// <param name="oldItemStringFormat">The old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemStringFormat" /> property.</param>
	/// <param name="newItemStringFormat">The new value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemStringFormat" /> property.</param>
	protected virtual void OnItemStringFormatChanged(string oldItemStringFormat, string newItemStringFormat)
	{
	}

	private static void OnItemBindingGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemBindingGroupChanged((BindingGroup)e.OldValue, (BindingGroup)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemBindingGroup" /> property changes.</summary>
	/// <param name="oldItemBindingGroup">The old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemBindingGroup" />.</param>
	/// <param name="newItemBindingGroup">The new value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemBindingGroup" />.</param>
	protected virtual void OnItemBindingGroupChanged(BindingGroup oldItemBindingGroup, BindingGroup newItemBindingGroup)
	{
	}

	private void CheckTemplateSource()
	{
		if (string.IsNullOrEmpty(DisplayMemberPath))
		{
			Helper.CheckTemplateAndTemplateSelector("Item", ItemTemplateProperty, ItemTemplateSelectorProperty, this);
			return;
		}
		if (!(ItemTemplateSelector is DisplayMemberTemplateSelector))
		{
			throw new InvalidOperationException(SR.ItemTemplateSelectorBreaksDisplayMemberPath);
		}
		if (!Helper.IsTemplateDefined(ItemTemplateProperty, this))
		{
			return;
		}
		throw new InvalidOperationException(SR.DisplayMemberPathAndItemTemplateDefined);
	}

	private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemContainerStyleChanged((Style)e.OldValue, (Style)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" /> property changes.</summary>
	/// <param name="oldItemContainerStyle">Old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" /> property.</param>
	/// <param name="newItemContainerStyle">New value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" /> property.</param>
	protected virtual void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)
	{
		Helper.CheckStyleAndStyleSelector("ItemContainer", ItemContainerStyleProperty, ItemContainerStyleSelectorProperty, this);
		if (_itemContainerGenerator != null)
		{
			_itemContainerGenerator.Refresh();
		}
	}

	private static void OnItemContainerStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemContainerStyleSelectorChanged((StyleSelector)e.OldValue, (StyleSelector)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyleSelector" /> property changes.</summary>
	/// <param name="oldItemContainerStyleSelector">Old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyleSelector" /> property.</param>
	/// <param name="newItemContainerStyleSelector">New value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyleSelector" /> property.</param>
	protected virtual void OnItemContainerStyleSelectorChanged(StyleSelector oldItemContainerStyleSelector, StyleSelector newItemContainerStyleSelector)
	{
		Helper.CheckStyleAndStyleSelector("ItemContainer", ItemContainerStyleProperty, ItemContainerStyleSelectorProperty, this);
		if (_itemContainerGenerator != null && ItemContainerStyle == null)
		{
			_itemContainerGenerator.Refresh();
		}
	}

	/// <summary>Returns the <see cref="T:System.Windows.Controls.ItemsControl" /> that the specified element hosts items for.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ItemsControl" /> that the specified element hosts items for, or null.</returns>
	/// <param name="element">The host element.</param>
	public static ItemsControl GetItemsOwner(DependencyObject element)
	{
		ItemsControl result = null;
		if (element is Panel { IsItemsHost: not false } panel)
		{
			ItemsPresenter itemsPresenter = ItemsPresenter.FromPanel(panel);
			result = ((itemsPresenter == null) ? (panel.TemplatedParent as ItemsControl) : itemsPresenter.Owner);
		}
		return result;
	}

	internal static DependencyObject GetItemsOwnerInternal(DependencyObject element)
	{
		ItemsControl itemsControl;
		return GetItemsOwnerInternal(element, out itemsControl);
	}

	internal static DependencyObject GetItemsOwnerInternal(DependencyObject element, out ItemsControl itemsControl)
	{
		DependencyObject dependencyObject = null;
		Panel panel = element as Panel;
		itemsControl = null;
		if (panel != null && panel.IsItemsHost)
		{
			ItemsPresenter itemsPresenter = ItemsPresenter.FromPanel(panel);
			if (itemsPresenter != null)
			{
				dependencyObject = itemsPresenter.TemplatedParent;
				itemsControl = itemsPresenter.Owner;
			}
			else
			{
				dependencyObject = panel.TemplatedParent;
				itemsControl = dependencyObject as ItemsControl;
			}
		}
		return dependencyObject;
	}

	private static ItemsPanelTemplate GetDefaultItemsPanelTemplate()
	{
		ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));
		itemsPanelTemplate.Seal();
		return itemsPanelTemplate;
	}

	private static void OnItemsPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnItemsPanelChanged((ItemsPanelTemplate)e.OldValue, (ItemsPanelTemplate)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemsPanel" /> property changes.</summary>
	/// <param name="oldItemsPanel">Old value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemsPanel" /> property.</param>
	/// <param name="newItemsPanel">New value of the <see cref="P:System.Windows.Controls.ItemsControl.ItemsPanel" /> property.</param>
	protected virtual void OnItemsPanelChanged(ItemsPanelTemplate oldItemsPanel, ItemsPanelTemplate newItemsPanel)
	{
		ItemContainerGenerator.OnPanelChanged();
	}

	private static void OnIsGroupingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnIsGroupingChanged(e);
	}

	internal virtual void OnIsGroupingChanged(DependencyPropertyChangedEventArgs e)
	{
		ShouldCoerceScrollUnitField.SetValue(this, value: true);
		CoerceValue(VirtualizingPanel.ScrollUnitProperty);
		ShouldCoerceCacheSizeField.SetValue(this, value: true);
		CoerceValue(VirtualizingPanel.CacheLengthUnitProperty);
		((IContainItemStorage)this).Clear();
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyle" /> property.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyle" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeGroupStyle()
	{
		return GroupStyle.Count > 0;
	}

	private void OnGroupStyleChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (_itemContainerGenerator != null)
		{
			_itemContainerGenerator.Refresh();
		}
	}

	private static void OnGroupStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).OnGroupStyleSelectorChanged((GroupStyleSelector)e.OldValue, (GroupStyleSelector)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyleSelector" /> property changes.</summary>
	/// <param name="oldGroupStyleSelector">Old value of the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyleSelector" /> property.</param>
	/// <param name="newGroupStyleSelector">New value of the <see cref="P:System.Windows.Controls.ItemsControl.GroupStyleSelector" /> property.</param>
	protected virtual void OnGroupStyleSelectorChanged(GroupStyleSelector oldGroupStyleSelector, GroupStyleSelector newGroupStyleSelector)
	{
		if (_itemContainerGenerator != null)
		{
			_itemContainerGenerator.Refresh();
		}
	}

	private static void OnAlternationCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ItemsControl obj = (ItemsControl)d;
		int oldAlternationCount = (int)e.OldValue;
		int newAlternationCount = (int)e.NewValue;
		obj.OnAlternationCountChanged(oldAlternationCount, newAlternationCount);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.AlternationCount" /> property changes.</summary>
	/// <param name="oldAlternationCount">The old value of <see cref="P:System.Windows.Controls.ItemsControl.AlternationCount" />.</param>
	/// <param name="newAlternationCount">The new value of <see cref="P:System.Windows.Controls.ItemsControl.AlternationCount" />.</param>
	protected virtual void OnAlternationCountChanged(int oldAlternationCount, int newAlternationCount)
	{
		ItemContainerGenerator.ChangeAlternationCount();
	}

	/// <summary>Gets the <see cref="P:System.Windows.Controls.ItemsControl.AlternationIndex" /> for the specified object.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.ItemsControl.AlternationIndex" />.</returns>
	/// <param name="element">The object from which to get the <see cref="P:System.Windows.Controls.ItemsControl.AlternationIndex" />.</param>
	public static int GetAlternationIndex(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(AlternationIndexProperty);
	}

	internal static void SetAlternationIndex(DependencyObject d, int value)
	{
		d.SetValue(AlternationIndexPropertyKey, value);
	}

	internal static void ClearAlternationIndex(DependencyObject d)
	{
		d.ClearValue(AlternationIndexPropertyKey);
	}

	/// <summary>Returns the <see cref="T:System.Windows.Controls.ItemsControl" /> that owns the specified container element.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ItemsControl" /> that owns the specified container element.</returns>
	/// <param name="container">The container element to return the <see cref="T:System.Windows.Controls.ItemsControl" /> for.</param>
	public static ItemsControl ItemsControlFromItemContainer(DependencyObject container)
	{
		if (!(container is UIElement uIElement))
		{
			return null;
		}
		if (LogicalTreeHelper.GetParent(uIElement) is ItemsControl itemsControl)
		{
			if (((IGeneratorHost)itemsControl).IsItemItsOwnContainer((object)uIElement))
			{
				return itemsControl;
			}
			return null;
		}
		UIElement element = VisualTreeHelper.GetParent(uIElement) as UIElement;
		return GetItemsOwner(element);
	}

	/// <summary>Returns the container that belongs to the specified <see cref="T:System.Windows.Controls.ItemsControl" /> that owns the given container element.</summary>
	/// <returns>The container that belongs to the specified <see cref="T:System.Windows.Controls.ItemsControl" /> that owns the given element, if <paramref name="itemsControl" /> is not null. If <paramref name="itemsControl" /> is null, returns the closest container that belongs to any <see cref="T:System.Windows.Controls.ItemsControl" />.</returns>
	/// <param name="itemsControl">The <see cref="T:System.Windows.Controls.ItemsControl" /> to return the container for.</param>
	/// <param name="element">The element to return the container for.</param>
	public static DependencyObject ContainerFromElement(ItemsControl itemsControl, DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (IsContainerForItemsControl(element, itemsControl))
		{
			return element;
		}
		FrameworkObject frameworkObject = new FrameworkObject(element);
		frameworkObject.Reset(frameworkObject.GetPreferVisualParent(force: true).DO);
		while (frameworkObject.DO != null && !IsContainerForItemsControl(frameworkObject.DO, itemsControl))
		{
			frameworkObject.Reset(frameworkObject.PreferVisualParent.DO);
		}
		return frameworkObject.DO;
	}

	/// <summary>Returns the container that belongs to the current <see cref="T:System.Windows.Controls.ItemsControl" /> that owns the given element.</summary>
	/// <returns>The container that belongs to the current <see cref="T:System.Windows.Controls.ItemsControl" /> that owns the given element or null if no such container exists.</returns>
	/// <param name="element">The element to return the container for.</param>
	public DependencyObject ContainerFromElement(DependencyObject element)
	{
		return ContainerFromElement(this, element);
	}

	private static bool IsContainerForItemsControl(DependencyObject element, ItemsControl itemsControl)
	{
		if (element.ContainsValue(ItemContainerGenerator.ItemForItemContainerProperty) && (itemsControl == null || itemsControl == ItemsControlFromItemContainer(element)))
		{
			return true;
		}
		return false;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">The object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		AddChild(value);
	}

	/// <summary>Adds the specified object as the child of the <see cref="T:System.Windows.Controls.ItemsControl" /> object. </summary>
	/// <param name="value">The object to add as a child.</param>
	protected virtual void AddChild(object value)
	{
		Items.Add(value);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">The text to add.</param>
	void IAddChild.AddText(string text)
	{
		AddText(text);
	}

	/// <summary>Adds the specified text string to the <see cref="T:System.Windows.Controls.ItemsControl" /> object.</summary>
	/// <param name="text">The string to add.</param>
	protected virtual void AddText(string text)
	{
		Items.Add(text);
	}

	bool IGeneratorHost.IsItemItsOwnContainer(object item)
	{
		return IsItemItsOwnContainer(item);
	}

	DependencyObject IGeneratorHost.GetContainerForItem(object item)
	{
		DependencyObject dependencyObject = ((!IsItemItsOwnContainerOverride(item)) ? GetContainerForItemOverride() : (item as DependencyObject));
		if (dependencyObject is Visual visual && VisualTreeHelper.GetParent(visual) is Visual visual2)
		{
			Invariant.Assert(visual2 is FrameworkElement, SR.ItemsControl_ParentNotFrameworkElement);
			if (visual2 is Panel panel && visual is UIElement)
			{
				panel.Children.RemoveNoVerify((UIElement)visual);
			}
			else
			{
				((FrameworkElement)visual2).TemplateChild = null;
			}
		}
		return dependencyObject;
	}

	void IGeneratorHost.PrepareItemContainer(DependencyObject container, object item)
	{
		if (container is GroupItem groupItem)
		{
			groupItem.PrepareItemContainer(item, this);
			return;
		}
		if (ShouldApplyItemContainerStyle(container, item))
		{
			ApplyItemContainerStyle(container, item);
		}
		PrepareContainerForItemOverride(container, item);
		if (!Helper.HasUnmodifiedDefaultValue(this, ItemBindingGroupProperty) && Helper.HasUnmodifiedDefaultOrInheritedValue(container, FrameworkElement.BindingGroupProperty))
		{
			BindingGroup itemBindingGroup = ItemBindingGroup;
			BindingGroup value = ((itemBindingGroup != null) ? new BindingGroup(itemBindingGroup) : null);
			container.SetValue(FrameworkElement.BindingGroupProperty, value);
		}
		if (container == item && TraceData.IsEnabled && (ItemTemplate != null || ItemTemplateSelector != null))
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.ItemTemplateForDirectItem, null, new object[1] { AvTrace.TypeName(item) });
		}
		if (container is TreeViewItem treeViewItem)
		{
			treeViewItem.PrepareItemContainer(item, this);
		}
	}

	void IGeneratorHost.ClearContainerForItem(DependencyObject container, object item)
	{
		if (!(container is GroupItem groupItem))
		{
			ClearContainerForItemOverride(container, item);
			if (container is TreeViewItem treeViewItem)
			{
				treeViewItem.ClearItemContainer(item, this);
			}
		}
		else
		{
			groupItem.ClearItemContainer(item, this);
		}
	}

	bool IGeneratorHost.IsHostForItemContainer(DependencyObject container)
	{
		ItemsControl itemsControl = ItemsControlFromItemContainer(container);
		if (itemsControl != null)
		{
			return itemsControl == this;
		}
		if (LogicalTreeHelper.GetParent(container) == null)
		{
			if (IsItemItsOwnContainerOverride(container) && HasItems)
			{
				return Items.Contains(container);
			}
			return false;
		}
		return false;
	}

	GroupStyle IGeneratorHost.GetGroupStyle(CollectionViewGroup group, int level)
	{
		GroupStyle groupStyle = null;
		if (GroupStyleSelector != null)
		{
			groupStyle = GroupStyleSelector(group, level);
		}
		if (groupStyle == null)
		{
			if (level >= GroupStyle.Count)
			{
				level = GroupStyle.Count - 1;
			}
			if (level >= 0)
			{
				groupStyle = GroupStyle[level];
			}
		}
		return groupStyle;
	}

	void IGeneratorHost.SetIsGrouping(bool isGrouping)
	{
		SetValue(IsGroupingPropertyKey, BooleanBoxes.Box(isGrouping));
	}

	/// <summary>Indicates that the initialization of the <see cref="T:System.Windows.Controls.ItemsControl" /> object is about to start.</summary>
	public override void BeginInit()
	{
		base.BeginInit();
		if (_items != null)
		{
			_items.BeginInit();
		}
	}

	/// <summary>Indicates that the initialization of the <see cref="T:System.Windows.Controls.ItemsControl" /> object is complete.</summary>
	public override void EndInit()
	{
		if (IsInitPending)
		{
			if (_items != null)
			{
				_items.EndInit();
			}
			base.EndInit();
		}
	}

	/// <summary>Determines if the specified item is (or is eligible to be) its own container.</summary>
	/// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	public bool IsItemItsOwnContainer(object item)
	{
		return IsItemItsOwnContainerOverride(item);
	}

	/// <summary>Determines if the specified item is (or is eligible to be) its own container.</summary>
	/// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	protected virtual bool IsItemItsOwnContainerOverride(object item)
	{
		return item is UIElement;
	}

	/// <summary>Creates or identifies the element that is used to display the given item.</summary>
	/// <returns>The element that is used to display the given item.</returns>
	protected virtual DependencyObject GetContainerForItemOverride()
	{
		return new ContentPresenter();
	}

	/// <summary>Prepares the specified element to display the specified item. </summary>
	/// <param name="element">Element used to display the specified item.</param>
	/// <param name="item">Specified item.</param>
	protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		if (element is HeaderedContentControl headeredContentControl)
		{
			headeredContentControl.PrepareHeaderedContentControl(item, ItemTemplate, ItemTemplateSelector, ItemStringFormat);
		}
		else if (element is ContentControl contentControl)
		{
			contentControl.PrepareContentControl(item, ItemTemplate, ItemTemplateSelector, ItemStringFormat);
		}
		else if (element is ContentPresenter contentPresenter)
		{
			contentPresenter.PrepareContentPresenter(item, ItemTemplate, ItemTemplateSelector, ItemStringFormat);
		}
		else if (element is HeaderedItemsControl headeredItemsControl)
		{
			headeredItemsControl.PrepareHeaderedItemsControl(item, this);
		}
		else if (element is ItemsControl itemsControl && itemsControl != this)
		{
			itemsControl.PrepareItemsControl(item, this);
		}
	}

	/// <summary>When overridden in a derived class, undoes the effects of the <see cref="M:System.Windows.Controls.ItemsControl.PrepareContainerForItemOverride(System.Windows.DependencyObject,System.Object)" /> method.</summary>
	/// <param name="element">The container element.</param>
	/// <param name="item">The item.</param>
	protected virtual void ClearContainerForItemOverride(DependencyObject element, object item)
	{
		if (element is HeaderedContentControl headeredContentControl)
		{
			headeredContentControl.ClearHeaderedContentControl(item);
		}
		else if (element is ContentControl contentControl)
		{
			contentControl.ClearContentControl(item);
		}
		else if (element is ContentPresenter contentPresenter)
		{
			contentPresenter.ClearContentPresenter(item);
		}
		else if (element is HeaderedItemsControl headeredItemsControl)
		{
			headeredItemsControl.ClearHeaderedItemsControl(item);
		}
		else if (element is ItemsControl itemsControl && itemsControl != this)
		{
			itemsControl.ClearItemsControl(item);
		}
	}

	/// <summary>Invoked when the <see cref="E:System.Windows.UIElement.TextInput" /> event is received.</summary>
	/// <param name="e">Information about the event.</param>
	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);
		if (!string.IsNullOrEmpty(e.Text) && IsTextSearchEnabled && (e.OriginalSource == this || ItemsControlFromItemContainer(e.OriginalSource as DependencyObject) == this))
		{
			TextSearch textSearch = TextSearch.EnsureInstance(this);
			if (textSearch != null)
			{
				textSearch.DoSearch(e.Text);
				e.Handled = true;
			}
		}
	}

	/// <summary>Invoked when the <see cref="E:System.Windows.UIElement.KeyDown" /> event is received.</summary>
	/// <param name="e">Information about the event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (IsTextSearchEnabled && e.Key == Key.Back)
		{
			TextSearch.EnsureInstance(this)?.DeleteLastCharacter();
		}
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		_itemsHost = null;
		_scrollHost = null;
		WriteControlFlag(ControlBoolFlags.ScrollHostValid, set: false);
		base.OnTemplateChangedInternal(oldTemplate, newTemplate);
	}

	/// <summary>Returns a value that indicates whether to apply the style from the <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyle" /> or <see cref="P:System.Windows.Controls.ItemsControl.ItemContainerStyleSelector" /> property to the container element of the specified item.</summary>
	/// <returns>Always true for the base implementation.</returns>
	/// <param name="container">The container element.</param>
	/// <param name="item">The item of interest.</param>
	protected virtual bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
	{
		return true;
	}

	internal void PrepareItemsControl(object item, ItemsControl parentItemsControl)
	{
		if (item != this)
		{
			DataTemplate itemTemplate = parentItemsControl.ItemTemplate;
			DataTemplateSelector itemTemplateSelector = parentItemsControl.ItemTemplateSelector;
			string itemStringFormat = parentItemsControl.ItemStringFormat;
			Style itemContainerStyle = parentItemsControl.ItemContainerStyle;
			StyleSelector itemContainerStyleSelector = parentItemsControl.ItemContainerStyleSelector;
			int alternationCount = parentItemsControl.AlternationCount;
			BindingGroup itemBindingGroup = parentItemsControl.ItemBindingGroup;
			if (itemTemplate != null)
			{
				SetValue(ItemTemplateProperty, itemTemplate);
			}
			if (itemTemplateSelector != null)
			{
				SetValue(ItemTemplateSelectorProperty, itemTemplateSelector);
			}
			if (itemStringFormat != null && Helper.HasDefaultValue(this, ItemStringFormatProperty))
			{
				SetValue(ItemStringFormatProperty, itemStringFormat);
			}
			if (itemContainerStyle != null && Helper.HasDefaultValue(this, ItemContainerStyleProperty))
			{
				SetValue(ItemContainerStyleProperty, itemContainerStyle);
			}
			if (itemContainerStyleSelector != null && Helper.HasDefaultValue(this, ItemContainerStyleSelectorProperty))
			{
				SetValue(ItemContainerStyleSelectorProperty, itemContainerStyleSelector);
			}
			if (alternationCount != 0 && Helper.HasDefaultValue(this, AlternationCountProperty))
			{
				SetValue(AlternationCountProperty, alternationCount);
			}
			if (itemBindingGroup != null && Helper.HasDefaultValue(this, ItemBindingGroupProperty))
			{
				SetValue(ItemBindingGroupProperty, itemBindingGroup);
			}
		}
	}

	internal void ClearItemsControl(object item)
	{
	}

	internal object OnBringItemIntoView(object arg)
	{
		ItemInfo itemInfo = arg as ItemInfo;
		if (itemInfo == null)
		{
			itemInfo = NewItemInfo(arg);
		}
		return OnBringItemIntoView(itemInfo);
	}

	internal object OnBringItemIntoView(ItemInfo info)
	{
		if (info.Container is FrameworkElement frameworkElement)
		{
			frameworkElement.BringIntoView();
		}
		else if ((info = LeaseItemInfo(info, ensureIndex: true)).Index >= 0)
		{
			if (!FrameworkCompatibilityPreferences.GetVSP45Compat())
			{
				UpdateLayout();
			}
			if (ItemsHost is VirtualizingPanel virtualizingPanel)
			{
				virtualizingPanel.BringIndexIntoView(info.Index);
			}
		}
		return null;
	}

	internal bool NavigateByLine(FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs)
	{
		DependencyObject dependencyObject = Keyboard.FocusedElement as DependencyObject;
		if (!FrameworkAppContextSwitches.KeyboardNavigationFromHyperlinkInItemsControlIsNotRelativeToFocusedElement)
		{
			while (dependencyObject != null && !(dependencyObject is FrameworkElement))
			{
				dependencyObject = KeyboardNavigation.GetParent(dependencyObject);
			}
		}
		return NavigateByLine(FocusedInfo, dependencyObject as FrameworkElement, direction, itemNavigateArgs);
	}

	internal void PrepareNavigateByLine(ItemInfo startingInfo, FrameworkElement startingElement, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs, out FrameworkElement container)
	{
		container = null;
		if (ItemsHost != null)
		{
			if (startingElement != null)
			{
				MakeVisible(startingElement, direction, alwaysAtTopOfViewport: false);
			}
			else
			{
				MakeVisible(startingInfo, direction, out startingElement);
			}
			object startingItem = ((startingInfo != null) ? startingInfo.Item : null);
			NavigateByLineInternal(startingItem, direction, startingElement, itemNavigateArgs, shouldFocus: false, out container);
		}
	}

	internal bool NavigateByLine(ItemInfo startingInfo, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs)
	{
		return NavigateByLine(startingInfo, null, direction, itemNavigateArgs);
	}

	internal bool NavigateByLine(ItemInfo startingInfo, FrameworkElement startingElement, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs)
	{
		if (ItemsHost == null)
		{
			return false;
		}
		if (startingElement != null)
		{
			MakeVisible(startingElement, direction, alwaysAtTopOfViewport: false);
		}
		else
		{
			MakeVisible(startingInfo, direction, out startingElement);
		}
		object startingItem = ((startingInfo != null) ? startingInfo.Item : null);
		FrameworkElement container;
		return NavigateByLineInternal(startingItem, direction, startingElement, itemNavigateArgs, shouldFocus: true, out container);
	}

	private bool NavigateByLineInternal(object startingItem, FocusNavigationDirection direction, FrameworkElement startingElement, ItemNavigateArgs itemNavigateArgs, bool shouldFocus, out FrameworkElement container)
	{
		container = null;
		if (startingItem == null && (startingElement == null || startingElement == this))
		{
			return NavigateToStartInternal(itemNavigateArgs, shouldFocus, out container);
		}
		FrameworkElement frameworkElement = null;
		if (startingElement == null || !ItemsHost.IsAncestorOf(startingElement))
		{
			startingElement = ScrollHost;
		}
		else
		{
			DependencyObject parent = VisualTreeHelper.GetParent(startingElement);
			while (parent != null && parent != ItemsHost)
			{
				KeyboardNavigationMode directionalNavigation = KeyboardNavigation.GetDirectionalNavigation(parent);
				if (directionalNavigation == KeyboardNavigationMode.Contained || directionalNavigation == KeyboardNavigationMode.Cycle)
				{
					return false;
				}
				parent = VisualTreeHelper.GetParent(parent);
			}
		}
		bool flag = ItemsHost != null && ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
		bool treeViewNavigation = this is TreeView;
		frameworkElement = KeyboardNavigation.Current.PredictFocusedElement(startingElement, direction, treeViewNavigation) as FrameworkElement;
		if (ScrollHost != null)
		{
			bool flag2 = false;
			FrameworkElement viewportElement = GetViewportElement();
			VirtualizingPanel virtualizingPanel = ItemsHost as VirtualizingPanel;
			bool flag3 = KeyboardNavigation.GetDirectionalNavigation(this) == KeyboardNavigationMode.Cycle;
			while (true)
			{
				if (frameworkElement != null)
				{
					if (virtualizingPanel == null || !ScrollHost.CanContentScroll || !VirtualizingPanel.GetIsVirtualizing(this))
					{
						break;
					}
					Rect elementRect;
					ElementViewportPosition elementViewportPosition = GetElementViewportPosition(viewportElement, TryGetTreeViewItemHeader(frameworkElement) as FrameworkElement, direction, fullyVisible: false, out elementRect);
					if (elementViewportPosition == ElementViewportPosition.CompletelyInViewport || elementViewportPosition == ElementViewportPosition.PartiallyInViewport)
					{
						if (!flag3)
						{
							break;
						}
						GetElementViewportPosition(viewportElement, startingElement, direction, fullyVisible: false, out var elementRect2);
						if (IsInDirectionForLineNavigation(elementRect2, elementRect, direction, flag))
						{
							break;
						}
					}
					frameworkElement = null;
				}
				double horizontalOffset = ScrollHost.HorizontalOffset;
				double verticalOffset = ScrollHost.VerticalOffset;
				switch (direction)
				{
				case FocusNavigationDirection.Down:
					flag2 = true;
					if (flag)
					{
						ScrollHost.LineRight();
					}
					else
					{
						ScrollHost.LineDown();
					}
					break;
				case FocusNavigationDirection.Up:
					flag2 = true;
					if (flag)
					{
						ScrollHost.LineLeft();
					}
					else
					{
						ScrollHost.LineUp();
					}
					break;
				}
				ScrollHost.UpdateLayout();
				if ((DoubleUtil.AreClose(horizontalOffset, ScrollHost.HorizontalOffset) && DoubleUtil.AreClose(verticalOffset, ScrollHost.VerticalOffset)) || (direction == FocusNavigationDirection.Down && (ScrollHost.VerticalOffset > ScrollHost.ExtentHeight || ScrollHost.HorizontalOffset > ScrollHost.ExtentWidth)) || (direction == FocusNavigationDirection.Up && (ScrollHost.VerticalOffset < 0.0 || ScrollHost.HorizontalOffset < 0.0)))
				{
					if (flag3)
					{
						switch (direction)
						{
						case FocusNavigationDirection.Up:
							return NavigateToEndInternal(itemNavigateArgs, shouldFocus: true, out container);
						case FocusNavigationDirection.Down:
							return NavigateToStartInternal(itemNavigateArgs, shouldFocus: true, out container);
						}
					}
					break;
				}
				frameworkElement = KeyboardNavigation.Current.PredictFocusedElement(startingElement, direction, treeViewNavigation) as FrameworkElement;
			}
			if (flag2 && frameworkElement != null && ItemsHost.IsAncestorOf(frameworkElement))
			{
				AdjustOffsetToAlignWithEdge(frameworkElement, direction);
			}
		}
		if (frameworkElement != null && ItemsHost.IsAncestorOf(frameworkElement))
		{
			ItemsControl itemsControl = null;
			object encapsulatingItem = GetEncapsulatingItem(frameworkElement, out container, out itemsControl);
			container = frameworkElement;
			if (!shouldFocus)
			{
				return false;
			}
			if (encapsulatingItem == DependencyProperty.UnsetValue || encapsulatingItem is CollectionViewGroupInternal)
			{
				return frameworkElement.Focus();
			}
			if (itemsControl != null)
			{
				return itemsControl.FocusItem(NewItemInfo(encapsulatingItem, container), itemNavigateArgs);
			}
		}
		return false;
	}

	internal void PrepareToNavigateByPage(ItemInfo startingInfo, FrameworkElement startingElement, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs, out FrameworkElement container)
	{
		container = null;
		if (ItemsHost != null)
		{
			if (startingElement != null)
			{
				MakeVisible(startingElement, direction, alwaysAtTopOfViewport: false);
			}
			else
			{
				MakeVisible(startingInfo, direction, out startingElement);
			}
			object startingItem = ((startingInfo != null) ? startingInfo.Item : null);
			NavigateByPageInternal(startingItem, direction, startingElement, itemNavigateArgs, shouldFocus: false, out container);
		}
	}

	internal bool NavigateByPage(FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs)
	{
		return NavigateByPage(FocusedInfo, Keyboard.FocusedElement as FrameworkElement, direction, itemNavigateArgs);
	}

	internal bool NavigateByPage(ItemInfo startingInfo, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs)
	{
		return NavigateByPage(startingInfo, null, direction, itemNavigateArgs);
	}

	internal bool NavigateByPage(ItemInfo startingInfo, FrameworkElement startingElement, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs)
	{
		if (ItemsHost == null)
		{
			return false;
		}
		if (startingElement != null)
		{
			MakeVisible(startingElement, direction, alwaysAtTopOfViewport: false);
		}
		else
		{
			MakeVisible(startingInfo, direction, out startingElement);
		}
		object startingItem = ((startingInfo != null) ? startingInfo.Item : null);
		FrameworkElement container;
		return NavigateByPageInternal(startingItem, direction, startingElement, itemNavigateArgs, shouldFocus: true, out container);
	}

	private bool NavigateByPageInternal(object startingItem, FocusNavigationDirection direction, FrameworkElement startingElement, ItemNavigateArgs itemNavigateArgs, bool shouldFocus, out FrameworkElement container)
	{
		container = null;
		if (startingItem == null && (startingElement == null || startingElement == this))
		{
			return NavigateToFirstItemOnCurrentPage(startingItem, direction, itemNavigateArgs, shouldFocus, out container);
		}
		FrameworkElement firstElement;
		object firstItemOnCurrentPage = GetFirstItemOnCurrentPage(startingElement, direction, out firstElement);
		if ((object.Equals(startingItem, firstItemOnCurrentPage) || object.Equals(startingElement, firstElement)) && ScrollHost != null)
		{
			bool flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
			do
			{
				double horizontalOffset = ScrollHost.HorizontalOffset;
				double verticalOffset = ScrollHost.VerticalOffset;
				switch (direction)
				{
				case FocusNavigationDirection.Up:
					if (flag)
					{
						ScrollHost.PageLeft();
					}
					else
					{
						ScrollHost.PageUp();
					}
					break;
				case FocusNavigationDirection.Down:
					if (flag)
					{
						ScrollHost.PageRight();
					}
					else
					{
						ScrollHost.PageDown();
					}
					break;
				}
				ScrollHost.UpdateLayout();
				if (DoubleUtil.AreClose(horizontalOffset, ScrollHost.HorizontalOffset) && DoubleUtil.AreClose(verticalOffset, ScrollHost.VerticalOffset))
				{
					break;
				}
				firstItemOnCurrentPage = GetFirstItemOnCurrentPage(startingElement, direction, out firstElement);
			}
			while (firstItemOnCurrentPage == DependencyProperty.UnsetValue);
		}
		container = firstElement;
		if (shouldFocus)
		{
			if (firstElement != null && (firstItemOnCurrentPage == DependencyProperty.UnsetValue || firstItemOnCurrentPage is CollectionViewGroupInternal))
			{
				return firstElement.Focus();
			}
			ItemsControl encapsulatingItemsControl = GetEncapsulatingItemsControl(firstElement);
			if (encapsulatingItemsControl != null)
			{
				return encapsulatingItemsControl.FocusItem(NewItemInfo(firstItemOnCurrentPage, firstElement), itemNavigateArgs);
			}
		}
		return false;
	}

	internal void NavigateToStart(ItemNavigateArgs itemNavigateArgs)
	{
		NavigateToStartInternal(itemNavigateArgs, shouldFocus: true, out var _);
	}

	internal bool NavigateToStartInternal(ItemNavigateArgs itemNavigateArgs, bool shouldFocus, out FrameworkElement container)
	{
		container = null;
		if (ItemsHost != null)
		{
			if (ScrollHost != null)
			{
				double num = 0.0;
				double num2 = 0.0;
				bool flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
				do
				{
					num = ScrollHost.HorizontalOffset;
					num2 = ScrollHost.VerticalOffset;
					if (flag)
					{
						ScrollHost.ScrollToLeftEnd();
					}
					else
					{
						ScrollHost.ScrollToTop();
					}
					ItemsHost.UpdateLayout();
				}
				while (!DoubleUtil.AreClose(num, ScrollHost.HorizontalOffset) || !DoubleUtil.AreClose(num2, ScrollHost.VerticalOffset));
			}
			FrameworkElement startingElement = FindEndFocusableLeafContainer(ItemsHost, last: false);
			FrameworkElement firstElement;
			object firstItemOnCurrentPage = GetFirstItemOnCurrentPage(startingElement, FocusNavigationDirection.Up, out firstElement);
			container = firstElement;
			if (shouldFocus)
			{
				if (firstElement != null && (firstItemOnCurrentPage == DependencyProperty.UnsetValue || firstItemOnCurrentPage is CollectionViewGroupInternal))
				{
					return firstElement.Focus();
				}
				ItemsControl encapsulatingItemsControl = GetEncapsulatingItemsControl(firstElement);
				if (encapsulatingItemsControl != null)
				{
					return encapsulatingItemsControl.FocusItem(NewItemInfo(firstItemOnCurrentPage, firstElement), itemNavigateArgs);
				}
			}
		}
		return false;
	}

	internal void NavigateToEnd(ItemNavigateArgs itemNavigateArgs)
	{
		NavigateToEndInternal(itemNavigateArgs, shouldFocus: true, out var _);
	}

	internal bool NavigateToEndInternal(ItemNavigateArgs itemNavigateArgs, bool shouldFocus, out FrameworkElement container)
	{
		container = null;
		if (ItemsHost != null)
		{
			if (ScrollHost != null)
			{
				double num = 0.0;
				double num2 = 0.0;
				bool flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
				do
				{
					num = ScrollHost.HorizontalOffset;
					num2 = ScrollHost.VerticalOffset;
					if (flag)
					{
						ScrollHost.ScrollToRightEnd();
					}
					else
					{
						ScrollHost.ScrollToBottom();
					}
					ItemsHost.UpdateLayout();
				}
				while (!DoubleUtil.AreClose(num, ScrollHost.HorizontalOffset) || !DoubleUtil.AreClose(num2, ScrollHost.VerticalOffset));
			}
			FrameworkElement startingElement = FindEndFocusableLeafContainer(ItemsHost, last: true);
			FrameworkElement firstElement;
			object firstItemOnCurrentPage = GetFirstItemOnCurrentPage(startingElement, FocusNavigationDirection.Down, out firstElement);
			container = firstElement;
			if (shouldFocus)
			{
				if (firstElement != null && (firstItemOnCurrentPage == DependencyProperty.UnsetValue || firstItemOnCurrentPage is CollectionViewGroupInternal))
				{
					return firstElement.Focus();
				}
				ItemsControl encapsulatingItemsControl = GetEncapsulatingItemsControl(firstElement);
				if (encapsulatingItemsControl != null)
				{
					return encapsulatingItemsControl.FocusItem(NewItemInfo(firstItemOnCurrentPage, firstElement), itemNavigateArgs);
				}
			}
		}
		return false;
	}

	private FrameworkElement FindEndFocusableLeafContainer(Panel itemsHost, bool last)
	{
		if (itemsHost == null)
		{
			return null;
		}
		UIElementCollection children = itemsHost.Children;
		if (children != null)
		{
			int count = children.Count;
			int i = (last ? (count - 1) : 0);
			for (int num = ((!last) ? 1 : (-1)); i >= 0 && i < count; i += num)
			{
				if (!(children[i] is FrameworkElement frameworkElement))
				{
					continue;
				}
				ItemsControl itemsControl = frameworkElement as ItemsControl;
				FrameworkElement frameworkElement2 = null;
				if (itemsControl != null)
				{
					if (itemsControl.ItemsHost != null)
					{
						frameworkElement2 = FindEndFocusableLeafContainer(itemsControl.ItemsHost, last);
					}
				}
				else if (frameworkElement is GroupItem { ItemsHost: not null } groupItem)
				{
					frameworkElement2 = FindEndFocusableLeafContainer(groupItem.ItemsHost, last);
				}
				if (frameworkElement2 != null)
				{
					return frameworkElement2;
				}
				if (FrameworkElement.KeyboardNavigation.IsFocusableInternal(frameworkElement))
				{
					return frameworkElement;
				}
			}
		}
		return null;
	}

	internal void NavigateToItem(ItemInfo info, ItemNavigateArgs itemNavigateArgs, bool alwaysAtTopOfViewport = false)
	{
		if (info != null)
		{
			NavigateToItem(info.Item, info.Index, itemNavigateArgs, alwaysAtTopOfViewport);
		}
	}

	internal void NavigateToItem(object item, ItemNavigateArgs itemNavigateArgs)
	{
		NavigateToItem(item, -1, itemNavigateArgs, alwaysAtTopOfViewport: false);
	}

	internal void NavigateToItem(object item, int itemIndex, ItemNavigateArgs itemNavigateArgs)
	{
		NavigateToItem(item, itemIndex, itemNavigateArgs, alwaysAtTopOfViewport: false);
	}

	internal void NavigateToItem(object item, ItemNavigateArgs itemNavigateArgs, bool alwaysAtTopOfViewport)
	{
		NavigateToItem(item, -1, itemNavigateArgs, alwaysAtTopOfViewport);
	}

	private void NavigateToItem(object item, int elementIndex, ItemNavigateArgs itemNavigateArgs, bool alwaysAtTopOfViewport)
	{
		if (item == DependencyProperty.UnsetValue)
		{
			return;
		}
		if (elementIndex == -1)
		{
			elementIndex = Items.IndexOf(item);
			if (elementIndex == -1)
			{
				return;
			}
		}
		bool flag = false;
		if (ItemsHost != null)
		{
			flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
		}
		FocusNavigationDirection direction = (flag ? FocusNavigationDirection.Right : FocusNavigationDirection.Down);
		MakeVisible(elementIndex, direction, alwaysAtTopOfViewport, out var container);
		FocusItem(NewItemInfo(item, container), itemNavigateArgs);
	}

	private object FindFocusable(int startIndex, int direction, out int foundIndex, out FrameworkElement foundContainer)
	{
		if (HasItems)
		{
			int count = Items.Count;
			while (startIndex >= 0 && startIndex < count)
			{
				FrameworkElement frameworkElement = ItemContainerGenerator.ContainerFromIndex(startIndex) as FrameworkElement;
				if (frameworkElement == null || Keyboard.IsFocusable(frameworkElement))
				{
					foundIndex = startIndex;
					foundContainer = frameworkElement;
					return Items[startIndex];
				}
				startIndex += direction;
			}
		}
		foundIndex = -1;
		foundContainer = null;
		return null;
	}

	private void AdjustOffsetToAlignWithEdge(FrameworkElement element, FocusNavigationDirection direction)
	{
		if (VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item)
		{
			return;
		}
		ScrollViewer scrollHost = ScrollHost;
		FrameworkElement viewportElement = GetViewportElement();
		element = TryGetTreeViewItemHeader(element) as FrameworkElement;
		Rect rect = new Rect(default(Point), element.RenderSize);
		rect = element.TransformToAncestor(viewportElement).TransformBounds(rect);
		bool flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
		switch (direction)
		{
		case FocusNavigationDirection.Down:
			if (flag)
			{
				scrollHost.ScrollToHorizontalOffset(scrollHost.HorizontalOffset - scrollHost.ViewportWidth + rect.Right);
			}
			else
			{
				scrollHost.ScrollToVerticalOffset(scrollHost.VerticalOffset - scrollHost.ViewportHeight + rect.Bottom);
			}
			break;
		case FocusNavigationDirection.Up:
			if (flag)
			{
				scrollHost.ScrollToHorizontalOffset(scrollHost.HorizontalOffset + rect.Left);
			}
			else
			{
				scrollHost.ScrollToVerticalOffset(scrollHost.VerticalOffset + rect.Top);
			}
			break;
		}
	}

	private void MakeVisible(int index, FocusNavigationDirection direction, bool alwaysAtTopOfViewport, out FrameworkElement container)
	{
		container = null;
		if (index >= 0)
		{
			container = ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
			if (container == null && ItemsHost is VirtualizingPanel virtualizingPanel)
			{
				virtualizingPanel.BringIndexIntoView(index);
				UpdateLayout();
				container = ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
			}
			MakeVisible(container, direction, alwaysAtTopOfViewport);
		}
	}

	private void MakeVisible(ItemInfo info, FocusNavigationDirection direction, out FrameworkElement container)
	{
		if (info != null)
		{
			MakeVisible(info.Index, direction, alwaysAtTopOfViewport: false, out container);
			info.Container = container;
		}
		else
		{
			MakeVisible(-1, direction, alwaysAtTopOfViewport: false, out container);
		}
	}

	internal void MakeVisible(FrameworkElement container, FocusNavigationDirection direction, bool alwaysAtTopOfViewport)
	{
		if (ScrollHost == null || ItemsHost == null)
		{
			return;
		}
		FrameworkElement viewportElement = GetViewportElement();
		while (container != null && !IsOnCurrentPage(viewportElement, container, direction, fullyVisible: false))
		{
			double horizontalOffset = ScrollHost.HorizontalOffset;
			double verticalOffset = ScrollHost.VerticalOffset;
			container.BringIntoView();
			ItemsHost.UpdateLayout();
			if (DoubleUtil.AreClose(horizontalOffset, ScrollHost.HorizontalOffset) && DoubleUtil.AreClose(verticalOffset, ScrollHost.VerticalOffset))
			{
				break;
			}
		}
		if (!(container != null && alwaysAtTopOfViewport))
		{
			return;
		}
		bool flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
		GetFirstItemOnCurrentPage(container, FocusNavigationDirection.Up, out var firstElement);
		while (firstElement != container)
		{
			double horizontalOffset2 = ScrollHost.HorizontalOffset;
			double verticalOffset = ScrollHost.VerticalOffset;
			if (flag)
			{
				ScrollHost.LineRight();
			}
			else
			{
				ScrollHost.LineDown();
			}
			ScrollHost.UpdateLayout();
			if (!DoubleUtil.AreClose(horizontalOffset2, ScrollHost.HorizontalOffset) || !DoubleUtil.AreClose(verticalOffset, ScrollHost.VerticalOffset))
			{
				GetFirstItemOnCurrentPage(container, FocusNavigationDirection.Up, out firstElement);
				continue;
			}
			break;
		}
	}

	private bool NavigateToFirstItemOnCurrentPage(object startingItem, FocusNavigationDirection direction, ItemNavigateArgs itemNavigateArgs, bool shouldFocus, out FrameworkElement container)
	{
		object firstItemOnCurrentPage = GetFirstItemOnCurrentPage(ItemContainerGenerator.ContainerFromItem(startingItem) as FrameworkElement, direction, out container);
		if (firstItemOnCurrentPage != DependencyProperty.UnsetValue && shouldFocus)
		{
			return FocusItem(NewItemInfo(firstItemOnCurrentPage, container), itemNavigateArgs);
		}
		return false;
	}

	private object GetFirstItemOnCurrentPage(FrameworkElement startingElement, FocusNavigationDirection direction, out FrameworkElement firstElement)
	{
		bool flag = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Horizontal;
		bool flag2 = ItemsHost.HasLogicalOrientation && ItemsHost.LogicalOrientation == Orientation.Vertical;
		if (ScrollHost != null && ScrollHost.CanContentScroll && VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item && !(this is TreeView) && !IsGrouping)
		{
			int foundIndex = -1;
			if (flag2)
			{
				if (direction == FocusNavigationDirection.Up)
				{
					return FindFocusable((int)ScrollHost.VerticalOffset, 1, out foundIndex, out firstElement);
				}
				return FindFocusable((int)(ScrollHost.VerticalOffset + Math.Max(ScrollHost.ViewportHeight - 1.0, 0.0)), -1, out foundIndex, out firstElement);
			}
			if (flag)
			{
				if (direction == FocusNavigationDirection.Up)
				{
					return FindFocusable((int)ScrollHost.HorizontalOffset, 1, out foundIndex, out firstElement);
				}
				return FindFocusable((int)(ScrollHost.HorizontalOffset + Math.Max(ScrollHost.ViewportWidth - 1.0, 0.0)), -1, out foundIndex, out firstElement);
			}
		}
		if (startingElement != null)
		{
			FrameworkElement frameworkElement = startingElement;
			if (flag)
			{
				switch (direction)
				{
				case FocusNavigationDirection.Up:
					direction = FocusNavigationDirection.Left;
					break;
				case FocusNavigationDirection.Down:
					direction = FocusNavigationDirection.Right;
					break;
				}
			}
			FrameworkElement viewportElement = GetViewportElement();
			bool treeViewNavigation = this is TreeView;
			frameworkElement = KeyboardNavigation.Current.PredictFocusedElementAtViewportEdge(startingElement, direction, treeViewNavigation, viewportElement, viewportElement) as FrameworkElement;
			object obj = null;
			firstElement = null;
			if (frameworkElement != null)
			{
				obj = GetEncapsulatingItem(frameworkElement, out firstElement);
			}
			if (frameworkElement == null || obj == DependencyProperty.UnsetValue)
			{
				ElementViewportPosition elementViewportPosition = GetElementViewportPosition(viewportElement, startingElement, direction, fullyVisible: false);
				if (elementViewportPosition == ElementViewportPosition.CompletelyInViewport || elementViewportPosition == ElementViewportPosition.PartiallyInViewport)
				{
					frameworkElement = startingElement;
					obj = GetEncapsulatingItem(frameworkElement, out firstElement);
				}
			}
			if (obj != null && obj is CollectionViewGroupInternal)
			{
				firstElement = frameworkElement;
			}
			return obj;
		}
		firstElement = null;
		return null;
	}

	internal FrameworkElement GetViewportElement()
	{
		FrameworkElement frameworkElement = ScrollHost;
		if (frameworkElement == null)
		{
			frameworkElement = ItemsHost;
		}
		else if (frameworkElement.GetTemplateChild("PART_ScrollContentPresenter") is ScrollContentPresenter scrollContentPresenter)
		{
			frameworkElement = scrollContentPresenter;
		}
		return frameworkElement;
	}

	private bool IsOnCurrentPage(object item, FocusNavigationDirection axis)
	{
		if (!(ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement element))
		{
			return false;
		}
		return GetElementViewportPosition(GetViewportElement(), element, axis, fullyVisible: false) == ElementViewportPosition.CompletelyInViewport;
	}

	private bool IsOnCurrentPage(FrameworkElement element, FocusNavigationDirection axis)
	{
		return GetElementViewportPosition(GetViewportElement(), element, axis, fullyVisible: false) == ElementViewportPosition.CompletelyInViewport;
	}

	private bool IsOnCurrentPage(FrameworkElement viewPort, FrameworkElement element, FocusNavigationDirection axis, bool fullyVisible)
	{
		return GetElementViewportPosition(viewPort, element, axis, fullyVisible) == ElementViewportPosition.CompletelyInViewport;
	}

	internal static ElementViewportPosition GetElementViewportPosition(FrameworkElement viewPort, UIElement element, FocusNavigationDirection axis, bool fullyVisible)
	{
		Rect elementRect;
		return GetElementViewportPosition(viewPort, element, axis, fullyVisible, out elementRect);
	}

	internal static ElementViewportPosition GetElementViewportPosition(FrameworkElement viewPort, UIElement element, FocusNavigationDirection axis, bool fullyVisible, out Rect elementRect)
	{
		return GetElementViewportPosition(viewPort, element, axis, fullyVisible, ignorePerpendicularAxis: false, out elementRect);
	}

	internal static ElementViewportPosition GetElementViewportPosition(FrameworkElement viewPort, UIElement element, FocusNavigationDirection axis, bool fullyVisible, bool ignorePerpendicularAxis, out Rect elementRect)
	{
		elementRect = Rect.Empty;
		if (viewPort == null)
		{
			return ElementViewportPosition.None;
		}
		if (element == null || !viewPort.IsAncestorOf(element))
		{
			return ElementViewportPosition.None;
		}
		Rect viewportRect = new Rect(default(Point), viewPort.RenderSize);
		Rect rect = new Rect(default(Point), element.RenderSize);
		rect = CorrectCatastrophicCancellation(element.TransformToAncestor(viewPort)).TransformBounds(rect);
		bool flag = axis == FocusNavigationDirection.Up || axis == FocusNavigationDirection.Down;
		bool flag2 = axis == FocusNavigationDirection.Left || axis == FocusNavigationDirection.Right;
		elementRect = rect;
		if (ignorePerpendicularAxis)
		{
			if (flag)
			{
				viewportRect = new Rect(double.NegativeInfinity, viewportRect.Top, double.PositiveInfinity, viewportRect.Height);
			}
			else if (flag2)
			{
				viewportRect = new Rect(viewportRect.Left, double.NegativeInfinity, viewportRect.Width, double.PositiveInfinity);
			}
		}
		if (fullyVisible)
		{
			if (viewportRect.Contains(rect))
			{
				return ElementViewportPosition.CompletelyInViewport;
			}
		}
		else if (flag)
		{
			if (DoubleUtil.LessThanOrClose(viewportRect.Top, rect.Top) && DoubleUtil.LessThanOrClose(rect.Bottom, viewportRect.Bottom))
			{
				return ElementViewportPosition.CompletelyInViewport;
			}
		}
		else if (flag2 && DoubleUtil.LessThanOrClose(viewportRect.Left, rect.Left) && DoubleUtil.LessThanOrClose(rect.Right, viewportRect.Right))
		{
			return ElementViewportPosition.CompletelyInViewport;
		}
		if (ElementIntersectsViewport(viewportRect, rect))
		{
			return ElementViewportPosition.PartiallyInViewport;
		}
		if ((flag && DoubleUtil.LessThanOrClose(rect.Bottom, viewportRect.Top)) || (flag2 && DoubleUtil.LessThanOrClose(rect.Right, viewportRect.Left)))
		{
			return ElementViewportPosition.BeforeViewport;
		}
		if ((flag && DoubleUtil.LessThanOrClose(viewportRect.Bottom, rect.Top)) || (flag2 && DoubleUtil.LessThanOrClose(viewportRect.Right, rect.Left)))
		{
			return ElementViewportPosition.AfterViewport;
		}
		return ElementViewportPosition.None;
	}

	internal static ElementViewportPosition GetElementViewportPosition(FrameworkElement viewPort, UIElement element, FocusNavigationDirection axis, bool fullyVisible, bool ignorePerpendicularAxis, out Rect elementRect, out Rect layoutRect)
	{
		ElementViewportPosition elementViewportPosition = GetElementViewportPosition(viewPort, element, axis, fullyVisible, ignorePerpendicularAxis: false, out elementRect);
		if (elementViewportPosition == ElementViewportPosition.None)
		{
			layoutRect = Rect.Empty;
			return elementViewportPosition;
		}
		Visual visual = VisualTreeHelper.GetParent(element) as Visual;
		layoutRect = CorrectCatastrophicCancellation(visual.TransformToAncestor(viewPort)).TransformBounds(element.PreviousArrangeRect);
		return elementViewportPosition;
	}

	private static GeneralTransform CorrectCatastrophicCancellation(GeneralTransform transform)
	{
		if (transform is MatrixTransform matrixTransform)
		{
			bool flag = false;
			Matrix matrix = matrixTransform.Matrix;
			if (matrix.OffsetX != 0.0 && LayoutDoubleUtil.AreClose(matrix.OffsetX, 0.0))
			{
				matrix.OffsetX = 0.0;
				flag = true;
			}
			if (matrix.OffsetY != 0.0 && LayoutDoubleUtil.AreClose(matrix.OffsetY, 0.0))
			{
				matrix.OffsetY = 0.0;
				flag = true;
			}
			if (flag)
			{
				transform = new MatrixTransform(matrix);
			}
		}
		return transform;
	}

	private static bool ElementIntersectsViewport(Rect viewportRect, Rect elementRect)
	{
		if (viewportRect.IsEmpty || elementRect.IsEmpty)
		{
			return false;
		}
		if (DoubleUtil.LessThan(elementRect.Right, viewportRect.Left) || LayoutDoubleUtil.AreClose(elementRect.Right, viewportRect.Left) || DoubleUtil.GreaterThan(elementRect.Left, viewportRect.Right) || LayoutDoubleUtil.AreClose(elementRect.Left, viewportRect.Right) || DoubleUtil.LessThan(elementRect.Bottom, viewportRect.Top) || LayoutDoubleUtil.AreClose(elementRect.Bottom, viewportRect.Top) || DoubleUtil.GreaterThan(elementRect.Top, viewportRect.Bottom) || LayoutDoubleUtil.AreClose(elementRect.Top, viewportRect.Bottom))
		{
			return false;
		}
		return true;
	}

	private bool IsInDirectionForLineNavigation(Rect fromRect, Rect toRect, FocusNavigationDirection direction, bool isHorizontal)
	{
		switch (direction)
		{
		case FocusNavigationDirection.Down:
			if (isHorizontal)
			{
				return DoubleUtil.GreaterThanOrClose(toRect.Left, fromRect.Left);
			}
			return DoubleUtil.GreaterThanOrClose(toRect.Top, fromRect.Top);
		case FocusNavigationDirection.Up:
			if (isHorizontal)
			{
				return DoubleUtil.LessThanOrClose(toRect.Right, fromRect.Right);
			}
			return DoubleUtil.LessThanOrClose(toRect.Bottom, fromRect.Bottom);
		default:
			return false;
		}
	}

	private static void OnGotFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		ItemsControl itemsControl = (ItemsControl)sender;
		if (e.OriginalSource is UIElement uIElement && uIElement != itemsControl)
		{
			object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(uIElement);
			if (obj != DependencyProperty.UnsetValue)
			{
				itemsControl._focusedInfo = itemsControl.NewItemInfo(obj, uIElement);
			}
			else if (itemsControl._focusedInfo != null && (!(itemsControl._focusedInfo.Container is UIElement ancestor) || !Helper.IsAnyAncestorOf(ancestor, uIElement)))
			{
				itemsControl._focusedInfo = null;
			}
		}
	}

	internal virtual bool FocusItem(ItemInfo info, ItemNavigateArgs itemNavigateArgs)
	{
		object item = info.Item;
		bool result = false;
		if (item != null && info.Container is UIElement uIElement)
		{
			result = uIElement.Focus();
		}
		if (itemNavigateArgs.DeviceUsed is KeyboardDevice)
		{
			KeyboardNavigation.ShowFocusVisual();
		}
		return result;
	}

	internal void DoAutoScroll()
	{
		DoAutoScroll(FocusedInfo);
	}

	internal void DoAutoScroll(ItemInfo startingInfo)
	{
		FrameworkElement frameworkElement = ((ScrollHost != null) ? ((FrameworkElement)ScrollHost) : ((FrameworkElement)ItemsHost));
		if (frameworkElement == null)
		{
			return;
		}
		Point position = Mouse.GetPosition(frameworkElement);
		Rect rect = new Rect(default(Point), frameworkElement.RenderSize);
		bool flag = false;
		if (position.Y < rect.Top)
		{
			NavigateByLine(startingInfo, FocusNavigationDirection.Up, new ItemNavigateArgs(Mouse.PrimaryDevice, Keyboard.Modifiers));
			flag = startingInfo != FocusedInfo;
		}
		else if (position.Y >= rect.Bottom)
		{
			NavigateByLine(startingInfo, FocusNavigationDirection.Down, new ItemNavigateArgs(Mouse.PrimaryDevice, Keyboard.Modifiers));
			flag = startingInfo != FocusedInfo;
		}
		if (flag)
		{
			return;
		}
		if (position.X < rect.Left)
		{
			FocusNavigationDirection direction = FocusNavigationDirection.Left;
			if (IsRTL(frameworkElement))
			{
				direction = FocusNavigationDirection.Right;
			}
			NavigateByLine(startingInfo, direction, new ItemNavigateArgs(Mouse.PrimaryDevice, Keyboard.Modifiers));
		}
		else if (position.X >= rect.Right)
		{
			FocusNavigationDirection direction2 = FocusNavigationDirection.Right;
			if (IsRTL(frameworkElement))
			{
				direction2 = FocusNavigationDirection.Left;
			}
			NavigateByLine(startingInfo, direction2, new ItemNavigateArgs(Mouse.PrimaryDevice, Keyboard.Modifiers));
		}
	}

	private bool IsRTL(FrameworkElement element)
	{
		return element.FlowDirection == FlowDirection.RightToLeft;
	}

	private static ItemsControl GetEncapsulatingItemsControl(FrameworkElement element)
	{
		while (element != null)
		{
			ItemsControl itemsControl = ItemsControlFromItemContainer(element);
			if (itemsControl != null)
			{
				return itemsControl;
			}
			element = VisualTreeHelper.GetParent(element) as FrameworkElement;
		}
		return null;
	}

	private static object GetEncapsulatingItem(FrameworkElement element, out FrameworkElement container)
	{
		ItemsControl itemsControl = null;
		return GetEncapsulatingItem(element, out container, out itemsControl);
	}

	private static object GetEncapsulatingItem(FrameworkElement element, out FrameworkElement container, out ItemsControl itemsControl)
	{
		object obj = DependencyProperty.UnsetValue;
		itemsControl = null;
		while (element != null)
		{
			itemsControl = ItemsControlFromItemContainer(element);
			if (itemsControl != null)
			{
				obj = itemsControl.ItemContainerGenerator.ItemFromContainer(element);
				if (obj != DependencyProperty.UnsetValue)
				{
					break;
				}
			}
			element = VisualTreeHelper.GetParent(element) as FrameworkElement;
		}
		container = element;
		return obj;
	}

	internal static DependencyObject TryGetTreeViewItemHeader(DependencyObject element)
	{
		if (element is TreeViewItem treeViewItem)
		{
			return treeViewItem.TryGetHeaderElement();
		}
		return element;
	}

	private void ApplyItemContainerStyle(DependencyObject container, object item)
	{
		FrameworkObject frameworkObject = new FrameworkObject(container);
		if (!frameworkObject.IsStyleSetFromGenerator && container.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue)
		{
			return;
		}
		Style style = ItemContainerStyle;
		if (style == null && ItemContainerStyleSelector != null)
		{
			style = ItemContainerStyleSelector.SelectStyle(item, container);
		}
		if (style != null)
		{
			if (!style.TargetType.IsInstanceOfType(container))
			{
				throw new InvalidOperationException(SR.Format(SR.StyleForWrongType, style.TargetType.Name, container.GetType().Name));
			}
			frameworkObject.Style = style;
			frameworkObject.IsStyleSetFromGenerator = true;
		}
		else if (frameworkObject.IsStyleSetFromGenerator)
		{
			frameworkObject.IsStyleSetFromGenerator = false;
			container.ClearValue(FrameworkElement.StyleProperty);
		}
	}

	private void RemoveItemContainerStyle(DependencyObject container)
	{
		if (new FrameworkObject(container).IsStyleSetFromGenerator)
		{
			container.ClearValue(FrameworkElement.StyleProperty);
		}
	}

	internal object GetItemOrContainerFromContainer(DependencyObject container)
	{
		object obj = ItemContainerGenerator.ItemFromContainer(container);
		if (obj == DependencyProperty.UnsetValue && ItemsControlFromItemContainer(container) == this && ((IGeneratorHost)this).IsItemItsOwnContainer((object)container))
		{
			obj = container;
		}
		return obj;
	}

	internal static bool EqualsEx(object o1, object o2)
	{
		try
		{
			return object.Equals(o1, o2);
		}
		catch (InvalidCastException)
		{
			return false;
		}
	}

	internal ItemInfo NewItemInfo(object item, DependencyObject container = null, int index = -1)
	{
		return new ItemInfo(item, container, index).Refresh(ItemContainerGenerator);
	}

	internal ItemInfo ItemInfoFromContainer(DependencyObject container)
	{
		return NewItemInfo(ItemContainerGenerator.ItemFromContainer(container), container, ItemContainerGenerator.IndexFromContainer(container));
	}

	internal ItemInfo ItemInfoFromIndex(int index)
	{
		if (index < 0)
		{
			return null;
		}
		return NewItemInfo(Items[index], ItemContainerGenerator.ContainerFromIndex(index), index);
	}

	internal ItemInfo NewUnresolvedItemInfo(object item)
	{
		return new ItemInfo(item, ItemInfo.UnresolvedContainer);
	}

	internal DependencyObject ContainerFromItemInfo(ItemInfo info)
	{
		DependencyObject dependencyObject = info.Container;
		if (dependencyObject == null)
		{
			dependencyObject = ((info.Index < 0) ? ItemContainerGenerator.ContainerFromItem(info.Item) : (info.Container = ItemContainerGenerator.ContainerFromIndex(info.Index)));
		}
		return dependencyObject;
	}

	internal void AdjustItemInfoAfterGeneratorChange(ItemInfo info)
	{
		if (info != null)
		{
			ItemInfo[] list = new ItemInfo[1] { info };
			AdjustItemInfosAfterGeneratorChange(list, claimUniqueContainer: false);
		}
	}

	internal void AdjustItemInfosAfterGeneratorChange(IEnumerable<ItemInfo> list, bool claimUniqueContainer)
	{
		bool flag = false;
		foreach (ItemInfo item2 in list)
		{
			DependencyObject container = item2.Container;
			if (container == null)
			{
				flag = true;
			}
			else if (item2.IsRemoved || !EqualsEx(item2.Item, container.ReadLocalValue(ItemContainerGenerator.ItemForItemContainerProperty)))
			{
				item2.Container = null;
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		List<DependencyObject> list2 = new List<DependencyObject>();
		if (claimUniqueContainer)
		{
			foreach (ItemInfo item3 in list)
			{
				DependencyObject container2 = item3.Container;
				if (container2 != null)
				{
					list2.Add(container2);
				}
			}
		}
		foreach (ItemInfo item4 in list)
		{
			DependencyObject container3 = item4.Container;
			if (container3 != null)
			{
				continue;
			}
			int itemIndex = item4.Index;
			if (itemIndex >= 0)
			{
				container3 = ItemContainerGenerator.ContainerFromIndex(itemIndex);
			}
			else
			{
				object item = item4.Item;
				ItemContainerGenerator.FindItem(((object item, List<DependencyObject> claimedContainers) state, object o, DependencyObject d) => EqualsEx(o, state.item) && !state.claimedContainers.Contains(d), (item, list2), out container3, out itemIndex);
			}
			if (container3 != null)
			{
				item4.Container = container3;
				item4.Index = itemIndex;
				if (claimUniqueContainer)
				{
					list2.Add(container3);
				}
			}
		}
	}

	internal void AdjustItemInfo(NotifyCollectionChangedEventArgs e, ItemInfo info)
	{
		if (info != null)
		{
			ItemInfo[] list = new ItemInfo[1] { info };
			AdjustItemInfos(e, list);
		}
	}

	internal void AdjustItemInfos(NotifyCollectionChangedEventArgs e, IEnumerable<ItemInfo> list)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
		{
			foreach (ItemInfo item in list)
			{
				int index3 = item.Index;
				if (index3 >= e.NewStartingIndex)
				{
					item.Index = index3 + 1;
				}
			}
			break;
		}
		case NotifyCollectionChangedAction.Remove:
		{
			foreach (ItemInfo item2 in list)
			{
				int index2 = item2.Index;
				if (index2 > e.OldStartingIndex)
				{
					item2.Index = index2 - 1;
				}
				else if (index2 == e.OldStartingIndex)
				{
					item2.Index = -1;
				}
			}
			break;
		}
		case NotifyCollectionChangedAction.Move:
		{
			int num;
			int num2;
			int num3;
			if (e.OldStartingIndex < e.NewStartingIndex)
			{
				num = e.OldStartingIndex + 1;
				num2 = e.NewStartingIndex;
				num3 = -1;
			}
			else
			{
				num = e.NewStartingIndex;
				num2 = e.OldStartingIndex - 1;
				num3 = 1;
			}
			{
				foreach (ItemInfo item3 in list)
				{
					int index = item3.Index;
					if (index == e.OldStartingIndex)
					{
						item3.Index = e.NewStartingIndex;
					}
					else if (num <= index && index <= num2)
					{
						item3.Index = index + num3;
					}
				}
				break;
			}
		}
		case NotifyCollectionChangedAction.Reset:
		{
			foreach (ItemInfo item4 in list)
			{
				item4.Index = -1;
				item4.Container = null;
			}
			break;
		}
		case NotifyCollectionChangedAction.Replace:
			break;
		}
	}

	internal ItemInfo LeaseItemInfo(ItemInfo info, bool ensureIndex = false)
	{
		if (info.Index < 0)
		{
			info = NewItemInfo(info.Item);
			if (ensureIndex && info.Index < 0)
			{
				info.Index = Items.IndexOf(info.Item);
			}
		}
		return info;
	}

	internal void RefreshItemInfo(ItemInfo info)
	{
		if (info != null)
		{
			info.Refresh(ItemContainerGenerator);
		}
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

	/// <summary>Provides a string representation of the <see cref="T:System.Windows.Controls.ItemsControl" /> object.</summary>
	/// <returns>The string representation of the object.</returns>
	public override string ToString()
	{
		int num = (HasItems ? Items.Count : 0);
		return SR.Format(SR.ToStringFormatString_ItemsControl, GetType(), num);
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		if (!AccessibilitySwitches.ItemsControlDoesNotSupportAutomation)
		{
			return new ItemsControlWrapperAutomationPeer(this);
		}
		return null;
	}

	internal override AutomationPeer OnCreateAutomationPeerInternal()
	{
		return new ItemsControlWrapperAutomationPeer(this);
	}
}
