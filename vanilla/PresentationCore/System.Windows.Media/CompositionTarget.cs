using System.Windows.Media.Composition;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

/// <summary>Represents the display surface of your application.</summary>
public abstract class CompositionTarget : DispatcherObject, IDisposable, ICompositionTarget
{
	internal enum HostStateFlags : uint
	{
		None,
		WorldTransform,
		ClipBounds
	}

	internal DUCE.MultiChannelResource _contentRoot;

	internal const DUCE.ResourceType s_contentRootType = DUCE.ResourceType.TYPE_VISUAL;

	private bool _isDisposed;

	private MS.Internal.SecurityCriticalDataForSet<Visual> _rootVisual;

	private RenderContext _cachedRenderContext;

	private Matrix _worldTransform = Matrix.Identity;

	private Rect _worldClipBounds = new Rect(-8.988465674311579E+307, -8.988465674311579E+307, double.MaxValue, double.MaxValue);

	internal bool IsDisposed => _isDisposed;

	/// <summary>Gets or sets the root visual of the <see cref="T:System.Windows.Media.CompositionTarget" />.</summary>
	/// <returns>The root visual of the <see cref="T:System.Windows.Media.CompositionTarget" />.</returns>
	public virtual Visual RootVisual
	{
		get
		{
			VerifyAPIReadOnly();
			return _rootVisual.Value;
		}
		set
		{
			VerifyAPIReadWrite();
			if (_rootVisual.Value != value)
			{
				SetRootVisual(value);
				MediaContext.From(base.Dispatcher).PostRender();
			}
		}
	}

	/// <summary>Gets a matrix that can be used to transform coordinates from this target to the rendering destination device.</summary>
	/// <returns>The transformation matrix. </returns>
	public abstract Matrix TransformToDevice { get; }

	/// <summary>Gets a matrix that can be used to transform coordinates from the rendering destination device to this target.</summary>
	/// <returns>The transformation matrix. </returns>
	public abstract Matrix TransformFromDevice { get; }

	internal Matrix WorldTransform => _worldTransform;

	internal Rect WorldClipBounds => _worldClipBounds;

	/// <summary>Occurs just before the objects in the composition tree are rendered.</summary>
	public static event EventHandler Rendering
	{
		add
		{
			MediaContext mediaContext = MediaContext.From(Dispatcher.CurrentDispatcher);
			mediaContext.Rendering += value;
			mediaContext.PostRender();
		}
		remove
		{
			MediaContext.From(Dispatcher.CurrentDispatcher).Rendering -= value;
		}
	}

	internal CompositionTarget()
	{
	}

	internal virtual void CreateUCEResources(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		_contentRoot.CreateOrAddRefOnChannel(this, outOfBandChannel, DUCE.ResourceType.TYPE_VISUAL);
		_contentRoot.DuplicateHandle(outOfBandChannel, channel);
		outOfBandChannel.CloseBatch();
		outOfBandChannel.Commit();
	}

	internal virtual void ReleaseUCEResources(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		if (_rootVisual.Value != null)
		{
			((DUCE.IResource)_rootVisual.Value).ReleaseOnChannel(channel);
		}
		if (_contentRoot.IsOnChannel(channel))
		{
			_contentRoot.ReleaseOnChannel(channel);
		}
		if (_contentRoot.IsOnChannel(outOfBandChannel))
		{
			_contentRoot.ReleaseOnChannel(outOfBandChannel);
		}
	}

	/// <summary>Disposes <see cref="T:System.Windows.Media.CompositionTarget" />.</summary>
	public virtual void Dispose()
	{
		VerifyAccess();
		if (!_isDisposed)
		{
			_isDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	internal object StateChangedCallback(object arg)
	{
		object[] array = arg as object[];
		HostStateFlags num = (HostStateFlags)array[0];
		if ((num & HostStateFlags.WorldTransform) != 0)
		{
			_worldTransform = (Matrix)array[1];
		}
		if ((num & HostStateFlags.ClipBounds) != 0)
		{
			_worldClipBounds = (Rect)array[2];
		}
		if (_rootVisual.Value != null)
		{
			Visual.PropagateFlags(_rootVisual.Value, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		}
		return null;
	}

	void ICompositionTarget.AddRefOnChannel(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		CreateUCEResources(channel, outOfBandChannel);
	}

	void ICompositionTarget.ReleaseOnChannel(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		ReleaseUCEResources(channel, outOfBandChannel);
	}

	void ICompositionTarget.Render(bool inResize, DUCE.Channel channel)
	{
		if (_rootVisual.Value != null)
		{
			bool flag = false;
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral | EventTrace.Keyword.KeywordPerf, EventTrace.Level.Info))
			{
				flag = true;
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientPrecomputeSceneBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info, PerfService.GetPerfElementID(this));
			}
			_rootVisual.Value.Precompute();
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientPrecomputeSceneEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info);
			}
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientCompileSceneBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info, PerfService.GetPerfElementID(this));
			}
			Compile(channel);
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientCompileSceneEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info);
			}
		}
	}

	private void Compile(DUCE.Channel channel)
	{
		MediaContext mediaContext = MediaContext.From(base.Dispatcher);
		Invariant.Assert(_rootVisual.Value != null);
		RenderContext renderContext = null;
		Invariant.Assert(channel != null);
		if (_cachedRenderContext != null)
		{
			renderContext = _cachedRenderContext;
			_cachedRenderContext = null;
		}
		else
		{
			renderContext = new RenderContext();
		}
		renderContext.Initialize(channel, _contentRoot.GetHandle(channel));
		if (mediaContext.IsConnected)
		{
			_rootVisual.Value.Render(renderContext, 0u);
		}
		_cachedRenderContext = renderContext;
	}

	private void SetRootVisual(Visual visual)
	{
		if (visual != null && (visual._parent != null || visual.IsRootElement))
		{
			throw new ArgumentException(SR.CompositionTarget_RootVisual_HasParent);
		}
		DUCE.Channel channel = MediaContext.From(base.Dispatcher).GetChannels().Channel;
		if (_rootVisual.Value != null && _contentRoot.IsOnChannel(channel))
		{
			ClearRootNode(channel);
			((DUCE.IResource)_rootVisual.Value).ReleaseOnChannel(channel);
			_rootVisual.Value.IsRootElement = false;
		}
		_rootVisual.Value = visual;
		if (_rootVisual.Value != null)
		{
			_rootVisual.Value.IsRootElement = true;
			_rootVisual.Value.SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsSubtreeDirtyForRender);
		}
	}

	private void ClearRootNode(DUCE.Channel channel)
	{
		DUCE.CompositionNode.RemoveAllChildren(_contentRoot.GetHandle(channel), channel);
	}

	internal void VerifyAPIReadOnly()
	{
		VerifyAccess();
		if (_isDisposed)
		{
			throw new ObjectDisposedException("CompositionTarget");
		}
	}

	internal void VerifyAPIReadWrite()
	{
		VerifyAccess();
		if (_isDisposed)
		{
			throw new ObjectDisposedException("CompositionTarget");
		}
		MediaContext.From(base.Dispatcher).VerifyWriteAccess();
	}
}
