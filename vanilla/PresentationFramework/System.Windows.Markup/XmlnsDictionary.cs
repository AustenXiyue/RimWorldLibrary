using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Represents a dictionary that contains xmlns mappings for XAML namespaces in WPF. </summary>
public class XmlnsDictionary : IDictionary, ICollection, IEnumerable, IXamlNamespaceResolver
{
	private struct NamespaceDeclaration
	{
		public string Prefix;

		public string Uri;

		public int ScopeCount;
	}

	private enum NamespaceScope
	{
		All,
		Local
	}

	private NamespaceDeclaration[] _nsDeclarations;

	private int _lastDecl;

	private int _countDecl;

	private bool _sealed;

	/// <summary>Gets a value that indicates whether the size of the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is fixed. </summary>
	/// <returns>true if the size is fixed; otherwise, false. </returns>
	public bool IsFixedSize => false;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is read-only. </summary>
	/// <returns>true if the dictionary is read-only; otherwise, false.</returns>
	public bool IsReadOnly => _sealed;

	/// <summary>Gets or sets the XAML namespace URI associated with the specified prefix.</summary>
	/// <returns>The corresponding XML namespace URI.</returns>
	/// <param name="prefix">The prefix from which to get or set the associated namespace.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefix" /> is null-or-The value to set is null.</exception>
	public string this[string prefix]
	{
		get
		{
			return LookupNamespace(prefix);
		}
		set
		{
			AddNamespace(prefix, value);
		}
	}

	/// <summary>Gets or sets the XAML namespace URI associated with the specified prefix.</summary>
	/// <returns>The corresponding XAML namespace URI.</returns>
	/// <param name="prefix">The prefix from which to get or set the associated XML namespace URI.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="prefix" /> is not a string-or-The value to set is not a string.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefix" /> is null-or-The value to set is null.</exception>
	public object this[object prefix]
	{
		get
		{
			if (!(prefix is string))
			{
				throw new ArgumentException(SR.ParserKeysAreStrings);
			}
			return LookupNamespace((string)prefix);
		}
		set
		{
			if (!(prefix is string) || !(value is string))
			{
				throw new ArgumentException(SR.ParserKeysAreStrings);
			}
			AddNamespace((string)prefix, (string)value);
		}
	}

	/// <summary>Gets a collection of all the keys in the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <returns>The collection of all the keys in the dictionary.</returns>
	public ICollection Keys
	{
		get
		{
			ArrayList arrayList = new ArrayList(_lastDecl + 1);
			for (int i = 0; i < _lastDecl; i++)
			{
				if (_nsDeclarations[i].Uri != null && !arrayList.Contains(_nsDeclarations[i].Prefix))
				{
					arrayList.Add(_nsDeclarations[i].Prefix);
				}
			}
			return arrayList;
		}
	}

	/// <summary>Gets a collection of all the values in the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <returns>A collection of all the values in the <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</returns>
	public ICollection Values
	{
		get
		{
			HybridDictionary hybridDictionary = new HybridDictionary(_lastDecl + 1);
			for (int i = 0; i < _lastDecl; i++)
			{
				if (_nsDeclarations[i].Uri != null)
				{
					hybridDictionary[_nsDeclarations[i].Prefix] = _nsDeclarations[i].Uri;
				}
			}
			return hybridDictionary.Values;
		}
	}

	/// <summary>Gets the number of items in the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <returns>The number of items in the <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</returns>
	public int Count => _countDecl;

