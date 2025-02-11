using MS.Internal;

namespace System.Windows.Documents;

internal sealed class DocumentSequenceTextPointer : ContentPosition, ITextPointer
{
	private ChildDocumentBlock _childBlock;

	private ITextPointer _childTp;

	private bool _isFrozen;

	Type ITextPointer.ParentType => GetElementType(this);

	ITextContainer ITextPointer.TextContainer => AggregatedContainer;

	bool ITextPointer.HasValidLayout
	{
		get
		{
			if (((ITextPointer)this).TextContainer.TextView != null && ((ITextPointer)this).TextContainer.TextView.IsValid)
			{
				return ((ITextPointer)this).TextContainer.TextView.Contains(this);
			}
			return false;
		}
	}

	bool ITextPointer.IsAtCaretUnitBoundary
	{
		get
		{
			Invariant.Assert(((ITextPointer)this).HasValidLayout);
			ITextView textView = ((ITextPointer)this).TextContainer.TextView;
			bool flag = textView.IsAtCaretUnitBoundary(this);
			if (!flag && ((ITextPointer)this).LogicalDirection == LogicalDirection.Backward)
			{
				ITextPointer position = ((ITextPointer)this).CreatePointer(LogicalDirection.Forward);
				flag = textView.IsAtCaretUnitBoundary(position);
			}
			return flag;
		}
	}

	LogicalDirection ITextPointer.LogicalDirection => _childTp.LogicalDirection;

	bool ITextPointer.IsAtInsertionPosition => TextPointerBase.IsAtInsertionPosition(this);

	bool ITextPointer.IsFrozen => _isFrozen;

	int ITextPointer.Offset => TextPointerBase.GetOffset(this);

	int ITextPointer.CharOffset
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal DocumentSequenceTextContainer AggregatedContainer => _childBlock.AggregatedContainer;

	internal ChildDocumentBlock ChildBlock
	{
		get
		{
			return _childBlock;
		}
		set
		{
			_childBlock = value;
		}
	}

	internal ITextPointer ChildPointer
	{
		get
		{
			return _childTp;
		}
		set
		{
			_childTp = value;
		}
	}

	internal DocumentSequenceTextPointer(ChildDocumentBlock childBlock, ITextPointer childPosition)
	{
		_childBlock = childBlock;
		_childTp = childPosition;
	}

	void ITextPointer.SetLogicalDirection(LogicalDirection direction)
	{
		_childTp.SetLogicalDirection(direction);
	}

	int ITextPointer.CompareTo(ITextPointer position)
	{
		return CompareTo(this, position);
	}

	int ITextPointer.CompareTo(StaticTextPointer position)
	{
		return ((ITextPointer)this).CompareTo((ITextPointer)position.Handle0);
	}

	int ITextPointer.GetOffsetToPosition(ITextPointer position)
	{
		return GetOffsetToPosition(this, position);
	}

	TextPointerContext ITextPointer.GetPointerContext(LogicalDirection direction)
	{
		return GetPointerContext(this, direction);
	}

	int ITextPointer.GetTextRunLength(LogicalDirection direction)
	{
		return GetTextRunLength(this, direction);
	}

	string ITextPointer.GetTextInRun(LogicalDirection direction)
	{
		return TextPointerBase.GetTextInRun(this, direction);
	}

