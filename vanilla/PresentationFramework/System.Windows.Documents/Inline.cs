using MS.Internal;

namespace System.Windows.Documents;

/// <summary>An abstract class that provides a base for all inline flow content elements.</summary>
[TextElementEditingBehavior(IsMergeable = true, IsTypographicOnly = true)]
public abstract class Inline : TextElement
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Inline.BaselineAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Inline.BaselineAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty BaselineAlignmentProperty = DependencyProperty.Register("BaselineAlignment", typeof(BaselineAlignment), typeof(Inline), new FrameworkPropertyMetadata(BaselineAlignment.Baseline, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidBaselineAlignment);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Inline.TextDecorations" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Inline.TextDecorations" /> dependency property.</returns>
	public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(Inline), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextDecorationCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Inline.FlowDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Inline.FlowDirection" /> dependency property.</returns>
	public static readonly DependencyProperty FlowDirectionProperty = FrameworkElement.FlowDirectionProperty.AddOwner(typeof(Inline));

	/// <summary>Gets an <see cref="T:System.Windows.Documents.InlineCollection" /> that contains the <see cref="T:System.Windows.Documents.Inline" /> elements that are siblings (peers) to this element.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.InlineCollection" /> object that contains the <see cref="T:System.Windows.Documents.Inline" /> elements that are siblings to this element.This property has no default value.</returns>
	public InlineCollection SiblingInlines
	{
		get
		{
			if (base.Parent == null)
			{
				return null;
			}
			return new InlineCollection(this, isOwnerParent: false);
		}
	}

	/// <summary>Gets the next <see cref="T:System.Windows.Documents.Inline" /> element that is a peer to this element.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.Inline" /> object representing the next <see cref="T:System.Windows.Documents.Inline" /> element that is a peer to this element, or null if there is no next <see cref="T:System.Windows.Documents.Inline" /> element.This property has no default value.</returns>
	public Inline NextInline => base.NextElement as Inline;

	/// <summary>Gets the previous <see cref="T:System.Windows.Documents.Inline" /> element that is a peer to this element.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.Inline" /> object representing the previous <see cref="T:System.Windows.Documents.Inline" /> element that is a peer to this element, or null if there is no previous <see cref="T:System.Windows.Documents.Inline" /> element.This property has no default value.</returns>
	public Inline PreviousInline => base.PreviousElement as Inline;

	/// <summary>Gets or sets the baseline alignment for the <see cref="T:System.Windows.Documents.Inline" /> element.   </summary>
	/// <returns>A member or the <see cref="T:System.Windows.BaselineAlignment" /> enumeration specifying the baseline alignment for the <see cref="T:System.Windows.Documents.Inline" /> element.The default value is <see cref="T:System.Windows.BaselineAlignment" />.Baseline.</returns>
	public BaselineAlignment BaselineAlignment
	{
		get
		{
			return (BaselineAlignment)GetValue(BaselineAlignmentProperty);
		}
		set
		{
			SetValue(BaselineAlignmentProperty, value);
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.TextDecorationCollection" /> that contains text decorations to apply to this element.  </summary>
	/// <returns>A <see cref="T:System.Windows.TextDecorationCollection" /> collection that contains text decorations to apply to this element.The default value is null (no text decorations applied).</returns>
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

	/// <summary>Gets or sets a value that specifies the relative direction for flow of content within a <see cref="T:System.Windows.Documents.Inline" /> element.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FlowDirection" /> enumeration specifying the relative flow direction.  Getting this property returns the currently effective flow direction.  Setting this property causes the contents of the <see cref="T:System.Windows.Documents.Inline" /> element to re-flow in the indicated direction.The default value is <see cref="F:System.Windows.FlowDirection.LeftToRight" />.</returns>
	public FlowDirection FlowDirection
	{
		get
		{
			return (FlowDirection)GetValue(FlowDirectionProperty);
		}
		set
		{
			SetValue(FlowDirectionProperty, value);
		}
	}

	/// <summary>Initializes base class values when called by a derived class.</summary>
	protected Inline()
	{
	}

	internal static Run CreateImplicitRun(DependencyObject parent)
	{
		return new Run();
	}

	internal static InlineUIContainer CreateImplicitInlineUIContainer(DependencyObject parent)
	{
		return new InlineUIContainer();
	}

	private static bool IsValidBaselineAlignment(object o)
	{
		BaselineAlignment baselineAlignment = (BaselineAlignment)o;
		if (baselineAlignment != BaselineAlignment.Baseline && baselineAlignment != BaselineAlignment.Bottom && baselineAlignment != BaselineAlignment.Center && baselineAlignment != BaselineAlignment.Subscript && baselineAlignment != BaselineAlignment.Superscript && baselineAlignment != BaselineAlignment.TextBottom && baselineAlignment != BaselineAlignment.TextTop)
		{
			return baselineAlignment == BaselineAlignment.Top;
		}
		return true;
	}
}
