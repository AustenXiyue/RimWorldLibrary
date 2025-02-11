using System.Windows.Media.TextFormatting;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal sealed class LineBreakRun : TextEndOfLine
{
	internal readonly PTS.FSFLRES BreakReason;

	internal LineBreakRun(int length, PTS.FSFLRES breakReason)
		: base(length)
	{
		BreakReason = breakReason;
	}
}
