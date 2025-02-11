using System.Windows.Markup;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>An inline-level flow content element which enables <see cref="T:System.Windows.UIElement" /> elements (i.e. a <see cref="T:System.Windows.Controls.Button" />) to be embedded (hosted) in flow content.</summary>
[ContentProperty("Child")]
[TextElementEditingBehavior(IsMergeable = false)]
public class InlineUIContainer : Inline
{
	private UIElementIsland _uiElementIsland;

	/// <summary>Gets or sets the <see cref="T:System.Windows.UIElement" /> hosted by the <see cref="T:System.Windows.Documents.InlineUIContainer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> hosted by the <see cref="T:System.Windows.Documents.InlineUIContainer" />.</returns>
	public UIElement Child
	{
		get
		{
			return base.ContentStart.GetAdjacentElement(LogicalDirection.Forward) as UIElement;
		}
		set
		{
			TextContainer textContainer = base.TextContainer;
			textContainer.BeginChange();
			try
			{
				TextPointer contentStart = base.ContentStart;
				UIElement child = Child;
				if (child != null)
				{
					textContainer.DeleteContentInternal(contentStart, base.ContentEnd);
					TextElement.ContainerTextElementField.ClearValue(child);
				}
				if (value != null)
				{
					TextElement.ContainerTextElementField.SetValue(value, this);
					contentStart.InsertUIElement(value);
				}
			}
			finally
			{
				textContainer.EndChange();
			}
		}
	}

	internal UIElementIsland UIElementIsland
	{
		get
		{
			UpdateUIElementIsland();
			return _uiElementIsland;
		}
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.InlineUIContainer" /> class.</summary>
	public InlineUIContainer()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.InlineUIContainer" /> class, taking a specified <see cref="T:System.Windows.UIElement" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.InlineUIContainer" />.</summary>
	/// <param name="childUIElement">An <see cref="T:System.Windows.UIElement" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.InlineUIContainer" />.</param>
	public InlineUIContainer(UIElement childUIElement)
		: this(childUIElement, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.InlineUIContainer" /> class, taking a specified <see cref="T:System.Windows.UIElement" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.InlineUIContainer" />, and a <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position for the new <see cref="T:System.Windows.Documents.InlineUIContainer" /> element.</summary>
	/// <param name="childUIElement">A <see cref="T:System.Windows.UIElement" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.InlineUIContainer" />.  This parameter may be null, in which case no <see cref="T:System.Windows.UIElement" /> is inserted.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the <see cref="T:System.Windows.Documents.InlineUIContainer" /> element after it is created, or null for no automatic insertion.</param>
	public InlineUIContainer(UIElement childUIElement, TextPointer insertionPosition)
	{
		insertionPosition?.TextContainer.BeginChange();
		try
		{
			insertionPosition?.InsertInline(this);
			Child = childUIElement;
		}
		finally
		{
			insertionPosition?.TextContainer.EndChange();
		}
	}

	private void UpdateUIElementIsland()
	{
		UIElement child = Child;
		if (_uiElementIsland == null || _uiElementIsland.Root != child)
		{
			if (_uiElementIsland != null)
			{
				_uiElementIsland.Dispose();
				_uiElementIsland = null;
			}
			if (child != null)
			{
				_uiElementIsland = new UIElementIsland(child);
			}
		}
	}
}
