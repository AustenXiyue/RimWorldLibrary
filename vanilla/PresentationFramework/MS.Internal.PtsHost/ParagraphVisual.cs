using System.Windows;
using System.Windows.Media;

namespace MS.Internal.PtsHost;

internal class ParagraphVisual : DrawingVisual
{
	private Brush _backgroundBrush;

	private Brush _borderBrush;

	private Thickness _borderThickness;

	private Rect _renderBounds;

	internal ParagraphVisual()
	{
		_renderBounds = Rect.Empty;
	}

	internal void DrawBackgroundAndBorder(Brush backgroundBrush, Brush borderBrush, Thickness borderThickness, Rect renderBounds, bool isFirstChunk, bool isLastChunk)
	{
		if (_backgroundBrush != backgroundBrush || _renderBounds != renderBounds || _borderBrush != borderBrush || !Thickness.AreClose(_borderThickness, borderThickness))
		{
			using (DrawingContext dc = RenderOpen())
			{
				DrawBackgroundAndBorderIntoContext(dc, backgroundBrush, borderBrush, borderThickness, renderBounds, isFirstChunk, isLastChunk);
			}
		}
	}

	internal void DrawBackgroundAndBorderIntoContext(DrawingContext dc, Brush backgroundBrush, Brush borderBrush, Thickness borderThickness, Rect renderBounds, bool isFirstChunk, bool isLastChunk)
	{
		_backgroundBrush = (Brush)FreezableOperations.GetAsFrozenIfPossible(backgroundBrush);
		_renderBounds = renderBounds;
		_borderBrush = (Brush)FreezableOperations.GetAsFrozenIfPossible(borderBrush);
		_borderThickness = borderThickness;
		if (!isFirstChunk)
		{
			_borderThickness.Top = 0.0;
		}
		if (!isLastChunk)
		{
			_borderThickness.Bottom = 0.0;
		}
		if (_borderBrush != null)
		{
			Pen pen = new Pen();
			pen.Brush = _borderBrush;
			pen.Thickness = _borderThickness.Left;
			if (pen.CanFreeze)
			{
				pen.Freeze();
			}
			if (_borderThickness.IsUniform)
			{
				dc.DrawRectangle(null, pen, new Rect(new Point(_renderBounds.Left + pen.Thickness * 0.5, _renderBounds.Bottom - pen.Thickness * 0.5), new Point(_renderBounds.Right - pen.Thickness * 0.5, _renderBounds.Top + pen.Thickness * 0.5)));
			}
			else
			{
				if (DoubleUtil.GreaterThanZero(_borderThickness.Left))
				{
					dc.DrawLine(pen, new Point(_renderBounds.Left + pen.Thickness / 2.0, _renderBounds.Top), new Point(_renderBounds.Left + pen.Thickness / 2.0, _renderBounds.Bottom));
				}
				if (DoubleUtil.GreaterThanZero(_borderThickness.Right))
				{
					pen = new Pen();
					pen.Brush = _borderBrush;
					pen.Thickness = _borderThickness.Right;
					if (pen.CanFreeze)
					{
						pen.Freeze();
					}
					dc.DrawLine(pen, new Point(_renderBounds.Right - pen.Thickness / 2.0, _renderBounds.Top), new Point(_renderBounds.Right - pen.Thickness / 2.0, _renderBounds.Bottom));
				}
				if (DoubleUtil.GreaterThanZero(_borderThickness.Top))
				{
					pen = new Pen();
					pen.Brush = _borderBrush;
					pen.Thickness = _borderThickness.Top;
					if (pen.CanFreeze)
					{
						pen.Freeze();
					}
					dc.DrawLine(pen, new Point(_renderBounds.Left, _renderBounds.Top + pen.Thickness / 2.0), new Point(_renderBounds.Right, _renderBounds.Top + pen.Thickness / 2.0));
				}
				if (DoubleUtil.GreaterThanZero(_borderThickness.Bottom))
				{
					pen = new Pen();
					pen.Brush = _borderBrush;
					pen.Thickness = _borderThickness.Bottom;
					if (pen.CanFreeze)
					{
						pen.Freeze();
					}
					dc.DrawLine(pen, new Point(_renderBounds.Left, _renderBounds.Bottom - pen.Thickness / 2.0), new Point(_renderBounds.Right, _renderBounds.Bottom - pen.Thickness / 2.0));
				}
			}
		}
		if (_backgroundBrush != null)
		{
			dc.DrawRectangle(_backgroundBrush, null, new Rect(new Point(_renderBounds.Left + _borderThickness.Left, _renderBounds.Top + _borderThickness.Top), new Point(_renderBounds.Right - _borderThickness.Right, _renderBounds.Bottom - _borderThickness.Bottom)));
		}
	}
}
