using MS.Internal;

namespace System.Windows.Documents;

internal static class TextTreeText
{
	internal static void InsertText(TextTreeRootTextBlock rootTextBlock, int offset, object text)
	{
		Invariant.Assert(text is string || text is char[], "Bad text parameter!");
		int localOffset;
		TextTreeTextBlock textTreeTextBlock = FindBlock(rootTextBlock, offset, out localOffset);
		int textLength = TextContainer.GetTextLength(text);
		int num = textTreeTextBlock.InsertText(localOffset, text, 0, textLength);
		if (num < textLength)
		{
			if (textTreeTextBlock.GapOffset < 2048)
			{
				InsertTextLeft(textTreeTextBlock, text, num);
			}
			else
			{
				InsertTextRight(textTreeTextBlock, text, num);
			}
		}
	}

	internal static void RemoveText(TextTreeRootTextBlock rootTextBlock, int offset, int count)
	{
		if (count == 0)
		{
			return;
		}
		int localOffset;
		TextTreeTextBlock textTreeTextBlock = FindBlock(rootTextBlock, offset, out localOffset);
		if (textTreeTextBlock.Count == localOffset)
		{
			textTreeTextBlock = (TextTreeTextBlock)textTreeTextBlock.GetNextNode();
			Invariant.Assert(textTreeTextBlock != null);
			localOffset = 0;
		}
		int localOffset2;
		TextTreeTextBlock textTreeTextBlock2 = FindBlock(rootTextBlock, offset + count, out localOffset2);
		int num;
		SplayTreeNode splayTreeNode;
		if (localOffset > 0 || count < textTreeTextBlock.Count)
		{
			num = Math.Min(count, textTreeTextBlock.Count - localOffset);
			textTreeTextBlock.RemoveText(localOffset, num);
			splayTreeNode = textTreeTextBlock.GetNextNode();
		}
		else
		{
			num = 0;
			splayTreeNode = textTreeTextBlock;
		}
		if (count > num)
		{
			int num2;
			SplayTreeNode splayTreeNode2;
			if (localOffset2 < textTreeTextBlock2.Count)
			{
				num2 = localOffset2;
				textTreeTextBlock2.RemoveText(0, localOffset2);
				splayTreeNode2 = textTreeTextBlock2.GetPreviousNode();
			}
			else
			{
				num2 = 0;
				splayTreeNode2 = textTreeTextBlock2;
			}
			if (num + num2 < count)
			{
				Remove((TextTreeTextBlock)splayTreeNode, (TextTreeTextBlock)splayTreeNode2);
			}
		}
	}

	internal static char[] CutText(TextTreeRootTextBlock rootTextBlock, int offset, int count)
	{
		char[] array = new char[count];
		ReadText(rootTextBlock, offset, count, array, 0);
		RemoveText(rootTextBlock, offset, count);
		return array;
	}

	internal static void ReadText(TextTreeRootTextBlock rootTextBlock, int offset, int count, char[] chars, int startIndex)
	{
		if (count <= 0)
		{
			return;
		}
		int localOffset;
		TextTreeTextBlock textTreeTextBlock = FindBlock(rootTextBlock, offset, out localOffset);
		while (true)
		{
			Invariant.Assert(textTreeTextBlock != null, "Caller asked for too much text!");
			int num = textTreeTextBlock.ReadText(localOffset, count, chars, startIndex);
			localOffset = 0;
			count -= num;
			if (count != 0)
			{
				startIndex += num;
				textTreeTextBlock = (TextTreeTextBlock)textTreeTextBlock.GetNextNode();
				continue;
			}
			break;
		}
	}

	internal static void InsertObject(TextTreeRootTextBlock rootTextBlock, int offset)
	{
		InsertText(rootTextBlock, offset, new string('\uffff', 1));
	}

	internal static void InsertElementEdges(TextTreeRootTextBlock rootTextBlock, int offset, int childSymbolCount)
	{
		if (childSymbolCount == 0)
		{
			InsertText(rootTextBlock, offset, new string('뻯', 2));
			return;
		}
		InsertText(rootTextBlock, offset, new string('뻯', 1));
		InsertText(rootTextBlock, offset + childSymbolCount + 1, new string('\0', 1));
	}

	internal static void RemoveElementEdges(TextTreeRootTextBlock rootTextBlock, int offset, int symbolCount)
	{
		Invariant.Assert(symbolCount >= 2, "Element must span at least two symbols!");
		if (symbolCount == 2)
		{
			RemoveText(rootTextBlock, offset, 2);
			return;
		}
		RemoveText(rootTextBlock, offset + symbolCount - 1, 1);
		RemoveText(rootTextBlock, offset, 1);
	}

