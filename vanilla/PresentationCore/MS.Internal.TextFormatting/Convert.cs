using System.Windows;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class Convert
{
	private Convert()
	{
	}

	public static FlowDirection LsTFlowToFlowDirection(LsTFlow lstflow)
	{
		switch (lstflow)
		{
		case LsTFlow.lstflowDefault:
		case LsTFlow.lstflowEN:
			return FlowDirection.LeftToRight;
		case LsTFlow.lstflowWS:
		case LsTFlow.lstflowWN:
			return FlowDirection.RightToLeft;
		default:
			return FlowDirection.LeftToRight;
		}
	}

	public static LsKTab LsKTabFromTabAlignment(TextTabAlignment tabAlignment)
	{
		return tabAlignment switch
		{
			TextTabAlignment.Right => LsKTab.lsktRight, 
			TextTabAlignment.Center => LsKTab.lsktCenter, 
			TextTabAlignment.Character => LsKTab.lsktChar, 
			_ => LsKTab.lsktLeft, 
		};
	}
}
