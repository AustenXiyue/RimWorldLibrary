using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Schema;

/// <summary>Provides the collections for contained elements in the <see cref="T:System.Xml.Schema.XmlSchema" /> class (for example, Attributes, AttributeGroups, Elements, and so on).</summary>
public class XmlSchemaObjectTable
{
	internal enum EnumeratorType
	{
		Keys,
		Values,
		DictionaryEntry
	}

	internal struct XmlSchemaObjectEntry
	{
		internal XmlQualifiedName qname;

		internal XmlSchemaObject xso;

		public XmlSchemaObjectEntry(XmlQualifiedName name, XmlSchemaObject value)
		{
			qname = name;
			xso = value;
		}

		public XmlSchemaObject IsMatch(string localName, string ns)
		{
			if (localName == qname.Name && ns == qname.Namespace)
			{
				return xso;
			}
			return null;
		}

		public void Reset()
		{
			qname = null;
			xso = null;
		}
	}

	internal class NamesCollection : ICollection, IEnumerable
	{
		private List<XmlSchemaObjectEntry> entries;

		private int size;

		public int Count => size;

		public object SyncRoot => ((ICollection)entries).SyncRoot;

		public bool IsSynchronized => ((ICollection)entries).IsSynchronized;

		internal NamesCollection(List<XmlSchemaObjectEntry> entries, int size)
		{
			this.entries = entries;
			this.size = size;
		}

		public void CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			for (int i = 0; i < size; i++)
			{
				array.SetValue(entries[i].qname, arrayIndex++);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return new XSOEnumerator(entries, size, EnumeratorType.Keys);
		}
	}

	internal class ValuesCollection : ICollection, IEnumerable
	{
		private List<XmlSchemaObjectEntry> entries;

		private int size;

		public int Count => size;

		public object SyncRoot => ((ICollection)entries).SyncRoot;

		public bool IsSynchronized => ((ICollection)entries).IsSynchronized;

		internal ValuesCollection(List<XmlSchemaObjectEntry> entries, int size)
		{
			this.entries = entries;
			this.size = size;
		}

		public void CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			for (int i = 0; i < size; i++)
			{
				array.SetValue(entries[i].xso, arrayIndex++);
			}
		}

