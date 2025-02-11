using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal sealed class UIElementParagraphResult : FloaterBaseParagraphResult
{
	internal override bool HasTextContent => true;

	internal UIElementParagraphResult(BaseParaClient paraClient)
		: base(paraClient)
	{
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		return ((UIElementParaClient)_paraClient).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition);
	}
}