	/// <summary>Gets a value that indicates whether access to this <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is thread safe. </summary>
	/// <returns>true if access to this dictionary is thread-safe; otherwise, false.</returns>
	public bool IsSynchronized => _nsDeclarations.IsSynchronized;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</returns>
	public object SyncRoot => _nsDeclarations.SyncRoot;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed. </summary>
	/// <returns>true if the dictionary is sealed; otherwise, false.</returns>
	public bool Sealed => _sealed;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> class.</summary>
	public XmlnsDictionary()
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> class by using the specified dictionary as a copy source.</summary>
	/// <param name="xmlnsDictionary">The dictionary on which to base the new <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlnsDictionary" /> is null.</exception>
	public XmlnsDictionary(XmlnsDictionary xmlnsDictionary)
	{
		if (xmlnsDictionary == null)
		{
			throw new ArgumentNullException("xmlnsDictionary");
		}
		if (xmlnsDictionary != null && xmlnsDictionary.Count > 0)
		{
			_lastDecl = xmlnsDictionary._lastDecl;
			if (_nsDeclarations == null)
			{
				_nsDeclarations = new NamespaceDeclaration[_lastDecl + 1];
			}
			_countDecl = 0;
			for (int i = 0; i <= _lastDecl; i++)
			{
				if (xmlnsDictionary._nsDeclarations[i].Uri != null)
				{
					_countDecl++;
				}
				_nsDeclarations[i].Prefix = xmlnsDictionary._nsDeclarations[i].Prefix;
				_nsDeclarations[i].Uri = xmlnsDictionary._nsDeclarations[i].Uri;
				_nsDeclarations[i].ScopeCount = xmlnsDictionary._nsDeclarations[i].ScopeCount;
			}
		}
		else
		{
			Initialize();
		}
	}

	/// <summary>Adds a prefix-URI pair to this <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</summary>
	/// <param name="prefix">The prefix of the XAML namespace to be added. </param>
	/// <param name="xmlNamespace">The XAML namespace URI the prefix maps to. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="prefix" /> or <paramref name="xmlNamespace" /> is not a string. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefix" /> or <paramref name="xmlNamespace" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void Add(object prefix, object xmlNamespace)
	{
		if (!(prefix is string) || !(xmlNamespace is string))
		{
			throw new ArgumentException(SR.ParserKeysAreStrings);
		}
		AddNamespace((string)prefix, (string)xmlNamespace);
	}

	/// <summary>Adds a prefix-URI pair to this <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</summary>
	/// <param name="prefix">The prefix of this XML namespace.</param>
	/// <param name="xmlNamespace">The XML namespace URI the prefix maps to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefix" /> or <paramref name="xmlNamespace" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void Add(string prefix, string xmlNamespace)
	{
		AddNamespace(prefix, xmlNamespace);
	}

	/// <summary>Removes all entries from this <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void Clear()
	{
		CheckSealed();
		_lastDecl = 0;
		_countDecl = 0;
	}

	/// <summary>Returns a value that indicates whether the specified prefix key is in this <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <returns>true if the requested prefix key is in the dictionary; otherwise, false.</returns>
	/// <param name="key">The prefix key to search for.</param>
	public bool Contains(object key)
	{
		return HasNamespace((string)key);
	}

	/// <summary>Removes the item with the specified prefix key from the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <param name="prefix">The prefix key to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void Remove(string prefix)
	{
		string xmlNamespace = LookupNamespace(prefix);
		RemoveNamespace(prefix, xmlNamespace);
	}

	/// <summary>Removes the item with the specified prefix key from the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <param name="prefix">The prefix key to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void Remove(object prefix)
	{
		Remove((string)prefix);
	}

