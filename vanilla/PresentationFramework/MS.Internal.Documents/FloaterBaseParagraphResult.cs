using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal abstract class FloaterBaseParagraphResult : ParagraphResult
{
	internal FloaterBaseParagraphResult(BaseParaClient paraClient)
		: base(paraClient)
	{
	}
}
