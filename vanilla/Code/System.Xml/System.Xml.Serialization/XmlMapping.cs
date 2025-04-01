using Unity;

namespace System.Xml.Serialization;

/// <summary>Supports mappings between .NET Framework types and XML Schema data types. </summary>
public abstract class XmlMapping
{
	private TypeScope scope;

	private bool generateSerializer;

	private bool isSoap;

	private ElementAccessor accessor;

	private string key;

	private bool shallow;

	private XmlMappingAccess access;

	internal ElementAccessor Accessor => accessor;

	internal TypeScope Scope => scope;

	/// <summary>Get the name of the mapped element.</summary>
	/// <returns>The name of the mapped element.</returns>
	public string ElementName => System.Xml.Serialization.Accessor.UnescapeName(Accessor.Name);

	/// <summary>Gets the name of the XSD element of the mapping.</summary>
	/// <returns>The XSD element name.</returns>
	public string XsdElementName => Accessor.Name;

	/// <summary>Gets the namespace of the mapped element.</summary>
	/// <returns>The namespace of the mapped element.</returns>
	public string Namespace => accessor.Namespace;

	internal bool GenerateSerializer
	{
		get
		{
			return generateSerializer;
		}
		set
		{
			generateSerializer = value;
		}
	}

	internal bool IsReadable => (access & XmlMappingAccess.Read) != 0;

	internal bool IsWriteable => (access & XmlMappingAccess.Write) != 0;

	internal bool IsSoap
	{
		get
		{
			return isSoap;
		}
		set
		{
			isSoap = value;
		}
	}

	internal string Key => key;

	internal XmlMapping(TypeScope scope, ElementAccessor accessor)
		: this(scope, accessor, XmlMappingAccess.Read | XmlMappingAccess.Write)
	{
	}

	internal XmlMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access)
	{
		this.scope = scope;
		this.accessor = accessor;
		this.access = access;
		shallow = scope == null;
	}

	/// <summary>Sets the key used to look up the mapping.</summary>
	/// <param name="key">A <see cref="T:System.String" /> that contains the lookup key.</param>
	public void SetKey(string key)
	{
		SetKeyInternal(key);
	}

	internal void SetKeyInternal(string key)
	{
		this.key = key;
	}

	internal static string GenerateKey(Type type, XmlRootAttribute root, string ns)
	{
		if (root == null)
		{
			root = (XmlRootAttribute)XmlAttributes.GetAttr(type, typeof(XmlRootAttribute));
		}
		return type.FullName + ":" + ((root == null) ? string.Empty : root.Key) + ":" + ((ns == null) ? string.Empty : ns);
	}

	internal void CheckShallow()
	{
		if (shallow)
		{
			throw new InvalidOperationException(Res.GetString("This mapping was not crated by reflection importer and cannot be used in this context."));
		}
	}

	internal static bool IsShallow(XmlMapping[] mappings)
	{
		for (int i = 0; i < mappings.Length; i++)
		{
			if (mappings[i] == null || mappings[i].shallow)
			{
				return true;
			}
		}
		return false;
	}

	internal XmlMapping()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
