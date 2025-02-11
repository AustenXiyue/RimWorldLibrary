using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal sealed class FixedTextContainer : ITextContainer
{
	private FixedDocument _fixedPanel;

	private FixedTextBuilder _fixedTextBuilder;

	private DependencyObject _parent;

	private FixedElement _containerElement;

	private FixedTextPointer _start;

	private FixedTextPointer _end;

	private Highlights _highlights;

	private ITextSelection _textSelection;

	private ITextView _textview;

	bool ITextContainer.IsReadOnly => true;

	ITextPointer ITextContainer.Start => _start;

	ITextPointer ITextContainer.End => _end;

	uint ITextContainer.Generation => 0u;

	Highlights ITextContainer.Highlights => Highlights;

	DependencyObject ITextContainer.Parent => _parent;

	ITextSelection ITextContainer.TextSelection
	{
		get
		{
			return TextSelection;
		}
		set
		{
			_textSelection = value;
		}
	}

	UndoManager ITextContainer.UndoManager => null;

	ITextView ITextContainer.TextView
	{
		get
		{
			return _textview;
		}
		set
		{
			_textview = value;
		}
	}

	int ITextContainer.SymbolCount => ((ITextContainer)this).Start.GetOffsetToPosition(((ITextContainer)this).End);

	int ITextContainer.IMECharCount
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal FixedDocument FixedDocument
	{
		get
		{
			if (_fixedPanel == null && _parent is FixedDocument)
			{
				_fixedPanel = (FixedDocument)_parent;
			}
			return _fixedPanel;
		}
	}

	internal FixedTextBuilder FixedTextBuilder => _fixedTextBuilder;

	internal FixedElement ContainerElement => _containerElement;

	internal Highlights Highlights
	{
		get
		{
			if (_highlights == null)
			{
				_highlights = new Highlights(this);
			}
			return _highlights;
		}
	}

	internal ITextSelection TextSelection => _textSelection;

	public event EventHandler Changing
	{
		add
		{
		}
		remove
		{
		}
	}

	public event TextContainerChangeEventHandler Change
	{
		add
		{
		}
		remove
		{
		}
	}

	public event TextContainerChangedEventHandler Changed
	{
		add
		{
		}
		remove
		{
		}
	}

	internal FixedTextContainer(DependencyObject parent)
	{
		_parent = parent;
		_CreateEmptyContainer();
	}

	void ITextContainer.BeginChange()
	{
	}

	void ITextContainer.BeginChangeNoUndo()
	{
		((ITextContainer)this).BeginChange();
	}

	void ITextContainer.EndChange()
	{
		((ITextContainer)this).EndChange(skipEvents: false);
	}

	void ITextContainer.EndChange(bool skipEvents)
	{
	}

	ITextPointer ITextContainer.CreatePointerAtOffset(int offset, LogicalDirection direction)
	{
		return ((ITextContainer)this).Start.CreatePointer(offset, direction);
	}

	ITextPointer ITextContainer.CreatePointerAtCharOffset(int charOffset, LogicalDirection direction)
	{
		throw new NotImplementedException();
	}

	ITextPointer ITextContainer.CreateDynamicTextPointer(StaticTextPointer position, LogicalDirection direction)
	{
		return ((ITextPointer)position.Handle0).CreatePointer(direction);
	}

	StaticTextPointer ITextContainer.CreateStaticPointerAtOffset(int offset)
	{
		return new StaticTextPointer(this, ((ITextContainer)this).CreatePointerAtOffset(offset, LogicalDirection.Forward));
	}

	TextPointerContext ITextContainer.GetPointerContext(StaticTextPointer pointer, LogicalDirection direction)
	{
		return ((ITextPointer)pointer.Handle0).GetPointerContext(direction);
	}

	int ITextContainer.GetOffsetToPosition(StaticTextPointer position1, StaticTextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).GetOffsetToPosition((ITextPointer)position2.Handle0);
	}

	int ITextContainer.GetTextInRun(StaticTextPointer position, LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		return ((ITextPointer)position.Handle0).GetTextInRun(direction, textBuffer, startIndex, count);
	}

	object ITextContainer.GetAdjacentElement(StaticTextPointer position, LogicalDirection direction)
	{
		return ((ITextPointer)position.Handle0).GetAdjacentElement(direction);
	}

	DependencyObject ITextContainer.GetParent(StaticTextPointer position)
	{
		return null;
	}

	StaticTextPointer ITextContainer.CreatePointer(StaticTextPointer position, int offset)
	{
		return new StaticTextPointer(this, ((ITextPointer)position.Handle0).CreatePointer(offset));
	}

	StaticTextPointer ITextContainer.GetNextContextPosition(StaticTextPointer position, LogicalDirection direction)
	{
		return new StaticTextPointer(this, ((ITextPointer)position.Handle0).GetNextContextPosition(direction));
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, StaticTextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).CompareTo((ITextPointer)position2.Handle0);
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, ITextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).CompareTo(position2);
	}

	object ITextContainer.GetValue(StaticTextPointer position, DependencyProperty formattingProperty)
	{
		return ((ITextPointer)position.Handle0).GetValue(formattingProperty);
	}

	internal FixedTextPointer VerifyPosition(ITextPointer position)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		if (position.TextContainer != this)
		{
			throw new ArgumentException(SR.Format(SR.NotInAssociatedContainer, "position"));
		}
		return (position as FixedTextPointer) ?? throw new ArgumentException(SR.Format(SR.BadFixedTextPosition, "position"));
	}

	internal int GetPageNumber(ITextPointer textPointer)
	{
		FixedTextPointer fixedTextPointer = textPointer as FixedTextPointer;
		int result = int.MaxValue;
		if (fixedTextPointer != null)
		{
			if (fixedTextPointer.CompareTo(((ITextContainer)this).Start) == 0)
			{
				result = 0;
			}
			else if (fixedTextPointer.CompareTo(((ITextContainer)this).End) == 0)
			{
				result = FixedDocument.PageCount - 1;
			}
			else
			{
				fixedTextPointer.FlowPosition.GetFlowNode(fixedTextPointer.LogicalDirection, out var flowNode, out var _);
				FixedElement fixedElement = flowNode.Cookie as FixedElement;
				FixedPosition fixedp;
				if (flowNode.Type == FlowNodeType.Boundary)
				{
					result = ((flowNode.Fp > 0) ? (FixedDocument.PageCount - 1) : 0);
				}
				else if (flowNode.Type == FlowNodeType.Virtual || flowNode.Type == FlowNodeType.Noop)
				{
					result = (int)flowNode.Cookie;
				}
				else if (fixedElement != null)
				{
					result = fixedElement.PageIndex;
				}
				else if (FixedTextBuilder.GetFixedPosition(fixedTextPointer.FlowPosition, fixedTextPointer.LogicalDirection, out fixedp))
				{
					result = fixedp.Page;
				}
			}
		}
		return result;
	}

	internal void GetMultiHighlights(FixedTextPointer start, FixedTextPointer end, Dictionary<FixedPage, ArrayList> highlights, FixedHighlightType t, Brush foregroundBrush, Brush backgroundBrush)
	{
		if (start.CompareTo(end) > 0)
		{
			FixedTextPointer fixedTextPointer = start;
			start = end;
			end = fixedTextPointer;
		}
		int startIndex = 0;
		int endIndex = 0;
		if (!_GetFixedNodesForFlowRange(start, end, out var elements, out startIndex, out endIndex))
		{
			return;
		}
		for (int i = 0; i < elements.Length; i++)
		{
			FixedSOMElement fixedSOMElement = elements[i];
			FixedNode fixedNode = fixedSOMElement.FixedNode;
			FixedPage fixedPage = FixedDocument.SyncGetPageWithCheck(fixedNode.Page);
			if (fixedPage == null)
			{
				continue;
			}
			DependencyObject element = fixedPage.GetElement(fixedNode);
			if (element == null)
			{
				continue;
			}
			int num = 0;
			UIElement element2;
			int num2;
			if (element is Image || element is Path)
			{
				element2 = (UIElement)element;
				num2 = 1;
			}
			else
			{
				if (!(element is Glyphs))
				{
					continue;
				}
				element2 = (UIElement)element;
				num = fixedSOMElement.StartIndex;
				num2 = fixedSOMElement.EndIndex;
			}
			if (i == 0)
			{
				num = startIndex;
			}
			if (i == elements.Length - 1)
			{
				num2 = endIndex;
			}
			ArrayList arrayList;
			if (highlights.ContainsKey(fixedPage))
			{
				arrayList = highlights[fixedPage];
			}
			else
			{
				arrayList = new ArrayList();
				highlights.Add(fixedPage, arrayList);
			}
			if (fixedSOMElement is FixedSOMTextRun { IsReversed: not false })
			{
				int num3 = num;
				num = fixedSOMElement.EndIndex - num2;
				num2 = fixedSOMElement.EndIndex - num3;
			}
			FixedHighlight value = new FixedHighlight(element2, num, num2, t, foregroundBrush, backgroundBrush);
			arrayList.Add(value);
		}
	}

	private void _CreateEmptyContainer()
	{
		_fixedTextBuilder = new FixedTextBuilder(this);
		_start = new FixedTextPointer(mutable: false, LogicalDirection.Backward, new FlowPosition(this, FixedTextBuilder.FixedFlowMap.FlowStartEdge, 1));
		_end = new FixedTextPointer(mutable: false, LogicalDirection.Forward, new FlowPosition(this, FixedTextBuilder.FixedFlowMap.FlowEndEdge, 0));
		_containerElement = new FixedElement(FixedElement.ElementType.Container, _start, _end, int.MaxValue);
		_start.FlowPosition.AttachElement(_containerElement);
		_end.FlowPosition.AttachElement(_containerElement);
	}

	internal void OnNewFlowElement(FixedElement parentElement, FixedElement.ElementType elementType, FlowPosition pStart, FlowPosition pEnd, object source, int pageIndex)
	{
		FixedTextPointer start = new FixedTextPointer(mutable: false, LogicalDirection.Backward, pStart);
		FixedTextPointer end = new FixedTextPointer(mutable: false, LogicalDirection.Forward, pEnd);
		FixedElement fixedElement = new FixedElement(elementType, start, end, pageIndex);
		if (source != null)
		{
			fixedElement.Object = source;
		}
		parentElement.Append(fixedElement);
		pStart.AttachElement(fixedElement);
		pEnd.AttachElement(fixedElement);
	}

	private bool _GetFixedNodesForFlowRange(ITextPointer start, ITextPointer end, out FixedSOMElement[] elements, out int startIndex, out int endIndex)
	{
		elements = null;
		startIndex = 0;
		endIndex = 0;
		if (start.CompareTo(end) == 0)
		{
			return false;
		}
		FixedTextPointer fixedTextPointer = (FixedTextPointer)start;
		FixedTextPointer fixedTextPointer2 = (FixedTextPointer)end;
		return FixedTextBuilder.GetFixedNodesForFlowRange(fixedTextPointer.FlowPosition, fixedTextPointer2.FlowPosition, out elements, out startIndex, out endIndex);
	}
}
