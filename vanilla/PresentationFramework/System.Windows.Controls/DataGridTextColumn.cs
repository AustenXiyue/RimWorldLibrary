using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column that hosts textual content in its cells.</summary>
public class DataGridTextColumn : DataGridBoundColumn
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontFamily" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontFamily" /> dependency property.</returns>
	public static readonly DependencyProperty FontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontSize" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontSize" /> dependency property.</returns>
	public static readonly DependencyProperty FontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontStyle" /> dependency property.</returns>
	public static readonly DependencyProperty FontStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontWeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTextColumn.FontWeight" /> dependency property.</returns>
	public static readonly DependencyProperty FontWeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridTextColumn.Foreground" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridTextColumn.Foreground" /> dependency property.</returns>
	public static readonly DependencyProperty ForegroundProperty;

	private static Style _defaultElementStyle;

	private static Style _defaultEditingElementStyle;

	/// <summary>The default value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.ElementStyle" /> property.</summary>
	/// <returns>An object that represents the style.</returns>
	public static Style DefaultElementStyle
	{
		get
		{
			if (_defaultElementStyle == null)
			{
				Style style = new Style(typeof(TextBlock));
				style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(2.0, 0.0, 2.0, 0.0)));
				style.Seal();
				_defaultElementStyle = style;
			}
			return _defaultElementStyle;
		}
	}

	/// <summary>The default value of the <see cref="P:System.Windows.Controls.DataGridBoundColumn.EditingElementStyle" /> property.</summary>
	/// <returns>An object that represents the style.</returns>
	public static Style DefaultEditingElementStyle
	{
		get
		{
			if (_defaultEditingElementStyle == null)
			{
				Style style = new Style(typeof(TextBox));
				style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0.0)));
				style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0.0)));
				style.Seal();
				_defaultEditingElementStyle = style;
			}
			return _defaultEditingElementStyle;
		}
	}

	/// <summary>Gets or sets the font family for the content of cells in the column.</summary>
	/// <returns>The font family of the content for cells in the column. The registered default is <see cref="P:System.Windows.SystemFonts.MessageFontFamily" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public FontFamily FontFamily
	{
		get
		{
			return (FontFamily)GetValue(FontFamilyProperty);
		}
		set
		{
			SetValue(FontFamilyProperty, value);
		}
	}

	/// <summary>Gets or sets the font size for the content of cells in the column.</summary>
	/// <returns>The font size of the content of cells in the column. The registered default is <see cref="P:System.Windows.SystemFonts.MessageFontSize" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	[TypeConverter(typeof(FontSizeConverter))]
	[Localizability(LocalizationCategory.None)]
	public double FontSize
	{
		get
		{
			return (double)GetValue(FontSizeProperty);
		}
		set
		{
			SetValue(FontSizeProperty, value);
		}
	}

	/// <summary>Gets or sets the font style for the content of cells in the column.</summary>
	/// <returns>The font style of the content of cells in the column. The registered default is <see cref="P:System.Windows.SystemFonts.MessageFontStyle" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public FontStyle FontStyle
	{
		get
		{
			return (FontStyle)GetValue(FontStyleProperty);
		}
		set
		{
			SetValue(FontStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the font weight for the content of cells in the column.</summary>
	/// <returns>The font weight of the contents of cells in the column. The registered default is <see cref="P:System.Windows.SystemFonts.MessageFontWeight" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public FontWeight FontWeight
	{
		get
		{
			return (FontWeight)GetValue(FontWeightProperty);
		}
		set
		{
			SetValue(FontWeightProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that is used to paint the text contents of cells in the column.</summary>
	/// <returns>The brush that is used to paint the contents of cells in the column. The registered default is <see cref="P:System.Windows.SystemColors.ControlTextBrush" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Brush Foreground
	{
		get
		{
			return (Brush)GetValue(ForegroundProperty);
		}
		set
		{
			SetValue(ForegroundProperty, value);
		}
	}

	static DataGridTextColumn()
	{
		FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits, DataGridColumn.NotifyPropertyChangeForRefreshContent));
		DataGridBoundColumn.ElementStyleProperty.OverrideMetadata(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
		DataGridBoundColumn.EditingElementStyleProperty.OverrideMetadata(typeof(DataGridTextColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
	}

	/// <summary>Gets a read-only <see cref="T:System.Windows.Controls.TextBlock" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
	/// <returns>A new, read-only text block control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
	{
		TextBlock textBlock = new TextBlock();
		SyncProperties(textBlock);
		ApplyStyle(isEditing: false, defaultToElementStyle: false, textBlock);
		ApplyBinding(textBlock, TextBlock.TextProperty);
		DataGridHelper.RestoreFlowDirection(textBlock, cell);
		return textBlock;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Controls.TextBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</summary>
	/// <returns>A new text box control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
	protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
	{
		TextBox textBox = new TextBox();
		SyncProperties(textBox);
		ApplyStyle(isEditing: true, defaultToElementStyle: false, textBox);
		ApplyBinding(textBox, TextBox.TextProperty);
		DataGridHelper.RestoreFlowDirection(textBox, cell);
		return textBox;
	}

	private void SyncProperties(FrameworkElement e)
	{
		DataGridHelper.SyncColumnProperty(this, e, TextElement.FontFamilyProperty, FontFamilyProperty);
		DataGridHelper.SyncColumnProperty(this, e, TextElement.FontSizeProperty, FontSizeProperty);
		DataGridHelper.SyncColumnProperty(this, e, TextElement.FontStyleProperty, FontStyleProperty);
		DataGridHelper.SyncColumnProperty(this, e, TextElement.FontWeightProperty, FontWeightProperty);
		DataGridHelper.SyncColumnProperty(this, e, TextElement.ForegroundProperty, ForegroundProperty);
	}

	/// <summary>Refreshes the contents of a cell in the column in response to a column property value change.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that has changed.</param>
	protected internal override void RefreshCellContent(FrameworkElement element, string propertyName)
	{
		if (element is DataGridCell { Content: FrameworkElement content })
		{
			switch (propertyName)
			{
			case "FontFamily":
				DataGridHelper.SyncColumnProperty(this, content, TextElement.FontFamilyProperty, FontFamilyProperty);
				break;
			case "FontSize":
				DataGridHelper.SyncColumnProperty(this, content, TextElement.FontSizeProperty, FontSizeProperty);
				break;
			case "FontStyle":
				DataGridHelper.SyncColumnProperty(this, content, TextElement.FontStyleProperty, FontStyleProperty);
				break;
			case "FontWeight":
				DataGridHelper.SyncColumnProperty(this, content, TextElement.FontWeightProperty, FontWeightProperty);
				break;
			case "Foreground":
				DataGridHelper.SyncColumnProperty(this, content, TextElement.ForegroundProperty, ForegroundProperty);
				break;
			}
		}
		base.RefreshCellContent(element, propertyName);
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
			if (editingEventArgs is TextCompositionEventArgs textCompositionEventArgs)
			{
				string text3 = (textBox.Text = ConvertTextForEdit(textCompositionEventArgs.Text));
				textBox.Select(text3.Length, 0);
				return text;
			}
			if (!(editingEventArgs is MouseButtonEventArgs) || !PlaceCaretOnTextBox(textBox, Mouse.GetPosition(textBox)))
			{
				textBox.SelectAll();
			}
			return text;
		}
		return null;
	}

	private string ConvertTextForEdit(string s)
	{
		if (s == "\b")
		{
			s = string.Empty;
		}
		return s;
	}

	/// <summary>Causes the column cell being edited to revert to the specified value.</summary>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="uneditedValue">The previous, unedited value in the cell being edited.</param>
	protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
	{
		DataGridHelper.CacheFlowDirection(editingElement, (editingElement != null) ? (editingElement.Parent as DataGridCell) : null);
		base.CancelCellEdit(editingElement, uneditedValue);
	}

	/// <summary>Performs any required validation before exiting the edit mode.</summary>
	/// <returns>false if validation fails; otherwise, true.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	protected override bool CommitCellEdit(FrameworkElement editingElement)
	{
		DataGridHelper.CacheFlowDirection(editingElement, (editingElement != null) ? (editingElement.Parent as DataGridCell) : null);
		return base.CommitCellEdit(editingElement);
	}

	private static bool PlaceCaretOnTextBox(TextBox textBox, Point position)
	{
		int characterIndexFromPoint = textBox.GetCharacterIndexFromPoint(position, snapToText: false);
		if (characterIndexFromPoint >= 0)
		{
			textBox.Select(characterIndexFromPoint, 0);
			return true;
		}
		return false;
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridTextColumn" /> class. </summary>
	public DataGridTextColumn()
	{
	}
}
