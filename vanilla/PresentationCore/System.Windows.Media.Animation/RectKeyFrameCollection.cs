using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Media.Animation;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> objects. </summary>
public class RectKeyFrameCollection : Freezable, IList, ICollection, IEnumerable
{
	private List<RectKeyFrame> _keyFrames;

	private static RectKeyFrameCollection s_emptyCollection;

	/// <summary> Gets an empty <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />.  </summary>
	/// <returns>An empty <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />.</returns>
	public static RectKeyFrameCollection Empty
	{
		get
		{
			if (s_emptyCollection == null)
			{
				RectKeyFrameCollection rectKeyFrameCollection = new RectKeyFrameCollection();
				rectKeyFrameCollection._keyFrames = new List<RectKeyFrame>(0);
				rectKeyFrameCollection.Freeze();
				s_emptyCollection = rectKeyFrameCollection;
			}
			return s_emptyCollection;
		}
	}

	/// <summary>Gets the number of key frames contained in the <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />.</summary>
	/// <returns>The number of key frames contained in the <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />. </returns>
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
			this[index] = (RectKeyFrame)value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> at the specified index position.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to get or set.</param>
	/// <exception cref="T:System.InvalidOperationException">The attempt to modify the collection is invalid because the collection is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.Media.Animation.RectKeyFrameCollection.Count" />.</exception>
	public RectKeyFrame this[int index]
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
				throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "RectKeyFrameCollection[{0}]", index));
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> class.</summary>
	public RectKeyFrameCollection()
	{
		_keyFrames = new List<RectKeyFrame>(2);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RectKeyFrameCollection Clone()
	{
		return (RectKeyFrameCollection)base.Clone();
	}

	/// <summary>Creates a new, frozen instance of <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />.</summary>
	/// <returns>A frozen instance of <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new RectKeyFrameCollection();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		RectKeyFrameCollection rectKeyFrameCollection = (RectKeyFrameCollection)sourceFreezable;
		base.CloneCore(sourceFreezable);
		int count = rectKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<RectKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			RectKeyFrame rectKeyFrame = (RectKeyFrame)rectKeyFrameCollection._keyFrames[i].Clone();
			_keyFrames.Add(rectKeyFrame);
			OnFreezablePropertyChanged(null, rectKeyFrame);
		}
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		RectKeyFrameCollection rectKeyFrameCollection = (RectKeyFrameCollection)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		int count = rectKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<RectKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			RectKeyFrame rectKeyFrame = (RectKeyFrame)rectKeyFrameCollection._keyFrames[i].CloneCurrentValue();
			_keyFrames.Add(rectKeyFrame);
			OnFreezablePropertyChanged(null, rectKeyFrame);
		}
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> object. </summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> object to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		RectKeyFrameCollection rectKeyFrameCollection = (RectKeyFrameCollection)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		int count = rectKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<RectKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			RectKeyFrame rectKeyFrame = (RectKeyFrame)rectKeyFrameCollection._keyFrames[i].GetAsFrozen();
			_keyFrames.Add(rectKeyFrame);
			OnFreezablePropertyChanged(null, rectKeyFrame);
		}
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		RectKeyFrameCollection rectKeyFrameCollection = (RectKeyFrameCollection)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		int count = rectKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<RectKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			RectKeyFrame rectKeyFrame = (RectKeyFrame)rectKeyFrameCollection._keyFrames[i].GetCurrentValueAsFrozen();
			_keyFrames.Add(rectKeyFrame);
			OnFreezablePropertyChanged(null, rectKeyFrame);
		}
	}

	/// <summary>Makes this instance of <see cref="T:System.Windows.Media.Animation.RectKeyFrameCollection" /> unmodifiable or determines whether it can be made unmodifiable.</summary>
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

	/// <summary>Copies all of the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> objects in a collection to a specified array. </summary>
	/// <param name="array">Identifies the array to which content is copied.</param>
	/// <param name="index">Index position in the array to which the contents of the collection are copied.</param>
	public void CopyTo(RectKeyFrame[] array, int index)
	{
		ReadPreamble();
		_keyFrames.CopyTo(array, index);
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
	/// <param name="keyFrame">The object to add to the <see cref="T:System.Collections.IList" />. </param>
	int IList.Add(object keyFrame)
	{
		return Add((RectKeyFrame)keyFrame);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to the end of the collection. </summary>
	/// <returns>The index at which the <paramref name="keyFrame" /> was added.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to add to the end of the collection.</param>
	public int Add(RectKeyFrame keyFrame)
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

	/// <summary>Removes all <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> objects from the collection. </summary>
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
	/// <param name="keyFrame">The object to locate in the <see cref="T:System.Collections.IList" />.    </param>
	bool IList.Contains(object keyFrame)
	{
		return Contains((RectKeyFrame)keyFrame);
	}

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.Windows.Media.Animation.RectKeyFrame" />. </summary>
	/// <returns>true if the collection contains <paramref name="keyFrame" />; otherwise, false.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to locate in the collection.</param>
	public bool Contains(RectKeyFrame keyFrame)
	{
		ReadPreamble();
		return _keyFrames.Contains(keyFrame);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="keyFrame">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
	int IList.IndexOf(object keyFrame)
	{
		return IndexOf((RectKeyFrame)keyFrame);
	}

	/// <summary> Searches for the specified <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> and returns the zero-based index of the first occurrence within the entire collection.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="keyFrame" /> within the entire collection, if found; otherwise, -1.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to locate in the collection.</param>
	public int IndexOf(RectKeyFrame keyFrame)
	{
		ReadPreamble();
		return _keyFrames.IndexOf(keyFrame);
	}

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
	/// <param name="keyFrame">The object to insert into the <see cref="T:System.Collections.IList" />. </param>
	void IList.Insert(int index, object keyFrame)
	{
		Insert(index, (RectKeyFrame)keyFrame);
	}

	/// <summary>Inserts a <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> into a specific location within the collection. </summary>
	/// <param name="index">The index position at which the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> is inserted.</param>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> object to insert in the collection.</param>
	public void Insert(int index, RectKeyFrame keyFrame)
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
		Remove((RectKeyFrame)keyFrame);
	}

	/// <summary>Removes a <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> object from the collection. </summary>
	/// <param name="keyFrame">Identifies the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to remove from the collection.</param>
	public void Remove(RectKeyFrame keyFrame)
	{
		WritePreamble();
		if (_keyFrames.Contains(keyFrame))
		{
			OnFreezablePropertyChanged(keyFrame, null);
			_keyFrames.Remove(keyFrame);
			WritePostscript();
		}
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> at the specified index position from the collection. </summary>
	/// <param name="index">Index position of the <see cref="T:System.Windows.Media.Animation.RectKeyFrame" /> to be removed.</param>
	public void RemoveAt(int index)
	{
		WritePreamble();
		OnFreezablePropertyChanged(_keyFrames[index], null);
		_keyFrames.RemoveAt(index);
		WritePostscript();
	}
}
