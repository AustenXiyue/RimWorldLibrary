using System;
using System.Collections;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct WeakReferenceListEnumerator : IEnumerator
{
	private int _i;

	private ArrayList _List;

	private object _StrongReference;

	object IEnumerator.Current => Current;

	public object Current
	{
		get
		{
			if (_StrongReference == null)
			{
				throw new InvalidOperationException(SR.Enumerator_VerifyContext);
			}
			return _StrongReference;
		}
	}

	public WeakReferenceListEnumerator(ArrayList List)
	{
		_i = 0;
		_List = List;
		_StrongReference = null;
	}

	public bool MoveNext()
	{
		object obj = null;
		while (_i < _List.Count)
		{
			obj = ((WeakReference)_List[_i++]).Target;
			if (obj != null)
			{
				break;
			}
		}
		_StrongReference = obj;
		return obj != null;
	}

	public void Reset()
	{
		_i = 0;
		_StrongReference = null;
	}
}
