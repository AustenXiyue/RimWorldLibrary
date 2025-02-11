using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>Serves as the base class for columns that can bind to a property in the data source of a <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
public abstract class DataGridBoundColumn : DataGridColumn
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridBoundColumn.ElementStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridBoundColumn.ElementStyle" /> dependency property.</returns>
	public static readonly DependencyProperty ElementStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridBoundColumn.EditingElementStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridBoundColumn.EditingElementStyle" /> dependency property.</returns>
	public static readonly DependencyProperty EditingElementStyleProperty;

	private BindingBase _binding;

	/// <summary>Gets or sets the binding that associates the column with a property in the data source.</summary>
	/// <returns>The object that represents the data binding for the column. The default is null.</returns>
	public virtual BindingBase Binding
	{
		get
		{
			return _binding;
		}
		set
		{
			if (_binding != value)
			{
				BindingBase binding = _binding;
				_binding = value;
				CoerceValue(DataGridColumn.IsReadOnlyProperty);
				CoerceValue(DataGridColumn.SortMemberPathProperty);
				OnBindingChanged(binding, _binding);
			}
		}
	}

	/// <summary>Gets or sets the style that is used when rendering the element that the column displays for a cell that is not in editing mode.</summary>
	/// <returns>The style that is used when rendering a display-only element. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style ElementStyle
	{
		get
		{
			return (Style)GetValue(ElementStyleProperty);
		}
		set
		{
			SetValue(ElementStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the style that is used when rendering the element that the column displays for a cell in editing mode.</summary>
	/// <returns>The style that is used when rendering an editing element. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style EditingElementStyle
	{
		get
		{
			return (Style)GetValue(EditingElementStyleProperty);
		}
		set
		{
			SetValue(EditingElementStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the binding object to use when getting or setting cell content for the clipboard.</summary>
	/// <returns>An object that represents the binding.</returns>
	public override BindingBase ClipboardContentBinding
	{
		get
		{
			return base.ClipboardContentBinding ?? Binding;
		}
		set
		{
			base.ClipboardContentBinding = value;
		}
	}

	static DataGridBoundColumn()
	{
		ElementStyleProperty = DependencyProperty.Register("ElementStyle", typeof(Style), typeof(DataGridBoundColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		EditingElementStyleProperty = DependencyProperty.Register("EditingElementStyle", typeof(Style), typeof(DataGridBoundColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DataGridColumn.SortMemberPathProperty.OverrideMetadata(typeof(DataGridBoundColumn), new FrameworkPropertyMetadata(null, OnCoerceSortMemberPath));
	}

	private static object OnCoerceSortMemberPath(DependencyObject d, object baseValue)
	{
		DataGridBoundColumn dataGridBoundColumn = (DataGridBoundColumn)d;
		string text = (string)baseValue;
		if (string.IsNullOrEmpty(text))
		{
			string pathFromBinding = DataGridHelper.GetPathFromBinding(dataGridBoundColumn.Binding as Binding);
			if (!string.IsNullOrEmpty(pathFromBinding))
			{
				text = pathFromBinding;
			}
		}
		return text;
	}

	/// <summary>Determines the value of the <see cref="P:System.Windows.Controls.DataGridColumn.IsReadOnly" /> property based on property rules from the data grid that contains this column.</summary>
	/// <returns>true if cells in the column cannot be edited based on rules from the data grid; otherwise, false.</returns>
	/// <param name="baseValue">The value that was passed to the delegate.</param>
	protected override bool OnCoerceIsReadOnly(bool baseValue)
	{
		if (DataGridHelper.IsOneWay(_binding))
		{
			return true;
		}
		return base.OnCoerceIsReadOnly(baseValue);
	}

	/// <summary>Notifies the <see cref="T:System.Windows.Controls.DataGrid" /> when the value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property changes.</summary>
	/// <param name="oldBinding">The previous binding.</param>
	/// <param name="newBinding">The binding that the column has been changed to.</param>
	protected virtual void OnBindingChanged(BindingBase oldBinding, BindingBase newBinding)
	{
		NotifyPropertyChanged("Binding");
	}

	internal void ApplyBinding(DependencyObject target, DependencyProperty property)
	{
		BindingBase binding = Binding;
		if (binding != null)
		{
			BindingOperations.SetBinding(target, property, binding);
		}
		else
		{
			BindingOperations.ClearBinding(target, property);
		}
	}

	internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
	{
		Style style = PickStyle(isEditing, defaultToElementStyle);
		if (style != null)
		{
			element.Style = style;
		}
	}

	private Style PickStyle(bool isEditing, bool defaultToElementStyle)
	{
		Style style = (isEditing ? EditingElementStyle : ElementStyle);
		if (isEditing && defaultToElementStyle && style == null)
		{
			style = ElementStyle;
		}
		return style;
	}

	/// <summary>Rebuilds the contents of a cell in the column in response to a binding change.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that has changed.</param>
	protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
	{
		if (element is DataGridCell { IsEditing: var isEditing } dataGridCell && (string.Compare(propertyName, "Binding", StringComparison.Ordinal) == 0 || (string.Compare(propertyName, "ElementStyle", StringComparison.Ordinal) == 0 && !isEditing) || (string.Compare(propertyName, "EditingElementStyle", StringComparison.Ordinal) == 0 && isEditing)))
		{
			dataGridCell.BuildVisualTree();
		}
		else
		{
			base.RefreshCellContent(element, propertyName);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridBoundColumn" /> class. </summary>
	protected DataGridBoundColumn()
	{
	}
}
