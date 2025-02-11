using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class FixedSOMTableCell : FixedSOMContainer
{
	private bool _containsTable;

	private int _columnSpan;

	internal override FixedElement.ElementType[] ElementTypes => new FixedElement.ElementType[1] { FixedElement.ElementType.TableCell };

	internal bool IsEmpty
	{
		get
		{
			foreach (FixedSOMContainer semanticBox in base.SemanticBoxes)
			{
				if (semanticBox is FixedSOMTable { IsEmpty: false })
				{
					return false;
				}
				if (semanticBox is FixedSOMFixedBlock { IsWhiteSpace: false })
				{
					return false;
				}
			}
			return true;
		}
	}

	internal int ColumnSpan
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

	public FixedSOMTableCell(double left, double top, double right, double bottom)
	{
		_boundingRect = new Rect(new Point(left, top), new Point(right, bottom));
		_containsTable = false;
		_columnSpan = 1;
	}

	public void AddContainer(FixedSOMContainer container)
	{
		if (!_containsTable || !_AddToInnerTable(container))
		{
			Add(container);
		}
		if (container is FixedSOMTable)
		{
			_containsTable = true;
		}
	}

	public override void SetRTFProperties(FixedElement element)
	{
		element.SetValue(Block.BorderThicknessProperty, new Thickness(1.0));
		element.SetValue(Block.BorderBrushProperty, Brushes.Black);
		element.SetValue(TableCell.ColumnSpanProperty, _columnSpan);
	}

	private bool _AddToInnerTable(FixedSOMContainer container)
	{
		foreach (FixedSOMSemanticBox semanticBox in _semanticBoxes)
		{
			if (semanticBox is FixedSOMTable fixedSOMTable && fixedSOMTable.AddContainer(container))
			{
				return true;
			}
		}
		return false;
	}
}
