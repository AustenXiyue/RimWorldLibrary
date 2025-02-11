namespace System.Windows.Documents;

internal sealed class FixedSOMTableRow : FixedSOMContainer
{
	internal override FixedElement.ElementType[] ElementTypes => new FixedElement.ElementType[1] { FixedElement.ElementType.TableRow };

	internal bool IsEmpty
	{
		get
		{
			foreach (FixedSOMTableCell semanticBox in base.SemanticBoxes)
			{
				if (!semanticBox.IsEmpty)
				{
					return false;
				}
			}
			return true;
		}
	}

	public void AddCell(FixedSOMTableCell cell)
	{
		Add(cell);
	}
}
