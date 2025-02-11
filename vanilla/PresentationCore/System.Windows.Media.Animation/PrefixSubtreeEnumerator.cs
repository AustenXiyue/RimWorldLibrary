namespace System.Windows.Media.Animation;

internal struct PrefixSubtreeEnumerator
{
	private Clock _rootClock;

	private Clock _currentClock;

	private SubtreeFlag _flags;

	internal Clock Current => _currentClock;

	internal PrefixSubtreeEnumerator(Clock root, bool processRoot)
	{
		_rootClock = root;
		_currentClock = null;
		_flags = ((!processRoot) ? SubtreeFlag.Reset : (SubtreeFlag.Reset | SubtreeFlag.ProcessRoot));
	}

	internal void SkipSubtree()
	{
		_flags |= SubtreeFlag.SkipSubtree;
	}

	public bool MoveNext()
	{
		if ((_flags & SubtreeFlag.Reset) != 0)
		{
			if ((_flags & SubtreeFlag.ProcessRoot) != 0)
			{
				_currentClock = _rootClock;
			}
			else if (_rootClock != null)
			{
				if (_rootClock is ClockGroup clockGroup)
				{
					_currentClock = clockGroup.FirstChild;
				}
				else
				{
					_currentClock = null;
				}
			}
			_flags &= ~SubtreeFlag.Reset;
		}
		else if (_currentClock != null)
		{
			Clock clock = ((!(_currentClock is ClockGroup clockGroup2)) ? null : clockGroup2.FirstChild);
			if ((_flags & SubtreeFlag.SkipSubtree) != 0 || clock == null)
			{
				while (_currentClock != _rootClock && (clock = _currentClock.NextSibling) == null)
				{
					_currentClock = _currentClock.InternalParent;
				}
				if (_currentClock == _rootClock)
				{
					clock = null;
				}
				_flags &= ~SubtreeFlag.SkipSubtree;
			}
			_currentClock = clock;
		}
		return _currentClock != null;
	}

	public void Reset()
	{
		_currentClock = null;
		_flags = (_flags & SubtreeFlag.ProcessRoot) | SubtreeFlag.Reset;
	}
}
