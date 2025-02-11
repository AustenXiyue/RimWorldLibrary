using System;
using System.Collections;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal class GenericEnumerator : IEnumerator
{
	internal delegate int GetGenerationIDDelegate();

	private IList _array;

	private object _current;

	private int _count;

	private int _position;

	private int _originalGenerationID;

	private GetGenerationIDDelegate _getGenerationID;

	object IEnumerator.Current
	{
		get
		{
			VerifyCurrent();
			return _current;
		}
	}

	private GenericEnumerator()
	{
	}

	internal GenericEnumerator(IList array, GetGenerationIDDelegate getGenerationID)
	{
		_array = array;
		_count = _array.Count;
		_position = -1;
		_getGenerationID = getGenerationID;
		_originalGenerationID = _getGenerationID();
	}

	private void VerifyCurrent()
	{
		if (-1 == _position || _position >= _count)
		{
			throw new InvalidOperationException(SR.Enumerator_VerifyContext);
		}
	}

	public bool MoveNext()
	{
		if (_getGenerationID() != _originalGenerationID)
		{
			throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
		}
		_position++;
		if (_position >= _count)
		{
			_position = _count;
			return false;
		}
		_current = _array[_position];
		return true;
	}

	public void Reset()
	{
		if (_getGenerationID() != _originalGenerationID)
		{
			throw new InvalidOperationException(SR.Enumerator_CollectionChanged);
		}
		_position = -1;
	}
}
