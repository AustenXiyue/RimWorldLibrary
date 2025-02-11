using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column that hosts <see cref="T:System.Windows.Controls.ComboBox" /> controls in its cells.</summary>
public class DataGridComboBoxColumn : DataGridColumn
{
	internal class TextBlockComboBox : ComboBox
	{
		static TextBlockComboBox()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlockComboBox), new FrameworkPropertyMetadata(TextBlockComboBoxStyleKey));
			KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(TextBlockComboBox), new FrameworkPropertyMetadata(false));
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.ElementStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.ElementStyle" /> dependency property.</returns>
	public static readonly DependencyProperty ElementStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.EditingElementStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.EditingElementStyle" /> dependency property.</returns>
	public static readonly DependencyProperty EditingElementStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.ItemsSource" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.ItemsSource" /> dependency property.</returns>
	public static readonly DependencyProperty ItemsSourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.DisplayMemberPath" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.DisplayMemberPath" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayMemberPathProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValuePath" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValuePath" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedValuePathProperty;

	private static Style _defaultElementStyle;

	private BindingBase _selectedValueBinding;

	private BindingBase _selectedItemBinding;

	private BindingBase _textBinding;

	/// <summary>Gets the resource key for the style to apply to a read-only combo box.</summary>
	/// <returns>The key for the style.</returns>
	public static ComponentResourceKey TextBlockComboBoxStyleKey => SystemResourceKey.DataGridComboBoxColumnTextBlockComboBoxStyleKey;

	private BindingBase EffectiveBinding
	{
		get
		{
			if (SelectedItemBinding != null)
			{
				return SelectedItemBinding;
			}
			if (SelectedValueBinding != null)
			{
				return SelectedValueBinding;
			}
			return TextBinding;
		}
	}

	/// <summary>Gets or sets the value of the selected item, obtained by using <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValuePath" />.</summary>
	/// <returns>The binding for the selected value.</returns>
	public virtual BindingBase SelectedValueBinding
	{
		get
		{
			return _selectedValueBinding;
		}
		set
		{
			if (_selectedValueBinding != value)
			{
				BindingBase selectedValueBinding = _selectedValueBinding;
				_selectedValueBinding = value;
				CoerceValue(DataGridColumn.IsReadOnlyProperty);
				CoerceValue(DataGridColumn.SortMemberPathProperty);
				OnSelectedValueBindingChanged(selectedValueBinding, _selectedValueBinding);
			}
		}
	}

	/// <summary>Gets or sets the binding for the currently selected item.</summary>
	/// <returns>The binding for the selected item.</returns>
	public virtual BindingBase SelectedItemBinding
	{
		get
		{
			return _selectedItemBinding;
		}
		set
		{
			if (_selectedItemBinding != value)
			{
				BindingBase selectedItemBinding = _selectedItemBinding;
				_selectedItemBinding = value;
				CoerceValue(DataGridColumn.IsReadOnlyProperty);
				CoerceValue(DataGridColumn.SortMemberPathProperty);
				OnSelectedItemBindingChanged(selectedItemBinding, _selectedItemBinding);
			}
		}
	}

	/// <summary>Gets or sets the binding for the text in the text box portion of the <see cref="T:System.Windows.Controls.ComboBox" /> control.</summary>
	/// <returns>The binding for the text of the currently selected item.</returns>
	public virtual BindingBase TextBinding
	{
		get
		{
			return _textBinding;
		}
		set
		{
			if (_textBinding != value)
			{
				BindingBase textBinding = _textBinding;
				_textBinding = value;
				CoerceValue(DataGridColumn.IsReadOnlyProperty);
				CoerceValue(DataGridColumn.SortMemberPathProperty);
				OnTextBindingChanged(textBinding, _textBinding);
			}
		}
	}

	/// <summary>Gets the default value of the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.ElementStyle" />.</summary>
	/// <returns>The default value of the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.ElementStyle" />.</returns>
	public static Style DefaultElementStyle
	{
		get
		{
			if (_defaultElementStyle == null)
			{
				Style style = new Style(typeof(ComboBox));
				style.Setters.Add(new Setter(Selector.IsSynchronizedWithCurrentItemProperty, false));
				style.Seal();
				_defaultElementStyle = style;
			}
			return _defaultElementStyle;
		}
	}

	/// <summary>Gets the default value of the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.EditingElementStyle" /> property.</summary>
	/// <returns>The default value of <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.EditingElementStyle" />.</returns>
	public static Style DefaultEditingElementStyle => DefaultElementStyle;

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
			return base.ClipboardContentBinding ?? EffectiveBinding;
		}
		set
		{
			base.ClipboardContentBinding = value;
		}
	}

	/// <summary>Gets or sets a collection that is used to generate the content of the combo box control.</summary>
	/// <returns>A collection that is used to generate the content of the combo box control. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public IEnumerable ItemsSource
	{
		get
		{
			return (IEnumerable)GetValue(ItemsSourceProperty);
		}
		set
		{
			SetValue(ItemsSourceProperty, value);
		}
	}

	/// <summary>Gets or sets a path to a value on the source object to provide the visual representation of the object.</summary>
	/// <returns>The path to a value on the source object. The registered default is an empty string (""). For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets the path that is used to get the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValue" /> from the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" />.</summary>
	/// <returns>The path to get the selected value. The registered default is an empty string (""). For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public string SelectedValuePath
	{
		get
		{
			return (string)GetValue(SelectedValuePathProperty);
		}
		set
		{
			SetValue(SelectedValuePathProperty, value);
		}
	}

	static DataGridComboBoxColumn()
	{
		ElementStyleProperty = DataGridBoundColumn.ElementStyleProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
		EditingElementStyleProperty = DataGridBoundColumn.EditingElementStyleProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
		ItemsSourceProperty = ItemsControl.ItemsSourceProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DisplayMemberPathProperty = ItemsControl.DisplayMemberPathProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(string.Empty, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		SelectedValuePathProperty = Selector.SelectedValuePathProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(string.Empty, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DataGridColumn.SortMemberPathProperty.OverrideMetadata(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(null, OnCoerceSortMemberPath));
	}

	private static object OnCoerceSortMemberPath(DependencyObject d, object baseValue)
	{
		DataGridComboBoxColumn dataGridComboBoxColumn = (DataGridComboBoxColumn)d;
		string text = (string)baseValue;
		if (string.IsNullOrEmpty(text))
		{
			string pathFromBinding = DataGridHelper.GetPathFromBinding(dataGridComboBoxColumn.EffectiveBinding as Binding);
			if (!string.IsNullOrEmpty(pathFromBinding))
			{
				text = pathFromBinding;
			}
		}
		return text;
	}

	/// <summary>Determines the value of the <see cref="P:System.Windows.Controls.DataGridColumn.IsReadOnly" /> property based on property rules from the <see cref="T:System.Windows.Controls.DataGrid" /> that contains this column.</summary>
	/// <returns>true if the combo boxes in the column cannot be edited; otherwise, false.</returns>
	/// <param name="baseValue">The value that was passed to the delegate.</param>
	protected override bool OnCoerceIsReadOnly(bool baseValue)
	{
		if (DataGridHelper.IsOneWay(EffectiveBinding))
		{
			return true;
		}
		return base.OnCoerceIsReadOnly(baseValue);
	}

	/// <summary>Notifies the <see cref="T:System.Windows.Controls.DataGrid" /> when the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValueBinding" /> property changes.</summary>
	/// <param name="oldBinding">The previous binding.</param>
	/// <param name="newBinding">The binding that the column has been changed to.</param>
	protected virtual void OnSelectedValueBindingChanged(BindingBase oldBinding, BindingBase newBinding)
	{
		NotifyPropertyChanged("SelectedValueBinding");
	}

	/// <summary>Notifies the <see cref="T:System.Windows.Controls.DataGrid" /> when the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedItemBinding" /> property changes.</summary>
	/// <param name="oldBinding">The previous binding.</param>
	/// <param name="newBinding">The binding that the column has been changed to.</param>
	protected virtual void OnSelectedItemBindingChanged(BindingBase oldBinding, BindingBase newBinding)
	{
		NotifyPropertyChanged("SelectedItemBinding");
	}

	/// <summary>Notifies the <see cref="T:System.Windows.Controls.DataGrid" /> when the <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.TextBinding" /> property changes.</summary>
	/// <param name="oldBinding">The previous binding.</param>
	/// <param name="newBinding">The binding that the column has been changed to.</param>
	protected virtual void OnTextBindingChanged(BindingBase oldBinding, BindingBase newBinding)
	{
		NotifyPropertyChanged("TextBinding");
	}

	private void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
	{
		Style style = PickStyle(isEditing, defaultToElementStyle);
		if (style != null)
		{
			element.Style = style;
		}
	}

	internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkContentElement element)
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

	private static void ApplyBinding(BindingBase binding, DependencyObject target, DependencyProperty property)
	{
		if (binding != null)
		{
			BindingOperations.SetBinding(target, property, binding);
		}
		else
		{
			BindingOperations.ClearBinding(target, property);
		}
	}

	/// <summary>Refreshes the contents of a cell in the column in response to a binding change.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that has changed.</param>
	protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
	{
		if (element is DataGridCell { IsEditing: var isEditing } dataGridCell)
		{
			if ((string.Compare(propertyName, "ElementStyle", StringComparison.Ordinal) == 0 && !isEditing) || (string.Compare(propertyName, "EditingElementStyle", StringComparison.Ordinal) == 0 && isEditing))
			{
				dataGridCell.BuildVisualTree();
				return;
			}
			ComboBox comboBox = dataGridCell.Content as ComboBox;
			switch (propertyName)
			{
			case "SelectedItemBinding":
				ApplyBinding(SelectedItemBinding, comboBox, Selector.SelectedItemProperty);
				break;
			case "SelectedValueBinding":
				ApplyBinding(SelectedValueBinding, comboBox, Selector.SelectedValueProperty);
				break;
			case "TextBinding":
				ApplyBinding(TextBinding, comboBox, ComboBox.TextProperty);
				break;
			case "SelectedValuePath":
				DataGridHelper.SyncColumnProperty(this, comboBox, Selector.SelectedValuePathProperty, SelectedValuePathProperty);
				break;
			case "DisplayMemberPath":
				DataGridHelper.SyncColumnProperty(this, comboBox, ItemsControl.DisplayMemberPathProperty, DisplayMemberPathProperty);
				break;
			case "ItemsSource":
				DataGridHelper.SyncColumnProperty(this, comboBox, ItemsControl.ItemsSourceProperty, ItemsSourceProperty);
				break;
			default:
				base.RefreshCellContent(element, propertyName);
				break;
			}
		}
		else
		{
			base.RefreshCellContent(element, propertyName);
		}
	}

	private object GetComboBoxSelectionValue(ComboBox comboBox)
	{
		if (SelectedItemBinding != null)
		{
			return comboBox.SelectedItem;
		}
		if (SelectedValueBinding != null)
		{
			return comboBox.SelectedValue;
		}
		return comboBox.Text;
	}

	/// <summary>Gets a read-only combo box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedItemBinding" />, <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValueBinding" />, and <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.TextBinding" /> values.</summary>
	/// <returns>A new, read-only combo box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedItemBinding" />, <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValueBinding" />, and <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.TextBinding" /> values.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
	{
		TextBlockComboBox textBlockComboBox = new TextBlockComboBox();
		ApplyStyle(isEditing: false, defaultToElementStyle: false, textBlockComboBox);
		ApplyColumnProperties(textBlockComboBox);
		DataGridHelper.RestoreFlowDirection(textBlockComboBox, cell);
		return textBlockComboBox;
	}

	/// <summary>Gets a combo box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedItemBinding" />, <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValueBinding" />, and <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.TextBinding" /> values.</summary>
	/// <returns>A new combo box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedItemBinding" />, <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.SelectedValueBinding" />, and <see cref="P:System.Windows.Controls.DataGridComboBoxColumn.TextBinding" /> values.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
	{
		ComboBox comboBox = new ComboBox();
		ApplyStyle(isEditing: true, defaultToElementStyle: false, comboBox);
		ApplyColumnProperties(comboBox);
		DataGridHelper.RestoreFlowDirection(comboBox, cell);
		return comboBox;
	}

	private void ApplyColumnProperties(ComboBox comboBox)
	{
		ApplyBinding(SelectedItemBinding, comboBox, Selector.SelectedItemProperty);
		ApplyBinding(SelectedValueBinding, comboBox, Selector.SelectedValueProperty);
		ApplyBinding(TextBinding, comboBox, ComboBox.TextProperty);
		DataGridHelper.SyncColumnProperty(this, comboBox, Selector.SelectedValuePathProperty, SelectedValuePathProperty);
		DataGridHelper.SyncColumnProperty(this, comboBox, ItemsControl.DisplayMemberPathProperty, DisplayMemberPathProperty);
		DataGridHelper.SyncColumnProperty(this, comboBox, ItemsControl.ItemsSourceProperty, ItemsSourceProperty);
	}

	/// <summary>Called when a cell in the column enters editing mode.</summary>
	/// <returns>The unedited value.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="editingEventArgs">Information about the user gesture that is causing a cell to enter editing mode.</param>
	protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
	{
		if (editingElement is ComboBox comboBox)
		{
			comboBox.Focus();
			object comboBoxSelectionValue = GetComboBoxSelectionValue(comboBox);
			if (IsComboBoxOpeningInputEvent(editingEventArgs))
			{
				comboBox.IsDropDownOpen = true;
			}
			return comboBoxSelectionValue;
		}
		return null;
	}

	/// <summary>Causes the column cell being edited to revert to the specified value.</summary>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="uneditedValue">The previous, unedited value in the cell being edited.</param>
	protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
	{
		if (editingElement is ComboBox { EditableTextBoxSite: not null } comboBox)
		{
			DataGridHelper.CacheFlowDirection(comboBox.EditableTextBoxSite, comboBox.Parent as DataGridCell);
			DataGridHelper.CacheFlowDirection(comboBox, comboBox.Parent as DataGridCell);
		}
		base.CancelCellEdit(editingElement, uneditedValue);
	}

	/// <summary>Performs any required validation before exiting the editing mode.</summary>
	/// <returns>false if validation fails; otherwise, true.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	protected override bool CommitCellEdit(FrameworkElement editingElement)
	{
		if (editingElement is ComboBox { EditableTextBoxSite: not null } comboBox)
		{
			DataGridHelper.CacheFlowDirection(comboBox.EditableTextBoxSite, comboBox.Parent as DataGridCell);
			DataGridHelper.CacheFlowDirection(comboBox, comboBox.Parent as DataGridCell);
		}
		return base.CommitCellEdit(editingElement);
	}

	internal override void OnInput(InputEventArgs e)
	{
		if (IsComboBoxOpeningInputEvent(e))
		{
			BeginEdit(e, handled: true);
		}
	}

	private static bool IsComboBoxOpeningInputEvent(RoutedEventArgs e)
	{
		if (e is KeyEventArgs keyEventArgs && keyEventArgs.RoutedEvent == Keyboard.KeyDownEvent && (keyEventArgs.KeyStates & KeyStates.Down) == KeyStates.Down)
		{
			bool flag = (keyEventArgs.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
			Key key = keyEventArgs.Key;
			if (key == Key.System)
			{
				key = keyEventArgs.SystemKey;
			}
			if (key != Key.F4 || flag)
			{
				return (key == Key.Up || key == Key.Down) && flag;
			}
			return true;
		}
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridComboBoxColumn" /> class. </summary>
	public DataGridComboBoxColumn()
	{
	}
}
