using System.Collections;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationFramework;
using MS.Internal.Text;

namespace System.Windows.Documents;

/// <summary>An abstract class used as the base class for the abstract <see cref="T:System.Windows.Documents.Block" /> and <see cref="T:System.Windows.Documents.Inline" /> classes.</summary>
public abstract class TextElement : FrameworkContentElement, IAddChild
{
	internal static readonly UncommonField<TextElement> ContainerTextElementField;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontWeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontStretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ForegroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.Background" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.Background" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TextElement.TextEffects" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TextElement.TextEffects" /> dependency property.</returns>
	public static readonly DependencyProperty TextEffectsProperty;

	private TextTreeTextElementNode _textElementNode;

	private TypographyProperties _typographyPropertiesGroup = Typography.Default;

	internal TextRange TextRange
	{
		get
		{
			VerifyAccess();
			TextContainer tree = EnsureTextContainer();
			TextPointer textPointer = new TextPointer(tree, _textElementNode, ElementEdge.AfterStart, LogicalDirection.Backward);
			textPointer.Freeze();
			TextPointer textPointer2 = new TextPointer(tree, _textElementNode, ElementEdge.BeforeEnd, LogicalDirection.Forward);
			textPointer2.Freeze();
			return new TextRange(textPointer, textPointer2);
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> that represents the position just before the start of the element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointerContext" /> that represents the position just before the start of the <see cref="T:System.Windows.Documents.TextElement" />.</returns>
	public TextPointer ElementStart
	{
		get
		{
			TextPointer textPointer = new TextPointer(EnsureTextContainer(), _textElementNode, ElementEdge.BeforeStart, LogicalDirection.Forward);
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal StaticTextPointer StaticElementStart => new StaticTextPointer(EnsureTextContainer(), _textElementNode, 0);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> that represents the start of content in the element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointerContext" /> that represents the start of the content in the <see cref="T:System.Windows.Documents.TextElement" />.</returns>
	public TextPointer ContentStart
	{
		get
		{
			TextPointer textPointer = new TextPointer(EnsureTextContainer(), _textElementNode, ElementEdge.AfterStart, LogicalDirection.Backward);
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal StaticTextPointer StaticContentStart => new StaticTextPointer(EnsureTextContainer(), _textElementNode, 1);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> that represents the end of the content in the element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> that represents the end of the content in the <see cref="T:System.Windows.Documents.TextElement" />.</returns>
	public TextPointer ContentEnd
	{
		get
		{
			TextPointer textPointer = new TextPointer(EnsureTextContainer(), _textElementNode, ElementEdge.BeforeEnd, LogicalDirection.Forward);
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal StaticTextPointer StaticContentEnd => new StaticTextPointer(EnsureTextContainer(), _textElementNode, _textElementNode.SymbolCount - 1);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> that represents the position just after the end of the element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> that represents the position just after the end of the <see cref="T:System.Windows.Documents.TextElement" />.</returns>
	public TextPointer ElementEnd
	{
		get
		{
			TextPointer textPointer = new TextPointer(EnsureTextContainer(), _textElementNode, ElementEdge.AfterEnd, LogicalDirection.Backward);
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal StaticTextPointer StaticElementEnd => new StaticTextPointer(EnsureTextContainer(), _textElementNode, _textElementNode.SymbolCount);

	/// <summary>Gets or sets the preferred top-level font family for the content of the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.FontFamily" /> object that specifies the preferred font family, or a primary preferred font family with one or more fallback font families. The default is the font determined by the <see cref="P:System.Windows.SystemFonts.MessageFontFamily" /> value.</returns>
	[Localizability(LocalizationCategory.Font, Modifiability = Modifiability.Unmodifiable)]
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

	/// <summary>Gets or sets the font style for the content of the element.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontStyles" /> class that specifies the desired font style. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontStyle" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font weight for the content of the element.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontWeights" /> class that specifies the desired font weight. The default value is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontWeight" /> value.</returns>
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

	/// <summary>Gets or sets the font-stretching characteristics for the content of the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.FontStretch" /> structure that specifies the desired font-stretching characteristics to use. The default is <see cref="P:System.Windows.FontStretches.Normal" />.</returns>
	public FontStretch FontStretch
	{
		get
		{
			return (FontStretch)GetValue(FontStretchProperty);
		}
		set
		{
			SetValue(FontStretchProperty, value);
		}
	}

	/// <summary>Gets or sets the font size for the content of the element.  </summary>
	/// <returns>The desired font size to use in device independent pixels,  greater than 0.001 and less than or equal to 35791.  The default depends on current system settings and depends on the <see cref="P:System.Windows.SystemFonts.MessageFontSize" /> value.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Documents.TextElement.FontSize" /> is set to a value greater than 35791 or less than or equal to 0.001.</exception>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> to apply to the content of the element.  </summary>
	/// <returns>The brush used to apply to the text contents. The default is <see cref="P:System.Windows.Media.Brushes.Black" />.</returns>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used to fill the background of the content area.  </summary>
	/// <returns>The brush used to fill the background of the content area, or null to not use a background brush. The default is null.</returns>
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	/// <summary>Gets or sets a collection of text effects to apply to the content of the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextEffectCollection" /> containing one or more <see cref="T:System.Windows.Media.TextEffect" /> objects that define effects to apply to the content in this element. The default is null (not an empty collection).</returns>
	public TextEffectCollection TextEffects
	{
		get
		{
			return (TextEffectCollection)GetValue(TextEffectsProperty);
		}
		set
		{
			SetValue(TextEffectsProperty, value);
		}
	}

	/// <summary>Gets the currently effective typography variations for the content of the element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.Typography" /> object that specifies the currently effective typography variations. For a list of default typography values, see <see cref="T:System.Windows.Documents.Typography" />.</returns>
	public Typography Typography => new Typography(this);

	/// <summary>Gets an enumerator that can iterate the logical children of the element.</summary>
	/// <returns>An enumerator for the logical children.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (!IsEmpty)
			{
				return new RangeContentEnumerator(ContentStart, ContentEnd);
			}
			return new RangeContentEnumerator(null, null);
		}
	}

	internal TextContainer TextContainer => EnsureTextContainer();

	internal bool IsEmpty
	{
		get
		{
			if (_textElementNode == null)
			{
				return true;
			}
			return _textElementNode.ContainedNode == null;
		}
	}

	internal bool IsInTree => _textElementNode != null;

	internal int ElementStartOffset
	{
		get
		{
			Invariant.Assert(IsInTree, "TextElement is not in any TextContainer, caller should ensure this.");
			return _textElementNode.GetSymbolOffset(EnsureTextContainer().Generation) - 1;
		}
	}

	internal int ContentStartOffset
	{
		get
		{
			Invariant.Assert(IsInTree, "TextElement is not in any TextContainer, caller should ensure this.");
			return _textElementNode.GetSymbolOffset(EnsureTextContainer().Generation);
		}
	}

	internal int ContentEndOffset
	{
		get
		{
			Invariant.Assert(IsInTree, "TextElement is not in any TextContainer, caller should ensure this.");
			return _textElementNode.GetSymbolOffset(EnsureTextContainer().Generation) + _textElementNode.SymbolCount - 2;
		}
	}

	internal int ElementEndOffset
	{
		get
		{
			Invariant.Assert(IsInTree, "TextElement is not in any TextContainer, caller should ensure this.");
			return _textElementNode.GetSymbolOffset(EnsureTextContainer().Generation) + _textElementNode.SymbolCount - 1;
		}
	}

	internal int SymbolCount
	{
		get
		{
			if (!IsInTree)
			{
				return 2;
			}
			return _textElementNode.SymbolCount;
		}
	}

	internal TextTreeTextElementNode TextElementNode
	{
		get
		{
			return _textElementNode;
		}
		set
		{
			_textElementNode = value;
		}
	}

	internal TypographyProperties TypographyPropertiesGroup
	{
		get
		{
			if (_typographyPropertiesGroup == null)
			{
				_typographyPropertiesGroup = GetTypographyProperties(this);
			}
			return _typographyPropertiesGroup;
		}
	}

	internal virtual bool IsIMEStructuralElement => false;

	internal int IMELeftEdgeCharCount
	{
		get
		{
			int result = 0;
			if (IsIMEStructuralElement)
			{
				result = (IsInTree ? ((!TextElementNode.IsFirstSibling) ? 1 : 0) : (-1));
			}
			return result;
		}
	}

	internal virtual bool IsFirstIMEVisibleSibling
	{
		get
		{
			bool result = false;
			if (IsIMEStructuralElement)
			{
				result = TextElementNode == null || TextElementNode.IsFirstSibling;
			}
			return result;
		}
	}

	internal TextElement NextElement
	{
		get
		{
			if (!IsInTree)
			{
				return null;
			}
			if (!(_textElementNode.GetNextNode() is TextTreeTextElementNode textTreeTextElementNode))
			{
				return null;
			}
			return textTreeTextElementNode.TextElement;
		}
	}

	internal TextElement PreviousElement
	{
		get
		{
			if (!IsInTree)
			{
				return null;
			}
			if (!(_textElementNode.GetPreviousNode() is TextTreeTextElementNode textTreeTextElementNode))
			{
				return null;
			}
			return textTreeTextElementNode.TextElement;
		}
	}

	internal TextElement FirstChildElement
	{
		get
		{
			if (!IsInTree)
			{
				return null;
			}
			if (!(_textElementNode.GetFirstContainedNode() is TextTreeTextElementNode textTreeTextElementNode))
			{
				return null;
			}
			return textTreeTextElementNode.TextElement;
		}
	}

	internal TextElement LastChildElement
	{
		get
		{
			if (!IsInTree)
			{
				return null;
			}
			if (!(_textElementNode.GetLastContainedNode() is TextTreeTextElementNode textTreeTextElementNode))
			{
				return null;
			}
			return textTreeTextElementNode.TextElement;
		}
	}

	static TextElement()
	{
		ContainerTextElementField = new UncommonField<TextElement>();
		FontFamilyProperty = DependencyProperty.RegisterAttached("FontFamily", typeof(FontFamily), typeof(TextElement), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), IsValidFontFamily);
		FontStyleProperty = DependencyProperty.RegisterAttached("FontStyle", typeof(FontStyle), typeof(TextElement), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		FontWeightProperty = DependencyProperty.RegisterAttached("FontWeight", typeof(FontWeight), typeof(TextElement), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		FontStretchProperty = DependencyProperty.RegisterAttached("FontStretch", typeof(FontStretch), typeof(TextElement), new FrameworkPropertyMetadata(FontStretches.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		FontSizeProperty = DependencyProperty.RegisterAttached("FontSize", typeof(double), typeof(TextElement), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), IsValidFontSize);
		ForegroundProperty = DependencyProperty.RegisterAttached("Foreground", typeof(Brush), typeof(TextElement), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(TextElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		TextEffectsProperty = DependencyProperty.Register("TextEffects", typeof(TextEffectCollection), typeof(TextElement), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextEffectCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		PropertyChangedCallback propertyChangedCallback = OnTypographyChanged;
		DependencyProperty[] typographyPropertiesList = Typography.TypographyPropertiesList;
		for (int i = 0; i < typographyPropertiesList.Length; i++)
		{
			typographyPropertiesList[i].OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(propertyChangedCallback));
		}
	}

	internal TextElement()
	{
	}

	internal void Reposition(TextPointer start, TextPointer end)
	{
		if (start != null)
		{
			ValidationHelper.VerifyPositionPair(start, end);
		}
		else if (end != null)
		{
			throw new ArgumentException(SR.TextElement_UnmatchedEndPointer);
		}
		if (start != null)
		{
			SplayTreeNode splayTreeNode = start.GetScopingNode();
			SplayTreeNode splayTreeNode2 = end.GetScopingNode();
			if (splayTreeNode == _textElementNode)
			{
				splayTreeNode = _textElementNode.GetContainingNode();
			}
			if (splayTreeNode2 == _textElementNode)
			{
				splayTreeNode2 = _textElementNode.GetContainingNode();
			}
			if (splayTreeNode != splayTreeNode2)
			{
				throw new ArgumentException(SR.Format(SR.InDifferentScope, "start", "end"));
			}
		}
		if (IsInTree)
		{
			TextContainer textContainer = EnsureTextContainer();
			if (start == null)
			{
				textContainer.BeginChange();
				try
				{
					textContainer.ExtractElementInternal(this);
					return;
				}
				finally
				{
					textContainer.EndChange();
				}
			}
			if (textContainer == start.TextContainer)
			{
				textContainer.BeginChange();
				try
				{
					textContainer.ExtractElementInternal(this);
					textContainer.InsertElementInternal(start, end, this);
					return;
				}
				finally
				{
					textContainer.EndChange();
				}
			}
			textContainer.BeginChange();
			try
			{
				textContainer.ExtractElementInternal(this);
			}
			finally
			{
				textContainer.EndChange();
			}
			start.TextContainer.BeginChange();
			try
			{
				start.TextContainer.InsertElementInternal(start, end, this);
				return;
			}
			finally
			{
				start.TextContainer.EndChange();
			}
		}
		if (start != null)
		{
			start.TextContainer.BeginChange();
			try
			{
				start.TextContainer.InsertElementInternal(start, end, this);
			}
			finally
			{
				start.TextContainer.EndChange();
			}
		}
	}

	internal void RepositionWithContent(TextPointer textPosition)
	{
		TextContainer textContainer;
		if (textPosition == null)
		{
			if (IsInTree)
			{
				textContainer = EnsureTextContainer();
				textContainer.BeginChange();
				try
				{
					textContainer.DeleteContentInternal(ElementStart, ElementEnd);
					return;
				}
				finally
				{
					textContainer.EndChange();
				}
			}
			return;
		}
		textContainer = textPosition.TextContainer;
		textContainer.BeginChange();
		try
		{
			textContainer.InsertElementInternal(textPosition, textPosition, this);
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	internal bool Contains(TextPointer position)
	{
		ValidationHelper.VerifyPosition(EnsureTextContainer(), position);
		if (ContentStart.CompareTo(position) <= 0)
		{
			return ContentEnd.CompareTo(position) >= 0;
		}
		return false;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static void SetFontFamily(DependencyObject element, FontFamily value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontFamilyProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static FontFamily GetFontFamily(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontFamily)element.GetValue(FontFamilyProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static void SetFontStyle(DependencyObject element, FontStyle value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontStyleProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.TextElement.FontStyle" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static FontStyle GetFontStyle(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontStyle)element.GetValue(FontStyleProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static void SetFontWeight(DependencyObject element, FontWeight value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontWeightProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.TextElement.FontWeight" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static FontWeight GetFontWeight(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontWeight)element.GetValue(FontWeightProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static void SetFontStretch(DependencyObject element, FontStretch value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontStretchProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.TextElement.FontStretch" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static FontStretch GetFontStretch(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontStretch)element.GetValue(FontStretchProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static void SetFontSize(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontSizeProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.TextElement.FontSize" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	[TypeConverter(typeof(FontSizeConverter))]
	public static double GetFontSize(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(FontSizeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static void SetForeground(DependencyObject element, Brush value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ForegroundProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.TextElement.Foreground" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public static Brush GetForeground(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Brush)element.GetValue(ForegroundProperty);
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		value.GetType();
		if (value is TextElement textElement)
		{
			TextSchema.ValidateChild(this, textElement, throwIfIllegalChild: true, throwIfIllegalHyperlinkDescendent: true);
			Append(textElement);
			return;
		}
		if (value is UIElement child)
		{
			if (this is InlineUIContainer inlineUIContainer)
			{
				if (inlineUIContainer.Child != null)
				{
					throw new ArgumentException(SR.Format(SR.TextSchema_ThisInlineUIContainerHasAChildUIElementAlready, GetType().Name, ((InlineUIContainer)this).Child.GetType().Name, value.GetType().Name));
				}
				inlineUIContainer.Child = child;
				return;
			}
			if (this is BlockUIContainer blockUIContainer)
			{
				if (blockUIContainer.Child != null)
				{
					throw new ArgumentException(SR.Format(SR.TextSchema_ThisBlockUIContainerHasAChildUIElementAlready, GetType().Name, ((BlockUIContainer)this).Child.GetType().Name, value.GetType().Name));
				}
				blockUIContainer.Child = child;
				return;
			}
			if (TextSchema.IsValidChild(this, typeof(InlineUIContainer)))
			{
				InlineUIContainer inlineUIContainer2 = Inline.CreateImplicitInlineUIContainer(this);
				Append(inlineUIContainer2);
				inlineUIContainer2.Child = child;
				return;
			}
			throw new ArgumentException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, GetType().Name, value.GetType().Name));
		}
		throw new ArgumentException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, GetType().Name, value.GetType().Name));
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (TextSchema.IsValidChild(this, typeof(string)))
		{
			Append(text);
		}
		else if (TextSchema.IsValidChild(this, typeof(Run)))
		{
			Run run = Inline.CreateImplicitRun(this);
			Append(run);
			run.Text = text;
		}
		else if (text.Trim().Length > 0)
		{
			throw new InvalidOperationException(SR.Format(SR.TextSchema_TextIsNotAllowed, GetType().Name));
		}
	}

	/// <summary>Handles notifications that one or more of the dependency properties that exist on the element have had their effective values changed. </summary>
	/// <param name="e">Arguments associated with the property value change.  The <see cref="P:System.Windows.DependencyPropertyChangedEventArgs.Property" /> property specifies which property has changed, the <see cref="P:System.Windows.DependencyPropertyChangedEventArgs.OldValue" /> property specifies the previous property value, and the <see cref="P:System.Windows.DependencyPropertyChangedEventArgs.NewValue" /> property specifies the new property value.</param>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		bool flag = e.NewValueSource == BaseValueSourceInternal.Local || e.OldValueSource == BaseValueSourceInternal.Local;
		if ((!flag && !e.IsAValueChange && !e.IsASubPropertyChange) || !IsInTree || !(e.Metadata is FrameworkPropertyMetadata frameworkPropertyMetadata))
		{
			return;
		}
		bool flag2 = frameworkPropertyMetadata.AffectsMeasure || frameworkPropertyMetadata.AffectsArrange || frameworkPropertyMetadata.AffectsParentMeasure || frameworkPropertyMetadata.AffectsParentArrange;
		bool flag3 = frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender);
		if (!(flag2 || flag3))
		{
			return;
		}
		TextContainer textContainer = EnsureTextContainer();
		textContainer.BeginChange();
		try
		{
			if (flag)
			{
				TextTreeUndo.CreatePropertyUndoUnit(this, e);
			}
			if (e.IsAValueChange || e.IsASubPropertyChange)
			{
				NotifyTypographicPropertyChanged(flag2, flag, e.Property);
			}
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	internal void NotifyTypographicPropertyChanged(bool affectsMeasureOrArrange, bool localValueChanged, DependencyProperty property)
	{
		if (!IsInTree)
		{
			return;
		}
		TextContainer textContainer = EnsureTextContainer();
		textContainer.NextLayoutGeneration();
		if (!textContainer.HasListeners)
		{
			return;
		}
		TextPointer textPointer = new TextPointer(textContainer, _textElementNode, ElementEdge.BeforeStart, LogicalDirection.Forward);
		textPointer.Freeze();
		textContainer.BeginChange();
		try
		{
			textContainer.BeforeAddChange();
			if (localValueChanged)
			{
				textContainer.AddLocalValueChange();
			}
			textContainer.AddChange(textPointer, _textElementNode.SymbolCount, _textElementNode.IMECharCount, PrecursorTextChangeType.PropertyModified, property, !affectsMeasureOrArrange);
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	internal static TypographyProperties GetTypographyProperties(DependencyObject element)
	{
		TypographyProperties typographyProperties = new TypographyProperties();
		typographyProperties.SetStandardLigatures((bool)element.GetValue(Typography.StandardLigaturesProperty));
		typographyProperties.SetContextualLigatures((bool)element.GetValue(Typography.ContextualLigaturesProperty));
		typographyProperties.SetDiscretionaryLigatures((bool)element.GetValue(Typography.DiscretionaryLigaturesProperty));
		typographyProperties.SetHistoricalLigatures((bool)element.GetValue(Typography.HistoricalLigaturesProperty));
		typographyProperties.SetAnnotationAlternates((int)element.GetValue(Typography.AnnotationAlternatesProperty));
		typographyProperties.SetContextualAlternates((bool)element.GetValue(Typography.ContextualAlternatesProperty));
		typographyProperties.SetHistoricalForms((bool)element.GetValue(Typography.HistoricalFormsProperty));
		typographyProperties.SetKerning((bool)element.GetValue(Typography.KerningProperty));
		typographyProperties.SetCapitalSpacing((bool)element.GetValue(Typography.CapitalSpacingProperty));
		typographyProperties.SetCaseSensitiveForms((bool)element.GetValue(Typography.CaseSensitiveFormsProperty));
		typographyProperties.SetStylisticSet1((bool)element.GetValue(Typography.StylisticSet1Property));
		typographyProperties.SetStylisticSet2((bool)element.GetValue(Typography.StylisticSet2Property));
		typographyProperties.SetStylisticSet3((bool)element.GetValue(Typography.StylisticSet3Property));
		typographyProperties.SetStylisticSet4((bool)element.GetValue(Typography.StylisticSet4Property));
		typographyProperties.SetStylisticSet5((bool)element.GetValue(Typography.StylisticSet5Property));
		typographyProperties.SetStylisticSet6((bool)element.GetValue(Typography.StylisticSet6Property));
		typographyProperties.SetStylisticSet7((bool)element.GetValue(Typography.StylisticSet7Property));
		typographyProperties.SetStylisticSet8((bool)element.GetValue(Typography.StylisticSet8Property));
		typographyProperties.SetStylisticSet9((bool)element.GetValue(Typography.StylisticSet9Property));
		typographyProperties.SetStylisticSet10((bool)element.GetValue(Typography.StylisticSet10Property));
		typographyProperties.SetStylisticSet11((bool)element.GetValue(Typography.StylisticSet11Property));
		typographyProperties.SetStylisticSet12((bool)element.GetValue(Typography.StylisticSet12Property));
		typographyProperties.SetStylisticSet13((bool)element.GetValue(Typography.StylisticSet13Property));
		typographyProperties.SetStylisticSet14((bool)element.GetValue(Typography.StylisticSet14Property));
		typographyProperties.SetStylisticSet15((bool)element.GetValue(Typography.StylisticSet15Property));
		typographyProperties.SetStylisticSet16((bool)element.GetValue(Typography.StylisticSet16Property));
		typographyProperties.SetStylisticSet17((bool)element.GetValue(Typography.StylisticSet17Property));
		typographyProperties.SetStylisticSet18((bool)element.GetValue(Typography.StylisticSet18Property));
		typographyProperties.SetStylisticSet19((bool)element.GetValue(Typography.StylisticSet19Property));
		typographyProperties.SetStylisticSet20((bool)element.GetValue(Typography.StylisticSet20Property));
		typographyProperties.SetFraction((FontFraction)element.GetValue(Typography.FractionProperty));
		typographyProperties.SetSlashedZero((bool)element.GetValue(Typography.SlashedZeroProperty));
		typographyProperties.SetMathematicalGreek((bool)element.GetValue(Typography.MathematicalGreekProperty));
		typographyProperties.SetEastAsianExpertForms((bool)element.GetValue(Typography.EastAsianExpertFormsProperty));
		typographyProperties.SetVariants((FontVariants)element.GetValue(Typography.VariantsProperty));
		typographyProperties.SetCapitals((FontCapitals)element.GetValue(Typography.CapitalsProperty));
		typographyProperties.SetNumeralStyle((FontNumeralStyle)element.GetValue(Typography.NumeralStyleProperty));
		typographyProperties.SetNumeralAlignment((FontNumeralAlignment)element.GetValue(Typography.NumeralAlignmentProperty));
		typographyProperties.SetEastAsianWidths((FontEastAsianWidths)element.GetValue(Typography.EastAsianWidthsProperty));
		typographyProperties.SetEastAsianLanguage((FontEastAsianLanguage)element.GetValue(Typography.EastAsianLanguageProperty));
		typographyProperties.SetStandardSwashes((int)element.GetValue(Typography.StandardSwashesProperty));
		typographyProperties.SetContextualSwashes((int)element.GetValue(Typography.ContextualSwashesProperty));
		typographyProperties.SetStylisticAlternates((int)element.GetValue(Typography.StylisticAlternatesProperty));
		return typographyProperties;
	}

	internal void DeepEndInit()
	{
		if (base.IsInitialized)
		{
			return;
		}
		if (!IsEmpty)
		{
			IEnumerator logicalChildren = LogicalChildren;
			while (logicalChildren.MoveNext())
			{
				if (logicalChildren.Current is TextElement textElement)
				{
					textElement.DeepEndInit();
				}
			}
		}
		EndInit();
		Invariant.Assert(base.IsInitialized);
	}

	internal static TextElement GetCommonAncestor(TextElement element1, TextElement element2)
	{
		if (element1 != element2)
		{
			int num = 0;
			int num2 = 0;
			TextElement textElement = element1;
			while (textElement.Parent is TextElement)
			{
				num++;
				textElement = (TextElement)textElement.Parent;
			}
			textElement = element2;
			while (textElement.Parent is TextElement)
			{
				num2++;
				textElement = (TextElement)textElement.Parent;
			}
			while (num > num2 && element1 != element2)
			{
				element1 = (TextElement)element1.Parent;
				num--;
			}
			while (num2 > num && element1 != element2)
			{
				element2 = (TextElement)element2.Parent;
				num2--;
			}
			while (element1 != element2)
			{
				element1 = element1.Parent as TextElement;
				element2 = element2.Parent as TextElement;
			}
		}
		Invariant.Assert(element1 == element2);
		return element1;
	}

	internal virtual void OnTextUpdated()
	{
	}

	internal virtual void BeforeLogicalTreeChange()
	{
	}

	internal virtual void AfterLogicalTreeChange()
	{
	}

	private static void OnTypographyChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
	{
		((TextElement)element)._typographyPropertiesGroup = null;
	}

	private void Append(string textData)
	{
		if (textData == null)
		{
			throw new ArgumentNullException("textData");
		}
		TextContainer textContainer = EnsureTextContainer();
		textContainer.BeginChange();
		try
		{
			textContainer.InsertTextInternal(new TextPointer(textContainer, _textElementNode, ElementEdge.BeforeEnd), textData);
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	private void Append(TextElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		TextContainer textContainer = EnsureTextContainer();
		textContainer.BeginChange();
		try
		{
			TextPointer textPointer = new TextPointer(textContainer, _textElementNode, ElementEdge.BeforeEnd);
			textContainer.InsertElementInternal(textPointer, textPointer, element);
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	private TextContainer EnsureTextContainer()
	{
		TextContainer textContainer;
		if (IsInTree)
		{
			textContainer = _textElementNode.GetTextTree();
			textContainer.EmptyDeadPositionList();
		}
		else
		{
			textContainer = new TextContainer(null, plainTextOnly: false);
			TextPointer start = textContainer.Start;
			textContainer.BeginChange();
			try
			{
				textContainer.InsertElementInternal(start, start, this);
			}
			finally
			{
				textContainer.EndChange();
			}
			Invariant.Assert(IsInTree);
		}
		return textContainer;
	}

	private static bool IsValidFontFamily(object o)
	{
		return o is FontFamily;
	}

	private static bool IsValidFontSize(object value)
	{
		double num = (double)value;
		double minWidth = TextDpi.MinWidth;
		double num2 = Math.Min(1000000, 160000);
		if (double.IsNaN(num))
		{
			return false;
		}
		if (num < minWidth)
		{
			return false;
		}
		if (num > num2)
		{
			return false;
		}
		return true;
	}
}
