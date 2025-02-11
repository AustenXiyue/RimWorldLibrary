using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that can be used to display or edit unformatted text.</summary>
[Localizability(LocalizationCategory.Text)]
[ContentProperty("Text")]
public class TextBox : TextBoxBase, IAddChild, ITextBoxViewHost
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.TextWrapping" /> dependency property.</summary>
	public static readonly DependencyProperty TextWrappingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.MinLines" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBox.MinLines" /> dependency property.</returns>
	public static readonly DependencyProperty MinLinesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.MaxLines" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBox.MaxLines" /> dependency property.</returns>
	public static readonly DependencyProperty MaxLinesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.Text" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBox.Text" /> dependency property.</returns>
	public static readonly DependencyProperty TextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.CharacterCasing" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBox.CharacterCasing" /> dependency property.</returns>
	public static readonly DependencyProperty CharacterCasingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.MaxLength" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBox.MaxLength" /> dependency property.</returns>
	public static readonly DependencyProperty MaxLengthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.TextAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBox.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBox.TextDecorations" /> dependency property.</summary>
	public static readonly DependencyProperty TextDecorationsProperty;

	private static DependencyObjectType _dType;

	private bool _minmaxChanged;

	private bool _isInsideTextContentChange;

	private object _newTextValue = DependencyProperty.UnsetValue;

	private bool _isTypographySet;

	private int _changeEventNestingCount;

	/// <summary>Gets or sets how the text box should wrap text.</summary>
	/// <returns>One of the <see cref="T:System.Windows.TextWrapping" /> values that indicates how the text box should wrap text. The default is <see cref="F:System.Windows.TextWrapping.NoWrap" />.</returns>
	public TextWrapping TextWrapping
	{
		get
		{
			return (TextWrapping)GetValue(TextWrappingProperty);
		}
		set
		{
			SetValue(TextWrappingProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum number of visible lines.</summary>
	/// <returns>The minimum number of visible lines. The default is 1.</returns>
	/// <exception cref="T:System.Exception">
	///   <see cref="P:System.Windows.Controls.TextBox.MinLines" /> is greater than <see cref="P:System.Windows.Controls.TextBox.MaxLines" />.</exception>
	[DefaultValue(1)]
	public int MinLines
	{
		get
		{
			return (int)GetValue(MinLinesProperty);
		}
		set
		{
			SetValue(MinLinesProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum number of visible lines.</summary>
	/// <returns>The maximum number of visible lines. The default is <see cref="F:System.Int32.MaxValue" />.</returns>
	/// <exception cref="T:System.Exception">
	///   <see cref="P:System.Windows.Controls.TextBox.MaxLines" /> is less than <see cref="P:System.Windows.Controls.TextBox.MinLines" />.</exception>
	[DefaultValue(int.MaxValue)]
	public int MaxLines
	{
		get
		{
			return (int)GetValue(MaxLinesProperty);
		}
		set
		{
			SetValue(MaxLinesProperty, value);
		}
	}

	/// <summary>Gets or sets the text contents of the text box.</summary>
	/// <returns>A string containing the text contents of the text box. The default is an empty string ("").</returns>
	[DefaultValue("")]
	[Localizability(LocalizationCategory.Text)]
	public string Text
	{
		get
		{
			return (string)GetValue(TextProperty);
		}
		set
		{
			SetValue(TextProperty, value);
		}
	}

	/// <summary>Gets or sets how characters are cased when they are manually entered into the text box.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.CharacterCasing" /> values that specifies how manually entered characters are cased. The default is <see cref="F:System.Windows.Controls.CharacterCasing.Normal" />.</returns>
	public CharacterCasing CharacterCasing
	{
		get
		{
			return (CharacterCasing)GetValue(CharacterCasingProperty);
		}
		set
		{
			SetValue(CharacterCasingProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum number of characters that can be manually entered into the text box.</summary>
	/// <returns>The maximum number of characters that can be manually entered into the text box. The default is 0, which indicates no limit.</returns>
	[DefaultValue(0)]
	[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable)]
	public int MaxLength
	{
		get
		{
			return (int)GetValue(MaxLengthProperty);
		}
		set
		{
			SetValue(MaxLengthProperty, value);
		}
	}

	/// <summary>Gets or sets the horizontal alignment of the contents of the text box. </summary>
	/// <returns>One of the <see cref="T:System.Windows.TextAlignment" /> values that specifies the horizontal alignment of the contents of the text box. The default is <see cref="F:System.Windows.TextAlignment.Left" />.</returns>
	public TextAlignment TextAlignment
	{
		get
		{
			return (TextAlignment)GetValue(TextAlignmentProperty);
		}
		set
		{
			SetValue(TextAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets the content of the current selection in the text box.</summary>
	/// <returns>The currently selected text in the text box.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SelectedText
	{
		get
		{
			return base.TextSelectionInternal.Text;
		}
		set
		{
			using (base.TextSelectionInternal.DeclareChangeBlock())
			{
				base.TextSelectionInternal.Text = value;
			}
		}
	}

	/// <summary>Gets or sets a value indicating the number of characters in the current selection in the text box.</summary>
	/// <returns>The number of characters in the current selection in the text box. The default is 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Windows.Controls.TextBox.SelectionLength" /> is set to a negative value.</exception>
	[DefaultValue(0)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectionLength
	{
		get
		{
			return base.TextSelectionInternal.Start.GetOffsetToPosition(base.TextSelectionInternal.End);
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", SR.ParameterCannotBeNegative);
			}
			int offsetToPosition = base.TextSelectionInternal.Start.GetOffsetToPosition(base.TextContainer.End);
			if (value > offsetToPosition)
			{
				value = offsetToPosition;
			}
			TextPointer textPointer = new TextPointer(base.TextSelectionInternal.Start, value, LogicalDirection.Forward);
			textPointer = textPointer.GetInsertionPosition(LogicalDirection.Forward);
			base.TextSelectionInternal.Select(base.TextSelectionInternal.Start, textPointer);
		}
	}

	/// <summary>Gets or sets a character index for the beginning of the current selection.</summary>
	/// <returns>The character index for the beginning of the current selection.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Windows.Controls.TextBox.SelectionStart" /> is set to a negative value.</exception>
	[DefaultValue(0)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectionStart
	{
		get
		{
			return base.TextSelectionInternal.Start.Offset;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", SR.ParameterCannotBeNegative);
			}
			int num = base.TextSelectionInternal.Start.GetOffsetToPosition(base.TextSelectionInternal.End);
			int symbolCount = base.TextContainer.SymbolCount;
			if (value > symbolCount)
			{
				value = symbolCount;
			}
			TextPointer textPointer = base.TextContainer.CreatePointerAtOffset(value, LogicalDirection.Forward);
			textPointer = textPointer.GetInsertionPosition(LogicalDirection.Forward);
			int offsetToPosition = textPointer.GetOffsetToPosition(base.TextContainer.End);
			if (num > offsetToPosition)
			{
				num = offsetToPosition;
			}
			TextPointer textPointer2 = new TextPointer(textPointer, num, LogicalDirection.Forward);
			textPointer2 = textPointer2.GetInsertionPosition(LogicalDirection.Forward);
			base.TextSelectionInternal.Select(textPointer, textPointer2);
		}
	}

	/// <summary>Gets or sets the insertion position index of the caret.</summary>
	/// <returns>The zero-based insertion position index of the caret. </returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int CaretIndex
	{
		get
		{
			return SelectionStart;
		}
		set
		{
			Select(value, 0);
		}
	}

	/// <summary>Gets the total number of lines in the text box.</summary>
	/// <returns>The total number of lines in the text box, or –1 if layout information is not available.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int LineCount
	{
		get
		{
			if (base.RenderScope == null)
			{
				return -1;
			}
			return GetLineIndexFromCharacterIndex(base.TextContainer.SymbolCount) + 1;
		}
	}

	/// <summary>Gets the text decorations to apply to the text box.</summary>
	/// <returns>A <see cref="T:System.Windows.TextDecorationCollection" /> collection that contains text decorations to apply to the text box. The default is null (no text decorations applied).</returns>
	public TextDecorationCollection TextDecorations
	{
		get
		{
			return (TextDecorationCollection)GetValue(TextDecorationsProperty);
		}
		set
		{
			SetValue(TextDecorationsProperty, value);
		}
	}

	/// <summary>Gets the currently effective typography variations for the text contents of the text box.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.Typography" /> object that specifies the currently effective typography variations. For a list of default typography values, see <see cref="T:System.Windows.Documents.Typography" />.</returns>
	public Typography Typography => new Typography(this);

	/// <summary>Gets an enumerator for the logical child elements of the <see cref="T:System.Windows.Controls.TextBox" />.</summary>
	/// <returns>An enumerator for the logical child elements of the <see cref="T:System.Windows.Controls.TextBox" />.</returns>
	protected internal override IEnumerator LogicalChildren => new RangeContentEnumerator(base.TextContainer.Start, base.TextContainer.End);

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	internal override int EffectiveValuesInitialSize => 42;

	internal TextSelection Selection => base.TextSelectionInternal;

	internal TextPointer StartPosition => base.TextContainer.Start;

	internal TextPointer EndPosition => base.TextContainer.End;

	internal bool IsTypographyDefaultValue => !_isTypographySet;

	ITextContainer ITextBoxViewHost.TextContainer => base.TextContainer;

	bool ITextBoxViewHost.IsTypographyDefaultValue => IsTypographyDefaultValue;

	static TextBox()
	{
		TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof(TextBox), new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTextWrappingChanged));
		MinLinesProperty = DependencyProperty.Register("MinLines", typeof(int), typeof(TextBox), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinMaxChanged), MinLinesValidateValue);
		MaxLinesProperty = DependencyProperty.Register("MaxLines", typeof(int), typeof(TextBox), new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinMaxChanged), MaxLinesValidateValue);
		TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnTextPropertyChanged, CoerceText, isAnimationProhibited: true, UpdateSourceTrigger.LostFocus));
		CharacterCasingProperty = DependencyProperty.Register("CharacterCasing", typeof(CharacterCasing), typeof(TextBox), new FrameworkPropertyMetadata(CharacterCasing.Normal), CharacterCasingValidateValue);
		MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof(int), typeof(TextBox), new FrameworkPropertyMetadata(0), MaxLengthValidateValue);
		TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(TextBox));
		TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(TextBox), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextDecorationCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TextBox));
		PropertyChangedCallback propertyChangedCallback = OnMinMaxChanged;
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(propertyChangedCallback));
		FrameworkElement.MinHeightProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(propertyChangedCallback));
		FrameworkElement.MaxHeightProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(propertyChangedCallback));
		Control.FontFamilyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(propertyChangedCallback));
		Control.FontSizeProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(propertyChangedCallback));
		PropertyChangedCallback propertyChangedCallback2 = OnTypographyChanged;
		DependencyProperty[] typographyPropertiesList = Typography.TypographyPropertiesList;
		for (int i = 0; i < typographyPropertiesList.Length; i++)
		{
			typographyPropertiesList[i].OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(propertyChangedCallback2));
		}
		TextBoxBase.HorizontalScrollBarVisibilityProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, TextBoxBase.OnScrollViewerPropertyChanged, CoerceHorizontalScrollBarVisibility));
		ControlsTraceLogger.AddControl(TelemetryControls.TextBox);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TextBox" /> class.</summary>
	public TextBox()
	{
		TextEditor.RegisterCommandHandlers(typeof(TextBox), acceptsRichContent: false, readOnly: false, registerEventListeners: false);
		InitializeTextContainer(new TextContainer(this, plainTextOnly: true)
		{
			CollectTextChanges = true
		});
		base.TextEditor.AcceptsRichContent = false;
	}

	/// <summary>Throws an exception in all cases.</summary>
	/// <param name="value">An object to add as a child.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">In all other cases.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		throw new InvalidOperationException(SR.Format(SR.TextBoxInvalidChild, value.ToString()));
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.TextContainer.End.InsertTextInRun(text);
	}

	/// <summary>Selects a range of text in the text box.</summary>
	/// <param name="start">The zero-based character index of the first character in the selection.</param>
	/// <param name="length">The length of the selection, in characters.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="start" /> or <paramref name="length" /> is negative.</exception>
	public void Select(int start, int length)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start", SR.ParameterCannotBeNegative);
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", SR.ParameterCannotBeNegative);
		}
		int symbolCount = base.TextContainer.SymbolCount;
		if (start > symbolCount)
		{
			start = symbolCount;
		}
		TextPointer textPointer = base.TextContainer.CreatePointerAtOffset(start, LogicalDirection.Forward);
		textPointer = textPointer.GetInsertionPosition(LogicalDirection.Forward);
		int offsetToPosition = textPointer.GetOffsetToPosition(base.TextContainer.End);
		if (length > offsetToPosition)
		{
			length = offsetToPosition;
		}
		TextPointer textPointer2 = new TextPointer(textPointer, length, LogicalDirection.Forward);
		textPointer2 = textPointer2.GetInsertionPosition(LogicalDirection.Forward);
		base.TextSelectionInternal.Select(textPointer, textPointer2);
	}

	/// <summary>Clears all the content from the text box.</summary>
	public void Clear()
	{
		using (base.TextSelectionInternal.DeclareChangeBlock())
		{
			base.TextContainer.DeleteContentInternal(base.TextContainer.Start, base.TextContainer.End);
			base.TextSelectionInternal.Select(base.TextContainer.Start, base.TextContainer.Start);
		}
	}

	/// <summary>Returns the zero-based index of the character that is closest to the specified point.</summary>
	/// <returns>The index of the character that is closest to the specified point, or –1 if no valid index can be found.</returns>
	/// <param name="point">A point in <see cref="T:System.Windows.Controls.TextBox" /> coordinate-space for which to return an index.</param>
	/// <param name="snapToText">true to return the nearest index if there is no character at the specified point; false to return –1 if there is no character at the specified point.</param>
	public int GetCharacterIndexFromPoint(Point point, bool snapToText)
	{
		if (base.RenderScope == null)
		{
			return -1;
		}
		TextPointer textPositionFromPointInternal = GetTextPositionFromPointInternal(point, snapToText);
		if (textPositionFromPointInternal != null)
		{
			int offset = textPositionFromPointInternal.Offset;
			if (textPositionFromPointInternal.LogicalDirection != 0)
			{
				return offset;
			}
			return offset - 1;
		}
		return -1;
	}

	/// <summary>Returns the zero-based character index for the first character in the specified line.</summary>
	/// <returns>The zero-based index for the first character in the specified line.</returns>
	/// <param name="lineIndex">The zero-based index of the line to retrieve the initial character index for.</param>
	public int GetCharacterIndexFromLineIndex(int lineIndex)
	{
		if (base.RenderScope == null)
		{
			return -1;
		}
		if (lineIndex < 0 || lineIndex >= LineCount)
		{
			throw new ArgumentOutOfRangeException("lineIndex");
		}
		return GetStartPositionOfLine(lineIndex)?.Offset ?? 0;
	}

	/// <summary>Returns the zero-based line index for the line that contains the specified character index.</summary>
	/// <returns>The zero-based index for the line that contains the specified character index.</returns>
	/// <param name="charIndex">The zero-based character index for which to retrieve the associated line index.</param>
	public int GetLineIndexFromCharacterIndex(int charIndex)
	{
		if (base.RenderScope == null)
		{
			return -1;
		}
		if (charIndex < 0 || charIndex > base.TextContainer.SymbolCount)
		{
			throw new ArgumentOutOfRangeException("charIndex");
		}
		if (base.TextContainer.CreatePointerAtOffset(charIndex, LogicalDirection.Forward).ValidateLayout())
		{
			return ((TextBoxView)base.RenderScope).GetLineIndexFromOffset(charIndex);
		}
		return -1;
	}

	/// <summary>Returns the number of characters in the specified line.</summary>
	/// <returns>The number of characters in the specified line.</returns>
	/// <param name="lineIndex">The zero-based line index for which to return a character count.</param>
	public int GetLineLength(int lineIndex)
	{
		if (base.RenderScope == null)
		{
			return -1;
		}
		if (lineIndex < 0 || lineIndex >= LineCount)
		{
			throw new ArgumentOutOfRangeException("lineIndex");
		}
		TextPointer startPositionOfLine = GetStartPositionOfLine(lineIndex);
		TextPointer endPositionOfLine = GetEndPositionOfLine(lineIndex);
		if (startPositionOfLine == null || endPositionOfLine == null)
		{
			return -1;
		}
		return startPositionOfLine.GetOffsetToPosition(endPositionOfLine);
	}

	/// <summary>Returns the line index for the first line that is currently visible in the text box.</summary>
	/// <returns>The zero-based index for the first visible line in the text box.</returns>
	public int GetFirstVisibleLineIndex()
	{
		if (base.RenderScope == null)
		{
			return -1;
		}
		double lineHeight = GetLineHeight();
		return (int)Math.Floor(base.VerticalOffset / lineHeight + 0.0001);
	}

	/// <summary>Returns the line index for the last line that is currently visible in the text box.</summary>
	/// <returns>The zero-based index for the last visible line in the text box.</returns>
	public int GetLastVisibleLineIndex()
	{
		if (base.RenderScope == null)
		{
			return -1;
		}
		double extentHeight = ((IScrollInfo)base.RenderScope).ExtentHeight;
		if (base.VerticalOffset + base.ViewportHeight >= extentHeight)
		{
			return LineCount - 1;
		}
		return (int)Math.Floor((base.VerticalOffset + base.ViewportHeight - 1.0) / GetLineHeight());
	}

	/// <summary>Scrolls the line at the specified line index into view.</summary>
	/// <param name="lineIndex">The zero-based line index of the line to scroll into view.</param>
	public void ScrollToLine(int lineIndex)
	{
		if (base.RenderScope != null)
		{
			if (lineIndex < 0 || lineIndex >= LineCount)
			{
				throw new ArgumentOutOfRangeException("lineIndex");
			}
			TextPointer startPositionOfLine = GetStartPositionOfLine(lineIndex);
			if (GetRectangleFromTextPositionInternal(startPositionOfLine, relativeToTextBox: false, out var rect))
			{
				base.RenderScope.BringIntoView(rect);
			}
		}
	}

	/// <summary>Returns the text that is currently displayed on the specified line.</summary>
	/// <returns>A string containing a copy of the text currently visible on the specified line.</returns>
	/// <param name="lineIndex">The zero-based line index for which to retrieve the currently displayed text.</param>
	public string GetLineText(int lineIndex)
	{
		if (base.RenderScope == null)
		{
			return null;
		}
		if (lineIndex < 0 || lineIndex >= LineCount)
		{
			throw new ArgumentOutOfRangeException("lineIndex");
		}
		TextPointer startPositionOfLine = GetStartPositionOfLine(lineIndex);
		TextPointer endPositionOfLine = GetEndPositionOfLine(lineIndex);
		if (startPositionOfLine != null && endPositionOfLine != null)
		{
			return TextRangeBase.GetTextInternal(startPositionOfLine, endPositionOfLine);
		}
		return Text;
	}

	/// <summary>Returns the rectangle for the leading edge of the character at the specified index.</summary>
	/// <returns>A rectangle for the leading edge of the character at the specified character index, or <see cref="P:System.Windows.Rect.Empty" /> if a bounding rectangle cannot be determined.</returns>
	/// <param name="charIndex">The zero-based character index of the character for which to retrieve the rectangle.</param>
	public Rect GetRectFromCharacterIndex(int charIndex)
	{
		return GetRectFromCharacterIndex(charIndex, trailingEdge: false);
	}

	/// <summary>Returns the rectangle for the leading or trailing edge of the character at the specified index.</summary>
	/// <returns>A rectangle for an edge of the character at the specified character index, or <see cref="P:System.Windows.Rect.Empty" /> if a bounding rectangle cannot be determined.</returns>
	/// <param name="charIndex">The zero-based character index of the character for which to retrieve the rectangle.</param>
	/// <param name="trailingEdge">true to get the trailing edge of the character; false to get the leading edge of the character.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="charIndex" /> is negative or is greater than the length of the content.</exception>
	public Rect GetRectFromCharacterIndex(int charIndex, bool trailingEdge)
	{
		if (charIndex < 0 || charIndex > base.TextContainer.SymbolCount)
		{
			throw new ArgumentOutOfRangeException("charIndex");
		}
		TextPointer textPointer = base.TextContainer.CreatePointerAtOffset(charIndex, LogicalDirection.Backward);
		textPointer = textPointer.GetInsertionPosition(LogicalDirection.Backward);
		if (trailingEdge && charIndex < base.TextContainer.SymbolCount)
		{
			textPointer = textPointer.GetNextInsertionPosition(LogicalDirection.Forward);
			Invariant.Assert(textPointer != null);
			textPointer = textPointer.GetPositionAtOffset(0, LogicalDirection.Backward);
		}
		else
		{
			textPointer = textPointer.GetPositionAtOffset(0, LogicalDirection.Forward);
		}
		GetRectangleFromTextPositionInternal(textPointer, relativeToTextBox: true, out var rect);
		return rect;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Controls.SpellingError" /> object associated with any spelling error at the specified character index.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.SpellingError" /> object containing the details of the spelling error found at the character indicated by <paramref name="charIndex" />, or null if no spelling error exists at the specified character.</returns>
	/// <param name="charIndex">The zero-based character index of a position in content to examine for a spelling error.</param>
	public SpellingError GetSpellingError(int charIndex)
	{
		if (charIndex < 0 || charIndex > base.TextContainer.SymbolCount)
		{
			throw new ArgumentOutOfRangeException("charIndex");
		}
		TextPointer position = base.TextContainer.CreatePointerAtOffset(charIndex, LogicalDirection.Forward);
		SpellingError spellingErrorAtPosition = base.TextEditor.GetSpellingErrorAtPosition(position, LogicalDirection.Forward);
		if (spellingErrorAtPosition == null && charIndex < base.TextContainer.SymbolCount - 1)
		{
			position = base.TextContainer.CreatePointerAtOffset(charIndex + 1, LogicalDirection.Forward);
			spellingErrorAtPosition = base.TextEditor.GetSpellingErrorAtPosition(position, LogicalDirection.Backward);
		}
		return spellingErrorAtPosition;
	}

	/// <summary>Returns the beginning character index for any spelling error that includes the specified character.</summary>
	/// <returns>The beginning character index for any spelling error that includes the character specified by <paramref name="charIndex" />, or –1 if the specified character is not part of a spelling error.</returns>
	/// <param name="charIndex">The zero-based character index of a position in content to examine for a spelling error.</param>
	public int GetSpellingErrorStart(int charIndex)
	{
		return GetSpellingError(charIndex)?.Start.Offset ?? (-1);
	}

	/// <summary>Returns the length of any spelling error that includes the specified character.</summary>
	/// <returns>The length of any spelling error that includes the character specified by charIndex, or 0 if the specified character is not part of a spelling error.</returns>
	/// <param name="charIndex">The zero-based character index of a position in content to examine for a spelling error.</param>
	public int GetSpellingErrorLength(int charIndex)
	{
		SpellingError spellingError = GetSpellingError(charIndex);
		if (spellingError != null)
		{
			return spellingError.End.Offset - spellingError.Start.Offset;
		}
		return 0;
	}

	/// <summary>Returns the beginning character index for the next spelling error in the contents of the text box.</summary>
	/// <returns>The character index for the beginning of the next spelling error in the contents of the text box, or –1 if no next spelling error exists.</returns>
	/// <param name="charIndex">The zero-based character index indicating a position from which to search for the next spelling error.</param>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the direction in which to search for the next spelling error, starting at the specified <paramref name="charIndex" />.</param>
	public int GetNextSpellingErrorCharacterIndex(int charIndex, LogicalDirection direction)
	{
		if (charIndex < 0 || charIndex > base.TextContainer.SymbolCount)
		{
			throw new ArgumentOutOfRangeException("charIndex");
		}
		if (base.TextContainer.SymbolCount == 0)
		{
			return -1;
		}
		ITextPointer position = base.TextContainer.CreatePointerAtOffset(charIndex, direction);
		return base.TextEditor.GetNextSpellingErrorPosition(position, direction)?.Offset ?? (-1);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for the text box.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for the text box.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TextBoxAutomationPeer(this);
	}

	/// <summary>Called when one or more of the dependency properties that exist on the element have had their effective values changed.</summary>
	/// <param name="e">Arguments for the associated event.</param>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (base.RenderScope != null && e.Property.GetMetadata(typeof(TextBox)) is FrameworkPropertyMetadata frameworkPropertyMetadata && (e.IsAValueChange || e.IsASubPropertyChange || e.Property == TextAlignmentProperty))
		{
			if (frameworkPropertyMetadata.AffectsMeasure || frameworkPropertyMetadata.AffectsArrange || frameworkPropertyMetadata.AffectsParentMeasure || frameworkPropertyMetadata.AffectsParentArrange || e.Property == Control.HorizontalContentAlignmentProperty || e.Property == Control.VerticalContentAlignmentProperty)
			{
				((TextBoxView)base.RenderScope).Remeasure();
			}
			else if (frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender))
			{
				((TextBoxView)base.RenderScope).Rerender();
			}
			if (Speller.IsSpellerAffectingProperty(e.Property) && base.TextEditor.Speller != null)
			{
				base.TextEditor.Speller.ResetErrors();
			}
		}
		if (UIElementAutomationPeer.FromElement(this) is TextBoxAutomationPeer textBoxAutomationPeer)
		{
			if (e.Property == TextProperty)
			{
				textBoxAutomationPeer.RaiseValuePropertyChangedEvent((string)e.OldValue, (string)e.NewValue);
			}
			if (e.Property == TextBoxBase.IsReadOnlyProperty)
			{
				textBoxAutomationPeer.RaiseIsReadOnlyPropertyChangedEvent((bool)e.OldValue, (bool)e.NewValue);
			}
		}
	}

	/// <summary>Sizes the text box to its content.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> structure indicating the new size of the text box.</returns>
	/// <param name="constraint">A <see cref="T:System.Windows.Size" /> structure that specifies the constraints on the size of the text box.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		if (MinLines > 1 && MaxLines < MinLines)
		{
			throw new Exception(SR.TextBoxMinMaxLinesMismatch);
		}
		Size result = base.MeasureOverride(constraint);
		if (_minmaxChanged)
		{
			if (base.ScrollViewer == null)
			{
				SetRenderScopeMinMaxHeight();
			}
			else
			{
				SetScrollViewerMinMaxHeight();
			}
			_minmaxChanged = false;
		}
		return result;
	}

	internal void OnTextWrappingChanged()
	{
		CoerceValue(TextBoxBase.HorizontalScrollBarVisibilityProperty);
	}

	internal override FrameworkElement CreateRenderScope()
	{
		return new TextBoxView(this);
	}

	internal override void AttachToVisualTree()
	{
		base.AttachToVisualTree();
		if (base.RenderScope != null)
		{
			OnTextWrappingChanged();
			_minmaxChanged = true;
		}
	}

	internal override string GetPlainText()
	{
		return Text;
	}

	internal override void DoLineUp()
	{
		if (base.ScrollViewer != null)
		{
			base.ScrollViewer.ScrollToVerticalOffset(base.VerticalOffset - GetLineHeight());
		}
	}

	internal override void DoLineDown()
	{
		if (base.ScrollViewer != null)
		{
			base.ScrollViewer.ScrollToVerticalOffset(base.VerticalOffset + GetLineHeight());
		}
	}

	internal override void OnTextContainerChanged(object sender, TextContainerChangedEventArgs e)
	{
		bool flag = false;
		string text = null;
		try
		{
			_changeEventNestingCount++;
			if (!_isInsideTextContentChange)
			{
				_isInsideTextContentChange = true;
				SetCurrentDeferredValue(deferredReference: (DeferredReference)(_newTextValue = new DeferredTextReference(base.TextContainer)), dp: TextProperty);
			}
		}
		finally
		{
			_changeEventNestingCount--;
			if (_changeEventNestingCount == 0)
			{
				if (FrameworkCompatibilityPreferences.GetKeepTextBoxDisplaySynchronizedWithTextProperty())
				{
					text = _newTextValue as string;
					flag = text != null && text != Text;
				}
				_isInsideTextContentChange = false;
				_newTextValue = DependencyProperty.UnsetValue;
			}
		}
		if (flag)
		{
			try
			{
				_newTextValue = text;
				_isInsideTextContentChange = true;
				_changeEventNestingCount++;
				OnTextPropertyChanged(text, Text);
			}
			finally
			{
				_changeEventNestingCount--;
				_isInsideTextContentChange = false;
				_newTextValue = DependencyProperty.UnsetValue;
			}
		}
		if (_changeEventNestingCount == 0)
		{
			base.OnTextContainerChanged(sender, e);
		}
	}

	internal void OnDeferredTextReferenceResolved(DeferredTextReference dtr, string s)
	{
		if (dtr == _newTextValue)
		{
			_newTextValue = s;
		}
	}

	internal override void OnScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		base.OnScrollChanged(sender, e);
		if (e.ViewportHeightChange != 0.0)
		{
			SetScrollViewerMinMaxHeight();
		}
	}

	internal void RaiseCourtesyTextChangedEvent()
	{
		OnTextChanged(new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None));
	}

	private bool GetRectangleFromTextPositionInternal(TextPointer position, bool relativeToTextBox, out Rect rect)
	{
		if (base.RenderScope == null)
		{
			rect = Rect.Empty;
			return false;
		}
		if (position.ValidateLayout())
		{
			rect = TextPointerBase.GetCharacterRect(position, position.LogicalDirection, relativeToTextBox);
		}
		else
		{
			rect = Rect.Empty;
		}
		return rect != Rect.Empty;
	}

	private TextPointer GetStartPositionOfLine(int lineIndex)
	{
		if (base.RenderScope == null)
		{
			return null;
		}
		Point point = default(Point);
		double lineHeight = GetLineHeight();
		point.Y = lineHeight * (double)lineIndex + lineHeight / 2.0 - base.VerticalOffset;
		point.X = 0.0 - base.HorizontalOffset;
		if (TextEditor.GetTextView(base.RenderScope).Validate(point))
		{
			TextPointer textPointer = (TextPointer)TextEditor.GetTextView(base.RenderScope).GetTextPositionFromPoint(point, snapToText: true);
			return (TextPointer)TextEditor.GetTextView(base.RenderScope).GetLineRange(textPointer).Start.CreatePointer(textPointer.LogicalDirection);
		}
		return null;
	}

	private TextPointer GetEndPositionOfLine(int lineIndex)
	{
		if (base.RenderScope == null)
		{
			return null;
		}
		Point point = default(Point);
		double lineHeight = GetLineHeight();
		point.Y = lineHeight * (double)lineIndex + lineHeight / 2.0 - base.VerticalOffset;
		point.X = 0.0;
		TextPointer textPointer;
		if (TextEditor.GetTextView(base.RenderScope).Validate(point))
		{
			textPointer = (TextPointer)TextEditor.GetTextView(base.RenderScope).GetTextPositionFromPoint(point, snapToText: true);
			textPointer = (TextPointer)TextEditor.GetTextView(base.RenderScope).GetLineRange(textPointer).End.CreatePointer(textPointer.LogicalDirection);
			if (TextPointerBase.IsNextToPlainLineBreak(textPointer, LogicalDirection.Forward))
			{
				textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
			}
		}
		else
		{
			textPointer = null;
		}
		return textPointer;
	}

	private static object CoerceHorizontalScrollBarVisibility(DependencyObject d, object value)
	{
		if (d is TextBox textBox && (textBox.TextWrapping == TextWrapping.Wrap || textBox.TextWrapping == TextWrapping.WrapWithOverflow))
		{
			return ScrollBarVisibility.Disabled;
		}
		return value;
	}

	private static bool MaxLengthValidateValue(object value)
	{
		return (int)value >= 0;
	}

	private static bool CharacterCasingValidateValue(object value)
	{
		if (CharacterCasing.Normal <= (CharacterCasing)value)
		{
			return (CharacterCasing)value <= CharacterCasing.Upper;
		}
		return false;
	}

	private static bool MinLinesValidateValue(object value)
	{
		return (int)value > 0;
	}

	private static bool MaxLinesValidateValue(object value)
	{
		return (int)value > 0;
	}

	private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBox)d)._minmaxChanged = true;
	}

	private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBox textBox = (TextBox)d;
		if (!textBox._isInsideTextContentChange || (textBox._newTextValue != DependencyProperty.UnsetValue && !(textBox._newTextValue is DeferredTextReference)))
		{
			textBox.OnTextPropertyChanged((string)e.OldValue, (string)e.NewValue);
		}
	}

	private void OnTextPropertyChanged(string oldText, string newText)
	{
		bool flag = false;
		int start = 0;
		bool flag2 = false;
		if (_isInsideTextContentChange)
		{
			if (object.Equals(_newTextValue, newText))
			{
				return;
			}
			flag = true;
		}
		if (newText == null)
		{
			newText = string.Empty;
		}
		bool flag3 = HasExpression(LookupEntry(TextProperty.GlobalIndex), TextProperty);
		string oldText2 = oldText;
		if (flag)
		{
			flag2 = true;
			oldText2 = (string)_newTextValue;
		}
		else if (flag3)
		{
			BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpression(this, TextProperty);
			flag2 = bindingExpression != null && bindingExpression.IsInUpdate && bindingExpression.IsInTransfer;
		}
		if (flag2)
		{
			start = ChooseCaretIndex(CaretIndex, oldText2, newText);
		}
		if (flag)
		{
			_newTextValue = newText;
		}
		_isInsideTextContentChange = true;
		try
		{
			using (base.TextSelectionInternal.DeclareChangeBlock())
			{
				base.TextContainer.DeleteContentInternal(base.TextContainer.Start, base.TextContainer.End);
				base.TextContainer.End.InsertTextInRun(newText);
				Select(start, 0);
			}
		}
		finally
		{
			if (!flag)
			{
				_isInsideTextContentChange = false;
			}
		}
		if (flag3)
		{
			UndoManager undoManager = base.TextEditor._GetUndoManager();
			if (undoManager != null && undoManager.IsEnabled)
			{
				undoManager.Clear();
			}
		}
	}

	private int ChooseCaretIndex(int oldIndex, string oldText, string newText)
	{
		int num = newText.IndexOf(oldText, StringComparison.Ordinal);
		if (oldText.Length > 0 && num >= 0)
		{
			return num + oldIndex;
		}
		if (oldIndex == 0)
		{
			return 0;
		}
		if (oldIndex == oldText.Length)
		{
			return newText.Length;
		}
		int i;
		for (i = 0; i < oldText.Length && i < newText.Length && oldText[i] == newText[i]; i++)
		{
		}
		int j;
		for (j = 0; j < oldText.Length && j < newText.Length && oldText[oldText.Length - 1 - j] == newText[newText.Length - 1 - j]; j++)
		{
		}
		if (2 * (i + j) >= Math.Min(oldText.Length, newText.Length))
		{
			if (oldIndex <= i)
			{
				return oldIndex;
			}
			if (oldIndex >= oldText.Length - j)
			{
				return newText.Length - (oldText.Length - oldIndex);
			}
		}
		char value = oldText[oldIndex - 1];
		int num2 = newText.IndexOf(value);
		int num3 = -1;
		int num4 = 1;
		while (num2 >= 0)
		{
			int num5 = 1;
			num = num2 - 1;
			while (num >= 0 && oldIndex - (num2 - num) >= 0 && newText[num] == oldText[oldIndex - (num2 - num)])
			{
				num5++;
				num--;
			}
			for (num = num2 + 1; num < newText.Length && oldIndex + (num - num2) < oldText.Length && newText[num] == oldText[oldIndex + (num - num2)]; num++)
			{
				num5++;
			}
			if (num5 > num4)
			{
				num3 = num2 + 1;
				num4 = num5;
			}
			num2 = newText.IndexOf(value, num2 + 1);
		}
		if (num3 >= 0)
		{
			return num3;
		}
		return newText.Length;
	}

	private static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TextBox)
		{
			((TextBox)d).OnTextWrappingChanged();
		}
	}

	private void SetScrollViewerMinMaxHeight()
	{
		if (base.RenderScope == null)
		{
			return;
		}
		if (ReadLocalValue(FrameworkElement.HeightProperty) != DependencyProperty.UnsetValue || ReadLocalValue(FrameworkElement.MaxHeightProperty) != DependencyProperty.UnsetValue || ReadLocalValue(FrameworkElement.MinHeightProperty) != DependencyProperty.UnsetValue)
		{
			base.ScrollViewer.ClearValue(FrameworkElement.MinHeightProperty);
			base.ScrollViewer.ClearValue(FrameworkElement.MaxHeightProperty);
			return;
		}
		double num = base.ScrollViewer.ActualHeight - base.ViewportHeight;
		double lineHeight = GetLineHeight();
		double num2 = num + lineHeight * (double)MinLines;
		if (MinLines > 1 && base.ScrollViewer.MinHeight != num2)
		{
			base.ScrollViewer.MinHeight = num2;
		}
		num2 = num + lineHeight * (double)MaxLines;
		if (MaxLines < int.MaxValue && base.ScrollViewer.MaxHeight != num2)
		{
			base.ScrollViewer.MaxHeight = num2;
		}
	}

	private void SetRenderScopeMinMaxHeight()
	{
		if (base.RenderScope == null)
		{
			return;
		}
		if (ReadLocalValue(FrameworkElement.HeightProperty) != DependencyProperty.UnsetValue || ReadLocalValue(FrameworkElement.MaxHeightProperty) != DependencyProperty.UnsetValue || ReadLocalValue(FrameworkElement.MinHeightProperty) != DependencyProperty.UnsetValue)
		{
			base.RenderScope.ClearValue(FrameworkElement.MinHeightProperty);
			base.RenderScope.ClearValue(FrameworkElement.MaxHeightProperty);
			return;
		}
		double lineHeight = GetLineHeight();
		double num = lineHeight * (double)MinLines;
		if (MinLines > 1 && base.RenderScope.MinHeight != num)
		{
			base.RenderScope.MinHeight = num;
		}
		num = lineHeight * (double)MaxLines;
		if (MaxLines < int.MaxValue && base.RenderScope.MaxHeight != num)
		{
			base.RenderScope.MaxHeight = num;
		}
	}

	private double GetLineHeight()
	{
		FontFamily fontFamily = (FontFamily)GetValue(Control.FontFamilyProperty);
		double num = (double)GetValue(TextElement.FontSizeProperty);
		if (TextOptions.GetTextFormattingMode(this) == TextFormattingMode.Ideal)
		{
			return fontFamily.LineSpacing * num;
		}
		return fontFamily.GetLineSpacingForDisplayMode(num, GetDpi().DpiScaleY);
	}

	/// <summary>Returns a value that indicates whether the effective value of the <see cref="P:System.Windows.Controls.TextBox.Text" /> property should be serialized during serialization of the <see cref="T:System.Windows.Controls.TextBox" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.TextBox.Text" /> property should be serialized; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for this object.</param>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="manager" /> is null.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeText(XamlDesignerSerializationManager manager)
	{
		return manager.XmlWriter == null;
	}

	private static void OnQueryScrollCommand(object target, CanExecuteRoutedEventArgs args)
	{
		args.CanExecute = true;
	}

	private static object CoerceText(DependencyObject d, object value)
	{
		if (value == null)
		{
			return string.Empty;
		}
		return value;
	}

	private static void OnTypographyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBox)d)._isTypographySet = true;
	}
}
