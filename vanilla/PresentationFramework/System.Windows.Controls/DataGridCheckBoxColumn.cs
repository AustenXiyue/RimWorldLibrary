using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column that hosts <see cref="T:System.Windows.Controls.CheckBox" /> controls in its cells.</summary>
public class DataGridCheckBoxColumn : DataGridBoundColumn
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridCheckBoxColumn.IsThreeState" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridCheckBoxColumn.IsThreeState" /> dependency property.</returns>
	public static readonly DependencyProperty IsThreeStateProperty;

	private static Style _defaultElementStyle;

	private static Style _defaultEditingElementStyle;

	/// <summary>Gets the default value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.ElementStyle" /> property. </summary>
	/// <returns>An object that represents the style.</returns>
	public static Style DefaultElementStyle
	{
		get
		{
			if (_defaultElementStyle == null)
			{
				Style style = new Style(typeof(CheckBox));
				style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
				style.Setters.Add(new Setter(UIElement.FocusableProperty, false));
				style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center));
				style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top));
				style.Seal();
				_defaultElementStyle = style;
			}
			return _defaultElementStyle;
		}
	}

	/// <summary>Gets the default value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.EditingElementStyle" /> property.</summary>
	/// <returns>An object that represents the style.</returns>
	public static Style DefaultEditingElementStyle
	{
		get
		{
			if (_defaultEditingElementStyle == null)
			{
				Style style = new Style(typeof(CheckBox));
				style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center));
				style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top));
				style.Seal();
				_defaultEditingElementStyle = style;
			}
			return _defaultEditingElementStyle;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the hosted <see cref="T:System.Windows.Controls.CheckBox" /> controls enable three states or two.</summary>
	/// <returns>true if the hosted controls support three states; false if they support two states. The default is false.</returns>
	public bool IsThreeState
	{
		get
		{
			return (bool)GetValue(IsThreeStateProperty);
		}
		set
		{
			SetValue(IsThreeStateProperty, value);
		}
	}

	static DataGridCheckBoxColumn()
	{
		IsThreeStateProperty = ToggleButton.IsThreeStateProperty.AddOwner(typeof(DataGridCheckBoxColumn), new FrameworkPropertyMetadata(false, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DataGridBoundColumn.ElementStyleProperty.OverrideMetadata(typeof(DataGridCheckBoxColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
		DataGridBoundColumn.EditingElementStyleProperty.OverrideMetadata(typeof(DataGridCheckBoxColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
	}

	/// <summary>Gets a read-only <see cref="T:System.Windows.Controls.CheckBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
	/// <returns>A new, read-only check box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
	{
		return GenerateCheckBox(isEditing: false, cell);
	}

	/// <summary>Gets a <see cref="T:System.Windows.Controls.CheckBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
	/// <returns>A new check box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
	{
		return GenerateCheckBox(isEditing: true, cell);
	}

	private CheckBox GenerateCheckBox(bool isEditing, DataGridCell cell)
	{
		CheckBox checkBox = ((cell != null) ? (cell.Content as CheckBox) : null);
		if (checkBox == null)
		{
			checkBox = new CheckBox();
		}
		checkBox.IsThreeState = IsThreeState;
		ApplyStyle(isEditing, defaultToElementStyle: true, checkBox);
		ApplyBinding(checkBox, ToggleButton.IsCheckedProperty);
		return checkBox;
	}

	/// <summary>Refreshes the contents of a cell in the column in response to a column property value change.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that has changed.</param>
	protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
	{
		if (element is DataGridCell dataGridCell && string.Compare(propertyName, "IsThreeState", StringComparison.Ordinal) == 0)
		{
			if (dataGridCell.Content is CheckBox checkBox)
			{
				checkBox.IsThreeState = IsThreeState;
			}
		}
		else
		{
			base.RefreshCellContent(element, propertyName);
		}
	}

	/// <summary>Called when a cell in the column enters editing mode.</summary>
	/// <returns>The unedited value.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="editingEventArgs">Information about the user gesture that is causing a cell to enter editing mode.</param>
	protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
	{
		if (editingElement is CheckBox checkBox)
		{
			checkBox.Focus();
			bool? isChecked = checkBox.IsChecked;
			if ((IsMouseLeftButtonDown(editingEventArgs) && IsMouseOver(checkBox, editingEventArgs)) || IsSpaceKeyDown(editingEventArgs))
			{
				checkBox.IsChecked = isChecked != true;
			}
			return isChecked;
		}
		return false;
	}

	internal override void OnInput(InputEventArgs e)
	{
		if (IsSpaceKeyDown(e))
		{
			BeginEdit(e, handled: true);
		}
	}

	private static bool IsMouseLeftButtonDown(RoutedEventArgs e)
	{
		if (e is MouseButtonEventArgs { ChangedButton: MouseButton.Left } mouseButtonEventArgs)
		{
			return mouseButtonEventArgs.ButtonState == MouseButtonState.Pressed;
		}
		return false;
	}

	private static bool IsMouseOver(CheckBox checkBox, RoutedEventArgs e)
	{
		return checkBox.InputHitTest(((MouseButtonEventArgs)e).GetPosition(checkBox)) != null;
	}

	private static bool IsSpaceKeyDown(RoutedEventArgs e)
	{
		if (e is KeyEventArgs keyEventArgs && keyEventArgs.RoutedEvent == Keyboard.KeyDownEvent && (keyEventArgs.KeyStates & KeyStates.Down) == KeyStates.Down)
		{
			return keyEventArgs.Key == Key.Space;
		}
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCheckBoxColumn" /> class. </summary>
	public DataGridCheckBoxColumn()
	{
	}
}
