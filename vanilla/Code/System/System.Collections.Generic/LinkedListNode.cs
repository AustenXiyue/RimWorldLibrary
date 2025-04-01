namespace System.Collections.Generic;

/// <summary>Represents a node in a <see cref="T:System.Collections.Generic.LinkedList`1" />. This class cannot be inherited.</summary>
/// <typeparam name="T">Specifies the element type of the linked list.</typeparam>
/// <filterpriority>1</filterpriority>
public sealed class LinkedListNode<T>
{
	internal LinkedList<T> list;

	internal LinkedListNode<T> next;

	internal LinkedListNode<T> prev;

	internal T item;

	/// <summary>Gets the <see cref="T:System.Collections.Generic.LinkedList`1" /> that the <see cref="T:System.Collections.Generic.LinkedListNode`1" /> belongs to.</summary>
	/// <returns>A reference to the <see cref="T:System.Collections.Generic.LinkedList`1" /> that the <see cref="T:System.Collections.Generic.LinkedListNode`1" /> belongs to, or null if the <see cref="T:System.Collections.Generic.LinkedListNode`1" /> is not linked.</returns>
	public LinkedList<T> List => list;

	/// <summary>Gets the next node in the <see cref="T:System.Collections.Generic.LinkedList`1" />.</summary>
	/// <returns>A reference to the next node in the <see cref="T:System.Collections.Generic.LinkedList`1" />, or null if the current node is the last element (<see cref="P:System.Collections.Generic.LinkedList`1.Last" />) of the <see cref="T:System.Collections.Generic.LinkedList`1" />.</returns>
	public LinkedListNode<T> Next
	{
		get
		{
			if (next != null && next != list.head)
			{
				return next;
			}
			return null;
		}
	}

	/// <summary>Gets the previous node in the <see cref="T:System.Collections.Generic.LinkedList`1" />.</summary>
	/// <returns>A reference to the previous node in the <see cref="T:System.Collections.Generic.LinkedList`1" />, or null if the current node is the first element (<see cref="P:System.Collections.Generic.LinkedList`1.First" />) of the <see cref="T:System.Collections.Generic.LinkedList`1" />.</returns>
	public LinkedListNode<T> Previous
	{
		get
		{
			if (prev != null && this != list.head)
			{
				return prev;
			}
			return null;
		}
	}

	/// <summary>Gets the value contained in the node.</summary>
	/// <returns>The value contained in the node.</returns>
	public T Value
	{
		get
		{
			return item;
		}
		set
		{
			item = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.LinkedListNode`1" /> class, containing the specified value.</summary>
	/// <param name="value">The value to contain in the <see cref="T:System.Collections.Generic.LinkedListNode`1" />.</param>
	public LinkedListNode(T value)
	{
		item = value;
	}

	internal LinkedListNode(LinkedList<T> list, T value)
	{
		this.list = list;
		item = value;
	}

	internal void Invalidate()
	{
		list = null;
		next = null;
		prev = null;
	}
}
