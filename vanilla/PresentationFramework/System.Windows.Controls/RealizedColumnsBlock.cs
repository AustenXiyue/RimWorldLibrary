namespace System.Windows.Controls;

internal struct RealizedColumnsBlock
{
	public int StartIndex { get; private set; }

	public int EndIndex { get; private set; }

	public int StartIndexOffset { get; private set; }

	public RealizedColumnsBlock(int startIndex, int endIndex, int startIndexOffset)
	{
		this = default(RealizedColumnsBlock);
		StartIndex = startIndex;
		EndIndex = endIndex;
		StartIndexOffset = startIndexOffset;
	}
}
