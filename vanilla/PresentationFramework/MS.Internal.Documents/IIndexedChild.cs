using System.Windows.Documents;

namespace MS.Internal.Documents;

internal interface IIndexedChild<TParent> where TParent : TextElement
{
	int Index { get; set; }

	void OnEnterParentTree();

	void OnExitParentTree();

	void OnAfterExitParentTree(TParent parent);
}
