using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace System.Windows.Markup;

internal class XmlnsCache
{
	private HybridDictionary _cacheTable;

	private Dictionary<string, string> _compatTable;

	private Dictionary<string, string> _compatTableReverse;

	private HybridDictionary _uriToAssemblyNameTable;

	internal XmlnsCache()
	{
		_compatTable = new Dictionary<string, string>();
		_compatTableReverse = new Dictionary<string, string>();
		_cacheTable = new HybridDictionary();
		_uriToAssemblyNameTable = new HybridDictionary();
	}

	internal List<ClrNamespaceAssemblyPair> GetMappingArray(string xmlns)
	{
		List<ClrNamespaceAssemblyPair> list = null;
		lock (this)
		{
			list = _cacheTable[xmlns] as List<ClrNamespaceAssemblyPair>;
			if (list == null)
			{
				if (_uriToAssemblyNameTable[xmlns] != null)
				{
					string[] array = (string[])_uriToAssemblyNameTable[xmlns];
					Assembly[] array2 = new Assembly[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = ReflectionHelper.LoadAssembly(array[i], null);
					}
					_cacheTable[xmlns] = GetClrnsToAssemblyNameMappingList(array2, xmlns);
				}
				else
				{
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					_cacheTable[xmlns] = GetClrnsToAssemblyNameMappingList(assemblies, xmlns);
					ProcessXmlnsCompatibleWithAttributes(assemblies);
				}
				list = _cacheTable[xmlns] as List<ClrNamespaceAssemblyPair>;
			}
		}
		return list;
	}

	internal void SetUriToAssemblyNameMapping(string namespaceUri, string[] asmNameList)
	{
		_uriToAssemblyNameTable[namespaceUri] = asmNameList;
	}

	internal string GetNewXmlnamespace(string oldXmlnamespace)
	{
		if (_compatTable.TryGetValue(oldXmlnamespace, out var value))
		{
			return value;
		}
		return null;
	}

	private Attribute[] GetAttributes(Assembly asm, Type attrType)
	{
		return Attribute.GetCustomAttributes(asm, attrType);
	}

	private void GetNamespacesFromDefinitionAttr(Attribute attr, out string xmlns, out string clrns)
	{
		XmlnsDefinitionAttribute xmlnsDefinitionAttribute = (XmlnsDefinitionAttribute)attr;
		xmlns = xmlnsDefinitionAttribute.XmlNamespace;
		clrns = xmlnsDefinitionAttribute.ClrNamespace;
	}

	private void GetNamespacesFromCompatAttr(Attribute attr, out string oldXmlns, out string newXmlns)
	{
		XmlnsCompatibleWithAttribute xmlnsCompatibleWithAttribute = (XmlnsCompatibleWithAttribute)attr;
		oldXmlns = xmlnsCompatibleWithAttribute.OldNamespace;
		newXmlns = xmlnsCompatibleWithAttribute.NewNamespace;
	}

	private List<ClrNamespaceAssemblyPair> GetClrnsToAssemblyNameMappingList(Assembly[] asmList, string xmlnsRequested)
	{
		List<ClrNamespaceAssemblyPair> list = new List<ClrNamespaceAssemblyPair>();
		for (int i = 0; i < asmList.Length; i++)
		{
			string fullName = asmList[i].FullName;
			Attribute[] attributes = GetAttributes(asmList[i], typeof(XmlnsDefinitionAttribute));
			for (int j = 0; j < attributes.Length; j++)
			{
				string xmlns = null;
				string clrns = null;
				GetNamespacesFromDefinitionAttr(attributes[j], out xmlns, out clrns);
				if (string.IsNullOrEmpty(xmlns) || string.IsNullOrEmpty(clrns))
				{
					throw new ArgumentException(SR.Format(SR.ParserAttributeArgsLow, "XmlnsDefinitionAttribute"));
				}
				if (string.CompareOrdinal(xmlnsRequested, xmlns) == 0)
				{
					list.Add(new ClrNamespaceAssemblyPair(clrns, fullName));
				}
			}
		}
		return list;
	}

	private void ProcessXmlnsCompatibleWithAttributes(Assembly[] asmList)
	{
		for (int i = 0; i < asmList.Length; i++)
		{
			Attribute[] attributes = GetAttributes(asmList[i], typeof(XmlnsCompatibleWithAttribute));
			for (int j = 0; j < attributes.Length; j++)
			{
				string oldXmlns = null;
				string newXmlns = null;
				GetNamespacesFromCompatAttr(attributes[j], out oldXmlns, out newXmlns);
				if (string.IsNullOrEmpty(oldXmlns) || string.IsNullOrEmpty(newXmlns))
				{
					throw new ArgumentException(SR.Format(SR.ParserAttributeArgsLow, "XmlnsCompatibleWithAttribute"));
				}
				if (_compatTable.ContainsKey(oldXmlns) && _compatTable[oldXmlns] != newXmlns)
				{
					throw new InvalidOperationException(SR.Format(SR.ParserCompatDuplicate, oldXmlns, _compatTable[oldXmlns]));
				}
				_compatTable[oldXmlns] = newXmlns;
				_compatTableReverse[newXmlns] = oldXmlns;
			}
		}
	}
}
