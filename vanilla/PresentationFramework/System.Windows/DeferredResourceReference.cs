using MS.Internal;

namespace System.Windows;

internal class DeferredResourceReference : DeferredReference
{
	private ResourceDictionary _dictionary;

	protected object _keyOrValue;

	private WeakReferenceList _inflatedList;

	internal virtual object Key => _keyOrValue;

	internal ResourceDictionary Dictionary
	{
		get
		{
			return _dictionary;
		}
		set
		{
			_dictionary = value;
		}
	}

	internal virtual object Value
	{
		get
		{
			return _keyOrValue;
		}
		set
		{
			_keyOrValue = value;
		}
	}

	internal virtual bool IsUnset => false;

	internal bool IsInflated => _dictionary == null;

	internal DeferredResourceReference(ResourceDictionary dictionary, object key)
	{
		_dictionary = dictionary;
		_keyOrValue = key;
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		if (_dictionary != null)
		{
			bool canCache;
			object value = _dictionary.GetValue(_keyOrValue, out canCache);
			if (canCache)
			{
				_keyOrValue = value;
				RemoveFromDictionary();
			}
			if (valueSource == BaseValueSourceInternal.ThemeStyle || valueSource == BaseValueSourceInternal.ThemeStyleTrigger || valueSource == BaseValueSourceInternal.Style || valueSource == BaseValueSourceInternal.TemplateTrigger || valueSource == BaseValueSourceInternal.StyleTrigger || valueSource == BaseValueSourceInternal.ParentTemplate || valueSource == BaseValueSourceInternal.ParentTemplateTrigger)
			{
				StyleHelper.SealIfSealable(value);
			}
			OnInflated();
			return value;
		}
		return _keyOrValue;
	}

	private void OnInflated()
	{
		if (_inflatedList != null)
		{
			WeakReferenceListEnumerator enumerator = _inflatedList.GetEnumerator();
			while (enumerator.MoveNext())
			{
				((ResourceReferenceExpression)enumerator.Current).OnDeferredResourceInflated(this);
			}
		}
	}

	internal override Type GetValueType()
	{
		bool found;
		if (_dictionary != null)
		{
			return _dictionary.GetValueType(_keyOrValue, out found);
		}
		if (_keyOrValue == null)
		{
			return null;
		}
		return _keyOrValue.GetType();
	}

	internal virtual void RemoveFromDictionary()
	{
		if (_dictionary != null)
		{
			_dictionary.DeferredResourceReferences.Remove(this);
			_dictionary = null;
		}
	}

	internal virtual void AddInflatedListener(ResourceReferenceExpression listener)
	{
		if (_inflatedList == null)
		{
			_inflatedList = new WeakReferenceList(this);
		}
		_inflatedList.Add(listener);
	}

	internal virtual void RemoveInflatedListener(ResourceReferenceExpression listener)
	{
		if (_inflatedList != null)
		{
			_inflatedList.Remove(listener);
		}
	}
}
