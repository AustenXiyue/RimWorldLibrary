using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column that hosts template-specified content in its cells.</summary>
public class DataGridTemplateColumn : DataGridColumn
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty CellTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty CellTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty CellEditingTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty CellEditingTemplateSelectorProperty;

	/// <summary>Gets or sets the template to use to display the contents of a cell that is not in editing mode.</summary>
	/// <returns>The template to use to display the contents of a cell that is not in editing mode. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplate CellTemplate
	{
		get
		{
			return (DataTemplate)GetValue(CellTemplateProperty);
		}
		set
		{
			SetValue(CellTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the object that determines which template to use to display the contents of a cell that is not in editing mode. </summary>
	/// <returns>The object that determines which template to use. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplateSelector CellTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty);
		}
		set
		{
			SetValue(CellTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets the template to use to display the contents of a cell that is in editing mode.</summary>
	/// <returns>The template that is used to display the contents of a cell that is in editing mode. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplate CellEditingTemplate
	{
		get
		{
			return (DataTemplate)GetValue(CellEditingTemplateProperty);
		}
		set
		{
			SetValue(CellEditingTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the object that determines which template to use to display the contents of a cell that is in editing mode.</summary>
	/// <returns>The object that determines which template to use to display the contents of a cell that is in editing mode. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplateSelector CellEditingTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(CellEditingTemplateSelectorProperty);
		}
		set
		{
			SetValue(CellEditingTemplateSelectorProperty, value);
		}
	}

	static DataGridTemplateColumn()
	{
		CellTemplateProperty = DependencyProperty.Register("CellTemplate", typeof(DataTemplate), typeof(DataGridTemplateColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		CellTemplateSelectorProperty = DependencyProperty.Register("CellTemplateSelector", typeof(DataTemplateSelector), typeof(DataGridTemplateColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		CellEditingTemplateProperty = DependencyProperty.Register("CellEditingTemplate", typeof(DataTemplate), typeof(DataGridTemplateColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		CellEditingTemplateSelectorProperty = DependencyProperty.Register("CellEditingTemplateSelector", typeof(DataTemplateSelector), typeof(DataGridTemplateColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DataGridColumn.CanUserSortProperty.OverrideMetadata(typeof(DataGridTemplateColumn), new FrameworkPropertyMetadata(null, OnCoerceTemplateColumnCanUserSort));
		DataGridColumn.SortMemberPathProperty.OverrideMetadata(typeof(DataGridTemplateColumn), new FrameworkPropertyMetadata(OnTemplateColumnSortMemberPathChanged));
	}

	private static void OnTemplateColumnSortMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridTemplateColumn)d).CoerceValue(DataGridColumn.CanUserSortProperty);
	}

	private static object OnCoerceTemplateColumnCanUserSort(DependencyObject d, object baseValue)
	{
		if (string.IsNullOrEmpty(((DataGridTemplateColumn)d).SortMemberPath))
		{
			return false;
		}
		return DataGridColumn.OnCoerceCanUserSort(d, baseValue);
	}

	private void ChooseCellTemplateAndSelector(bool isEditing, out DataTemplate template, out DataTemplateSelector templateSelector)
	{
		template = null;
		templateSelector = null;
		if (isEditing)
		{
			template = CellEditingTemplate;
			templateSelector = CellEditingTemplateSelector;
		}
		if (template == null && templateSelector == null)
		{
			template = CellTemplate;
			templateSelector = CellTemplateSelector;
		}
	}

	private FrameworkElement LoadTemplateContent(bool isEditing, object dataItem, DataGridCell cell)
	{
		ChooseCellTemplateAndSelector(isEditing, out var template, out var templateSelector);
		if (template != null || templateSelector != null)
		{
			ContentPresenter contentPresenter = new ContentPresenter();
			BindingOperations.SetBinding(contentPresenter, ContentPresenter.ContentProperty, new Binding());
			contentPresenter.ContentTemplate = template;
			contentPresenter.ContentTemplateSelector = templateSelector;
			return contentPresenter;
		}
		return null;
	}

	/// <summary>Gets an element defined by the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellTemplate" /> that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
	/// <returns>A new, read-only element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
	{
		return LoadTemplateContent(isEditing: false, dataItem, cell);
	}

	/// <summary>Gets an element defined by the <see cref="P:System.Windows.Controls.DataGridTemplateColumn.CellEditingTemplate" /> that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
	/// <returns>A new editing element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
	{
		return LoadTemplateContent(isEditing: true, dataItem, cell);
	}

	/// <summary>Refreshes the contents of a cell in the column in response to a template property value change.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that has changed.</param>
	protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
	{
		if (element is DataGridCell { IsEditing: var isEditing } dataGridCell && ((!isEditing && (string.Compare(propertyName, "CellTemplate", StringComparison.Ordinal) == 0 || string.Compare(propertyName, "CellTemplateSelector", StringComparison.Ordinal) == 0)) || (isEditing && (string.Compare(propertyName, "CellEditingTemplate", StringComparison.Ordinal) == 0 || string.Compare(propertyName, "CellEditingTemplateSelector", StringComparison.Ordinal) == 0))))
		{
			dataGridCell.BuildVisualTree();
		}
		else
		{
			base.RefreshCellContent(element, propertyName);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridTemplateColumn" /> class. </summary>
	public DataGridTemplateColumn()
	{
	}
}
