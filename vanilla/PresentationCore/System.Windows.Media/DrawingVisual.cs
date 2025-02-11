using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>
///   <see cref="T:System.Windows.Media.DrawingVisual" /> is a visual object that can be used to render vector graphics on the screen. The content is persisted by the system.</summary>
public class DrawingVisual : ContainerVisual
{
	private IDrawingContent _content;

	/// <summary>Gets the drawing content of the <see cref="T:System.Windows.Media.DrawingVisual" /> object.</summary>
	/// <returns>Gets a value of type <see cref="T:System.Windows.Media.DrawingGroup" /> that represents the collection of <see cref="T:System.Windows.Media.Drawing" /> objects in the <see cref="T:System.Windows.Media.DrawingVisual" />.</returns>
	public DrawingGroup Drawing => GetDrawing();

	/// <summary>Determines whether a point coordinate value is within the bounds of the <see cref="T:System.Windows.Media.DrawingVisual" /> object.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.HitTestResult" />, representing the <see cref="T:System.Windows.Media.Visual" /> returned from a hit test. </returns>
	/// <param name="hitTestParameters">A value of type <see cref="T:System.Windows.Media.PointHitTestParameters" /> that specifies the <see cref="T:System.Windows.Point" /> to hit test against.</param>
	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (hitTestParameters == null)
		{
			throw new ArgumentNullException("hitTestParameters");
		}
		if (_content != null && _content.HitTestPoint(hitTestParameters.HitPoint))
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}
		return null;
	}

	/// <summary>Determines whether a geometry value is within the bounds of the visual object.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.GeometryHitTestResult" />.</returns>
	/// <param name="hitTestParameters">A value of type <see cref="T:System.Windows.Media.GeometryHitTestParameters" /> that specifies the <see cref="T:System.Windows.Media.Geometry" /> to hit test against.</param>
	protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		if (hitTestParameters == null)
		{
			throw new ArgumentNullException("hitTestParameters");
		}
		if (_content != null && GetHitTestBounds().IntersectsWith(hitTestParameters.Bounds))
		{
			IntersectionDetail intersectionDetail = _content.HitTestGeometry(hitTestParameters.InternalHitGeometry);
			if (intersectionDetail != IntersectionDetail.Empty)
			{
				return new GeometryHitTestResult(this, intersectionDetail);
			}
		}
		return null;
	}

	/// <summary>Opens the <see cref="T:System.Windows.Media.DrawingVisual" /> object for rendering. The returned <see cref="T:System.Windows.Media.DrawingContext" /> value can be used to render into the <see cref="T:System.Windows.Media.DrawingVisual" />.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.DrawingContext" />.</returns>
	public DrawingContext RenderOpen()
	{
		VerifyAPIReadWrite();
		return new VisualDrawingContext(this);
	}

	internal override void RenderClose(IDrawingContent newContent)
	{
		IDrawingContent content = _content;
		_content = null;
		if (content != null)
		{
			content.PropagateChangedHandler(base.ContentsChangedHandler, adding: false);
			DisconnectAttachedResource(VisualProxyFlags.IsContentConnected, content);
		}
		newContent?.PropagateChangedHandler(base.ContentsChangedHandler, adding: true);
		_content = newContent;
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		Visual.PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
	}

	internal override void FreeContent(DUCE.Channel channel)
	{
		if (_content != null && CheckFlagsAnd(channel, VisualProxyFlags.IsContentConnected))
		{
			DUCE.CompositionNode.SetContent(_proxy.GetHandle(channel), DUCE.ResourceHandle.Null, channel);
			SetFlags(channel, value: false, VisualProxyFlags.IsContentConnected);
			_content.ReleaseOnChannel(channel);
		}
		base.FreeContent(channel);
	}

	internal override Rect GetContentBounds()
	{
		if (_content != null)
		{
			_ = Rect.Empty;
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			BoundsDrawingContextWalker ctx = mediaContext.AcquireBoundsDrawingContextWalker();
			Rect contentBounds = _content.GetContentBounds(ctx);
			mediaContext.ReleaseBoundsDrawingContextWalker(ctx);
			return contentBounds;
		}
		return Rect.Empty;
	}

	internal void WalkContent(DrawingContextWalker walker)
	{
		VerifyAPIReadOnly();
		if (_content != null)
		{
			_content.WalkContent(walker);
		}
	}

	internal override void RenderContent(RenderContext ctx, bool isOnChannel)
	{
		DUCE.Channel channel = ctx.Channel;
		if (_content != null)
		{
			DUCE.CompositionNode.SetContent(_proxy.GetHandle(channel), _content.AddRefOnChannel(channel), channel);
			SetFlags(channel, value: true, VisualProxyFlags.IsContentConnected);
		}
		else if (isOnChannel)
		{
			DUCE.CompositionNode.SetContent(_proxy.GetHandle(channel), DUCE.ResourceHandle.Null, channel);
		}
	}

	internal override DrawingGroup GetDrawing()
	{
		VerifyAPIReadOnly();
		DrawingGroup result = null;
		if (_content != null)
		{
			result = DrawingServices.DrawingGroupFromRenderData((RenderData)_content);
		}
		return result;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingVisual" /> class.</summary>
	public DrawingVisual()
	{
	}
}
