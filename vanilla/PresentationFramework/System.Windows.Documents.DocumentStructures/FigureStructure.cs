using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a drawing, chart, or diagram in a document. </summary>
public class FigureStructure : SemanticBasicElement, IAddChild, IEnumerable<NamedElement>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.FigureStructure" /> class. </summary>
	public FigureStructure()
	{
		_elementType = FixedElement.ElementType.Figure;
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> to add. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks. </exception>
	void IAddChild.AddChild(object value)
	{
		if (value is NamedElement)
		{
			_elementList.Add((BlockElement)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(NamedElement)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.  </param>
	void IAddChild.AddText(string text)
	{
	}

	/// <summary>Add a named element to the figure.</summary>
	/// <param name="element">The element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The element is null.</exception>
	public void Add(NamedElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		((IAddChild)this).AddChild((object)element);
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
