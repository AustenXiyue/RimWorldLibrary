using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml.MS.Impl;
using MS.Internal.Xaml.Parser;

namespace System.Xaml.Schema;

internal class XamlNamespace
{
	public readonly XamlSchemaContext SchemaContext;

	private List<AssemblyNamespacePair> _assemblyNamespaces;

	private ConcurrentDictionary<string, XamlType> _typeCache;

	private ICollection<XamlType> _allPublicTypes;

	public bool IsClrNamespace { get; private set; }

	public bool IsResolved => _assemblyNamespaces != null;

	internal int RevisionNumber
	{
		get
		{
			if (_assemblyNamespaces == null)
			{
				return 0;
			}
			return _assemblyNamespaces.Count;
		}
	}

	public XamlNamespace(XamlSchemaContext schemaContext)
	{
		SchemaContext = schemaContext;
	}

	public XamlNamespace(XamlSchemaContext schemaContext, string clrNs, string assemblyName)
	{
		SchemaContext = schemaContext;
		_assemblyNamespaces = GetClrNamespacePair(clrNs, assemblyName);
		if (_assemblyNamespaces != null)
		{
			Initialize();
		}
		IsClrNamespace = true;
	}

	private void Initialize()
	{
		_typeCache = XamlSchemaContext.CreateDictionary<string, XamlType>();
	}

	public ICollection<XamlType> GetAllXamlTypes()
	{
		return _allPublicTypes ?? (_allPublicTypes = LookupAllTypes());
	}

	public XamlType GetXamlType(string typeName, params XamlType[] typeArgs)
	{
		if (!IsResolved)
		{
			return null;
		}
		if (typeArgs == null || typeArgs.Length == 0)
		{
			return TryGetXamlType(typeName) ?? TryGetXamlType(GetTypeExtensionName(typeName));
		}
		Type[] typeArgs2 = ConvertArrayOfXamlTypesToTypes(typeArgs);
		return TryGetXamlType(typeName, typeArgs2) ?? TryGetXamlType(GetTypeExtensionName(typeName), typeArgs2);
	}

	private XamlType TryGetXamlType(string typeName)
	{
		if (_typeCache.TryGetValue(typeName, out var value))
		{
			return value;
		}
		Type type = TryGetType(typeName);
		if (type == null)
		{
			return null;
		}
		value = SchemaContext.GetXamlType(type);
		if (value == null)
		{
			return null;
		}
		return XamlSchemaContext.TryAdd(_typeCache, typeName, value);
	}

	private XamlType TryGetXamlType(string typeName, Type[] typeArgs)
	{
		typeName = GenericTypeNameScanner.StripSubscript(typeName, out var subscript);
		typeName = MangleGenericTypeName(typeName, typeArgs.Length);
		Type type = TryGetXamlType(typeName)?.UnderlyingType;
		if (type == null)
		{
			return null;
		}
		Type type2 = type.MakeGenericType(typeArgs);
		if (!string.IsNullOrEmpty(subscript))
		{
			type2 = MakeArrayType(type2, subscript);
			if (type2 == null)
			{
				return null;
			}
		}
		return SchemaContext.GetXamlType(type2);
	}

	private static Type MakeArrayType(Type elementType, string subscript)
	{
		Type type = elementType;
		int pos = 0;
		do
		{
			int num = GenericTypeNameScanner.ParseSubscriptSegment(subscript, ref pos);
			Type type2;
			switch (num)
			{
			case 0:
				return null;
			default:
				type2 = type.MakeArrayType(num);
				break;
			case 1:
				type2 = type.MakeArrayType();
				break;
			}
			type = type2;
		}
		while (pos < subscript.Length);
		return type;
	}

	private static string MangleGenericTypeName(string typeName, int paramNum)
	{
		return typeName + "`" + paramNum;
	}

	private Type[] ConvertArrayOfXamlTypesToTypes(XamlType[] typeArgs)
	{
		Type[] array = new Type[typeArgs.Length];
		for (int i = 0; i < typeArgs.Length; i++)
		{
			array[i] = typeArgs[i].UnderlyingType;
		}
		return array;
	}

	private Type TryGetType(string typeName)
	{
		Type type = SearchAssembliesForShortName(typeName);
		if (type == null && IsClrNamespace)
		{
			type = XamlLanguage.LookupClrNamespaceType(_assemblyNamespaces[0], typeName);
		}
		if (type == null)
		{
			return null;
		}
		Type type2 = type;
		while (type2.IsNested)
		{
			if (type2.IsNestedPrivate)
			{
				return null;
			}
			type2 = type2.DeclaringType;
		}
		return type;
	}

	private ICollection<XamlType> LookupAllTypes()
	{
		List<XamlType> list = new List<XamlType>();
		if (IsResolved)
		{
			foreach (AssemblyNamespacePair assemblyNamespace in _assemblyNamespaces)
			{
				Assembly assembly = assemblyNamespace.Assembly;
				if (assembly == null)
				{
					continue;
				}
				string clrNamespace = assemblyNamespace.ClrNamespace;
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (KS.Eq(type.Namespace, clrNamespace))
					{
						XamlType xamlType = SchemaContext.GetXamlType(type);
						list.Add(xamlType);
					}
				}
			}
		}
		return list.AsReadOnly();
	}

	private List<AssemblyNamespacePair> GetClrNamespacePair(string clrNs, string assemblyName)
	{
		Assembly assembly = SchemaContext.OnAssemblyResolve(assemblyName);
		if (assembly == null)
		{
			return null;
		}
		return new List<AssemblyNamespacePair>
		{
			new AssemblyNamespacePair(assembly, clrNs)
		};
	}

	private Type SearchAssembliesForShortName(string shortName)
	{
		foreach (AssemblyNamespacePair assemblyNamespace in _assemblyNamespaces)
		{
			Assembly assembly = assemblyNamespace.Assembly;
			if (!(assembly == null))
			{
				string name = assemblyNamespace.ClrNamespace + "." + shortName;
				Type type = assembly.GetType(name);
				if (type != null)
				{
					return type;
				}
			}
		}
		return null;
	}

	internal void AddAssemblyNamespacePair(AssemblyNamespacePair pair)
	{
		List<AssemblyNamespacePair> list;
		if (_assemblyNamespaces == null)
		{
			list = new List<AssemblyNamespacePair>();
			Initialize();
		}
		else
		{
			list = new List<AssemblyNamespacePair>(_assemblyNamespaces);
		}
		list.Add(pair);
		_assemblyNamespaces = list;
	}

	private string GetTypeExtensionName(string typeName)
	{
		return typeName + "Extension";
	}
}
