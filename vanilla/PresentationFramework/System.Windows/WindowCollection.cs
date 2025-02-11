using System.Collections;

namespace System.Windows;

/// <summary>Represents a collection of <see cref="T:System.Windows.Window" /> objects. This class cannot be inherited.</summary>
public sealed class WindowCollection : ICollection, IEnumerable
{
	private ArrayList _list;

	/// <summary>Gets the <see cref="T:System.Windows.Window" /> or <see cref="T:System.Windows.Navigation.NavigationWindow" /> object at the specified index.</summary>
	/// <returns>A <see cref="T:System.Windows.Window" /> or <see cref="T:System.Windows.Navigation.NavigationWindow" /> object.</returns>
	/// <param name="index">The index of the specified <see cref="T:System.Windows.Window" /> or <see cref="T:System.Windows.Navigation.NavigationWindow" />.</param>
	public Window this[int index] => _list[index] as Window;

	/// <summary>Gets the number of <see cref="T:System.Windows.Window" /> objects contained in the <see cref="T:System.Windows.WindowCollection" />.</summary>
	/// <returns>The number of <see cref="T:System.Windows.Window" /> objects contained in the <see cref="T:System.Windows.WindowCollection" />.</returns>
	public int Count => _list.Count;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Windows.WindowCollection" /> object is thread safe. </summary>
	/// <returns>true if the object is thread safe; otherwise, false.</returns>
	public bool IsSynchronized => _list.IsSynchronized;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot => _list.SyncRoot;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.WindowCollection" /> class.</summary>
	public WindowCollection()
	{
		_list = new ArrayList(1);
	}

	internal WindowCollection(int count)
	{
		_list = new ArrayList(count);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that you can use to enumerate the <see cref="T:System.Windows.Window" /> objects in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that you can use to enumerate the <see cref="T:System.Windows.Window" /> objects in the collection.</returns>
	public IEnumerator GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
	/// <param name="array">A zero-based <see cref="T:System.Array" /> that receives the copied items from the <see cref="T:System.Windows.WindowCollection" />.</param>
	/// <param name="index">The first position in the specified <see cref="T:System.Array" /> to receive the copied contents.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		_list.CopyTo(array, index);
	}

	/// <summary>Copies each <see cref="T:System.Windows.Window" /> object in the collection to an array, starting from the specified index.</summary>
	/// <param name="array">An array of type <see cref="T:System.Windows.Window" /> that the <see cref="T:System.Windows.Window" /> objects in the collection are copied to.</param>
	/// <param name="index">The position in the collection to start copying from.</param>
	public void CopyTo(Window[] array, int index)
	{
		_list.CopyTo(array, index);
	}

	internal WindowCollection Clone()
	{
		lock (_list.SyncRoot)
		{
			WindowCollection windowCollection = new WindowCollection(_list.Count);
			for (int i = 0; i < _list.Count; i++)
			{
				windowCollection._list.Add(_list[i]);
			}
			return windowCollection;
		}
	}

	internal void Remove(Window win)
	{
		lock (_list.SyncRoot)
		{
			_list.Remove(win);
		}
	}

	internal void RemoveAt(int index)
	{
		lock (_list.SyncRoot)
		{
			_list.Remove(index);
		}
	}

	internal int Add(Window win)
	{
		lock (_list.SyncRoot)
		{
			return _list.Add(win);
		}
	}

	internal bool HasItem(Window win)
	{
		lock (_list.SyncRoot)
		{
			for (int i = 0; i < _list.Count; i++)
			{
				if (_list[i] == win)
				{
					return true;
				}
			}
		}
		return false;
	}
}
