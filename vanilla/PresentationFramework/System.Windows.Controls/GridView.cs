using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a view mode that displays data items in columns for a <see cref="T:System.Windows.Controls.ListView" /> control.</summary>
[StyleTypedProperty(Property = "ColumnHeaderContainerStyle", StyleTargetType = typeof(GridViewColumnHeader))]
[ContentProperty("Columns")]
public class GridView : ViewBase, IAddChild
{
	/// <summary>Identifies the <see cref="F:System.Windows.Controls.GridView.ColumnCollectionProperty" /> attachedproperty. </summary>
	public static readonly DependencyProperty ColumnCollectionProperty = DependencyProperty.RegisterAttached("ColumnCollection", typeof(GridViewColumnCollection), typeof(GridView));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderContainerStyle" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderContainerStyleProperty = DependencyProperty.Register("ColumnHeaderContainerStyle", typeof(Style), typeof(GridView));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderTemplate" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderTemplateProperty = DependencyProperty.Register("ColumnHeaderTemplate", typeof(DataTemplate), typeof(GridView), new FrameworkPropertyMetadata(OnColumnHeaderTemplateChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderTemplateSelector" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty = DependencyProperty.Register("ColumnHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(GridView), new FrameworkPropertyMetadata(OnColumnHeaderTemplateSelectorChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnHeaderStringFormatProperty = DependencyProperty.Register("ColumnHeaderStringFormat", typeof(string), typeof(GridView));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.AllowsColumnReorder" /> dependency property. </summary>
	public static readonly DependencyProperty AllowsColumnReorderProperty = DependencyProperty.Register("AllowsColumnReorder", typeof(bool), typeof(GridView), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderContextMenu" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderContextMenuProperty = DependencyProperty.Register("ColumnHeaderContextMenu", typeof(ContextMenu), typeof(GridView));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridView.ColumnHeaderToolTip" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderToolTipProperty = DependencyProperty.Register("ColumnHeaderToolTip", typeof(object), typeof(GridView));

	private GridViewColumnCollection _columns;

	private GridViewHeaderRowPresenter _gvheaderRP;

	/// <summary>Gets the key that references the style that is defined for the <see cref="T:System.Windows.Controls.ScrollViewer" /> control that encloses the content that is displayed by a <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <returns>A <see cref="T:System.Windows.ResourceKey" /> that references the <see cref="T:System.Windows.Style" /> that is applied to the <see cref="T:System.Windows.Controls.ScrollViewer" /> control for a <see cref="T:System.Windows.Controls.GridView" />. The default value is the style for the <see cref="T:System.Windows.Controls.ScrollViewer" /> object of a <see cref="T:System.Windows.Controls.ListView" /> in the current theme.</returns>
	public static ResourceKey GridViewScrollViewerStyleKey => SystemResourceKey.GridViewScrollViewerStyleKey;

	/// <summary>Gets the key that references the style that is defined for the <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <returns>A <see cref="T:System.Windows.ResourceKey" /> that references the <see cref="T:System.Windows.Style" /> that is applied to the <see cref="T:System.Windows.Controls.GridView" />. The default value is the style for the <see cref="T:System.Windows.Controls.ListView" /> in the current theme.</returns>
	public static ResourceKey GridViewStyleKey => SystemResourceKey.GridViewStyleKey;

	/// <summary>Gets the key that references the style that is defined for each <see cref="T:System.Windows.Controls.ListViewItem" /> in a <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <returns>A <see cref="T:System.Windows.ResourceKey" /> that references the style for each <see cref="T:System.Windows.Controls.ListViewItem" />. The default value references the default style for a <see cref="T:System.Windows.Controls.ListViewItem" /> control in the current theme.</returns>
	public static ResourceKey GridViewItemContainerStyleKey => SystemResourceKey.GridViewItemContainerStyleKey;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Controls.GridViewColumn" /> objects that is defined for this <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <returns>The collection of columns in the <see cref="T:System.Windows.Controls.GridView" />. The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public GridViewColumnCollection Columns
	{
		get
		{
			if (_columns == null)
			{
				_columns = new GridViewColumnCollection();
				_columns.Owner = this;
				_columns.InViewMode = true;
			}
			return _columns;
		}
	}

	/// <summary>Gets or sets the style to apply to column headers. </summary>
	/// <returns>The <see cref="T:System.Windows.Style" /> that is used to define the display properties for column headers. The default value is null.</returns>
	public Style ColumnHeaderContainerStyle
	{
		get
		{
			return (Style)GetValue(ColumnHeaderContainerStyleProperty);
		}
		set
		{
			SetValue(ColumnHeaderContainerStyleProperty, value);
		}
	}

	/// <summary>Gets or sets a template to use to display the column headers. </summary>
	/// <returns>The <see cref="T:System.Windows.DataTemplate" /> to use to display the column headers as part of the <see cref="T:System.Windows.Controls.GridView" />. The default value is null.</returns>
	public DataTemplate ColumnHeaderTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ColumnHeaderTemplateProperty);
		}
		set
		{
			SetValue(ColumnHeaderTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the selector object that provides logic for selecting a template to use for each column header. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataTemplateSelector" /> object that determines the data template to use for each column header. The default value is null. </returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector ColumnHeaderTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(ColumnHeaderTemplateSelectorProperty);
		}
		set
		{
			SetValue(ColumnHeaderTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the column headers of the <see cref="T:System.Windows.Controls.GridView" /> if they are displayed as strings.</summary>
	/// <returns>A composite string that specifies how to format the column headers of the <see cref="T:System.Windows.Controls.GridView" /> if they are displayed as strings. The default is null.</returns>
	public string ColumnHeaderStringFormat
	{
		get
		{
			return (string)GetValue(ColumnHeaderStringFormatProperty);
		}
		set
		{
			SetValue(ColumnHeaderStringFormatProperty, value);
		}
	}

	/// <summary>Gets or sets whether columns in a <see cref="T:System.Windows.Controls.GridView" /> can be reordered by a drag-and-drop operation. </summary>
	/// <returns>true if columns can be reordered; otherwise, false. The default value is true.</returns>
	public bool AllowsColumnReorder
	{
		get
		{
			return (bool)GetValue(AllowsColumnReorderProperty);
		}
		set
		{
			SetValue(AllowsColumnReorderProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.ContextMenu" /> for the <see cref="T:System.Windows.Controls.GridView" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ContextMenu" /> for the column headers in a <see cref="T:System.Windows.Controls.GridView" />. The default value is null.</returns>
	public ContextMenu ColumnHeaderContextMenu
	{
		get
		{
			return (ContextMenu)GetValue(ColumnHeaderContextMenuProperty);
		}
		set
		{
			SetValue(ColumnHeaderContextMenuProperty, value);
		}
	}

	/// <summary>Gets or sets the content of a tooltip that appears when the mouse pointer pauses over one of the column headers. </summary>
	/// <returns>An object that represents the content that appears as a tooltip when the mouse pointer is paused over one of the column headers. The default value is not defined.</returns>
	public object ColumnHeaderToolTip
	{
		get
		{
			return GetValue(ColumnHeaderToolTipProperty);
		}
		set
		{
			SetValue(ColumnHeaderToolTipProperty, value);
		}
	}

	/// <summary>Gets the reference for the default style for the <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Controls.GridView.GridViewStyleKey" />. The default value is the <see cref="P:System.Windows.Controls.GridView.GridViewStyleKey" /> in the current theme.</returns>
	protected internal override object DefaultStyleKey => GridViewStyleKey;

	/// <summary>Gets the reference to the default style for the container of the data items in the <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Controls.GridView.GridViewItemContainerStyleKey" />. The default value is the <see cref="P:System.Windows.Controls.GridView.GridViewItemContainerStyleKey" /> in the current theme.</returns>
	protected internal override object ItemContainerDefaultStyleKey => GridViewItemContainerStyleKey;

	internal GridViewHeaderRowPresenter HeaderRowPresenter
	{
		get
		{
			return _gvheaderRP;
		}
		set
		{
			_gvheaderRP = value;
		}
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="column">The child object to add.</param>
	void IAddChild.AddChild(object column)
	{
		AddChild(column);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Controls.GridViewColumn" /> object to a <see cref="T:System.Windows.Controls.GridView" />.</summary>
	/// <param name="column">The column to add </param>
	protected virtual void AddChild(object column)
	{
		if (column is GridViewColumn item)
		{
			Columns.Add(item);
			return;
		}
		throw new InvalidOperationException(SR.ListView_IllegalChildrenType);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		AddText(text);
	}

	/// <summary>Not supported.</summary>
	/// <param name="text">Text string</param>
	protected virtual void AddText(string text)
	{
		AddChild(text);
	}

	/// <summary>Returns the string representation of the <see cref="T:System.Windows.Controls.GridView" /> object.</summary>
	/// <returns>A string that indicates the number of columns in the <see cref="T:System.Windows.Controls.GridView" />.</returns>
	public override string ToString()
	{
		return SR.Format(SR.ToStringFormatString_GridView, GetType(), Columns.Count);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation for this <see cref="T:System.Windows.Controls.GridView" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.GridViewAutomationPeer" /> for this <see cref="T:System.Windows.Controls.GridView" />.</returns>
	/// <param name="parent">The <see cref="T:System.Windows.Controls.ListView" /> control that implements this <see cref="T:System.Windows.Controls.GridView" /> view.</param>
	protected internal override IViewAutomationPeer GetAutomationPeer(ListView parent)
	{
		return new GridViewAutomationPeer(this, parent);
	}

	/// <summary>Gets the contents of the <see cref="P:System.Windows.Controls.GridView.ColumnCollection" /> attached property.</summary>
	/// <returns>The <see cref="P:System.Windows.Controls.GridView.ColumnCollection" /> of the specified <see cref="T:System.Windows.DependencyObject" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> that is associated with the collection.</param>
	public static GridViewColumnCollection GetColumnCollection(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (GridViewColumnCollection)element.GetValue(ColumnCollectionProperty);
	}

	/// <summary>Sets the contents of the <see cref="P:System.Windows.Controls.GridView.ColumnCollection" /> attached property.</summary>
	/// <param name="element">The <see cref="T:System.Windows.Controls.GridView" /> object.</param>
	/// <param name="collection">The <see cref="T:System.Windows.Controls.GridViewColumnCollection" /> object to assign.</param>
	public static void SetColumnCollection(DependencyObject element, GridViewColumnCollection collection)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ColumnCollectionProperty, collection);
	}

	/// <summary>Determines whether to serialize the <see cref="P:System.Windows.Controls.GridView.ColumnCollection" /> attached property.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.GridView.ColumnCollection" /> must be serialized; otherwise, false.</returns>
	/// <param name="obj">The object on which the <see cref="P:System.Windows.Controls.GridView.ColumnCollection" /> is set.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool ShouldSerializeColumnCollection(DependencyObject obj)
	{
		if (obj is ListViewItem { ParentSelector: ListView { View: GridView view } } listViewItem)
		{
			return listViewItem.ReadLocalValue(ColumnCollectionProperty) as GridViewColumnCollection != view.Columns;
		}
		return true;
	}

	private static void OnColumnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridView d2 = (GridView)d;
		Helper.CheckTemplateAndTemplateSelector("GridViewColumnHeader", ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, d2);
	}

	private static void OnColumnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridView d2 = (GridView)d;
		Helper.CheckTemplateAndTemplateSelector("GridViewColumnHeader", ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, d2);
	}

	/// <summary>Prepares a <see cref="T:System.Windows.Controls.ListViewItem" /> for display according to the definition of this <see cref="T:System.Windows.Controls.GridView" /> object.</summary>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ListViewItem" /> to display.</param>
	protected internal override void PrepareItem(ListViewItem item)
	{
		base.PrepareItem(item);
		SetColumnCollection(item, _columns);
	}

	/// <summary>Removes all settings, bindings, and styling from a <see cref="T:System.Windows.Controls.ListViewItem" />.</summary>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ListViewItem" /> to clear.</param>
	protected internal override void ClearItem(ListViewItem item)
	{
		item.ClearValue(ColumnCollectionProperty);
		base.ClearItem(item);
	}

	internal override void OnInheritanceContextChangedCore(EventArgs args)
	{
		base.OnInheritanceContextChangedCore(args);
		if (_columns == null)
		{
			return;
		}
		foreach (GridViewColumn column in _columns)
		{
			column.OnInheritanceContextChanged(args);
		}
	}

	internal override void OnThemeChanged()
	{
		if (_columns != null)
		{
			for (int i = 0; i < _columns.Count; i++)
			{
				_columns[i].OnThemeChanged();
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GridView" /> class.</summary>
	public GridView()
	{
	}
}
