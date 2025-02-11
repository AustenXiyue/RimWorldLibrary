using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>Represents a cell in a table.</summary>
public class TableCellStructure : SemanticBasicElement, IAddChild, IEnumerable<BlockElement>, IEnumerable
{
	private int _rowSpan;

	private int _columnSpan;

	/// <summary>Gets or sets the number of rows spanned by the cell.</summary>
	/// <returns>The number of rows that the cell spans. The default is 1.</returns>
	public int RowSpan
	{
		get
		{
			return _rowSpan;
		}
		set
		{
			_rowSpan = value;
		}
	}

	/// <summary>Gets or sets the number of columns spanned by the cell.</summary>
	/// <returns>The number of columns that the cell spans. The default is 1.</returns>
	public int ColumnSpan
	{
		get
		{
			return _columnSpan;
		}
		set
		{
			_columnSpan = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.TableCellStructure" /> class. </summary>
	public TableCellStructure()
	{
		_elementType = FixedElement.ElementType.TableCell;
		_rowSpan = 1;
		_columnSpan = 1;
	}

	/// <summary>Adds a block element to the table cell.</summary>
	/// <param name="element">The element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The element is null.</exception>
	public void Add(BlockElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		((IAddChild)this).AddChild((object)element);
	}

	/// <summary>Adds a child object to the <see cref="T:System.Windows.Documents.DocumentStructures.TableCellStructure" />. </summary>
	/// <param name="value">The child object to add.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not one of the types that can be a child of this class. See Remarks. </exception>
	void IAddChild.AddChild(object value)
	{
		if (value is ParagraphStructure || value is TableStructure || value is ListStructure || value is FigureStructure)
		{
			_elementList.Add((BlockElement)value);
			return;
		}
		throw new ArgumentException(SR.Format(SR.DocumentStructureUnexpectedParameterType4, value.GetType(), typeof(ParagraphStructure), typeof(TableStructure), typeof(ListStructure), typeof(FigureStructure)), "value");
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

	/// <summary>This API is not implemented.</summary>
	/// <returns>This API is not implemented.</returns>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<BlockElement>)this).GetEnumerator();
	}
}
