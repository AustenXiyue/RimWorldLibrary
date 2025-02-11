namespace System.Windows.Input;

internal class AvalonAdapterImpl : IAvalonAdapter
{
	bool IAvalonAdapter.OnNoMoreTabStops(TraversalRequest request, ref bool ShouldCycle)
	{
		return false;
	}
}
