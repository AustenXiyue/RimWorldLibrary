namespace System.Windows.Input;

internal interface IAvalonAdapter
{
	bool OnNoMoreTabStops(TraversalRequest request, ref bool ShouldCycle);
}
