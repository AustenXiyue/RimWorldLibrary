using System.Collections;
using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Returns the set of captured groups in a single match.</summary>
[Serializable]
public class GroupCollection : ICollection, IEnumerable
{
	internal Match _match;

	internal Hashtable _captureMap;

	internal Group[] _groups;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Text.RegularExpressions.GroupCollection" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Text.RegularExpressions.Match" /> object to synchronize.</returns>
	public object SyncRoot => _match;

	/// <summary>Gets a value that indicates whether access to the <see cref="T:System.Text.RegularExpressions.GroupCollection" /> is synchronized (thread-safe).</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets a value that indicates whether the collection is read-only.</summary>
	/// <returns>true in all cases.</returns>
	public bool IsReadOnly => true;

	/// <summary>Returns the number of groups in the collection.</summary>
	/// <returns>The number of groups in the collection.</returns>
	public int Count => _match._matchcount.Length;

	/// <summary>Enables access to a member of the collection by integer index.</summary>
	/// <returns>The member of the collection specified by <paramref name="groupnum" />.</returns>
	/// <param name="groupnum">The zero-based index of the collection member to be retrieved. </param>
	public Group this[int groupnum] => GetGroup(groupnum);

	/// <summary>Enables access to a member of the collection by string index.</summary>
	/// <returns>The member of the collection specified by <paramref name="groupname" />.</returns>
	/// <param name="groupname">The name of a capturing group. </param>
	public Group this[string groupname]
	{
		get
		{
			if (_match._regex == null)
			{
				return Group._emptygroup;
			}
			return GetGroup(_match._regex.GroupNumberFromName(groupname));
		}
	}

	internal GroupCollection(Match match, Hashtable caps)
	{
		_match = match;
		_captureMap = caps;
	}

	internal Group GetGroup(int groupnum)
	{
		if (_captureMap != null)
		{
			object obj = _captureMap[groupnum];
			if (obj == null)
			{
				return Group._emptygroup;
			}
			return GetGroupImpl((int)obj);
		}
		if (groupnum >= _match._matchcount.Length || groupnum < 0)
		{
			return Group._emptygroup;
		}
		return GetGroupImpl(groupnum);
	}

	internal Group GetGroupImpl(int groupnum)
	{
		if (groupnum == 0)
		{
			return _match;
		}
		if (_groups == null)
		{
			_groups = new Group[_match._matchcount.Length - 1];
			for (int i = 0; i < _groups.Length; i++)
			{
				string name = _match._regex.GroupNameFromNumber(i + 1);
				_groups[i] = new Group(_match._text, _match._matches[i + 1], _match._matchcount[i + 1], name);
			}
		}
		return _groups[groupnum - 1];
	}

	/// <summary>Copies all the elements of the collection to the given array beginning at the given index.</summary>
	/// <param name="array">The array the collection is to be copied into. </param>
	/// <param name="arrayIndex">The position in the destination array where the copying is to begin. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="arrayIndex" /> is outside the bounds of <paramref name="array" />.-or-<paramref name="arrayIndex" /> plus <see cref="P:System.Text.RegularExpressions.GroupCollection.Count" /> is outside the bounds of <paramref name="array" />.</exception>
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
	/// <returns>An enumerator that contains all <see cref="T:System.Text.RegularExpressions.Group" /> objects in the <see cref="T:System.Text.RegularExpressions.GroupCollection" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return new GroupEnumerator(this);
	}

	internal GroupCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
