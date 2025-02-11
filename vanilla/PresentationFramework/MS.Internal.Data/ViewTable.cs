using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class ViewTable : HybridDictionary
{
	internal ViewRecord this[CollectionViewSource cvs]
	{
		get
		{
			return (ViewRecord)base[new WeakRefKey(cvs)];
		}
		set
		{
			base[new WeakRefKey(cvs)] = value;
		}
	}

	internal bool Purge()
	{
		ArrayList arrayList = new ArrayList();
		IDictionaryEnumerator dictionaryEnumerator = GetEnumerator();
		try
		{
			while (dictionaryEnumerator.MoveNext())
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)dictionaryEnumerator.Current;
				WeakRefKey weakRefKey = (WeakRefKey)dictionaryEntry.Key;
				if (weakRefKey.Target != null)
				{
					continue;
				}
				if (((ViewRecord)dictionaryEntry.Value).View is CollectionView collectionView)
				{
					if (!collectionView.IsInUse)
					{
						collectionView.DetachFromSourceCollection();
						arrayList.Add(weakRefKey);
					}
				}
				else
				{
					arrayList.Add(weakRefKey);
				}
			}
		}
		finally
		{
			IDisposable disposable = dictionaryEnumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			Remove(arrayList[i]);
		}
		if (arrayList.Count <= 0)
		{
			return base.Count == 0;
		}
		return true;
	}
}
