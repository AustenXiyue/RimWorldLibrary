using System.Windows.Media.Composition;
using System.Windows.Threading;

namespace System.Windows.Media.Animation;

internal abstract class AnimationClockResource : DUCE.IResource
{
	private DUCE.MultiChannelResource _duceResource;

	private bool _isResourceInvalid;

	protected AnimationClock _animationClock;

	public AnimationClock AnimationClock => _animationClock;

	protected bool IsResourceInvalid
	{
		get
		{
			return _isResourceInvalid;
		}
		set
		{
			_isResourceInvalid = value;
		}
	}

	protected abstract DUCE.ResourceType ResourceType { get; }

	protected AnimationClockResource(AnimationClock animationClock)
	{
		_animationClock = animationClock;
		if (_animationClock != null)
		{
			_animationClock.CurrentTimeInvalidated += OnChanged;
		}
	}

	protected void OnChanged(object sender, EventArgs args)
	{
		MediaContext mediaContext = MediaContext.From(((DispatcherObject)sender).Dispatcher);
		_ = mediaContext.Channel;
		if (!IsResourceInvalid && _duceResource.IsOnAnyChannel)
		{
			mediaContext.ResourcesUpdated += UpdateResourceFromMediaContext;
			IsResourceInvalid = true;
		}
	}

	internal virtual void PropagateChangedHandlersCore(EventHandler handler, bool adding)
	{
		if (_animationClock != null)
		{
			if (adding)
			{
				_animationClock.CurrentTimeInvalidated += handler;
			}
			else
			{
				_animationClock.CurrentTimeInvalidated -= handler;
			}
		}
	}

	private void UpdateResourceFromMediaContext(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (IsResourceInvalid && (skipOnChannelCheck || _duceResource.IsOnChannel(channel)))
		{
			UpdateResource(_duceResource.GetHandle(channel), channel);
			IsResourceInvalid = false;
		}
	}

	protected abstract void UpdateResource(DUCE.ResourceHandle handle, DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, ResourceType))
			{
				UpdateResource(_duceResource.GetHandle(channel), channel);
			}
			return _duceResource.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			_duceResource.ReleaseOnChannel(channel);
		}
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return _duceResource.GetHandle(channel);
		}
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _duceResource.GetChannelCount();
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _duceResource.GetChannel(index);
	}

	void DUCE.IResource.RemoveChildFromParent(DUCE.IResource parent, DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	DUCE.ResourceHandle DUCE.IResource.Get3DHandle(DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}
}
