using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Media.Animation;

/// <summary>Represents a collection of <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> objects. </summary>
public class Point3DKeyFrameCollection : Freezable, IList, ICollection, IEnumerable
{
	private List<Point3DKeyFrame> _keyFrames;

	private static Point3DKeyFrameCollection s_emptyCollection;

	/// <summary>Gets an empty <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />. </summary>
	/// <returns>An empty <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />.</returns>
	public static Point3DKeyFrameCollection Empty
	{
		get
		{
			if (s_emptyCollection == null)
			{
				Point3DKeyFrameCollection point3DKeyFrameCollection = new Point3DKeyFrameCollection();
				point3DKeyFrameCollection._keyFrames = new List<Point3DKeyFrame>(0);
				point3DKeyFrameCollection.Freeze();
				s_emptyCollection = point3DKeyFrameCollection;
			}
			return s_emptyCollection;
		}
	}

	/// <summary>Gets the number of key frames contained in the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />. </summary>
	/// <returns>The number of key frames contained in the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />.</returns>
	public int Count
	{
		get
		{
			ReadPreamble();
			return _keyFrames.Count;
		}
	}

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread-safe).</summary>
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

	/// <summary>Gets an object that can be used to synchronize access to the collection. </summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	public object SyncRoot
	{
		get
		{
			ReadPreamble();
			return ((ICollection)_keyFrames).SyncRoot;
		}
	}

	/// <summary>Gets a value that indicates if the collection is frozen. </summary>
	/// <returns>true if the collection is frozen; otherwise, false.</returns>
	public bool IsFixedSize
	{
		get
		{
			ReadPreamble();
			return base.IsFrozen;
		}
	}

	/// <summary>Gets a value that indicates if the collection is read-only. </summary>
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
			this[index] = (Point3DKeyFrame)value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> at the specified index position. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to get or set.</param>
	/// <exception cref="T:System.InvalidOperationException">The attempt to modify the collection is invalid because the collection is frozen (its <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Windows.Media.Animation.Vector3DKeyFrameCollection.Count" />.</exception>
	public Point3DKeyFrame this[int index]
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
				throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Point3DKeyFrameCollection[{0}]", index));
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> class.</summary>
	public Point3DKeyFrameCollection()
	{
		_keyFrames = new List<Point3DKeyFrame>(2);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Point3DKeyFrameCollection Clone()
	{
		return (Point3DKeyFrameCollection)base.Clone();
	}

	/// <summary>Creates a new, frozen instance of <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />.              </summary>
	/// <returns>A frozen instance of <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new Point3DKeyFrameCollection();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		Point3DKeyFrameCollection point3DKeyFrameCollection = (Point3DKeyFrameCollection)sourceFreezable;
		base.CloneCore(sourceFreezable);
		int count = point3DKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<Point3DKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			Point3DKeyFrame point3DKeyFrame = (Point3DKeyFrame)point3DKeyFrameCollection._keyFrames[i].Clone();
			_keyFrames.Add(point3DKeyFrame);
			OnFreezablePropertyChanged(null, point3DKeyFrame);
		}
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		Point3DKeyFrameCollection point3DKeyFrameCollection = (Point3DKeyFrameCollection)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		int count = point3DKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<Point3DKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			Point3DKeyFrame point3DKeyFrame = (Point3DKeyFrame)point3DKeyFrameCollection._keyFrames[i].CloneCurrentValue();
			_keyFrames.Add(point3DKeyFrame);
			OnFreezablePropertyChanged(null, point3DKeyFrame);
		}
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> object.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> object to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		Point3DKeyFrameCollection point3DKeyFrameCollection = (Point3DKeyFrameCollection)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		int count = point3DKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<Point3DKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			Point3DKeyFrame point3DKeyFrame = (Point3DKeyFrame)point3DKeyFrameCollection._keyFrames[i].GetAsFrozen();
			_keyFrames.Add(point3DKeyFrame);
			OnFreezablePropertyChanged(null, point3DKeyFrame);
		}
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		Point3DKeyFrameCollection point3DKeyFrameCollection = (Point3DKeyFrameCollection)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		int count = point3DKeyFrameCollection._keyFrames.Count;
		_keyFrames = new List<Point3DKeyFrame>(count);
		for (int i = 0; i < count; i++)
		{
			Point3DKeyFrame point3DKeyFrame = (Point3DKeyFrame)point3DKeyFrameCollection._keyFrames[i].GetCurrentValueAsFrozen();
			_keyFrames.Add(point3DKeyFrame);
			OnFreezablePropertyChanged(null, point3DKeyFrame);
		}
	}

	/// <summary>Makes this instance of <see cref="T:System.Windows.Media.Animation.Point3DKeyFrameCollection" /> unmodifiable or determines whether it can be made unmodifiable.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if the specified <see cref="T:System.Windows.Freezable" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If <paramref name="isChecking" /> is false, this method returns true if the if the specified <see cref="T:System.Windows.Freezable" /> is now unmodifiable, or false if it cannot be made unmodifiable, with the side effect of having made the actual change in frozen status to this object.</returns>
	/// <param name="isChecking">true if the <see cref="T:System.Windows.Freezable" /> instance should actually freeze itself when this method is called. false if the <see cref="T:System.Windows.Freezable" /> should simply return whether it can be frozen.</param>
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

	/// <summary>Copies all of the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> objects in a collection to a specified array. </summary>
	/// <param name="array">Identifies the array to which content is copied.</param>
	/// <param name="index">Index position in the array to which the contents of the collection are copied.</param>
	public void CopyTo(Point3DKeyFrame[] array, int index)
	{
		ReadPreamble();
		_keyFrames.CopyTo(array, index);
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
	/// <param name="keyFrame">The object to add to the <see cref="T:System.Collections.IList" />.</param>
	int IList.Add(object keyFrame)
	{
		return Add((Point3DKeyFrame)keyFrame);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to the end of the collection. </summary>
	/// <returns>The index at which the <paramref name="keyFrame" /> was added.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to add to the end of the collection.</param>
	public int Add(Point3DKeyFrame keyFrame)
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

	/// <summary>Removes all <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> objects from the collection.</summary>
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
	/// <param name="keyFrame">The object to locate in the <see cref="T:System.Collections.IList" />. </param>
	bool IList.Contains(object keyFrame)
	{
		return Contains((Point3DKeyFrame)keyFrame);
	}

	/// <summary>Gets a value that indicates whether the collection contains the specified <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" />. </summary>
	/// <returns>true if the collection contains <paramref name="keyFrame" />; otherwise, false.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to locate in the collection.</param>
	public bool Contains(Point3DKeyFrame keyFrame)
	{
		ReadPreamble();
		return _keyFrames.Contains(keyFrame);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="keyFrame">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
	int IList.IndexOf(object keyFrame)
	{
		return IndexOf((Point3DKeyFrame)keyFrame);
	}

	/// <summary> Searches for the specified <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> and returns the zero-based index of the first occurrence within the entire collection. </summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="keyFrame" /> within the entire collection, if found; otherwise, -1.</returns>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to locate in the collection.</param>
	public int IndexOf(Point3DKeyFrame keyFrame)
	{
		ReadPreamble();
		return _keyFrames.IndexOf(keyFrame);
	}

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
	/// <param name="keyFrame">The object to insert into the <see cref="T:System.Collections.IList" />. </param>
	void IList.Insert(int index, object keyFrame)
	{
		Insert(index, (Point3DKeyFrame)keyFrame);
	}

	/// <summary> Inserts a <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> into a specific location within the collection.  </summary>
	/// <param name="index">The index position at which the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> is inserted.</param>
	/// <param name="keyFrame">The <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> object to insert in the collection.</param>
	public void Insert(int index, Point3DKeyFrame keyFrame)
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
		Remove((Point3DKeyFrame)keyFrame);
	}

	/// <summary>Removes a <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> object from the collection. </summary>
	/// <param name="keyFrame">Identifies the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to remove from the collection.</param>
	public void Remove(Point3DKeyFrame keyFrame)
	{
		WritePreamble();
		if (_keyFrames.Contains(keyFrame))
		{
			OnFreezablePropertyChanged(keyFrame, null);
			_keyFrames.Remove(keyFrame);
			WritePostscript();
		}
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> at the specified index position from the collection. </summary>
	/// <param name="index">Index position of the <see cref="T:System.Windows.Media.Animation.Point3DKeyFrame" /> to be removed.</param>
	public void RemoveAt(int index)
	{
		WritePreamble();
		OnFreezablePropertyChanged(_keyFrames[index], null);
		_keyFrames.RemoveAt(index);
		WritePostscript();
	}
}
