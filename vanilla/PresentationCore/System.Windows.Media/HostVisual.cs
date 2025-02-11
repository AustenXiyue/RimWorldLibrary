using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media.Composition;
using System.Windows.Threading;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a <see cref="T:System.Windows.Media.Visual" /> object that can be connected anywhere to a parent visual tree.</summary>
public class HostVisual : ContainerVisual
{
	private class DisconnectData
	{
		public DispatcherOperation DispatcherOperation { get; private set; }

		public DUCE.Channel Channel { get; private set; }

		public Dispatcher ChannelDispatcher { get; private set; }

		public HostVisual HostVisual { get; private set; }

		public DUCE.ResourceHandle HostHandle { get; private set; }

		public DUCE.ResourceHandle TargetHandle { get; private set; }

		public DUCE.MultiChannelResource ContentRoot { get; private set; }

		public DisconnectData Next { get; set; }

		public DisconnectData(DispatcherOperation op, DUCE.Channel channel, Dispatcher dispatcher, HostVisual hostVisual, DUCE.ResourceHandle hostHandle, DUCE.ResourceHandle targetHandle, DUCE.MultiChannelResource contentRoot, DisconnectData next)
		{
			DispatcherOperation = op;
			Channel = channel;
			ChannelDispatcher = dispatcher;
			HostVisual = hostVisual;
			HostHandle = hostHandle;
			TargetHandle = targetHandle;
			ContentRoot = contentRoot;
			Next = next;
		}
	}

	private VisualTarget _target;

	private Dictionary<DUCE.Channel, Dispatcher> _connectedChannels = new Dictionary<DUCE.Channel, Dispatcher>();

	private static DisconnectData _disconnectData;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.HostVisual" /> class.</summary>
	public HostVisual()
	{
	}

