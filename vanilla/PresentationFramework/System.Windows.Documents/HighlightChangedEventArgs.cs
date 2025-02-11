using System.Collections;

namespace System.Windows.Documents;

internal abstract class HighlightChangedEventArgs
{
	internal abstract IList Ranges { get; }

	internal abstract Type OwnerType { get; }
}
