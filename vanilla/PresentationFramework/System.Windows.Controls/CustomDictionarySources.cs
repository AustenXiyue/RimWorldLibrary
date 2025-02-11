using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using MS.Internal.IO.Packaging;

namespace System.Windows.Controls;

internal class CustomDictionarySources : IList<Uri>, ICollection<Uri>, IEnumerable<Uri>, IEnumerable, IList, ICollection
{
	private readonly List<Uri> _uriList;

	private readonly TextBoxBase _owner;

	Uri IList<Uri>.this[int index]
	{
		get
		{
			return _uriList[index];
		}
		set
		{
			ValidateUri(value);
			Uri uri = _uriList[index];
			if (Speller != null)
			{
				Speller.OnDictionaryUriRemoved(uri);
			}
			_uriList[index] = value;
			if (Speller != null)
			{
				Speller.OnDictionaryUriAdded(value);
			}
		}
	}

	int ICollection<Uri>.Count => _uriList.Count;

	bool ICollection<Uri>.IsReadOnly => ((ICollection<Uri>)_uriList).IsReadOnly;

	bool IList.IsFixedSize => ((IList)_uriList).IsFixedSize;

	bool IList.IsReadOnly => ((IList)_uriList).IsReadOnly;

	object IList.this[int index]
	{
		get
		{
			return _uriList[index];
		}
		set
		{
			((IList<Uri>)this)[index] = (Uri)value;
		}
	}

	int ICollection.Count => ((ICollection<Uri>)this).Count;

	bool ICollection.IsSynchronized => ((ICollection)_uriList).IsSynchronized;

	object ICollection.SyncRoot => ((ICollection)_uriList).SyncRoot;

	private Speller Speller
	{
		get
		{
			if (_owner.TextEditor == null)
			{
				return null;
			}
			return _owner.TextEditor.Speller;
		}
	}

	internal CustomDictionarySources(TextBoxBase owner)
	{
		_owner = owner;
		_uriList = new List<Uri>();
	}

	public IEnumerator<Uri> GetEnumerator()
	{
		return _uriList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _uriList.GetEnumerator();
	}

	int IList<Uri>.IndexOf(Uri item)
	{
		return _uriList.IndexOf(item);
	}

	void IList<Uri>.Insert(int index, Uri item)
	{
		if (_uriList.Contains(item))
		{
			throw new ArgumentException(SR.CustomDictionaryItemAlreadyExists, "item");
		}
		ValidateUri(item);
		_uriList.Insert(index, item);
		if (Speller != null)
		{
			Speller.OnDictionaryUriAdded(item);
		}
	}

	void IList<Uri>.RemoveAt(int index)
	{
		Uri uri = _uriList[index];
		_uriList.RemoveAt(index);
		if (Speller != null)
		{
			Speller.OnDictionaryUriRemoved(uri);
		}
	}

	void ICollection<Uri>.Add(Uri item)
	{
		ValidateUri(item);
		if (!_uriList.Contains(item))
		{
			_uriList.Add(item);
		}
		if (Speller != null)
		{
			Speller.OnDictionaryUriAdded(item);
		}
	}

	void ICollection<Uri>.Clear()
	{
		_uriList.Clear();
		if (Speller != null)
		{
			Speller.OnDictionaryUriCollectionCleared();
		}
	}

	bool ICollection<Uri>.Contains(Uri item)
	{
		return _uriList.Contains(item);
	}

	void ICollection<Uri>.CopyTo(Uri[] array, int arrayIndex)
	{
		_uriList.CopyTo(array, arrayIndex);
	}

	bool ICollection<Uri>.Remove(Uri item)
	{
		bool num = _uriList.Remove(item);
		if (num && Speller != null)
		{
			Speller.OnDictionaryUriRemoved(item);
		}
		return num;
	}

	int IList.Add(object value)
	{
		((ICollection<Uri>)this).Add((Uri)value);
		return _uriList.IndexOf((Uri)value);
	}

	void IList.Clear()
	{
		((ICollection<Uri>)this).Clear();
	}

	bool IList.Contains(object value)
	{
		return ((IList)_uriList).Contains(value);
	}

	int IList.IndexOf(object value)
	{
		return ((IList)_uriList).IndexOf(value);
	}

	void IList.Insert(int index, object value)
	{
		((IList<Uri>)this).Insert(index, (Uri)value);
	}

	void IList.Remove(object value)
	{
		((ICollection<Uri>)this).Remove((Uri)value);
	}

	void IList.RemoveAt(int index)
	{
		((IList<Uri>)this).RemoveAt(index);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_uriList).CopyTo(array, index);
	}

	private static void ValidateUri(Uri item)
	{
		if (item == null)
		{
			throw new ArgumentException(SR.CustomDictionaryNullItem);
		}
		if (item.IsAbsoluteUri && !item.IsUnc && !item.IsFile && !PackUriHelper.IsPackUri(item))
		{
			throw new NotSupportedException(SR.CustomDictionarySourcesUnsupportedURI);
		}
	}
}
