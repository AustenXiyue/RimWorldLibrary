using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace System.Windows.Documents;

internal sealed class FixedDSBuilder
{
	private class NameHashFixedNode
	{
		internal UIElement uiElement;

		internal int index;

		internal NameHashFixedNode(UIElement e, int i)
		{
			uiElement = e;
			index = i;
		}
	}

	private StoryFragments _storyFragments;

	private FixedPage _fixedPage;

	private List<FixedNode> _fixedNodes;

	private BitArray _visitedArray;

	private Dictionary<string, NameHashFixedNode> _nameHashTable;

	private FixedTextBuilder.FlowModelBuilder _flowBuilder;

	public StoryFragments StoryFragments => _storyFragments;

	public FixedDSBuilder(FixedPage fp, StoryFragments sf)
	{
		_nameHashTable = new Dictionary<string, NameHashFixedNode>();
		_fixedPage = fp;
		_storyFragments = sf;
	}

	public void BuildNameHashTable(string Name, UIElement e, int indexToFixedNodes)
	{
		if (!_nameHashTable.ContainsKey(Name))
		{
			_nameHashTable.Add(Name, new NameHashFixedNode(e, indexToFixedNodes));
		}
	}

	public void ConstructFlowNodes(FixedTextBuilder.FlowModelBuilder flowBuilder, List<FixedNode> fixedNodes)
	{
		_fixedNodes = fixedNodes;
		_visitedArray = new BitArray(fixedNodes.Count);
		_flowBuilder = flowBuilder;
		foreach (StoryFragment storyFragment in StoryFragments.StoryFragmentList)
		{
			foreach (BlockElement blockElement in storyFragment.BlockElementList)
			{
				_CreateFlowNodes(blockElement);
			}
		}
		_flowBuilder.AddStartNode(FixedElement.ElementType.Paragraph);
		for (int i = 0; i < _visitedArray.Count; i++)
		{
			if (!_visitedArray[i])
			{
				AddFixedNodeInFlow(i, null);
			}
		}
		_flowBuilder.AddEndNode();
		_flowBuilder.AddLeftoverHyperlinks();
	}

	private void AddFixedNodeInFlow(int index, UIElement e)
	{
		if (!_visitedArray[index])
		{
			FixedNode fixedNode = _fixedNodes[index];
			if (e == null)
			{
				e = _fixedPage.GetElement(fixedNode) as UIElement;
			}
			_visitedArray[index] = true;
			FixedSOMElement fixedSOMElement = FixedSOMElement.CreateFixedSOMElement(_fixedPage, e, fixedNode, -1, -1);
			if (fixedSOMElement != null)
			{
				_flowBuilder.AddElement(fixedSOMElement);
			}
		}
	}

	private void _CreateFlowNodes(BlockElement be)
	{
		if (be is NamedElement ne)
		{
			ConstructSomElement(ne);
		}
		else
		{
			if (!(be is SemanticBasicElement semanticBasicElement))
			{
				return;
			}
			_flowBuilder.AddStartNode(be.ElementType);
			XmlLanguage value = (XmlLanguage)_fixedPage.GetValue(FrameworkElement.LanguageProperty);
			_flowBuilder.FixedElement.SetValue(FixedElement.LanguageProperty, value);
			SpecialProcessing(semanticBasicElement);
			foreach (BlockElement blockElement in semanticBasicElement.BlockElementList)
			{
				_CreateFlowNodes(blockElement);
			}
			_flowBuilder.AddEndNode();
		}
	}

	private void AddChildofFixedNodeinFlow(int[] childIndex, NamedElement ne)
	{
		FixedNode item = FixedNode.Create(_fixedNodes[0].Page, childIndex);
		int num = _fixedNodes.BinarySearch(item);
		if (num >= 0)
		{
			int num2 = num - 1;
			while (num2 >= 0 && _fixedNodes[num2].ComparetoIndex(childIndex) == 0)
			{
				num2--;
			}
			for (int i = num2 + 1; i < _fixedNodes.Count && _fixedNodes[i].ComparetoIndex(childIndex) == 0; i++)
			{
				AddFixedNodeInFlow(i, null);
			}
		}
	}

	private void SpecialProcessing(SemanticBasicElement sbe)
	{
		if (sbe is ListItemStructure { Marker: not null } listItemStructure && _nameHashTable.TryGetValue(listItemStructure.Marker, out var value))
		{
			_visitedArray[value.index] = true;
		}
	}

	private void ConstructSomElement(NamedElement ne)
	{
		if (_nameHashTable.TryGetValue(ne.NameReference, out var value))
		{
			if (value.uiElement is Glyphs || value.uiElement is Path || value.uiElement is Image)
			{
				AddFixedNodeInFlow(value.index, value.uiElement);
			}
			else if (value.uiElement is Canvas)
			{
				int[] childIndex = _fixedPage._CreateChildIndex(value.uiElement);
				AddChildofFixedNodeinFlow(childIndex, ne);
			}
		}
	}
}
