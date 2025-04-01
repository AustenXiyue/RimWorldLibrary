using System.Collections;
using System.Collections.Generic;

namespace System.Xml;

/// <summary>Resolves, adds, and removes namespaces to a collection and provides scope management for these namespaces. </summary>
public class XmlNamespaceManager : IXmlNamespaceResolver, IEnumerable
{
	private struct NamespaceDeclaration
	{
		public string prefix;

		public string uri;

		public int scopeId;

		public int previousNsIndex;

		public void Set(string prefix, string uri, int scopeId, int previousNsIndex)
		{
			this.prefix = prefix;
			this.uri = uri;
			this.scopeId = scopeId;
			this.previousNsIndex = previousNsIndex;
		}
	}

	private static volatile IXmlNamespaceResolver s_EmptyResolver;

	private NamespaceDeclaration[] nsdecls;

	private int lastDecl;

	private XmlNameTable nameTable;

	private int scopeId;

	private Dictionary<string, int> hashTable;

	private bool useHashtable;

	private string xml;

	private string xmlNs;

	private const int MinDeclsCountForHashtable = 16;

	internal static IXmlNamespaceResolver EmptyResolver
	{
		get
		{
			if (s_EmptyResolver == null)
			{
				s_EmptyResolver = new XmlNamespaceManager(new NameTable());
			}
			return s_EmptyResolver;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this object.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNameTable" /> used by this object.</returns>
	public virtual XmlNameTable NameTable => nameTable;

	/// <summary>Gets the namespace URI for the default namespace.</summary>
	/// <returns>Returns the namespace URI for the default namespace, or String.Empty if there is no default namespace.</returns>
	public virtual string DefaultNamespace
	{
		get
		{
			string text = LookupNamespace(string.Empty);
			if (text != null)
			{
				return text;
			}
			return string.Empty;
		}
	}

	internal XmlNamespaceManager()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlNamespaceManager" /> class with the specified <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="nameTable">The <see cref="T:System.Xml.XmlNameTable" /> to use. </param>
	/// <exception cref="T:System.NullReferenceException">null is passed to the constructor </exception>
	public XmlNamespaceManager(XmlNameTable nameTable)
	{
		this.nameTable = nameTable;
		xml = nameTable.Add("xml");
		xmlNs = nameTable.Add("xmlns");
		nsdecls = new NamespaceDeclaration[8];
		string text = nameTable.Add(string.Empty);
		nsdecls[0].Set(text, text, -1, -1);
		nsdecls[1].Set(xmlNs, nameTable.Add("http://www.w3.org/2000/xmlns/"), -1, -1);
		nsdecls[2].Set(xml, nameTable.Add("http://www.w3.org/XML/1998/namespace"), 0, -1);
		lastDecl = 2;
		scopeId = 1;
	}

	/// <summary>Pushes a namespace scope onto the stack.</summary>
	public virtual void PushScope()
	{
		scopeId++;
	}

	/// <summary>Pops a namespace scope off the stack.</summary>
	/// <returns>true if there are namespace scopes left on the stack; false if there are no more namespaces to pop.</returns>
	public virtual bool PopScope()
	{
		int num = lastDecl;
		if (scopeId == 1)
		{
			return false;
		}
		while (nsdecls[num].scopeId == scopeId)
		{
			if (useHashtable)
			{
				hashTable[nsdecls[num].prefix] = nsdecls[num].previousNsIndex;
			}
			num--;
		}
		lastDecl = num;
		scopeId--;
		return true;
	}

	/// <summary>Adds the given namespace to the collection.</summary>
	/// <param name="prefix">The prefix to associate with the namespace being added. Use String.Empty to add a default namespace.Note   If the <see cref="T:System.Xml.XmlNamespaceManager" /> will be used for resolving namespaces in an XML Path Language (XPath) expression, a prefix must be specified. If an XPath expression does not include a prefix, it is assumed that the namespace Uniform Resource Identifier (URI) is the empty namespace. For more information about XPath expressions and the <see cref="T:System.Xml.XmlNamespaceManager" />, refer to the <see cref="M:System.Xml.XmlNode.SelectNodes(System.String)" /> and <see cref="M:System.Xml.XPath.XPathExpression.SetContext(System.Xml.XmlNamespaceManager)" /> methods.</param>
	/// <param name="uri">The namespace to add. </param>
	/// <exception cref="T:System.ArgumentException">The value for <paramref name="prefix" /> is "xml" or "xmlns". </exception>
	/// <exception cref="T:System.ArgumentNullException">The value for <paramref name="prefix" /> or <paramref name="uri" /> is null. </exception>
	public virtual void AddNamespace(string prefix, string uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		prefix = nameTable.Add(prefix);
		uri = nameTable.Add(uri);
		if (Ref.Equal(xml, prefix) && !uri.Equals("http://www.w3.org/XML/1998/namespace"))
		{
			throw new ArgumentException(Res.GetString("Prefix \"xml\" is reserved for use by XML and can be mapped only to namespace name \"http://www.w3.org/XML/1998/namespace\"."));
		}
		if (Ref.Equal(xmlNs, prefix))
		{
			throw new ArgumentException(Res.GetString("Prefix \"xmlns\" is reserved for use by XML."));
		}
		int num = LookupNamespaceDecl(prefix);
		int previousNsIndex = -1;
		if (num != -1)
		{
			if (nsdecls[num].scopeId == scopeId)
			{
				nsdecls[num].uri = uri;
				return;
			}
			previousNsIndex = num;
		}
		if (lastDecl == nsdecls.Length - 1)
		{
			NamespaceDeclaration[] destinationArray = new NamespaceDeclaration[nsdecls.Length * 2];
			Array.Copy(nsdecls, 0, destinationArray, 0, nsdecls.Length);
			nsdecls = destinationArray;
		}
		nsdecls[++lastDecl].Set(prefix, uri, scopeId, previousNsIndex);
		if (useHashtable)
		{
			hashTable[prefix] = lastDecl;
		}
		else if (lastDecl >= 16)
		{
			hashTable = new Dictionary<string, int>(lastDecl);
			for (int i = 0; i <= lastDecl; i++)
			{
				hashTable[nsdecls[i].prefix] = i;
			}
			useHashtable = true;
		}
	}

	/// <summary>Removes the given namespace for the given prefix.</summary>
	/// <param name="prefix">The prefix for the namespace </param>
	/// <param name="uri">The namespace to remove for the given prefix. The namespace removed is from the current namespace scope. Namespaces outside the current scope are ignored. </param>
	/// <exception cref="T:System.ArgumentNullException">The value of <paramref name="prefix" /> or <paramref name="uri" /> is null. </exception>
	public virtual void RemoveNamespace(string prefix, string uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		for (int num = LookupNamespaceDecl(prefix); num != -1; num = nsdecls[num].previousNsIndex)
		{
			if (string.Equals(nsdecls[num].uri, uri) && nsdecls[num].scopeId == scopeId)
			{
				nsdecls[num].uri = null;
			}
		}
	}

	/// <summary>Returns an enumerator to use to iterate through the namespaces in the <see cref="T:System.Xml.XmlNamespaceManager" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> containing the prefixes stored by the <see cref="T:System.Xml.XmlNamespaceManager" />.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(lastDecl + 1);
		for (int i = 0; i <= lastDecl; i++)
		{
			if (nsdecls[i].uri != null)
			{
				dictionary[nsdecls[i].prefix] = nsdecls[i].prefix;
			}
		}
		return dictionary.Keys.GetEnumerator();
	}

	/// <summary>Gets a collection of namespace names keyed by prefix which can be used to enumerate the namespaces currently in scope.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringDictionary" /> object containing a collection of namespace and prefix pairs currently in scope.</returns>
	/// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope" /> value that specifies the type of namespace nodes to return.</param>
	public virtual IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
	{
		int i = 0;
		switch (scope)
		{
		case XmlNamespaceScope.All:
			i = 2;
			break;
		case XmlNamespaceScope.ExcludeXml:
			i = 3;
			break;
		case XmlNamespaceScope.Local:
			i = lastDecl;
			while (nsdecls[i].scopeId == scopeId)
			{
				i--;
			}
			i++;
			break;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(lastDecl - i + 1);
		for (; i <= lastDecl; i++)
		{
			string prefix = nsdecls[i].prefix;
			string uri = nsdecls[i].uri;
			if (uri != null)
			{
				if (uri.Length > 0 || prefix.Length > 0 || scope == XmlNamespaceScope.Local)
				{
					dictionary[prefix] = uri;
				}
				else
				{
					dictionary.Remove(prefix);
				}
			}
		}
		return dictionary;
	}

	/// <summary>Gets the namespace URI for the specified prefix.</summary>
	/// <returns>Returns the namespace URI for <paramref name="prefix" /> or null if there is no mapped namespace. The returned string is atomized.For more information on atomized strings, see <see cref="T:System.Xml.XmlNameTable" />.</returns>
	/// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass String.Empty. </param>
	public virtual string LookupNamespace(string prefix)
	{
		int num = LookupNamespaceDecl(prefix);
		if (num != -1)
		{
			return nsdecls[num].uri;
		}
		return null;
	}

	private int LookupNamespaceDecl(string prefix)
	{
		if (useHashtable)
		{
			if (hashTable.TryGetValue(prefix, out var value))
			{
				while (value != -1 && nsdecls[value].uri == null)
				{
					value = nsdecls[value].previousNsIndex;
				}
				return value;
			}
			return -1;
		}
		for (int num = lastDecl; num >= 0; num--)
		{
			if ((object)nsdecls[num].prefix == prefix && nsdecls[num].uri != null)
			{
				return num;
			}
		}
		for (int num2 = lastDecl; num2 >= 0; num2--)
		{
			if (string.Equals(nsdecls[num2].prefix, prefix) && nsdecls[num2].uri != null)
			{
				return num2;
			}
		}
		return -1;
	}

	/// <summary>Finds the prefix declared for the given namespace URI.</summary>
	/// <returns>The matching prefix. If there is no mapped prefix, the method returns String.Empty. If a null value is supplied, then null is returned.</returns>
	/// <param name="uri">The namespace to resolve for the prefix. </param>
	public virtual string LookupPrefix(string uri)
	{
		for (int num = lastDecl; num >= 0; num--)
		{
			if (string.Equals(nsdecls[num].uri, uri))
			{
				string prefix = nsdecls[num].prefix;
				if (string.Equals(LookupNamespace(prefix), uri))
				{
					return prefix;
				}
			}
		}
		return null;
	}

	/// <summary>Gets a value indicating whether the supplied prefix has a namespace defined for the current pushed scope.</summary>
	/// <returns>true if there is a namespace defined; otherwise, false.</returns>
	/// <param name="prefix">The prefix of the namespace you want to find. </param>
	public virtual bool HasNamespace(string prefix)
	{
		int num = lastDecl;
		while (nsdecls[num].scopeId == scopeId)
		{
			if (string.Equals(nsdecls[num].prefix, prefix) && nsdecls[num].uri != null)
			{
				if (prefix.Length > 0 || nsdecls[num].uri.Length > 0)
				{
					return true;
				}
				return false;
			}
			num--;
		}
		return false;
	}

	internal bool GetNamespaceDeclaration(int idx, out string prefix, out string uri)
	{
		idx = lastDecl - idx;
		if (idx < 0)
		{
			prefix = (uri = null);
			return false;
		}
		prefix = nsdecls[idx].prefix;
		uri = nsdecls[idx].uri;
		return true;
	}
}
