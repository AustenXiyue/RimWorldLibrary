using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using MS.Internal;

namespace System.Windows.Documents;

/// <summary>Represents a selection of content between two <see cref="T:System.Windows.Documents.TextPointer" /> positions.</summary>
public class TextRange : ITextRange
{
	private class ChangeBlock : IDisposable
	{
		private readonly ITextRange _range;

		private readonly bool _disableScroll;

		internal ChangeBlock(ITextRange range, bool disableScroll)
		{
			_range = range;
			_disableScroll = disableScroll;
			_range.BeginChange();
		}

		void IDisposable.Dispose()
		{
			_range.EndChange(_disableScroll, skipEvents: false);
			GC.SuppressFinalize(this);
		}
	}

	[Flags]
	private enum Flags
	{
		IgnoreTextUnitBoundaries = 1,
		IsChanged = 2,
		IsTableCellRange = 4
	}

	private List<TextSegment> _textSegments;

	private int _changeBlockLevel;

	private ChangeBlockUndoRecord _changeBlockUndoRecord;

	private uint _ContentGeneration;

	private Flags _flags;

	private bool _useRestrictiveXamlXmlReader;

	bool ITextRange.IgnoreTextUnitBoundaries => CheckFlags(Flags.IgnoreTextUnitBoundaries);

	ITextPointer ITextRange.Start => TextRangeBase.GetStart(this);

	ITextPointer ITextRange.End => TextRangeBase.GetEnd(this);

	bool ITextRange.IsEmpty => TextRangeBase.GetIsEmpty(this);

	List<TextSegment> ITextRange.TextSegments => TextRangeBase.GetTextSegments(this);

	bool ITextRange.HasConcreteTextContainer
	{
		get
		{
			Invariant.Assert(_textSegments != null, "_textSegments must not be null");
			Invariant.Assert(_textSegments.Count > 0, "_textSegments.Count must be > 0");
			return _textSegments[0].Start is TextPointer;
		}
	}

	string ITextRange.Text
	{
		get
		{
			return TextRangeBase.GetText(this);
		}
		set
		{
			TextRangeBase.SetText(this, value);
		}
	}

	string ITextRange.Xml => TextRangeBase.GetXml(this);

	int ITextRange.ChangeBlockLevel => TextRangeBase.GetChangeBlockLevel(this);

	bool ITextRange.IsTableCellRange => TextRangeBase.GetIsTableCellRange(this);

	bool ITextRange._IsTableCellRange
	{
		get
		{
			return CheckFlags(Flags.IsTableCellRange);
		}
		set
		{
			SetFlags(value, Flags.IsTableCellRange);
		}
	}

	List<TextSegment> ITextRange._TextSegments
	{
		get
		{
			return _textSegments;
		}
		set
		{
			_textSegments = value;
		}
	}

	int ITextRange._ChangeBlockLevel
	{
		get
		{
			return _changeBlockLevel;
		}
		set
		{
			_changeBlockLevel = value;
		}
	}

	ChangeBlockUndoRecord ITextRange._ChangeBlockUndoRecord
	{
		get
		{
			return _changeBlockUndoRecord;
		}
		set
		{
			_changeBlockUndoRecord = value;
		}
	}

	bool ITextRange._IsChanged
	{
		get
		{
			return _IsChanged;
		}
		set
		{
			_IsChanged = value;
		}
	}

	uint ITextRange._ContentGeneration
	{
		get
		{
			return _ContentGeneration;
		}
		set
		{
			_ContentGeneration = value;
		}
	}

	/// <summary>Gets the position that marks the beginning of the current selection.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> that points to the beginning of the current selection.</returns>
	public TextPointer Start => (TextPointer)((ITextRange)this).Start;

	/// <summary>Get the position that marks the end of the current selection.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> that points to the end of the current selection.</returns>
	public TextPointer End => (TextPointer)((ITextRange)this).End;

