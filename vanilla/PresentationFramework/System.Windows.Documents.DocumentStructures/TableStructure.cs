using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a table in a document.</summary>
public class TableStructure : SemanticBasicElement, IAddChild, IEnumerable<TableRowGroupStructure>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.TableStructure" /> class. </summary>
	public TableStructure()
	{
		_elementType = FixedElement.ElementType.Table;
	}

	/// <summary>Adds a group of rows to a table.</summary>
	/// <param name="tableRowGroup">The group of rows to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The group of rows is null.</exception>
	public void Add(TableRowGroupStructure tableRowGroup)
	{
		if (tableRowGroup == null)
		{
			throw new ArgumentNullException("tableRowGroup");
		}
		((IAddChild)this).AddChild((object)tableRowGroup);
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is TableRowGroupStructure)
		{
			_elementList.Add((TableRowGroupStructure)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(TableRowGroupStructure)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<TableRowGroupStructure> IEnumerable<TableRowGroupStructure>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<TableRowGroupStructure>)this).GetEnumerator();
	}
}
