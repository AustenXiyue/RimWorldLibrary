using System.Collections;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>Represents an ordered collection of <see cref="T:System.Windows.UIElement" /> child elements. </summary>
public class UIElementCollection : IList, ICollection, IEnumerable
{
	private readonly VisualCollection _visualChildren;

	private readonly UIElement _visualParent;

	private readonly FrameworkElement _logicalParent;

	/// <summary>Gets the actual number of elements in the collection. </summary>
	/// <returns>The actual number of elements in the collection.</returns>
	public virtual int Count => _visualChildren.Count;

	/// <summary>Gets a value that indicates whether access to the <see cref="T:System.Collections.ICollection" /> interface is synchronized (thread-safe).</summary>
	/// <returns>true if access to the collection is synchronized; otherwise, false.</returns>
	public virtual bool IsSynchronized => _visualChildren.IsSynchronized;

	/// <summary>Gets an object that you can use to synchronize access to the <see cref="T:System.Collections.ICollection" /> interface. </summary>
	/// <returns>
	///   <see cref="T:System.Object" /> that you can use to synchronize access to the <see cref="T:System.Collections.ICollection" /> interface.</returns>
	public virtual object SyncRoot => _visualChildren.SyncRoot;

	/// <summary>Gets or sets the number of elements that the <see cref="T:System.Windows.Controls.UIElementCollection" /> can contain. </summary>
	/// <returns>The total number of elements the collection can contain.</returns>
	public virtual int Capacity
	{
		get
		{
			return _visualChildren.Capacity;
		}
		set
		{
			VerifyWriteAccess();
			_visualChildren.Capacity = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.UIElement" /> stored at the zero-based index position of the <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <returns>A <see cref="T:System.Windows.UIElement" /> at the specified <paramref name="index" /> position.</returns>
	/// <param name="index">The index position of the <see cref="T:System.Windows.UIElement" />.</param>
	public virtual UIElement this[int index]
	{
		get
		{
			return _visualChildren[index] as UIElement;
		}
		set
		{
			VerifyWriteAccess();
			ValidateElement(value);
			VisualCollection visualChildren = _visualChildren;
			if (visualChildren[index] != value)
			{
				if (visualChildren[index] is UIElement element)
				{
					ClearLogicalParent(element);
				}
				visualChildren[index] = value;
				SetLogicalParent(value);
				_visualParent.InvalidateMeasure();
			}
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="P:System.Collections.IList.IsFixedSize" />.</summary>
	/// <returns>true if the list has a fixed size; otherwise, false. </returns>
	bool IList.IsFixedSize => false;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="P:System.Collections.IList.IsReadOnly" />.</summary>
	/// <returns>true if the list is read-only; otherwise, false. </returns>
	bool IList.IsReadOnly => false;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="P:System.Collections.IList.Item(System.Int32)" />.</summary>
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
			this[index] = Cast(value);
		}
	}

	internal UIElement VisualParent => _visualParent;

	internal FrameworkElement LogicalParent => _logicalParent;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.UIElementCollection" /> class. </summary>
	/// <param name="visualParent">The <see cref="T:System.Windows.UIElement" /> parent of the collection.</param>
	/// <param name="logicalParent">The logical parent of the elements in the collection.</param>
	public UIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
	{
		if (visualParent == null)
		{
			throw new ArgumentNullException(SR.Format(SR.Panel_NoNullVisualParent, "visualParent", GetType()));
		}
		_visualChildren = new VisualCollection(visualParent);
		_visualParent = visualParent;
		_logicalParent = logicalParent;
	}

	/// <summary>Copies a <see cref="T:System.Windows.UIElement" /> from a <see cref="T:System.Windows.Controls.UIElementCollection" /> to an array, starting at a specified index position. </summary>
	/// <param name="array">An array into which the collection is copied.</param>
	/// <param name="index">The index position of the element where copying begins.</param>
	public virtual void CopyTo(Array array, int index)
	{
		_visualChildren.CopyTo(array, index);
	}

	/// <summary>Copies a <see cref="T:System.Windows.UIElement" /> from a <see cref="T:System.Windows.Controls.UIElementCollection" /> to an array, starting at a specified index position. </summary>
	/// <param name="array">An array of <see cref="T:System.Windows.UIElement" /> objects.</param>
	/// <param name="index">The index position of the element where copying begins.</param>
	public virtual void CopyTo(UIElement[] array, int index)
	{
		_visualChildren.CopyTo(array, index);
	}

	internal void SetInternal(int index, UIElement item)
	{
		ValidateElement(item);
		VisualCollection visualChildren = _visualChildren;
		if (visualChildren[index] != item)
		{
			visualChildren[index] = null;
			visualChildren[index] = item;
			_visualParent.InvalidateMeasure();
		}
	}

	/// <summary>Adds the specified element to the <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <returns>The index position of the added element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> to add.</param>
	public virtual int Add(UIElement element)
	{
		VerifyWriteAccess();
		return AddInternal(element);
	}

	internal int AddInternal(UIElement element)
	{
		ValidateElement(element);
		SetLogicalParent(element);
		int result = _visualChildren.Add(element);
		_visualParent.InvalidateMeasure();
		return result;
	}

	/// <summary>Returns the index position of a specified element in a <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <returns>The index position of the element.  -1 if the element is not in the collection.</returns>
	/// <param name="element">The element whose index position is required.</param>
	public virtual int IndexOf(UIElement element)
	{
		return _visualChildren.IndexOf(element);
	}

	/// <summary>Removes the specified element from a <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <param name="element">The element to remove from the collection.</param>
	public virtual void Remove(UIElement element)
	{
		VerifyWriteAccess();
		RemoveInternal(element);
	}

	internal void RemoveInternal(UIElement element)
	{
		_visualChildren.Remove(element);
		ClearLogicalParent(element);
		_visualParent.InvalidateMeasure();
	}

	internal virtual void RemoveNoVerify(UIElement element)
	{
		_visualChildren.Remove(element);
	}

	/// <summary>Determines whether a specified element is in the <see cref="T:System.Windows.Controls.UIElementCollection" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.UIElement" /> is found in the collection; otherwise, false.</returns>
	/// <param name="element">The element to find.</param>
	public virtual bool Contains(UIElement element)
	{
		return _visualChildren.Contains(element);
	}

	/// <summary>Removes all elements from a <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	public virtual void Clear()
	{
		VerifyWriteAccess();
		ClearInternal();
	}

	internal void ClearInternal()
	{
		VisualCollection visualChildren = _visualChildren;
		int count = visualChildren.Count;
		if (count <= 0)
		{
			return;
		}
		Visual[] array = new Visual[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = visualChildren[i];
		}
		visualChildren.Clear();
		for (int j = 0; j < count; j++)
		{
			if (array[j] is UIElement element)
			{
				ClearLogicalParent(element);
			}
		}
		_visualParent.InvalidateMeasure();
	}

	/// <summary>Inserts an element into a <see cref="T:System.Windows.Controls.UIElementCollection" /> at the specified index position. </summary>
	/// <param name="index">The index position where you want to insert the element.</param>
	/// <param name="element">The element to insert into the <see cref="T:System.Windows.Controls.UIElementCollection" />.</param>
	public virtual void Insert(int index, UIElement element)
	{
		VerifyWriteAccess();
		InsertInternal(index, element);
	}

	internal void InsertInternal(int index, UIElement element)
	{
		ValidateElement(element);
		SetLogicalParent(element);
		_visualChildren.Insert(index, element);
		_visualParent.InvalidateMeasure();
	}

	/// <summary>Removes the <see cref="T:System.Windows.UIElement" /> at the specified index. </summary>
	/// <param name="index">The index of the <see cref="T:System.Windows.UIElement" /> that you want to remove.</param>
	public virtual void RemoveAt(int index)
	{
		VerifyWriteAccess();
		VisualCollection visualChildren = _visualChildren;
		UIElement uIElement = visualChildren[index] as UIElement;
		visualChildren.RemoveAt(index);
		if (uIElement != null)
		{
			ClearLogicalParent(uIElement);
		}
		_visualParent.InvalidateMeasure();
	}

	/// <summary>Removes a range of elements from a <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <param name="index">The index position of the element where removal begins.</param>
	/// <param name="count">The number of elements to remove.</param>
	public virtual void RemoveRange(int index, int count)
	{
		VerifyWriteAccess();
		RemoveRangeInternal(index, count);
	}

	internal void RemoveRangeInternal(int index, int count)
	{
		VisualCollection visualChildren = _visualChildren;
		int count2 = visualChildren.Count;
		if (count > count2 - index)
		{
			count = count2 - index;
		}
		if (count <= 0)
		{
			return;
		}
		Visual[] array = new Visual[count];
		int num = index;
		for (int i = 0; i < count; i++)
		{
			array[i] = visualChildren[num];
			num++;
		}
		visualChildren.RemoveRange(index, count);
		for (num = 0; num < count; num++)
		{
			if (array[num] is UIElement element)
			{
				ClearLogicalParent(element);
			}
		}
		_visualParent.InvalidateMeasure();
	}

	internal void MoveVisualChild(Visual visual, Visual destination)
	{
		_visualChildren.Move(visual, destination);
	}

	private UIElement Cast(object value)
	{
		if (value == null)
		{
			throw new ArgumentException(SR.Format(SR.Collection_NoNull, "UIElementCollection"));
		}
		return (value as UIElement) ?? throw new ArgumentException(SR.Format(SR.Collection_BadType, "UIElementCollection", value.GetType().Name, "UIElement"));
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.Collections.IList.Add(System.Object)" />.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The object to add.</param>
	int IList.Add(object value)
	{
		return Add(Cast(value));
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.Collections.IList.Contains(System.Object)" />.</summary>
	/// <returns>true if the object was found in the list; otherwise, false.</returns>
	/// <param name="value">The object to locate in the list.</param>
	bool IList.Contains(object value)
	{
		return Contains(value as UIElement);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.Collections.IList.IndexOf(System.Object)" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1. </returns>
	/// <param name="value">The object to locate in the list.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(value as UIElement);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.Collections.IList.Insert(System.Int32,System.Object)" />.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
	/// <param name="value">The object to insert to the list.</param>
	void IList.Insert(int index, object value)
	{
		Insert(index, Cast(value));
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.Collections.IList.Remove(System.Object)" />.</summary>
	/// <param name="value">The object to remove from the list.</param>
	void IList.Remove(object value)
	{
		Remove(value as UIElement);
	}

	/// <summary>Returns an enumerator that can iterate the <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can list the members of this collection.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		return _visualChildren.GetEnumerator();
	}

	/// <summary>Sets the logical parent of an element in a <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> whose logical parent is set.</param>
	protected void SetLogicalParent(UIElement element)
	{
		if (_logicalParent != null)
		{
			_logicalParent.AddLogicalChild(element);
		}
	}

	/// <summary>Clears the logical parent of an element when the element leaves a <see cref="T:System.Windows.Controls.UIElementCollection" />. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> whose logical parent is being cleared.</param>
	protected void ClearLogicalParent(UIElement element)
	{
		if (_logicalParent != null)
		{
			_logicalParent.RemoveLogicalChild(element);
		}
	}

	private void ValidateElement(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException(SR.Format(SR.Panel_NoNullChildren, GetType()));
		}
	}

	private void VerifyWriteAccess()
	{
		if (_visualParent is Panel { IsDataBound: not false })
		{
			throw new InvalidOperationException(SR.Panel_BoundPanel_NoChildren);
		}
	}
}