		public IEnumerator GetEnumerator()
		{
			return new XSOEnumerator(entries, size, EnumeratorType.Values);
		}
	}

	internal class XSOEnumerator : IEnumerator
	{
		private List<XmlSchemaObjectEntry> entries;

		private EnumeratorType enumType;

		protected int currentIndex;

		protected int size;

		protected XmlQualifiedName currentKey;

		protected XmlSchemaObject currentValue;

		public object Current
		{
			get
			{
				if (currentIndex == -1)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has not started. Call MoveNext.", string.Empty));
				}
				if (currentIndex >= size)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has already finished.", string.Empty));
				}
				return enumType switch
				{
					EnumeratorType.Keys => currentKey, 
					EnumeratorType.Values => currentValue, 
					EnumeratorType.DictionaryEntry => new DictionaryEntry(currentKey, currentValue), 
					_ => null, 
				};
			}
		}

		internal XSOEnumerator(List<XmlSchemaObjectEntry> entries, int size, EnumeratorType enumType)
		{
			this.entries = entries;
			this.size = size;
			this.enumType = enumType;
			currentIndex = -1;
		}

		public bool MoveNext()
		{
			if (currentIndex >= size - 1)
			{
				currentValue = null;
				currentKey = null;
				return false;
			}
			currentIndex++;
			currentValue = entries[currentIndex].xso;
			currentKey = entries[currentIndex].qname;
			return true;
		}

		public void Reset()
		{
			currentIndex = -1;
			currentValue = null;
			currentKey = null;
		}
	}

	internal class XSODictionaryEnumerator : XSOEnumerator, IDictionaryEnumerator, IEnumerator
	{
		public DictionaryEntry Entry
		{
			get
			{
				if (currentIndex == -1)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has not started. Call MoveNext.", string.Empty));
				}
				if (currentIndex >= size)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has already finished.", string.Empty));
				}
				return new DictionaryEntry(currentKey, currentValue);
			}
		}

		public object Key
		{
			get
			{
				if (currentIndex == -1)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has not started. Call MoveNext.", string.Empty));
				}
				if (currentIndex >= size)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has already finished.", string.Empty));
				}
				return currentKey;
			}
		}

		public object Value
		{
			get
			{
				if (currentIndex == -1)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has not started. Call MoveNext.", string.Empty));
				}
				if (currentIndex >= size)
				{
					throw new InvalidOperationException(Res.GetString("Enumeration has already finished.", string.Empty));
				}
				return currentValue;
			}
		}

		internal XSODictionaryEnumerator(List<XmlSchemaObjectEntry> entries, int size, EnumeratorType enumType)
			: base(entries, size, enumType)
		{
		}
	}

	private Dictionary<XmlQualifiedName, XmlSchemaObject> table = new Dictionary<XmlQualifiedName, XmlSchemaObject>();

	private List<XmlSchemaObjectEntry> entries = new List<XmlSchemaObjectEntry>();

	/// <summary>Gets the number of items contained in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</summary>
	/// <returns>The number of items contained in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</returns>
	public int Count => table.Count;

	/// <summary>Returns the element in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> specified by qualified name.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaObject" /> of the element in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" /> specified by qualified name.</returns>
	/// <param name="name">The <see cref="T:System.Xml.XmlQualifiedName" /> of the element to return.</param>
	public XmlSchemaObject this[XmlQualifiedName name]
	{
		get
		{
			if (table.TryGetValue(name, out var value))
			{
				return value;
			}
			return null;
		}
	}

	/// <summary>Returns a collection of all the named elements in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</summary>
	/// <returns>A collection of all the named elements in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</returns>
	public ICollection Names => new NamesCollection(entries, table.Count);

	/// <summary>Returns a collection of all the values for all the elements in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</summary>
	/// <returns>A collection of all the values for all the elements in the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</returns>
	public ICollection Values => new ValuesCollection(entries, table.Count);

	internal XmlSchemaObjectTable()
	{
	}

	internal void Add(XmlQualifiedName name, XmlSchemaObject value)
	{
		table.Add(name, value);
		entries.Add(new XmlSchemaObjectEntry(name, value));
	}

	internal void Insert(XmlQualifiedName name, XmlSchemaObject value)
	{
		XmlSchemaObject value2 = null;
		if (table.TryGetValue(name, out value2))
		{
			table[name] = value;
			int index = FindIndexByValue(value2);
			entries[index] = new XmlSchemaObjectEntry(name, value);
		}
		else
		{
			Add(name, value);
		}
	}

	internal void Replace(XmlQualifiedName name, XmlSchemaObject value)
	{
		if (table.TryGetValue(name, out var value2))
		{
			table[name] = value;
			int index = FindIndexByValue(value2);
			entries[index] = new XmlSchemaObjectEntry(name, value);
		}
	}

	internal void Clear()
	{
		table.Clear();
		entries.Clear();
	}

	internal void Remove(XmlQualifiedName name)
	{
		if (table.TryGetValue(name, out var value))
		{
			table.Remove(name);
			int index = FindIndexByValue(value);
			entries.RemoveAt(index);
		}
	}

	private int FindIndexByValue(XmlSchemaObject xso)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].xso == xso)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Determines if the qualified name specified exists in the collection.</summary>
	/// <returns>true if the qualified name specified exists in the collection; otherwise, false.</returns>
	/// <param name="name">The <see cref="T:System.Xml.XmlQualifiedName" />.</param>
	public bool Contains(XmlQualifiedName name)
	{
		return table.ContainsKey(name);
	}

	/// <summary>Returns an enumerator that can iterate through the <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> that can iterate through <see cref="T:System.Xml.Schema.XmlSchemaObjectTable" />.</returns>
	public IDictionaryEnumerator GetEnumerator()
	{
		return new XSODictionaryEnumerator(entries, table.Count, EnumeratorType.DictionaryEntry);
	}
}