	private static TextTreeTextBlock FindBlock(TextTreeRootTextBlock rootTextBlock, int offset, out int localOffset)
	{
		int nodeOffset;
		TextTreeTextBlock textTreeTextBlock = (TextTreeTextBlock)rootTextBlock.ContainedNode.GetSiblingAtOffset(offset, out nodeOffset);
		if (textTreeTextBlock.LeftSymbolCount == offset)
		{
			TextTreeTextBlock textTreeTextBlock2 = (TextTreeTextBlock)textTreeTextBlock.GetPreviousNode();
			if (textTreeTextBlock2 != null)
			{
				textTreeTextBlock = textTreeTextBlock2;
				nodeOffset -= textTreeTextBlock.SymbolCount;
				Invariant.Assert(nodeOffset >= 0);
			}
		}
		localOffset = offset - nodeOffset;
		Invariant.Assert(localOffset >= 0 && localOffset <= textTreeTextBlock.Count);
		return textTreeTextBlock;
	}

	private static void InsertTextLeft(TextTreeTextBlock rightBlock, object text, int textOffset)
	{
		int num = -1;
		int textLength = TextContainer.GetTextLength(text);
		if (rightBlock.GapOffset == 0)
		{
			TextTreeTextBlock textTreeTextBlock = (TextTreeTextBlock)rightBlock.GetPreviousNode();
			if (textTreeTextBlock != null)
			{
				textOffset += textTreeTextBlock.InsertText(textTreeTextBlock.Count, text, textOffset, textLength);
			}
		}
		if (textOffset >= textLength)
		{
			return;
		}
		int num2 = 1;
		TextTreeTextBlock textTreeTextBlock2 = rightBlock.SplitBlock();
		textOffset += textTreeTextBlock2.InsertText(textTreeTextBlock2.Count, text, textOffset, textLength);
		if (textOffset < textLength)
		{
			int num3 = Math.Min(rightBlock.FreeCapacity, textLength - textOffset);
			num = textLength - num3;
			rightBlock.InsertText(0, text, num, textLength);
			if (textOffset < num)
			{
				num2 += (num - textOffset + 4096 - 1) / 4096;
			}
		}
		for (int i = 1; i < num2; i++)
		{
			TextTreeTextBlock textTreeTextBlock3 = new TextTreeTextBlock(4096);
			textOffset += textTreeTextBlock3.InsertText(0, text, textOffset, num);
			textTreeTextBlock3.InsertAtNode(textTreeTextBlock2, insertBefore: false);
			textTreeTextBlock2 = textTreeTextBlock3;
		}
		Invariant.Assert(num2 == 1 || textOffset == num, "Not all text copied!");
	}

	private static void InsertTextRight(TextTreeTextBlock leftBlock, object text, int textOffset)
	{
		int num = TextContainer.GetTextLength(text);
		int num2;
		if (leftBlock.GapOffset == leftBlock.Count)
		{
			TextTreeTextBlock textTreeTextBlock = (TextTreeTextBlock)leftBlock.GetNextNode();
			if (textTreeTextBlock != null)
			{
				num2 = Math.Min(textTreeTextBlock.FreeCapacity, num - textOffset);
				textTreeTextBlock.InsertText(0, text, num - num2, num);
				num -= num2;
			}
		}
		if (textOffset >= num)
		{
			return;
		}
		int num3 = 1;
		TextTreeTextBlock textTreeTextBlock2 = leftBlock.SplitBlock();
		num2 = Math.Min(textTreeTextBlock2.FreeCapacity, num - textOffset);
		textTreeTextBlock2.InsertText(0, text, num - num2, num);
		num -= num2;
		if (textOffset < num)
		{
			textOffset += leftBlock.InsertText(leftBlock.Count, text, textOffset, num);
			if (textOffset < num)
			{
				num3 += (num - textOffset + 4096 - 1) / 4096;
			}
		}
		for (int i = 0; i < num3 - 1; i++)
		{
			TextTreeTextBlock textTreeTextBlock3 = new TextTreeTextBlock(4096);
			textOffset += textTreeTextBlock3.InsertText(0, text, textOffset, num);
			textTreeTextBlock3.InsertAtNode(leftBlock, insertBefore: false);
			leftBlock = textTreeTextBlock3;
		}
		Invariant.Assert(textOffset == num, "Not all text copied!");
	}

	internal static void Remove(TextTreeTextBlock firstNode, TextTreeTextBlock lastNode)
	{
		SplayTreeNode previousNode = firstNode.GetPreviousNode();
		SplayTreeNode splayTreeNode;
		if (previousNode != null)
		{
			previousNode.Split();
			splayTreeNode = previousNode.ParentNode;
			previousNode.ParentNode = null;
		}
		else
		{
			splayTreeNode = firstNode.GetContainingNode();
		}
		SplayTreeNode rightSubTree = lastNode.Split();
		SplayTreeNode splayTreeNode2 = SplayTreeNode.Join(previousNode, rightSubTree);
		if (splayTreeNode != null)
		{
			splayTreeNode.ContainedNode = splayTreeNode2;
		}
		if (splayTreeNode2 != null)
		{
			splayTreeNode2.ParentNode = splayTreeNode;
		}
	}
}
