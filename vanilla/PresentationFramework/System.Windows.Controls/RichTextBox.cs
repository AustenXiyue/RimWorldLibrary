using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Documents;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a rich editing control which operates on <see cref="T:System.Windows.Documents.FlowDocument" /> objects.</summary>
[Localizability(LocalizationCategory.Inherit)]
[ContentProperty("Document")]
public class RichTextBox : TextBoxBase, IAddChild
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.RichTextBox.IsDocumentEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.RichTextBox.IsDocumentEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsDocumentEnabledProperty;

	private FlowDocument _document;

	private bool _implicitDocument;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Documents.FlowDocument" /> that represents the contents of the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.FlowDocument" /> object that represents the contents of the <see cref="T:System.Windows.Controls.RichTextBox" />.By default, this property is set to an empty <see cref="T:System.Windows.Documents.FlowDocument" />.  Specifically, the empty <see cref="T:System.Windows.Documents.FlowDocument" /> contains a single <see cref="T:System.Windows.Documents.Paragraph" />, which contains a single <see cref="T:System.Windows.Documents.Run" /> which contains no text.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set this property to null.</exception>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to a <see cref="T:System.Windows.Documents.FlowDocument" /> that represents the contents of another <see cref="T:System.Windows.Controls.RichTextBox" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">This property is set while a change block has been activated.</exception>
	public FlowDocument Document
	{
		get
		{
			Invariant.Assert(_document != null);
			return _document;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value != _document && value.StructuralCache != null && value.StructuralCache.TextContainer != null && value.StructuralCache.TextContainer.TextSelection != null)
			{
				throw new ArgumentException(SR.RichTextBox_DocumentBelongsToAnotherRichTextBoxAlready);
			}
			if (_document != null && base.TextSelectionInternal.ChangeBlockLevel > 0)
			{
				throw new InvalidOperationException(SR.RichTextBox_CantSetDocumentInsideChangeBlock);
			}
			if (value == _document)
			{
				return;
			}
			bool num = _document == null;
			if (_document != null)
			{
				_document.PageSizeChanged -= OnPageSizeChangedHandler;
				RemoveLogicalChild(_document);
				_document.TextContainer.CollectTextChanges = false;
				_document = null;
			}
			if (!num)
			{
				_implicitDocument = false;
			}
			_document = value;
			_document.SetDpi(GetDpi());
			FrameworkElement renderScope = base.RenderScope;
			_document.TextContainer.CollectTextChanges = true;
			InitializeTextContainer(_document.TextContainer);
			_document.PageSizeChanged += OnPageSizeChangedHandler;
			AddLogicalChild(_document);
			if (renderScope != null)
			{
				AttachToVisualTree();
			}
			TransferInheritedPropertiesToFlowDocument();
			if (num)
			{
				return;
			}
			ChangeUndoLimit(base.UndoLimit);
			ChangeUndoEnabled(base.IsUndoEnabled);
			Invariant.Assert(base.PendingUndoAction == UndoAction.None);
			base.PendingUndoAction = UndoAction.Clear;
			try
			{
				OnTextChanged(new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.Clear));
			}
			finally
			{
				base.PendingUndoAction = UndoAction.None;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can interact with <see cref="T:System.Windows.UIElement" /> and <see cref="T:System.Windows.ContentElement" /> objects within the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>true if the user can interact with <see cref="T:System.Windows.UIElement" /> and <see cref="T:System.Windows.ContentElement" /> objects within the <see cref="T:System.Windows.Controls.RichTextBox" />; otherwise, false.</returns>
	public bool IsDocumentEnabled
	{
		get
		{
			return (bool)GetValue(IsDocumentEnabledProperty);
		}
		set
		{
			SetValue(IsDocumentEnabledProperty, value);
		}
	}

	/// <summary>Gets an enumerator that can iterate the logical children of the RichTextBox.</summary>
	/// <returns>An enumerator for the logical children.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (_document == null)
			{
				return EmptyEnumerator.Instance;
			}
			return new SingleChildEnumerator(_document);
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextSelection" /> object containing the current selection in the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextSelection" /> object containing the current selection in the <see cref="T:System.Windows.Controls.RichTextBox" />.The default returned <see cref="T:System.Windows.Documents.TextSelection" /> has an <see cref="P:System.Windows.Documents.TextRange.IsEmpty" /> property value of True. An empty <see cref="T:System.Windows.Documents.TextSelection" /> renders as a caret in the text area with no selection.</returns>
	public TextSelection Selection => base.TextSelectionInternal;

	/// <summary>Gets or sets the position of the input caret.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> object specifying the position of the input caret.By default, the caret is at the first insertion position at the beginning of the content hosted by the <see cref="T:System.Windows.Controls.RichTextBox" />. See <see cref="T:System.Windows.Documents.TextPointer" /> for more information on text position terminology like "insertion position".</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set this property to null.</exception>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to a <see cref="T:System.Windows.Documents.TextPointer" /> that references a position outside of the current document.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public TextPointer CaretPosition
	{
		get
		{
			return Selection.MovingPosition;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!Selection.Start.IsInSameDocument(value))
			{
				throw new ArgumentException(SR.RichTextBox_PointerNotInSameDocument, "value");
			}
			Selection.SetCaretToPosition(value, value.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: false);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static RichTextBox()
	{
		IsDocumentEnabledProperty = DependencyProperty.Register("IsDocumentEnabled", typeof(bool), typeof(RichTextBox), new FrameworkPropertyMetadata(false, OnIsDocumentEnabledChanged));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RichTextBox), new FrameworkPropertyMetadata(typeof(RichTextBox)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(RichTextBox));
		KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(typeof(RichTextBox), new FrameworkPropertyMetadata(true));
		TextBoxBase.AutoWordSelectionProperty.OverrideMetadata(typeof(RichTextBox), new FrameworkPropertyMetadata(true));
		if (!FrameworkAppContextSwitches.UseAdornerForTextboxSelectionRendering)
		{
			TextBoxBase.SelectionOpacityProperty.OverrideMetadata(typeof(RichTextBox), new FrameworkPropertyMetadata(0.4));
		}
		HookupInheritablePropertyListeners();
		ControlsTraceLogger.AddControl(TelemetryControls.RichTextBox);
	}

	/// <summary>Initializes a new, default instance of the <see cref="T:System.Windows.Controls.RichTextBox" /> class.</summary>
	public RichTextBox()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.RichTextBox" /> class, adding a specified <see cref="T:System.Windows.Documents.FlowDocument" /> as the initial content.</summary>
	/// <param name="document">A <see cref="T:System.Windows.Documents.FlowDocument" /> to be added as the initial contents of the new <see cref="T:System.Windows.Controls.RichTextBox" />.</param>
	public RichTextBox(FlowDocument document)
	{
		TextEditor.RegisterCommandHandlers(typeof(RichTextBox), acceptsRichContent: true, readOnly: false, registerEventListeners: false);
		if (document == null)
		{
			document = new FlowDocument();
			document.Blocks.Add(new Paragraph());
			_implicitDocument = true;
		}
		Document = document;
		Invariant.Assert(base.TextContainer != null);
		Invariant.Assert(base.TextEditor != null);
		Invariant.Assert(base.TextEditor.TextContainer == base.TextContainer);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value"> An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is FlowDocument))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(FlowDocument)), "value");
		}
		if (!_implicitDocument)
		{
			throw new ArgumentException(SR.Format(SR.CanOnlyHaveOneChild, GetType(), value.GetType()));
		}
		Document = (FlowDocument)value;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text"> A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> that points to the insertion point closest to the specified position.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> specifying the closest insertion position for the supplied point, or null if <paramref name="snapToText" /> is false and the supplied <see cref="T:System.Windows.Point" /> is not within any character's bounding box. Note that the <see cref="T:System.Windows.Documents.TextPointer" /> returned is usually the position between two characters. Use the <see cref="P:System.Windows.Documents.TextPointer.LogicalDirection" /> property of the returned <see cref="T:System.Windows.Documents.TextPointer" /> to determine which of the two characters the <see cref="T:System.Windows.Documents.TextPointer" /> corresponds to.</returns>
	/// <param name="point">A <see cref="T:System.Windows.Point" /> object specifying the position to retrieve a <see cref="T:System.Windows.Documents.TextPointer" /> for.</param>
	/// <param name="snapToText">If true, this method always returns a <see cref="T:System.Windows.Documents.TextPointer" /> specifying the closest insertion position for the <see cref="T:System.Windows.Point" /> specified, regardless or whether or not the supplied <see cref="T:System.Windows.Point" /> is inside a character's bounding box.If false, this method returns null if the specified <see cref="T:System.Windows.Point" /> does not fall within any character bounding box.</param>
	/// <exception cref="T:System.InvalidOperationException">Raised if layout information for the <see cref="T:System.Windows.Controls.RichTextBox" /> is not current.</exception>
	public TextPointer GetPositionFromPoint(Point point, bool snapToText)
	{
		if (base.RenderScope == null)
		{
			return null;
		}
		return GetTextPositionFromPointInternal(point, snapToText);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Controls.SpellingError" /> object associated with any spelling error at a specified position in the contents of the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.SpellingError" /> object containing the details of the spelling error found at the character indicated by <paramref name="position" />, or null if no spelling error exists at the specified character. </returns>
	/// <param name="position">A <see cref="T:System.Windows.Documents.TextPointer" /> that specifies a position and logical direction that resolves to a character to examine for a spelling error. Use the <see cref="P:System.Windows.Documents.TextPointer.LogicalDirection" /> property of this <see cref="T:System.Windows.Documents.TextPointer" /> to specify the direction of the character to examine.</param>
	public SpellingError GetSpellingError(TextPointer position)
	{
		ValidationHelper.VerifyPosition(base.TextContainer, position);
		return base.TextEditor.GetSpellingErrorAtPosition(position, position.LogicalDirection);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextRange" /> object covering any misspelled word at a specified position in the contents of the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextRange" /> object covering any misspelled word that includes the character specified by <paramref name="position" />, or null if no spelling error exists at the specified character.</returns>
	/// <param name="position">A <see cref="T:System.Windows.Documents.TextPointer" /> that specifies a position and logical direction that resolves to a character to examine for a spelling error. Use the <see cref="P:System.Windows.Documents.TextPointer.LogicalDirection" /> property of this <see cref="T:System.Windows.Documents.TextPointer" /> to specify the direction of the character to examine.</param>
	public TextRange GetSpellingErrorRange(TextPointer position)
	{
		ValidationHelper.VerifyPosition(base.TextContainer, position);
		SpellingError spellingErrorAtPosition = base.TextEditor.GetSpellingErrorAtPosition(position, position.LogicalDirection);
		if (spellingErrorAtPosition != null)
		{
			return new TextRange(spellingErrorAtPosition.Start, spellingErrorAtPosition.End);
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> that points to the next spelling error in the contents of the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> that points to the next spelling error in the contents of the <see cref="T:System.Windows.Controls.RichTextBox" />, or null if no next spelling error exists.</returns>
	/// <param name="position">A <see cref="T:System.Windows.Documents.TextPointer" /> indicating a position from which to search for the next spelling error.</param>
	/// <param name="direction">A <see cref="T:System.Windows.Documents.LogicalDirection" /> in which to search for the next spelling error, starting at the specified <paramref name="posision" />.</param>
	public TextPointer GetNextSpellingErrorPosition(TextPointer position, LogicalDirection direction)
	{
		ValidationHelper.VerifyPosition(base.TextContainer, position);
		return (TextPointer)base.TextEditor.GetNextSpellingErrorPosition(position, direction);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.RichTextBox" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new RichTextBoxAutomationPeer(this);
	}

	protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo)
	{
		Document?.SetDpi(newDpiScaleInfo);
	}

	/// <summary>Called to re-measure the <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> structure indicating the new size of the <see cref="T:System.Windows.Controls.RichTextBox" />.</returns>
	/// <param name="constraint">A <see cref="T:System.Windows.Size" /> structure specifying constraints on the size of the <see cref="T:System.Windows.Controls.RichTextBox" />.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		if (constraint.Width == double.PositiveInfinity)
		{
			constraint.Width = base.MinWidth;
		}
		return base.MeasureOverride(constraint);
	}

	internal override FrameworkElement CreateRenderScope()
	{
		FlowDocumentView flowDocumentView = new FlowDocumentView();
		flowDocumentView.Document = Document;
		flowDocumentView.Document.PagePadding = new Thickness(5.0, 0.0, 5.0, 0.0);
		flowDocumentView.OverridesDefaultStyle = true;
		return flowDocumentView;
	}

	/// <summary>Returns a value that indicates whether or not the effective value of the <see cref="P:System.Windows.Controls.RichTextBox.Document" /> property should be serialized during serialization of a <see cref="T:System.Windows.Controls.RichTextBox" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.RichTextBox.Document" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeDocument()
	{
		Block firstBlock = _document.Blocks.FirstBlock;
		if (_implicitDocument && (firstBlock == null || (firstBlock == _document.Blocks.LastBlock && firstBlock is Paragraph)))
		{
			Inline inline = ((firstBlock == null) ? null : ((Paragraph)firstBlock).Inlines.FirstInline);
			if (inline == null || (inline == ((Paragraph)firstBlock).Inlines.LastInline && inline is Run && inline.ContentStart.CompareTo(inline.ContentEnd) == 0))
			{
				return false;
			}
		}
		return true;
	}

	private static void HookupInheritablePropertyListeners()
	{
		PropertyChangedCallback propertyChangedCallback = OnFormattingPropertyChanged;
		DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(FlowDocument));
		for (int i = 0; i < inheritableProperties.Length; i++)
		{
			inheritableProperties[i].OverrideMetadata(typeof(RichTextBox), new FrameworkPropertyMetadata(propertyChangedCallback));
		}
		PropertyChangedCallback propertyChangedCallback2 = OnBehavioralPropertyChanged;
		DependencyProperty[] behavioralProperties = TextSchema.BehavioralProperties;
		for (int j = 0; j < behavioralProperties.Length; j++)
		{
			behavioralProperties[j].OverrideMetadata(typeof(RichTextBox), new FrameworkPropertyMetadata(propertyChangedCallback2));
		}
	}

	private void TransferInheritedPropertiesToFlowDocument()
	{
		if (_implicitDocument)
		{
			DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(FlowDocument));
			foreach (DependencyProperty dependencyProperty in inheritableProperties)
			{
				TransferFormattingProperty(dependencyProperty, GetValue(dependencyProperty));
			}
		}
		DependencyProperty[] behavioralProperties = TextSchema.BehavioralProperties;
		foreach (DependencyProperty dependencyProperty2 in behavioralProperties)
		{
			TransferBehavioralProperty(dependencyProperty2, GetValue(dependencyProperty2));
		}
	}

	private static void OnFormattingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RichTextBox richTextBox = (RichTextBox)d;
		if (richTextBox._implicitDocument)
		{
			richTextBox.TransferFormattingProperty(e.Property, e.NewValue);
		}
	}

	private void TransferFormattingProperty(DependencyProperty property, object inheritedValue)
	{
		Invariant.Assert(_implicitDocument, "We only supposed to do this for implicit documents");
		object value = _document.GetValue(property);
		if (!TextSchema.ValuesAreEqual(inheritedValue, value))
		{
			_document.ClearValue(property);
			value = _document.GetValue(property);
			if (!TextSchema.ValuesAreEqual(inheritedValue, value))
			{
				_document.SetValue(property, inheritedValue);
			}
		}
	}

	private static void OnBehavioralPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RichTextBox)d).TransferBehavioralProperty(e.Property, e.NewValue);
	}

	private void TransferBehavioralProperty(DependencyProperty property, object inheritedValue)
	{
		_document.SetValue(property, inheritedValue);
	}

	private void OnPageSizeChangedHandler(object sender, EventArgs e)
	{
		if (base.RenderScope != null)
		{
			if (Document != null)
			{
				Document.TextWrapping = TextWrapping.Wrap;
			}
			base.RenderScope.ClearValue(FrameworkElement.WidthProperty);
			base.RenderScope.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
			if (base.RenderScope.HorizontalAlignment != HorizontalAlignment.Stretch)
			{
				base.RenderScope.HorizontalAlignment = HorizontalAlignment.Stretch;
			}
		}
	}

	private static void OnIsDocumentEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RichTextBox richTextBox = (RichTextBox)d;
		if (richTextBox.Document != null)
		{
			richTextBox.Document.CoerceValue(UIElement.IsEnabledProperty);
		}
	}
}
