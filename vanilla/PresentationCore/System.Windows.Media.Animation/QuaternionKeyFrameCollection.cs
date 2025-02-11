using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Media.Animation;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> objects. </summary>
public class QuaternionKeyFrameCollection : Freezable, IList, ICollection, IEnumerable
{
	private List<QuaternionKeyFrame> _keyFrames;

	private static QuaternionKeyFrameCollection s_emptyCollection;

	/// <summary> Gets an empty <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />.  </summary>
	/// <returns>An empty <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />.</returns>
	public static QuaternionKeyFrameCollection Empty
	{
		get
		{
			if (s_emptyCollection == null)
			{
				QuaternionKeyFrameCollection quaternionKeyFrameCollection = new QuaternionKeyFrameCollection();
				quaternionKeyFrameCollection._keyFrames = new List<QuaternionKeyFrame>(0);
				quaternionKeyFrameCollection.Freeze();
				s_emptyCollection = quaternionKeyFrameCollection;
			}
			return s_emptyCollection;
		}
	}

	/// <summary>Gets the number of key frames contained in the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />.</summary>
	/// <returns>The number of key frames contained in the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />. </returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _keyFrames.Count;
		}
	}

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread-safe). </summary>
	/// <returns>true if access to the collection is synchronized (thread-safe); otherwise, false.</returns>
	public bool IsSynchronized
	{
		get
		{
			ReadPreamble();
			if (!base.IsFrozen)
			{
				return base.Dispatcher != null;
			}
			return true;
		}
	}

	/// <summary> Gets an object that can be used to synchronize access to the collection. </summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot
	{
		get
		{
			ReadPreamble();
			return ((ICollection)_keyFrames).SyncRoot;
		}
	}

	/// <summary>Gets a value that indicates if the collection size can ever change.</summary>
	/// <returns>true if the collection is frozen; otherwise, false.</returns>
	public bool IsFixedSize
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary> Gets a value that indicates if the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	public bool IsReadOnly
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (QuaternionKeyFrame)value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> at the specified index position.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to get or set.</param>
	/// <exception cref="T:System.InvalidOperationException">The attempt to modify the collection is invalid because the collection is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.Media.Animation.QuaternionKeyFrameCollection.Count" />.</exception>
	public QuaternionKeyFrame this[int index]
	{
		get
		{
			ReadPreamble();
			return _keyFrames[index];
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "QuaternionKeyFrameCollection[{0}]", index));
			}
			WritePreamble();
			if (value != _keyFrames[index])
			{
				OnFreezablePropertyChanged(_keyFrames[index], value);
				_keyFrames[index] = value;
				WritePostscript();
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> class.</summary>
	public QuaternionKeyFrameCollection()
	{
		_keyFrames = new List<QuaternionKeyFrame>(2);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new QuaternionKeyFrameCollection Clone()
	{
		return (QuaternionKeyFrameCollection)base.Clone();
	}

	/// <summary>Creates a new, frozen instance of <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />.</summary>
	/// <returns>A frozen instance of <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new QuaternionKeyFrameCollection();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		QuaternionKeyFrameCollection quaternionKeyFrameCollection = (QuaternionKeyFrameCollection)sourceFreezable;
		base.CloneCore(sourceFreezable);
		int count = quaternionKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<QuaternionKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			QuaternionKeyFrame quaternionKeyFrame = (QuaternionKeyFrame)quaternionKeyFrameCollection._keyFrames[i].Clone();
			_keyFrames.Add(quaternionKeyFrame);
			OnFreezablePropertyChanged(null, quaternionKeyFrame);
		}
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		QuaternionKeyFrameCollection quaternionKeyFrameCollection = (QuaternionKeyFrameCollection)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		int count = quaternionKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<QuaternionKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			QuaternionKeyFrame quaternionKeyFrame = (QuaternionKeyFrame)quaternionKeyFrameCollection._keyFrames[i].CloneCurrentValue();
			_keyFrames.Add(quaternionKeyFrame);
			OnFreezablePropertyChanged(null, quaternionKeyFrame);
		}
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> object. </summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> object to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		QuaternionKeyFrameCollection quaternionKeyFrameCollection = (QuaternionKeyFrameCollection)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		int count = quaternionKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<QuaternionKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			QuaternionKeyFrame quaternionKeyFrame = (QuaternionKeyFrame)quaternionKeyFrameCollection._keyFrames[i].GetAsFrozen();
			_keyFrames.Add(quaternionKeyFrame);
			OnFreezablePropertyChanged(null, quaternionKeyFrame);
		}
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		QuaternionKeyFrameCollection quaternionKeyFrameCollection = (QuaternionKeyFrameCollection)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		int count = quaternionKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<QuaternionKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			QuaternionKeyFrame quaternionKeyFrame = (QuaternionKeyFrame)quaternionKeyFrameCollection._keyFrames[i].GetCurrentValueAsFrozen();
			_keyFrames.Add(quaternionKeyFrame);
			OnFreezablePropertyChanged(null, quaternionKeyFrame);
		}
	}

	/// <summary>Makes this instance of <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrameCollection" /> read-only or determines whether it can be made read-only.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if this instance can be made read-only, or false if it cannot be made read-only. If <paramref name="isChecking" /> is false, this method returns true if this instance is now read-only, or false if it cannot be made read-only, with the side effect of having begun to change the frozen status of this object.</returns>
	/// <param name="isChecking">true to check if this instance can be frozen; false to freeze this instance. </param>
	protected override bool FreezeCore(bool isChecking)
	{
		bool flag = base.FreezeCore(isChecking);
		for (int i = 0; i < _keyFrames.Count && flag; i++)
		{
			flag &= Freezable.Freeze(_keyFrames[i], isChecking);
		}
		return flag;
	}

	/// <summary> Returns an enumerator that can iterate through the collection. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can iterate through the collection.</returns>
	public IEnumerator GetEnumerator()
	{
		ReadPreamble();
		return _keyFrames.GetEnumerator();
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	void ICollection.CopyTo(Array array, int index)
	{
		ReadPreamble();
		((ICollection)_keyFrames).CopyTo(array, index);
	}

	/// <summary>Copies all of the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> objects in a collection to a specified array. </summary>
	/// <param name="array">Identifies the array to which content is copied.</param>
	/// <param name="index">Index position in the array to which the contents of the collection are copied.</param>
	public void CopyTo(QuaternionKeyFrame[] array, int index)
	{
		ReadPreamble();
		_keyFrames.CopyTo(array, index);
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
	/// <param name="keyFrame">The object to add to the <see cref="T:System.Collections.IList" />.</param>
	int IList.Add(object keyFrame)
	{
		return Add((QuaternionKeyFrame)keyFrame);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to the end of the collection. </summary>
	/// <returns>The index at which the <paramref name="keyFrame" /> was added.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to add to the end of the collection.</param>
	public int Add(QuaternionKeyFrame keyFrame)
	{
		if (keyFrame == null)
		{
			throw new ArgumentNullException("keyFrame");
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, keyFrame);
		_keyFrames.Add(keyFrame);
		WritePostscript();
		return _keyFrames.Count - 1;
	}

	/// <summary>Removes all <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> objects from the collection. </summary>
	public void Clear()
	{
		WritePreamble();
		if (_keyFrames.Count > 0)
		{
			for (int i = 0; i < _keyFrames.Count; i++)
			{
				OnFreezablePropertyChanged(_keyFrames[i], null);
			}
			_keyFrames.Clear();
			WritePostscript();
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="keyFrame">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
	bool IList.Contains(object keyFrame)
	{
		return Contains((QuaternionKeyFrame)keyFrame);
	}

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" />. </summary>
	/// <returns>true if the collection contains <paramref name="keyFrame" />; otherwise, false.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to locate in the collection.</param>
	public bool Contains(QuaternionKeyFrame keyFrame)
	{
		ReadPreamble();
		return _keyFrames.Contains(keyFrame);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="keyFrame">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
	int IList.IndexOf(object keyFrame)
	{
		return IndexOf((QuaternionKeyFrame)keyFrame);
	}

	/// <summary> Searches for the specified <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> and returns the zero-based index of the first occurrence within the entire collection.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="keyFrame" /> within the entire collection, if found; otherwise, -1.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to locate in the collection.</param>
	public int IndexOf(QuaternionKeyFrame keyFrame)
	{
		ReadPreamble();
		return _keyFrames.IndexOf(keyFrame);
	}

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
	/// <param name="keyFrame">The object to insert into the <see cref="T:System.Collections.IList" />. </param>
	void IList.Insert(int index, object keyFrame)
	{
		Insert(index, (QuaternionKeyFrame)keyFrame);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> into a specific location within the collection. </summary>
	/// <param name="index">The index position at which the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> is inserted.</param>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> object to insert in the collection.</param>
	public void Insert(int index, QuaternionKeyFrame keyFrame)
	{
		if (keyFrame == null)
		{
			throw new ArgumentNullException("keyFrame");
		}
		WritePreamble();
		OnFreezablePropertyChanged(null, keyFrame);
		_keyFrames.Insert(index, keyFrame);
		WritePostscript();
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="keyFrame">The object to remove from the <see cref="T:System.Collections.IList" />.</param>
	void IList.Remove(object keyFrame)
	{
		Remove((QuaternionKeyFrame)keyFrame);
	}

	/// <summary>Removes a <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> object from the collection. </summary>
	/// <param name="keyFrame">Identifies the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to remove from the collection.</param>
	public void Remove(QuaternionKeyFrame keyFrame)
	{
		WritePreamble();
		if (_keyFrames.Contains(keyFrame))
		{
			OnFreezablePropertyChanged(keyFrame, null);
			_keyFrames.Remove(keyFrame);
			WritePostscript();
		}
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> at the specified index position from the collection. </summary>
	/// <param name="index">Index position of the <see cref="T:System.Windows.Media.Animation.QuaternionKeyFrame" /> to be removed.</param>
	public void RemoveAt(int index)
	{
		WritePreamble();
		OnFreezablePropertyChanged(_keyFrames[index], null);
		_keyFrames.RemoveAt(index);
		WritePostscript();
	}
}
