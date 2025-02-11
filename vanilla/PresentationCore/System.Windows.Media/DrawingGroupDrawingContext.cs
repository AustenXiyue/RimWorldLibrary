namespace System.Windows.Media;

internal class DrawingGroupDrawingContext : DrawingDrawingContext
{
	private DrawingGroup _drawingGroup;

	internal DrawingGroupDrawingContext(DrawingGroup drawingGroup)
	{
		_drawingGroup = drawingGroup;
	}

	protected override void CloseCore(DrawingCollection rootDrawingGroupChildren)
	{
		_drawingGroup.Close(rootDrawingGroupChildren);
	}
}