	/// <summary>Gets a value indicating whether or not the current selection is empty.</summary>
	/// <returns>true if the current selection is empty; otherwise, false.</returns>
	public bool IsEmpty => ((ITextRange)this).IsEmpty;

	internal bool HasConcreteTextContainer => ((ITextRange)this).HasConcreteTextContainer;

	internal FrameworkElement ContainingFrameworkElement
	{
		get
		{
			if (HasConcreteTextContainer)
			{
				return Start.ContainingFrameworkElement;
			}
			return null;
		}
	}

	/// <summary>Gets or sets the plain text contents of the current selection.</summary>
	/// <returns>A string containing the plain text contents of the current selection.</returns>
	/// <exception cref="T:System.ArgumentNullException">Occurs when an attempt is made to set this property to null.</exception>
	public string Text
	{
		get
		{
			return ((ITextRange)this).Text;
		}
		set
		{
			((ITextRange)this).Text = value;
		}
	}

	internal string Xml
	{
		get
		{
			return ((ITextRange)this).Xml;
		}
		set
		{
			TextRangeBase.BeginChange(this);
			try
			{
				if (XamlReader.Load(new XmlTextReader(new StringReader(value)), _useRestrictiveXamlXmlReader) is TextElement xmlVirtual)
				{
					SetXmlVirtual(xmlVirtual);
				}
			}
			finally
			{
				TextRangeBase.EndChange(this);
			}
		}
	}

	internal bool IsTableCellRange => ((ITextRange)this).IsTableCellRange;

	internal bool _IsChanged
	{
		get
		{
			return CheckFlags(Flags.IsChanged);
		}
		set
		{
			SetFlags(value, Flags.IsChanged);
		}
	}

	internal int ChangeBlockLevel => _changeBlockLevel;

	event EventHandler ITextRange.Changed
	{
		add
		{
			Changed += value;
		}
		remove
		{
			Changed -= value;
		}
	}

	/// <summary>Occurs when the range is repositioned to cover a new span of content.</summary>
	public event EventHandler Changed;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.TextRange" /> class, taking two specified <see cref="T:System.Windows.Documents.TextPointer" /> positions as the beginning and end positions for the new range.</summary>
	/// <param name="position1">A fixed anchor position that marks one end of the selection used to form the new <see cref="T:System.Windows.Documents.TextRange" />.</param>
	/// <param name="position2">A movable position that marks the other end of the selection used to form the new <see cref="T:System.Windows.Documents.TextRange" />.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="position1" /> and <paramref name="position2" /> are not positioned within the same document.</exception>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="position1" /> or <paramref name="position2" /> is null.</exception>
	public TextRange(TextPointer position1, TextPointer position2)
		: this((ITextPointer)position1, (ITextPointer)position2)
	{
	}

	internal TextRange(ITextPointer position1, ITextPointer position2)
		: this(position1, position2, ignoreTextUnitBoundaries: false)
	{
	}

	internal TextRange(TextPointer position1, TextPointer position2, bool useRestrictiveXamlXmlReader)
		: this((ITextPointer)position1, (ITextPointer)position2)
	{
		_useRestrictiveXamlXmlReader = useRestrictiveXamlXmlReader;
	}

	internal TextRange(ITextPointer position1, ITextPointer position2, bool ignoreTextUnitBoundaries)
	{
		if (position1 == null)
		{
			throw new ArgumentNullException("position1");
		}
		if (position2 == null)
		{
			throw new ArgumentNullException("position2");
		}
		SetFlags(ignoreTextUnitBoundaries, Flags.IgnoreTextUnitBoundaries);
		ValidationHelper.VerifyPosition(position1.TextContainer, position1, "position1");
		ValidationHelper.VerifyPosition(position1.TextContainer, position2, "position2");
		TextRangeBase.Select(this, position1, position2);
	}

	bool ITextRange.Contains(ITextPointer position)
	{
		return TextRangeBase.Contains(this, position);
	}

