using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Contains the <see cref="T:System.Windows.Input.TabletDevice" /> objects that represent the digitizer devices of a tablet device.</summary>
public class TabletDeviceCollection : ICollection, IEnumerable
{
	private static TabletDeviceCollection _emptyTabletDeviceCollection;

	/// <summary>Gets the number of <see cref="T:System.Windows.Input.TabletDevice" /> objects in the collection.</summary>
	/// <returns>The number of <see cref="T:System.Windows.Input.TabletDevice" /> objects in the collection.</returns>
	public int Count
	{
		get
		{
			if (TabletDevices == null)
			{
				throw new ObjectDisposedException("TabletDeviceCollection");
			}
			return TabletDevices.Count;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.TabletDevice" /> object at the specified index within the collection.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.TabletDevice" /> object at the specified index within the collection.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Input.TabletDevice" /> that is retrieved from the collection.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">If <paramref name="index" /> is less than 0 or <paramref name="index" /> is greater than or equal to the number of <see cref="T:System.Windows.Input.TabletDeviceCollection" /> objects in the collection.</exception>
	public TabletDevice this[int index]
	{
		get
		{
			if (index >= Count || index < 0)
			{
				throw new ArgumentException(SR.Format(SR.Stylus_IndexOutOfRange, index.ToString(CultureInfo.InvariantCulture)), "index");
			}
			return TabletDevices[index];
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot => this;

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the collection is synchronized (thread safe); otherwise, false. The default is false.</returns>
	public bool IsSynchronized => false;

	internal List<TabletDevice> TabletDevices { get; set; } = new List<TabletDevice>();

	internal static TabletDeviceCollection EmptyTabletDeviceCollection
	{
		get
		{
			if (_emptyTabletDeviceCollection == null)
			{
				_emptyTabletDeviceCollection = new TabletDeviceCollection();
			}
			return _emptyTabletDeviceCollection;
		}
	}

	internal T As<T>() where T : TabletDeviceCollection
	{
		return this as T;
	}

	/// <summary>This member supports the .NET Framework and is not intended to be used from your code.</summary>
	/// <param name="array">The array.</param>
	/// <param name="index">The index.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		Array.Copy(TabletDevices.ToArray(), 0, array, index, Count);
	}

	/// <summary>Copies all elements in the current collection to the specified one-dimensional array, starting at the specified destination array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of elements copied from the collection. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in the array parameter where copying begins.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="index" /> + collection count is greater than or equal to <paramref name="array.length" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">If <paramref name="index" /> is less than 0 or <paramref name="index" /> is greater than or equal to <paramref name="array.length" />.</exception>
	public void CopyTo(TabletDevice[] array, int index)
	{
		TabletDevices.CopyTo(array, index);
	}

	/// <summary>This member supports the .NET Framework and is not intended to be used from your code.</summary>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return TabletDevices.GetEnumerator();
	}
}
