using MS.Internal.Documents;

namespace System.Windows.Documents;

internal static class TextTreeUndo
{
	internal static void CreateInsertUndoUnit(TextContainer tree, int symbolOffset, int symbolCount)
	{
		GetOrClearUndoManager(tree)?.Add(new TextTreeInsertUndoUnit(tree, symbolOffset, symbolCount));
	}

	internal static void CreateInsertElementUndoUnit(TextContainer tree, int symbolOffset, bool deep)
	{
		GetOrClearUndoManager(tree)?.Add(new TextTreeInsertElementUndoUnit(tree, symbolOffset, deep));
	}

	internal static void CreatePropertyUndoUnit(TextElement element, DependencyPropertyChangedEventArgs e)
	{
		TextContainer textContainer = element.TextContainer;
		UndoManager orClearUndoManager = GetOrClearUndoManager(textContainer);
		if (orClearUndoManager != null)
		{
			PropertyRecord propertyRecord = default(PropertyRecord);
			propertyRecord.Property = e.Property;
			propertyRecord.Value = ((e.OldValueSource == BaseValueSourceInternal.Local) ? e.OldValue : DependencyProperty.UnsetValue);
			orClearUndoManager.Add(new TextTreePropertyUndoUnit(textContainer, element.TextElementNode.GetSymbolOffset(textContainer.Generation) + 1, propertyRecord));
		}
	}

	internal static TextTreeDeleteContentUndoUnit CreateDeleteContentUndoUnit(TextContainer tree, TextPointer start, TextPointer end)
	{
		if (start.CompareTo(end) == 0)
		{
			return null;
		}
		UndoManager orClearUndoManager = GetOrClearUndoManager(tree);
		if (orClearUndoManager == null)
		{
			return null;
		}
		TextTreeDeleteContentUndoUnit textTreeDeleteContentUndoUnit = new TextTreeDeleteContentUndoUnit(tree, start, end);
		orClearUndoManager.Add(textTreeDeleteContentUndoUnit);
		return textTreeDeleteContentUndoUnit;
	}

	internal static TextTreeExtractElementUndoUnit CreateExtractElementUndoUnit(TextContainer tree, TextTreeTextElementNode elementNode)
	{
		UndoManager orClearUndoManager = GetOrClearUndoManager(tree);
		if (orClearUndoManager == null)
		{
			return null;
		}
		TextTreeExtractElementUndoUnit textTreeExtractElementUndoUnit = new TextTreeExtractElementUndoUnit(tree, elementNode);
		orClearUndoManager.Add(textTreeExtractElementUndoUnit);
		return textTreeExtractElementUndoUnit;
	}

	internal static UndoManager GetOrClearUndoManager(ITextContainer textContainer)
	{
		UndoManager undoManager = textContainer.UndoManager;
		if (undoManager == null)
		{
			return null;
		}
		if (!undoManager.IsEnabled)
		{
			return null;
		}
		if (undoManager.OpenedUnit == null)
		{
			undoManager.Clear();
			return null;
		}
		return undoManager;
	}
}
