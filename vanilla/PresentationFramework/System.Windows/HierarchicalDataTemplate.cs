using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows;

/// <summary>Represents a <see cref="T:System.Windows.DataTemplate" /> that supports <see cref="T:System.Windows.Controls.HeaderedItemsControl" />, such as <see cref="T:System.Windows.Controls.TreeViewItem" /> or <see cref="T:System.Windows.Controls.MenuItem" />.</summary>
public class HierarchicalDataTemplate : DataTemplate
{
	private BindingBase _itemsSourceBinding;

	private DataTemplate _itemTemplate;

	private DataTemplateSelector _itemTemplateSelector;

	private Style _itemContainerStyle;

	private StyleSelector _itemContainerStyleSelector;

	private string _itemStringFormat;

	private int _alternationCount;

	private BindingGroup _itemBindingGroup;

	private bool _itemTemplateSet;

	private bool _itemTemplateSelectorSet;

	private bool _itemContainerStyleSet;

	private bool _itemContainerStyleSelectorSet;

	private bool _itemStringFormatSet;

	private bool _alternationCountSet;

	private bool _itemBindingGroupSet;

	/// <summary>Gets or sets the binding for this data template, which indicates where to find the collection that represents the next level in the data hierarchy.</summary>
	/// <returns>The default is null.</returns>
	public BindingBase ItemsSource
	{
		get
		{
			return _itemsSourceBinding;
		}
		set
		{
			CheckSealed();
			_itemsSourceBinding = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.DataTemplate" /> to apply to the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> property on a generated <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> (such as a <see cref="T:System.Windows.Controls.MenuItem" /> or a <see cref="T:System.Windows.Controls.TreeViewItem" />), to indicate how to display items from the next level in the data hierarchy.</summary>
	/// <returns>The <see cref="T:System.Windows.DataTemplate" /> to apply to the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplate" /> property on a generated <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> (such as a <see cref="T:System.Windows.Controls.MenuItem" /> or a <see cref="T:System.Windows.Controls.TreeViewItem" />), to indicate how to display items from the next level in the data hierarchy.</returns>
	public DataTemplate ItemTemplate
	{
		get
		{
			return _itemTemplate;
		}
		set
		{
			CheckSealed();
			_itemTemplate = value;
			_itemTemplateSet = true;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.DataTemplateSelector" /> to apply to the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> property on a generated <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> (such as a <see cref="T:System.Windows.Controls.MenuItem" /> or a <see cref="T:System.Windows.Controls.TreeViewItem" />), to indicate how to select a template to display items from the next level in the data hierarchy.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataTemplateSelector" /> object to apply to the <see cref="P:System.Windows.Controls.ItemsControl.ItemTemplateSelector" /> property on a generated <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> (such as a <see cref="T:System.Windows.Controls.MenuItem" /> or a <see cref="T:System.Windows.Controls.TreeViewItem" />), to indicate how to select a template to display items from the next level in the data hierarchy.</returns>
	public DataTemplateSelector ItemTemplateSelector
	{
		get
		{
			return _itemTemplateSelector;
		}
		set
		{
			CheckSealed();
			_itemTemplateSelector = value;
			_itemTemplateSelectorSet = true;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Style" /> that is applied to the item container for each child item.</summary>
	/// <returns>The <see cref="T:System.Windows.Style" /> that is applied to the item container for each child item.</returns>
	public Style ItemContainerStyle
	{
		get
		{
			return _itemContainerStyle;
		}
		set
		{
			CheckSealed();
			_itemContainerStyle = value;
			_itemContainerStyleSet = true;
		}
	}

	/// <summary>Gets or sets custom style-selection logic for a style that can be applied to each item container. </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.StyleSelector" /> that chooses which style to use as the <see cref="P:System.Windows.HierarchicalDataTemplate.ItemContainerStyle" />. The default is null.</returns>
	public StyleSelector ItemContainerStyleSelector
	{
		get
		{
			return _itemContainerStyleSelector;
		}
		set
		{
			CheckSealed();
			_itemContainerStyleSelector = value;
			_itemContainerStyleSelectorSet = true;
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the items in the next level in the data hierarchy if they are displayed as strings.</summary>
	/// <returns>A composite string that specifies how to format the items in the next level of the data hierarchy if they are displayed as strings.</returns>
	public string ItemStringFormat
	{
		get
		{
			return _itemStringFormat;
		}
		set
		{
			CheckSealed();
			_itemStringFormat = value;
			_itemStringFormatSet = true;
		}
	}

	/// <summary>Gets or sets the number of alternating item containers for the child items.</summary>
	/// <returns>The number of alternating item containers for the next level of items.</returns>
	public int AlternationCount
	{
		get
		{
			return _alternationCount;
		}
		set
		{
			CheckSealed();
			_alternationCount = value;
			_alternationCountSet = true;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Data.BindingGroup" /> that is copied to each child item.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingGroup" /> that is copied to each child item.</returns>
	public BindingGroup ItemBindingGroup
	{
		get
		{
			return _itemBindingGroup;
		}
		set
		{
			CheckSealed();
			_itemBindingGroup = value;
			_itemBindingGroupSet = true;
		}
	}

	internal bool IsItemTemplateSet => _itemTemplateSet;

	internal bool IsItemTemplateSelectorSet => _itemTemplateSelectorSet;

	internal bool IsItemContainerStyleSet => _itemContainerStyleSet;

	internal bool IsItemContainerStyleSelectorSet => _itemContainerStyleSelectorSet;

	internal bool IsItemStringFormatSet => _itemStringFormatSet;

	internal bool IsAlternationCountSet => _alternationCountSet;

	internal bool IsItemBindingGroupSet => _itemBindingGroupSet;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.HierarchicalDataTemplate" /> class.</summary>
	public HierarchicalDataTemplate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.HierarchicalDataTemplate" /> class with the specified type for which the template is intended.</summary>
	/// <param name="dataType">The type for which this template is intended. </param>
	public HierarchicalDataTemplate(object dataType)
		: base(dataType)
	{
	}
}