	/// <summary>Copies the entries in the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> to the specified <see cref="T:System.Collections.DictionaryEntry" /> array. </summary>
	/// <param name="array">The array to copy the table data into.</param>
	/// <param name="index">The zero-based index in the destination array where copying starts.</param>
	public void CopyTo(DictionaryEntry[] array, int index)
	{
		CopyTo((Array)array, index);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object that can be used to iterate through the collection.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		HybridDictionary hybridDictionary = new HybridDictionary(_lastDecl);
		for (int i = 0; i < _lastDecl; i++)
		{
			if (_nsDeclarations[i].Uri != null)
			{
				hybridDictionary[_nsDeclarations[i].Prefix] = _nsDeclarations[i].Uri;
			}
		}
		return hybridDictionary.GetEnumerator();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Copies the entries in the <see cref="T:System.Windows.Markup.XmlnsDictionary" /> to the specified array. </summary>
	/// <param name="array">The array to copy the table data into.</param>
	/// <param name="index">The zero-based index in the destination array where copying starts.</param>
	public void CopyTo(Array array, int index)
	{
		GetNamespacesInScope(NamespaceScope.All)?.CopyTo(array, index);
	}

	/// <summary>Retrieves a XAML namespace for the provided prefix string.</summary>
	/// <returns>The requested XAML namespace URI.</returns>
	/// <param name="prefix">The prefix to retrieve the XAML namespace for.</param>
	public string GetNamespace(string prefix)
	{
		return LookupNamespace(prefix);
	}

	/// <summary>Returns all possible prefix-XAML namespace mappings (<see cref="T:System.Xaml.NamespaceDeclaration" /> values) that are available in the active schema context.</summary>
	/// <returns>An enumerable set of <see cref="T:System.Xaml.NamespaceDeclaration" /> values. To get the prefix strings specifically, get the <see cref="P:System.Xaml.NamespaceDeclaration.Prefix" /> value from each value returned.</returns>
	public IEnumerable<System.Xaml.NamespaceDeclaration> GetNamespacePrefixes()
	{
		if (_lastDecl > 0)
		{
			for (int i = _lastDecl - 1; i >= 0; i--)
			{
				yield return new System.Xaml.NamespaceDeclaration(_nsDeclarations[i].Uri, _nsDeclarations[i].Prefix);
			}
		}
	}

	/// <summary>Returns a dictionary enumerator that iterates through this <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</summary>
	/// <returns>The dictionary enumerator for this dictionary.</returns>
	protected IDictionaryEnumerator GetDictionaryEnumerator()
	{
		HybridDictionary hybridDictionary = new HybridDictionary(_lastDecl);
		for (int i = 0; i < _lastDecl; i++)
		{
			if (_nsDeclarations[i].Uri != null)
			{
				hybridDictionary[_nsDeclarations[i].Prefix] = _nsDeclarations[i].Uri;
			}
		}
		return hybridDictionary.GetEnumerator();
	}

	/// <summary>Returns an enumerator that iterates through this <see cref="T:System.Windows.Markup.XmlnsDictionary" />.</summary>
	/// <returns>The enumerator for this dictionary.</returns>
	protected IEnumerator GetEnumerator()
	{
		return Keys.GetEnumerator();
	}

	/// <summary>Locks the dictionary so that it cannot be changed. </summary>
	public void Seal()
	{
		_sealed = true;
	}

	/// <summary>Returns the XAML namespace URI that corresponds to the specified XML namespace prefix. </summary>
	/// <returns>The XAML namespace URI that corresponds to the specified prefix if it exists in this <see cref="T:System.Windows.Markup.XmlnsDictionary" />; otherwise, null.</returns>
	/// <param name="prefix">The XAML namespace prefix to look up.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="prefix" /> is null.</exception>
	public string LookupNamespace(string prefix)
	{
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		if (_lastDecl > 0)
		{
			for (int num = _lastDecl - 1; num >= 0; num--)
			{
				if (_nsDeclarations[num].Prefix == prefix && !string.IsNullOrEmpty(_nsDeclarations[num].Uri))
				{
					return _nsDeclarations[num].Uri;
				}
			}
		}
		return null;
	}

	/// <summary>Returns the prefix that corresponds to the specified XAML namespace URI. </summary>
	/// <returns>The XML prefix that corresponds to the given namespace; otherwise, null.</returns>
	/// <param name="xmlNamespace">The XAML namespace URI to look up.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlNamespace" /> is null.</exception>
	public string LookupPrefix(string xmlNamespace)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (_lastDecl > 0)
		{
			for (int num = _lastDecl - 1; num >= 0; num--)
			{
				if (_nsDeclarations[num].Uri == xmlNamespace)
				{
					return _nsDeclarations[num].Prefix;
				}
			}
		}
		return null;
	}

	/// <summary>Looks up the XAML namespace that corresponds to the default XAML namespace. </summary>
	/// <returns>The namespace that corresponds to the default XML namespace if one exists; otherwise, null.</returns>
	public string DefaultNamespace()
	{
		string text = LookupNamespace(string.Empty);
		if (text != null)
		{
			return text;
		}
		return string.Empty;
	}

	/// <summary>Pushes the scope of the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void PushScope()
	{
		CheckSealed();
		_nsDeclarations[_lastDecl].ScopeCount++;
	}

