using System.Threading;
using Unity;

namespace System.Xml.Linq;

/// <summary>Represents an XML namespace. This class cannot be inherited. </summary>
public sealed class XNamespace
{
	internal const string xmlPrefixNamespace = "http://www.w3.org/XML/1998/namespace";

	internal const string xmlnsPrefixNamespace = "http://www.w3.org/2000/xmlns/";

	private static XHashtable<WeakReference> namespaces;

	private static WeakReference refNone;

	private static WeakReference refXml;

	private static WeakReference refXmlns;

	private string namespaceName;

	private int hashCode;

	private XHashtable<XName> names;

	private const int NamesCapacity = 8;

	private const int NamespacesCapacity = 32;

	/// <summary>Gets the Uniform Resource Identifier (URI) of this namespace.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the URI of the namespace.</returns>
	public string NamespaceName => namespaceName;

	/// <summary>Gets the <see cref="T:System.Xml.Linq.XNamespace" /> object that corresponds to no namespace.</summary>
	/// <returns>The <see cref="T:System.Xml.Linq.XNamespace" /> that corresponds to no namespace.</returns>
	public static XNamespace None => EnsureNamespace(ref refNone, string.Empty);

	/// <summary>Gets the <see cref="T:System.Xml.Linq.XNamespace" /> object that corresponds to the XML URI (http://www.w3.org/XML/1998/namespace).</summary>
	/// <returns>The <see cref="T:System.Xml.Linq.XNamespace" /> that corresponds to the XML URI (http://www.w3.org/XML/1998/namespace).</returns>
	public static XNamespace Xml => EnsureNamespace(ref refXml, "http://www.w3.org/XML/1998/namespace");

	/// <summary>Gets the <see cref="T:System.Xml.Linq.XNamespace" /> object that corresponds to the xmlns URI (http://www.w3.org/2000/xmlns/).</summary>
	/// <returns>The <see cref="T:System.Xml.Linq.XNamespace" /> that corresponds to the xmlns URI (http://www.w3.org/2000/xmlns/).</returns>
	public static XNamespace Xmlns => EnsureNamespace(ref refXmlns, "http://www.w3.org/2000/xmlns/");

	internal XNamespace(string namespaceName)
	{
		this.namespaceName = namespaceName;
		hashCode = namespaceName.GetHashCode();
		names = new XHashtable<XName>(ExtractLocalName, 8);
	}

	/// <summary>Returns an <see cref="T:System.Xml.Linq.XName" /> object created from this <see cref="T:System.Xml.Linq.XNamespace" /> and the specified local name.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XName" /> created from this <see cref="T:System.Xml.Linq.XNamespace" /> and the specified local name.</returns>
	/// <param name="localName">A <see cref="T:System.String" /> that contains a local name.</param>
	public XName GetName(string localName)
	{
		if (localName == null)
		{
			throw new ArgumentNullException("localName");
		}
		return GetName(localName, 0, localName.Length);
	}

	/// <summary>Returns the URI of this <see cref="T:System.Xml.Linq.XNamespace" />.</summary>
	/// <returns>The URI of this <see cref="T:System.Xml.Linq.XNamespace" />.</returns>
	public override string ToString()
	{
		return namespaceName;
	}

	/// <summary>Gets an <see cref="T:System.Xml.Linq.XNamespace" /> for the specified Uniform Resource Identifier (URI).</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNamespace" /> created from the specified URI.</returns>
	/// <param name="namespaceName">A <see cref="T:System.String" /> that contains a namespace URI.</param>
	public static XNamespace Get(string namespaceName)
	{
		if (namespaceName == null)
		{
			throw new ArgumentNullException("namespaceName");
		}
		return Get(namespaceName, 0, namespaceName.Length);
	}

	/// <summary>Converts a string containing a Uniform Resource Identifier (URI) to an <see cref="T:System.Xml.Linq.XNamespace" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNamespace" /> constructed from the URI string.</returns>
	/// <param name="namespaceName">A <see cref="T:System.String" /> that contains the namespace URI.</param>
	[CLSCompliant(false)]
	public static implicit operator XNamespace(string namespaceName)
	{
		if (namespaceName == null)
		{
			return null;
		}
		return Get(namespaceName);
	}