	void ITextRange.Select(ITextPointer position1, ITextPointer position2)
	{
		TextRangeBase.Select(this, position1, position2);
	}

	void ITextRange.SelectWord(ITextPointer position)
	{
		TextRangeBase.SelectWord(this, position);
	}

	void ITextRange.SelectParagraph(ITextPointer position)
	{
		TextRangeBase.SelectParagraph(this, position);
	}

	void ITextRange.ApplyTypingHeuristics(bool overType)
	{
		TextRangeBase.ApplyTypingHeuristics(this, overType);
	}

	object ITextRange.GetPropertyValue(DependencyProperty formattingProperty)
	{
		return TextRangeBase.GetPropertyValue(this, formattingProperty);
	}

	UIElement ITextRange.GetUIElementSelected()
	{
		return TextRangeBase.GetUIElementSelected(this);
	}

	bool ITextRange.CanSave(string dataFormat)
	{
		return TextRangeBase.CanSave(this, dataFormat);
	}

	void ITextRange.Save(Stream stream, string dataFormat)
	{
		TextRangeBase.Save(this, stream, dataFormat, preserveTextElements: false);
	}

	void ITextRange.Save(Stream stream, string dataFormat, bool preserveTextElements)
	{
		TextRangeBase.Save(this, stream, dataFormat, preserveTextElements);
	}

	void ITextRange.BeginChange()
	{
		TextRangeBase.BeginChange(this);
	}

	void ITextRange.BeginChangeNoUndo()
	{
		TextRangeBase.BeginChangeNoUndo(this);
	}

	void ITextRange.EndChange()
	{
		TextRangeBase.EndChange(this, disableScroll: false, skipEvents: false);
	}

	void ITextRange.EndChange(bool disableScroll, bool skipEvents)
	{
		TextRangeBase.EndChange(this, disableScroll, skipEvents);
	}

	IDisposable ITextRange.DeclareChangeBlock()
	{
		return new ChangeBlock(this, disableScroll: false);
	}

	IDisposable ITextRange.DeclareChangeBlock(bool disableScroll)
	{
		return new ChangeBlock(this, disableScroll);
	}

	void ITextRange.NotifyChanged(bool disableScroll, bool skipEvents)
	{
		TextRangeBase.NotifyChanged(this, disableScroll);
	}

