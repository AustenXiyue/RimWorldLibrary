using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Provides functionality for connecting one visual tree to another visual tree across thread boundaries.</summary>
public class VisualTarget : CompositionTarget
{
	private DUCE.Channel _outOfBandChannel;

	private HostVisual _hostVisual;

	private bool _connected;

	/// <summary>Returns a matrix that can be used to transform coordinates from the <see cref="T:System.Windows.Media.VisualTarget" /> to the rendering destination device.</summary>
	/// <returns>Gets a value of type <see cref="T:System.Windows.Media.Matrix" />.</returns>
	public override Matrix TransformToDevice
	{
		get
		{
			VerifyAPIReadOnly();
			Matrix worldTransform = base.WorldTransform;
			worldTransform.Invert();
			return worldTransform;
		}
	}

	/// <summary>Returns a matrix that can be used to transform coordinates from the rendering destination device to the <see cref="T:System.Windows.Media.VisualTarget" />.</summary>
	/// <returns>Gets a value of type <see cref="T:System.Windows.Media.Matrix" />.</returns>
	public override Matrix TransformFromDevice
	{
		get
		{
			VerifyAPIReadOnly();
			return base.WorldTransform;
		}
	}

	internal DUCE.Channel OutOfBandChannel => _outOfBandChannel;

	/// <summary>Constructor that creates a new <see cref="T:System.Windows.Media.VisualTarget" />.</summary>
	/// <param name="hostVisual">A value of type <see cref="T:System.Windows.Media.HostVisual" />.</param>
	public VisualTarget(HostVisual hostVisual)
	{
		if (hostVisual == null)
		{
			throw new ArgumentNullException("hostVisual");
		}
		_hostVisual = hostVisual;
		_connected = false;
		MediaContext.RegisterICompositionTarget(base.Dispatcher, this);
	}

	private void BeginHosting()
	{
		try
		{
			_hostVisual.BeginHosting(this);
			_connected = true;
		}
		catch
		{
			MediaContext.UnregisterICompositionTarget(base.Dispatcher, this);
			throw;
		}
	}

	internal override void CreateUCEResources(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		_outOfBandChannel = outOfBandChannel;
		base.CreateUCEResources(channel, outOfBandChannel);
		StateChangedCallback(new object[1] { HostStateFlags.None });
		_contentRoot.CreateOrAddRefOnChannel(this, outOfBandChannel, DUCE.ResourceType.TYPE_VISUAL);
		_contentRoot.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_VISUAL);
		BeginHosting();
	}

	/// <summary>Cleans up the state associated with the <see cref="T:System.Windows.Interop.HwndTarget" />.</summary>
	public override void Dispose()
	{
		try
		{
			VerifyAccess();
			if (!base.IsDisposed && _hostVisual != null && _connected)
			{
				RootVisual = null;
				MediaContext.UnregisterICompositionTarget(base.Dispatcher, this);
			}
		}
		finally
		{
			base.Dispose();
		}
	}

	private void EndHosting()
	{
		_hostVisual.EndHosting();
		_connected = false;
	}

	internal override void ReleaseUCEResources(DUCE.Channel channel, DUCE.Channel outOfBandChannel)
	{
		EndHosting();
		_contentRoot.ReleaseOnChannel(channel);
		if (_contentRoot.IsOnChannel(outOfBandChannel))
		{
			_contentRoot.ReleaseOnChannel(outOfBandChannel);
		}
		base.ReleaseUCEResources(channel, outOfBandChannel);
	}
}
