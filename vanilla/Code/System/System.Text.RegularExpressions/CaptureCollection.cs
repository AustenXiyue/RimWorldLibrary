using System.Collections;
using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Represents the set of captures made by a single capturing group. </summary>
[Serializable]
public class CaptureCollection : ICollection, IEnumerable
{
	internal Group _group;

	internal int _capcount;

	internal Capture[] _captures;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot => _group;

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread-safe).</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets a value that indicates whether the collection is read only.</summary>
	/// <returns>true in all cases.</returns>
	public bool IsReadOnly => true;

	/// <summary>Gets the number of substrings captured by the group.</summary>
	/// <returns>The number of items in the <see cref="T:System.Text.RegularExpressions.CaptureCollection" />.</returns>
	public int Count => _capcount;

	/// <summary>Gets an individual member of the collection.</summary>
	/// <returns>The captured substring at position <paramref name="i" /> in the collection.</returns>
	/// <param name="i">Index into the capture collection. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="i" /> is less than 0 or greater than <see cref="P:System.Text.RegularExpressions.CaptureCollection.Count" />. </exception>
	public Capture this[int i] => GetCapture(i);

	internal CaptureCollection(Group group)
	{
		_group = group;
		_capcount = _group._capcount;
	}

	/// <summary>Copies all the elements of the collection to the given array beginning at the given index.</summary>
	/// <param name="array">The array the collection is to be copied into. </param>
	/// <param name="arrayIndex">The position in the destination array where copying is to begin. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array " />is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is outside the bounds of <paramref name="array" />. -or-<paramref name="arrayIndex" /> plus <see cref="P:System.Text.RegularExpressions.CaptureCollection.Count" /> is outside the bounds of <paramref name="array" />. </exception>
	public void CopyTo(Array array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = arrayIndex;
		for (int i = 0; i < Count; i++)
		{
			array.SetValue(this[i], num);
			num++;
		}
	}

	/// <summary>Provides an enumerator that iterates through the collection.</summary>
	/// <returns>An object that contains all <see cref="T:System.Text.RegularExpressions.Capture" /> objects within the <see cref="T:System.Text.RegularExpressions.CaptureCollection" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return new CaptureEnumerator(this);
	}

	internal Capture GetCapture(int i)
	{
		if (i == _capcount - 1 && i >= 0)
		{
			return _group;
		}
		if (i >= _capcount || i < 0)
		{
			throw new ArgumentOutOfRangeException("i");
		}
		if (_captures == null)
		{
			_captures = new Capture[_capcount];
			for (int j = 0; j < _capcount - 1; j++)
			{
				_captures[j] = new Capture(_group._text, _group._caps[j * 2], _group._caps[j * 2 + 1]);
			}
		}
		return _captures[i];
	}

	internal CaptureCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
