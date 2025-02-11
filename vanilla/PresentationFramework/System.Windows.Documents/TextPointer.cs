using MS.Internal;

namespace System.Windows.Documents;

/// <summary>Represents a position within a <see cref="T:System.Windows.Documents.FlowDocument" /> or <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
public class TextPointer : ContentPosition, ITextPointer
{
	[Flags]
	private enum Flags
	{
		EdgeMask = 0xF,
		IsFrozen = 0x10,
		IsCaretUnitBoundaryCacheValid = 0x20,
		CaretUnitBoundaryCache = 0x40
	}

	private TextContainer _tree;

	private TextTreeNode _node;

	private uint _generation;

	private uint _layoutGeneration;

	private uint _flags;

	/// <summary>Gets a value that indicates whether the text container associated with the current position has a valid (up-to-date) layout.</summary>
	/// <returns>true if the layout is current and valid; otherwise, false.</returns>
	public bool HasValidLayout
	{
		get
		{
			if (_tree.TextView != null)
			{
				if (_tree.TextView.IsValid)
				{
					return _tree.TextView.Contains(this);
				}
				return false;
			}
			return false;
		}
	}

	/// <summary>Gets the logical direction associated with the current position which is used to disambiguate content associated with the current position.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.LogicalDirection" /> value that is associated with the current position.</returns>
	public LogicalDirection LogicalDirection => GetGravityInternal();

