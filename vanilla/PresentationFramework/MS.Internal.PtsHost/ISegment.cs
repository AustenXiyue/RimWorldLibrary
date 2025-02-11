namespace MS.Internal.PtsHost;

internal interface ISegment
{
	void GetFirstPara(out int successful, out nint firstParaName);

	void GetNextPara(BaseParagraph currentParagraph, out int found, out nint nextParaName);

	void UpdGetFirstChangeInSegment(out int fFound, out int fChangeFirst, out nint nmpBeforeChange);
}