	/// <summary>Implements <see cref="M:System.Windows.Media.HostVisual.HitTestCore(System.Windows.Media.PointHitTestParameters)" /> to supply base hit testing behavior (returning <see cref="T:System.Windows.Media.PointHitTestParameters" />).</summary>
	/// <returns>Returns a value of type <see cref="T:System.Windows.Media.HitTestResult" />. The <see cref="P:System.Windows.Media.HitTestResult.VisualHit" /> property contains the visual object that was hit.</returns>
	/// <param name="hitTestParameters">A value of type <see cref="T:System.Windows.Media.PointHitTestParameters" />.</param>
	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		return null;
	}

	/// <summary>Implements <see cref="M:System.Windows.Media.HostVisual.HitTestCore(System.Windows.Media.GeometryHitTestParameters)" /> to supply base hit testing behavior (returning <see cref="T:System.Windows.Media.GeometryHitTestParameters" />).</summary>
	/// <returns>Returns a value of type <see cref="T:System.Windows.Media.GeometryHitTestResult" />. The <see cref="P:System.Windows.Media.GeometryHitTestResult.VisualHit" /> property contains the visual that was hit.</returns>
	/// <param name="hitTestParameters">A value of type <see cref="T:System.Windows.Media.GeometryHitTestParameters" />.</param>
	protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		return null;
	}

	internal override Rect GetContentBounds()
	{
		return Rect.Empty;
	}

	internal override void RenderContent(RenderContext ctx, bool isOnChannel)
	{
		EnsureHostedVisualConnected(ctx.Channel);
	}

	internal override void FreeContent(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (!DoPendingDisconnect(channel))
			{
				DisconnectHostedVisual(channel, removeChannelFromCollection: true);
			}
		}
		base.FreeContent(channel);
	}

	internal void BeginHosting(VisualTarget target)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_target != null)
			{
				throw new InvalidOperationException(SR.VisualTarget_AnotherTargetAlreadyConnected);
			}
			_target = target;
			if (CheckAccess())
			{
				Invalidate();
				return;
			}
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate(object args)
			{
				((HostVisual)args).Invalidate();
				return (object)null;
			}, this);
		}
	}

	internal void EndHosting()
	{
		using (CompositionEngineLock.Acquire())
		{
			DisconnectHostedVisualOnAllChannels();
			_target = null;
		}
	}

	internal object DoHandleDuplication(object channel)
	{
		DUCE.ResourceHandle resourceHandle = DUCE.ResourceHandle.Null;
		using (CompositionEngineLock.Acquire())
		{
			resourceHandle = _target._contentRoot.DuplicateHandle(_target.OutOfBandChannel, (DUCE.Channel)channel);
			_target.OutOfBandChannel.CloseBatch();
			_target.OutOfBandChannel.Commit();
		}
		return resourceHandle;
	}

	private void EnsureHostedVisualConnected(DUCE.Channel channel)
	{
		if (channel.IsSynchronous || _target == null || _connectedChannels.ContainsKey(channel))
		{
			return;
		}
		DUCE.ResourceHandle hChild = DUCE.ResourceHandle.Null;
		bool flag = true;
		if (_target.CheckAccess())
		{
			_target._contentRoot.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_VISUAL);
			hChild = _target._contentRoot.GetHandle(channel);
		}
		else
		{
			object obj = _target.Dispatcher.Invoke(DispatcherPriority.Normal, TimeSpan.FromMilliseconds(1000.0), new DispatcherOperationCallback(DoHandleDuplication), channel);
			if (obj != null)
			{
				hChild = (DUCE.ResourceHandle)obj;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			if (!hChild.IsNull)
			{
				using (CompositionEngineLock.Acquire())
				{
					DUCE.CompositionNode.InsertChildAt(_proxy.GetHandle(channel), hChild, 0u, channel);
				}
				Dispatcher value = Dispatcher.FromThread(Thread.CurrentThread);
				_connectedChannels.Add(channel, value);
				SetFlags(channel, value: true, VisualProxyFlags.IsContentNodeConnected);
			}
		}
		else
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate(object args)
			{
				((HostVisual)args).Invalidate();
				return (object)null;
			}, this);
		}
	}

	private void DisconnectHostedVisualOnAllChannels()
	{
		IDictionaryEnumerator dictionaryEnumerator = _connectedChannels.GetEnumerator();
		while (dictionaryEnumerator.MoveNext())
		{
			DisconnectHostedVisual((DUCE.Channel)dictionaryEnumerator.Key, removeChannelFromCollection: false);
		}
		_connectedChannels.Clear();
	}

	private void DisconnectHostedVisual(DUCE.Channel channel, bool removeChannelFromCollection)
	{
		if (_target != null && _connectedChannels.TryGetValue(channel, out var value))
		{
			if (value != null && value.CheckAccess())
			{
				Disconnect(channel, value, _proxy.GetHandle(channel), _target._contentRoot.GetHandle(channel), _target._contentRoot);
			}
			else if (value != null)
			{
				_disconnectData = new DisconnectData(value.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(DoDisconnectHostedVisual), channel), channel, value, this, _proxy.GetHandle(channel), _target._contentRoot.GetHandle(channel), _target._contentRoot, _disconnectData);
			}
			if (removeChannelFromCollection)
			{
				_connectedChannels.Remove(channel);
			}
		}
	}

	private object DoDisconnectHostedVisual(object arg)
	{
		using (CompositionEngineLock.Acquire())
		{
			DoPendingDisconnect((DUCE.Channel)arg);
		}
		return null;
	}

	private bool DoPendingDisconnect(DUCE.Channel channel)
	{
		DisconnectData disconnectData = _disconnectData;
		DisconnectData disconnectData2 = null;
		while (disconnectData != null && (disconnectData.HostVisual != this || disconnectData.Channel != channel))
		{
			disconnectData2 = disconnectData;
			disconnectData = disconnectData.Next;
		}
		if (disconnectData == null)
		{
			return false;
		}
		if (disconnectData2 == null)
		{
			_disconnectData = disconnectData.Next;
		}
		else
		{
			disconnectData2.Next = disconnectData.Next;
		}
		disconnectData.DispatcherOperation.Abort();
		Disconnect(disconnectData.Channel, disconnectData.ChannelDispatcher, disconnectData.HostHandle, disconnectData.TargetHandle, disconnectData.ContentRoot);
		return true;
	}

	private void Disconnect(DUCE.Channel channel, Dispatcher channelDispatcher, DUCE.ResourceHandle hostHandle, DUCE.ResourceHandle targetHandle, DUCE.MultiChannelResource contentRoot)
	{
		channelDispatcher.VerifyAccess();
		DUCE.CompositionNode.RemoveChild(hostHandle, targetHandle, channel);
		contentRoot.ReleaseOnChannel(channel);
		SetFlags(channel, value: false, VisualProxyFlags.IsContentNodeConnected);
	}

	private void Invalidate()
	{
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		PropagateChangedFlags();
	}
}
