using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlResourceDeserializer
{
	private Stack<BamlTreeNode> _bamlTreeStack = new Stack<BamlTreeNode>();

	private Dictionary<string, Stack<ILocalizabilityInheritable>> _propertyInheritanceTreeStack = new Dictionary<string, Stack<ILocalizabilityInheritable>>(8);

	private BamlTreeNode _currentParent;

	private BamlStartDocumentNode _root;

	private BamlReader _reader;

	private int _nodeCount;

	internal static BamlTree LoadBaml(Stream bamlStream)
	{
		return new BamlResourceDeserializer().LoadBamlImp(bamlStream);
	}

	private BamlResourceDeserializer()
	{
	}

	private BamlTree LoadBamlImp(Stream bamlSteam)
	{
		_reader = new BamlReader(bamlSteam);
		_reader.Read();
		if (_reader.NodeType != BamlNodeType.StartDocument)
		{
			throw new XamlParseException(SR.InvalidStartOfBaml);
		}
		_root = new BamlStartDocumentNode();
		PushNodeToStack(_root);
		Hashtable hashtable = new Hashtable(8);
		while (_bamlTreeStack.Count > 0 && _reader.Read())
		{
			switch (_reader.NodeType)
			{
			case BamlNodeType.StartElement:
			{
				BamlTreeNode node11 = new BamlStartElementNode(_reader.AssemblyName, _reader.Name, _reader.IsInjected, _reader.CreateUsingTypeConverter);
				PushNodeToStack(node11);
				break;
			}
			case BamlNodeType.EndElement:
			{
				BamlTreeNode node10 = new BamlEndElementNode();
				AddChildToCurrentParent(node10);
				PopStack();
				break;
			}
			case BamlNodeType.StartComplexProperty:
			{
				BamlStartComplexPropertyNode bamlStartComplexPropertyNode = new BamlStartComplexPropertyNode(_reader.AssemblyName, _reader.Name.Substring(0, _reader.Name.LastIndexOf('.')), _reader.LocalName);
				bamlStartComplexPropertyNode.LocalizabilityAncestor = PeekPropertyStack(bamlStartComplexPropertyNode.PropertyName);
				PushPropertyToStack(bamlStartComplexPropertyNode.PropertyName, bamlStartComplexPropertyNode);
				PushNodeToStack(bamlStartComplexPropertyNode);
				break;
			}
			case BamlNodeType.EndComplexProperty:
			{
				BamlTreeNode node9 = new BamlEndComplexPropertyNode();
				AddChildToCurrentParent(node9);
				PopStack();
				break;
			}
			case BamlNodeType.Event:
			{
				BamlTreeNode node8 = new BamlEventNode(_reader.Name, _reader.Value);
				AddChildToCurrentParent(node8);
				break;
			}
			case BamlNodeType.RoutedEvent:
			{
				BamlTreeNode node7 = new BamlRoutedEventNode(_reader.AssemblyName, _reader.Name.Substring(0, _reader.Name.LastIndexOf('.')), _reader.LocalName, _reader.Value);
				AddChildToCurrentParent(node7);
				break;
			}
			case BamlNodeType.PIMapping:
			{
				BamlTreeNode node6 = new BamlPIMappingNode(_reader.XmlNamespace, _reader.ClrNamespace, _reader.AssemblyName);
				AddChildToCurrentParent(node6);
				break;
			}
			case BamlNodeType.LiteralContent:
			{
				BamlTreeNode node5 = new BamlLiteralContentNode(_reader.Value);
				AddChildToCurrentParent(node5);
				break;
			}
			case BamlNodeType.Text:
			{
				BamlTreeNode node4 = new BamlTextNode(_reader.Value, _reader.TypeConverterAssemblyName, _reader.TypeConverterName);
				AddChildToCurrentParent(node4);
				break;
			}
			case BamlNodeType.StartConstructor:
			{
				BamlTreeNode node3 = new BamlStartConstructorNode();
				AddChildToCurrentParent(node3);
				break;
			}
			case BamlNodeType.EndConstructor:
			{
				BamlTreeNode node2 = new BamlEndConstructorNode();
				AddChildToCurrentParent(node2);
				break;
			}
			case BamlNodeType.EndDocument:
			{
				BamlTreeNode node = new BamlEndDocumentNode();
				AddChildToCurrentParent(node);
				PopStack();
				break;
			}
			default:
				throw new XamlParseException(SR.Format(SR.UnRecognizedBamlNodeType, _reader.NodeType));
			}
			if (!_reader.HasProperties)
			{
				continue;
			}
			hashtable.Clear();
			_reader.MoveToFirstProperty();
			do
			{
				switch (_reader.NodeType)
				{
				case BamlNodeType.ConnectionId:
				{
					BamlTreeNode node16 = new BamlConnectionIdNode(_reader.ConnectionId);
					AddChildToCurrentParent(node16);
					break;
				}
				case BamlNodeType.Property:
				{
					BamlPropertyNode bamlPropertyNode = new BamlPropertyNode(_reader.AssemblyName, _reader.Name.Substring(0, _reader.Name.LastIndexOf('.')), _reader.LocalName, _reader.Value, _reader.AttributeUsage);
					bamlPropertyNode.LocalizabilityAncestor = PeekPropertyStack(bamlPropertyNode.PropertyName);
					PushPropertyToStack(bamlPropertyNode.PropertyName, bamlPropertyNode);
					AddChildToCurrentParent(bamlPropertyNode);
					if (hashtable.Contains(_reader.Name))
					{
						object obj = hashtable[_reader.Name];
						int num = 2;
						if (obj is BamlPropertyNode)
						{
							((BamlPropertyNode)obj).Index = 1;
						}
						else
						{
							num = (int)obj;
						}
						bamlPropertyNode.Index = num;
						hashtable[_reader.Name] = ++num;
					}
					else
					{
						hashtable[_reader.Name] = bamlPropertyNode;
					}
					break;
				}
				case BamlNodeType.DefAttribute:
				{
					if (_reader.Name == "Uid")
					{
						((BamlStartElementNode)_currentParent).Uid = _reader.Value;
					}
					BamlTreeNode node15 = new BamlDefAttributeNode(_reader.Name, _reader.Value);
					AddChildToCurrentParent(node15);
					break;
				}
				case BamlNodeType.XmlnsProperty:
				{
					BamlTreeNode node14 = new BamlXmlnsPropertyNode(_reader.LocalName, _reader.Value);
					AddChildToCurrentParent(node14);
					break;
				}
				case BamlNodeType.ContentProperty:
				{
					BamlTreeNode node13 = new BamlContentPropertyNode(_reader.AssemblyName, _reader.Name.Substring(0, _reader.Name.LastIndexOf('.')), _reader.LocalName);
					AddChildToCurrentParent(node13);
					break;
				}
				case BamlNodeType.PresentationOptionsAttribute:
				{
					BamlTreeNode node12 = new BamlPresentationOptionsAttributeNode(_reader.Name, _reader.Value);
					AddChildToCurrentParent(node12);
					break;
				}
				default:
					throw new XamlParseException(SR.Format(SR.UnRecognizedBamlNodeType, _reader.NodeType));
				}
			}
			while (_reader.MoveToNextProperty());
		}
		if (_reader.Read() || _bamlTreeStack.Count > 0)
		{
			throw new XamlParseException(SR.InvalidEndOfBaml);
		}
		return new BamlTree(_root, _nodeCount);
	}

	private void PushNodeToStack(BamlTreeNode node)
	{
		if (_currentParent != null)
		{
			_currentParent.AddChild(node);
		}
		_bamlTreeStack.Push(node);
		_currentParent = node;
		_nodeCount++;
	}

	private void AddChildToCurrentParent(BamlTreeNode node)
	{
		if (_currentParent == null)
		{
			throw new InvalidOperationException(SR.NullParentNode);
		}
		_currentParent.AddChild(node);
		_nodeCount++;
	}

	private void PopStack()
	{
		BamlTreeNode bamlTreeNode = _bamlTreeStack.Pop();
		if (bamlTreeNode.Children != null)
		{
			foreach (BamlTreeNode child in bamlTreeNode.Children)
			{
				if (child is BamlStartComplexPropertyNode bamlStartComplexPropertyNode)
				{
					PopPropertyFromStack(bamlStartComplexPropertyNode.PropertyName);
				}
			}
		}
		if (_bamlTreeStack.Count > 0)
		{
			_currentParent = _bamlTreeStack.Peek();
		}
		else
		{
			_currentParent = null;
		}
	}

	private void PushPropertyToStack(string propertyName, ILocalizabilityInheritable node)
	{
		Stack<ILocalizabilityInheritable> stack;
		if (_propertyInheritanceTreeStack.ContainsKey(propertyName))
		{
			stack = _propertyInheritanceTreeStack[propertyName];
		}
		else
		{
			stack = new Stack<ILocalizabilityInheritable>();
			_propertyInheritanceTreeStack.Add(propertyName, stack);
		}
		stack.Push(node);
	}

	private void PopPropertyFromStack(string propertyName)
	{
		_propertyInheritanceTreeStack[propertyName].Pop();
	}

	private ILocalizabilityInheritable PeekPropertyStack(string propertyName)
	{
		if (_propertyInheritanceTreeStack.ContainsKey(propertyName))
		{
			Stack<ILocalizabilityInheritable> stack = _propertyInheritanceTreeStack[propertyName];
			if (stack.Count > 0)
			{
				return stack.Peek();
			}
		}
		return _root;
	}
}
