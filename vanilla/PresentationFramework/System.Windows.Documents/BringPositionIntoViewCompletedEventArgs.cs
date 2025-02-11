using System.ComponentModel;

namespace System.Windows.Documents;

internal class BringPositionIntoViewCompletedEventArgs : AsyncCompletedEventArgs
{
	public BringPositionIntoViewCompletedEventArgs(ITextPointer position, bool succeeded, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
	}
}
