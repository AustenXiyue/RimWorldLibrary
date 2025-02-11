using System.Windows.Data;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal class TextContainer : ITextContainer
{
	private class ExtractChangeEventArgs
	{
		private readonly TextContainer _textTree;

		private readonly TextPointer _startPosition;

		private readonly int _symbolCount;

		private readonly int _charCount;

		private readonly int _childCharCount;

		private readonly TextTreeTextElementNode _newFirstIMEVisibleNode;

		private readonly TextTreeTextElementNode _formerFirstIMEVisibleNode;

		internal TextContainer TextContainer => _textTree;

		internal int ChildIMECharCount => _childCharCount;

		internal ExtractChangeEventArgs(TextContainer textTree, TextPointer startPosition, TextTreeTextElementNode node, TextTreeTextElementNode newFirstIMEVisibleNode, TextTreeTextElementNode formerFirstIMEVisibleNode, int charCount, int childCharCount)
		{
			_textTree = textTree;
			_startPosition = startPosition;
			_symbolCount = node.SymbolCount;
			_charCount = charCount;
			_childCharCount = childCharCount;
			_newFirstIMEVisibleNode = newFirstIMEVisibleNode;
			_formerFirstIMEVisibleNode = formerFirstIMEVisibleNode;
		}

		internal void AddChange()
		{
			_textTree.AddChange(_startPosition, _symbolCount, _charCount, PrecursorTextChangeType.ContentRemoved);
			if (_newFirstIMEVisibleNode != null)
			{
				_textTree.RaiseEventForNewFirstIMEVisibleNode(_newFirstIMEVisibleNode);
			}
			if (_formerFirstIMEVisibleNode != null)
			{
				_textTree.RaiseEventForFormerFirstIMEVisibleNode(_formerFirstIMEVisibleNode);
			}
		}
	}

	[Flags]
	private enum Flags
	{
		ReadOnly = 1,
		PlainTextOnly = 2,
		CollectTextChanges = 4
	}

	private readonly DependencyObject _parent;

	private TextTreeRootNode _rootNode;

	private Highlights _highlights;

	private int _changeBlockLevel;

	private TextContainerChangedEventArgs _changes;

	private ITextView _textview;

	private UndoManager _undoManager;

	private ITextSelection _textSelection;

	private ChangeBlockUndoRecord _changeBlockUndoRecord;

	private EventHandler ChangingHandler;

	private TextContainerChangeEventHandler ChangeHandler;

	private TextContainerChangedEventHandler ChangedHandler;

	private Flags _flags;

	internal TextPointer Start
	{
		get
		{
			EmptyDeadPositionList();
			DemandCreatePositionState();
			TextPointer textPointer = new TextPointer(this, _rootNode, ElementEdge.AfterStart, LogicalDirection.Backward);
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal TextPointer End
	{
		get
		{
			EmptyDeadPositionList();
			DemandCreatePositionState();
			TextPointer textPointer = new TextPointer(this, _rootNode, ElementEdge.BeforeEnd, LogicalDirection.Forward);
			textPointer.Freeze();
			return textPointer;
		}
	}

	internal DependencyObject Parent => _parent;

	bool ITextContainer.IsReadOnly => CheckFlags(Flags.ReadOnly);

	ITextPointer ITextContainer.Start => Start;

	ITextPointer ITextContainer.End => End;

	uint ITextContainer.Generation => Generation;

	Highlights ITextContainer.Highlights => Highlights;

	DependencyObject ITextContainer.Parent => Parent;

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

	UndoManager ITextContainer.UndoManager => UndoManager;

	ITextView ITextContainer.TextView
	{
		get
		{
			return TextView;
		}
		set
		{
			TextView = value;
		}
	}

	internal ITextView TextView
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

	int ITextContainer.SymbolCount => SymbolCount;

	internal int SymbolCount => InternalSymbolCount - 2;

	internal int InternalSymbolCount
	{
		get
		{
			if (_rootNode != null)
			{
				return _rootNode.SymbolCount;
			}
			return 2;
		}
	}

	internal int IMECharCount
	{
		get
		{
			if (_rootNode != null)
			{
				return _rootNode.IMECharCount;
			}
			return 0;
		}
	}

	int ITextContainer.IMECharCount => IMECharCount;

	internal TextTreeRootTextBlock RootTextBlock
	{
		get
		{
			Invariant.Assert(_rootNode != null, "Asking for TextBlocks before root node create!");
			return _rootNode.RootTextBlock;
		}
	}

	internal uint Generation
	{
		get
		{
			Invariant.Assert(_rootNode != null, "Asking for Generation before root node create!");
			return _rootNode.Generation;
		}
	}

	internal uint PositionGeneration
	{
		get
		{
			Invariant.Assert(_rootNode != null, "Asking for PositionGeneration before root node create!");
			return _rootNode.PositionGeneration;
		}
	}

	internal uint LayoutGeneration
	{
		get
		{
			Invariant.Assert(_rootNode != null, "Asking for LayoutGeneration before root node create!");
			return _rootNode.LayoutGeneration;
		}
	}

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

	internal TextTreeRootNode RootNode => _rootNode;

	internal TextTreeNode FirstContainedNode
	{
		get
		{
			if (_rootNode != null)
			{
				return (TextTreeNode)_rootNode.GetFirstContainedNode();
			}
			return null;
		}
	}

	internal TextTreeNode LastContainedNode
	{
		get
		{
			if (_rootNode != null)
			{
				return (TextTreeNode)_rootNode.GetLastContainedNode();
			}
			return null;
		}
	}

	internal UndoManager UndoManager => _undoManager;

	internal ITextSelection TextSelection => _textSelection;

	internal bool HasListeners
	{
		get
		{
			if (ChangingHandler == null && ChangeHandler == null)
			{
				return ChangedHandler != null;
			}
			return true;
		}
	}

	internal bool PlainTextOnly => CheckFlags(Flags.PlainTextOnly);

	internal bool CollectTextChanges
	{
		get
		{
			return CheckFlags(Flags.CollectTextChanges);
		}
		set
		{
			SetFlags(value, Flags.CollectTextChanges);
		}
	}

	private Dispatcher Dispatcher
	{
		get
		{
			if (Parent == null)
			{
				return null;
			}
			return Parent.Dispatcher;
		}
	}

	event EventHandler ITextContainer.Changing
	{
		add
		{
			Changing += value;
		}
		remove
		{
			Changing -= value;
		}
	}

	event TextContainerChangeEventHandler ITextContainer.Change
	{
		add
		{
			Change += value;
		}
		remove
		{
			Change -= value;
		}
	}

	event TextContainerChangedEventHandler ITextContainer.Changed
	{
		add
		{
			Changed += value;
		}
		remove
		{
			Changed -= value;
		}
	}

	internal event EventHandler Changing
	{
		add
		{
			ChangingHandler = (EventHandler)Delegate.Combine(ChangingHandler, value);
		}
		remove
		{
			ChangingHandler = (EventHandler)Delegate.Remove(ChangingHandler, value);
		}
	}

	internal event TextContainerChangeEventHandler Change
	{
		add
		{
			ChangeHandler = (TextContainerChangeEventHandler)Delegate.Combine(ChangeHandler, value);
		}
		remove
		{
			ChangeHandler = (TextContainerChangeEventHandler)Delegate.Remove(ChangeHandler, value);
		}
	}

	internal event TextContainerChangedEventHandler Changed
	{
		add
		{
			ChangedHandler = (TextContainerChangedEventHandler)Delegate.Combine(ChangedHandler, value);
		}
		remove
		{
			ChangedHandler = (TextContainerChangedEventHandler)Delegate.Remove(ChangedHandler, value);
		}
	}

	internal TextContainer(DependencyObject parent, bool plainTextOnly)
	{
		_parent = parent;
		SetFlags(plainTextOnly, Flags.PlainTextOnly);
	}

	public override string ToString()
	{
		return base.ToString();
	}

	internal void EnableUndo(FrameworkElement uiScope)
	{
		Invariant.Assert(_undoManager == null, SR.TextContainer_UndoManagerCreatedMoreThanOnce);
		_undoManager = new UndoManager();
		UndoManager.AttachUndoManager(uiScope, _undoManager);
	}

	internal void DisableUndo(FrameworkElement uiScope)
	{
		Invariant.Assert(_undoManager != null, "UndoManager not created.");
		Invariant.Assert(_undoManager == UndoManager.GetUndoManager(uiScope));
		UndoManager.DetachUndoManager(uiScope);
		_undoManager = null;
	}

	internal void SetValue(TextPointer position, DependencyProperty property, object value)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		EmptyDeadPositionList();
		ValidateSetValue(position);
		BeginChange();
		try
		{
			TextElement obj = position.Parent as TextElement;
			Invariant.Assert(obj != null);
			obj.SetValue(property, value);
		}
		finally
		{
			EndChange();
		}
	}

	internal void SetValues(TextPointer position, LocalValueEnumerator values)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		EmptyDeadPositionList();
		ValidateSetValue(position);
		BeginChange();
		try
		{
			TextElement textElement = position.Parent as TextElement;
			Invariant.Assert(textElement != null);
			values.Reset();
			while (values.MoveNext())
			{
				LocalValueEntry current = values.Current;
				if (!(current.Property.Name == "CachedSource") && !current.Property.ReadOnly && current.Property != Run.TextProperty)
				{
					if (current.Value is BindingExpressionBase bindingExpressionBase)
					{
						textElement.SetValue(current.Property, bindingExpressionBase.Value);
					}
					else
					{
						textElement.SetValue(current.Property, current.Value);
					}
				}
			}
		}
		finally
		{
			EndChange();
		}
	}

	internal void BeginChange()
	{
		BeginChange(undo: true);
	}

	internal void BeginChangeNoUndo()
	{
		BeginChange(undo: false);
	}

	internal void EndChange()
	{
		EndChange(skipEvents: false);
	}

	internal void EndChange(bool skipEvents)
	{
		Invariant.Assert(_changeBlockLevel > 0, "Unmatched EndChange call!");
		_changeBlockLevel--;
		if (_changeBlockLevel != 0)
		{
			return;
		}
		try
		{
			_rootNode.DispatcherProcessingDisabled.Dispose();
			if (_changes != null)
			{
				TextContainerChangedEventArgs changes = _changes;
				_changes = null;
				if (ChangedHandler != null && !skipEvents)
				{
					ChangedHandler(this, changes);
				}
			}
		}
		finally
		{
			if (_changeBlockUndoRecord != null)
			{
				try
				{
					_changeBlockUndoRecord.OnEndChange();
				}
				finally
				{
					_changeBlockUndoRecord = null;
				}
			}
		}
	}

	void ITextContainer.BeginChange()
	{
		BeginChange();
	}

	void ITextContainer.BeginChangeNoUndo()
	{
		BeginChangeNoUndo();
	}

	void ITextContainer.EndChange()
	{
		EndChange(skipEvents: false);
	}

	void ITextContainer.EndChange(bool skipEvents)
	{
		EndChange(skipEvents);
	}

	ITextPointer ITextContainer.CreatePointerAtOffset(int offset, LogicalDirection direction)
	{
		return CreatePointerAtOffset(offset, direction);
	}

	internal TextPointer CreatePointerAtOffset(int offset, LogicalDirection direction)
	{
		EmptyDeadPositionList();
		DemandCreatePositionState();
		return new TextPointer(this, offset + 1, direction);
	}

	ITextPointer ITextContainer.CreatePointerAtCharOffset(int charOffset, LogicalDirection direction)
	{
		return CreatePointerAtCharOffset(charOffset, direction);
	}

	internal TextPointer CreatePointerAtCharOffset(int charOffset, LogicalDirection direction)
	{
		EmptyDeadPositionList();
		DemandCreatePositionState();
		GetNodeAndEdgeAtCharOffset(charOffset, out var node, out var edge);
		if (node != null)
		{
			return new TextPointer(this, node, edge, direction);
		}
		return null;
	}

	ITextPointer ITextContainer.CreateDynamicTextPointer(StaticTextPointer position, LogicalDirection direction)
	{
		return CreatePointerAtOffset(GetInternalOffset(position) - 1, direction);
	}

	internal StaticTextPointer CreateStaticPointerAtOffset(int offset)
	{
		EmptyDeadPositionList();
		DemandCreatePositionState();
		GetNodeAndEdgeAtOffset(offset + 1, splitNode: false, out var node, out var _);
		int handle = offset + 1 - node.GetSymbolOffset(Generation);
		return new StaticTextPointer(this, node, handle);
	}

	StaticTextPointer ITextContainer.CreateStaticPointerAtOffset(int offset)
	{
		return CreateStaticPointerAtOffset(offset);
	}

	TextPointerContext ITextContainer.GetPointerContext(StaticTextPointer pointer, LogicalDirection direction)
	{
		TextTreeNode textTreeNode = (TextTreeNode)pointer.Handle0;
		int handle = pointer.Handle1;
		if (textTreeNode is TextTreeTextNode && handle > 0 && handle < textTreeNode.SymbolCount)
		{
			return TextPointerContext.Text;
		}
		ElementEdge edgeFromOffset;
		if (direction == LogicalDirection.Forward)
		{
			edgeFromOffset = textTreeNode.GetEdgeFromOffset(handle, LogicalDirection.Forward);
			return TextPointer.GetPointerContextForward(textTreeNode, edgeFromOffset);
		}
		edgeFromOffset = textTreeNode.GetEdgeFromOffset(handle, LogicalDirection.Backward);
		return TextPointer.GetPointerContextBackward(textTreeNode, edgeFromOffset);
	}

	internal int GetInternalOffset(StaticTextPointer position)
	{
		TextTreeNode textTreeNode = (TextTreeNode)position.Handle0;
		int handle = position.Handle1;
		if (textTreeNode is TextTreeTextNode)
		{
			return textTreeNode.GetSymbolOffset(Generation) + handle;
		}
		return TextPointer.GetSymbolOffset(this, textTreeNode, textTreeNode.GetEdgeFromOffsetNoBias(handle));
	}

	int ITextContainer.GetOffsetToPosition(StaticTextPointer position1, StaticTextPointer position2)
	{
		return GetInternalOffset(position2) - GetInternalOffset(position1);
	}

	int ITextContainer.GetTextInRun(StaticTextPointer position, LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		TextTreeNode textTreeNode = (TextTreeNode)position.Handle0;
		int num = position.Handle1;
		TextTreeTextNode textTreeTextNode = textTreeNode as TextTreeTextNode;
		if (textTreeTextNode == null || num == 0 || num == textTreeNode.SymbolCount)
		{
			textTreeTextNode = TextPointer.GetAdjacentTextNodeSibling(textTreeNode, textTreeNode.GetEdgeFromOffsetNoBias(num), direction);
			num = -1;
		}
		if (textTreeTextNode != null)
		{
			return TextPointer.GetTextInRun(this, textTreeTextNode.GetSymbolOffset(Generation), textTreeTextNode, num, direction, textBuffer, startIndex, count);
		}
		return 0;
	}

	object ITextContainer.GetAdjacentElement(StaticTextPointer position, LogicalDirection direction)
	{
		TextTreeNode textTreeNode = (TextTreeNode)position.Handle0;
		int handle = position.Handle1;
		if (textTreeNode is TextTreeTextNode && handle > 0 && handle < textTreeNode.SymbolCount)
		{
			return null;
		}
		return TextPointer.GetAdjacentElement(textTreeNode, textTreeNode.GetEdgeFromOffset(handle, direction), direction);
	}

	private TextTreeNode GetScopingNode(StaticTextPointer position)
	{
		TextTreeNode textTreeNode = (TextTreeNode)position.Handle0;
		int handle = position.Handle1;
		if (textTreeNode is TextTreeTextNode && handle > 0 && handle < textTreeNode.SymbolCount)
		{
			return textTreeNode;
		}
		return TextPointer.GetScopingNode(textTreeNode, textTreeNode.GetEdgeFromOffsetNoBias(handle));
	}

	DependencyObject ITextContainer.GetParent(StaticTextPointer position)
	{
		return GetScopingNode(position).GetLogicalTreeNode();
	}

	StaticTextPointer ITextContainer.CreatePointer(StaticTextPointer position, int offset)
	{
		int num = GetInternalOffset(position) - 1;
		return ((ITextContainer)this).CreateStaticPointerAtOffset(num + offset);
	}

	StaticTextPointer ITextContainer.GetNextContextPosition(StaticTextPointer position, LogicalDirection direction)
	{
		TextTreeNode node = (TextTreeNode)position.Handle0;
		int handle = position.Handle1;
		ElementEdge edge;
		bool flag;
		if (node is TextTreeTextNode && handle > 0 && handle < node.SymbolCount)
		{
			if (PlainTextOnly)
			{
				node = (TextTreeNode)node.GetContainingNode();
				edge = ((direction == LogicalDirection.Backward) ? ElementEdge.AfterStart : ElementEdge.BeforeEnd);
			}
			else
			{
				while (((direction == LogicalDirection.Forward) ? node.GetNextNode() : node.GetPreviousNode()) is TextTreeTextNode textTreeTextNode)
				{
					node = textTreeTextNode;
				}
				edge = ((direction == LogicalDirection.Backward) ? ElementEdge.BeforeStart : ElementEdge.AfterEnd);
			}
			flag = true;
		}
		else if (direction == LogicalDirection.Forward)
		{
			edge = node.GetEdgeFromOffset(handle, LogicalDirection.Forward);
			flag = TextPointer.GetNextNodeAndEdge(node, edge, PlainTextOnly, out node, out edge);
		}
		else
		{
			edge = node.GetEdgeFromOffset(handle, LogicalDirection.Backward);
			flag = TextPointer.GetPreviousNodeAndEdge(node, edge, PlainTextOnly, out node, out edge);
		}
		if (flag)
		{
			return new StaticTextPointer(this, node, node.GetOffsetFromEdge(edge));
		}
		return StaticTextPointer.Null;
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, StaticTextPointer position2)
	{
		int internalOffset = GetInternalOffset(position1);
		int internalOffset2 = GetInternalOffset(position2);
		if (internalOffset < internalOffset2)
		{
			return -1;
		}
		if (internalOffset > internalOffset2)
		{
			return 1;
		}
		return 0;
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, ITextPointer position2)
	{
		int internalOffset = GetInternalOffset(position1);
		int num = position2.Offset + 1;
		if (internalOffset < num)
		{
			return -1;
		}
		if (internalOffset > num)
		{
			return 1;
		}
		return 0;
	}

	object ITextContainer.GetValue(StaticTextPointer position, DependencyProperty formattingProperty)
	{
		DependencyObject dependencyParent = GetScopingNode(position).GetDependencyParent();
		if (dependencyParent != null)
		{
			return dependencyParent.GetValue(formattingProperty);
		}
		return DependencyProperty.UnsetValue;
	}

	internal void BeforeAddChange()
	{
		Invariant.Assert(_changeBlockLevel > 0, "All public APIs must call BeginChange!");
		if (HasListeners)
		{
			if (ChangingHandler != null)
			{
				ChangingHandler(this, EventArgs.Empty);
			}
			if (_changes == null)
			{
				_changes = new TextContainerChangedEventArgs();
			}
		}
	}

	internal void AddChange(TextPointer startPosition, int symbolCount, int charCount, PrecursorTextChangeType textChange)
	{
		AddChange(startPosition, symbolCount, charCount, textChange, null, affectsRenderOnly: false);
	}

	internal void AddChange(TextPointer startPosition, int symbolCount, int charCount, PrecursorTextChangeType textChange, DependencyProperty property, bool affectsRenderOnly)
	{
		Invariant.Assert(textChange != PrecursorTextChangeType.ElementAdded && textChange != PrecursorTextChangeType.ElementExtracted, "Need second TextPointer for ElementAdded/Extracted operations!");
		AddChange(startPosition, null, symbolCount, 0, charCount, textChange, property, affectsRenderOnly);
	}

	internal void AddChange(TextPointer startPosition, TextPointer endPosition, int symbolCount, int leftEdgeCharCount, int childCharCount, PrecursorTextChangeType textChange, DependencyProperty property, bool affectsRenderOnly)
	{
		Invariant.Assert(_changeBlockLevel > 0, "All public APIs must call BeginChange!");
		Invariant.Assert(!CheckFlags(Flags.ReadOnly) || textChange == PrecursorTextChangeType.PropertyModified, "Illegal to modify TextContainer structure inside Change event scope!");
		if (HasListeners)
		{
			if (_changes == null)
			{
				_changes = new TextContainerChangedEventArgs();
			}
			Invariant.Assert(_changes != null, "Missing call to BeforeAddChange!");
			_changes.AddChange(textChange, startPosition.Offset, symbolCount, CollectTextChanges);
			if (ChangeHandler != null)
			{
				FireChangeEvent(startPosition, endPosition, symbolCount, leftEdgeCharCount, childCharCount, textChange, property, affectsRenderOnly);
			}
		}
	}

	internal void AddLocalValueChange()
	{
		Invariant.Assert(_changeBlockLevel > 0, "All public APIs must call BeginChange!");
		_changes.SetLocalPropertyValueChanged();
	}

	internal void InsertTextInternal(TextPointer position, object text)
	{
		Invariant.Assert(text is string || text is char[], "Unexpected type for 'text' parameter!");
		int textLength = GetTextLength(text);
		if (textLength != 0)
		{
			DemandCreateText();
			position.SyncToTreeGeneration();
			if (Invariant.Strict && position.Node.SymbolCount == 0)
			{
				Invariant.Assert(position.Node is TextTreeTextNode);
				Invariant.Assert((position.Edge == ElementEdge.AfterEnd && position.Node.GetPreviousNode() is TextTreeTextNode && position.Node.GetPreviousNode().SymbolCount > 0) || (position.Edge == ElementEdge.BeforeStart && position.Node.GetNextNode() is TextTreeTextNode && position.Node.GetNextNode().SymbolCount > 0));
			}
			BeforeAddChange();
			TextPointer startPosition = (HasListeners ? new TextPointer(position, LogicalDirection.Backward) : null);
			LogicalDirection logicalDirection = ((position.Edge != ElementEdge.BeforeStart && position.Edge != ElementEdge.BeforeEnd) ? LogicalDirection.Forward : LogicalDirection.Backward);
			TextTreeTextNode textTreeTextNode = position.GetAdjacentTextNodeSibling(logicalDirection);
			if (textTreeTextNode != null && ((logicalDirection == LogicalDirection.Backward && textTreeTextNode.AfterEndReferenceCount) || (logicalDirection == LogicalDirection.Forward && textTreeTextNode.BeforeStartReferenceCount)))
			{
				textTreeTextNode = null;
			}
			SplayTreeNode containingNode;
			if (textTreeTextNode == null)
			{
				textTreeTextNode = new TextTreeTextNode();
				textTreeTextNode.InsertAtPosition(position);
				containingNode = textTreeTextNode.GetContainingNode();
			}
			else
			{
				textTreeTextNode.Splay();
				containingNode = textTreeTextNode.ParentNode;
			}
			textTreeTextNode.SymbolCount += textLength;
			UpdateContainerSymbolCount(containingNode, textLength, textLength);
			int symbolOffset = textTreeTextNode.GetSymbolOffset(Generation);
			TextTreeText.InsertText(_rootNode.RootTextBlock, symbolOffset, text);
			TextTreeUndo.CreateInsertUndoUnit(this, symbolOffset, textLength);
			NextGeneration(deletedContent: false);
			AddChange(startPosition, textLength, textLength, PrecursorTextChangeType.ContentAdded);
			if (position.Parent is TextElement textElement)
			{
				textElement.OnTextUpdated();
			}
		}
	}

	internal void InsertElementInternal(TextPointer startPosition, TextPointer endPosition, TextElement element)
	{
		Invariant.Assert(!PlainTextOnly);
		Invariant.Assert(startPosition.TextContainer == this);
		Invariant.Assert(endPosition.TextContainer == this);
		DemandCreateText();
		startPosition.SyncToTreeGeneration();
		endPosition.SyncToTreeGeneration();
		bool flag = startPosition.CompareTo(endPosition) != 0;
		BeforeAddChange();
		char[] array;
		ExtractChangeEventArgs extractChangeEventArgs;
		TextTreeTextElementNode textTreeTextElementNode;
		bool flag4;
		int num;
		if (element.TextElementNode != null)
		{
			bool flag2 = this == element.TextContainer;
			if (!flag2)
			{
				element.TextContainer.BeginChange();
			}
			bool flag3 = true;
			try
			{
				array = element.TextContainer.ExtractElementInternal(element, deep: true, out extractChangeEventArgs);
				flag3 = false;
			}
			finally
			{
				if (flag3 && !flag2)
				{
					element.TextContainer.EndChange();
				}
			}
			textTreeTextElementNode = element.TextElementNode;
			num = extractChangeEventArgs.ChildIMECharCount;
			if (flag2)
			{
				startPosition.SyncToTreeGeneration();
				endPosition.SyncToTreeGeneration();
				extractChangeEventArgs.AddChange();
				extractChangeEventArgs = null;
			}
			flag4 = false;
		}
		else
		{
			array = null;
			textTreeTextElementNode = new TextTreeTextElementNode();
			num = 0;
			flag4 = true;
			extractChangeEventArgs = null;
		}
		DependencyObject logicalTreeNode = startPosition.GetLogicalTreeNode();
		TextElementCollectionHelper.MarkDirty(logicalTreeNode);
		if (flag4)
		{
			textTreeTextElementNode.TextElement = element;
			element.TextElementNode = textTreeTextElementNode;
		}
		TextTreeTextElementNode textTreeTextElementNode2 = null;
		int num2 = 0;
		if (flag)
		{
			textTreeTextElementNode2 = startPosition.GetAdjacentTextElementNodeSibling(LogicalDirection.Forward);
			if (textTreeTextElementNode2 != null)
			{
				num2 = -textTreeTextElementNode2.IMELeftEdgeCharCount;
				textTreeTextElementNode2.IMECharCount += num2;
			}
		}
		int num3 = InsertElementToSiblingTree(startPosition, endPosition, textTreeTextElementNode);
		num += textTreeTextElementNode.IMELeftEdgeCharCount;
		TextTreeTextElementNode textTreeTextElementNode3 = null;
		int num4 = 0;
		if (element.IsFirstIMEVisibleSibling && !flag)
		{
			textTreeTextElementNode3 = (TextTreeTextElementNode)textTreeTextElementNode.GetNextNode();
			if (textTreeTextElementNode3 != null)
			{
				num4 = textTreeTextElementNode3.IMELeftEdgeCharCount;
				textTreeTextElementNode3.IMECharCount += num4;
			}
		}
		UpdateContainerSymbolCount(textTreeTextElementNode.GetContainingNode(), (array == null) ? 2 : array.Length, num + num4 + num2);
		int symbolOffset = textTreeTextElementNode.GetSymbolOffset(Generation);
		if (flag4)
		{
			TextTreeText.InsertElementEdges(_rootNode.RootTextBlock, symbolOffset, num3);
		}
		else
		{
			TextTreeText.InsertText(_rootNode.RootTextBlock, symbolOffset, array);
		}
		NextGeneration(deletedContent: false);
		TextTreeUndo.CreateInsertElementUndoUnit(this, symbolOffset, array != null);
		if (extractChangeEventArgs != null)
		{
			extractChangeEventArgs.AddChange();
			extractChangeEventArgs.TextContainer.EndChange();
		}
		if (HasListeners)
		{
			TextPointer startPosition2 = new TextPointer(this, textTreeTextElementNode, ElementEdge.BeforeStart);
			if (num3 == 0 || array != null)
			{
				AddChange(startPosition2, (array == null) ? 2 : array.Length, num, PrecursorTextChangeType.ContentAdded);
			}
			else
			{
				TextPointer endPosition2 = new TextPointer(this, textTreeTextElementNode, ElementEdge.BeforeEnd);
				AddChange(startPosition2, endPosition2, textTreeTextElementNode.SymbolCount, textTreeTextElementNode.IMELeftEdgeCharCount, textTreeTextElementNode.IMECharCount - textTreeTextElementNode.IMELeftEdgeCharCount, PrecursorTextChangeType.ElementAdded, null, affectsRenderOnly: false);
			}
			if (num4 != 0)
			{
				RaiseEventForFormerFirstIMEVisibleNode(textTreeTextElementNode3);
			}
			if (num2 != 0)
			{
				RaiseEventForNewFirstIMEVisibleNode(textTreeTextElementNode2);
			}
		}
		element.BeforeLogicalTreeChange();
		try
		{
			LogicalTreeHelper.AddLogicalChild(logicalTreeNode, element);
		}
		finally
		{
			element.AfterLogicalTreeChange();
		}
		if (flag4)
		{
			ReparentLogicalChildren(textTreeTextElementNode, textTreeTextElementNode.TextElement, logicalTreeNode);
		}
		if (flag)
		{
			element.OnTextUpdated();
		}
	}

	internal void InsertEmbeddedObjectInternal(TextPointer position, DependencyObject embeddedObject)
	{
		Invariant.Assert(!PlainTextOnly);
		DemandCreateText();
		position.SyncToTreeGeneration();
		BeforeAddChange();
		DependencyObject logicalTreeNode = position.GetLogicalTreeNode();
		TextTreeNode textTreeNode = new TextTreeObjectNode(embeddedObject);
		textTreeNode.InsertAtPosition(position);
		UpdateContainerSymbolCount(textTreeNode.GetContainingNode(), textTreeNode.SymbolCount, textTreeNode.IMECharCount);
		int symbolOffset = textTreeNode.GetSymbolOffset(Generation);
		TextTreeText.InsertObject(_rootNode.RootTextBlock, symbolOffset);
		NextGeneration(deletedContent: false);
		TextTreeUndo.CreateInsertUndoUnit(this, symbolOffset, 1);
		LogicalTreeHelper.AddLogicalChild(logicalTreeNode, embeddedObject);
		if (HasListeners)
		{
			TextPointer startPosition = new TextPointer(this, textTreeNode, ElementEdge.BeforeStart);
			AddChange(startPosition, 1, 1, PrecursorTextChangeType.ContentAdded);
		}
	}

	internal void DeleteContentInternal(TextPointer startPosition, TextPointer endPosition)
	{
		startPosition.SyncToTreeGeneration();
		endPosition.SyncToTreeGeneration();
		if (startPosition.CompareTo(endPosition) != 0)
		{
			BeforeAddChange();
			TextTreeUndoUnit textTreeUndoUnit = TextTreeUndo.CreateDeleteContentUndoUnit(this, startPosition, endPosition);
			TextTreeNode scopingNode = startPosition.GetScopingNode();
			TextElementCollectionHelper.MarkDirty(scopingNode.GetLogicalTreeNode());
			int num = 0;
			TextTreeTextElementNode nextIMEVisibleNode = GetNextIMEVisibleNode(startPosition, endPosition);
			if (nextIMEVisibleNode != null)
			{
				num = -nextIMEVisibleNode.IMELeftEdgeCharCount;
				nextIMEVisibleNode.IMECharCount += num;
			}
			int charCount;
			int num2 = CutTopLevelLogicalNodes(scopingNode, startPosition, endPosition, out charCount);
			num2 += DeleteContentFromSiblingTree(scopingNode, startPosition, endPosition, num != 0, out var charCount2);
			charCount += charCount2;
			Invariant.Assert(num2 > 0);
			textTreeUndoUnit?.SetTreeHashCode();
			TextPointer startPosition2 = new TextPointer(startPosition, LogicalDirection.Forward);
			AddChange(startPosition2, num2, charCount, PrecursorTextChangeType.ContentRemoved);
			if (num != 0)
			{
				RaiseEventForNewFirstIMEVisibleNode(nextIMEVisibleNode);
			}
		}
	}

	internal void GetNodeAndEdgeAtOffset(int offset, out SplayTreeNode node, out ElementEdge edge)
	{
		GetNodeAndEdgeAtOffset(offset, splitNode: true, out node, out edge);
	}

	internal void GetNodeAndEdgeAtOffset(int offset, bool splitNode, out SplayTreeNode node, out ElementEdge edge)
	{
		Invariant.Assert(offset >= 1 && offset <= InternalSymbolCount - 1, "Bogus symbol offset!");
		bool flag = false;
		node = _rootNode;
		int num = 0;
		while (true)
		{
			Invariant.Assert(node.Generation != _rootNode.Generation || node.SymbolOffsetCache == -1 || node.SymbolOffsetCache == num, "Bad node offset cache!");
			node.Generation = _rootNode.Generation;
			node.SymbolOffsetCache = num;
			if (offset == num)
			{
				edge = ElementEdge.BeforeStart;
				flag = true;
				break;
			}
			if (node is TextTreeRootNode || node is TextTreeTextElementNode)
			{
				if (offset == num + 1)
				{
					edge = ElementEdge.AfterStart;
					break;
				}
				if (offset == num + node.SymbolCount - 1)
				{
					edge = ElementEdge.BeforeEnd;
					break;
				}
			}
			if (offset == num + node.SymbolCount)
			{
				edge = ElementEdge.AfterEnd;
				flag = true;
				break;
			}
			if (node.ContainedNode == null)
			{
				Invariant.Assert(node is TextTreeTextNode);
				if (splitNode)
				{
					node = ((TextTreeTextNode)node).Split(offset - num, ElementEdge.AfterEnd);
				}
				edge = ElementEdge.BeforeStart;
				break;
			}
			node = node.ContainedNode;
			num++;
			node = node.GetSiblingAtOffset(offset - num, out var nodeOffset);
			num += nodeOffset;
		}
		if (flag)
		{
			node = AdjustForZeroWidthNode(node, edge);
		}
	}

	internal void GetNodeAndEdgeAtCharOffset(int charOffset, out TextTreeNode node, out ElementEdge edge)
	{
		Invariant.Assert(charOffset >= 0 && charOffset <= IMECharCount, "Bogus char offset!");
		if (IMECharCount == 0)
		{
			node = null;
			edge = ElementEdge.BeforeStart;
			return;
		}
		bool flag = false;
		node = _rootNode;
		int num = 0;
		while (true)
		{
			int num2 = 0;
			if (node is TextTreeTextElementNode textTreeTextElementNode)
			{
				num2 = textTreeTextElementNode.IMELeftEdgeCharCount;
				if (num2 > 0)
				{
					if (charOffset == num)
					{
						edge = ElementEdge.BeforeStart;
						break;
					}
					if (charOffset == num + num2)
					{
						edge = ElementEdge.AfterStart;
						break;
					}
				}
			}
			else if (node is TextTreeTextNode || node is TextTreeObjectNode)
			{
				if (charOffset == num)
				{
					edge = ElementEdge.BeforeStart;
					flag = true;
					break;
				}
				if (charOffset == num + node.IMECharCount)
				{
					edge = ElementEdge.AfterEnd;
					flag = true;
					break;
				}
			}
			if (node.ContainedNode == null)
			{
				Invariant.Assert(node is TextTreeTextNode);
				node = ((TextTreeTextNode)node).Split(charOffset - num, ElementEdge.AfterEnd);
				edge = ElementEdge.BeforeStart;
				break;
			}
			node = (TextTreeNode)node.ContainedNode;
			num += num2;
			node = (TextTreeNode)node.GetSiblingAtCharOffset(charOffset - num, out var nodeCharOffset);
			num += nodeCharOffset;
		}
		if (flag)
		{
			node = (TextTreeNode)AdjustForZeroWidthNode(node, edge);
		}
	}

	internal void EmptyDeadPositionList()
	{
	}

	internal static int GetTextLength(object text)
	{
		Invariant.Assert(text is string || text is char[], "Bad text parameter!");
		if (!(text is string { Length: var length }))
		{
			return ((char[])text).Length;
		}
		return length;
	}

	internal void AssertTree()
	{
	}

	internal int GetContentHashCode()
	{
		return InternalSymbolCount;
	}

	internal void NextLayoutGeneration()
	{
		_rootNode.LayoutGeneration++;
	}

	internal void ExtractElementInternal(TextElement element)
	{
		ExtractElementInternal(element, deep: false, out var _);
	}

	internal bool IsAtCaretUnitBoundary(TextPointer position)
	{
		position.DebugAssertGeneration();
		Invariant.Assert(position.HasValidLayout);
		if (_rootNode.CaretUnitBoundaryCacheOffset != position.GetSymbolOffset())
		{
			_rootNode.CaretUnitBoundaryCacheOffset = position.GetSymbolOffset();
			_rootNode.CaretUnitBoundaryCache = _textview.IsAtCaretUnitBoundary(position);
			if (!_rootNode.CaretUnitBoundaryCache && position.LogicalDirection == LogicalDirection.Backward)
			{
				TextPointer positionAtOffset = position.GetPositionAtOffset(0, LogicalDirection.Forward);
				_rootNode.CaretUnitBoundaryCache = _textview.IsAtCaretUnitBoundary(positionAtOffset);
			}
		}
		return _rootNode.CaretUnitBoundaryCache;
	}

	private void ReparentLogicalChildren(SplayTreeNode containerNode, DependencyObject newParentLogicalNode, DependencyObject oldParentLogicalNode)
	{
		ReparentLogicalChildren(containerNode.GetFirstContainedNode(), null, newParentLogicalNode, oldParentLogicalNode);
	}

	private void ReparentLogicalChildren(SplayTreeNode firstChildNode, SplayTreeNode lastChildNode, DependencyObject newParentLogicalNode, DependencyObject oldParentLogicalNode)
	{
		Invariant.Assert(newParentLogicalNode != null || oldParentLogicalNode != null, "Both new and old parents should not be null");
		SplayTreeNode splayTreeNode = firstChildNode;
		while (splayTreeNode != null)
		{
			DependencyObject dependencyObject = null;
			if (splayTreeNode is TextTreeTextElementNode textTreeTextElementNode)
			{
				dependencyObject = textTreeTextElementNode.TextElement;
			}
			else if (splayTreeNode is TextTreeObjectNode textTreeObjectNode)
			{
				dependencyObject = textTreeObjectNode.EmbeddedElement;
			}
			TextElement textElement = dependencyObject as TextElement;
			textElement?.BeforeLogicalTreeChange();
			try
			{
				if (oldParentLogicalNode != null)
				{
					LogicalTreeHelper.RemoveLogicalChild(oldParentLogicalNode, dependencyObject);
				}
				if (newParentLogicalNode != null)
				{
					LogicalTreeHelper.AddLogicalChild(newParentLogicalNode, dependencyObject);
				}
			}
			finally
			{
				textElement?.AfterLogicalTreeChange();
			}
			if (splayTreeNode != lastChildNode)
			{
				splayTreeNode = splayTreeNode.GetNextNode();
				continue;
			}
			break;
		}
	}

	private SplayTreeNode AdjustForZeroWidthNode(SplayTreeNode node, ElementEdge edge)
	{
		if (!(node is TextTreeTextNode textTreeTextNode))
		{
			Invariant.Assert(node.SymbolCount > 0, "Only TextTreeTextNodes may have zero symbol counts!");
			return node;
		}
		if (textTreeTextNode.SymbolCount == 0)
		{
			SplayTreeNode nextNode = textTreeTextNode.GetNextNode();
			if (nextNode != null)
			{
				if (Invariant.Strict && nextNode.SymbolCount == 0)
				{
					Invariant.Assert(nextNode is TextTreeTextNode);
					Invariant.Assert(!textTreeTextNode.BeforeStartReferenceCount);
					Invariant.Assert(!((TextTreeTextNode)nextNode).AfterEndReferenceCount);
					Invariant.Assert(textTreeTextNode.GetPreviousNode() == null || textTreeTextNode.GetPreviousNode().SymbolCount > 0, "Found three consecutive zero-width text nodes! (1)");
					Invariant.Assert(nextNode.GetNextNode() == null || nextNode.GetNextNode().SymbolCount > 0, "Found three consecutive zero-width text nodes! (2)");
				}
				if (!textTreeTextNode.BeforeStartReferenceCount)
				{
					node = nextNode;
				}
			}
		}
		else if (edge == ElementEdge.BeforeStart)
		{
			if (textTreeTextNode.AfterEndReferenceCount)
			{
				SplayTreeNode previousNode = textTreeTextNode.GetPreviousNode();
				if (previousNode != null && previousNode.SymbolCount == 0 && !((TextTreeNode)previousNode).AfterEndReferenceCount)
				{
					Invariant.Assert(previousNode is TextTreeTextNode);
					node = previousNode;
				}
			}
		}
		else if (textTreeTextNode.BeforeStartReferenceCount)
		{
			SplayTreeNode nextNode = textTreeTextNode.GetNextNode();
			if (nextNode != null && nextNode.SymbolCount == 0 && !((TextTreeNode)nextNode).BeforeStartReferenceCount)
			{
				Invariant.Assert(nextNode is TextTreeTextNode);
				node = nextNode;
			}
		}
		return node;
	}

	private int InsertElementToSiblingTree(TextPointer startPosition, TextPointer endPosition, TextTreeTextElementNode elementNode)
	{
		int num = 0;
		int childCharCount = 0;
		if (startPosition.CompareTo(endPosition) == 0)
		{
			int num2 = elementNode.IMECharCount - elementNode.IMELeftEdgeCharCount;
			elementNode.InsertAtPosition(startPosition);
			if (elementNode.ContainedNode != null)
			{
				num = elementNode.SymbolCount - 2;
				childCharCount = num2;
			}
		}
		else
		{
			num = InsertElementToSiblingTreeComplex(startPosition, endPosition, elementNode, out childCharCount);
		}
		elementNode.SymbolCount = num + 2;
		elementNode.IMECharCount = childCharCount + elementNode.IMELeftEdgeCharCount;
		return num;
	}

	private int InsertElementToSiblingTreeComplex(TextPointer startPosition, TextPointer endPosition, TextTreeTextElementNode elementNode, out int childCharCount)
	{
		SplayTreeNode scopingNode = startPosition.GetScopingNode();
		SplayTreeNode leftSubTree;
		SplayTreeNode middleSubTree;
		SplayTreeNode rightSubTree;
		int result = CutContent(startPosition, endPosition, out childCharCount, out leftSubTree, out middleSubTree, out rightSubTree);
		SplayTreeNode.Join(elementNode, leftSubTree, rightSubTree);
		elementNode.ContainedNode = middleSubTree;
		middleSubTree.ParentNode = elementNode;
		scopingNode.ContainedNode = elementNode;
		elementNode.ParentNode = scopingNode;
		return result;
	}

	private int DeleteContentFromSiblingTree(SplayTreeNode containingNode, TextPointer startPosition, TextPointer endPosition, bool newFirstIMEVisibleNode, out int charCount)
	{
		if (startPosition.CompareTo(endPosition) == 0)
		{
			if (newFirstIMEVisibleNode)
			{
				UpdateContainerSymbolCount(containingNode, 0, -1);
			}
			charCount = 0;
			return 0;
		}
		int symbolOffset = startPosition.GetSymbolOffset();
		SplayTreeNode leftSubTree;
		SplayTreeNode middleSubTree;
		SplayTreeNode rightSubTree;
		int num = CutContent(startPosition, endPosition, out charCount, out leftSubTree, out middleSubTree, out rightSubTree);
		if (middleSubTree != null)
		{
			TextTreeNode previousNode;
			ElementEdge previousEdge;
			if (leftSubTree != null)
			{
				previousNode = (TextTreeNode)leftSubTree.GetMaxSibling();
				previousEdge = ElementEdge.AfterEnd;
			}
			else
			{
				previousNode = (TextTreeNode)containingNode;
				previousEdge = ElementEdge.AfterStart;
			}
			TextTreeNode nextNode;
			ElementEdge nextEdge;
			if (rightSubTree != null)
			{
				nextNode = (TextTreeNode)rightSubTree.GetMinSibling();
				nextEdge = ElementEdge.BeforeStart;
			}
			else
			{
				nextNode = (TextTreeNode)containingNode;
				nextEdge = ElementEdge.BeforeEnd;
			}
			AdjustRefCountsForContentDelete(ref previousNode, previousEdge, ref nextNode, nextEdge, (TextTreeNode)middleSubTree);
			leftSubTree?.Splay();
			rightSubTree?.Splay();
			middleSubTree.Splay();
			Invariant.Assert(middleSubTree.ParentNode == null, "Assigning fixup node to parented child!");
			middleSubTree.ParentNode = new TextTreeFixupNode(previousNode, previousEdge, nextNode, nextEdge);
		}
		SplayTreeNode splayTreeNode2 = (containingNode.ContainedNode = SplayTreeNode.Join(leftSubTree, rightSubTree));
		if (splayTreeNode2 != null)
		{
			splayTreeNode2.ParentNode = containingNode;
		}
		if (num > 0)
		{
			int num2 = 0;
			if (newFirstIMEVisibleNode)
			{
				num2 = -1;
			}
			UpdateContainerSymbolCount(containingNode, -num, -charCount + num2);
			TextTreeText.RemoveText(_rootNode.RootTextBlock, symbolOffset, num);
			NextGeneration(deletedContent: true);
			Invariant.Assert(startPosition.Parent == endPosition.Parent);
			if (startPosition.Parent is TextElement textElement)
			{
				textElement.OnTextUpdated();
			}
		}
		return num;
	}

	private int CutTopLevelLogicalNodes(TextTreeNode containingNode, TextPointer startPosition, TextPointer endPosition, out int charCount)
	{
		Invariant.Assert(startPosition.GetScopingNode() == endPosition.GetScopingNode(), "startPosition/endPosition not in same sibling tree!");
		SplayTreeNode splayTreeNode = startPosition.GetAdjacentSiblingNode(LogicalDirection.Forward);
		SplayTreeNode adjacentSiblingNode = endPosition.GetAdjacentSiblingNode(LogicalDirection.Forward);
		int num = 0;
		charCount = 0;
		DependencyObject logicalTreeNode = containingNode.GetLogicalTreeNode();
		while (splayTreeNode != adjacentSiblingNode)
		{
			object child = null;
			SplayTreeNode nextNode = splayTreeNode.GetNextNode();
			if (splayTreeNode is TextTreeTextElementNode { IMECharCount: var iMECharCount } textTreeTextElementNode)
			{
				char[] array = TextTreeText.CutText(_rootNode.RootTextBlock, textTreeTextElementNode.GetSymbolOffset(Generation), textTreeTextElementNode.SymbolCount);
				ExtractElementFromSiblingTree(containingNode, textTreeTextElementNode, deep: true);
				Invariant.Assert(textTreeTextElementNode.TextElement.TextElementNode != textTreeTextElementNode);
				TextTreeTextElementNode textElementNode = textTreeTextElementNode.TextElement.TextElementNode;
				UpdateContainerSymbolCount(containingNode, -textElementNode.SymbolCount, -iMECharCount);
				NextGeneration(deletedContent: true);
				TextContainer textContainer = new TextContainer(null, plainTextOnly: false);
				TextPointer start = textContainer.Start;
				textContainer.InsertElementToSiblingTree(start, start, textElementNode);
				Invariant.Assert(array.Length == textElementNode.SymbolCount);
				textContainer.UpdateContainerSymbolCount(textElementNode.GetContainingNode(), textElementNode.SymbolCount, textElementNode.IMECharCount);
				textContainer.DemandCreateText();
				TextTreeText.InsertText(textContainer.RootTextBlock, 1, array);
				textContainer.NextGeneration(deletedContent: false);
				child = textElementNode.TextElement;
				num += textElementNode.SymbolCount;
				charCount += iMECharCount;
			}
			else if (splayTreeNode is TextTreeObjectNode textTreeObjectNode)
			{
				child = textTreeObjectNode.EmbeddedElement;
			}
			LogicalTreeHelper.RemoveLogicalChild(logicalTreeNode, child);
			splayTreeNode = nextNode;
		}
		if (num > 0)
		{
			startPosition.SyncToTreeGeneration();
			endPosition.SyncToTreeGeneration();
		}
		return num;
	}

	private void AdjustRefCountsForContentDelete(ref TextTreeNode previousNode, ElementEdge previousEdge, ref TextTreeNode nextNode, ElementEdge nextEdge, TextTreeNode middleSubTree)
	{
		bool leftEdgeReferenceCount = false;
		bool rightEdgeReferenceCount = false;
		GetReferenceCounts((TextTreeNode)middleSubTree.GetMinSibling(), ref leftEdgeReferenceCount, ref rightEdgeReferenceCount);
		previousNode = previousNode.IncrementReferenceCount(previousEdge, rightEdgeReferenceCount);
		nextNode = nextNode.IncrementReferenceCount(nextEdge, leftEdgeReferenceCount);
	}

	private void GetReferenceCounts(TextTreeNode node, ref bool leftEdgeReferenceCount, ref bool rightEdgeReferenceCount)
	{
		do
		{
			leftEdgeReferenceCount |= node.BeforeStartReferenceCount || node.BeforeEndReferenceCount;
			rightEdgeReferenceCount |= node.AfterStartReferenceCount || node.AfterEndReferenceCount;
			if (node.ContainedNode != null)
			{
				GetReferenceCounts((TextTreeNode)node.ContainedNode.GetMinSibling(), ref leftEdgeReferenceCount, ref rightEdgeReferenceCount);
			}
			node = (TextTreeNode)node.GetNextNode();
		}
		while (node != null);
	}

	private void AdjustRefCountsForShallowDelete(ref TextTreeNode previousNode, ElementEdge previousEdge, ref TextTreeNode nextNode, ElementEdge nextEdge, ref TextTreeNode firstContainedNode, ref TextTreeNode lastContainedNode, TextTreeTextElementNode extractedElementNode)
	{
		previousNode = previousNode.IncrementReferenceCount(previousEdge, extractedElementNode.AfterStartReferenceCount);
		nextNode = nextNode.IncrementReferenceCount(nextEdge, extractedElementNode.BeforeEndReferenceCount);
		if (firstContainedNode != null)
		{
			firstContainedNode = firstContainedNode.IncrementReferenceCount(ElementEdge.BeforeStart, extractedElementNode.BeforeStartReferenceCount);
		}
		else
		{
			nextNode = nextNode.IncrementReferenceCount(nextEdge, extractedElementNode.BeforeStartReferenceCount);
		}
		if (lastContainedNode != null)
		{
			lastContainedNode = lastContainedNode.IncrementReferenceCount(ElementEdge.AfterEnd, extractedElementNode.AfterEndReferenceCount);
		}
		else
		{
			previousNode = previousNode.IncrementReferenceCount(previousEdge, extractedElementNode.AfterEndReferenceCount);
		}
	}

	private int CutContent(TextPointer startPosition, TextPointer endPosition, out int charCount, out SplayTreeNode leftSubTree, out SplayTreeNode middleSubTree, out SplayTreeNode rightSubTree)
	{
		Invariant.Assert(startPosition.GetScopingNode() == endPosition.GetScopingNode(), "startPosition/endPosition not in same sibling tree!");
		Invariant.Assert(startPosition.CompareTo(endPosition) != 0, "CutContent doesn't expect empty span!");
		switch (startPosition.Edge)
		{
		case ElementEdge.BeforeStart:
			leftSubTree = startPosition.Node.GetPreviousNode();
			break;
		case ElementEdge.AfterStart:
			leftSubTree = null;
			break;
		default:
			Invariant.Assert(condition: false, "Unexpected edge!");
			leftSubTree = null;
			break;
		case ElementEdge.AfterEnd:
			leftSubTree = startPosition.Node;
			break;
		}
		switch (endPosition.Edge)
		{
		case ElementEdge.BeforeStart:
			rightSubTree = endPosition.Node;
			break;
		default:
			Invariant.Assert(condition: false, "Unexpected edge! (2)");
			rightSubTree = null;
			break;
		case ElementEdge.BeforeEnd:
			rightSubTree = null;
			break;
		case ElementEdge.AfterEnd:
			rightSubTree = endPosition.Node.GetNextNode();
			break;
		}
		if (rightSubTree == null)
		{
			if (leftSubTree == null)
			{
				middleSubTree = startPosition.GetScopingNode().ContainedNode;
			}
			else
			{
				middleSubTree = leftSubTree.GetNextNode();
			}
		}
		else
		{
			middleSubTree = rightSubTree.GetPreviousNode();
			if (middleSubTree == leftSubTree)
			{
				middleSubTree = null;
			}
		}
		if (leftSubTree != null)
		{
			leftSubTree.Split();
			Invariant.Assert(leftSubTree.Role == SplayTreeNodeRole.LocalRoot);
			leftSubTree.ParentNode.ContainedNode = null;
			leftSubTree.ParentNode = null;
		}
		int num = 0;
		charCount = 0;
		if (middleSubTree != null)
		{
			if (rightSubTree != null)
			{
				middleSubTree.Split();
			}
			else
			{
				middleSubTree.Splay();
			}
			Invariant.Assert(middleSubTree.Role == SplayTreeNodeRole.LocalRoot, "middleSubTree is not a local root!");
			if (middleSubTree.ParentNode != null)
			{
				middleSubTree.ParentNode.ContainedNode = null;
				middleSubTree.ParentNode = null;
			}
			for (SplayTreeNode splayTreeNode = middleSubTree; splayTreeNode != null; splayTreeNode = splayTreeNode.RightChildNode)
			{
				num += splayTreeNode.LeftSymbolCount + splayTreeNode.SymbolCount;
				charCount += splayTreeNode.LeftCharCount + splayTreeNode.IMECharCount;
			}
		}
		if (rightSubTree != null)
		{
			rightSubTree.Splay();
		}
		Invariant.Assert(leftSubTree == null || leftSubTree.Role == SplayTreeNodeRole.LocalRoot);
		Invariant.Assert(middleSubTree == null || middleSubTree.Role == SplayTreeNodeRole.LocalRoot);
		Invariant.Assert(rightSubTree == null || rightSubTree.Role == SplayTreeNodeRole.LocalRoot);
		return num;
	}

	private char[] ExtractElementInternal(TextElement element, bool deep, out ExtractChangeEventArgs extractChangeEventArgs)
	{
		BeforeAddChange();
		SplayTreeNode splayTreeNode = null;
		SplayTreeNode lastChildNode = null;
		extractChangeEventArgs = null;
		char[] result = null;
		TextTreeTextElementNode textElementNode = element.TextElementNode;
		SplayTreeNode containingNode = textElementNode.GetContainingNode();
		bool flag = textElementNode.ContainedNode == null;
		TextPointer textPointer = new TextPointer(this, textElementNode, ElementEdge.BeforeStart, LogicalDirection.Backward);
		TextPointer textPointer2 = null;
		if (!flag)
		{
			textPointer2 = new TextPointer(this, textElementNode, ElementEdge.AfterEnd, LogicalDirection.Backward);
		}
		int symbolOffset = textElementNode.GetSymbolOffset(Generation);
		DependencyObject logicalTreeNode = ((TextTreeNode)containingNode).GetLogicalTreeNode();
		TextElementCollectionHelper.MarkDirty(logicalTreeNode);
		element.BeforeLogicalTreeChange();
		try
		{
			LogicalTreeHelper.RemoveLogicalChild(logicalTreeNode, element);
		}
		finally
		{
			element.AfterLogicalTreeChange();
		}
		TextTreeUndoUnit textTreeUndoUnit = ((!deep || flag) ? ((TextTreeUndoUnit)TextTreeUndo.CreateExtractElementUndoUnit(this, textElementNode)) : ((TextTreeUndoUnit)TextTreeUndo.CreateDeleteContentUndoUnit(this, textPointer, textPointer2)));
		if (!deep && !flag)
		{
			splayTreeNode = textElementNode.GetFirstContainedNode();
			lastChildNode = textElementNode.GetLastContainedNode();
		}
		int iMECharCount = textElementNode.IMECharCount;
		int iMELeftEdgeCharCount = textElementNode.IMELeftEdgeCharCount;
		int num = 0;
		TextTreeTextElementNode textTreeTextElementNode = null;
		if ((deep || flag) && element.IsFirstIMEVisibleSibling)
		{
			textTreeTextElementNode = (TextTreeTextElementNode)textElementNode.GetNextNode();
			if (textTreeTextElementNode != null)
			{
				num = -textTreeTextElementNode.IMELeftEdgeCharCount;
				textTreeTextElementNode.IMECharCount += num;
			}
		}
		ExtractElementFromSiblingTree(containingNode, textElementNode, deep);
		int num2 = 0;
		TextTreeTextElementNode textTreeTextElementNode2 = splayTreeNode as TextTreeTextElementNode;
		if (textTreeTextElementNode2 != null)
		{
			num2 = textTreeTextElementNode2.IMELeftEdgeCharCount;
			textTreeTextElementNode2.IMECharCount += num2;
		}
		if (!deep)
		{
			element.TextElementNode = null;
			TextTreeText.RemoveElementEdges(_rootNode.RootTextBlock, symbolOffset, textElementNode.SymbolCount);
		}
		else
		{
			result = TextTreeText.CutText(_rootNode.RootTextBlock, symbolOffset, textElementNode.SymbolCount);
		}
		if (deep)
		{
			UpdateContainerSymbolCount(containingNode, -textElementNode.SymbolCount, -iMECharCount + num + num2);
		}
		else
		{
			UpdateContainerSymbolCount(containingNode, -2, -iMELeftEdgeCharCount + num + num2);
		}
		NextGeneration(deletedContent: true);
		textTreeUndoUnit?.SetTreeHashCode();
		if (deep)
		{
			extractChangeEventArgs = new ExtractChangeEventArgs(this, textPointer, textElementNode, (num == 0) ? null : textTreeTextElementNode, (num2 == 0) ? null : textTreeTextElementNode2, iMECharCount, iMECharCount - iMELeftEdgeCharCount);
		}
		else if (flag)
		{
			AddChange(textPointer, 2, iMECharCount, PrecursorTextChangeType.ContentRemoved);
		}
		else
		{
			AddChange(textPointer, textPointer2, textElementNode.SymbolCount, iMELeftEdgeCharCount, iMECharCount - iMELeftEdgeCharCount, PrecursorTextChangeType.ElementExtracted, null, affectsRenderOnly: false);
		}
		if (extractChangeEventArgs == null)
		{
			if (num != 0)
			{
				RaiseEventForNewFirstIMEVisibleNode(textTreeTextElementNode);
			}
			if (num2 != 0)
			{
				RaiseEventForFormerFirstIMEVisibleNode(textTreeTextElementNode2);
			}
		}
		if (!deep && !flag)
		{
			ReparentLogicalChildren(splayTreeNode, lastChildNode, logicalTreeNode, element);
		}
		if (element.TextElementNode != null)
		{
			element.TextElementNode.IMECharCount -= iMELeftEdgeCharCount;
		}
		return result;
	}

	private void ExtractElementFromSiblingTree(SplayTreeNode containingNode, TextTreeTextElementNode elementNode, bool deep)
	{
		TextTreeNode previousNode = (TextTreeNode)elementNode.GetPreviousNode();
		ElementEdge previousEdge = ElementEdge.AfterEnd;
		if (previousNode == null)
		{
			previousNode = (TextTreeNode)containingNode;
			previousEdge = ElementEdge.AfterStart;
		}
		TextTreeNode nextNode = (TextTreeNode)elementNode.GetNextNode();
		ElementEdge nextEdge = ElementEdge.BeforeStart;
		if (nextNode == null)
		{
			nextNode = (TextTreeNode)containingNode;
			nextEdge = ElementEdge.BeforeEnd;
		}
		elementNode.Remove();
		Invariant.Assert(elementNode.Role == SplayTreeNodeRole.LocalRoot);
		if (deep)
		{
			AdjustRefCountsForContentDelete(ref previousNode, previousEdge, ref nextNode, nextEdge, elementNode);
			elementNode.ParentNode = new TextTreeFixupNode(previousNode, previousEdge, nextNode, nextEdge);
			DeepCopy(elementNode);
			return;
		}
		SplayTreeNode containedNode = elementNode.ContainedNode;
		elementNode.ContainedNode = null;
		TextTreeNode firstContainedNode;
		TextTreeNode lastContainedNode;
		if (containedNode != null)
		{
			containedNode.ParentNode = null;
			firstContainedNode = (TextTreeNode)containedNode.GetMinSibling();
			lastContainedNode = (TextTreeNode)containedNode.GetMaxSibling();
		}
		else
		{
			firstContainedNode = null;
			lastContainedNode = null;
		}
		AdjustRefCountsForShallowDelete(ref previousNode, previousEdge, ref nextNode, nextEdge, ref firstContainedNode, ref lastContainedNode, elementNode);
		elementNode.ParentNode = new TextTreeFixupNode(previousNode, previousEdge, nextNode, nextEdge, firstContainedNode, lastContainedNode);
		if (containedNode != null)
		{
			containedNode.Splay();
			SplayTreeNode splayTreeNode = containedNode;
			if (previousNode != containingNode)
			{
				previousNode.Split();
				Invariant.Assert(previousNode.Role == SplayTreeNodeRole.LocalRoot);
				Invariant.Assert(previousNode.RightChildNode == null);
				SplayTreeNode minSibling = containedNode.GetMinSibling();
				minSibling.Splay();
				previousNode.RightChildNode = minSibling;
				minSibling.ParentNode = previousNode;
				splayTreeNode = previousNode;
			}
			if (nextNode != containingNode)
			{
				nextNode.Splay();
				Invariant.Assert(nextNode.Role == SplayTreeNodeRole.LocalRoot);
				Invariant.Assert(nextNode.LeftChildNode == null);
				SplayTreeNode maxSibling = containedNode.GetMaxSibling();
				maxSibling.Splay();
				nextNode.LeftChildNode = maxSibling;
				nextNode.LeftSymbolCount += maxSibling.LeftSymbolCount + maxSibling.SymbolCount;
				nextNode.LeftCharCount += maxSibling.LeftCharCount + maxSibling.IMECharCount;
				maxSibling.ParentNode = nextNode;
				splayTreeNode = nextNode;
			}
			containingNode.ContainedNode = splayTreeNode;
			if (splayTreeNode != null)
			{
				splayTreeNode.ParentNode = containingNode;
			}
		}
	}

	private TextTreeTextElementNode DeepCopy(TextTreeTextElementNode elementNode)
	{
		TextTreeTextElementNode textTreeTextElementNode = (TextTreeTextElementNode)elementNode.Clone();
		elementNode.TextElement.TextElementNode = textTreeTextElementNode;
		if (elementNode.ContainedNode != null)
		{
			textTreeTextElementNode.ContainedNode = DeepCopyContainedNodes((TextTreeNode)elementNode.ContainedNode.GetMinSibling());
			textTreeTextElementNode.ContainedNode.ParentNode = textTreeTextElementNode;
		}
		return textTreeTextElementNode;
	}

	private TextTreeNode DeepCopyContainedNodes(TextTreeNode node)
	{
		TextTreeNode result = null;
		TextTreeNode textTreeNode = null;
		do
		{
			TextTreeNode textTreeNode2 = ((!(node is TextTreeTextElementNode elementNode)) ? node.Clone() : DeepCopy(elementNode));
			Invariant.Assert(textTreeNode2 != null || (node is TextTreeTextNode && node.SymbolCount == 0));
			if (textTreeNode2 != null)
			{
				textTreeNode2.ParentNode = textTreeNode;
				if (textTreeNode != null)
				{
					textTreeNode.RightChildNode = textTreeNode2;
				}
				else
				{
					Invariant.Assert(textTreeNode2.Role == SplayTreeNodeRole.LocalRoot);
					result = textTreeNode2;
				}
				textTreeNode = textTreeNode2;
			}
			node = (TextTreeNode)node.GetNextNode();
		}
		while (node != null);
		return result;
	}

	private void DemandCreatePositionState()
	{
		if (_rootNode == null)
		{
			_rootNode = new TextTreeRootNode(this);
		}
	}

	private void DemandCreateText()
	{
		Invariant.Assert(_rootNode != null, "Unexpected DemandCreateText call before position allocation.");
		if (_rootNode.RootTextBlock == null)
		{
			_rootNode.RootTextBlock = new TextTreeRootTextBlock();
			TextTreeText.InsertElementEdges(_rootNode.RootTextBlock, 0, 0);
		}
	}

	private void UpdateContainerSymbolCount(SplayTreeNode containingNode, int symbolCount, int charCount)
	{
		do
		{
			containingNode.Splay();
			containingNode.SymbolCount += symbolCount;
			containingNode.IMECharCount += charCount;
			containingNode = containingNode.ParentNode;
		}
		while (containingNode != null);
	}

	private void NextGeneration(bool deletedContent)
	{
		AssertTree();
		AssertTreeAndTextSize();
		_rootNode.Generation++;
		if (deletedContent)
		{
			_rootNode.PositionGeneration++;
		}
		NextLayoutGeneration();
	}

	private DependencyProperty[] LocalValueEnumeratorToArray(LocalValueEnumerator valuesEnumerator)
	{
		DependencyProperty[] array = new DependencyProperty[valuesEnumerator.Count];
		int num = 0;
		valuesEnumerator.Reset();
		while (valuesEnumerator.MoveNext())
		{
			array[num++] = valuesEnumerator.Current.Property;
		}
		return array;
	}

	private void ValidateSetValue(TextPointer position)
	{
		if (position.TextContainer != this)
		{
			throw new InvalidOperationException(SR.Format(SR.NotInThisTree, "position"));
		}
		position.SyncToTreeGeneration();
		if (!(position.Parent is TextElement))
		{
			throw new InvalidOperationException(SR.NoElement);
		}
	}

	private void AssertTreeAndTextSize()
	{
		if (Invariant.Strict && _rootNode.RootTextBlock != null)
		{
			int num = 0;
			for (TextTreeTextBlock textTreeTextBlock = (TextTreeTextBlock)_rootNode.RootTextBlock.ContainedNode.GetMinSibling(); textTreeTextBlock != null; textTreeTextBlock = (TextTreeTextBlock)textTreeTextBlock.GetNextNode())
			{
				Invariant.Assert(textTreeTextBlock.Count > 0, "Empty TextBlock!");
				num += textTreeTextBlock.Count;
			}
			Invariant.Assert(num == InternalSymbolCount, "TextContainer.SymbolCount does not match TextTreeText size!");
		}
	}

	private void BeginChange(bool undo)
	{
		if (undo && _changeBlockUndoRecord == null && _changeBlockLevel == 0)
		{
			Invariant.Assert(_changeBlockLevel == 0);
			_changeBlockUndoRecord = new ChangeBlockUndoRecord(this, string.Empty);
		}
		if (_changeBlockLevel == 0)
		{
			DemandCreatePositionState();
			if (Dispatcher != null)
			{
				_rootNode.DispatcherProcessingDisabled = Dispatcher.DisableProcessing();
			}
		}
		_changeBlockLevel++;
	}

	private void FireChangeEvent(TextPointer startPosition, TextPointer endPosition, int symbolCount, int leftEdgeCharCount, int childCharCount, PrecursorTextChangeType precursorTextChange, DependencyProperty property, bool affectsRenderOnly)
	{
		Invariant.Assert(ChangeHandler != null);
		SetFlags(value: true, Flags.ReadOnly);
		try
		{
			switch (precursorTextChange)
			{
			case PrecursorTextChangeType.ElementAdded:
			{
				Invariant.Assert(symbolCount > 2, "ElementAdded must span at least two element edges and one content symbol!");
				TextContainerChangeEventArgs e4 = new TextContainerChangeEventArgs(startPosition, 1, leftEdgeCharCount, TextChangeType.ContentAdded);
				TextContainerChangeEventArgs e5 = new TextContainerChangeEventArgs(endPosition, 1, 0, TextChangeType.ContentAdded);
				ChangeHandler(this, e4);
				ChangeHandler(this, e5);
				break;
			}
			case PrecursorTextChangeType.ElementExtracted:
			{
				Invariant.Assert(symbolCount > 2, "ElementExtracted must span at least two element edges and one content symbol!");
				TextContainerChangeEventArgs e2 = new TextContainerChangeEventArgs(startPosition, 1, leftEdgeCharCount, TextChangeType.ContentRemoved);
				TextContainerChangeEventArgs e3 = new TextContainerChangeEventArgs(endPosition, 1, 0, TextChangeType.ContentRemoved);
				ChangeHandler(this, e2);
				ChangeHandler(this, e3);
				break;
			}
			default:
			{
				TextContainerChangeEventArgs e = new TextContainerChangeEventArgs(startPosition, symbolCount, leftEdgeCharCount + childCharCount, ConvertSimplePrecursorChangeToTextChange(precursorTextChange), property, affectsRenderOnly);
				ChangeHandler(this, e);
				break;
			}
			}
		}
		finally
		{
			SetFlags(value: false, Flags.ReadOnly);
		}
	}

	private TextChangeType ConvertSimplePrecursorChangeToTextChange(PrecursorTextChangeType precursorTextChange)
	{
		Invariant.Assert(precursorTextChange != PrecursorTextChangeType.ElementAdded && precursorTextChange != PrecursorTextChangeType.ElementExtracted);
		return (TextChangeType)precursorTextChange;
	}

	private TextTreeTextElementNode GetNextIMEVisibleNode(TextPointer startPosition, TextPointer endPosition)
	{
		TextTreeTextElementNode result = null;
		if (startPosition.GetAdjacentElement(LogicalDirection.Forward) is TextElement { IsFirstIMEVisibleSibling: not false })
		{
			result = (TextTreeTextElementNode)endPosition.GetAdjacentSiblingNode(LogicalDirection.Forward);
		}
		return result;
	}

	private void RaiseEventForFormerFirstIMEVisibleNode(TextTreeNode node)
	{
		TextPointer startPosition = new TextPointer(this, node, ElementEdge.BeforeStart);
		AddChange(startPosition, 0, 1, PrecursorTextChangeType.ContentAdded);
	}

	private void RaiseEventForNewFirstIMEVisibleNode(TextTreeNode node)
	{
		TextPointer startPosition = new TextPointer(this, node, ElementEdge.BeforeStart);
		AddChange(startPosition, 0, 1, PrecursorTextChangeType.ContentRemoved);
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}
}
