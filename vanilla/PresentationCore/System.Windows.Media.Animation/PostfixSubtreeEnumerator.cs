namespace System.Windows.Media.Animation;

internal struct PostfixSubtreeEnumerator
{
	private Clock _rootClock;

	private Clock _currentClock;

	private SubtreeFlag _flags;

	internal Clock Current => _currentClock;

	internal PostfixSubtreeEnumerator(Clock root, bool processRoot)
	{
		_rootClock = root;
		_currentClock = null;
		_flags = ((!processRoot) ? SubtreeFlag.Reset : (SubtreeFlag.Reset | SubtreeFlag.ProcessRoot));
	}

	public bool MoveNext()
	{
		if ((_flags & SubtreeFlag.Reset) != 0)
		{
			_currentClock = _rootClock;
			_flags &= ~SubtreeFlag.Reset;
		}
		else if (_currentClock == _rootClock)
		{
			_currentClock = null;
		}
		if (_currentClock != null)
		{
			Clock currentClock = _currentClock;
			if (_currentClock != _rootClock && (currentClock = _currentClock.NextSibling) == null)
			{
				_currentClock = _currentClock.InternalParent;
			}
			else
			{
				do
				{
					_currentClock = currentClock;
				}
				while ((currentClock = _currentClock.FirstChild) != null);
			}
			if (_currentClock == _rootClock && (_flags & SubtreeFlag.ProcessRoot) == 0)
			{
				_currentClock = null;
			}
		}
		return _currentClock != null;
	}
}
