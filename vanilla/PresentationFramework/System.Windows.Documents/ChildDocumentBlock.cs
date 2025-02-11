namespace System.Windows.Documents;

internal sealed class ChildDocumentBlock
{
	[Flags]
	internal enum BlockStatus
	{
		None = 0,
		UnloadedBlock = 1
	}

	private readonly DocumentSequenceTextContainer _aggregatedContainer;

	private readonly DocumentReference _docRef;

	private ITextContainer _container;

	private DocumentSequenceHighlightLayer _highlightLayer;

	private BlockStatus _status;

	private ChildDocumentBlock _previousBlock;

	private ChildDocumentBlock _nextBlock;

	internal DocumentSequenceTextContainer AggregatedContainer => _aggregatedContainer;

	internal ITextContainer ChildContainer
	{
		get
		{
			_EnsureBlockLoaded();
			return _container;
		}
	}

	internal DocumentSequenceHighlightLayer ChildHighlightLayer
	{
		get
		{
			if (_highlightLayer == null)
			{
				_highlightLayer = new DocumentSequenceHighlightLayer(_aggregatedContainer);
				ChildContainer.Highlights.AddLayer(_highlightLayer);
			}
			return _highlightLayer;
		}
	}

	internal DocumentReference DocRef => _docRef;

	internal ITextPointer End => ChildContainer.End;

	internal bool IsHead => _previousBlock == null;

	internal bool IsTail => _nextBlock == null;

	internal ChildDocumentBlock PreviousBlock => _previousBlock;

	internal ChildDocumentBlock NextBlock => _nextBlock;

	internal ChildDocumentBlock(DocumentSequenceTextContainer aggregatedContainer, ITextContainer childContainer)
	{
		_aggregatedContainer = aggregatedContainer;
		_container = childContainer;
	}

	internal ChildDocumentBlock(DocumentSequenceTextContainer aggregatedContainer, DocumentReference docRef)
	{
		_aggregatedContainer = aggregatedContainer;
		_docRef = docRef;
		_SetStatus(BlockStatus.UnloadedBlock);
	}

	internal ChildDocumentBlock InsertNextBlock(ChildDocumentBlock newBlock)
	{
		newBlock._nextBlock = _nextBlock;
		newBlock._previousBlock = this;
		if (_nextBlock != null)
		{
			_nextBlock._previousBlock = newBlock;
		}
		_nextBlock = newBlock;
		return newBlock;
	}

	private void _EnsureBlockLoaded()
	{
		if (_HasStatus(BlockStatus.UnloadedBlock))
		{
			_ClearStatus(BlockStatus.UnloadedBlock);
			if (_docRef.GetDocument(forceReload: false) is IServiceProvider serviceProvider && serviceProvider.GetService(typeof(ITextContainer)) is ITextContainer container)
			{
				_container = container;
			}
			if (_container == null)
			{
				_container = new NullTextContainer();
			}
		}
	}

	private bool _HasStatus(BlockStatus flags)
	{
		return (_status & flags) == flags;
	}

	private void _SetStatus(BlockStatus flags)
	{
		_status |= flags;
	}

	private void _ClearStatus(BlockStatus flags)
	{
		_status &= ~flags;
	}
}
