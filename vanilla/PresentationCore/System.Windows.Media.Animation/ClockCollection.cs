using System.Collections;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.Media.Animation.Clock" /> objects. </summary>
public class ClockCollection : ICollection<Clock>, IEnumerable<Clock>, IEnumerable
{
	internal struct ClockEnumerator : IEnumerator<Clock>, IEnumerator, IDisposable
	{
		private Clock _owner;

		Clock IEnumerator<Clock>.Current
		{
			get
			{
				throw new InvalidOperationException(SR.Timing_EnumeratorOutOfRange);
			}
		}

		object IEnumerator.Current => ((IEnumerator<Clock>)this).Current;

		internal ClockEnumerator(Clock owner)
		{
			_owner = owner;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		public bool MoveNext()
		{
			if (_owner is ClockGroup { InternalChildren: not null })
			{
				throw new InvalidOperationException(SR.Timing_EnumeratorInvalidated);
			}
			return false;
		}
	}

	private Clock _owner;

	/// <summary>Gets the number of items contained in this <see cref="T:System.Windows.Media.Animation.ClockCollection" />. </summary>
	/// <returns>The number of items contained in this instance. </returns>
	public int Count
	{
		get
		{
			if (_owner is ClockGroup { InternalChildren: { } internalChildren })
			{
				return internalChildren.Count;
			}
			return 0;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Windows.Media.Animation.ClockCollection" /> is read-only.</summary>
	/// <returns>true if this instance is read-only; otherwise false.</returns>
	public bool IsReadOnly => true;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Animation.Clock" /> at the specified index position. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.Clock" /> object at the specified <paramref name="index" /> position.</returns>
	/// <param name="index">The index position to access.</param>
	public Clock this[int index]
	{
		get
		{
			List<Clock> list = null;
			if (_owner is ClockGroup clockGroup)
			{
				list = clockGroup.InternalChildren;
			}
			if (list == null)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return list[index];
		}
	}

	/// <summary>Removes all items from this <see cref="T:System.Windows.Media.Animation.ClockCollection" />.</summary>
	public void Clear()
	{
		throw new NotSupportedException();
	}

	/// <summary>Adds a new <see cref="T:System.Windows.Media.Animation.Clock" /> object to the end of this <see cref="T:System.Windows.Media.Animation.ClockCollection" />.</summary>
	/// <param name="item">The object to add.</param>
	public void Add(Clock item)
	{
		throw new NotSupportedException();
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Media.Animation.Clock" /> from the <see cref="T:System.Windows.Media.Animation.ClockCollection" />.</summary>
	/// <returns>true if <paramref name="item" /> was successfully removed; otherwise, false.</returns>
	/// <param name="item">The object to remove.</param>
	public bool Remove(Clock item)
	{
		throw new NotSupportedException();
	}

	/// <summary>Indicates whether the <see cref="T:System.Windows.Media.Animation.ClockCollection" /> contains the specified <see cref="T:System.Windows.Media.Animation.Clock" /> object.</summary>
	/// <returns>true if <paramref name="item" /> is found; otherwise, false.</returns>
	/// <param name="item">The object to locate.</param>
	public bool Contains(Clock item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		foreach (Clock item2 in (IEnumerable<Clock>)this)
		{
			if (item2.Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Copies the <see cref="T:System.Windows.Media.Animation.Clock" /> objects of this <see cref="T:System.Windows.Media.Animation.ClockCollection" /> to an array of Clocks, starting at the specified index position. </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="index">The zero-based index position where copying begins.</param>
	public void CopyTo(Clock[] array, int index)
	{
		if (_owner is ClockGroup { InternalChildren: { } internalChildren })
		{
			internalChildren.CopyTo(array, index);
		}
	}

	IEnumerator<Clock> IEnumerable<Clock>.GetEnumerator()
	{
		List<Clock> list = null;
		if (_owner is ClockGroup clockGroup)
		{
			list = clockGroup.InternalChildren;
		}
		if (list != null)
		{
			return list.GetEnumerator();
		}
		return new ClockEnumerator(_owner);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new ClockEnumerator(_owner);
	}

	/// <summary>Indicates whether this instance is equal to the specified object. </summary>
	/// <returns>true if <paramref name="obj" /> is equal to this instance; otherwise false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		if (obj is ClockCollection)
		{
			return this == (ClockCollection)obj;
		}
		return false;
	}

	/// <summary>Indicates whether the two specified <see cref="T:System.Windows.Media.Animation.ClockCollection" /> collections are equal.</summary>
	/// <returns>true if <paramref name="objA" /> and <paramref name="objB" /> are equal; otherwise, false.</returns>
	/// <param name="objA">The first value to compare.</param>
	/// <param name="objB">The second value to compare.</param>
	public static bool Equals(ClockCollection objA, ClockCollection objB)
	{
		return objA == objB;
	}

	/// <summary>Overloaded operator that compares two <see cref="T:System.Windows.Media.Animation.ClockCollection" /> collections for equality.</summary>
	/// <returns>true if <paramref name="objA" /> and <paramref name="objB" /> are equal; otherwise false.</returns>
	/// <param name="objA">The first object to compare.</param>
	/// <param name="objB">The second object to compare.</param>
	public static bool operator ==(ClockCollection objA, ClockCollection objB)
	{
		if ((object)objA == objB)
		{
			return true;
		}
		if ((object)objA == null || (object)objB == null)
		{
			return false;
		}
		return objA._owner == objB._owner;
	}

	/// <summary>Overloaded operator that compares two <see cref="T:System.Windows.Media.Animation.ClockCollection" /> collections for inequality.</summary>
	/// <returns>true if <paramref name="objA" /> and <paramref name="objB" /> are not equal; otherwise false.</returns>
	/// <param name="objA">The first object to compare.</param>
	/// <param name="objB">The second object to compare.</param>
	public static bool operator !=(ClockCollection objA, ClockCollection objB)
	{
		return !(objA == objB);
	}

	/// <summary>Returns a 32-bit signed integer hash code representing this instance.</summary>
	/// <returns>A 32-bit signed integer.</returns>
	public override int GetHashCode()
	{
		return _owner.GetHashCode();
	}

	internal ClockCollection(Clock owner)
	{
		_owner = owner;
	}

	private ClockCollection()
	{
	}
}
