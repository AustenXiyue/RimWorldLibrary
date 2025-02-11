using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal sealed class DocumentSequenceTextContainer : ITextContainer
{
	private sealed class DocumentSequenceHighlights : Highlights
	{
		internal DocumentSequenceHighlights(DocumentSequenceTextContainer textContainer)
			: base(textContainer)
		{
		}

		internal override object GetHighlightValue(StaticTextPointer textPosition, LogicalDirection direction, Type highlightLayerOwnerType)
		{
			if (EnsureParentPosition(textPosition, direction, out var parentPosition))
			{
				return base.GetHighlightValue(parentPosition, direction, highlightLayerOwnerType);
			}
			return DependencyProperty.UnsetValue;
		}

		internal override bool IsContentHighlighted(StaticTextPointer textPosition, LogicalDirection direction)
		{
			if (EnsureParentPosition(textPosition, direction, out var parentPosition))
			{
				return base.IsContentHighlighted(parentPosition, direction);
			}
			return false;
		}

		internal override StaticTextPointer GetNextHighlightChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
		{
			StaticTextPointer staticTextPointer = StaticTextPointer.Null;
			if (EnsureParentPosition(textPosition, direction, out var parentPosition))
			{
				staticTextPointer = base.GetNextHighlightChangePosition(parentPosition, direction);
				if (textPosition.TextContainer.Highlights != this)
				{
					staticTextPointer = GetStaticPositionInChildContainer(staticTextPointer, direction, textPosition);
				}
			}
			return staticTextPointer;
		}

		internal override StaticTextPointer GetNextPropertyChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
		{
			StaticTextPointer staticTextPointer = StaticTextPointer.Null;
			if (EnsureParentPosition(textPosition, direction, out var parentPosition))
			{
				staticTextPointer = base.GetNextPropertyChangePosition(parentPosition, direction);
				if (textPosition.TextContainer.Highlights != this)
				{
					staticTextPointer = GetStaticPositionInChildContainer(staticTextPointer, direction, textPosition);
				}
			}
			return staticTextPointer;
		}

		private bool EnsureParentPosition(StaticTextPointer textPosition, LogicalDirection direction, out StaticTextPointer parentPosition)
		{
			parentPosition = textPosition;
			if (textPosition.TextContainer.Highlights != this)
			{
				if (textPosition.GetPointerContext(direction) == TextPointerContext.None)
				{
					return false;
				}
				ITextPointer tp = textPosition.CreateDynamicTextPointer(LogicalDirection.Forward);
				ITextPointer textPointer = ((DocumentSequenceTextContainer)base.TextContainer).MapChildPositionToParent(tp);
				parentPosition = textPointer.CreateStaticPointer();
			}
			return true;
		}

		private StaticTextPointer GetStaticPositionInChildContainer(StaticTextPointer textPosition, LogicalDirection direction, StaticTextPointer originalPosition)
		{
			StaticTextPointer result = StaticTextPointer.Null;
			if (!textPosition.IsNull)
			{
				ITextPointer childPointer = (textPosition.CreateDynamicTextPointer(LogicalDirection.Forward) as DocumentSequenceTextPointer).ChildPointer;
				if (childPointer.TextContainer != originalPosition.TextContainer)
				{
					if (IsContentHighlighted(originalPosition, direction))
					{
						childPointer = ((direction == LogicalDirection.Forward) ? originalPosition.TextContainer.End : originalPosition.TextContainer.Start);
						result = childPointer.CreateStaticPointer();
					}
					else
					{
						result = StaticTextPointer.Null;
					}
				}
				else
				{
					result = childPointer.CreateStaticPointer();
				}
			}
			return result;
		}
	}

	private readonly FixedDocumentSequence _parent;

	private DocumentSequenceTextPointer _start;

	private DocumentSequenceTextPointer _end;

	private ChildDocumentBlock _doclistHead;

	private ChildDocumentBlock _doclistTail;

	private ITextSelection _textSelection;

	private Highlights _highlights;

	private int _changeBlockLevel;

	private TextContainerChangedEventArgs _changes;

	private ITextView _textview;

	private bool _isReadOnly;

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

	internal Highlights Highlights
	{
		get
		{
			if (_highlights == null)
			{
				_highlights = new DocumentSequenceHighlights(this);
			}
			return _highlights;
		}
	}

	internal ITextSelection TextSelection => _textSelection;

	public event EventHandler Changing;

	public event TextContainerChangeEventHandler Change;

	public event TextContainerChangedEventHandler Changed;

	internal DocumentSequenceTextContainer(DependencyObject parent)
	{
		_parent = (FixedDocumentSequence)parent;
		_Initialize();
	}

	void ITextContainer.BeginChange()
	{
		_changeBlockLevel++;
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
		Invariant.Assert(_changeBlockLevel > 0, "Unmatched EndChange call!");
		_changeBlockLevel--;
		if (_changeBlockLevel == 0 && _changes != null)
		{
			TextContainerChangedEventArgs changes = _changes;
			_changes = null;
			if (this.Changed != null && !skipEvents)
			{
				this.Changed(this, changes);
			}
		}
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

	internal DocumentSequenceTextPointer VerifyPosition(ITextPointer position)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		if (position.TextContainer != this)
		{
			throw new ArgumentException(SR.Format(SR.NotInAssociatedContainer, "position"));
		}
		return (position as DocumentSequenceTextPointer) ?? throw new ArgumentException(SR.Format(SR.BadFixedTextPosition, "position"));
	}

	internal DocumentSequenceTextPointer MapChildPositionToParent(ITextPointer tp)
	{
		for (ChildDocumentBlock childDocumentBlock = _doclistHead; childDocumentBlock != null; childDocumentBlock = childDocumentBlock.NextBlock)
		{
			if (childDocumentBlock.ChildContainer == tp.TextContainer)
			{
				return new DocumentSequenceTextPointer(childDocumentBlock, tp);
			}
		}
		return null;
	}

	internal ChildDocumentBlock FindChildBlock(DocumentReference docRef)
	{
		for (ChildDocumentBlock nextBlock = _doclistHead.NextBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
		{
			if (nextBlock.DocRef == docRef)
			{
				return nextBlock;
			}
		}
		return null;
	}

	internal int GetChildBlockDistance(ChildDocumentBlock block1, ChildDocumentBlock block2)
	{
		if (block1 == block2)
		{
			return 0;
		}
		int num = 0;
		for (ChildDocumentBlock childDocumentBlock = block1; childDocumentBlock != null; childDocumentBlock = childDocumentBlock.NextBlock)
		{
			if (childDocumentBlock == block2)
			{
				return num;
			}
			num++;
		}
		num = 0;
		for (ChildDocumentBlock childDocumentBlock = block1; childDocumentBlock != null; childDocumentBlock = childDocumentBlock.PreviousBlock)
		{
			if (childDocumentBlock == block2)
			{
				return num;
			}
			num--;
		}
		return 0;
	}

	private void _Initialize()
	{
		_doclistHead = new ChildDocumentBlock(this, new NullTextContainer());
		_doclistTail = new ChildDocumentBlock(this, new NullTextContainer());
		_doclistHead.InsertNextBlock(_doclistTail);
		ChildDocumentBlock childDocumentBlock = _doclistHead;
		foreach (DocumentReference reference in _parent.References)
		{
			childDocumentBlock.InsertNextBlock(new ChildDocumentBlock(this, reference));
			childDocumentBlock = childDocumentBlock.NextBlock;
		}
		if (_parent.References.Count != 0)
		{
			_start = new DocumentSequenceTextPointer(_doclistHead.NextBlock, _doclistHead.NextBlock.ChildContainer.Start);
			_end = new DocumentSequenceTextPointer(_doclistTail.PreviousBlock, _doclistTail.PreviousBlock.ChildContainer.End);
		}
		else
		{
			_start = new DocumentSequenceTextPointer(_doclistHead, _doclistHead.ChildContainer.Start);
			_end = new DocumentSequenceTextPointer(_doclistTail, _doclistTail.ChildContainer.End);
		}
		_parent.References.CollectionChanged += _OnContentChanged;
		Highlights.Changed += _OnHighlightChanged;
	}

	private void AddChange(ITextPointer startPosition, int symbolCount, PrecursorTextChangeType precursorTextChange)
	{
		Invariant.Assert(!_isReadOnly, "Illegal to modify DocumentSequenceTextContainer inside Change event scope!");
		((ITextContainer)this).BeginChange();
		try
		{
			if (this.Changing != null)
			{
				this.Changing(this, EventArgs.Empty);
			}
			if (_changes == null)
			{
				_changes = new TextContainerChangedEventArgs();
			}
			_changes.AddChange(precursorTextChange, DocumentSequenceTextPointer.GetOffsetToPosition(_start, startPosition), symbolCount, collectTextChanges: false);
			if (this.Change != null)
			{
				Invariant.Assert(precursorTextChange == PrecursorTextChangeType.ContentAdded || precursorTextChange == PrecursorTextChangeType.ContentRemoved);
				TextChangeType textChange = ((precursorTextChange != 0) ? TextChangeType.ContentRemoved : TextChangeType.ContentAdded);
				_isReadOnly = true;
				try
				{
					this.Change(this, new TextContainerChangeEventArgs(startPosition, symbolCount, -1, textChange));
					return;
				}
				finally
				{
					_isReadOnly = false;
				}
			}
		}
		finally
		{
			((ITextContainer)this).EndChange();
		}
	}

	private void _OnContentChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			if (args.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			object obj = args.NewItems[0];
			if (args.NewStartingIndex != _parent.References.Count - 1)
			{
				throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
			}
			ChildDocumentBlock childDocumentBlock = new ChildDocumentBlock(this, (DocumentReference)obj);
			ChildDocumentBlock previousBlock = _doclistTail.PreviousBlock;
			previousBlock.InsertNextBlock(childDocumentBlock);
			DocumentSequenceTextPointer startPosition = new DocumentSequenceTextPointer(previousBlock, previousBlock.End);
			_end = new DocumentSequenceTextPointer(childDocumentBlock, childDocumentBlock.ChildContainer.End);
			if (childDocumentBlock.NextBlock == _doclistTail && childDocumentBlock.PreviousBlock == _doclistHead)
			{
				_start = new DocumentSequenceTextPointer(childDocumentBlock, childDocumentBlock.ChildContainer.Start);
			}
			_ = childDocumentBlock.ChildContainer;
			int symbolCount = 1;
			AddChange(startPosition, symbolCount, PrecursorTextChangeType.ContentAdded);
			return;
		}
		throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
	}

	private void _OnHighlightChanged(object sender, HighlightChangedEventArgs args)
	{
		int num = 0;
		DocumentSequenceTextPointer documentSequenceTextPointer = null;
		ChildDocumentBlock childDocumentBlock = null;
		List<TextSegment> list = new List<TextSegment>(4);
		while (num < args.Ranges.Count)
		{
			TextSegment textSegment = (TextSegment)args.Ranges[num];
			DocumentSequenceTextPointer documentSequenceTextPointer2 = (DocumentSequenceTextPointer)textSegment.End;
			if (documentSequenceTextPointer == null)
			{
				documentSequenceTextPointer = (DocumentSequenceTextPointer)textSegment.Start;
			}
			ChildDocumentBlock childDocumentBlock2 = childDocumentBlock;
			childDocumentBlock = documentSequenceTextPointer.ChildBlock;
			if (childDocumentBlock2 != null && childDocumentBlock != childDocumentBlock2 && !(childDocumentBlock2.ChildContainer is NullTextContainer) && list.Count != 0)
			{
				childDocumentBlock2.ChildHighlightLayer.RaiseHighlightChangedEvent(new ReadOnlyCollection<TextSegment>(list));
				list.Clear();
			}
			ITextPointer childPointer = documentSequenceTextPointer.ChildPointer;
			if (documentSequenceTextPointer2.ChildBlock != childDocumentBlock)
			{
				ITextPointer end = documentSequenceTextPointer.ChildPointer.TextContainer.End;
				if (childPointer.CompareTo(end) != 0)
				{
					list.Add(new TextSegment(childPointer, end));
				}
				if (!(childDocumentBlock.ChildContainer is NullTextContainer) && list.Count != 0)
				{
					childDocumentBlock.ChildHighlightLayer.RaiseHighlightChangedEvent(new ReadOnlyCollection<TextSegment>(list));
				}
				childDocumentBlock = childDocumentBlock.NextBlock;
				documentSequenceTextPointer = new DocumentSequenceTextPointer(childDocumentBlock, childDocumentBlock.ChildContainer.Start);
				list.Clear();
			}
			else
			{
				ITextPointer end = documentSequenceTextPointer2.ChildPointer;
				if (childPointer.CompareTo(end) != 0)
				{
					list.Add(new TextSegment(childPointer, end));
				}
				num++;
				documentSequenceTextPointer = null;
			}
		}
		if (list.Count > 0 && childDocumentBlock != null && !(childDocumentBlock.ChildContainer is NullTextContainer))
		{
			childDocumentBlock.ChildHighlightLayer.RaiseHighlightChangedEvent(new ReadOnlyCollection<TextSegment>(list));
		}
	}
}
