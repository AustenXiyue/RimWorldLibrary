using System.Collections;
using System.Windows.Controls;

namespace System.Windows.Documents;

internal sealed class FlowPosition : IComparable
{
	private FixedTextContainer _container;

	private FlowNode _flowNode;

	private int _offset;

	internal FixedTextContainer TextContainer => _container;

	internal bool IsBoundary => _flowNode.Type == FlowNodeType.Boundary;

	internal bool IsStart => _flowNode.Type == FlowNodeType.Start;

	internal bool IsEnd => _flowNode.Type == FlowNodeType.End;

	internal bool IsSymbol
	{
		get
		{
			FlowNodeType type = _flowNode.Type;
			if (type != FlowNodeType.Start && type != FlowNodeType.End)
			{
				return type == FlowNodeType.Object;
			}
			return true;
		}
	}

	internal bool IsRun => _flowNode.Type == FlowNodeType.Run;

	internal bool IsObject => _flowNode.Type == FlowNodeType.Object;

	internal FlowNode FlowNode => _flowNode;

	private int _NodeLength
	{
		get
		{
			if (IsRun)
			{
				return (int)_flowNode.Cookie;
			}
			return 1;
		}
	}

	private FixedTextBuilder _FixedTextBuilder => _container.FixedTextBuilder;

	private FixedFlowMap _FixedFlowMap => _container.FixedTextBuilder.FixedFlowMap;

	internal FlowPosition(FixedTextContainer container, FlowNode node, int offset)
	{
		_container = container;
		_flowNode = node;
		_offset = offset;
	}

	public object Clone()
	{
		return new FlowPosition(_container, _flowNode, _offset);
	}

