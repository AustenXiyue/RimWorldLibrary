using System.Collections;
using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Provides enumeration support for the local values of any dependency properties that exist on a <see cref="T:System.Windows.DependencyObject" />.</summary>
public struct LocalValueEnumerator : IEnumerator
{
	private int _index;

	private LocalValueEntry[] _snapshot;

	private int _count;

	/// <summary>Gets the current element in the collection.</summary>
	/// <returns>The current <see cref="T:System.Windows.LocalValueEntry" /> in the collection.</returns>
	public LocalValueEntry Current
	{
		get
		{
			if (_index == -1)
			{
				throw new InvalidOperationException(SR.LocalValueEnumerationReset);
			}
			if (_index >= Count)
			{
				throw new InvalidOperationException(SR.LocalValueEnumerationOutOfBounds);
			}
			return _snapshot[_index];
		}
	}

	/// <summary>For a description of this members, see <see cref="P:System.Collections.IEnumerator.Current" />.</summary>
	/// <returns>The current element in the collection.</returns>
	object IEnumerator.Current => Current;

	/// <summary>Gets the number of items that are represented in the collection. </summary>
	/// <returns>The number of items in the collection.</returns>
	public int Count => _count;

	/// <summary>Returns a hash code for the current <see cref="T:System.Windows.LocalValueEnumerator" />.</summary>
	/// <returns>A 32-bit integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether the provided <see cref="T:System.Windows.LocalValueEnumerator" /> is equivalent to this <see cref="T:System.Windows.LocalValueEnumerator" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.LocalValueEnumerator" /> is equal to the current <see cref="T:System.Windows.LocalValueEnumerator" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.LocalValueEnumerator" /> to compare with the current <see cref="T:System.Windows.LocalValueEnumerator" />.</param>
	public override bool Equals(object obj)
	{
		if (obj is LocalValueEnumerator localValueEnumerator)
		{
			if (_count == localValueEnumerator._count && _index == localValueEnumerator._index)
			{
				return _snapshot == localValueEnumerator._snapshot;
			}
			return false;
		}
		return false;
	}

	/// <summary>Compares whether two specified <see cref="T:System.Windows.LocalValueEnumerator" /> objects are the same.</summary>
	/// <returns>true if the <paramref name="obj1" /><see cref="T:System.Windows.LocalValueEnumerator" /> is equal to the <paramref name="obj2" /><see cref="T:System.Windows.LocalValueEnumerator" />; otherwise, false.</returns>
	/// <param name="obj1">The first object to compare.</param>
	/// <param name="obj2">The second object to compare.</param>
	public static bool operator ==(LocalValueEnumerator obj1, LocalValueEnumerator obj2)
	{
		return obj1.Equals(obj2);
	}

	/// <summary>Compares two specified <see cref="T:System.Windows.LocalValueEnumerator" /> objects to determine whether they are not the same.</summary>
	/// <returns>true if the instances are not equal; otherwise, false.</returns>
	/// <param name="obj1">The first object to compare.</param>
	/// <param name="obj2">The second object to compare.</param>
	public static bool operator !=(LocalValueEnumerator obj1, LocalValueEnumerator obj2)
	{
		return !(obj1 == obj2);
	}

	/// <summary>Advances the enumerator to the next element of the collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	public bool MoveNext()
	{
		_index++;
		return _index < Count;
	}

	/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
	public void Reset()
	{
		_index = -1;
	}

	internal LocalValueEnumerator(LocalValueEntry[] snapshot, int count)
	{
		_index = -1;
		_count = count;
		_snapshot = snapshot;
	}
}
