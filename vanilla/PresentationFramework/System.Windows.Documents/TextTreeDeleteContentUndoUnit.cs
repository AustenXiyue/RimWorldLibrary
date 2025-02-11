using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeDeleteContentUndoUnit : TextTreeUndoUnit
{
	private abstract class ContentContainer
	{
		private ContentContainer _nextContainer;

		internal ContentContainer NextContainer
		{
			get
			{
				return _nextContainer;
			}
			set
			{
				_nextContainer = value;
			}
		}

		internal abstract void Do(TextPointer navigator);
	}

	private class TextContentContainer : ContentContainer
	{
		private readonly char[] _text;

		internal TextContentContainer(char[] text)
		{
			_text = text;
		}

		internal override void Do(TextPointer navigator)
		{
			navigator.TextContainer.InsertTextInternal(navigator, _text);
		}
	}

	private class ObjectContentContainer : ContentContainer
	{
		private readonly string _xml;

		private readonly object _element;

		internal ObjectContentContainer(string xml, object element)
		{
			_xml = xml;
			_element = element;
		}

		internal override void Do(TextPointer navigator)
		{
			DependencyObject dependencyObject = null;
			if (_xml != null)
			{
				try
				{
					dependencyObject = (DependencyObject)XamlReader.Load(new XmlTextReader(new StringReader(_xml)));
				}
				catch (XamlParseException ex)
				{
					Invariant.Assert(ex != null);
				}
			}
			if (dependencyObject == null)
			{
				dependencyObject = new Grid();
			}
			navigator.TextContainer.InsertEmbeddedObjectInternal(navigator, dependencyObject);
		}
	}

	private class ElementContentContainer : ContentContainer
	{
		private readonly Type _elementType;

		private readonly PropertyRecord[] _localValues;

		private readonly ResourceDictionary _resources;

		private readonly ContentContainer _childContainer;

		internal ElementContentContainer(Type elementType, PropertyRecord[] localValues, ResourceDictionary resources, ContentContainer childContainer)
		{
			_elementType = elementType;
			_localValues = localValues;
			_childContainer = childContainer;
			_resources = resources;
		}

		internal override void Do(TextPointer navigator)
		{
			TextElement obj = (TextElement)Activator.CreateInstance(_elementType);
			obj.Reposition(navigator, navigator);
			navigator.MoveToNextContextPosition(LogicalDirection.Backward);
			navigator.TextContainer.SetValues(navigator, TextTreeUndoUnit.ArrayToLocalValueEnumerator(_localValues));
			obj.Resources = _resources;
			for (ContentContainer contentContainer = _childContainer; contentContainer != null; contentContainer = contentContainer.NextContainer)
			{
				contentContainer.Do(navigator);
			}
			navigator.MoveToNextContextPosition(LogicalDirection.Forward);
		}
	}

	private class TableElementContentContainer : ElementContentContainer
	{
		private TableColumn[] _columns;

		private int _cpTable;

		internal TableElementContentContainer(Table table, PropertyRecord[] localValues, ContentContainer childContainer)
			: base(table.GetType(), localValues, table.Resources, childContainer)
		{
			_cpTable = table.TextContainer.Start.GetOffsetToPosition(table.ContentStart);
			_columns = SaveColumns(table);
		}

		internal override void Do(TextPointer navigator)
		{
			base.Do(navigator);
			if (_columns != null)
			{
				RestoreColumns((Table)new TextPointer(navigator.TextContainer.Start, _cpTable, LogicalDirection.Forward).Parent, _columns);
			}
		}
	}

	private readonly ContentContainer _content;

	internal TextTreeDeleteContentUndoUnit(TextContainer tree, TextPointer start, TextPointer end)
		: base(tree, start.GetSymbolOffset())
	{
		start.DebugAssertGeneration();
		end.DebugAssertGeneration();
		Invariant.Assert(start.GetScopingNode() == end.GetScopingNode(), "start/end have different scope!");
		TextTreeNode adjacentNode = start.GetAdjacentNode(LogicalDirection.Forward);
		TextTreeNode adjacentNode2 = end.GetAdjacentNode(LogicalDirection.Forward);
		_content = CopyContent(adjacentNode, adjacentNode2);
	}

	public override void DoCore()
	{
		VerifyTreeContentHashCode();
		TextPointer navigator = new TextPointer(base.TextContainer, base.SymbolOffset, LogicalDirection.Forward);
		for (ContentContainer contentContainer = _content; contentContainer != null; contentContainer = contentContainer.NextContainer)
		{
			contentContainer.Do(navigator);
		}
	}

	internal static TableColumn[] SaveColumns(Table table)
	{
		TableColumn[] array;
		if (table.Columns.Count > 0)
		{
			array = new TableColumn[table.Columns.Count];
			for (int i = 0; i < table.Columns.Count; i++)
			{
				array[i] = CopyColumn(table.Columns[i]);
			}
		}
		else
		{
			array = null;
		}
		return array;
	}

	internal static void RestoreColumns(Table table, TableColumn[] savedColumns)
	{
		if (savedColumns == null)
		{
			return;
		}
		for (int i = 0; i < savedColumns.Length; i++)
		{
			if (table.Columns.Count <= i)
			{
				table.Columns.Add(CopyColumn(savedColumns[i]));
			}
		}
	}

	private static TableColumn CopyColumn(TableColumn sourceTableColumn)
	{
		TableColumn tableColumn = new TableColumn();
		LocalValueEnumerator localValueEnumerator = sourceTableColumn.GetLocalValueEnumerator();
		while (localValueEnumerator.MoveNext())
		{
			LocalValueEntry current = localValueEnumerator.Current;
			if (!current.Property.ReadOnly)
			{
				tableColumn.SetValue(current.Property, current.Value);
			}
		}
		return tableColumn;
	}

	private ContentContainer CopyContent(TextTreeNode node, TextTreeNode haltNode)
	{
		ContentContainer result = null;
		ContentContainer contentContainer = null;
		while (node != haltNode && node != null)
		{
			ContentContainer container;
			if (node is TextTreeTextNode textNode)
			{
				node = CopyTextNode(textNode, haltNode, out container);
			}
			else if (node is TextTreeObjectNode objectNode)
			{
				node = CopyObjectNode(objectNode, out container);
			}
			else
			{
				Invariant.Assert(node is TextTreeTextElementNode, "Unexpected TextTreeNode type!");
				TextTreeTextElementNode elementNode = (TextTreeTextElementNode)node;
				node = CopyElementNode(elementNode, out container);
			}
			if (contentContainer == null)
			{
				result = container;
			}
			else
			{
				contentContainer.NextContainer = container;
			}
			contentContainer = container;
		}
		return result;
	}

	private TextTreeNode CopyTextNode(TextTreeTextNode textNode, TextTreeNode haltNode, out ContentContainer container)
	{
		Invariant.Assert(textNode != haltNode, "Expect at least one node to copy!");
		int symbolOffset = textNode.GetSymbolOffset(base.TextContainer.Generation);
		int num = 0;
		SplayTreeNode splayTreeNode = textNode;
		do
		{
			num += textNode.SymbolCount;
			splayTreeNode = textNode.GetNextNode();
			textNode = splayTreeNode as TextTreeTextNode;
		}
		while (textNode != null && textNode != haltNode);
		char[] array = new char[num];
		TextTreeText.ReadText(base.TextContainer.RootTextBlock, symbolOffset, num, array, 0);
		container = new TextContentContainer(array);
		return (TextTreeNode)splayTreeNode;
	}

	private TextTreeNode CopyObjectNode(TextTreeObjectNode objectNode, out ContentContainer container)
	{
		string xml = XamlWriter.Save(objectNode.EmbeddedElement);
		container = new ObjectContentContainer(xml, objectNode.EmbeddedElement);
		return (TextTreeNode)objectNode.GetNextNode();
	}

	private TextTreeNode CopyElementNode(TextTreeTextElementNode elementNode, out ContentContainer container)
	{
		if (elementNode.TextElement is Table)
		{
			container = new TableElementContentContainer(elementNode.TextElement as Table, TextTreeUndoUnit.GetPropertyRecordArray(elementNode.TextElement), CopyContent((TextTreeNode)elementNode.GetFirstContainedNode(), null));
		}
		else
		{
			container = new ElementContentContainer(elementNode.TextElement.GetType(), TextTreeUndoUnit.GetPropertyRecordArray(elementNode.TextElement), elementNode.TextElement.Resources, CopyContent((TextTreeNode)elementNode.GetFirstContainedNode(), null));
		}
		return (TextTreeNode)elementNode.GetNextNode();
	}
}
