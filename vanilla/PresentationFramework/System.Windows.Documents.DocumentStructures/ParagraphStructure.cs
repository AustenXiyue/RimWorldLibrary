using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a paragraph in a document. </summary>
public class ParagraphStructure : SemanticBasicElement, IAddChild, IEnumerable<NamedElement>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.ParagraphStructure" /> class. </summary>
	public ParagraphStructure()
	{
		_elementType = FixedElement.ElementType.Paragraph;
	}

	/// <summary>Adds a named element to the paragraph.</summary>
	/// <param name="element">The element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> is null.</exception>
	public void Add(NamedElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		((IAddChild)this).AddChild((object)element);
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is NamedElement)
		{
			_elementList.Add((BlockElement)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.DocumentStructureUnexpectedParameterType1, value.GetType(), typeof(NamedElement)), "value");
	}

	/// <summary>Not implemented.</summary>
	/// <param name="text">Not used.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<NamedElement> IEnumerable<NamedElement>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<NamedElement>)this).GetEnumerator();
	}
}
