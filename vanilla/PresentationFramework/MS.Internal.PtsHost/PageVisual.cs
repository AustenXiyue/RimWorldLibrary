using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace MS.Internal.PtsHost;

internal class PageVisual : DrawingVisual, IContentHost
{
	private readonly WeakReference _owner;

	private Brush _backgroundBrush;

	private Rect _renderBounds;

	internal Visual Child
	{
		get
		{
			VisualCollection children = base.Children;
			if (children.Count != 0)
			{
				return children[0];
			}
			return null;
		}
		set
		{
			VisualCollection children = base.Children;
			if (children.Count == 0)
			{
				children.Add(value);
			}
			else if (children[0] != value)
			{
				children[0] = value;
			}
		}
	}

	IEnumerator<IInputElement> IContentHost.HostedElements
	{
		get
		{
			if (_owner.Target is IContentHost contentHost)
			{
				return contentHost.HostedElements;
			}
			return null;
		}
	}

	internal PageVisual(FlowDocumentPage owner)
	{
		_owner = new WeakReference(owner);
	}

	internal void DrawBackground(Brush backgroundBrush, Rect renderBounds)
	{
		if (_backgroundBrush == backgroundBrush && !(_renderBounds != renderBounds))
		{
			return;
		}
		_backgroundBrush = backgroundBrush;
		_renderBounds = renderBounds;
		using DrawingContext drawingContext = RenderOpen();
		if (_backgroundBrush != null)
		{
			drawingContext.DrawRectangle(_backgroundBrush, null, _renderBounds);
		}
		else
		{
			drawingContext.DrawRectangle(Brushes.Transparent, null, _renderBounds);
		}
	}

	internal void ClearDrawingContext()
	{
		RenderOpen()?.Close();
	}

	IInputElement IContentHost.InputHitTest(Point point)
	{
		if (_owner.Target is IContentHost contentHost)
		{
			return contentHost.InputHitTest(point);
		}
		return null;
	}

	ReadOnlyCollection<Rect> IContentHost.GetRectangles(ContentElement child)
	{
		if (_owner.Target is IContentHost contentHost)
		{
			return contentHost.GetRectangles(child);
		}
		return new ReadOnlyCollection<Rect>(new List<Rect>(0));
	}

	void IContentHost.OnChildDesiredSizeChanged(UIElement child)
	{
		if (_owner.Target is IContentHost contentHost)
		{
			contentHost.OnChildDesiredSizeChanged(child);
		}
	}
}
