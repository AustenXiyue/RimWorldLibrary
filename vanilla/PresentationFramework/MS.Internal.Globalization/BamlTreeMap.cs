using System.Collections;
using System.IO;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;

namespace MS.Internal.Globalization;

internal class BamlTreeMap
{
	private Hashtable _keyToBamlNodeIndexMap;

	private Hashtable _uidToBamlNodeIndexMap;

	private LocalizableResourceBuilder _localizableResourceBuilder;

	private BamlLocalizationDictionary _localizableResources;

	private BamlTree _tree;

	private InternalBamlLocalizabilityResolver _resolver;

	internal BamlLocalizationDictionary LocalizationDictionary
	{
		get
		{
			EnsureMap();
			return _localizableResources;
		}
	}

	internal InternalBamlLocalizabilityResolver Resolver => _resolver;

	internal BamlTreeMap(BamlLocalizer localizer, BamlTree tree, BamlLocalizabilityResolver resolver, TextReader comments)
	{
		_tree = tree;
		_resolver = new InternalBamlLocalizabilityResolver(localizer, resolver, comments);
		_localizableResourceBuilder = new LocalizableResourceBuilder(_resolver);
	}

	internal BamlTreeNode MapKeyToBamlTreeNode(BamlLocalizableResourceKey key, BamlTree tree)
	{
		if (_keyToBamlNodeIndexMap.Contains(key))
		{
			return tree[(int)_keyToBamlNodeIndexMap[key]];
		}
		return null;
	}

	internal BamlStartElementNode MapUidToBamlTreeElementNode(string uid, BamlTree tree)
	{
		if (_uidToBamlNodeIndexMap.Contains(uid))
		{
			return tree[(int)_uidToBamlNodeIndexMap[uid]] as BamlStartElementNode;
		}
		return null;
	}

	internal void EnsureMap()
	{
		if (_localizableResources != null)
		{
			return;
		}
		_resolver.InitLocalizabilityCache();
		_keyToBamlNodeIndexMap = new Hashtable(_tree.Size);
		_uidToBamlNodeIndexMap = new Hashtable(_tree.Size / 2);
		_localizableResources = new BamlLocalizationDictionary();
		for (int i = 0; i < _tree.Size; i++)
		{
			BamlTreeNode bamlTreeNode = _tree[i];
			if (bamlTreeNode.Unidentifiable)
			{
				continue;
			}
			if (bamlTreeNode.NodeType == BamlNodeType.StartElement)
			{
				BamlStartElementNode bamlStartElementNode = (BamlStartElementNode)bamlTreeNode;
				_resolver.AddClassAndAssembly(bamlStartElementNode.TypeFullName, bamlStartElementNode.AssemblyName);
			}
			BamlLocalizableResourceKey key = GetKey(bamlTreeNode);
			if (key == null)
			{
				continue;
			}
			if (bamlTreeNode.NodeType == BamlNodeType.StartElement)
			{
				if (_uidToBamlNodeIndexMap.ContainsKey(key.Uid))
				{
					_resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.DuplicateUid));
					bamlTreeNode.Unidentifiable = true;
					if (bamlTreeNode.Children == null)
					{
						continue;
					}
					foreach (BamlTreeNode child in bamlTreeNode.Children)
					{
						if (child.NodeType != BamlNodeType.StartElement)
						{
							child.Unidentifiable = true;
						}
					}
					continue;
				}
				_uidToBamlNodeIndexMap.Add(key.Uid, i);
			}
			_keyToBamlNodeIndexMap.Add(key, i);
			if (_localizableResources.RootElementKey == null && bamlTreeNode.NodeType == BamlNodeType.StartElement && bamlTreeNode.Parent != null && bamlTreeNode.Parent.NodeType == BamlNodeType.StartDocument)
			{
				_localizableResources.SetRootElementKey(key);
			}
			BamlLocalizableResource bamlLocalizableResource = _localizableResourceBuilder.BuildFromNode(key, bamlTreeNode);
			if (bamlLocalizableResource != null)
			{
				_localizableResources.Add(key, bamlLocalizableResource);
			}
		}
		_resolver.ReleaseLocalizabilityCache();
	}

	internal static BamlLocalizableResourceKey GetKey(BamlTreeNode node)
	{
		BamlLocalizableResourceKey result = null;
		switch (node.NodeType)
		{
		case BamlNodeType.StartElement:
		{
			BamlStartElementNode bamlStartElementNode2 = (BamlStartElementNode)node;
			if (bamlStartElementNode2.Uid != null)
			{
				result = new BamlLocalizableResourceKey(bamlStartElementNode2.Uid, bamlStartElementNode2.TypeFullName, "$Content", bamlStartElementNode2.AssemblyName);
			}
			break;
		}
		case BamlNodeType.Property:
		{
			BamlPropertyNode bamlPropertyNode = (BamlPropertyNode)node;
			BamlStartElementNode bamlStartElementNode3 = (BamlStartElementNode)bamlPropertyNode.Parent;
			if (bamlStartElementNode3.Uid != null)
			{
				string uid = ((bamlPropertyNode.Index > 0) ? string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}.{1}_{2}", bamlStartElementNode3.Uid, bamlPropertyNode.PropertyName, bamlPropertyNode.Index) : bamlStartElementNode3.Uid);
				result = new BamlLocalizableResourceKey(uid, bamlPropertyNode.OwnerTypeFullName, bamlPropertyNode.PropertyName, bamlPropertyNode.AssemblyName);
			}
			break;
		}
		case BamlNodeType.LiteralContent:
		{
			_ = (BamlLiteralContentNode)node;
			BamlStartElementNode bamlStartElementNode = (BamlStartElementNode)node.Parent;
			if (bamlStartElementNode.Uid != null)
			{
				result = new BamlLocalizableResourceKey(bamlStartElementNode.Uid, bamlStartElementNode.TypeFullName, "$LiteralContent", bamlStartElementNode.AssemblyName);
			}
			break;
		}
		}
		return result;
	}
}
