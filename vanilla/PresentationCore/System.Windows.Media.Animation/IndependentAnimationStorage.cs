using System.Windows.Media.Composition;

namespace System.Windows.Media.Animation;

internal abstract class IndependentAnimationStorage : AnimationStorage, DUCE.IResource
{
	protected MediaContext.ResourcesUpdatedHandler _updateResourceHandler;

	protected DUCE.MultiChannelResource _duceResource;

	private bool _isValid = true;

	protected abstract DUCE.ResourceType ResourceType { get; }

	protected abstract void UpdateResourceCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, ResourceType))
			{
				_updateResourceHandler = UpdateResource;
				UpdateResourceCore(channel);
			}
			return _duceResource.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			_duceResource.ReleaseOnChannel(channel);
			if (!_duceResource.IsOnAnyChannel)
			{
				DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
				if (!_isValid)
				{
					MediaContext.From(dependencyObject.Dispatcher).ResourcesUpdated -= _updateResourceHandler;
					_isValid = true;
				}
				_updateResourceHandler = null;
			}
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

	private void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			UpdateResourceCore(channel);
			_isValid = true;
		}
	}

	internal void InvalidateResource()
	{
		if (_isValid && _updateResourceHandler != null)
		{
			_ = (DependencyObject)_dependencyObject.Target;
			_isValid = false;
			MediaContext.CurrentMediaContext.ResourcesUpdated += _updateResourceHandler;
		}
	}

	internal static DUCE.ResourceHandle GetResourceHandle(DependencyObject d, DependencyProperty dp, DUCE.Channel channel)
	{
		if (!(AnimationStorage.GetStorage(d, dp) is IndependentAnimationStorage independentAnimationStorage))
		{
			return DUCE.ResourceHandle.Null;
		}
		return ((DUCE.IResource)independentAnimationStorage).GetHandle(channel);
	}
}