	int ITextPointer.GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		return GetTextInRun(this, direction, textBuffer, startIndex, count);
	}

	object ITextPointer.GetAdjacentElement(LogicalDirection direction)
	{
		return GetAdjacentElement(this, direction);
	}

	Type ITextPointer.GetElementType(LogicalDirection direction)
	{
		return GetElementType(this, direction);
	}

	bool ITextPointer.HasEqualScope(ITextPointer position)
	{
		return HasEqualScope(this, position);
	}

	object ITextPointer.GetValue(DependencyProperty property)
	{
		return GetValue(this, property);
	}

	object ITextPointer.ReadLocalValue(DependencyProperty property)
	{
		return ReadLocalValue(this, property);
	}

	LocalValueEnumerator ITextPointer.GetLocalValueEnumerator()
	{
		return GetLocalValueEnumerator(this);
	}

	ITextPointer ITextPointer.CreatePointer()
	{
		return CreatePointer(this);
	}

	StaticTextPointer ITextPointer.CreateStaticPointer()
	{
		return new StaticTextPointer(((ITextPointer)this).TextContainer, ((ITextPointer)this).CreatePointer());
	}

	ITextPointer ITextPointer.CreatePointer(int distance)
	{
		return CreatePointer(this, distance);
	}

	ITextPointer ITextPointer.CreatePointer(LogicalDirection gravity)
	{
		return CreatePointer(this, gravity);
	}

	ITextPointer ITextPointer.CreatePointer(int distance, LogicalDirection gravity)
	{
		return CreatePointer(this, distance, gravity);
	}

	void ITextPointer.Freeze()
	{
		_isFrozen = true;
	}

	ITextPointer ITextPointer.GetFrozenPointer(LogicalDirection logicalDirection)
	{
		return TextPointerBase.GetFrozenPointer(this, logicalDirection);
	}

	void ITextPointer.InsertTextInRun(string textData)
	{
		throw new InvalidOperationException(SR.DocumentReadOnly);
	}

	void ITextPointer.DeleteContentToPosition(ITextPointer limit)
	{
		throw new InvalidOperationException(SR.DocumentReadOnly);
	}

	ITextPointer ITextPointer.GetNextContextPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		if (textPointer.MoveToNextContextPosition(direction))
		{
			textPointer.Freeze();
		}
		else
		{
			textPointer = null;
		}
		return textPointer;
	}

	ITextPointer ITextPointer.GetInsertionPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		textPointer.MoveToInsertionPosition(direction);
		textPointer.Freeze();
		return textPointer;
	}

	ITextPointer ITextPointer.GetFormatNormalizedPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		TextPointerBase.MoveToFormatNormalizedPosition(textPointer, direction);
		textPointer.Freeze();
		return textPointer;
	}

	ITextPointer ITextPointer.GetNextInsertionPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		if (textPointer.MoveToNextInsertionPosition(direction))
		{
			textPointer.Freeze();
		}
		else
		{
			textPointer = null;
		}
		return textPointer;
	}

	bool ITextPointer.ValidateLayout()
	{
		return TextPointerBase.ValidateLayout(this, ((ITextPointer)this).TextContainer.TextView);
	}

	bool ITextPointer.MoveToNextContextPosition(LogicalDirection direction)
	{
		return iScan(this, direction);
	}

	int ITextPointer.MoveByOffset(int offset)
	{
		if (_isFrozen)
		{
			throw new InvalidOperationException(SR.TextPositionIsFrozen);
		}
		if (iScan(this, offset))
		{
			return offset;
		}
		return 0;
	}

	void ITextPointer.MoveToPosition(ITextPointer position)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = AggregatedContainer.VerifyPosition(position);
		LogicalDirection logicalDirection = ChildPointer.LogicalDirection;
		ChildBlock = documentSequenceTextPointer.ChildBlock;
		if (ChildPointer.TextContainer == documentSequenceTextPointer.ChildPointer.TextContainer)
		{
			ChildPointer.MoveToPosition(documentSequenceTextPointer.ChildPointer);
			return;
		}
		ChildPointer = documentSequenceTextPointer.ChildPointer.CreatePointer();
		ChildPointer.SetLogicalDirection(logicalDirection);
	}

	void ITextPointer.MoveToElementEdge(ElementEdge edge)
	{
		ChildPointer.MoveToElementEdge(edge);
	}

	int ITextPointer.MoveToLineBoundary(int count)
	{
		return TextPointerBase.MoveToLineBoundary(this, ((ITextPointer)this).TextContainer.TextView, count, respectNonMeargeableInlineStart: true);
	}

	Rect ITextPointer.GetCharacterRect(LogicalDirection direction)
	{
		return TextPointerBase.GetCharacterRect(this, direction);
	}

	bool ITextPointer.MoveToInsertionPosition(LogicalDirection direction)
	{
		return TextPointerBase.MoveToInsertionPosition(this, direction);
	}

	bool ITextPointer.MoveToNextInsertionPosition(LogicalDirection direction)
	{
		return TextPointerBase.MoveToNextInsertionPosition(this, direction);
	}

	public static int CompareTo(DocumentSequenceTextPointer thisTp, ITextPointer position)
	{
		DocumentSequenceTextPointer tp = thisTp.AggregatedContainer.VerifyPosition(position);
		return xGapAwareCompareTo(thisTp, tp);
	}

	public static int GetOffsetToPosition(DocumentSequenceTextPointer thisTp, ITextPointer position)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = thisTp.AggregatedContainer.VerifyPosition(position);
		int num = xGapAwareCompareTo(thisTp, documentSequenceTextPointer);
		if (num == 0)
		{
			return 0;
		}
		if (num <= 0)
		{
			return xGapAwareGetDistance(thisTp, documentSequenceTextPointer);
		}
		return -1 * xGapAwareGetDistance(documentSequenceTextPointer, thisTp);
	}

	public static TextPointerContext GetPointerContext(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return xGapAwareGetSymbolType(thisTp, direction);
	}

	public static int GetTextRunLength(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return thisTp.ChildPointer.GetTextRunLength(direction);
	}

	public static int GetTextInRun(DocumentSequenceTextPointer thisTp, LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		if (textBuffer == null)
		{
			throw new ArgumentNullException("textBuffer");
		}
		if (startIndex < 0)
		{
			throw new ArgumentException(SR.Format(SR.NegativeValue, "startIndex"));
		}
		if (startIndex > textBuffer.Length)
		{
			throw new ArgumentException(SR.Format(SR.StartIndexExceedsBufferSize, startIndex, textBuffer.Length));
		}
		if (count < 0)
		{
			throw new ArgumentException(SR.Format(SR.NegativeValue, "count"));
		}
		if (count > textBuffer.Length - startIndex)
		{
			throw new ArgumentException(SR.Format(SR.MaxLengthExceedsBufferSize, count, textBuffer.Length, startIndex));
		}
		return thisTp.ChildPointer.GetTextInRun(direction, textBuffer, startIndex, count);
	}

	public static object GetAdjacentElement(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return xGapAwareGetEmbeddedElement(thisTp, direction);
	}

	public static Type GetElementType(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return xGetClingDSTP(thisTp, direction).ChildPointer.GetElementType(direction);
	}

	public static Type GetElementType(DocumentSequenceTextPointer thisTp)
	{
		return thisTp.ChildPointer.ParentType;
	}

	public static bool HasEqualScope(DocumentSequenceTextPointer thisTp, ITextPointer position)
	{
		DocumentSequenceTextPointer documentSequenceTextPointer = thisTp.AggregatedContainer.VerifyPosition(position);
		if (thisTp.ChildPointer.TextContainer == documentSequenceTextPointer.ChildPointer.TextContainer)
		{
			return thisTp.ChildPointer.HasEqualScope(documentSequenceTextPointer.ChildPointer);
		}
		if (thisTp.ChildPointer.ParentType == typeof(FixedDocument))
		{
			return documentSequenceTextPointer.ChildPointer.ParentType == typeof(FixedDocument);
		}
		return false;
	}

	public static object GetValue(DocumentSequenceTextPointer thisTp, DependencyProperty property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		return thisTp.ChildPointer.GetValue(property);
	}

	public static object ReadLocalValue(DocumentSequenceTextPointer thisTp, DependencyProperty property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		return thisTp.ChildPointer.ReadLocalValue(property);
	}

	public static LocalValueEnumerator GetLocalValueEnumerator(DocumentSequenceTextPointer thisTp)
	{
		return thisTp.ChildPointer.GetLocalValueEnumerator();
	}

	public static ITextPointer CreatePointer(DocumentSequenceTextPointer thisTp)
	{
		return CreatePointer(thisTp, 0, thisTp.ChildPointer.LogicalDirection);
	}

	public static ITextPointer CreatePointer(DocumentSequenceTextPointer thisTp, int distance)
	{
		return CreatePointer(thisTp, distance, thisTp.ChildPointer.LogicalDirection);
	}

	public static ITextPointer CreatePointer(DocumentSequenceTextPointer thisTp, LogicalDirection gravity)
	{
		return CreatePointer(thisTp, 0, gravity);
	}

	public static ITextPointer CreatePointer(DocumentSequenceTextPointer thisTp, int distance, LogicalDirection gravity)
	{
		ValidationHelper.VerifyDirection(gravity, "gravity");
		DocumentSequenceTextPointer documentSequenceTextPointer = new DocumentSequenceTextPointer(thisTp.ChildBlock, thisTp.ChildPointer.CreatePointer(gravity));
		if (distance != 0 && !xGapAwareScan(documentSequenceTextPointer, distance))
		{
			throw new ArgumentException(SR.BadDistance, "distance");
		}
		return documentSequenceTextPointer;
	}

	internal static bool iScan(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		bool flag = thisTp.ChildPointer.MoveToNextContextPosition(direction);
		if (!flag)
		{
			flag = xGapAwareScan(thisTp, (direction == LogicalDirection.Forward) ? 1 : (-1));
		}
		return flag;
	}

	internal static bool iScan(DocumentSequenceTextPointer thisTp, int distance)
	{
		return xGapAwareScan(thisTp, distance);
	}

	private static DocumentSequenceTextPointer xGetClingDSTP(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		TextPointerContext pointerContext = thisTp.ChildPointer.GetPointerContext(direction);
		if (pointerContext != 0)
		{
			return thisTp;
		}
		ChildDocumentBlock childDocumentBlock = thisTp.ChildBlock;
		ITextPointer textPointer = thisTp.ChildPointer;
		if (direction == LogicalDirection.Forward)
		{
			while (pointerContext == TextPointerContext.None && !childDocumentBlock.IsTail)
			{
				childDocumentBlock = childDocumentBlock.NextBlock;
				textPointer = childDocumentBlock.ChildContainer.Start;
				pointerContext = textPointer.GetPointerContext(direction);
			}
		}
		else
		{
			while (pointerContext == TextPointerContext.None && !childDocumentBlock.IsHead)
			{
				childDocumentBlock = childDocumentBlock.PreviousBlock;
				textPointer = childDocumentBlock.ChildContainer.End;
				pointerContext = textPointer.GetPointerContext(direction);
			}
		}
		return new DocumentSequenceTextPointer(childDocumentBlock, textPointer);
	}

	private static TextPointerContext xGapAwareGetSymbolType(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		return xGetClingDSTP(thisTp, direction).ChildPointer.GetPointerContext(direction);
	}

	private static object xGapAwareGetEmbeddedElement(DocumentSequenceTextPointer thisTp, LogicalDirection direction)
	{
		return xGetClingDSTP(thisTp, direction).ChildPointer.GetAdjacentElement(direction);
	}

	private static int xGapAwareCompareTo(DocumentSequenceTextPointer thisTp, DocumentSequenceTextPointer tp)
	{
		if (thisTp == tp)
		{
			return 0;
		}
		ChildDocumentBlock childBlock = thisTp.ChildBlock;
		ChildDocumentBlock childBlock2 = tp.ChildBlock;
		int childBlockDistance = thisTp.AggregatedContainer.GetChildBlockDistance(childBlock, childBlock2);
		if (childBlockDistance == 0)
		{
			return thisTp.ChildPointer.CompareTo(tp.ChildPointer);
		}
		if (childBlockDistance < 0)
		{
			return (!xUnseparated(tp, thisTp)) ? 1 : 0;
		}
		if (!xUnseparated(thisTp, tp))
		{
			return -1;
		}
		return 0;
	}

	private static bool xUnseparated(DocumentSequenceTextPointer tp1, DocumentSequenceTextPointer tp2)
	{
		if (tp1.ChildPointer.GetPointerContext(LogicalDirection.Forward) != 0 || tp2.ChildPointer.GetPointerContext(LogicalDirection.Backward) != 0)
		{
			return false;
		}
		for (ChildDocumentBlock nextBlock = tp1.ChildBlock.NextBlock; nextBlock != tp2.ChildBlock; nextBlock = nextBlock.NextBlock)
		{
			if (nextBlock.ChildContainer.Start.GetPointerContext(LogicalDirection.Forward) != 0)
			{
				return false;
			}
		}
		return true;
	}

	private static int xGapAwareGetDistance(DocumentSequenceTextPointer tp1, DocumentSequenceTextPointer tp2)
	{
		if (tp1 == tp2)
		{
			return 0;
		}
		int num = 0;
		DocumentSequenceTextPointer documentSequenceTextPointer = new DocumentSequenceTextPointer(tp1.ChildBlock, tp1.ChildPointer);
		while (documentSequenceTextPointer.ChildBlock != tp2.ChildBlock)
		{
			num += documentSequenceTextPointer.ChildPointer.GetOffsetToPosition(documentSequenceTextPointer.ChildPointer.TextContainer.End);
			ChildDocumentBlock childDocumentBlock = (documentSequenceTextPointer.ChildBlock = documentSequenceTextPointer.ChildBlock.NextBlock);
			documentSequenceTextPointer.ChildPointer = childDocumentBlock.ChildContainer.Start;
		}
		return num + documentSequenceTextPointer.ChildPointer.GetOffsetToPosition(tp2.ChildPointer);
	}

	private static bool xGapAwareScan(DocumentSequenceTextPointer thisTp, int distance)
	{
		ChildDocumentBlock childDocumentBlock = thisTp.ChildBlock;
		bool flag = true;
		ITextPointer textPointer = thisTp.ChildPointer;
		if (textPointer == null)
		{
			flag = false;
			textPointer = thisTp.ChildPointer.CreatePointer();
		}
		LogicalDirection logicalDirection = ((distance > 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		distance = Math.Abs(distance);
		while (distance > 0)
		{
			switch (textPointer.GetPointerContext(logicalDirection))
			{
			case TextPointerContext.ElementStart:
				textPointer.MoveToNextContextPosition(logicalDirection);
				distance--;
				break;
			case TextPointerContext.ElementEnd:
				textPointer.MoveToNextContextPosition(logicalDirection);
				distance--;
				break;
			case TextPointerContext.EmbeddedElement:
				textPointer.MoveToNextContextPosition(logicalDirection);
				distance--;
				break;
			case TextPointerContext.Text:
			{
				int textRunLength = textPointer.GetTextRunLength(logicalDirection);
				int num = ((textRunLength < distance) ? textRunLength : distance);
				distance -= num;
				if (logicalDirection == LogicalDirection.Backward)
				{
					num *= -1;
				}
				textPointer.MoveByOffset(num);
				break;
			}
			case TextPointerContext.None:
				if ((!childDocumentBlock.IsHead || logicalDirection != 0) && (!childDocumentBlock.IsTail || logicalDirection != LogicalDirection.Forward))
				{
					childDocumentBlock = ((logicalDirection == LogicalDirection.Forward) ? childDocumentBlock.NextBlock : childDocumentBlock.PreviousBlock);
					textPointer = ((logicalDirection == LogicalDirection.Forward) ? childDocumentBlock.ChildContainer.Start.CreatePointer(textPointer.LogicalDirection) : childDocumentBlock.ChildContainer.End.CreatePointer(textPointer.LogicalDirection));
					break;
				}
				return false;
			}
		}
		thisTp.ChildBlock = childDocumentBlock;
		if (flag)
		{
			thisTp.ChildPointer = textPointer;
		}
		else
		{
			thisTp.ChildPointer = textPointer.CreatePointer();
		}
		return true;
	}
}
