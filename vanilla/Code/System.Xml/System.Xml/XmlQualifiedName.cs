using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System.Xml;

/// <summary>Represents an XML qualified name.</summary>
[Serializable]
public class XmlQualifiedName
{
	private delegate int HashCodeOfStringDelegate(string s, int sLen, long additionalEntropy);

	private static HashCodeOfStringDelegate hashCodeDelegate = null;

	private string name;

	private string ns;

	[NonSerialized]
	private int hash;

	/// <summary>Provides an empty <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	public static readonly XmlQualifiedName Empty = new XmlQualifiedName(string.Empty);

	/// <summary>Gets a string representation of the namespace of the <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	/// <returns>A string representation of the namespace or String.Empty if a namespace is not defined for the object.</returns>
	public string Namespace => ns;

	/// <summary>Gets a string representation of the qualified name of the <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	/// <returns>A string representation of the qualified name or String.Empty if a name is not defined for the object.</returns>
	public string Name => name;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlQualifiedName" /> is empty.</summary>
	/// <returns>true if name and namespace are empty strings; otherwise, false.</returns>
	public bool IsEmpty
	{
		get
		{
			if (Name.Length == 0)
			{
				return Namespace.Length == 0;
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlQualifiedName" /> class.</summary>
	public XmlQualifiedName()
		: this(string.Empty, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlQualifiedName" /> class with the specified name.</summary>
	/// <param name="name">The local name to use as the name of the <see cref="T:System.Xml.XmlQualifiedName" /> object. </param>
	public XmlQualifiedName(string name)
		: this(name, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlQualifiedName" /> class with the specified name and namespace.</summary>
	/// <param name="name">The local name to use as the name of the <see cref="T:System.Xml.XmlQualifiedName" /> object. </param>
	/// <param name="ns">The namespace for the <see cref="T:System.Xml.XmlQualifiedName" /> object. </param>
	public XmlQualifiedName(string name, string ns)
	{
		this.ns = ((ns == null) ? string.Empty : ns);
		this.name = ((name == null) ? string.Empty : name);
	}

	/// <summary>Returns the hash code for the <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	/// <returns>A hash code for this object.</returns>
	public override int GetHashCode()
	{
		if (hash == 0)
		{
			if (hashCodeDelegate == null)
			{
				hashCodeDelegate = GetHashCodeDelegate();
			}
			hash = hashCodeDelegate(Name, Name.Length, 0L);
		}
		return hash;
	}

	/// <summary>Returns the string value of the <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	/// <returns>The string value of the <see cref="T:System.Xml.XmlQualifiedName" /> in the format of namespace:localname. If the object does not have a namespace defined, this method returns just the local name.</returns>
	public override string ToString()
	{
		if (Namespace.Length != 0)
		{
			return Namespace + ":" + Name;
		}
		return Name;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Xml.XmlQualifiedName" /> object is equal to the current <see cref="T:System.Xml.XmlQualifiedName" /> object. </summary>
	/// <returns>true if the two are the same instance object; otherwise, false.</returns>
	/// <param name="other">The <see cref="T:System.Xml.XmlQualifiedName" /> to compare. </param>
	public override bool Equals(object other)
	{
		if (this == other)
		{
			return true;
		}
		XmlQualifiedName xmlQualifiedName = other as XmlQualifiedName;
		if (xmlQualifiedName != null)
		{
			if (Name == xmlQualifiedName.Name)
			{
				return Namespace == xmlQualifiedName.Namespace;
			}
			return false;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Xml.XmlQualifiedName" /> objects.</summary>
	/// <returns>true if the two objects have the same name and namespace values; otherwise, false.</returns>
	/// <param name="a">An <see cref="T:System.Xml.XmlQualifiedName" /> to compare. </param>
	/// <param name="b">An <see cref="T:System.Xml.XmlQualifiedName" /> to compare. </param>
	public static bool operator ==(XmlQualifiedName a, XmlQualifiedName b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		if (a.Name == b.Name)
		{
			return a.Namespace == b.Namespace;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Xml.XmlQualifiedName" /> objects.</summary>
	/// <returns>true if the name and namespace values for the two objects differ; otherwise, false.</returns>
	/// <param name="a">An <see cref="T:System.Xml.XmlQualifiedName" /> to compare. </param>
	/// <param name="b">An <see cref="T:System.Xml.XmlQualifiedName" /> to compare. </param>
	public static bool operator !=(XmlQualifiedName a, XmlQualifiedName b)
	{
		return !(a == b);
	}

	/// <summary>Returns the string value of the <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	/// <returns>The string value of the <see cref="T:System.Xml.XmlQualifiedName" /> in the format of namespace:localname. If the object does not have a namespace defined, this method returns just the local name.</returns>
	/// <param name="name">The name of the object. </param>
	/// <param name="ns">The namespace of the object. </param>
	public static string ToString(string name, string ns)
	{
		if (ns != null && ns.Length != 0)
		{
			return ns + ":" + name;
		}
		return name;
	}

	[SecuritySafeCritical]
	[ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
	private static HashCodeOfStringDelegate GetHashCodeDelegate()
	{
		if (!IsRandomizedHashingDisabled())
		{
			MethodInfo method = typeof(string).GetMethod("InternalMarvin32HashString", BindingFlags.Static | BindingFlags.NonPublic);
			if (method != null)
			{
				return (HashCodeOfStringDelegate)Delegate.CreateDelegate(typeof(HashCodeOfStringDelegate), method);
			}
		}
		return GetHashCodeOfString;
	}

	private static bool IsRandomizedHashingDisabled()
	{
		return false;
	}

	private static int GetHashCodeOfString(string s, int length, long additionalEntropy)
	{
		return s.GetHashCode();
	}

	internal void Init(string name, string ns)
	{
		this.name = name;
		this.ns = ns;
		hash = 0;
	}

	internal void SetNamespace(string ns)
	{
		this.ns = ns;
	}

	internal void Verify()
	{
		XmlConvert.VerifyNCName(name);
		if (ns.Length != 0)
		{
			XmlConvert.ToUri(ns);
		}
	}

	internal void Atomize(XmlNameTable nameTable)
	{
		name = nameTable.Add(name);
		ns = nameTable.Add(ns);
	}

	internal static XmlQualifiedName Parse(string s, IXmlNamespaceResolver nsmgr, out string prefix)
	{
		ValidateNames.ParseQNameThrow(s, out prefix, out var localName);
		string text = nsmgr.LookupNamespace(prefix);
		if (text == null)
		{
			if (prefix.Length != 0)
			{
				throw new XmlException("'{0}' is an undeclared prefix.", prefix);
			}
			text = string.Empty;
		}
		return new XmlQualifiedName(localName, text);
	}

	internal XmlQualifiedName Clone()
	{
		return (XmlQualifiedName)MemberwiseClone();
	}

	internal static int Compare(XmlQualifiedName a, XmlQualifiedName b)
	{
		if (null == a)
		{
			if (!(null == b))
			{
				return -1;
			}
			return 0;
		}
		if (null == b)
		{
			return 1;
		}
		int num = string.CompareOrdinal(a.Namespace, b.Namespace);
		if (num == 0)
		{
			num = string.CompareOrdinal(a.Name, b.Name);
		}
		return num;
	}
}
