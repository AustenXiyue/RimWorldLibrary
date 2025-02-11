namespace System.Windows.Documents;

internal static class TextElementCollectionHelper
{
	private class ParentCollectionPair
	{
		private readonly DependencyObject _parent;

		private readonly object _collection;

		internal DependencyObject Parent => _parent;

		internal object Collection => _collection;

		internal ParentCollectionPair(DependencyObject parent, object collection)
		{
			_parent = parent;
			_collection = collection;
		}
	}

	private static readonly WeakReference[] _cleanParentList = new WeakReference[10];

	internal static void MarkDirty(DependencyObject parent)
	{
		if (parent == null)
		{
			return;
		}
		lock (_cleanParentList)
		{
			for (int i = 0; i < _cleanParentList.Length; i++)
			{
				if (_cleanParentList[i] != null)
				{
					ParentCollectionPair parentCollectionPair = (ParentCollectionPair)_cleanParentList[i].Target;
					if (parentCollectionPair == null || parentCollectionPair.Parent == parent)
					{
						_cleanParentList[i] = null;
					}
				}
			}
		}
	}

	internal static void MarkClean(DependencyObject parent, object collection)
	{
		lock (_cleanParentList)
		{
			int firstFreeIndex;
			int num = GetCleanParentIndex(parent, collection, out firstFreeIndex);
			if (num == -1)
			{
				num = ((firstFreeIndex >= 0) ? firstFreeIndex : (_cleanParentList.Length - 1));
				_cleanParentList[num] = new WeakReference(new ParentCollectionPair(parent, collection));
			}
			TouchCleanParent(num);
		}
	}

	internal static bool IsCleanParent(DependencyObject parent, object collection)
	{
		int num = -1;
		lock (_cleanParentList)
		{
			num = GetCleanParentIndex(parent, collection, out var _);
			if (num >= 0)
			{
				TouchCleanParent(num);
			}
		}
		return num >= 0;
	}

	private static void TouchCleanParent(int index)
	{
		WeakReference weakReference = _cleanParentList[index];
		Array.Copy(_cleanParentList, 0, _cleanParentList, 1, index);
		_cleanParentList[0] = weakReference;
	}

	private static int GetCleanParentIndex(DependencyObject parent, object collection, out int firstFreeIndex)
	{
		int result = -1;
		firstFreeIndex = -1;
		for (int i = 0; i < _cleanParentList.Length; i++)
		{
			if (_cleanParentList[i] == null)
			{
				if (firstFreeIndex == -1)
				{
					firstFreeIndex = i;
				}
				continue;
			}
			ParentCollectionPair parentCollectionPair = (ParentCollectionPair)_cleanParentList[i].Target;
			if (parentCollectionPair == null)
			{
				_cleanParentList[i] = null;
				if (firstFreeIndex == -1)
				{
					firstFreeIndex = i;
				}
			}
			else if (parentCollectionPair.Parent == parent && parentCollectionPair.Collection == collection)
			{
				result = i;
			}
		}
		return result;
	}
}
