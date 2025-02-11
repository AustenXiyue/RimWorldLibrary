using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a section of content in a document.</summary>
public class SectionStructure : SemanticBasicElement, IAddChild, IEnumerable<BlockElement>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.SectionStructure" /> class. </summary>
	public SectionStructure()
	{
		_elementType = FixedElement.ElementType.Section;
	}

	/// <summary>Adds a block to the section.</summary>
	/// <param name="element">The block element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The element is null.</exception>
	public void Add(BlockElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		((IAddChild)this).AddChild((object)element);
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value is ParagraphStructure || value is FigureStructure || value is ListStructure || value is TableStructure)
		{
			_elementList.Add((BlockElement)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.DocumentStructureUnexpectedParameterType4, value.GetType(), typeof(ParagraphStructure), typeof(FigureStructure), typeof(ListStructure), typeof(TableStructure)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<BlockElement> IEnumerable<BlockElement>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>System.Collections.IEnumerator</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<BlockElement>)this).GetEnumerator();
	}
}
