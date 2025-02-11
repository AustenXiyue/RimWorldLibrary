namespace System.Windows.Media;

internal static class HTFBInterpreter
{
	internal const int c_DoHitTest = 2;

	internal const int c_IncludeChidren = 4;

	internal const int c_Stop = 8;

	internal static bool DoHitTest(HitTestFilterBehavior behavior)
	{
		return (behavior & HitTestFilterBehavior.ContinueSkipChildren) == HitTestFilterBehavior.ContinueSkipChildren;
	}

	internal static bool IncludeChildren(HitTestFilterBehavior behavior)
	{
		return (behavior & HitTestFilterBehavior.ContinueSkipSelf) == HitTestFilterBehavior.ContinueSkipSelf;
	}

	internal static bool Stop(HitTestFilterBehavior behavior)
	{
		return (behavior & HitTestFilterBehavior.Stop) == HitTestFilterBehavior.Stop;
	}

	internal static bool SkipSubgraph(HitTestFilterBehavior behavior)
	{
		return behavior == HitTestFilterBehavior.ContinueSkipSelfAndChildren;
	}
}
