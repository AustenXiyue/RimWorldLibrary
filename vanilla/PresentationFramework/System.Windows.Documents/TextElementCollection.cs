using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using MS.Internal;

namespace System.Windows.Documents;

/// <summary>Provides standard facilities for creating and managing a type-safe, ordered collection of <see cref="T:System.Windows.Documents.TextElement" /> objects.  This is a generic collection for working with objects of a specified type that derives from <see cref="T:System.Windows.Documents.TextElement" />.</summary>
/// <typeparam name="TextElementType">Type specifier for the generic collection.  Acceptable types are constrained to a type of <see cref="T:System.Windows.Documents.TextElement" /> or a descendant of <see cref="T:System.Windows.Documents.TextElement" />.</typeparam>
public class TextElementCollection<TextElementType> : IList, ICollection, IEnumerable, ICollection<TextElementType>, IEnumerable<TextElementType> where TextElementType : TextElement
{
	private struct ElementIndexCache
	{
		private readonly int _index;

		private readonly TextElementType _element;

		internal int Index => _index;

		internal TextElementType Element => _element;

		internal ElementIndexCache(int index, TextElementType element)
		{
			Invariant.Assert(index == -1 || element != null);
			_index = index;
			_element = element;
		}

		internal bool IsValid(TextElementCollection<TextElementType> collection)
		{
			if (_index >= 0)
			{
				return TextElementCollectionHelper.IsCleanParent(_element.Parent, collection);
			}
			return false;
		}
	}

	private DependencyObject _owner;

	private bool _isOwnerParent;

	private ElementIndexCache _indexCache;

	/// <summary>Gets the number of items currently in the collection.</summary>
	/// <returns>The number of items currently in the collection.</returns>
	public int Count
	{
		get
		{
			int num = 0;
			TextElement textElement;
			if (_indexCache.IsValid(this))
			{
				textElement = _indexCache.Element;
				num += _indexCache.Index;
			}
			else
			{
				textElement = FirstChild;
			}
			while (textElement != null)
			{
				num++;
				textElement = textElement.NextElement;
			}
			return num;
		}
	}

	/// <summary>Gets a value that indicates whether or not the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	public bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IList" /> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => IsReadOnly;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	object IList.this[int index]
	{
		get
		{
			if (index < 0)
			{
				throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
			}
			TextElementType elementAtIndex = GetElementAtIndex(index);
			if (elementAtIndex == null)
			{
				throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
			}
			SetCache(index, elementAtIndex);
			return elementAtIndex;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is TextElementType))
			{
				throw new ArgumentException(SR.Format(SR.TextElementCollection_TextElementTypeExpected, typeof(TextElementType).Name), "value");
			}
			ValidateChild((TextElementType)value);
			TextContainer.BeginChange();
			try
			{
				TextElementType val = RemoveAtInternal(index);
				((val == null) ? ContentEnd : val.ElementStart).InsertTextElement((TextElementType)value);
				SetCache(index, (TextElementType)value);
			}
			finally
			{
				TextContainer.EndChange();
			}
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.  Use the type-safe <see cref="P:System.Windows.Documents.TextElementCollection`1.Count" /> property instead.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection" />.</returns>
	int ICollection.Count => Count;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => true;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => TextContainer;

	internal DependencyObject Owner => _owner;

	internal DependencyObject Parent
	{
		get
		{
			if (!_isOwnerParent)
			{
				return ((TextElement)_owner).Parent;
			}
			return _owner;
		}
	}

	internal TextContainer TextContainer
	{
		get
		{
			if (_owner is TextBlock)
			{
				return (TextContainer)((TextBlock)_owner).TextContainer;
			}
			if (_owner is FlowDocument)
			{
				return ((FlowDocument)_owner).TextContainer;
			}
			return ((TextElement)_owner).TextContainer;
		}
	}

	internal TextElementType FirstChild
	{
		get
		{
			if (Parent is TextElement)
			{
				return (TextElementType)((TextElement)Parent).FirstChildElement;
			}
			return (TextElementType)((!(TextContainer.FirstContainedNode is TextTreeTextElementNode textTreeTextElementNode)) ? null : textTreeTextElementNode.TextElement);
		}
	}

	internal TextElementType LastChild
	{
		get
		{
			if (Parent is TextElement)
			{
				return (TextElementType)((TextElement)Parent).LastChildElement;
			}
			return (TextElementType)((!(TextContainer.LastContainedNode is TextTreeTextElementNode textTreeTextElementNode)) ? null : textTreeTextElementNode.TextElement);
		}
	}