	/// <summary>Combines an <see cref="T:System.Xml.Linq.XNamespace" /> object with a local name to create an <see cref="T:System.Xml.Linq.XName" />.</summary>
	/// <returns>The new <see cref="T:System.Xml.Linq.XName" /> constructed from the namespace and local name.</returns>
	/// <param name="ns">An <see cref="T:System.Xml.Linq.XNamespace" /> that contains the namespace.</param>
	/// <param name="localName">A <see cref="T:System.String" /> that contains the local name.</param>
	public static XName operator +(XNamespace ns, string localName)
	{
		if (ns == null)
		{
			throw new ArgumentNullException("ns");
		}
		return ns.GetName(localName);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Xml.Linq.XNamespace" /> is equal to the current <see cref="T:System.Xml.Linq.XNamespace" />.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that indicates whether the specified <see cref="T:System.Xml.Linq.XNamespace" /> is equal to the current <see cref="T:System.Xml.Linq.XNamespace" />.</returns>
	/// <param name="obj">The <see cref="T:System.Xml.Linq.XNamespace" /> to compare to the current <see cref="T:System.Xml.Linq.XNamespace" />.</param>
	public override bool Equals(object obj)
	{
		return this == obj;
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Xml.Linq.XNamespace" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the hash code for the <see cref="T:System.Xml.Linq.XNamespace" />.</returns>
	public override int GetHashCode()
	{
		return hashCode;
	}

	/// <summary>Returns a value indicating whether two instances of <see cref="T:System.Xml.Linq.XNamespace" /> are equal.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that indicates whether <paramref name="left" /> and <paramref name="right" /> are equal.</returns>
	/// <param name="left">The first <see cref="T:System.Xml.Linq.XNamespace" /> to compare.</param>
	/// <param name="right">The second <see cref="T:System.Xml.Linq.XNamespace" /> to compare.</param>
	public static bool operator ==(XNamespace left, XNamespace right)
	{
		return (object)left == right;
	}

	/// <summary>Returns a value indicating whether two instances of <see cref="T:System.Xml.Linq.XNamespace" /> are not equal.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that indicates whether <paramref name="left" /> and <paramref name="right" /> are not equal.</returns>
	/// <param name="left">The first <see cref="T:System.Xml.Linq.XNamespace" /> to compare.</param>
	/// <param name="right">The second <see cref="T:System.Xml.Linq.XNamespace" /> to compare.</param>
	public static bool operator !=(XNamespace left, XNamespace right)
	{
		return (object)left != right;
	}

	internal XName GetName(string localName, int index, int count)
	{
		if (names.TryGetValue(localName, index, count, out var value))
		{
			return value;
		}
		return names.Add(new XName(this, localName.Substring(index, count)));
	}

	internal static XNamespace Get(string namespaceName, int index, int count)
	{
		if (count == 0)
		{
			return None;
		}
		if (namespaces == null)
		{
			Interlocked.CompareExchange(ref namespaces, new XHashtable<WeakReference>(ExtractNamespace, 32), null);
		}
		XNamespace xNamespace;
		do
		{
			if (!namespaces.TryGetValue(namespaceName, index, count, out var value))
			{
				if (count == "http://www.w3.org/XML/1998/namespace".Length && string.CompareOrdinal(namespaceName, index, "http://www.w3.org/XML/1998/namespace", 0, count) == 0)
				{
					return Xml;
				}
				if (count == "http://www.w3.org/2000/xmlns/".Length && string.CompareOrdinal(namespaceName, index, "http://www.w3.org/2000/xmlns/", 0, count) == 0)
				{
					return Xmlns;
				}
				value = namespaces.Add(new WeakReference(new XNamespace(namespaceName.Substring(index, count))));
			}
			xNamespace = ((value != null) ? ((XNamespace)value.Target) : null);
		}
		while (xNamespace == null);
		return xNamespace;
	}

	private static string ExtractLocalName(XName n)
	{
		return n.LocalName;
	}

	private static string ExtractNamespace(WeakReference r)
	{
		XNamespace xNamespace;
		if (r == null || (xNamespace = (XNamespace)r.Target) == null)
		{
			return null;
		}
		return xNamespace.NamespaceName;
	}

	private static XNamespace EnsureNamespace(ref WeakReference refNmsp, string namespaceName)
	{
		XNamespace xNamespace;
		while (true)
		{
			WeakReference weakReference = refNmsp;
			if (weakReference != null)
			{
				xNamespace = (XNamespace)weakReference.Target;
				if (xNamespace != null)
				{
					break;
				}
			}
			Interlocked.CompareExchange(ref refNmsp, new WeakReference(new XNamespace(namespaceName)), weakReference);
		}
		return xNamespace;
	}

	internal XNamespace()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