	/// <summary>Pops the scope of the <see cref="T:System.Windows.Markup.XmlnsDictionary" />. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Markup.XmlnsDictionary" /> is sealed.</exception>
	public void PopScope()
	{
		CheckSealed();
		int scopeCount = _nsDeclarations[_lastDecl].ScopeCount;
		int num = _lastDecl;
		while (num > 0 && _nsDeclarations[num - 1].ScopeCount == scopeCount)
		{
			num--;
		}
		if (_nsDeclarations[num].ScopeCount > 0)
		{
			_nsDeclarations[num].ScopeCount--;
			_nsDeclarations[num].Prefix = string.Empty;
			_nsDeclarations[num].Uri = null;
		}
		_lastDecl = num;
	}

	internal void Unseal()
	{
		_sealed = false;
	}

	private void Initialize()
	{
		_nsDeclarations = new NamespaceDeclaration[8];
		_nsDeclarations[0].Prefix = string.Empty;
		_nsDeclarations[0].Uri = null;
		_nsDeclarations[0].ScopeCount = 0;
		_lastDecl = 0;
		_countDecl = 0;
	}

	private void CheckSealed()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.ParserDictionarySealed);
		}
	}

	private void AddNamespace(string prefix, string xmlNamespace)
	{
		CheckSealed();
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		int scopeCount = _nsDeclarations[_lastDecl].ScopeCount;
		if (_lastDecl > 0)
		{
			int num = _lastDecl - 1;
			while (num >= 0 && _nsDeclarations[num].ScopeCount == scopeCount)
			{
				if (string.Equals(_nsDeclarations[num].Prefix, prefix))
				{
					_nsDeclarations[num].Uri = xmlNamespace;
					return;
				}
				num--;
			}
			if (_lastDecl == _nsDeclarations.Length - 1)
			{
				NamespaceDeclaration[] array = new NamespaceDeclaration[_nsDeclarations.Length * 2];
				Array.Copy(_nsDeclarations, 0, array, 0, _nsDeclarations.Length);
				_nsDeclarations = array;
			}
		}
		_countDecl++;
		_nsDeclarations[_lastDecl].Prefix = prefix;
		_nsDeclarations[_lastDecl].Uri = xmlNamespace;
		_lastDecl++;
		_nsDeclarations[_lastDecl].ScopeCount = scopeCount;
	}

	private void RemoveNamespace(string prefix, string xmlNamespace)
	{
		CheckSealed();
		if (_lastDecl <= 0)
		{
			return;
		}
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		int scopeCount = _nsDeclarations[_lastDecl - 1].ScopeCount;
		int num = _lastDecl - 1;
		while (num >= 0 && _nsDeclarations[num].ScopeCount == scopeCount)
		{
			if (_nsDeclarations[num].Prefix == prefix && _nsDeclarations[num].Uri == xmlNamespace)
			{
				_nsDeclarations[num].Uri = null;
				_countDecl--;
			}
			num--;
		}
	}

	private IDictionary GetNamespacesInScope(NamespaceScope scope)
	{
		int i = 0;
		switch (scope)
		{
		case NamespaceScope.All:
			i = 0;
			break;
		case NamespaceScope.Local:
		{
			i = _lastDecl;
			int scopeCount = _nsDeclarations[i].ScopeCount;
			while (_nsDeclarations[i].ScopeCount == scopeCount)
			{
				i--;
			}
			i++;
			break;
		}
		}
		HybridDictionary hybridDictionary = new HybridDictionary(_lastDecl - i + 1);
		for (; i < _lastDecl; i++)
		{
			string prefix = _nsDeclarations[i].Prefix;
			string uri = _nsDeclarations[i].Uri;
			if (uri.Length > 0 || prefix.Length > 0)
			{
				hybridDictionary[prefix] = uri;
			}
			else
			{
				hybridDictionary.Remove(prefix);
			}
		}
		return hybridDictionary;
	}

	private bool HasNamespace(string prefix)
	{
		if (_lastDecl > 0)
		{
			for (int num = _lastDecl - 1; num >= 0; num--)
			{
				if (_nsDeclarations[num].Prefix == prefix && _nsDeclarations[num].Uri != null)
				{
					if (prefix.Length > 0 || _nsDeclarations[num].Uri.Length > 0)
					{
						return true;
					}
					return false;
				}
			}
		}
		return false;
	}
}
