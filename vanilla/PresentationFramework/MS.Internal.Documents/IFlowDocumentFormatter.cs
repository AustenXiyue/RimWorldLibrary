using System.Windows.Documents;

namespace MS.Internal.Documents;

internal interface IFlowDocumentFormatter
{
	bool IsLayoutDataValid { get; }

	void OnContentInvalidated(bool affectsLayout);

	void OnContentInvalidated(bool affectsLayout, ITextPointer start, ITextPointer end);

	void Suspend();
}
