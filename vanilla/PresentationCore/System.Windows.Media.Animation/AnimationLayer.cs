using System.Collections.Generic;

namespace System.Windows.Media.Animation;

internal class AnimationLayer
{
	private object _snapshotValue = DependencyProperty.UnsetValue;

	private IList<AnimationClock> _animationClocks;

	private AnimationStorage _ownerStorage;

	private EventHandler _removeRequestedHandler;

	private bool _hasStickySnapshotValue;

	internal AnimationLayer(AnimationStorage ownerStorage)
	{
		_ownerStorage = ownerStorage;
		_removeRequestedHandler = OnRemoveRequested;
	}

	internal void ApplyAnimationClocks(IList<AnimationClock> newAnimationClocks, HandoffBehavior handoffBehavior, object defaultDestinationValue)
	{
		if (handoffBehavior == HandoffBehavior.SnapshotAndReplace)
		{
			EventHandler value = OnCurrentStateInvalidated;
			if (_hasStickySnapshotValue)
			{
				_animationClocks[0].CurrentStateInvalidated -= value;
				DetachAnimationClocks();
			}
			else if (_animationClocks != null)
			{
				_snapshotValue = GetCurrentValue(defaultDestinationValue);
				DetachAnimationClocks();
			}
			else
			{
				_snapshotValue = defaultDestinationValue;
			}
			if (newAnimationClocks != null && newAnimationClocks[0].CurrentState == ClockState.Stopped)
			{
				_hasStickySnapshotValue = true;
				newAnimationClocks[0].CurrentStateInvalidated += value;
			}
			else
			{
				_hasStickySnapshotValue = false;
			}
			SetAnimationClocks(newAnimationClocks);
		}
		else if (newAnimationClocks != null)
		{
			if (_animationClocks == null)
			{
				SetAnimationClocks(newAnimationClocks);
			}
			else
			{
				AppendAnimationClocks(newAnimationClocks);
			}
		}
	}

	private void DetachAnimationClocks()
	{
		int count = _animationClocks.Count;
		for (int i = 0; i < count; i++)
		{
			_ownerStorage.DetachAnimationClock(_animationClocks[i], _removeRequestedHandler);
		}
		_animationClocks = null;
	}

	private void SetAnimationClocks(IList<AnimationClock> animationClocks)
	{
		_animationClocks = animationClocks;
		int count = animationClocks.Count;
		for (int i = 0; i < count; i++)
		{
			_ownerStorage.AttachAnimationClock(animationClocks[i], _removeRequestedHandler);
		}
	}

	private void OnCurrentStateInvalidated(object sender, EventArgs args)
	{
		_hasStickySnapshotValue = false;
		((AnimationClock)sender).CurrentStateInvalidated -= OnCurrentStateInvalidated;
	}

	private void OnRemoveRequested(object sender, EventArgs args)
	{
		AnimationClock animationClock = (AnimationClock)sender;
		int num = _animationClocks.IndexOf(animationClock);
		if (_hasStickySnapshotValue && num == 0)
		{
			_animationClocks[0].CurrentStateInvalidated -= OnCurrentStateInvalidated;
			_hasStickySnapshotValue = false;
		}
		_animationClocks.RemoveAt(num);
		_ownerStorage.DetachAnimationClock(animationClock, _removeRequestedHandler);
		AnimationStorage ownerStorage = _ownerStorage;
		if (_animationClocks.Count == 0)
		{
			_animationClocks = null;
			_snapshotValue = DependencyProperty.UnsetValue;
			_ownerStorage.RemoveLayer(this);
			_ownerStorage = null;
		}
		ownerStorage.WritePostscript();
	}

	private void AppendAnimationClocks(IList<AnimationClock> newAnimationClocks)
	{
		int count = newAnimationClocks.Count;
		List<AnimationClock> list = _animationClocks as List<AnimationClock>;
		if (list == null)
		{
			int num = ((_animationClocks != null) ? _animationClocks.Count : 0);
			list = new List<AnimationClock>(num + count);
			for (int i = 0; i < num; i++)
			{
				list.Add(_animationClocks[i]);
			}
			_animationClocks = list;
		}
		for (int j = 0; j < count; j++)
		{
			AnimationClock animationClock = newAnimationClocks[j];
			list.Add(animationClock);
			_ownerStorage.AttachAnimationClock(animationClock, _removeRequestedHandler);
		}
	}

	internal object GetCurrentValue(object defaultDestinationValue)
	{
		if (_hasStickySnapshotValue && _animationClocks[0].CurrentState == ClockState.Stopped)
		{
			return _snapshotValue;
		}
		if (_animationClocks == null)
		{
			return _snapshotValue;
		}
		object obj = _snapshotValue;
		bool flag = false;
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = defaultDestinationValue;
		}
		int count = _animationClocks.Count;
		for (int i = 0; i < count; i++)
		{
			AnimationClock animationClock = _animationClocks[i];
			if (animationClock.CurrentState != ClockState.Stopped)
			{
				flag = true;
				obj = animationClock.GetCurrentValue(obj, defaultDestinationValue);
			}
		}
		if (flag)
		{
			return obj;
		}
		return defaultDestinationValue;
	}
}
