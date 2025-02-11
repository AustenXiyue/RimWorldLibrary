using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.KnownBoxes;

[FriendAccessAllowed]
internal static class FillRuleBoxes
{
	internal static object EvenOddBox = FillRule.EvenOdd;

	internal static object NonzeroBox = FillRule.Nonzero;

	internal static object Box(FillRule value)
	{
		if (value == FillRule.Nonzero)
		{
			return NonzeroBox;
		}
		return EvenOddBox;
	}
}
