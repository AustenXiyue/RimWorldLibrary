namespace System.Windows.Media;

internal class VisualDrawingContext : RenderDataDrawingContext
{
	private Visual _ownerVisual;

	internal VisualDrawingContext(Visual ownerVisual)
	{
		_ownerVisual = ownerVisual;
	}

	protected override void CloseCore(RenderData renderData)
	{
		_ownerVisual.RenderClose(renderData);
	}
}
