using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Xaml;
using System.Xml;
using Microsoft.Win32;

namespace System.Windows.Markup;

internal class RestrictiveXamlXmlReader : XamlXmlReader
{
	private const string AllowedTypesForRestrictiveXamlContexts = "SOFTWARE\\Microsoft\\.NETFramework\\Windows Presentation Foundation\\XPSAllowedTypes";

	private static readonly HashSet<string> AllXamlNamespaces = new HashSet<string>(XamlLanguage.XamlNamespaces);

	private static readonly Type DependencyObjectType = typeof(DependencyObject);

	private static readonly HashSet<string> SafeTypesFromRegistry = ReadAllowedTypesForRestrictedXamlContexts();

	private HashSet<Type> _safeTypesSet = new HashSet<Type>
	{
		typeof(ResourceDictionary),
		typeof(StaticResourceExtension),
		typeof(FigureStructure),
		typeof(ListItemStructure),
		typeof(ListStructure),
		typeof(NamedElement),
		typeof(ParagraphStructure),
		typeof(SectionStructure),
		typeof(StoryBreak),
		typeof(StoryFragment),
		typeof(StoryFragments),
		typeof(TableCellStructure),
		typeof(TableRowGroupStructure),
		typeof(TableRowStructure),
		typeof(TableStructure),
		typeof(LinkTarget)
	};

	private static HashSet<string> ReadAllowedTypesForRestrictedXamlContexts()
	{
		HashSet<string> hashSet = new HashSet<string>();
		try
		{
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			if (registryKey != null)
			{
				using RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework\\Windows Presentation Foundation\\XPSAllowedTypes", writable: false);
				if (registryKey2 != null)
				{
					string[] valueNames = registryKey2.GetValueNames();
					foreach (string name in valueNames)
					{
						object value = registryKey2.GetValue(name);
						if (value != null)
						{
							hashSet.Add(value.ToString());
						}
					}
				}
			}
		}
		catch
		{
		}
		return hashSet;
	}

	public RestrictiveXamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
		: base(xmlReader, schemaContext, settings)
	{
	}

	internal RestrictiveXamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings, List<Type> safeTypes)
		: base(xmlReader, schemaContext, settings)
	{
		if (safeTypes == null)
		{
			return;
		}
		foreach (Type safeType in safeTypes)
		{
			_safeTypesSet.Add(safeType);
		}
	}

	public override bool Read()
	{
		int num = 0;
		bool result;
		while (result = base.Read())
		{
			if (num <= 0)
			{
				if ((NodeType != System.Xaml.XamlNodeType.StartObject || IsAllowedType(Type.UnderlyingType)) && (NodeType != System.Xaml.XamlNodeType.StartMember || !(Member is XamlDirective directive) || IsAllowedDirective(directive)))
				{
					break;
				}
				num = 1;
				continue;
			}
			switch (NodeType)
			{
			case System.Xaml.XamlNodeType.StartObject:
			case System.Xaml.XamlNodeType.GetObject:
			case System.Xaml.XamlNodeType.StartMember:
				num++;
				break;
			case System.Xaml.XamlNodeType.EndObject:
			case System.Xaml.XamlNodeType.EndMember:
				num--;
				break;
			}
		}
		return result;
	}

	private bool IsAllowedDirective(XamlDirective directive)
	{
		if (SafeTypesFromRegistry.Contains("*"))
		{
			return true;
		}
		bool flag = false;
		foreach (string xamlNamespace in directive.GetXamlNamespaces())
		{
			if (AllXamlNamespaces.Contains(xamlNamespace))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return true;
		}
		if (directive.Name == XamlLanguage.Items.Name || directive.Name == XamlLanguage.Key.Name || directive.Name == XamlLanguage.Name.Name || Member == XamlLanguage.PositionalParameters)
		{
			return true;
		}
		return false;
	}

	private bool IsAllowedType(Type type)
	{
		if ((object)type == null || SafeTypesFromRegistry.Contains("*") || _safeTypesSet.Contains(type) || SafeTypesFromRegistry.Contains(type.FullName))
		{
			return true;
		}
		bool flag = type.Namespace != null && (type.Namespace.Equals("System.Windows", StringComparison.Ordinal) || type.Namespace.StartsWith("System.Windows.", StringComparison.Ordinal));
		bool flag2 = type.IsSubclassOf(DependencyObjectType);
		if (type.IsPrimitive || (flag && flag2))
		{
			_safeTypesSet.Add(type);
			return true;
		}
		return false;
	}
}
