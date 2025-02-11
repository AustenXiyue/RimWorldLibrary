using System;

namespace MS.Internal.Documents;

internal class RowLayoutCompletedEventArgs : EventArgs
{
	private readonly int _pivotRowIndex;

	public int PivotRowIndex => _pivotRowIndex;

	public RowLayoutCompletedEventArgs(int pivotRowIndex)
	{
		_pivotRowIndex = pivotRowIndex;
	}
}
