using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a set of one or more rows in a table.</summary>
public class TableRowGroupStructure : SemanticBasicElement, IAddChild, IEnumerable<TableRowStructure>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.TableRowGroupStructure" /> class. </summary>
	public TableRowGroupStructure()
	{
		_elementType = FixedElement.ElementType.TableRowGroup;
	}

	/// <summary>Adds a row to the table row group.</summary>
	/// <param name="tableRow">The row to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The row is null.</exception>
	public void Add(TableRowStructure tableRow)
	{
		if (tableRow == null)
		{
			throw new ArgumentNullException("tableRow");
		}
		((IAddChild)this).AddChild((object)tableRow);
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is TableRowStructure)
		{
			_elementList.Add((TableRowStructure)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(TableRowStructure)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<TableRowStructure> IEnumerable<TableRowStructure>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<TableRowStructure>)this).GetEnumerator();
	}
}