	void ITextRange.FireChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}

	/// <summary>Checks whether a position (specified by a <see cref="T:System.Windows.Documents.TextPointer" />) is located within the current selection.</summary>
	/// <returns>true if the specified position is located within the current selection; otherwise, false.</returns>
	/// <param name="textPointer">A position to test for inclusion in the current selection.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when textPointer is not in the same document as the current selection.</exception>
	public bool Contains(TextPointer textPointer)
	{
		return ((ITextRange)this).Contains((ITextPointer)textPointer);
	}

	/// <summary>Updates the current selection, taking two <see cref="T:System.Windows.Documents.TextPointer" /> positions to indicate the updated selection.</summary>
	/// <param name="position1">A fixed anchor position that marks one end of the updated selection.</param>
	/// <param name="position2">A movable position that marks the other end of the updated selection.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="position1" /> and <paramref name="position2" /> are not positioned within the same document.</exception>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="position1" /> or <paramref name="position2" /> is null.</exception>
	public void Select(TextPointer position1, TextPointer position2)
	{
		((ITextRange)this).Select((ITextPointer)position1, (ITextPointer)position2);
	}

	internal void SelectWord(TextPointer textPointer)
	{
		((ITextRange)this).SelectWord((ITextPointer)textPointer);
	}

	internal void SelectParagraph(ITextPointer position)
	{
		((ITextRange)this).SelectParagraph(position);
	}

	/// <summary>Applies a specified formatting property and value to the current selection.</summary>
	/// <param name="formattingProperty">A formatting property to apply.</param>
	/// <param name="value">The value for the formatting property.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="formattingProperty" /> does not specify a valid formatting property, or <paramref name="value" /> specifies an invalid value for <paramref name="formattingProperty" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="formattingProperty" /> is null.</exception>
	public void ApplyPropertyValue(DependencyProperty formattingProperty, object value)
	{
		ApplyPropertyValue(formattingProperty, value, applyToParagraphs: false, PropertyValueAction.SetValue);
	}

	internal void ApplyPropertyValue(DependencyProperty formattingProperty, object value, bool applyToParagraphs)
	{
		ApplyPropertyValue(formattingProperty, value, applyToParagraphs, PropertyValueAction.SetValue);
	}

	internal void ApplyPropertyValue(DependencyProperty formattingProperty, object value, bool applyToParagraphs, PropertyValueAction propertyValueAction)
	{
		Invariant.Assert(HasConcreteTextContainer, "Can't apply property to non-TextContainer range!");
		if (formattingProperty == null)
		{
			throw new ArgumentNullException("formattingProperty");
		}
		if (!TextSchema.IsCharacterProperty(formattingProperty) && !TextSchema.IsParagraphProperty(formattingProperty))
		{
			throw new ArgumentException(SR.Format(SR.TextEditorPropertyIsNotApplicableForTextFormatting, formattingProperty.Name));
		}
		if (value is string && formattingProperty.PropertyType != typeof(string))
		{
			TypeConverter converter = TypeDescriptor.GetConverter(formattingProperty.PropertyType);
			Invariant.Assert(converter != null);
			value = converter.ConvertFromString((string)value);
		}
		if (!formattingProperty.IsValidValue(value) && (!(formattingProperty.PropertyType == typeof(Thickness)) || !(value is Thickness)))
		{
			throw new ArgumentException(SR.Format(SR.TextEditorTypeOfParameterIsNotAppropriateForFormattingProperty, (value == null) ? "null" : value.GetType().Name, formattingProperty.Name), "value");
		}
		if (propertyValueAction != 0 && propertyValueAction != PropertyValueAction.IncreaseByAbsoluteValue && propertyValueAction != PropertyValueAction.DecreaseByAbsoluteValue && propertyValueAction != PropertyValueAction.IncreaseByPercentageValue && propertyValueAction != PropertyValueAction.DecreaseByPercentageValue)
		{
			throw new ArgumentException(SR.TextRange_InvalidParameterValue, "propertyValueAction");
		}
		if (propertyValueAction != 0 && !TextSchema.IsPropertyIncremental(formattingProperty))
		{
			throw new ArgumentException(SR.Format(SR.TextRange_PropertyCannotBeIncrementedOrDecremented, formattingProperty.Name), "propertyValueAction");
		}
		ApplyPropertyToTextVirtual(formattingProperty, value, applyToParagraphs, propertyValueAction);
	}

	/// <summary>Removes all formatting properties (represented by <see cref="T:System.Windows.Documents.Inline" /> elements) from the current selection.</summary>
	public void ClearAllProperties()
	{
		Invariant.Assert(HasConcreteTextContainer, "Can't clear properties in non-TextContainer range");
		ClearAllPropertiesVirtual();
	}

	/// <summary>Returns the effective value of a specified formatting property on the current selection.</summary>
	/// <returns>An object specifying the value of the specified formatting property.</returns>
	/// <param name="formattingProperty">A formatting property to get the value of with respect to the current selection.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="formattingProperty" /> does not specify a valid formatting property, or <paramref name="value" /> specifies an invalid value for <paramref name="formattingProperty" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="formattingProperty" /> is null.</exception>
	public object GetPropertyValue(DependencyProperty formattingProperty)
	{
		if (formattingProperty == null)
		{
			throw new ArgumentNullException("formattingProperty");
		}
		if (!TextSchema.IsCharacterProperty(formattingProperty) && !TextSchema.IsParagraphProperty(formattingProperty))
		{
			throw new ArgumentException(SR.Format(SR.TextEditorPropertyIsNotApplicableForTextFormatting, formattingProperty.Name));
		}
		return ((ITextRange)this).GetPropertyValue(formattingProperty);
	}

	internal UIElement GetUIElementSelected()
	{
		return ((ITextRange)this).GetUIElementSelected();
	}

	/// <summary>Checks whether the current selection can be saved as a specified data format.</summary>
	/// <returns>true if the current selection can be saved as the specified data format; otherwise, false.</returns>
	/// <param name="dataFormat">A data format to check for save compatibility with the current selection.  See <see cref="T:System.Windows.DataFormats" /> for a list of predefined data formats.</param>
	public bool CanSave(string dataFormat)
	{
		return ((ITextRange)this).CanSave(dataFormat);
	}

	/// <summary>Checks whether the current selection can be loaded with content in a specified data format.</summary>
	/// <returns>true if the current selection can be loaded with content in the specified data format; otherwise, false.</returns>
	/// <param name="dataFormat">A data format to check for load-compatibility into the current selection.  See <see cref="T:System.Windows.DataFormats" /> for a list of predefined data formats.</param>
	public bool CanLoad(string dataFormat)
	{
		return TextRangeBase.CanLoad(this, dataFormat);
	}

	/// <summary>Saves the current selection to a specified stream in a specified data format.</summary>
	/// <param name="stream">An empty, writable stream to save the current selection to.</param>
	/// <param name="dataFormat">A data format to save the current selection as.  Currently supported data formats are <see cref="F:System.Windows.DataFormats.Rtf" />, <see cref="F:System.Windows.DataFormats.Text" />, <see cref="F:System.Windows.DataFormats.Xaml" />, and <see cref="F:System.Windows.DataFormats.XamlPackage" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="dataFormat" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The specified data format is unsupported.-orContent loaded from <paramref name="stream" /> does not match the specified data format.</exception>
	public void Save(Stream stream, string dataFormat)
	{
		((ITextRange)this).Save(stream, dataFormat);
	}

	/// <summary>Saves the current selection to a specified stream in a specified data format, with the option of preserving custom <see cref="T:System.Windows.Documents.TextElement" /> objects.</summary>
	/// <param name="stream">An empty, writable stream to save the current selection to.</param>
	/// <param name="dataFormat">A data format to save the current selection as.  Currently supported data formats are <see cref="F:System.Windows.DataFormats.Rtf" />, <see cref="F:System.Windows.DataFormats.Text" />, <see cref="F:System.Windows.DataFormats.Xaml" />, and <see cref="F:System.Windows.DataFormats.XamlPackage" />.</param>
	/// <param name="preserveTextElements">true to preserve custom <see cref="T:System.Windows.Documents.TextElement" /> objects; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="stream" /> or <paramref name="dataFormat" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Occurs when the specified data format is unsupported.  May also be raised if content loaded from <paramref name="stream" /> does not match the specified data format.</exception>
	public void Save(Stream stream, string dataFormat, bool preserveTextElements)
	{
		((ITextRange)this).Save(stream, dataFormat, preserveTextElements);
	}

	/// <summary>Loads the current selection in a specified data format from a specified stream.</summary>
	/// <param name="stream">A readable stream that contains data to load into the current selection.</param>
	/// <param name="dataFormat">A data format to load the data as.  Currently supported data formats are <see cref="F:System.Windows.DataFormats.Rtf" />, <see cref="F:System.Windows.DataFormats.Text" />, <see cref="F:System.Windows.DataFormats.Xaml" />, and <see cref="F:System.Windows.DataFormats.XamlPackage" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="stream" /> or <paramref name="dataFormat" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Occurs when the specified data format is unsupported.  May also be raised if content loaded from <paramref name="stream" /> does not match the specified data format.</exception>
	public void Load(Stream stream, string dataFormat)
	{
		LoadVirtual(stream, dataFormat);
	}

	internal void InsertEmbeddedUIElement(FrameworkElement embeddedElement)
	{
		Invariant.Assert(embeddedElement != null);
		InsertEmbeddedUIElementVirtual(embeddedElement);
	}

	internal void InsertImage(Image image)
	{
		BitmapSource bitmapSource = (BitmapSource)image.Source;
		Invariant.Assert(bitmapSource != null);
		if (double.IsNaN(image.Height))
		{
			if ((double)bitmapSource.PixelHeight < 300.0)
			{
				image.Height = bitmapSource.PixelHeight;
			}
			else
			{
				image.Height = 300.0;
			}
		}
		if (double.IsNaN(image.Width))
		{
			if ((double)bitmapSource.PixelHeight < 300.0)
			{
				image.Width = bitmapSource.PixelWidth;
			}
			else
			{
				image.Width = 300.0 / (double)bitmapSource.PixelHeight * (double)bitmapSource.PixelWidth;
			}
		}
		InsertEmbeddedUIElement(image);
	}

	internal virtual void SetXmlVirtual(TextElement fragment)
	{
		if (!IsTableCellRange)
		{
			TextRangeSerialization.PasteXml(this, fragment);
		}
	}

	internal virtual void LoadVirtual(Stream stream, string dataFormat)
	{
		TextRangeBase.Load(this, stream, dataFormat);
	}

	internal Table InsertTable(int rowCount, int columnCount)
	{
		Invariant.Assert(HasConcreteTextContainer, "InsertTable: TextRange must belong to non-abstract TextContainer");
		return InsertTableVirtual(rowCount, columnCount);
	}

	internal TextRange InsertRows(int rowCount)
	{
		Invariant.Assert(HasConcreteTextContainer, "InsertRows: TextRange must belong to non-abstract TextContainer");
		return InsertRowsVirtual(rowCount);
	}

	internal bool DeleteRows()
	{
		Invariant.Assert(HasConcreteTextContainer, "DeleteRows: TextRange must belong to non-abstract TextContainer");
		return DeleteRowsVirtual();
	}

	internal TextRange InsertColumns(int columnCount)
	{
		Invariant.Assert(HasConcreteTextContainer, "InsertColumns: TextRange must belong to non-abstract TextContainer");
		return InsertColumnsVirtual(columnCount);
	}

	internal bool DeleteColumns()
	{
		Invariant.Assert(HasConcreteTextContainer, "DeleteColumns: TextRange must belong to non-abstract TextContainer");
		return DeleteColumnsVirtual();
	}

	internal TextRange MergeCells()
	{
		Invariant.Assert(HasConcreteTextContainer, "MergeCells: TextRange must belong to non-abstract TextContainer");
		return MergeCellsVirtual();
	}

	internal TextRange SplitCell(int splitCountHorizontal, int splitCountVertical)
	{
		Invariant.Assert(HasConcreteTextContainer, "SplitCells: TextRange must belong to non-abstract TextContainer");
		return SplitCellVirtual(splitCountHorizontal, splitCountVertical);
	}

	internal void BeginChange()
	{
		((ITextRange)this).BeginChange();
	}

	internal void EndChange()
	{
		((ITextRange)this).EndChange();
	}

	internal IDisposable DeclareChangeBlock()
	{
		return ((ITextRange)this).DeclareChangeBlock();
	}

	internal IDisposable DeclareChangeBlock(bool disableScroll)
	{
		return ((ITextRange)this).DeclareChangeBlock(disableScroll);
	}

	internal virtual void InsertEmbeddedUIElementVirtual(FrameworkElement embeddedElement)
	{
		Invariant.Assert(HasConcreteTextContainer, "Can't insert embedded object to non-TextContainer range!");
		Invariant.Assert(embeddedElement != null);
		TextRangeBase.BeginChange(this);
		try
		{
			Text = string.Empty;
			Paragraph paragraph = TextRangeEditTables.EnsureInsertionPosition(Start).Paragraph;
			if (paragraph != null)
			{
				if (Paragraph.HasNoTextContent(paragraph))
				{
					BlockUIContainer blockUIContainer = new BlockUIContainer(embeddedElement);
					blockUIContainer.TextAlignment = TextRangeEdit.GetTextAlignmentFromHorizontalAlignment(embeddedElement.HorizontalAlignment);
					paragraph.SiblingBlocks.InsertAfter(paragraph, blockUIContainer);
					paragraph.SiblingBlocks.Remove(paragraph);
					Select(blockUIContainer.ContentStart, blockUIContainer.ContentEnd);
				}
				else
				{
					InlineUIContainer inlineUIContainer = new InlineUIContainer(embeddedElement);
					TextRangeEdit.SplitFormattingElements(Start, keepEmptyFormatting: false).InsertTextElement(inlineUIContainer);
					Select(inlineUIContainer.ElementStart, inlineUIContainer.ElementEnd);
				}
			}
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual void ApplyPropertyToTextVirtual(DependencyProperty formattingProperty, object value, bool applyToParagraphs, PropertyValueAction propertyValueAction)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			for (int i = 0; i < _textSegments.Count; i++)
			{
				TextSegment textSegment = _textSegments[i];
				if (formattingProperty == FrameworkElement.FlowDirectionProperty)
				{
					if (applyToParagraphs || IsEmpty || TextRangeBase.IsParagraphBoundaryCrossed(this))
					{
						TextRangeEdit.SetParagraphProperty((TextPointer)textSegment.Start, (TextPointer)textSegment.End, formattingProperty, value, propertyValueAction);
					}
					else
					{
						TextRangeEdit.SetInlineProperty((TextPointer)textSegment.Start, (TextPointer)textSegment.End, formattingProperty, value, propertyValueAction);
					}
				}
				else if (TextSchema.IsCharacterProperty(formattingProperty))
				{
					TextRangeEdit.SetInlineProperty((TextPointer)textSegment.Start, (TextPointer)textSegment.End, formattingProperty, value, propertyValueAction);
				}
				else if (TextSchema.IsParagraphProperty(formattingProperty))
				{
					if (formattingProperty.PropertyType == typeof(Thickness) && (FlowDirection)textSegment.Start.GetValue(Block.FlowDirectionProperty) == FlowDirection.RightToLeft)
					{
						value = new Thickness(((Thickness)value).Right, ((Thickness)value).Top, ((Thickness)value).Left, ((Thickness)value).Bottom);
					}
					TextRangeEdit.SetParagraphProperty((TextPointer)textSegment.Start, (TextPointer)textSegment.End, formattingProperty, value, propertyValueAction);
				}
			}
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual void ClearAllPropertiesVirtual()
	{
		TextRangeBase.BeginChange(this);
		try
		{
			TextRangeEdit.CharacterResetFormatting(Start, End);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual Table InsertTableVirtual(int rowCount, int columnCount)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.InsertTable(End, rowCount, columnCount);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual TextRange InsertRowsVirtual(int rowCount)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.InsertRows(this, rowCount);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual bool DeleteRowsVirtual()
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.DeleteRows(this);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual TextRange InsertColumnsVirtual(int columnCount)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.InsertColumns(this, columnCount);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual bool DeleteColumnsVirtual()
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.DeleteColumns(this);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual TextRange MergeCellsVirtual()
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.MergeCells(this);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	internal virtual TextRange SplitCellVirtual(int splitCountHorizontal, int splitCountVertical)
	{
		TextRangeBase.BeginChange(this);
		try
		{
			return TextRangeEditTables.SplitCell(this, splitCountHorizontal, splitCountVertical);
		}
		finally
		{
			TextRangeBase.EndChange(this);
		}
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}
}
