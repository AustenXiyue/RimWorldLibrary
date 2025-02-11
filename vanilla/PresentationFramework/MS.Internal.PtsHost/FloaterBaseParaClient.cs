using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal abstract class FloaterBaseParaClient : BaseParaClient
{
	protected FloaterBaseParaClient(FloaterBaseParagraph paragraph)
		: base(paragraph)
	{
	}

	internal virtual void ArrangeFloater(PTS.FSRECT rcFloater, PTS.FSRECT rcHostPara, uint fswdirParent, PageContext pageContext)
	{
	}

	internal abstract override TextContentRange GetTextContentRange();
}
