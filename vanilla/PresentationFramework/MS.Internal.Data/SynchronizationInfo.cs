using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal struct SynchronizationInfo
{
	public static readonly SynchronizationInfo None = new SynchronizationInfo(null, null);

	private object _context;

	private MethodInfo _callbackMethod;

	private WeakReference _callbackTarget;

	public bool IsSynchronized
	{
		get
		{
			if (_context == null)
			{
				return _callbackMethod != null;
			}
			return true;
		}
	}

	public bool IsAlive
	{
		get
		{
			if (!(_callbackMethod != null) || !_callbackTarget.IsAlive)
			{
				if (_callbackMethod == null)
				{
					return _context != null;
				}
				return false;
			}
			return true;
		}
	}

	public SynchronizationInfo(object context, CollectionSynchronizationCallback callback)
	{
		if (callback == null)
		{
			_context = context;
			_callbackMethod = null;
			_callbackTarget = null;
		}
		else
		{
			_context = new WeakReference(context);
			_callbackMethod = callback.Method;
			object target = callback.Target;
			_callbackTarget = ((target != null) ? new WeakReference(target) : ViewManager.StaticWeakRef);
		}
	}

	public void AccessCollection(IEnumerable collection, Action accessMethod, bool writeAccess)
	{
		if (_callbackMethod != null)
		{
			object obj = _callbackTarget.Target;
			if (obj == null)
			{
				throw new InvalidOperationException(SR.Format(SR.CollectionView_MissingSynchronizationCallback, collection));
			}
			if (_callbackTarget == ViewManager.StaticWeakRef)
			{
				obj = null;
			}
			object obj2 = ((_context is WeakReference weakReference) ? weakReference.Target : _context);
			_callbackMethod.Invoke(obj, new object[4] { collection, obj2, accessMethod, writeAccess });
			return;
		}
		if (_context != null)
		{
			lock (_context)
			{
				accessMethod();
				return;
			}
		}
		accessMethod();
	}
}
