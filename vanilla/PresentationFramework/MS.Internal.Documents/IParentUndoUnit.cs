namespace MS.Internal.Documents;

internal interface IParentUndoUnit : IUndoUnit
{
	IUndoUnit LastUnit { get; }

	IParentUndoUnit OpenedUnit { get; }

	string Description { get; set; }

	bool Locked { get; }

	object Container { get; set; }

	void Clear();

	void Open(IParentUndoUnit newUnit);

	void Close(UndoCloseAction closeAction);

	void Close(IParentUndoUnit closingUnit, UndoCloseAction closeAction);

	void Add(IUndoUnit newUnit);

	void OnNextAdd();

	void OnNextDiscard();
}
