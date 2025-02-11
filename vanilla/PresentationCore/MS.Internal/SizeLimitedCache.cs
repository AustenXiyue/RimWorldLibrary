using System;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace MS.Internal;

[FriendAccessAllowed]
internal class SizeLimitedCache<K, V>
{
	private class Node
	{
		private V _resource;

		private K _key;

		private bool _isPermanent;

		private Node _next;

		private Node _previous;

		public K Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		public V Resource
		{
			get
			{
				return _resource;
			}
			set
			{
				_resource = value;
			}
		}

		public bool IsPermanent
		{
			get
			{
				return _isPermanent;
			}
			set
			{
				_isPermanent = value;
			}
		}

		public Node Next
		{
			get
			{
				return _next;
			}
			set
			{
				_next = value;
			}
		}

		public Node Previous
		{
			get
			{
				return _previous;
			}
			set
			{
				_previous = value;
			}
		}

		public Node(K key, V resource, bool isPermanent)
		{
			Key = key;
			Resource = resource;
			IsPermanent = isPermanent;
		}
	}

	private int _maximumItems;

	private int _permanentCount;

	private Node _begin;

	private Node _end;

	private Dictionary<K, Node> _nodeLookup;

	public int MaximumItems => _maximumItems;

	public SizeLimitedCache(int maximumItems)
	{
		if (maximumItems <= 0)
		{
			throw new ArgumentOutOfRangeException("maximumItems");
		}
		_maximumItems = maximumItems;
		_permanentCount = 0;
		_begin = new Node(default(K), default(V), isPermanent: false);
		_end = new Node(default(K), default(V), isPermanent: false);
		_begin.Next = _end;
		_end.Previous = _begin;
		_nodeLookup = new Dictionary<K, Node>();
	}

	public void Add(K key, V resource, bool isPermanent)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (resource == null)
		{
			throw new ArgumentNullException("resource");
		}
		if (!_nodeLookup.ContainsKey(key))
		{
			Node node = new Node(key, resource, isPermanent);
			if (!isPermanent)
			{
				if (IsFull())
				{
					RemoveOldest();
				}
				InsertAtEnd(node);
			}
			else
			{
				_permanentCount++;
			}
			_nodeLookup[key] = node;
			return;
		}
		Node node2 = _nodeLookup[key];
		if (!node2.IsPermanent)
		{
			RemoveFromList(node2);
		}
		if (!node2.IsPermanent && isPermanent)
		{
			_permanentCount++;
		}
		else if (node2.IsPermanent && !isPermanent)
		{
			_permanentCount--;
			if (IsFull())
			{
				RemoveOldest();
			}
		}
		node2.IsPermanent = isPermanent;
		node2.Resource = resource;
		if (!isPermanent)
		{
			InsertAtEnd(node2);
		}
	}

	public void Remove(K key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (_nodeLookup.ContainsKey(key))
		{
			Node node = _nodeLookup[key];
			_nodeLookup.Remove(key);
			if (!node.IsPermanent)
			{
				RemoveFromList(node);
			}
			else
			{
				_permanentCount--;
			}
		}
	}

	public V Get(K key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (!_nodeLookup.ContainsKey(key))
		{
			return default(V);
		}
		Node node = _nodeLookup[key];
		if (!node.IsPermanent)
		{
			RemoveFromList(node);
			InsertAtEnd(node);
		}
		return node.Resource;
	}

	private void RemoveOldest()
	{
		Node next = _begin.Next;
		_nodeLookup.Remove(next.Key);
		RemoveFromList(next);
	}

	private void InsertAtEnd(Node node)
	{
		node.Next = _end;
		node.Previous = _end.Previous;
		node.Previous.Next = node;
		_end.Previous = node;
	}

	private void RemoveFromList(Node node)
	{
		node.Previous.Next = node.Next;
		node.Next.Previous = node.Previous;
	}

	private bool IsFull()
	{
		return _nodeLookup.Count - _permanentCount >= _maximumItems;
	}
}
