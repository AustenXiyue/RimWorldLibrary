namespace MS.Internal.Documents;

internal interface IUndoUnit
{
	void Do();

	bool Merge(IUndoUnit unit);
}
