using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

internal sealed class FixedFlowMap
{
	internal const int FixedOrderStartPage = int.MinValue;

	internal const int FixedOrderEndPage = int.MaxValue;

	internal const int FixedOrderStartVisual = int.MinValue;

	internal const int FixedOrderEndVisual = int.MaxValue;

	internal const int FlowOrderBoundaryScopeId = int.MinValue;

	internal const int FlowOrderVirtualScopeId = -1;

	internal const int FlowOrderScopeIdStart = 0;

	private List<FlowNode> _flowOrder;

	private FlowNode _flowStart;

	private FlowNode _flowEnd;

	private static readonly FixedNode s_FixedStart = FixedNode.Create(int.MinValue, 1, int.MinValue, -1, null);

	private static readonly FixedNode s_FixedEnd = FixedNode.Create(int.MaxValue, 1, int.MaxValue, -1, null);

	private Hashtable _mapping;

	private FixedNode _cachedFixedNode;

	private List<FixedSOMElement> _cachedEntry;

	internal FlowNode this[int fp] => _flowOrder[fp];

	internal FixedNode FixedStartEdge => s_FixedStart;

	internal FlowNode FlowStartEdge => _flowStart;

	internal FlowNode FlowEndEdge => _flowEnd;

	internal int FlowCount => _flowOrder.Count;

	internal FixedFlowMap()
	{
		_Init();
	}

	internal void MappingReplace(FlowNode flowOld, List<FlowNode> flowNew)
	{
		int fp = flowOld.Fp;
		_flowOrder.RemoveAt(fp);
		_flowOrder.InsertRange(fp, flowNew);
		for (int i = fp; i < _flowOrder.Count; i++)
		{
			_flowOrder[i].SetFp(i);
		}
	}

	internal FixedSOMElement MappingGetFixedSOMElement(FixedNode fixedp, int offset)
	{
		List<FixedSOMElement> list = _GetEntry(fixedp);
		if (list != null)
		{
			foreach (FixedSOMElement item in list)
			{
				if (offset >= item.StartIndex && offset <= item.EndIndex)
				{
					return item;
				}
			}
		}
		return null;
	}

	internal FlowNode FlowOrderInsertBefore(FlowNode nextFlow, FlowNode newFlow)
	{
		_FlowOrderInsertBefore(nextFlow, newFlow);
		return newFlow;
	}

	internal void AddFixedElement(FixedSOMElement element)
	{
		_AddEntry(element);
	}

	private void _Init()
	{
		_flowStart = new FlowNode(int.MinValue, FlowNodeType.Boundary, null);
		_flowEnd = new FlowNode(int.MinValue, FlowNodeType.Boundary, null);
		_flowOrder = new List<FlowNode>();
		_flowOrder.Add(_flowStart);
		_flowStart.SetFp(0);
		_flowOrder.Add(_flowEnd);
		_flowEnd.SetFp(1);
		_mapping = new Hashtable();
	}

	internal void _FlowOrderInsertBefore(FlowNode nextFlow, FlowNode newFlow)
	{
		newFlow.SetFp(nextFlow.Fp);
		_flowOrder.Insert(newFlow.Fp, newFlow);
		int i = newFlow.Fp + 1;
		for (int count = _flowOrder.Count; i < count; i++)
		{
			_flowOrder[i].IncreaseFp();
		}
	}

	private List<FixedSOMElement> _GetEntry(FixedNode node)
	{
		if (_cachedEntry == null || node != _cachedFixedNode)
		{
			_cachedEntry = (List<FixedSOMElement>)_mapping[node];
			_cachedFixedNode = node;
		}
		return _cachedEntry;
	}

	private void _AddEntry(FixedSOMElement element)
	{
		FixedNode fixedNode = element.FixedNode;
		List<FixedSOMElement> list;
		if (_mapping.ContainsKey(fixedNode))
		{
			list = (List<FixedSOMElement>)_mapping[fixedNode];
		}
		else
		{
			list = new List<FixedSOMElement>();
			_mapping.Add(fixedNode, list);
		}
		list.Add(element);
	}
}
