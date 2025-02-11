using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column that hosts <see cref="T:System.Uri" /> elements in its cells.</summary>
public class DataGridHyperlinkColumn : DataGridBoundColumn
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.TargetName" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.TargetName" /> dependency property.</returns>
	public static readonly DependencyProperty TargetNameProperty;

	private BindingBase _contentBinding;

	/// <summary>Gets or sets the name of a target window or frame for the hyperlink.</summary>
	/// <returns>The name of the target window or frame. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public string TargetName
	{
		get
		{
			return (string)GetValue(TargetNameProperty);
		}
		set
		{
			SetValue(TargetNameProperty, value);
		}
	}

	/// <summary>Gets or sets the binding to the text of the hyperlink.</summary>
	/// <returns>The binding to the text of the hyperlink.</returns>
	public BindingBase ContentBinding
	{
		get
		{
			return _contentBinding;
		}
		set
		{
			if (_contentBinding != value)
			{
				BindingBase contentBinding = _contentBinding;
				_contentBinding = value;
				OnContentBindingChanged(contentBinding, value);
			}
		}
	}

	/// <summary>The default value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.ElementStyle" /> property.</summary>
	/// <returns>An object that represents the style.</returns>
	public static Style DefaultElementStyle => DataGridTextColumn.DefaultElementStyle;

	/// <summary>The default value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.EditingElementStyle" /> property.</summary>
	/// <returns>An object that represents the style.</returns>
	public static Style DefaultEditingElementStyle => DataGridTextColumn.DefaultEditingElementStyle;

	static DataGridHyperlinkColumn()
	{
		TargetNameProperty = Hyperlink.TargetNameProperty.AddOwner(typeof(DataGridHyperlinkColumn), new FrameworkPropertyMetadata(null, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DataGridBoundColumn.ElementStyleProperty.OverrideMetadata(typeof(DataGridHyperlinkColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
		DataGridBoundColumn.EditingElementStyleProperty.OverrideMetadata(typeof(DataGridHyperlinkColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
	}

	/// <summary>Notifies the <see cref="T:System.Windows.Controls.DataGrid" /> when the <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.ContentBinding" /> property changes.</summary>
	/// <param name="oldBinding">The previous binding.</param>
	/// <param name="newBinding">The binding that the column has been changed to.</param>
	protected virtual void OnContentBindingChanged(BindingBase oldBinding, BindingBase newBinding)
	{
		NotifyPropertyChanged("ContentBinding");
	}

	private void ApplyContentBinding(DependencyObject target, DependencyProperty property)
	{
		if (ContentBinding != null)
		{
			BindingOperations.SetBinding(target, property, ContentBinding);
		}
		else if (Binding != null)
		{
			BindingOperations.SetBinding(target, property, Binding);
		}
		else
		{
			BindingOperations.ClearBinding(target, property);
		}
	}

	/// <summary>Refreshes the contents of a cell in the column in response to a column property value change.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that has changed.</param>
	protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
	{
		if (element is DataGridCell { IsEditing: false } dataGridCell)
		{
			if (string.Compare(propertyName, "ContentBinding", StringComparison.Ordinal) == 0)
			{
				dataGridCell.BuildVisualTree();
			}
			else if (string.Compare(propertyName, "TargetName", StringComparison.Ordinal) == 0 && dataGridCell.Content is TextBlock textBlock && textBlock.Inlines.Count > 0 && textBlock.Inlines.FirstInline is Hyperlink hyperlink)
			{
				hyperlink.TargetName = TargetName;
			}
		}
		else
		{
			base.RefreshCellContent(element, propertyName);
		}
	}

	/// <summary>Gets a read-only <see cref="T:System.Windows.Documents.Hyperlink" /> element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.ContentBinding" /> property value.</summary>
	/// <returns>A new, read-only hyperlink element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.ContentBinding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
	{
		TextBlock textBlock = new TextBlock();
		Hyperlink hyperlink = new Hyperlink();
		InlineUIContainer inlineUIContainer = new InlineUIContainer();
		ContentPresenter contentPresenter = new ContentPresenter();
		textBlock.Inlines.Add(hyperlink);
		hyperlink.Inlines.Add(inlineUIContainer);
		inlineUIContainer.Child = contentPresenter;
		hyperlink.TargetName = TargetName;
		ApplyStyle(isEditing: false, defaultToElementStyle: false, textBlock);
		ApplyBinding(hyperlink, Hyperlink.NavigateUriProperty);
		ApplyContentBinding(contentPresenter, ContentPresenter.ContentProperty);
		DataGridHelper.RestoreFlowDirection(textBlock, cell);
		return textBlock;
	}

	/// <summary>Gets an editable <see cref="T:System.Windows.Controls.TextBox" /> element that is bound to the column's <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.ContentBinding" /> property value.</summary>
	/// <returns>A new text box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridHyperlinkColumn.ContentBinding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
	{
		TextBox textBox = new TextBox();
		ApplyStyle(isEditing: true, defaultToElementStyle: false, textBox);
		ApplyBinding(textBox, TextBox.TextProperty);
		DataGridHelper.RestoreFlowDirection(textBox, cell);
		return textBox;
	}

	/// <summary>Called when a cell in the column enters editing mode.</summary>
	/// <returns>The unedited value of the cell.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="editingEventArgs">Information about the user gesture that is causing a cell to enter editing mode.</param>
	protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
	{
		if (editingElement is TextBox textBox)
		{
			textBox.Focus();
			string text = textBox.Text;
			if (editingEventArgs is TextCompositionEventArgs { Text: var text2 })
			{
				textBox.Text = text2;
				textBox.Select(text2.Length, 0);
				return text;
			}
			textBox.SelectAll();
			return text;
		}
		return null;
	}

	/// <summary>Causes the column cell being edited to revert to the specified value.</summary>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="uneditedValue">The previous, unedited value in the cell being edited.</param>
	protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
	{
		DataGridHelper.CacheFlowDirection(editingElement, (editingElement != null) ? (editingElement.Parent as DataGridCell) : null);
		base.CancelCellEdit(editingElement, uneditedValue);
	}

	/// <summary>Performs any required validation before exiting edit mode.</summary>
	/// <returns>false if validation fails; otherwise, true.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	protected override bool CommitCellEdit(FrameworkElement editingElement)
	{
		DataGridHelper.CacheFlowDirection(editingElement, (editingElement != null) ? (editingElement.Parent as DataGridCell) : null);
		return base.CommitCellEdit(editingElement);
	}

	internal override void OnInput(InputEventArgs e)
	{
		if (DataGridHelper.HasNonEscapeCharacters(e as TextCompositionEventArgs))
		{
			BeginEdit(e, handled: true);
		}
		else if (DataGridHelper.IsImeProcessed(e as KeyEventArgs) && base.DataGridOwner != null)
		{
			DataGridCell currentCellContainer = base.DataGridOwner.CurrentCellContainer;
			if (currentCellContainer != null && !currentCellContainer.IsEditing)
			{
				BeginEdit(e, handled: false);
				base.Dispatcher.Invoke(delegate
				{
				}, DispatcherPriority.Background);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridHyperlinkColumn" /> class. </summary>
	public DataGridHyperlinkColumn()
	{
	}
}
