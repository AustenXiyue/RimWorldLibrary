using System;
using System.Windows;
using System.Windows.Media;

namespace MS.Internal.TextFormatting;

internal sealed class DrawingState : IDisposable
{
	private TextMetrics.FullTextLine _currentLine;

	private DrawingContext _drawingContext;

	private Point _lineOrigin;

	private Point _vectorToLineOrigin;

	private MatrixTransform _antiInversion;

	private bool _overrideBaseGuidelineY;

	private double _baseGuidelineY;

	internal DrawingContext DrawingContext => _drawingContext;

	internal MatrixTransform AntiInversion => _antiInversion;

	internal Point LineOrigin => _lineOrigin;

	internal Point VectorToLineOrigin => _vectorToLineOrigin;

	internal TextMetrics.FullTextLine CurrentLine => _currentLine;

	internal DrawingState(DrawingContext drawingContext, Point lineOrigin, MatrixTransform antiInversion, TextMetrics.FullTextLine currentLine)
	{
		_drawingContext = drawingContext;
		_antiInversion = antiInversion;
		_currentLine = currentLine;
		if (antiInversion == null)
		{
			_lineOrigin = lineOrigin;
		}
		else
		{
			_vectorToLineOrigin = lineOrigin;
		}
		if (_drawingContext != null)
		{
			_baseGuidelineY = lineOrigin.Y + currentLine.Baseline;
			_drawingContext.PushGuidelineY1(_baseGuidelineY);
		}
	}

	internal void SetGuidelineY(double runGuidelineY)
	{
		if (_drawingContext != null)
		{
			Invariant.Assert(!_overrideBaseGuidelineY);
			if (runGuidelineY != _baseGuidelineY)
			{
				_drawingContext.PushGuidelineY1(runGuidelineY);
				_overrideBaseGuidelineY = true;
			}
		}
	}

	internal void UnsetGuidelineY()
	{
		if (_overrideBaseGuidelineY)
		{
			_drawingContext.Pop();
			_overrideBaseGuidelineY = false;
		}
	}

	public void Dispose()
	{
		if (_drawingContext != null)
		{
			_drawingContext.Pop();
		}
	}
}
