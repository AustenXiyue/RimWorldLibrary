using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;
using System.Xml;

namespace MS.Internal.Globalization;

internal static class BamlTreeUpdater
{
	private class BamlTreeUpdateMap
	{
		private BamlTreeMap _originalMap;

		private BamlTree _tree;

		private Hashtable _uidToNewBamlNodeIndexMap;

		private Hashtable _keyToNewBamlNodeIndexMap;

		private Dictionary<string, string> _contentPropertyTable;

		internal BamlLocalizationDictionary LocalizationDictionary => _originalMap.LocalizationDictionary;

		internal InternalBamlLocalizabilityResolver Resolver => _originalMap.Resolver;

		internal BamlTreeUpdateMap(BamlTreeMap map, BamlTree tree)
		{
			_uidToNewBamlNodeIndexMap = new Hashtable(8);
			_keyToNewBamlNodeIndexMap = new Hashtable(8);
			_originalMap = map;
			_tree = tree;
		}

		internal BamlTreeNode MapKeyToBamlTreeNode(BamlLocalizableResourceKey key)
		{
			BamlTreeNode bamlTreeNode = _originalMap.MapKeyToBamlTreeNode(key, _tree);
			if (bamlTreeNode == null && _keyToNewBamlNodeIndexMap.Contains(key))
			{
				bamlTreeNode = _tree[(int)_keyToNewBamlNodeIndexMap[key]];
			}
			return bamlTreeNode;
		}

		internal bool IsNewBamlTreeNode(BamlLocalizableResourceKey key)
		{
			return _keyToNewBamlNodeIndexMap.Contains(key);
		}

		internal BamlStartElementNode MapUidToBamlTreeElementNode(string uid)
		{
			BamlStartElementNode bamlStartElementNode = _originalMap.MapUidToBamlTreeElementNode(uid, _tree);
			if (bamlStartElementNode == null && _uidToNewBamlNodeIndexMap.Contains(uid))
			{
				bamlStartElementNode = _tree[(int)_uidToNewBamlNodeIndexMap[uid]] as BamlStartElementNode;
			}
			return bamlStartElementNode;
		}

		internal void AddBamlTreeNode(string uid, BamlLocalizableResourceKey key, BamlTreeNode node)
		{
			_tree.AddTreeNode(node);
			if (uid != null)
			{
				_uidToNewBamlNodeIndexMap[uid] = _tree.Size - 1;
			}
			_keyToNewBamlNodeIndexMap[key] = _tree.Size - 1;
		}

