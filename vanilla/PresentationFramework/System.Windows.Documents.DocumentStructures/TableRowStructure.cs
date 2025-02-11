using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a row of one or more cells in a table.</summary>
public class TableRowStructure : SemanticBasicElement, IAddChild, IEnumerable<TableCellStructure>, IEnumerable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.TableRowStructure" /> class. </summary>
	public TableRowStructure()
	{
		_elementType = FixedElement.ElementType.TableRow;
	}

	/// <summary>Adds a cell to a table row.</summary>
	/// <param name="tableCell">The cell to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The cell is null.</exception>
	public void Add(TableCellStructure tableCell)
	{
		if (tableCell == null)
		{
			throw new ArgumentNullException("tableCell");
		}
		((IAddChild)this).AddChild((object)tableCell);
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code. </summary>
	/// <param name="value">The child <see cref="T:System.Object" /> that is added.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks.</exception>
	void IAddChild.AddChild(object value)
	{
		if (value is TableCellStructure)
		{
			_elementList.Add((TableCellStructure)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(TableCellStructure)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
	}

	IEnumerator<TableCellStructure> IEnumerable<TableCellStructure>.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>This method has not been implemented.</summary>
	/// <returns>Always raises <see cref="T:System.NotSupportedException" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<TableCellStructure>)this).GetEnumerator();
	}
}
