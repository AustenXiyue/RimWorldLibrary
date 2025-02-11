using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents an item in a list or outline. </summary>
public class ListItemStructure : SemanticBasicElement, IAddChild, IEnumerable<BlockElement>, IEnumerable
{
	private string _markerName;

	/// <summary>Gets or sets the name of the numeral, character, or bullet symbol for the list item as it appears in the formatting markup of the document.</summary>
	/// <returns>A <see cref="T:System.String" /> marking list item.</returns>
	public string Marker
	{
		get
		{
			return _markerName;
		}
		set
		{
			_markerName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.ListItemStructure" /> class. </summary>
	public ListItemStructure()
	{
		_elementType = FixedElement.ElementType.ListItem;
	}

	/// <summary>Adds a block to a list item.</summary>
	/// <param name="element">The block to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> is null.</exception>
	public void Add(BlockElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		((IAddChild)this).AddChild((object)element);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is ParagraphStructure || value is TableStructure || value is ListStructure || value is FigureStructure)
		{
			_elementList.Add((BlockElement)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.DocumentStructureUnexpectedParameterType4, value.GetType(), typeof(ParagraphStructure), typeof(TableStructure), typeof(ListStructure), typeof(FigureStructure)), "value");
	}

	/// <summary>Not implemented.</summary>
	/// <param name="text">Not used.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<BlockElement> IEnumerable<BlockElement>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<BlockElement>)this).GetEnumerator();
	}
}
