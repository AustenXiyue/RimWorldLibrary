using System.Collections.Generic;

namespace System.Windows.Media.Animation;

internal struct WeakRefEnumerator<T>
{
	private List<WeakReference> _list;

	private T _current;

	private int _readIndex;

	private int _writeIndex;

	internal T Current => _current;

	internal int CurrentIndex => _writeIndex - 1;

	internal WeakRefEnumerator(List<WeakReference> list)
	{
		_list = list;
		_readIndex = 0;
		_writeIndex = 0;
		_current = default(T);
	}

	internal void Dispose()
	{
		if (_readIndex != _writeIndex)
		{
			_list.RemoveRange(_writeIndex, _readIndex - _writeIndex);
			_readIndex = (_writeIndex = _list.Count);
		}
		_current = default(T);
	}

	internal bool MoveNext()
	{
		while (_readIndex < _list.Count)
		{
			WeakReference weakReference = _list[_readIndex];
			_current = (T)weakReference.Target;
			if (_current != null)
			{
				if (_writeIndex != _readIndex)
				{
					_list[_writeIndex] = weakReference;
				}
				_readIndex++;
				_writeIndex++;
				return true;
			}
			_readIndex++;
		}
		Dispose();
		return false;
	}
}
