using System.Windows.Media.TextFormatting;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal sealed class ParagraphBreakRun : TextEndOfParagraph
{
	internal readonly PTS.FSFLRES BreakReason;

	internal ParagraphBreakRun(int length, PTS.FSFLRES breakReason)
		: base(length)
	{
		BreakReason = breakReason;
	}
}
