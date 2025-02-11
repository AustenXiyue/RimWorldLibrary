using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MS.Internal;

internal static class TraceLevelStore
{
	private struct Key
	{
		private object _element;

		private int _hashcode;

		internal Key(object element, bool useWeakRef)
		{
			_element = new WeakReference(element);
			_hashcode = element.GetHashCode();
		}

		internal Key(object element)
		{
			_element = element;
			_hashcode = element.GetHashCode();
		}

		public override int GetHashCode()
		{
			return _hashcode;
		}

		public override bool Equals(object o)
		{
			if (o is Key key)
			{
				if (_hashcode != key._hashcode)
				{
					return false;
				}
				object obj = ((_element is WeakReference weakReference) ? weakReference.Target : _element);
				object obj2 = ((key._element is WeakReference weakReference2) ? weakReference2.Target : key._element);
				if (obj != null && obj2 != null)
				{
					return obj == obj2;
				}
				return _element == key._element;
			}
			return false;
		}

		public static bool operator ==(Key key1, Key key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(Key key1, Key key2)
		{
			return !key1.Equals(key2);
		}
	}

	private static Dictionary<Key, PresentationTraceLevel> _dictionary = new Dictionary<Key, PresentationTraceLevel>();

	internal static PresentationTraceLevel GetTraceLevel(object element)
	{
		if (element == null || _dictionary.Count == 0)
		{
			return PresentationTraceLevel.None;
		}
		PresentationTraceLevel value;
		lock (_dictionary)
		{
			Key key = new Key(element);
			if (!_dictionary.TryGetValue(key, out value))
			{
				value = PresentationTraceLevel.None;
				return value;
			}
		}
		return value;
	}

	internal static void SetTraceLevel(object element, PresentationTraceLevel traceLevel)
	{
		if (element == null)
		{
			return;
		}
		lock (_dictionary)
		{
			Key key = new Key(element, useWeakRef: true);
			if (traceLevel > PresentationTraceLevel.None)
			{
				_dictionary[key] = traceLevel;
			}
			else
			{
				_dictionary.Remove(key);
			}
		}
	}
}
