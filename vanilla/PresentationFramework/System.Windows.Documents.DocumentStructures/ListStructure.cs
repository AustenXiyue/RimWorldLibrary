using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a list of items in a document.</summary>
public class ListStructure : SemanticBasicElement, IAddChild, IEnumerable<ListItemStructure>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.ListStructure" /> class. </summary>
	public ListStructure()
	{
		_elementType = FixedElement.ElementType.List;
	}

	/// <summary>Adds a list item to the list.</summary>
	/// <param name="listItem">The list item to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="listItem" /> is null.</exception>
	public void Add(ListItemStructure listItem)
	{
		if (listItem == null)
		{
			throw new ArgumentNullException("listItem");
		}
		((IAddChild)this).AddChild((object)listItem);
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is ListItemStructure)
		{
			_elementList.Add((ListItemStructure)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(ListItemStructure)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.  </param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<ListItemStructure> IEnumerable<ListItemStructure>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<ListItemStructure>)this).GetEnumerator();
	}
}