	public int CompareTo(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		if (!(o is FlowPosition flow))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, o.GetType(), typeof(FlowPosition)), "o");
		}
		return _OverlapAwareCompare(flow);
	}

	public override int GetHashCode()
	{
		return _flowNode.GetHashCode() ^ _offset.GetHashCode();
	}

	internal int GetDistance(FlowPosition flow)
	{
		if (_flowNode.Equals(flow._flowNode))
		{
			return flow._offset - _offset;
		}
		int num = _OverlapAwareCompare(flow);
		FlowPosition flowPosition;
		FlowPosition flowPosition2;
		if (num == -1)
		{
			flowPosition = (FlowPosition)Clone();
			flowPosition2 = flow;
		}
		else
		{
			flowPosition = (FlowPosition)flow.Clone();
			flowPosition2 = this;
		}
		int num2 = 0;
		while (!flowPosition._IsSamePosition(flowPosition2))
		{
			if (flowPosition._flowNode.Equals(flowPosition2._flowNode))
			{
				num2 += flowPosition2._offset - flowPosition._offset;
				break;
			}
			int num3 = flowPosition._vScan(LogicalDirection.Forward, -1);
			num2 += num3;
		}
		return num * -1 * num2;
	}

	internal TextPointerContext GetPointerContext(LogicalDirection dir)
	{
		return _vGetSymbolType(dir);
	}

	internal int GetTextRunLength(LogicalDirection dir)
	{
		FlowPosition clingPosition = GetClingPosition(dir);
		if (dir == LogicalDirection.Forward)
		{
			return clingPosition._NodeLength - clingPosition._offset;
		}
		return clingPosition._offset;
	}

	internal int GetTextInRun(LogicalDirection dir, int maxLength, char[] chars, int startIndex)
	{
		FlowPosition clingPosition = GetClingPosition(dir);
		int nodeLength = clingPosition._NodeLength;
		int val = ((dir != LogicalDirection.Forward) ? clingPosition._offset : (nodeLength - clingPosition._offset));
		maxLength = Math.Min(maxLength, val);
		string flowText = _container.FixedTextBuilder.GetFlowText(clingPosition._flowNode);
		if (dir == LogicalDirection.Forward)
		{
			Array.Copy(flowText.ToCharArray(clingPosition._offset, maxLength), 0, chars, startIndex, maxLength);
		}
		else
		{
			Array.Copy(flowText.ToCharArray(clingPosition._offset - maxLength, maxLength), 0, chars, startIndex, maxLength);
		}
		return maxLength;
	}

	internal object GetAdjacentElement(LogicalDirection dir)
	{
		FlowPosition clingPosition = GetClingPosition(dir);
		FlowNodeType type = clingPosition._flowNode.Type;
		if (type == FlowNodeType.Noop)
		{
			return string.Empty;
		}
		object @object = ((FixedElement)clingPosition._flowNode.Cookie).GetObject();
		Image image = @object as Image;
		if (type == FlowNodeType.Object && image != null)
		{
			FixedSOMElement[] fixedSOMElements = clingPosition._flowNode.FixedSOMElements;
			if (fixedSOMElements != null && fixedSOMElements.Length != 0 && fixedSOMElements[0] is FixedSOMImage fixedSOMImage)
			{
				image.Width = fixedSOMImage.BoundingRect.Width;
				image.Height = fixedSOMImage.BoundingRect.Height;
			}
		}
		return @object;
	}

	internal FixedElement GetElement(LogicalDirection dir)
	{
		return (FixedElement)GetClingPosition(dir)._flowNode.Cookie;
	}

	internal FixedElement GetScopingElement()
	{
		FlowPosition flowPosition = (FlowPosition)Clone();
		int num = 0;
		TextPointerContext pointerContext;
		while (flowPosition.FlowNode.Fp > 0 && !IsVirtual(_FixedFlowMap[flowPosition.FlowNode.Fp - 1]) && (pointerContext = flowPosition.GetPointerContext(LogicalDirection.Backward)) != 0)
		{
			switch (pointerContext)
			{
			case TextPointerContext.ElementStart:
				if (num == 0)
				{
					return (FixedElement)flowPosition.GetClingPosition(LogicalDirection.Backward)._flowNode.Cookie;
				}
				num--;
				break;
			case TextPointerContext.ElementEnd:
				num++;
				break;
			}
			flowPosition.Move(LogicalDirection.Backward);
		}
		return _container.ContainerElement;
	}

	internal bool Move(int distance)
	{
		LogicalDirection dir = ((distance >= 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		distance = Math.Abs(distance);
		FlowNode flowNode = _flowNode;
		int offset = _offset;
		while (distance > 0)
		{
			int num = _vScan(dir, distance);
			if (num == 0)
			{
				_flowNode = flowNode;
				_offset = offset;
				return false;
			}
			distance -= num;
		}
		return true;
	}

	internal bool Move(LogicalDirection dir)
	{
		if (_vScan(dir, -1) > 0)
		{
			return true;
		}
		return false;
	}

	internal void MoveTo(FlowPosition flow)
	{
		_flowNode = flow._flowNode;
		_offset = flow._offset;
	}

	internal void AttachElement(FixedElement e)
	{
		_flowNode.AttachElement(e);
	}

	internal void GetFlowNode(LogicalDirection direction, out FlowNode flowNode, out int offsetStart)
	{
		FlowPosition clingPosition = GetClingPosition(direction);
		offsetStart = clingPosition._offset;
		flowNode = clingPosition._flowNode;
	}

	internal void GetFlowNodes(FlowPosition pEnd, out FlowNode[] flowNodes, out int offsetStart, out int offsetEnd)
	{
		flowNodes = null;
		offsetStart = 0;
		offsetEnd = 0;
		FlowPosition clingPosition = GetClingPosition(LogicalDirection.Forward);
		offsetStart = clingPosition._offset;
		ArrayList arrayList = new ArrayList();
		int num = GetDistance(pEnd);
		while (num > 0)
		{
			int num2 = clingPosition._vScan(LogicalDirection.Forward, num);
			num -= num2;
			if (clingPosition.IsRun || clingPosition.IsObject)
			{
				arrayList.Add(clingPosition._flowNode);
				offsetEnd = clingPosition._offset;
			}
		}
		flowNodes = (FlowNode[])arrayList.ToArray(typeof(FlowNode));
	}

	internal FlowPosition GetClingPosition(LogicalDirection dir)
	{
		FlowPosition flowPosition = (FlowPosition)Clone();
		if (dir == LogicalDirection.Forward)
		{
			if (_offset == _NodeLength)
			{
				FlowNode flowNode = _xGetNextFlowNode();
				if (!FlowNode.IsNull(flowNode))
				{
					flowPosition._flowNode = flowNode;
					flowPosition._offset = 0;
				}
			}
		}
		else if (_offset == 0)
		{
			FlowNode flowNode = _xGetPreviousFlowNode();
			if (!FlowNode.IsNull(flowNode))
			{
				flowPosition._flowNode = flowNode;
				flowPosition._offset = flowPosition._NodeLength;
			}
		}
		return flowPosition;
	}

	internal bool IsVirtual(FlowNode flowNode)
	{
		return flowNode.Type == FlowNodeType.Virtual;
	}

	private int _vScan(LogicalDirection dir, int limit)
	{
		if (limit == 0)
		{
			return 0;
		}
		FlowNode flowNode = _flowNode;
		int result = 0;
		if (dir == LogicalDirection.Forward)
		{
			if (_offset == _NodeLength || flowNode.Type == FlowNodeType.Boundary)
			{
				flowNode = _xGetNextFlowNode();
				if (FlowNode.IsNull(flowNode))
				{
					return result;
				}
				_flowNode = flowNode;
				result = _NodeLength;
			}
			else
			{
				result = _NodeLength - _offset;
			}
			_offset = _NodeLength;
			if (limit > 0 && result > limit)
			{
				int num = result - limit;
				result = limit;
				_offset -= num;
			}
		}
		else
		{
			if (_offset == 0 || flowNode.Type == FlowNodeType.Boundary)
			{
				flowNode = _xGetPreviousFlowNode();
				if (FlowNode.IsNull(flowNode))
				{
					return result;
				}
				_flowNode = flowNode;
				result = _NodeLength;
			}
			else
			{
				result = _offset;
			}
			_offset = 0;
			if (limit > 0 && result > limit)
			{
				int num2 = result - limit;
				result = limit;
				_offset += num2;
			}
		}
		return result;
	}

	private TextPointerContext _vGetSymbolType(LogicalDirection dir)
	{
		FlowNode flowNode = _flowNode;
		if (dir == LogicalDirection.Forward)
		{
			if (_offset == _NodeLength)
			{
				flowNode = _xGetNextFlowNode();
			}
			if (!FlowNode.IsNull(flowNode))
			{
				return _FlowNodeTypeToTextSymbol(flowNode.Type);
			}
		}
		else
		{
			if (_offset == 0)
			{
				flowNode = _xGetPreviousFlowNode();
			}
			if (!FlowNode.IsNull(flowNode))
			{
				return _FlowNodeTypeToTextSymbol(flowNode.Type);
			}
		}
		return TextPointerContext.None;
	}

	private FlowNode _xGetPreviousFlowNode()
	{
		if (_flowNode.Fp > 1)
		{
			FlowNode flowNode = _FixedFlowMap[_flowNode.Fp - 1];
			if (IsVirtual(flowNode))
			{
				_FixedTextBuilder.EnsureTextOMForPage((int)flowNode.Cookie);
				flowNode = _FixedFlowMap[_flowNode.Fp - 1];
			}
			if (flowNode.Type != 0)
			{
				return flowNode;
			}
		}
		return null;
	}

	private FlowNode _xGetNextFlowNode()
	{
		if (_flowNode.Fp < _FixedFlowMap.FlowCount - 1)
		{
			FlowNode flowNode = _FixedFlowMap[_flowNode.Fp + 1];
			if (IsVirtual(flowNode))
			{
				_FixedTextBuilder.EnsureTextOMForPage((int)flowNode.Cookie);
				flowNode = _FixedFlowMap[_flowNode.Fp + 1];
			}
			if (flowNode.Type != 0)
			{
				return flowNode;
			}
		}
		return null;
	}

	private bool _IsSamePosition(FlowPosition flow)
	{
		if (flow == null)
		{
			return false;
		}
		return _OverlapAwareCompare(flow) == 0;
	}

	private int _OverlapAwareCompare(FlowPosition flow)
	{
		if (this == flow)
		{
			return 0;
		}
		int num = _flowNode.CompareTo(flow._flowNode);
		if (num < 0)
		{
			if (_flowNode.Fp == flow._flowNode.Fp - 1 && _offset == _NodeLength && flow._offset == 0)
			{
				return 0;
			}
		}
		else if (num > 0)
		{
			if (flow._flowNode.Fp == _flowNode.Fp - 1 && flow._offset == flow._NodeLength && _offset == 0)
			{
				return 0;
			}
		}
		else
		{
			num = _offset.CompareTo(flow._offset);
		}
		return num;
	}

	private TextPointerContext _FlowNodeTypeToTextSymbol(FlowNodeType t)
	{
		switch (t)
		{
		case FlowNodeType.Start:
			return TextPointerContext.ElementStart;
		case FlowNodeType.End:
			return TextPointerContext.ElementEnd;
		case FlowNodeType.Run:
			return TextPointerContext.Text;
		case FlowNodeType.Object:
		case FlowNodeType.Noop:
			return TextPointerContext.EmbeddedElement;
		default:
			return TextPointerContext.None;
		}
	}
}
