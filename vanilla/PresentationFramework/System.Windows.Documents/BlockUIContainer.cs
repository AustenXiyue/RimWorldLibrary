using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>A block-level flow content element which enables <see cref="T:System.Windows.UIElement" /> elements (i.e. a <see cref="T:System.Windows.Controls.Button" />) to be embedded (hosted) in flow content.</summary>
[ContentProperty("Child")]
public class BlockUIContainer : Block
{
	/// <summary>Gets or sets the <see cref="T:System.Windows.UIElement" /> hosted by the <see cref="T:System.Windows.Documents.BlockUIContainer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> hosted by the <see cref="T:System.Windows.Documents.BlockUIContainer" />.</returns>
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

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.BlockUIContainer" /> class.</summary>
	public BlockUIContainer()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.BlockUIContainer" /> class, taking a specified <see cref="T:System.Windows.UIElement" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.BlockUIContainer" />.</summary>
	/// <param name="uiElement">An <see cref="T:System.Windows.UIElement" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.BlockUIContainer" />.</param>
	public BlockUIContainer(UIElement uiElement)
	{
		if (uiElement == null)
		{
			throw new ArgumentNullException("uiElement");
		}
		Child = uiElement;
	}
}
