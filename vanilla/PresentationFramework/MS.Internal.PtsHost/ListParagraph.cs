using System.Windows;
using System.Windows.Documents;

namespace MS.Internal.PtsHost;

internal sealed class ListParagraph : ContainerParagraph
{
	internal ListParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		ListParaClient listParaClient = new ListParaClient(this);
		paraClientHandle = listParaClient.Handle;
	}

	protected override BaseParagraph GetParagraph(ITextPointer textPointer, bool fEmptyOk)
	{
		Invariant.Assert(textPointer is TextPointer);
		BaseParagraph baseParagraph = null;
		while (baseParagraph == null)
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.ElementStart:
			{
				TextElement adjacentElementFromOuterPosition = ((TextPointer)textPointer).GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
				if (adjacentElementFromOuterPosition is ListItem)
				{
					baseParagraph = new ListItemParagraph(adjacentElementFromOuterPosition, base.StructuralCache);
					break;
				}
				if (adjacentElementFromOuterPosition is List)
				{
					baseParagraph = new ListParagraph(adjacentElementFromOuterPosition, base.StructuralCache);
					break;
				}
				if (((TextPointer)textPointer).IsFrozen)
				{
					textPointer = textPointer.CreatePointer();
				}
				textPointer.MoveToPosition(adjacentElementFromOuterPosition.ElementEnd);
				continue;
			}
			case TextPointerContext.ElementEnd:
				if (base.Element != ((TextPointer)textPointer).Parent)
				{
					if (((TextPointer)textPointer).IsFrozen)
					{
						textPointer = textPointer.CreatePointer();
					}
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					continue;
				}
				break;
			default:
				if (((TextPointer)textPointer).IsFrozen)
				{
					textPointer = textPointer.CreatePointer();
				}
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				continue;
			}
			break;
		}
		if (baseParagraph != null)
		{
			base.StructuralCache.CurrentFormatContext.DependentMax = (TextPointer)textPointer;
		}
		return baseParagraph;
	}
}