	private TextPointer ContentStart
	{
		get
		{
			if (!(Parent is TextElement))
			{
				return TextContainer.Start;
			}
			return ((TextElement)Parent).ContentStart;
		}
	}

	private TextPointer ContentEnd
	{
		get
		{
			if (!(Parent is TextElement))
			{
				return TextContainer.End;
			}
			return ((TextElement)Parent).ContentEnd;
		}
	}

	internal TextElementCollection(DependencyObject owner, bool isOwnerParent)
	{
		if (isOwnerParent)
		{
			Invariant.Assert(owner is TextElement || owner is FlowDocument || owner is TextBlock);
		}
		else
		{
			Invariant.Assert(owner is TextElement);
		}
		_owner = owner;
		_isOwnerParent = isOwnerParent;
		_indexCache = new ElementIndexCache(-1, null);
	}

	/// <summary>Appends a specified item to the collection.</summary>
	/// <param name="item">An item to append to the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised when item already belongs to a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when item is null.</exception>
	public void Add(TextElementType item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		ValidateChild(item);
		TextContainer.BeginChange();
		try
		{
			item.RepositionWithContent(ContentEnd);
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	/// <summary>Clears all items from the collection.</summary>
	public void Clear()
	{
		TextContainer textContainer = TextContainer;
		textContainer.BeginChange();
		try
		{
			textContainer.DeleteContentInternal(ContentStart, ContentEnd);
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	/// <summary>Queries for the presence of a specified item in the collection.</summary>
	/// <returns>true if the specified item is present in the collection; otherwise, false.</returns>
	/// <param name="item">An item to query for the presence of in the collection.</param>
	public bool Contains(TextElementType item)
	{
		if (item == null)
		{
			return false;
		}
		TextElementType val = FirstChild;
		while (val != null && val != item)
		{
			val = (TextElementType)val.NextElement;
		}
		return val == item;
	}

	/// <summary>Copies the contents of the collection and inserts them into a specified array starting at a specified index position in the array.</summary>
	/// <param name="array">A one-dimensional array to which the collection contents will be copied.  This array must use zero-based indexing.</param>
	/// <param name="arrayIndex">A zero-based index in <paramref name="array" /> specifying the position at which to begin inserting the copied collection objects.</param>
	/// <exception cref="T:System.ArgumentException">Raised when array includes items that are not compatible with the type <see cref="T:System.Windows.Documents.TextElement" />, or if arrayIndex specifies a position that falls outside of the bounds of array.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when array is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Raised when arrayIndex is less than 0.</exception>
	public void CopyTo(TextElementType[] array, int arrayIndex)
	{
		((ICollection)this).CopyTo((Array)array, arrayIndex);
	}

	/// <summary>Removes a specified item from the collection.</summary>
	/// <returns>true if the specified item was found and removed; otherwise, false.</returns>
	/// <param name="item">An item to be removed fro the collection.</param>
	public bool Remove(TextElementType item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.Parent != Parent)
		{
			return false;
		}
		TextContainer textContainer = TextContainer;
		textContainer.BeginChange();
		try
		{
			item.RepositionWithContent(null);
		}
		finally
		{
			textContainer.EndChange();
		}
		return true;
	}

	/// <summary>Inserts a specified item in the collection after a specified collection item.</summary>
	/// <param name="previousSibling">An item in the collection after which the new item will be inserted.</param>
	/// <param name="newItem">An item to insert into the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised when newItem already belongs to a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when previousSibling or newItem is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Raised when previousSibling does not belong to this collection.</exception>
	public void InsertAfter(TextElementType previousSibling, TextElementType newItem)
	{
		if (previousSibling == null)
		{
			throw new ArgumentNullException("previousSibling");
		}
		if (newItem == null)
		{
			throw new ArgumentNullException("newItem");
		}
		if (previousSibling.Parent != Parent)
		{
			throw new InvalidOperationException(SR.Format(SR.TextElementCollection_PreviousSiblingDoesNotBelongToThisCollection, previousSibling.GetType().Name));
		}
		if (newItem.Parent != null)
		{
			throw new ArgumentException(SR.Format(SR.TextSchema_TheChildElementBelongsToAnotherTreeAlready, GetType().Name));
		}
		ValidateChild(newItem);
		TextContainer.BeginChange();
		try
		{
			newItem.RepositionWithContent(previousSibling.ElementEnd);
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	/// <summary>Inserts a specified item in the collection before a specified collection item.</summary>
	/// <param name="nextSibling">An item in the collection before which the new item will be inserted.</param>
	/// <param name="newItem">An item to insert into the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised when newItem already belongs to a collection.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when nextSibling or newItem is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Raised when nextSibling does not belong to this collection.</exception>
	public void InsertBefore(TextElementType nextSibling, TextElementType newItem)
	{
		if (nextSibling == null)
		{
			throw new ArgumentNullException("nextSibling");
		}
		if (newItem == null)
		{
			throw new ArgumentNullException("newItem");
		}
		if (nextSibling.Parent != Parent)
		{
			throw new InvalidOperationException(SR.Format(SR.TextElementCollection_NextSiblingDoesNotBelongToThisCollection, nextSibling.GetType().Name));
		}
		if (newItem.Parent != null)
		{
			throw new ArgumentException(SR.Format(SR.TextSchema_TheChildElementBelongsToAnotherTreeAlready, GetType().Name));
		}
		ValidateChild(newItem);
		TextContainer.BeginChange();
		try
		{
			newItem.RepositionWithContent(nextSibling.ElementStart);
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	/// <summary>Appends a specified range of items to the collection.</summary>
	/// <param name="range">An object that implements the <see cref="T:System.Collections.IEnumerable" /> interface, and that specifies a range of items to add to the collection.</param>
	/// <exception cref="T:System.ArgumentException">Raised when range includes any null items.</exception>
	/// <exception cref="T:System.ArgumentNullException">Raised when range is null.</exception>
	public void AddRange(IEnumerable range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		IEnumerator enumerator = range.GetEnumerator();
		if (enumerator == null)
		{
			throw new ArgumentException(SR.TextElementCollection_NoEnumerator, "range");
		}
		TextContainer.BeginChange();
		try
		{
			while (enumerator.MoveNext())
			{
				if (!(enumerator.Current is TextElementType item))
				{
					throw new ArgumentException(SR.Format(SR.TextElementCollection_ItemHasUnexpectedType, "range", typeof(TextElementType).Name, typeof(TextElementType).Name), "value");
				}
				Add(item);
			}
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	/// <summary>Returns an enumerator for the contents of the collection.</summary>
	/// <returns>An enumerator for the contents of the collection.</returns>
	public IEnumerator<TextElementType> GetEnumerator()
	{
		return new TextElementEnumerator<TextElementType>(ContentStart, ContentEnd);
	}

	/// <summary>Returns an enumerator that iterates through a collection.  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.GetEnumerator" /> method instead.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new RangeContentEnumerator(ContentStart, ContentEnd);
	}

	internal virtual int OnAdd(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is TextElementType))
		{
			throw new ArgumentException(SR.Format(SR.TextElementCollection_TextElementTypeExpected, typeof(TextElementType).Name), "value");
		}
		ValidateChild((TextElementType)value);
		TextContainer.BeginChange();
		try
		{
			bool isCacheSafePreviousIndex = _indexCache.IsValid(this);
			Add((TextElementType)value);
			return IndexOfInternal(value, isCacheSafePreviousIndex);
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	/// <summary>Adds an item to the <see cref="T:System.Collections.IList" />.  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.Add(`0)" /> method instead.</summary>
	/// <returns>The position into which the new element was inserted.</returns>
	/// <param name="value">The object to add to the <see cref="T:System.Collections.IList" />. </param>
	int IList.Add(object value)
	{
		return OnAdd(value);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.Clear" /> method instead.</summary>
	void IList.Clear()
	{
		Clear();
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value..  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.Contains(`0)" /> method instead.</summary>
	/// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />. </param>
	bool IList.Contains(object value)
	{
		if (!(value is TextElementType item))
		{
			return false;
		}
		return Contains(item);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
	/// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />. </param>
	int IList.IndexOf(object value)
	{
		return IndexOfInternal(value, isCacheSafePreviousIndex: false);
	}

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.InsertAfter(`0,`0)" /> or <see cref="M:System.Windows.Documents.TextElementCollection`1.InsertBefore(`0,`0)" /> methods instead.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
	/// <param name="value">The object to insert into the <see cref="T:System.Collections.IList" />. </param>
	void IList.Insert(int index, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is TextElementType val))
		{
			throw new ArgumentException(SR.Format(SR.TextElementCollection_TextElementTypeExpected, typeof(TextElementType).Name), "value");
		}
		if (index < 0)
		{
			throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
		}
		if (val.Parent != null)
		{
			throw new ArgumentException(SR.Format(SR.TextSchema_TheChildElementBelongsToAnotherTreeAlready, GetType().Name));
		}
		ValidateChild(val);
		TextContainer.BeginChange();
		try
		{
			TextPointer textPointer;
			if (FirstChild == null)
			{
				if (index != 0)
				{
					throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
				}
				textPointer = ContentStart;
			}
			else
			{
				bool atCollectionEnd;
				TextElementType elementAtIndex = GetElementAtIndex(index, out atCollectionEnd);
				if (!atCollectionEnd && elementAtIndex == null)
				{
					throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
				}
				textPointer = (atCollectionEnd ? ContentEnd : elementAtIndex.ElementStart);
			}
			textPointer.InsertTextElement(val);
			SetCache(index, val);
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.Remove(`0)" /> method instead.</summary>
	/// <param name="value">The object to remove from the <see cref="T:System.Collections.IList" />. </param>
	void IList.Remove(object value)
	{
		if (value is TextElementType item)
		{
			Remove(item);
		}
	}

	/// <summary>Removes the <see cref="T:System.Collections.IList" /> item at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	void IList.RemoveAt(int index)
	{
		RemoveAtInternal(index);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index..  Use the type-safe <see cref="M:System.Windows.Documents.TextElementCollection`1.CopyTo(`0[],System.Int32)" /> method instead.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		int count = Count;
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Type elementType = array.GetType().GetElementType();
		if (elementType == null || !elementType.IsAssignableFrom(typeof(TextElementType)))
		{
			throw new ArgumentException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (arrayIndex > array.Length)
		{
			throw new ArgumentException("arrayIndex");
		}
		if (array.Length < arrayIndex + count)
		{
			throw new ArgumentException(SR.Format(SR.TextElementCollection_CannotCopyToArrayNotSufficientMemory, count, arrayIndex, array.Length));
		}
		for (TextElementType val = FirstChild; val != null; val = (TextElementType)val.NextElement)
		{
			array.SetValue(val, arrayIndex++);
		}
	}

	private TextElementType RemoveAtInternal(int index)
	{
		if (index < 0)
		{
			throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
		}
		TextElementType elementAtIndex = GetElementAtIndex(index);
		if (elementAtIndex == null)
		{
			throw new IndexOutOfRangeException(SR.TextElementCollection_IndexOutOfRange);
		}
		TextElementType val = (TextElementType)elementAtIndex.NextElement;
		TextContainer textContainer = TextContainer;
		textContainer.BeginChange();
		try
		{
			TextElementType val2 = val;
			if (val2 == null)
			{
				val2 = (TextElementType)elementAtIndex.PreviousElement;
				index--;
			}
			elementAtIndex.RepositionWithContent(null);
			if (val2 != null)
			{
				SetCache(index, val2);
			}
		}
		finally
		{
			textContainer.EndChange();
		}
		return val;
	}

	private TextElementType GetElementAtIndex(int index)
	{
		bool atCollectionEnd;
		return GetElementAtIndex(index, out atCollectionEnd);
	}

	private TextElementType GetElementAtIndex(int index, out bool atCollectionEnd)
	{
		bool flag = true;
		TextElementType val;
		if (_indexCache.IsValid(this))
		{
			if (_indexCache.Index == index)
			{
				val = _indexCache.Element;
				index = 0;
			}
			else if (_indexCache.Index < index)
			{
				val = _indexCache.Element;
				index -= _indexCache.Index;
			}
			else
			{
				val = _indexCache.Element;
				index = _indexCache.Index - index;
				flag = false;
			}
		}
		else
		{
			val = FirstChild;
		}
		while (index > 0 && val != null)
		{
			val = (TextElementType)(flag ? val.NextElement : val.PreviousElement);
			index--;
		}
		atCollectionEnd = index == 0 && val == null;
		return val;
	}

	private void SetCache(int index, TextElementType item)
	{
		_indexCache = new ElementIndexCache(index, item);
		TextElementCollectionHelper.MarkClean(Parent, this);
	}

	private int IndexOfInternal(object value, bool isCacheSafePreviousIndex)
	{
		TextElementType val = value as TextElementType;
		if (value == null)
		{
			return -1;
		}
		if (_indexCache.IsValid(this) && val == _indexCache.Element)
		{
			return _indexCache.Index;
		}
		int num;
		TextElementType val2;
		if (isCacheSafePreviousIndex)
		{
			num = _indexCache.Index;
			val2 = _indexCache.Element;
		}
		else
		{
			num = 0;
			val2 = FirstChild;
		}
		while (val2 != null)
		{
			if (val2 == val)
			{
				SetCache(num, val);
				return num;
			}
			val2 = (TextElementType)val2.NextElement;
			num++;
		}
		return -1;
	}

	internal virtual void ValidateChild(TextElementType child)
	{
	}
}