	/// <summary>Gets the logical parent that scopes the current position.</summary>
	/// <returns>The logical parent that scopes the current position.</returns>
	public DependencyObject Parent
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			return GetLogicalTreeNode();
		}
	}

	/// <summary>Gets a value that indicates whether the current position is an insertion position.</summary>
	/// <returns>true if the current position is an insertion position; otherwise, false.</returns>
	public bool IsAtInsertionPosition
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			return TextPointerBase.IsAtInsertionPosition(this);
		}
	}

	/// <summary>Gets a value that indicates whether the current position is at the beginning of a line.</summary>
	/// <returns>true if the current position is at the beginning of a line; otherwise, false.</returns>
	public bool IsAtLineStartPosition
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			ValidateLayout();
			if (!HasValidLayout)
			{
				return false;
			}
			TextSegment lineRange = _tree.TextView.GetLineRange(this);
			if (!lineRange.IsNull)
			{
				TextPointer textPointer = new TextPointer(this);
				TextPointerContext pointerContext = textPointer.GetPointerContext(LogicalDirection.Backward);
				while ((pointerContext == TextPointerContext.ElementStart || pointerContext == TextPointerContext.ElementEnd) && TextSchema.IsFormattingType(textPointer.GetAdjacentElement(LogicalDirection.Backward).GetType()))
				{
					textPointer.MoveToNextContextPosition(LogicalDirection.Backward);
					pointerContext = textPointer.GetPointerContext(LogicalDirection.Backward);
				}
				if (textPointer.CompareTo((TextPointer)lineRange.Start) <= 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>Gets the paragraph that scopes the current position, if any.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.Paragraph" /> that scopes the current position, or null if no such paragraph exists.</returns>
	public Paragraph Paragraph
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			return ParentBlock as Paragraph;
		}
	}

	internal Block ParagraphOrBlockUIContainer
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			Block parentBlock = ParentBlock;
			if (!(parentBlock is Paragraph) && !(parentBlock is BlockUIContainer))
			{
				return null;
			}
			return parentBlock;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> at the beginning of content in the text container associated with the current position.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> at the beginning of content in the text container associated with the current position.</returns>
	public TextPointer DocumentStart => TextContainer.Start;

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> at the end of content in the text container associated with the current position.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> at the end of content in the text container associated with the current position.</returns>
	public TextPointer DocumentEnd => TextContainer.End;

	internal bool IsFrozen
	{
		get
		{
			_tree.EmptyDeadPositionList();
			return (_flags & 0x10) == 16;
		}
	}

	Type ITextPointer.ParentType
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			return Parent?.GetType();
		}
	}

	ITextContainer ITextPointer.TextContainer => TextContainer;

	bool ITextPointer.HasValidLayout => HasValidLayout;

	bool ITextPointer.IsAtCaretUnitBoundary
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			ValidateLayout();
			if (!HasValidLayout)
			{
				return false;
			}
			if (_layoutGeneration != _tree.LayoutGeneration)
			{
				IsCaretUnitBoundaryCacheValid = false;
			}
			if (!IsCaretUnitBoundaryCacheValid)
			{
				CaretUnitBoundaryCache = _tree.IsAtCaretUnitBoundary(this);
				_layoutGeneration = _tree.LayoutGeneration;
				IsCaretUnitBoundaryCacheValid = true;
			}
			return CaretUnitBoundaryCache;
		}
	}

	LogicalDirection ITextPointer.LogicalDirection => LogicalDirection;

	bool ITextPointer.IsAtInsertionPosition => IsAtInsertionPosition;

	bool ITextPointer.IsFrozen => IsFrozen;

	int ITextPointer.Offset => Offset;

	internal int Offset
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			return GetSymbolOffset() - 1;
		}
	}

	int ITextPointer.CharOffset => CharOffset;

	internal int CharOffset
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			int num;
			switch (Edge)
			{
			case ElementEdge.BeforeStart:
				num = _node.GetIMECharOffset();
				break;
			case ElementEdge.AfterStart:
				num = _node.GetIMECharOffset();
				if (_node is TextTreeTextElementNode textTreeTextElementNode)
				{
					num += textTreeTextElementNode.IMELeftEdgeCharCount;
				}
				break;
			case ElementEdge.BeforeEnd:
			case ElementEdge.AfterEnd:
				num = _node.GetIMECharOffset() + _node.IMECharCount;
				break;
			default:
				Invariant.Assert(condition: false, "Unknown value for position edge");
				num = 0;
				break;
			}
			return num;
		}
	}

	internal TextContainer TextContainer => _tree;

	internal FrameworkElement ContainingFrameworkElement => (FrameworkElement)_tree.Parent;

	internal bool IsAtRowEnd => TextPointerBase.IsAtRowEnd(this);

	internal bool HasNonMergeableInlineAncestor => GetNonMergeableInlineAncestor() != null;

	internal bool IsAtNonMergeableInlineStart => TextPointerBase.IsAtNonMergeableInlineStart(this);

	internal TextTreeNode Node => _node;

	internal ElementEdge Edge => (ElementEdge)(_flags & 0xF);

	internal Block ParentBlock
	{
		get
		{
			_tree.EmptyDeadPositionList();
			SyncToTreeGeneration();
			DependencyObject parent = Parent;
			while (parent is Inline && !(parent is AnchoredBlock))
			{
				parent = ((Inline)parent).Parent;
			}
			return parent as Block;
		}
	}

	private bool IsCaretUnitBoundaryCacheValid
	{
		get
		{
			return (_flags & 0x20) == 32;
		}
		set
		{
			_flags = (_flags & 0xFFFFFFDFu) | (uint)(value ? 32 : 0);
			VerifyFlags();
		}
	}

	private bool CaretUnitBoundaryCache
	{
		get
		{
			return (_flags & 0x40) == 64;
		}
		set
		{
			_flags = (_flags & 0xFFFFFFBFu) | (uint)(value ? 64 : 0);
			VerifyFlags();
		}
	}

	internal TextPointer(TextPointer textPointer)
	{
		if (textPointer == null)
		{
			throw new ArgumentNullException("textPointer");
		}
		InitializeOffset(textPointer, 0, textPointer.GetGravityInternal());
	}

	internal TextPointer(TextPointer position, int offset)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		InitializeOffset(position, offset, position.GetGravityInternal());
	}

	internal TextPointer(TextPointer position, LogicalDirection direction)
	{
		InitializeOffset(position, 0, direction);
	}

	internal TextPointer(TextPointer position, int offset, LogicalDirection direction)
	{
		InitializeOffset(position, offset, direction);
	}

	internal TextPointer(TextContainer textContainer, int offset, LogicalDirection direction)
	{
		if (offset < 1 || offset > textContainer.InternalSymbolCount - 1)
		{
			throw new ArgumentException(SR.BadDistance);
		}
		textContainer.GetNodeAndEdgeAtOffset(offset, out var node, out var edge);
		Initialize(textContainer, (TextTreeNode)node, edge, direction, textContainer.PositionGeneration, caretUnitBoundaryCache: false, isCaretUnitBoundaryCacheValid: false, textContainer.LayoutGeneration);
	}

	internal TextPointer(TextContainer tree, TextTreeNode node, ElementEdge edge)
	{
		Initialize(tree, node, edge, LogicalDirection.Forward, tree.PositionGeneration, caretUnitBoundaryCache: false, isCaretUnitBoundaryCacheValid: false, tree.LayoutGeneration);
	}

	internal TextPointer(TextContainer tree, TextTreeNode node, ElementEdge edge, LogicalDirection direction)
	{
		Initialize(tree, node, edge, direction, tree.PositionGeneration, caretUnitBoundaryCache: false, isCaretUnitBoundaryCacheValid: false, tree.LayoutGeneration);
	}

	internal TextPointer CreatePointer()
	{
		return new TextPointer(this);
	}

	internal TextPointer CreatePointer(LogicalDirection gravity)
	{
		return new TextPointer(this, gravity);
	}

	/// <summary>Indicates whether the specified position is in the same text container as the current position.</summary>
	/// <returns>true if <paramref name="textPosition" /> indicates a position that is in the same text container as the current position; otherwise, false.</returns>
	/// <param name="textPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> that specifies a position to compare to the current position.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textPosition" /> is null.</exception>
	public bool IsInSameDocument(TextPointer textPosition)
	{
		if (textPosition == null)
		{
			throw new ArgumentNullException("textPosition");
		}
		_tree.EmptyDeadPositionList();
		return TextContainer == textPosition.TextContainer;
	}

	/// <summary>Performs an ordinal comparison between the positions specified by the current <see cref="T:System.Windows.Documents.TextPointer" /> and a second specified <see cref="T:System.Windows.Documents.TextPointer" />.</summary>
	/// <returns>–1 if the current <see cref="T:System.Windows.Documents.TextPointer" /> precedes <paramref name="position" />; 0 if the locations are the same; +1 if the current <see cref="T:System.Windows.Documents.TextPointer" /> follows <paramref name="position" />.  </returns>
	/// <param name="position">A <see cref="T:System.Windows.Documents.TextPointer" /> that specifies a position to compare to the current position.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="position" /> specifies a position outside of the text container associated with the current position.</exception>
	public int CompareTo(TextPointer position)
	{
		_tree.EmptyDeadPositionList();
		ValidationHelper.VerifyPosition(_tree, position);
		SyncToTreeGeneration();
		position.SyncToTreeGeneration();
		int symbolOffset = GetSymbolOffset();
		int symbolOffset2 = position.GetSymbolOffset();
		if (symbolOffset < symbolOffset2)
		{
			return -1;
		}
		if (symbolOffset > symbolOffset2)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>Returns a category indicator for the content adjacent to the current <see cref="T:System.Windows.Documents.TextPointer" /> in the specified logical direction.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Documents.TextPointerContext" /> values that indicates the category for adjacent content in the specified logical direction.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to determine the category for adjacent content.</param>
	public TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		if (direction != LogicalDirection.Forward)
		{
			return GetPointerContextBackward(_node, Edge);
		}
		return GetPointerContextForward(_node, Edge);
	}

	/// <summary>Returns the number of Unicode characters between the current <see cref="T:System.Windows.Documents.TextPointer" /> and the next non-text symbol, in the specified logical direction.</summary>
	/// <returns>The number of Unicode characters between the current <see cref="T:System.Windows.Documents.TextPointer" /> and the next non-text symbol.  This number may be 0 if there is no adjacent text.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to count the number of characters.</param>
	public int GetTextRunLength(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		int num = 0;
		if (_tree.PlainTextOnly)
		{
			Invariant.Assert(GetScopingNode() is TextTreeRootNode);
			num = ((direction != LogicalDirection.Forward) ? (GetSymbolOffset() - 1) : (_tree.InternalSymbolCount - GetSymbolOffset() - 1));
		}
		else
		{
			for (TextTreeNode textTreeNode = GetAdjacentTextNodeSibling(direction); textTreeNode != null; textTreeNode = ((direction == LogicalDirection.Forward) ? textTreeNode.GetNextNode() : textTreeNode.GetPreviousNode()) as TextTreeTextNode)
			{
				num += textTreeNode.SymbolCount;
			}
		}
		return num;
	}

	/// <summary>Returns the count of symbols between the current <see cref="T:System.Windows.Documents.TextPointer" /> and a second specified <see cref="T:System.Windows.Documents.TextPointer" />.</summary>
	/// <returns>The relative number of symbols between the current <see cref="T:System.Windows.Documents.TextPointer" /> and <paramref name="position" />.  A negative value indicates that the current <see cref="T:System.Windows.Documents.TextPointer" /> follows the position specified by <paramref name="position" />, 0 indicates that the positions are equal, and a positive value indicates that the current <see cref="T:System.Windows.Documents.TextPointer" /> precedes the position specified by <paramref name="position" />.</returns>
	/// <param name="position">A <see cref="T:System.Windows.Documents.TextPointer" /> that specifies a position to find the distance (in symbols) to.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="position" /> specifies a position outside of the text container associated with the current position.</exception>
	public int GetOffsetToPosition(TextPointer position)
	{
		_tree.EmptyDeadPositionList();
		ValidationHelper.VerifyPosition(_tree, position);
		SyncToTreeGeneration();
		position.SyncToTreeGeneration();
		return position.GetSymbolOffset() - GetSymbolOffset();
	}

	/// <summary>Returns a string containing any text adjacent to the current <see cref="T:System.Windows.Documents.TextPointer" /> in the specified logical direction.</summary>
	/// <returns>A string containing any adjacent text in the specified logical direction, or <see cref="F:System.String.Empty" /> if no adjacent text can be found.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to find and return any adjacent text.</param>
	public string GetTextInRun(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return TextPointerBase.GetTextInRun(this, direction);
	}

	/// <summary>Copies the specified maximum number of characters from any adjacent text in the specified direction into a caller-supplied character array.</summary>
	/// <returns>The number of characters actually copied into <paramref name="textBuffer" />.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to find and copy any adjacent text.</param>
	/// <param name="textBuffer">A buffer into which any text is copied.</param>
	/// <param name="startIndex">An index into <paramref name="textBuffer" /> at which to begin writing copied text.</param>
	/// <param name="count">The maximum number of characters to copy.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is less than 0 or greater than the <see cref="P:System.Array.Length" /> property of <paramref name="textBuffer" />. -or-<paramref name="count" /> is less than 0 or greater than the remaining space in <paramref name="textBuffer" /> (<paramref name="textBuffer" />.<see cref="P:System.Array.Length" /> minus <paramref name="startIndex" />).</exception>
	public int GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		SyncToTreeGeneration();
		TextTreeTextNode adjacentTextNodeSibling = GetAdjacentTextNodeSibling(direction);
		if (adjacentTextNodeSibling != null)
		{
			return GetTextInRun(_tree, GetSymbolOffset(), adjacentTextNodeSibling, -1, direction, textBuffer, startIndex, count);
		}
		return 0;
	}

	/// <summary>Returns the element, if any, that borders the current <see cref="T:System.Windows.Documents.TextPointer" /> in the specified logical direction. </summary>
	/// <returns>The adjacent element in the specified <paramref name="direction" />, or null if no adjacent element exists.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to search for an adjacent element.</param>
	public DependencyObject GetAdjacentElement(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return GetAdjacentElement(_node, Edge, direction);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the position indicated by the specified offset, in symbols, from the beginning of the current <see cref="T:System.Windows.Documents.TextPointer" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the position indicated by the specified offset, or null if no corresponding position can be found.</returns>
	/// <param name="offset">An offset, in symbols, for which to calculate and return the position.  If the offset is negative, the position is calculated in the logical direction opposite of that indicated by the <see cref="P:System.Windows.Documents.TextPointer.LogicalDirection" /> property.</param>
	public TextPointer GetPositionAtOffset(int offset)
	{
		return GetPositionAtOffset(offset, LogicalDirection);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the position indicated by the specified offset, in symbols, from the beginning of the current <see cref="T:System.Windows.Documents.TextPointer" /> and in the specified direction.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the position indicated by the specified offset, or null if the offset extends past the end of the content.</returns>
	/// <param name="offset">An offset, in symbols, for which to calculate and return the position.  If the offset is negative, the returned <see cref="T:System.Windows.Documents.TextPointer" /> precedes the current <see cref="T:System.Windows.Documents.TextPointer" />; otherwise, it follows.</param>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction of the returned <see cref="T:System.Windows.Documents.TextPointer" />.</param>
	public TextPointer GetPositionAtOffset(int offset, LogicalDirection direction)
	{
		TextPointer textPointer = new TextPointer(this, direction);
		if (textPointer.MoveByOffset(offset) == offset)
		{
			textPointer.Freeze();
			return textPointer;
		}
		return null;
	}

	/// <summary>Returns a pointer to the next symbol in the specified logical direction.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the next symbol in the requested direction, or null if the current <see cref="T:System.Windows.Documents.TextPointer" /> borders the start or end of content.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to search for the next symbol.</param>
	public TextPointer GetNextContextPosition(LogicalDirection direction)
	{
		return (TextPointer)((ITextPointer)this).GetNextContextPosition(direction);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the closest insertion position in the specified logical direction.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the closest insertion position in the specified direction.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to search for the closest insertion position.</param>
	public TextPointer GetInsertionPosition(LogicalDirection direction)
	{
		return (TextPointer)((ITextPointer)this).GetInsertionPosition(direction);
	}

	internal TextPointer GetInsertionPosition()
	{
		return GetInsertionPosition(LogicalDirection.Forward);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the next insertion position in the specified logical direction.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> that identifies the next insertion position in the requested direction, or null if no next insertion position can be found.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to search for the next insertion position.</param>
	public TextPointer GetNextInsertionPosition(LogicalDirection direction)
	{
		return (TextPointer)((ITextPointer)this).GetNextInsertionPosition(direction);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the beginning of a line that is specified relative to the current <see cref="T:System.Windows.Documents.TextPointer" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> pointing to the beginning of the specified line (with the <see cref="P:System.Windows.Documents.TextPointer.LogicalDirection" /> set to <see cref="F:System.Windows.Documents.LogicalDirection.Forward" />), or null if the specified line is out of range or otherwise cannot be located.</returns>
	/// <param name="count">The number of start-of-line markers to skip when determining the line for which to return the starting position. Negative values specify preceding lines, 0 specifies the current line, and positive values specify following lines.</param>
	public TextPointer GetLineStartPosition(int count)
	{
		int actualCount;
		TextPointer lineStartPosition = GetLineStartPosition(count, out actualCount);
		if (actualCount == count)
		{
			return lineStartPosition;
		}
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the beginning of a line that is specified relative to the current <see cref="T:System.Windows.Documents.TextPointer" />, and reports how many lines were skipped.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> pointing to the beginning of the specified line (with the <see cref="P:System.Windows.Documents.TextPointer.LogicalDirection" /> set to <see cref="F:System.Windows.Documents.LogicalDirection.Forward" />), or to the beginning of the line closest to the specified line if the specified line is out of range.</returns>
	/// <param name="count">The number of start-of-line markers to skip when determining the line for which to return the starting position. Negative values specify preceding lines, 0 specifies the current line, and positive values specify following lines.</param>
	/// <param name="actualCount">When this method returns, contains the actual number of start-of-line markers that were skipped when determining the line for which to return the starting position.  This value may be less than <paramref name="count" /> if the beginning or end of content is encountered before the specified number of lines are skipped. This parameter is passed uninitialized.</param>
	public TextPointer GetLineStartPosition(int count, out int actualCount)
	{
		ValidateLayout();
		TextPointer textPointer = new TextPointer(this);
		if (HasValidLayout)
		{
			actualCount = textPointer.MoveToLineBoundary(count);
		}
		else
		{
			actualCount = 0;
		}
		textPointer.SetLogicalDirection(LogicalDirection.Forward);
		textPointer.Freeze();
		return textPointer;
	}

	/// <summary>Returns a bounding box (<see cref="T:System.Windows.Rect" />) for content that borders the current <see cref="T:System.Windows.Documents.TextPointer" /> in the specified logical direction.</summary>
	/// <returns>A bounding box for content that borders the current <see cref="T:System.Windows.Documents.TextPointer" /> in the specified direction, or <see cref="P:System.Windows.Rect.Empty" /> if current, valid layout information is unavailable.</returns>
	/// <param name="direction">One of the <see cref="T:System.Windows.Documents.LogicalDirection" /> values that specifies the logical direction in which to find a content bounding box.</param>
	public Rect GetCharacterRect(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		ValidateLayout();
		if (!HasValidLayout)
		{
			return Rect.Empty;
		}
		return TextPointerBase.GetCharacterRect(this, direction);
	}

	/// <summary>Inserts the specified text into the text <see cref="T:System.Windows.Documents.Run" /> at the current position.</summary>
	/// <param name="textData">The text to insert.</param>
	/// <exception cref="T:System.InvalidOperationException">The current position is not within a <see cref="T:System.Windows.Documents.Run" /> element.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textData" /> is null.</exception>
	public void InsertTextInRun(string textData)
	{
		if (textData == null)
		{
			throw new ArgumentNullException("textData");
		}
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		TextPointer position = ((!TextSchema.IsInTextContent(this)) ? TextRangeEditTables.EnsureInsertionPosition(this) : this);
		_tree.BeginChange();
		try
		{
			_tree.InsertTextInternal(position, textData);
		}
		finally
		{
			_tree.EndChange();
		}
	}

	/// <summary>Deletes the specified number of characters from the position indicated by the current <see cref="T:System.Windows.Documents.TextPointer" />.</summary>
	/// <returns>The number of characters actually deleted.</returns>
	/// <param name="count">The number of characters to delete, starting at the current position. Specify a positive value to delete characters that follow the current position; specify a negative value to delete characters that precede the current position.</param>
	/// <exception cref="T:System.InvalidOperationException">The method is called at a position where text is not allowed.</exception>
	public int DeleteTextInRun(int count)
	{
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		if (!TextSchema.IsInTextContent(this))
		{
			return 0;
		}
		LogicalDirection direction = ((count >= 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		int textRunLength = GetTextRunLength(direction);
		if (count > 0 && count > textRunLength)
		{
			count = textRunLength;
		}
		else if (count < 0 && count < -textRunLength)
		{
			count = -textRunLength;
		}
		TextPointer textPointer = new TextPointer(this, count);
		_tree.BeginChange();
		try
		{
			if (count > 0)
			{
				_tree.DeleteContentInternal(this, textPointer);
			}
			else if (count < 0)
			{
				_tree.DeleteContentInternal(textPointer, this);
			}
		}
		finally
		{
			_tree.EndChange();
		}
		return count;
	}

	internal void InsertTextElement(TextElement textElement)
	{
		Invariant.Assert(textElement != null);
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		ValidationHelper.ValidateChild(this, textElement, "textElement");
		if (textElement.Parent != null)
		{
			throw new InvalidOperationException(SR.TextPointer_CannotInsertTextElementBecauseItBelongsToAnotherTree);
		}
		textElement.RepositionWithContent(this);
	}

	/// <summary>Inserts a paragraph break at the current position.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the beginning (<see cref="P:System.Windows.Documents.TextElement.ContentStart" />) of the new paragraph.</returns>
	/// <exception cref="T:System.InvalidOperationException">This method is called on a position that cannot be split to accommodate a new paragraph, such as in the scope of a <see cref="T:System.Windows.Documents.Hyperlink" /> or <see cref="T:System.Windows.Documents.InlineUIContainer" />. </exception>
	public TextPointer InsertParagraphBreak()
	{
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		if (TextContainer.Parent != null)
		{
			Type type = TextContainer.Parent.GetType();
			if (!TextSchema.IsValidChildOfContainer(type, typeof(Paragraph)))
			{
				throw new InvalidOperationException(SR.Format(SR.TextSchema_IllegalElement, "Paragraph", type));
			}
		}
		Inline nonMergeableInlineAncestor = GetNonMergeableInlineAncestor();
		if (nonMergeableInlineAncestor != null)
		{
			throw new InvalidOperationException(SR.Format(SR.TextSchema_CannotSplitElement, nonMergeableInlineAncestor.GetType().Name));
		}
		_tree.BeginChange();
		try
		{
			return TextRangeEdit.InsertParagraphBreak(this, moveIntoSecondParagraph: true);
		}
		finally
		{
			_tree.EndChange();
		}
	}

	/// <summary>Inserts a line break at the current position.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> positioned immediately after the <see cref="T:System.Windows.Documents.LineBreak" /> element inserted by this method.</returns>
	public TextPointer InsertLineBreak()
	{
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		_tree.BeginChange();
		try
		{
			return TextRangeEdit.InsertLineBreak(this);
		}
		finally
		{
			_tree.EndChange();
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The string that represents the object.</returns>
	public override string ToString()
	{
		return base.ToString();
	}

	internal Inline GetNonMergeableInlineAncestor()
	{
		Inline inline = Parent as Inline;
		while (inline != null && TextSchema.IsMergeableInline(inline.GetType()))
		{
			inline = inline.Parent as Inline;
		}
		return inline;
	}

	internal ListItem GetListAncestor()
	{
		TextElement textElement = Parent as TextElement;
		while (textElement != null && !(textElement is ListItem))
		{
			textElement = textElement.Parent as TextElement;
		}
		return textElement as ListItem;
	}

	internal static int GetTextInRun(TextContainer textContainer, int symbolOffset, TextTreeTextNode textNode, int nodeOffset, LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
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
		Invariant.Assert(textNode != null, "textNode is expected to be non-null");
		textContainer.EmptyDeadPositionList();
		int num;
		if (nodeOffset < 0)
		{
			num = 0;
		}
		else
		{
			num = ((direction == LogicalDirection.Forward) ? nodeOffset : (textNode.SymbolCount - nodeOffset));
			symbolOffset += nodeOffset;
		}
		int num2 = 0;
		while (textNode != null)
		{
			num2 += Math.Min(count - num2, textNode.SymbolCount - num);
			num = 0;
			if (num2 == count)
			{
				break;
			}
			textNode = ((direction == LogicalDirection.Forward) ? textNode.GetNextNode() : textNode.GetPreviousNode()) as TextTreeTextNode;
		}
		if (direction == LogicalDirection.Backward)
		{
			symbolOffset -= num2;
		}
		if (num2 > 0)
		{
			TextTreeText.ReadText(textContainer.RootTextBlock, symbolOffset, num2, textBuffer, startIndex);
		}
		return num2;
	}

	internal static DependencyObject GetAdjacentElement(TextTreeNode node, ElementEdge edge, LogicalDirection direction)
	{
		TextTreeNode adjacentNode = GetAdjacentNode(node, edge, direction);
		if (adjacentNode is TextTreeObjectNode)
		{
			return ((TextTreeObjectNode)adjacentNode).EmbeddedElement;
		}
		if (adjacentNode is TextTreeTextElementNode)
		{
			return ((TextTreeTextElementNode)adjacentNode).TextElement;
		}
		return null;
	}

	internal void MoveToPosition(TextPointer textPosition)
	{
		ValidationHelper.VerifyPosition(_tree, textPosition);
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		textPosition.SyncToTreeGeneration();
		MoveToNode(_tree, textPosition.Node, textPosition.Edge);
	}

	internal int MoveByOffset(int offset)
	{
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		if (offset != 0)
		{
			int symbolOffset = GetSymbolOffset();
			int num = symbolOffset + offset;
			if (num < 1)
			{
				if (offset > 0)
				{
					num = _tree.InternalSymbolCount - 1;
					offset = num - symbolOffset;
				}
				else
				{
					offset += 1 - num;
					num = 1;
				}
			}
			else if (num > _tree.InternalSymbolCount - 1)
			{
				offset -= num - (_tree.InternalSymbolCount - 1);
				num = _tree.InternalSymbolCount - 1;
			}
			_tree.GetNodeAndEdgeAtOffset(num, out var node, out var edge);
			MoveToNode(_tree, (TextTreeNode)node, edge);
		}
		return offset;
	}

	internal bool MoveToNextContextPosition(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		TextTreeNode node;
		ElementEdge edge;
		bool flag = ((direction != LogicalDirection.Forward) ? GetPreviousNodeAndEdge(out node, out edge) : GetNextNodeAndEdge(out node, out edge));
		if (flag)
		{
			SetNodeAndEdge(AdjustRefCounts(node, edge, _node, Edge), edge);
			DebugAssertGeneration();
		}
		AssertState();
		return flag;
	}

	internal bool MoveToInsertionPosition(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return TextPointerBase.MoveToInsertionPosition(this, direction);
	}

	internal bool MoveToNextInsertionPosition(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return TextPointerBase.MoveToNextInsertionPosition(this, direction);
	}

	internal int MoveToLineBoundary(int count)
	{
		VerifyNotFrozen();
		ValidateLayout();
		if (!HasValidLayout)
		{
			return 0;
		}
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return TextPointerBase.MoveToLineBoundary(this, _tree.TextView, count);
	}

	internal void InsertUIElement(UIElement uiElement)
	{
		if (uiElement == null)
		{
			throw new ArgumentNullException("uiElement");
		}
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		ValidationHelper.ValidateChild(this, uiElement, "uiElement");
		if (!((TextElement)Parent).IsEmpty)
		{
			throw new InvalidOperationException(SR.TextSchema_UIElementNotAllowedInThisPosition);
		}
		_tree.BeginChange();
		try
		{
			_tree.InsertEmbeddedObjectInternal(this, uiElement);
		}
		finally
		{
			_tree.EndChange();
		}
	}

	internal TextElement GetAdjacentElementFromOuterPosition(LogicalDirection direction)
	{
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return GetAdjacentTextElementNodeSibling(direction)?.TextElement;
	}

	internal void SetLogicalDirection(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		if (GetGravityInternal() == direction)
		{
			return;
		}
		SyncToTreeGeneration();
		SplayTreeNode splayTreeNode = _node;
		ElementEdge elementEdge;
		switch (Edge)
		{
		case ElementEdge.BeforeStart:
			splayTreeNode = _node.GetPreviousNode();
			if (splayTreeNode != null)
			{
				elementEdge = ElementEdge.AfterEnd;
				break;
			}
			splayTreeNode = _node.GetContainingNode();
			Invariant.Assert(splayTreeNode != null, "Bad tree state: newNode must be non-null (BeforeStart)");
			elementEdge = ElementEdge.AfterStart;
			break;
		case ElementEdge.AfterStart:
			splayTreeNode = _node.GetFirstContainedNode();
			if (splayTreeNode != null)
			{
				elementEdge = ElementEdge.BeforeStart;
				break;
			}
			splayTreeNode = _node;
			elementEdge = ElementEdge.BeforeEnd;
			break;
		case ElementEdge.BeforeEnd:
			splayTreeNode = _node.GetLastContainedNode();
			if (splayTreeNode != null)
			{
				elementEdge = ElementEdge.AfterEnd;
				break;
			}
			splayTreeNode = _node;
			elementEdge = ElementEdge.AfterStart;
			break;
		case ElementEdge.AfterEnd:
			splayTreeNode = _node.GetNextNode();
			if (splayTreeNode != null)
			{
				elementEdge = ElementEdge.BeforeStart;
				break;
			}
			splayTreeNode = _node.GetContainingNode();
			Invariant.Assert(splayTreeNode != null, "Bad tree state: newNode must be non-null (AfterEnd)");
			elementEdge = ElementEdge.BeforeEnd;
			break;
		default:
			Invariant.Assert(condition: false, "Bad ElementEdge value");
			elementEdge = Edge;
			break;
		}
		SetNodeAndEdge(AdjustRefCounts((TextTreeNode)splayTreeNode, elementEdge, _node, Edge), elementEdge);
		Invariant.Assert(GetGravityInternal() == direction, "Inconsistent position gravity");
	}

	internal void Freeze()
	{
		_tree.EmptyDeadPositionList();
		SetIsFrozen();
	}

	internal TextPointer GetFrozenPointer(LogicalDirection logicalDirection)
	{
		ValidationHelper.VerifyDirection(logicalDirection, "logicalDirection");
		_tree.EmptyDeadPositionList();
		return (TextPointer)TextPointerBase.GetFrozenPointer(this, logicalDirection);
	}

	void ITextPointer.SetLogicalDirection(LogicalDirection direction)
	{
		SetLogicalDirection(direction);
	}

	int ITextPointer.CompareTo(ITextPointer position)
	{
		return CompareTo((TextPointer)position);
	}

	int ITextPointer.CompareTo(StaticTextPointer position)
	{
		int num = Offset + 1;
		int internalOffset = TextContainer.GetInternalOffset(position);
		if (num < internalOffset)
		{
			return -1;
		}
		if (num > internalOffset)
		{
			return 1;
		}
		return 0;
	}

	int ITextPointer.GetOffsetToPosition(ITextPointer position)
	{
		return GetOffsetToPosition((TextPointer)position);
	}

	TextPointerContext ITextPointer.GetPointerContext(LogicalDirection direction)
	{
		return GetPointerContext(direction);
	}

	int ITextPointer.GetTextRunLength(LogicalDirection direction)
	{
		return GetTextRunLength(direction);
	}

	string ITextPointer.GetTextInRun(LogicalDirection direction)
	{
		return TextPointerBase.GetTextInRun(this, direction);
	}

	int ITextPointer.GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		return GetTextInRun(direction, textBuffer, startIndex, count);
	}

	object ITextPointer.GetAdjacentElement(LogicalDirection direction)
	{
		return GetAdjacentElement(direction);
	}

	Type ITextPointer.GetElementType(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return GetElement(direction)?.GetType();
	}

	bool ITextPointer.HasEqualScope(ITextPointer position)
	{
		_tree.EmptyDeadPositionList();
		ValidationHelper.VerifyPosition(_tree, position);
		TextPointer textPointer = (TextPointer)position;
		SyncToTreeGeneration();
		textPointer.SyncToTreeGeneration();
		TextTreeNode scopingNode = GetScopingNode();
		TextTreeNode scopingNode2 = textPointer.GetScopingNode();
		return scopingNode == scopingNode2;
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

	object ITextPointer.GetValue(DependencyProperty formattingProperty)
	{
		if (formattingProperty == null)
		{
			throw new ArgumentNullException("formattingProperty");
		}
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		DependencyObject dependencyParent = GetDependencyParent();
		if (dependencyParent == null)
		{
			return DependencyProperty.UnsetValue;
		}
		return dependencyParent.GetValue(formattingProperty);
	}

	object ITextPointer.ReadLocalValue(DependencyProperty formattingProperty)
	{
		if (formattingProperty == null)
		{
			throw new ArgumentNullException("formattingProperty");
		}
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return ((Parent as TextElement) ?? throw new InvalidOperationException(SR.Format(SR.NoScopingElement, "This TextPointer"))).ReadLocalValue(formattingProperty);
	}

	LocalValueEnumerator ITextPointer.GetLocalValueEnumerator()
	{
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return (Parent as TextElement)?.GetLocalValueEnumerator() ?? new DependencyObject().GetLocalValueEnumerator();
	}

	ITextPointer ITextPointer.CreatePointer()
	{
		return ((ITextPointer)this).CreatePointer(0, LogicalDirection);
	}

	StaticTextPointer ITextPointer.CreateStaticPointer()
	{
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		return new StaticTextPointer(_tree, _node, _node.GetOffsetFromEdge(Edge));
	}

	ITextPointer ITextPointer.CreatePointer(int offset)
	{
		return ((ITextPointer)this).CreatePointer(offset, LogicalDirection);
	}

	ITextPointer ITextPointer.CreatePointer(LogicalDirection gravity)
	{
		return ((ITextPointer)this).CreatePointer(0, gravity);
	}

	ITextPointer ITextPointer.CreatePointer(int offset, LogicalDirection gravity)
	{
		return new TextPointer(this, offset, gravity);
	}

	void ITextPointer.Freeze()
	{
		Freeze();
	}

	ITextPointer ITextPointer.GetFrozenPointer(LogicalDirection logicalDirection)
	{
		return GetFrozenPointer(logicalDirection);
	}

	bool ITextPointer.MoveToNextContextPosition(LogicalDirection direction)
	{
		return MoveToNextContextPosition(direction);
	}

	int ITextPointer.MoveByOffset(int offset)
	{
		return MoveByOffset(offset);
	}

	void ITextPointer.MoveToPosition(ITextPointer position)
	{
		MoveToPosition((TextPointer)position);
	}

	void ITextPointer.MoveToElementEdge(ElementEdge edge)
	{
		MoveToElementEdge(edge);
	}

	internal void MoveToElementEdge(ElementEdge edge)
	{
		ValidationHelper.VerifyElementEdge(edge, "edge");
		VerifyNotFrozen();
		_tree.EmptyDeadPositionList();
		SyncToTreeGeneration();
		TextTreeNode scopingNode = GetScopingNode();
		if (!(scopingNode is TextTreeTextElementNode node))
		{
			if (!(scopingNode is TextTreeRootNode))
			{
				throw new InvalidOperationException(SR.Format(SR.NoScopingElement, "This TextNavigator"));
			}
		}
		else
		{
			MoveToNode(_tree, node, edge);
		}
	}

	int ITextPointer.MoveToLineBoundary(int count)
	{
		return MoveToLineBoundary(count);
	}

	Rect ITextPointer.GetCharacterRect(LogicalDirection direction)
	{
		return GetCharacterRect(direction);
	}

	bool ITextPointer.MoveToInsertionPosition(LogicalDirection direction)
	{
		return MoveToInsertionPosition(direction);
	}

	bool ITextPointer.MoveToNextInsertionPosition(LogicalDirection direction)
	{
		return MoveToNextInsertionPosition(direction);
	}

	void ITextPointer.InsertTextInRun(string textData)
	{
		InsertTextInRun(textData);
	}

	void ITextPointer.DeleteContentToPosition(ITextPointer limit)
	{
		_tree.BeginChange();
		try
		{
			TextRangeEditTables.DeleteContent(this, (TextPointer)limit);
		}
		finally
		{
			_tree.EndChange();
		}
	}

	bool ITextPointer.ValidateLayout()
	{
		return ValidateLayout();
	}

	internal bool ValidateLayout()
	{
		return TextPointerBase.ValidateLayout(this, _tree.TextView);
	}

	internal TextTreeTextNode GetAdjacentTextNodeSibling(LogicalDirection direction)
	{
		return GetAdjacentSiblingNode(direction) as TextTreeTextNode;
	}

	internal static TextTreeTextNode GetAdjacentTextNodeSibling(TextTreeNode node, ElementEdge edge, LogicalDirection direction)
	{
		return GetAdjacentSiblingNode(node, edge, direction) as TextTreeTextNode;
	}

	internal TextTreeTextElementNode GetAdjacentTextElementNodeSibling(LogicalDirection direction)
	{
		return GetAdjacentSiblingNode(direction) as TextTreeTextElementNode;
	}

	internal TextTreeTextElementNode GetAdjacentTextElementNode(LogicalDirection direction)
	{
		return GetAdjacentNode(direction) as TextTreeTextElementNode;
	}

	internal TextTreeNode GetAdjacentSiblingNode(LogicalDirection direction)
	{
		DebugAssertGeneration();
		return GetAdjacentSiblingNode(_node, Edge, direction);
	}

	internal static TextTreeNode GetAdjacentSiblingNode(TextTreeNode node, ElementEdge edge, LogicalDirection direction)
	{
		SplayTreeNode splayTreeNode = ((direction == LogicalDirection.Forward) ? (edge switch
		{
			ElementEdge.BeforeStart => node, 
			ElementEdge.AfterStart => node.GetFirstContainedNode(), 
			ElementEdge.AfterEnd => node.GetNextNode(), 
			_ => null, 
		}) : (edge switch
		{
			ElementEdge.BeforeStart => node.GetPreviousNode(), 
			ElementEdge.BeforeEnd => node.GetLastContainedNode(), 
			ElementEdge.AfterEnd => node, 
			_ => null, 
		}));
		return (TextTreeNode)splayTreeNode;
	}

	internal int GetSymbolOffset()
	{
		DebugAssertGeneration();
		return GetSymbolOffset(_tree, _node, Edge);
	}

	internal static int GetSymbolOffset(TextContainer tree, TextTreeNode node, ElementEdge edge)
	{
		switch (edge)
		{
		case ElementEdge.BeforeStart:
			return node.GetSymbolOffset(tree.Generation);
		case ElementEdge.AfterStart:
			return node.GetSymbolOffset(tree.Generation) + 1;
		case ElementEdge.BeforeEnd:
			return node.GetSymbolOffset(tree.Generation) + node.SymbolCount - 1;
		case ElementEdge.AfterEnd:
			return node.GetSymbolOffset(tree.Generation) + node.SymbolCount;
		default:
			Invariant.Assert(condition: false, "Unknown value for position edge");
			return 0;
		}
	}

	internal DependencyObject GetLogicalTreeNode()
	{
		DebugAssertGeneration();
		return GetScopingNode().GetLogicalTreeNode();
	}

	internal void SyncToTreeGeneration()
	{
		TextTreeFixupNode textTreeFixupNode = null;
		if (_generation == _tree.PositionGeneration)
		{
			return;
		}
		IsCaretUnitBoundaryCacheValid = false;
		SplayTreeNode splayTreeNode = _node;
		ElementEdge elementEdge = Edge;
		while (true)
		{
			SplayTreeNode splayTreeNode2 = splayTreeNode;
			SplayTreeNode splayTreeNode3 = splayTreeNode;
			SplayTreeNode parentNode;
			while (true)
			{
				parentNode = splayTreeNode2.ParentNode;
				if (parentNode == null)
				{
					break;
				}
				textTreeFixupNode = parentNode as TextTreeFixupNode;
				if (textTreeFixupNode != null)
				{
					break;
				}
				if (splayTreeNode2.Role == SplayTreeNodeRole.LocalRoot)
				{
					splayTreeNode3.Splay();
					splayTreeNode3 = parentNode;
				}
				splayTreeNode2 = parentNode;
			}
			if (parentNode == null)
			{
				break;
			}
			if (GetGravityInternal() == LogicalDirection.Forward)
			{
				if (elementEdge == ElementEdge.BeforeStart && textTreeFixupNode.FirstContainedNode != null)
				{
					splayTreeNode = textTreeFixupNode.FirstContainedNode;
					Invariant.Assert(elementEdge == ElementEdge.BeforeStart, "edge BeforeStart is expected");
				}
				else
				{
					splayTreeNode = textTreeFixupNode.NextNode;
					elementEdge = textTreeFixupNode.NextEdge;
				}
			}
			else if (elementEdge == ElementEdge.AfterEnd && textTreeFixupNode.LastContainedNode != null)
			{
				splayTreeNode = textTreeFixupNode.LastContainedNode;
				Invariant.Assert(elementEdge == ElementEdge.AfterEnd, "edge AfterEnd is expected");
			}
			else
			{
				splayTreeNode = textTreeFixupNode.PreviousNode;
				elementEdge = textTreeFixupNode.PreviousEdge;
			}
		}
		SetNodeAndEdge((TextTreeNode)splayTreeNode, elementEdge);
		_generation = _tree.PositionGeneration;
		AssertState();
	}

	internal TextTreeNode GetScopingNode()
	{
		return GetScopingNode(_node, Edge);
	}

	internal static TextTreeNode GetScopingNode(TextTreeNode node, ElementEdge edge)
	{
		switch (edge)
		{
		case ElementEdge.BeforeStart:
		case ElementEdge.AfterEnd:
			return (TextTreeNode)node.GetContainingNode();
		default:
			return node;
		}
	}

	internal void DebugAssertGeneration()
	{
		Invariant.Assert(_generation == _tree.PositionGeneration, "TextPointer not synchronized to tree generation!");
	}

	internal bool GetNextNodeAndEdge(out TextTreeNode node, out ElementEdge edge)
	{
		DebugAssertGeneration();
		return GetNextNodeAndEdge(_node, Edge, _tree.PlainTextOnly, out node, out edge);
	}

	internal static bool GetNextNodeAndEdge(TextTreeNode sourceNode, ElementEdge sourceEdge, bool plainTextOnly, out TextTreeNode node, out ElementEdge edge)
	{
		node = sourceNode;
		edge = sourceEdge;
		SplayTreeNode splayTreeNode = node;
		SplayTreeNode splayTreeNode2 = node;
		bool flag;
		bool flag2;
		do
		{
			flag = false;
			flag2 = false;
			switch (edge)
			{
			case ElementEdge.BeforeStart:
				splayTreeNode = splayTreeNode2.GetFirstContainedNode();
				if (splayTreeNode != null)
				{
					break;
				}
				if (splayTreeNode2 is TextTreeTextElementNode)
				{
					splayTreeNode = splayTreeNode2;
					edge = ElementEdge.BeforeEnd;
					break;
				}
				flag = splayTreeNode2 is TextTreeTextNode;
				edge = ElementEdge.BeforeEnd;
				goto case ElementEdge.BeforeEnd;
			case ElementEdge.AfterStart:
				splayTreeNode = splayTreeNode2.GetFirstContainedNode();
				if (splayTreeNode != null)
				{
					if (splayTreeNode is TextTreeTextElementNode)
					{
						edge = ElementEdge.AfterStart;
						break;
					}
					flag = splayTreeNode is TextTreeTextNode;
					flag2 = splayTreeNode.GetNextNode() is TextTreeTextNode;
					edge = ElementEdge.AfterEnd;
				}
				else if (splayTreeNode2 is TextTreeTextElementNode)
				{
					splayTreeNode = splayTreeNode2;
					edge = ElementEdge.AfterEnd;
				}
				else
				{
					Invariant.Assert(splayTreeNode2 is TextTreeRootNode, "currentNode is expected to be TextTreeRootNode");
				}
				break;
			case ElementEdge.BeforeEnd:
				splayTreeNode = splayTreeNode2.GetNextNode();
				if (splayTreeNode != null)
				{
					flag2 = splayTreeNode is TextTreeTextNode;
					edge = ElementEdge.BeforeStart;
				}
				else
				{
					splayTreeNode = splayTreeNode2.GetContainingNode();
				}
				break;
			case ElementEdge.AfterEnd:
			{
				SplayTreeNode nextNode = splayTreeNode2.GetNextNode();
				flag = nextNode is TextTreeTextNode;
				splayTreeNode = nextNode;
				if (splayTreeNode != null)
				{
					if (splayTreeNode is TextTreeTextElementNode)
					{
						edge = ElementEdge.AfterStart;
					}
					else
					{
						flag2 = splayTreeNode.GetNextNode() is TextTreeTextNode;
					}
					break;
				}
				SplayTreeNode containingNode = splayTreeNode2.GetContainingNode();
				if (!(containingNode is TextTreeRootNode))
				{
					splayTreeNode = containingNode;
				}
				break;
			}
			default:
				Invariant.Assert(condition: false, "Unknown ElementEdge value");
				break;
			}
			splayTreeNode2 = splayTreeNode;
			if (flag && flag2 && plainTextOnly)
			{
				splayTreeNode = splayTreeNode.GetContainingNode();
				Invariant.Assert(splayTreeNode is TextTreeRootNode);
				if (edge == ElementEdge.BeforeStart)
				{
					edge = ElementEdge.BeforeEnd;
					break;
				}
				splayTreeNode = splayTreeNode.GetLastContainedNode();
				Invariant.Assert(splayTreeNode != null);
				Invariant.Assert(edge == ElementEdge.AfterEnd);
				break;
			}
		}
		while (flag && flag2);
		if (splayTreeNode != null)
		{
			node = (TextTreeNode)splayTreeNode;
		}
		return splayTreeNode != null;
	}

	internal bool GetPreviousNodeAndEdge(out TextTreeNode node, out ElementEdge edge)
	{
		DebugAssertGeneration();
		return GetPreviousNodeAndEdge(_node, Edge, _tree.PlainTextOnly, out node, out edge);
	}

	internal static bool GetPreviousNodeAndEdge(TextTreeNode sourceNode, ElementEdge sourceEdge, bool plainTextOnly, out TextTreeNode node, out ElementEdge edge)
	{
		node = sourceNode;
		edge = sourceEdge;
		SplayTreeNode splayTreeNode = node;
		SplayTreeNode splayTreeNode2 = node;
		bool flag;
		bool flag2;
		do
		{
			flag = false;
			flag2 = false;
			switch (edge)
			{
			case ElementEdge.BeforeStart:
				splayTreeNode = splayTreeNode2.GetPreviousNode();
				if (splayTreeNode != null)
				{
					if (splayTreeNode is TextTreeTextElementNode)
					{
						edge = ElementEdge.BeforeEnd;
						break;
					}
					flag = splayTreeNode is TextTreeTextNode;
					flag2 = flag && splayTreeNode.GetPreviousNode() is TextTreeTextNode;
				}
				else
				{
					SplayTreeNode containingNode = splayTreeNode2.GetContainingNode();
					if (!(containingNode is TextTreeRootNode))
					{
						splayTreeNode = containingNode;
					}
				}
				break;
			case ElementEdge.AfterStart:
				splayTreeNode = splayTreeNode2.GetPreviousNode();
				if (splayTreeNode != null)
				{
					flag2 = splayTreeNode is TextTreeTextNode;
					edge = ElementEdge.AfterEnd;
				}
				else
				{
					splayTreeNode = splayTreeNode2.GetContainingNode();
				}
				break;
			case ElementEdge.BeforeEnd:
				splayTreeNode = splayTreeNode2.GetLastContainedNode();
				if (splayTreeNode != null)
				{
					if (splayTreeNode is TextTreeTextElementNode)
					{
						edge = ElementEdge.BeforeEnd;
						break;
					}
					flag = splayTreeNode is TextTreeTextNode;
					flag2 = flag && splayTreeNode.GetPreviousNode() is TextTreeTextNode;
					edge = ElementEdge.BeforeStart;
				}
				else if (splayTreeNode2 is TextTreeTextElementNode)
				{
					splayTreeNode = splayTreeNode2;
					edge = ElementEdge.BeforeStart;
				}
				else
				{
					Invariant.Assert(splayTreeNode2 is TextTreeRootNode, "currentNode is expected to be a TextTreeRootNode");
				}
				break;
			case ElementEdge.AfterEnd:
				splayTreeNode = splayTreeNode2.GetLastContainedNode();
				if (splayTreeNode != null)
				{
					break;
				}
				if (splayTreeNode2 is TextTreeTextElementNode)
				{
					splayTreeNode = splayTreeNode2;
					edge = ElementEdge.AfterStart;
					break;
				}
				flag = splayTreeNode2 is TextTreeTextNode;
				edge = ElementEdge.AfterStart;
				goto case ElementEdge.AfterStart;
			default:
				Invariant.Assert(condition: false, "Unknown ElementEdge value");
				break;
			}
			splayTreeNode2 = splayTreeNode;
			if (flag && flag2 && plainTextOnly)
			{
				splayTreeNode = splayTreeNode.GetContainingNode();
				Invariant.Assert(splayTreeNode is TextTreeRootNode);
				if (edge == ElementEdge.AfterEnd)
				{
					edge = ElementEdge.AfterStart;
					break;
				}
				splayTreeNode = splayTreeNode.GetFirstContainedNode();
				Invariant.Assert(splayTreeNode != null);
				Invariant.Assert(edge == ElementEdge.BeforeStart);
				break;
			}
		}
		while (flag && flag2);
		if (splayTreeNode != null)
		{
			node = (TextTreeNode)splayTreeNode;
		}
		return splayTreeNode != null;
	}

	internal static TextPointerContext GetPointerContextForward(TextTreeNode node, ElementEdge edge)
	{
		switch (edge)
		{
		case ElementEdge.BeforeStart:
			return node.GetPointerContext(LogicalDirection.Forward);
		case ElementEdge.AfterStart:
			if (node.ContainedNode != null)
			{
				return ((TextTreeNode)node.GetFirstContainedNode()).GetPointerContext(LogicalDirection.Forward);
			}
			goto case ElementEdge.BeforeEnd;
		case ElementEdge.BeforeEnd:
			Invariant.Assert(node.ParentNode != null || node is TextTreeRootNode, "Inconsistent node.ParentNode");
			return (node.ParentNode != null) ? TextPointerContext.ElementEnd : TextPointerContext.None;
		case ElementEdge.AfterEnd:
		{
			TextTreeNode textTreeNode = (TextTreeNode)node.GetNextNode();
			if (textTreeNode != null)
			{
				return textTreeNode.GetPointerContext(LogicalDirection.Forward);
			}
			Invariant.Assert(node.GetContainingNode() != null, "Bad position!");
			return (!(node.GetContainingNode() is TextTreeRootNode)) ? TextPointerContext.ElementEnd : TextPointerContext.None;
		}
		default:
			Invariant.Assert(condition: false, "Unreachable code.");
			return TextPointerContext.Text;
		}
	}

	internal static TextPointerContext GetPointerContextBackward(TextTreeNode node, ElementEdge edge)
	{
		switch (edge)
		{
		case ElementEdge.BeforeStart:
		{
			TextTreeNode textTreeNode2 = (TextTreeNode)node.GetPreviousNode();
			if (textTreeNode2 != null)
			{
				return textTreeNode2.GetPointerContext(LogicalDirection.Backward);
			}
			Invariant.Assert(node.GetContainingNode() != null, "Bad position!");
			return (!(node.GetContainingNode() is TextTreeRootNode)) ? TextPointerContext.ElementStart : TextPointerContext.None;
		}
		case ElementEdge.AfterStart:
			Invariant.Assert(node.ParentNode != null || node is TextTreeRootNode, "Inconsistent node.ParentNode");
			return (node.ParentNode != null) ? TextPointerContext.ElementStart : TextPointerContext.None;
		case ElementEdge.BeforeEnd:
		{
			TextTreeNode textTreeNode = (TextTreeNode)node.GetLastContainedNode();
			if (textTreeNode != null)
			{
				return textTreeNode.GetPointerContext(LogicalDirection.Backward);
			}
			goto case ElementEdge.AfterStart;
		}
		case ElementEdge.AfterEnd:
			return node.GetPointerContext(LogicalDirection.Backward);
		default:
			Invariant.Assert(condition: false, "Unknown ElementEdge value");
			return TextPointerContext.Text;
		}
	}

	internal void InsertInline(Inline inline)
	{
		TextPointer textPointer = this;
		if (!TextSchema.ValidateChild(textPointer, inline.GetType(), throwIfIllegalChild: false, throwIfIllegalHyperlinkDescendent: true))
		{
			if (textPointer.Parent == null)
			{
				throw new InvalidOperationException(SR.TextSchema_CannotInsertContentInThisPosition);
			}
			textPointer = TextRangeEditTables.EnsureInsertionPosition(this);
			Invariant.Assert(textPointer.Parent is Run, "EnsureInsertionPosition() must return a position in text content");
			Run run = (Run)textPointer.Parent;
			if (run.IsEmpty)
			{
				run.RepositionWithContent(null);
			}
			else
			{
				textPointer = TextRangeEdit.SplitFormattingElement(textPointer, keepEmptyFormatting: false);
			}
			Invariant.Assert(TextSchema.IsValidChild(textPointer, inline.GetType()));
		}
		inline.RepositionWithContent(textPointer);
	}

	internal static DependencyObject GetCommonAncestor(TextPointer position1, TextPointer position2)
	{
		TextElement textElement = position1.Parent as TextElement;
		TextElement textElement2 = position2.Parent as TextElement;
		if (textElement == null)
		{
			return position1.Parent;
		}
		if (textElement2 == null)
		{
			return position2.Parent;
		}
		return TextElement.GetCommonAncestor(textElement, textElement2);
	}

	private void InitializeOffset(TextPointer position, int distance, LogicalDirection direction)
	{
		position.SyncToTreeGeneration();
		SplayTreeNode node;
		ElementEdge edge;
		bool isCaretUnitBoundaryCacheValid;
		if (distance != 0)
		{
			int num = position.GetSymbolOffset() + distance;
			if (num < 1 || num > position.TextContainer.InternalSymbolCount - 1)
			{
				throw new ArgumentException(SR.BadDistance);
			}
			position.TextContainer.GetNodeAndEdgeAtOffset(num, out node, out edge);
			isCaretUnitBoundaryCacheValid = false;
		}
		else
		{
			node = position.Node;
			edge = position.Edge;
			isCaretUnitBoundaryCacheValid = position.IsCaretUnitBoundaryCacheValid;
		}
		Initialize(position.TextContainer, (TextTreeNode)node, edge, direction, position.TextContainer.PositionGeneration, position.CaretUnitBoundaryCache, isCaretUnitBoundaryCacheValid, position._layoutGeneration);
	}

	private void Initialize(TextContainer tree, TextTreeNode node, ElementEdge edge, LogicalDirection gravity, uint generation, bool caretUnitBoundaryCache, bool isCaretUnitBoundaryCacheValid, uint layoutGeneration)
	{
		_tree = tree;
		RepositionForGravity(ref node, ref edge, gravity);
		SetNodeAndEdge(node.IncrementReferenceCount(edge), edge);
		_generation = generation;
		CaretUnitBoundaryCache = caretUnitBoundaryCache;
		IsCaretUnitBoundaryCacheValid = isCaretUnitBoundaryCacheValid;
		_layoutGeneration = layoutGeneration;
		VerifyFlags();
		tree.AssertTree();
		AssertState();
	}

	private void VerifyNotFrozen()
	{
		if (IsFrozen)
		{
			throw new InvalidOperationException(SR.TextPositionIsFrozen);
		}
	}

	private TextTreeNode AdjustRefCounts(TextTreeNode newNode, ElementEdge newNodeEdge, TextTreeNode oldNode, ElementEdge oldNodeEdge)
	{
		Invariant.Assert(oldNode.ParentNode == null || oldNode.IsChildOfNode(oldNode.ParentNode), "Trying to add ref a dead node!");
		Invariant.Assert(newNode.ParentNode == null || newNode.IsChildOfNode(newNode.ParentNode), "Trying to add ref a dead node!");
		TextTreeNode result = newNode;
		if (newNode != oldNode || newNodeEdge != oldNodeEdge)
		{
			result = newNode.IncrementReferenceCount(newNodeEdge);
			oldNode.DecrementReferenceCount(oldNodeEdge);
		}
		return result;
	}

	private static void RepositionForGravity(ref TextTreeNode node, ref ElementEdge edge, LogicalDirection gravity)
	{
		SplayTreeNode splayTreeNode = node;
		ElementEdge elementEdge = edge;
		switch (edge)
		{
		case ElementEdge.BeforeStart:
			if (gravity == LogicalDirection.Backward)
			{
				splayTreeNode = node.GetPreviousNode();
				elementEdge = ElementEdge.AfterEnd;
				if (splayTreeNode == null)
				{
					splayTreeNode = node.GetContainingNode();
					elementEdge = ElementEdge.AfterStart;
				}
			}
			break;
		case ElementEdge.AfterStart:
			if (gravity == LogicalDirection.Forward)
			{
				splayTreeNode = node.GetFirstContainedNode();
				elementEdge = ElementEdge.BeforeStart;
				if (splayTreeNode == null)
				{
					splayTreeNode = node;
					elementEdge = ElementEdge.BeforeEnd;
				}
			}
			break;
		case ElementEdge.BeforeEnd:
			if (gravity == LogicalDirection.Backward)
			{
				splayTreeNode = node.GetLastContainedNode();
				elementEdge = ElementEdge.AfterEnd;
				if (splayTreeNode == null)
				{
					splayTreeNode = node;
					elementEdge = ElementEdge.AfterStart;
				}
			}
			break;
		case ElementEdge.AfterEnd:
			if (gravity == LogicalDirection.Forward)
			{
				splayTreeNode = node.GetNextNode();
				elementEdge = ElementEdge.BeforeStart;
				if (splayTreeNode == null)
				{
					splayTreeNode = node.GetContainingNode();
					elementEdge = ElementEdge.BeforeEnd;
				}
			}
			break;
		}
		node = (TextTreeNode)splayTreeNode;
		edge = elementEdge;
	}

	private LogicalDirection GetGravityInternal()
	{
		if (Edge != ElementEdge.BeforeStart && Edge != ElementEdge.BeforeEnd)
		{
			return LogicalDirection.Backward;
		}
		return LogicalDirection.Forward;
	}

	private DependencyObject GetDependencyParent()
	{
		DebugAssertGeneration();
		return GetScopingNode().GetDependencyParent();
	}

	internal TextTreeNode GetAdjacentNode(LogicalDirection direction)
	{
		return GetAdjacentNode(_node, Edge, direction);
	}

	internal static TextTreeNode GetAdjacentNode(TextTreeNode node, ElementEdge edge, LogicalDirection direction)
	{
		TextTreeNode textTreeNode = GetAdjacentSiblingNode(node, edge, direction);
		if (textTreeNode == null)
		{
			textTreeNode = ((edge != ElementEdge.AfterStart && edge != ElementEdge.BeforeEnd) ? ((TextTreeNode)node.GetContainingNode()) : node);
		}
		return textTreeNode;
	}

	private void MoveToNode(TextContainer tree, TextTreeNode node, ElementEdge edge)
	{
		RepositionForGravity(ref node, ref edge, GetGravityInternal());
		_tree = tree;
		SetNodeAndEdge(AdjustRefCounts(node, edge, _node, Edge), edge);
		_generation = tree.PositionGeneration;
	}

	private TextElement GetElement(LogicalDirection direction)
	{
		DebugAssertGeneration();
		return GetAdjacentTextElementNode(direction)?.TextElement;
	}

	private void AssertState()
	{
		if (Invariant.Strict)
		{
			Invariant.Assert(_node != null, "Null position node!");
			if (GetGravityInternal() == LogicalDirection.Forward)
			{
				Invariant.Assert(Edge == ElementEdge.BeforeStart || Edge == ElementEdge.BeforeEnd, "Bad position edge/gravity pair! (1)");
			}
			else
			{
				Invariant.Assert(Edge == ElementEdge.AfterStart || Edge == ElementEdge.AfterEnd, "Bad position edge/gravity pair! (2)");
			}
			if (_node is TextTreeRootNode)
			{
				Invariant.Assert(Edge != ElementEdge.BeforeStart && Edge != ElementEdge.AfterEnd, "Position at outer edge of root!");
			}
			else if (_node is TextTreeTextNode || _node is TextTreeObjectNode)
			{
				Invariant.Assert(Edge != ElementEdge.AfterStart && Edge != ElementEdge.BeforeEnd, "Position at inner leaf node edge!");
			}
			else
			{
				Invariant.Assert(_node is TextTreeTextElementNode, "Unknown node type!");
			}
			Invariant.Assert(_tree != null, "Position has no tree!");
		}
	}

	private void SetNodeAndEdge(TextTreeNode node, ElementEdge edge)
	{
		Invariant.Assert(edge == ElementEdge.BeforeStart || edge == ElementEdge.AfterStart || edge == ElementEdge.BeforeEnd || edge == ElementEdge.AfterEnd);
		_node = node;
		_flags = (_flags & 0xFFFFFFF0u) | (uint)edge;
		VerifyFlags();
		IsCaretUnitBoundaryCacheValid = false;
	}

	private void SetIsFrozen()
	{
		_flags |= 16u;
		VerifyFlags();
	}

	private void VerifyFlags()
	{
		ElementEdge elementEdge = (ElementEdge)(_flags & 0xF);
		Invariant.Assert(elementEdge == ElementEdge.BeforeStart || elementEdge == ElementEdge.AfterStart || elementEdge == ElementEdge.BeforeEnd || elementEdge == ElementEdge.AfterEnd);
	}
}
