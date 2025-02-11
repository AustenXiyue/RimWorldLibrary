using System.Collections;
using System.Windows.Controls;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>Represents a collection of <see cref="T:System.Windows.Documents.Inline" /> elements. <see cref="T:System.Windows.Documents.InlineCollection" /> defines the allowable child content of the <see cref="T:System.Windows.Documents.Paragraph" />, <see cref="T:System.Windows.Documents.Span" />, and <see cref="T:System.Windows.Controls.TextBlock" /> elements. </summary>
[ContentWrapper(typeof(Run))]
[ContentWrapper(typeof(InlineUIContainer))]
[WhitespaceSignificantCollection]
public class InlineCollection : TextElementCollection<Inline>, IList, ICollection, IEnumerable
{
	/// <summary>Gets the first <see cref="T:System.Windows.Documents.Inline" /> element within this instance of <see cref="T:System.Windows.Documents.InlineCollection" />.</summary>
	/// <returns>The first <see cref="T:System.Windows.Documents.Inline" /> element within this instance of <see cref="T:System.Windows.Documents.InlineCollection" />.</returns>
	public Inline FirstInline => base.FirstChild;

	/// <summary>Gets the last <see cref="T:System.Windows.Documents.Inline" /> element within this instance of <see cref="T:System.Windows.Documents.InlineCollection" />.</summary>
	/// <returns>The last <see cref="T:System.Windows.Documents.Inline" /> element within this instance of <see cref="T:System.Windows.Documents.InlineCollection" />.</returns>
	public Inline LastInline => base.LastChild;

	internal InlineCollection(DependencyObject owner, bool isOwnerParent)
		: base(owner, isOwnerParent)
	{
	}

	internal override int OnAdd(object value)
	{
		if (value is string text)
		{
			return AddText(text, returnIndex: true);
		}
		base.TextContainer.BeginChange();
		try
		{
			if (value is UIElement uiElement)
			{
				return AddUIElement(uiElement, returnIndex: true);
			}
			return base.OnAdd(value);
		}
		finally
		{
			base.TextContainer.EndChange();
		}
	}

	/// <summary>Adds an implicit <see cref="T:System.Windows.Documents.Run" /> element with the given text, supplied as a <see cref="T:System.String" />.</summary>
	/// <param name="text">Text set as the <see cref="P:System.Windows.Documents.Run.Text" /> property for the implicit <see cref="T:System.Windows.Documents.Run" />.</param>
	public void Add(string text)
	{
		AddText(text, returnIndex: false);
	}

	/// <summary>Adds an implicit <see cref="T:System.Windows.Documents.InlineUIContainer" /> with the supplied <see cref="T:System.Windows.UIElement" /> already in it. </summary>
	/// <param name="uiElement">
	///   <see cref="T:System.Windows.UIElement" /> set as the <see cref="P:System.Windows.Documents.InlineUIContainer.Child" /> property for the implicit <see cref="T:System.Windows.Documents.InlineUIContainer" />.</param>
	public void Add(UIElement uiElement)
	{
		AddUIElement(uiElement, returnIndex: false);
	}

	internal override void ValidateChild(Inline child)
	{
		base.ValidateChild(child);
		if (base.Parent is TextElement)
		{
			TextSchema.ValidateChild((TextElement)base.Parent, child, throwIfIllegalChild: true, throwIfIllegalHyperlinkDescendent: true);
		}
		else if (!TextSchema.IsValidChildOfContainer(base.Parent.GetType(), child.GetType()))
		{
			throw new InvalidOperationException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, base.Parent.GetType().Name, child.GetType().Name));
		}
	}

	private int AddText(string text, bool returnIndex)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (base.Parent is TextBlock)
		{
			TextBlock textBlock = (TextBlock)base.Parent;
			if (!textBlock.HasComplexContent)
			{
				textBlock.Text += text;
				return 0;
			}
		}
		base.TextContainer.BeginChange();
		try
		{
			Run run = Inline.CreateImplicitRun(base.Parent);
			int result;
			if (returnIndex)
			{
				result = base.OnAdd(run);
			}
			else
			{
				Add(run);
				result = -1;
			}
			run.Text = text;
			return result;
		}
		finally
		{
			base.TextContainer.EndChange();
		}
	}

	private int AddUIElement(UIElement uiElement, bool returnIndex)
	{
		if (uiElement == null)
		{
			throw new ArgumentNullException("uiElement");
		}
		InlineUIContainer inlineUIContainer = Inline.CreateImplicitInlineUIContainer(base.Parent);
		int result;
		if (returnIndex)
		{
			result = base.OnAdd(inlineUIContainer);
		}
		else
		{
			Add(inlineUIContainer);
			result = -1;
		}
		inlineUIContainer.Child = uiElement;
		return result;
	}
}