		internal string GetContentProperty(string assemblyName, string fullTypeName)
		{
			string clrNamespace = string.Empty;
			string typeShortName = fullTypeName;
			int num = fullTypeName.LastIndexOf('.');
			if (num >= 0)
			{
				clrNamespace = fullTypeName.Substring(0, num);
				typeShortName = fullTypeName.Substring(num + 1);
			}
			short knownTypeIdFromName = BamlMapTable.GetKnownTypeIdFromName(assemblyName, clrNamespace, typeShortName);
			if (knownTypeIdFromName != 0)
			{
				return KnownTypes.GetContentPropertyName((KnownElements)(-knownTypeIdFromName));
			}
			string value = null;
			if (_contentPropertyTable != null && _contentPropertyTable.TryGetValue(fullTypeName, out value))
			{
				return value;
			}
			Type type = Assembly.Load(assemblyName).GetType(fullTypeName);
			if (type != null)
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(ContentPropertyAttribute), inherit: true);
				if (customAttributes.Length != 0)
				{
					value = (customAttributes[0] as ContentPropertyAttribute).Name;
					if (_contentPropertyTable == null)
					{
						_contentPropertyTable = new Dictionary<string, string>(8);
					}
					_contentPropertyTable.Add(fullTypeName, value);
				}
			}
			return value;
		}
	}

	internal static void UpdateTree(BamlTree tree, BamlTreeMap treeMap, BamlLocalizationDictionary dictionary)
	{
		if (dictionary.Count <= 0)
		{
			return;
		}
		BamlTreeUpdateMap treeMap2 = new BamlTreeUpdateMap(treeMap, tree);
		CreateMissingBamlTreeNode(dictionary, treeMap2);
		BamlLocalizationDictionaryEnumerator enumerator = dictionary.GetEnumerator();
		ArrayList arrayList = new ArrayList();
		while (enumerator.MoveNext())
		{
			if (!ApplyChangeToBamlTree(enumerator.Key, enumerator.Value, treeMap2))
			{
				arrayList.Add(enumerator.Entry);
			}
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)arrayList[i];
			ApplyChangeToBamlTree((BamlLocalizableResourceKey)dictionaryEntry.Key, (BamlLocalizableResource)dictionaryEntry.Value, treeMap2);
		}
	}

	private static void CreateMissingBamlTreeNode(BamlLocalizationDictionary dictionary, BamlTreeUpdateMap treeMap)
	{
		BamlLocalizationDictionaryEnumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BamlLocalizableResourceKey key = enumerator.Key;
			BamlLocalizableResource value = enumerator.Value;
			if (treeMap.MapKeyToBamlTreeNode(key) != null)
			{
				continue;
			}
			if (key.PropertyName == "$Content")
			{
				if (treeMap.MapUidToBamlTreeElementNode(key.Uid) == null)
				{
					BamlStartElementNode bamlStartElementNode = new BamlStartElementNode(treeMap.Resolver.ResolveAssemblyFromClass(key.ClassName), key.ClassName, isInjected: false, useTypeConverter: false);
					bamlStartElementNode.AddChild(new BamlDefAttributeNode("Uid", key.Uid));
					TryAddContentPropertyToNewElement(treeMap, bamlStartElementNode);
					bamlStartElementNode.AddChild(new BamlEndElementNode());
					treeMap.AddBamlTreeNode(key.Uid, key, bamlStartElementNode);
				}
			}
			else
			{
				BamlTreeNode node = ((!(key.PropertyName == "$LiteralContent")) ? ((BamlTreeNode)new BamlPropertyNode(treeMap.Resolver.ResolveAssemblyFromClass(key.ClassName), key.ClassName, key.PropertyName, value.Content, BamlAttributeUsage.Default)) : ((BamlTreeNode)new BamlLiteralContentNode(value.Content)));
				treeMap.AddBamlTreeNode(null, key, node);
			}
		}
	}

	private static bool ApplyChangeToBamlTree(BamlLocalizableResourceKey key, BamlLocalizableResource resource, BamlTreeUpdateMap treeMap)
	{
		if (resource == null || resource.Content == null || !resource.Modifiable)
		{
			return true;
		}
		if (!treeMap.LocalizationDictionary.Contains(key) && !treeMap.IsNewBamlTreeNode(key))
		{
			return true;
		}
		BamlTreeNode bamlTreeNode = treeMap.MapKeyToBamlTreeNode(key);
		Invariant.Assert(bamlTreeNode != null);
		switch (bamlTreeNode.NodeType)
		{
		case BamlNodeType.LiteralContent:
		{
			BamlLiteralContentNode bamlLiteralContentNode = (BamlLiteralContentNode)bamlTreeNode;
			bamlLiteralContentNode.Content = BamlResourceContentUtil.UnescapeString(resource.Content);
			if (bamlLiteralContentNode.Parent == null)
			{
				BamlTreeNode bamlTreeNode2 = treeMap.MapUidToBamlTreeElementNode(key.Uid);
				if (bamlTreeNode2 == null)
				{
					return false;
				}
				bamlTreeNode2.AddChild(bamlLiteralContentNode);
			}
			break;
		}
		case BamlNodeType.Property:
		{
			BamlPropertyNode obj = (BamlPropertyNode)bamlTreeNode;
			obj.Value = BamlResourceContentUtil.UnescapeString(resource.Content);
			if (obj.Parent == null)
			{
				BamlStartElementNode bamlStartElementNode = treeMap.MapUidToBamlTreeElementNode(key.Uid);
				if (bamlStartElementNode == null)
				{
					return false;
				}
				bamlStartElementNode.InsertProperty(bamlTreeNode);
			}
			break;
		}
		case BamlNodeType.StartElement:
		{
			string text = null;
			if (treeMap.LocalizationDictionary.Contains(key))
			{
				text = treeMap.LocalizationDictionary[key].Content;
			}
			if (resource.Content != text)
			{
				ReArrangeChildren(key, bamlTreeNode, resource.Content, treeMap);
			}
			break;
		}
		}
		return true;
	}

	private static void ReArrangeChildren(BamlLocalizableResourceKey key, BamlTreeNode node, string translation, BamlTreeUpdateMap treeMap)
	{
		IList<BamlTreeNode> newChildren = SplitXmlContent(key, translation, treeMap);
		MergeChildrenList(key, treeMap, node, newChildren);
	}

	private static void MergeChildrenList(BamlLocalizableResourceKey key, BamlTreeUpdateMap treeMap, BamlTreeNode parent, IList<BamlTreeNode> newChildren)
	{
		if (newChildren == null)
		{
			return;
		}
		List<BamlTreeNode> children = parent.Children;
		int i = 0;
		StringBuilder stringBuilder = new StringBuilder();
		if (children != null)
		{
			Hashtable hashtable = new Hashtable(newChildren.Count);
			foreach (BamlTreeNode newChild in newChildren)
			{
				if (newChild.NodeType != BamlNodeType.StartElement)
				{
					continue;
				}
				BamlStartElementNode bamlStartElementNode = (BamlStartElementNode)newChild;
				if (bamlStartElementNode.Uid != null)
				{
					if (hashtable.ContainsKey(bamlStartElementNode.Uid))
					{
						treeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.DuplicateElement));
						return;
					}
					hashtable[bamlStartElementNode.Uid] = null;
				}
			}
			parent.Children = null;
			for (int j = 0; j < children.Count - 1; j++)
			{
				BamlTreeNode bamlTreeNode = children[j];
				switch (bamlTreeNode.NodeType)
				{
				case BamlNodeType.StartElement:
				{
					BamlStartElementNode bamlStartElementNode2 = (BamlStartElementNode)bamlTreeNode;
					if (bamlStartElementNode2.Uid != null)
					{
						if (!hashtable.ContainsKey(bamlStartElementNode2.Uid))
						{
							parent.Children = children;
							treeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.MismatchedElements));
							return;
						}
						hashtable.Remove(bamlStartElementNode2.Uid);
					}
					while (i < newChildren.Count)
					{
						BamlTreeNode bamlTreeNode2 = newChildren[i++];
						Invariant.Assert(bamlTreeNode2 != null);
						if (bamlTreeNode2.NodeType == BamlNodeType.Text)
						{
							stringBuilder.Append(((BamlTextNode)bamlTreeNode2).Content);
							continue;
						}
						TryFlushTextToBamlNode(parent, stringBuilder);
						parent.AddChild(bamlTreeNode2);
						if (bamlTreeNode2.NodeType == BamlNodeType.StartElement)
						{
							break;
						}
					}
					break;
				}
				default:
					parent.AddChild(bamlTreeNode);
					break;
				case BamlNodeType.Text:
					break;
				}
			}
		}
		for (; i < newChildren.Count; i++)
		{
			BamlTreeNode bamlTreeNode3 = newChildren[i];
			Invariant.Assert(bamlTreeNode3 != null);
			if (bamlTreeNode3.NodeType == BamlNodeType.Text)
			{
				stringBuilder.Append(((BamlTextNode)bamlTreeNode3).Content);
				continue;
			}
			TryFlushTextToBamlNode(parent, stringBuilder);
			parent.AddChild(bamlTreeNode3);
		}
		TryFlushTextToBamlNode(parent, stringBuilder);
		parent.AddChild(new BamlEndElementNode());
	}

	private static void TryFlushTextToBamlNode(BamlTreeNode parent, StringBuilder textContent)
	{
		if (textContent.Length > 0)
		{
			BamlTreeNode child = new BamlTextNode(textContent.ToString());
			parent.AddChild(child);
			textContent.Length = 0;
		}
	}

	private static IList<BamlTreeNode> SplitXmlContent(BamlLocalizableResourceKey key, string content, BamlTreeUpdateMap bamlTreeMap)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<ROOT>");
		stringBuilder.Append(content);
		stringBuilder.Append("</ROOT>");
		IList<BamlTreeNode> list = new List<BamlTreeNode>(4);
		XmlDocument xmlDocument = new XmlDocument();
		bool flag = true;
		try
		{
			xmlDocument.LoadXml(stringBuilder.ToString());
			if (xmlDocument.FirstChild is XmlElement { HasChildNodes: not false } xmlElement)
			{
				for (int i = 0; i < xmlElement.ChildNodes.Count && flag; i++)
				{
					flag = GetBamlTreeNodeFromXmlNode(key, xmlElement.ChildNodes[i], bamlTreeMap, list);
				}
			}
		}
		catch (XmlException)
		{
			bamlTreeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.SubstitutionAsPlaintext));
			flag = GetBamlTreeNodeFromText(key, content, bamlTreeMap, list);
		}
		if (!flag)
		{
			return null;
		}
		return list;
	}

	private static bool GetBamlTreeNodeFromXmlNode(BamlLocalizableResourceKey key, XmlNode node, BamlTreeUpdateMap bamlTreeMap, IList<BamlTreeNode> newChildrenList)
	{
		if (node.NodeType == XmlNodeType.Text)
		{
			return GetBamlTreeNodeFromText(key, node.Value, bamlTreeMap, newChildrenList);
		}
		if (node.NodeType == XmlNodeType.Element)
		{
			XmlElement xmlElement = node as XmlElement;
			string text = bamlTreeMap.Resolver.ResolveFormattingTagToClass(xmlElement.Name);
			bool flag = string.IsNullOrEmpty(text);
			string text2 = null;
			if (!flag)
			{
				text2 = bamlTreeMap.Resolver.ResolveAssemblyFromClass(text);
				flag = string.IsNullOrEmpty(text2);
			}
			if (flag)
			{
				bamlTreeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.UnknownFormattingTag));
				return false;
			}
			string text3 = null;
			if (xmlElement.HasAttributes)
			{
				text3 = xmlElement.GetAttribute("Uid");
				if (!string.IsNullOrEmpty(text3))
				{
					text3 = BamlResourceContentUtil.UnescapeString(text3);
				}
			}
			BamlStartElementNode bamlStartElementNode = null;
			if (text3 != null)
			{
				bamlStartElementNode = bamlTreeMap.MapUidToBamlTreeElementNode(text3);
			}
			if (bamlStartElementNode == null)
			{
				bamlStartElementNode = new BamlStartElementNode(text2, text, isInjected: false, useTypeConverter: false);
				if (text3 != null)
				{
					bamlTreeMap.AddBamlTreeNode(text3, new BamlLocalizableResourceKey(text3, text, "$Content", text2), bamlStartElementNode);
					bamlStartElementNode.AddChild(new BamlDefAttributeNode("Uid", text3));
				}
				TryAddContentPropertyToNewElement(bamlTreeMap, bamlStartElementNode);
				bamlStartElementNode.AddChild(new BamlEndElementNode());
			}
			else if (bamlStartElementNode.TypeFullName != text)
			{
				bamlTreeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.DuplicateUid));
				return false;
			}
			newChildrenList.Add(bamlStartElementNode);
			bool flag2 = true;
			if (xmlElement.HasChildNodes)
			{
				IList<BamlTreeNode> list = new List<BamlTreeNode>();
				for (int i = 0; i < xmlElement.ChildNodes.Count && flag2; i++)
				{
					flag2 = GetBamlTreeNodeFromXmlNode(key, xmlElement.ChildNodes[i], bamlTreeMap, list);
				}
				if (flag2)
				{
					MergeChildrenList(key, bamlTreeMap, bamlStartElementNode, list);
				}
			}
			return flag2;
		}
		return true;
	}

	private static bool GetBamlTreeNodeFromText(BamlLocalizableResourceKey key, string content, BamlTreeUpdateMap bamlTreeMap, IList<BamlTreeNode> newChildrenList)
	{
		BamlStringToken[] array = BamlResourceContentUtil.ParseChildPlaceholder(content);
		if (array == null)
		{
			bamlTreeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.IncompleteElementPlaceholder));
			return false;
		}
		bool result = true;
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i].Type)
			{
			case BamlStringToken.TokenType.Text:
			{
				BamlTreeNode item = new BamlTextNode(array[i].Value);
				newChildrenList.Add(item);
				break;
			}
			case BamlStringToken.TokenType.ChildPlaceHolder:
			{
				BamlTreeNode bamlTreeNode = bamlTreeMap.MapUidToBamlTreeElementNode(array[i].Value);
				if (bamlTreeNode != null)
				{
					newChildrenList.Add(bamlTreeNode);
					break;
				}
				bamlTreeMap.Resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(new BamlLocalizableResourceKey(array[i].Value, string.Empty, string.Empty), BamlLocalizerError.InvalidUid));
				result = false;
				break;
			}
			}
		}
		return result;
	}

	private static void TryAddContentPropertyToNewElement(BamlTreeUpdateMap bamlTreeMap, BamlStartElementNode bamlNode)
	{
		string contentProperty = bamlTreeMap.GetContentProperty(bamlNode.AssemblyName, bamlNode.TypeFullName);
		if (!string.IsNullOrEmpty(contentProperty))
		{
			bamlNode.AddChild(new BamlContentPropertyNode(bamlNode.AssemblyName, bamlNode.TypeFullName, contentProperty));
		}
	}
}
