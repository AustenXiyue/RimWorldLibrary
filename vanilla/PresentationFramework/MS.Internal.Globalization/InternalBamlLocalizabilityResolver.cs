using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;
using System.Xml;
using MS.Utility;

namespace MS.Internal.Globalization;

internal class InternalBamlLocalizabilityResolver : BamlLocalizabilityResolver
{
	private class ElementComments
	{
		internal string ElementId;

		internal PropertyComment[] LocalizationAttributes;

		internal PropertyComment[] LocalizationComments;

		internal ElementComments()
		{
			ElementId = null;
			LocalizationAttributes = Array.Empty<PropertyComment>();
			LocalizationComments = Array.Empty<PropertyComment>();
		}
	}

	private BamlLocalizabilityResolver _externalResolver;

	private FrugalObjectList<string> _assemblyNames;

	private Hashtable _classNameToAssemblyIndex;

	private Dictionary<string, ElementLocalizability> _classAttributeTable;

	private Dictionary<string, LocalizabilityAttribute> _propertyAttributeTable;

	private ElementComments[] _comments;

	private int _commentsIndex;

	private XmlDocument _commentsDocument;

	private BamlLocalizer _localizer;

	private TextReader _commentingText;

	private LocalizabilityAttribute DefaultAttribute => new LocalizabilityAttribute(LocalizationCategory.Inherit)
	{
		Modifiability = Modifiability.Inherit,
		Readability = Readability.Inherit
	};

	internal InternalBamlLocalizabilityResolver(BamlLocalizer localizer, BamlLocalizabilityResolver externalResolver, TextReader comments)
	{
		_localizer = localizer;
		_externalResolver = externalResolver;
		_commentingText = comments;
	}

	internal void AddClassAndAssembly(string className, string assemblyName)
	{
		if (assemblyName != null && !_classNameToAssemblyIndex.Contains(className))
		{
			int num = _assemblyNames.IndexOf(assemblyName);
			if (num < 0)
			{
				_assemblyNames.Add(assemblyName);
				num = _assemblyNames.Count - 1;
			}
			_classNameToAssemblyIndex.Add(className, num);
		}
	}

	internal void InitLocalizabilityCache()
	{
		_assemblyNames = new FrugalObjectList<string>();
		_classNameToAssemblyIndex = new Hashtable(8);
		_classAttributeTable = new Dictionary<string, ElementLocalizability>(8);
		_propertyAttributeTable = new Dictionary<string, LocalizabilityAttribute>(8);
		_comments = new ElementComments[8];
		_commentsIndex = 0;
		XmlDocument xmlDocument = null;
		if (_commentingText != null)
		{
			xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(_commentingText);
			}
			catch (XmlException)
			{
				RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(new BamlLocalizableResourceKey(string.Empty, string.Empty, string.Empty), BamlLocalizerError.InvalidCommentingXml));
				xmlDocument = null;
			}
		}
		_commentsDocument = xmlDocument;
	}

	internal void ReleaseLocalizabilityCache()
	{
		_propertyAttributeTable = null;
		_comments = null;
		_commentsIndex = 0;
		_commentsDocument = null;
	}

	internal LocalizabilityGroup GetLocalizabilityComment(BamlStartElementNode node, string localName)
	{
		ElementComments elementComments = LookupCommentForElement(node);
		for (int i = 0; i < elementComments.LocalizationAttributes.Length; i++)
		{
			if (elementComments.LocalizationAttributes[i].PropertyName == localName)
			{
				return (LocalizabilityGroup)elementComments.LocalizationAttributes[i].Value;
			}
		}
		return null;
	}

	internal string GetStringComment(BamlStartElementNode node, string localName)
	{
		ElementComments elementComments = LookupCommentForElement(node);
		for (int i = 0; i < elementComments.LocalizationComments.Length; i++)
		{
			if (elementComments.LocalizationComments[i].PropertyName == localName)
			{
				return (string)elementComments.LocalizationComments[i].Value;
			}
		}
		return null;
	}

	internal void RaiseErrorNotifyEvent(BamlLocalizerErrorNotifyEventArgs e)
	{
		_localizer.RaiseErrorNotifyEvent(e);
	}

	public override ElementLocalizability GetElementLocalizability(string assembly, string className)
	{
		if (_externalResolver == null || assembly == null || assembly.Length == 0 || className == null || className.Length == 0)
		{
			return new ElementLocalizability(null, DefaultAttribute);
		}
		if (_classAttributeTable.ContainsKey(className))
		{
			return _classAttributeTable[className];
		}
		ElementLocalizability elementLocalizability = _externalResolver.GetElementLocalizability(assembly, className);
		if (elementLocalizability == null || elementLocalizability.Attribute == null)
		{
			elementLocalizability = new ElementLocalizability(null, DefaultAttribute);
		}
		_classAttributeTable[className] = elementLocalizability;
		return elementLocalizability;
	}

	public override LocalizabilityAttribute GetPropertyLocalizability(string assembly, string className, string property)
	{
		if (_externalResolver == null || assembly == null || assembly.Length == 0 || className == null || className.Length == 0 || property == null || property.Length == 0)
		{
			return DefaultAttribute;
		}
		string key = className + ":" + property;
		if (_propertyAttributeTable.ContainsKey(key))
		{
			return _propertyAttributeTable[key];
		}
		LocalizabilityAttribute localizabilityAttribute = _externalResolver.GetPropertyLocalizability(assembly, className, property);
		if (localizabilityAttribute == null)
		{
			localizabilityAttribute = DefaultAttribute;
		}
		_propertyAttributeTable[key] = localizabilityAttribute;
		return localizabilityAttribute;
	}

	public override string ResolveFormattingTagToClass(string formattingTag)
	{
		foreach (KeyValuePair<string, ElementLocalizability> item in _classAttributeTable)
		{
			if (item.Value.FormattingTag == formattingTag)
			{
				return item.Key;
			}
		}
		string text = null;
		if (_externalResolver != null)
		{
			text = _externalResolver.ResolveFormattingTagToClass(formattingTag);
			if (!string.IsNullOrEmpty(text))
			{
				if (_classAttributeTable.ContainsKey(text))
				{
					_classAttributeTable[text].FormattingTag = formattingTag;
				}
				else
				{
					_classAttributeTable[text] = new ElementLocalizability(formattingTag, null);
				}
			}
		}
		return text;
	}

	public override string ResolveAssemblyFromClass(string className)
	{
		if (className == null || className.Length == 0)
		{
			return string.Empty;
		}
		if (_classNameToAssemblyIndex.Contains(className))
		{
			return _assemblyNames[(int)_classNameToAssemblyIndex[className]];
		}
		string text = null;
		if (_externalResolver != null)
		{
			text = _externalResolver.ResolveAssemblyFromClass(className);
			AddClassAndAssembly(className, text);
		}
		return text;
	}

	private ElementComments LookupCommentForElement(BamlStartElementNode node)
	{
		if (node.Uid == null)
		{
			return new ElementComments();
		}
		for (int i = 0; i < _comments.Length; i++)
		{
			if (_comments[i] != null && _comments[i].ElementId == node.Uid)
			{
				return _comments[i];
			}
		}
		ElementComments elementComments = new ElementComments();
		elementComments.ElementId = node.Uid;
		if (_commentsDocument != null)
		{
			XmlElement xmlElement = FindElementByID(_commentsDocument, node.Uid);
			if (xmlElement != null)
			{
				string attribute = xmlElement.GetAttribute("Attributes");
				SetLocalizationAttributes(node, elementComments, attribute);
				attribute = xmlElement.GetAttribute("Comments");
				SetLocalizationComments(node, elementComments, attribute);
			}
		}
		if (node.Children != null)
		{
			for (int j = 0; j < node.Children.Count; j++)
			{
				if (elementComments.LocalizationComments.Length != 0 && elementComments.LocalizationAttributes.Length != 0)
				{
					break;
				}
				BamlTreeNode bamlTreeNode = node.Children[j];
				if (bamlTreeNode.NodeType == BamlNodeType.Property)
				{
					BamlPropertyNode bamlPropertyNode = (BamlPropertyNode)bamlTreeNode;
					if (LocComments.IsLocCommentsProperty(bamlPropertyNode.OwnerTypeFullName, bamlPropertyNode.PropertyName) && elementComments.LocalizationComments.Length == 0)
					{
						SetLocalizationComments(node, elementComments, bamlPropertyNode.Value);
					}
					else if (LocComments.IsLocLocalizabilityProperty(bamlPropertyNode.OwnerTypeFullName, bamlPropertyNode.PropertyName) && elementComments.LocalizationAttributes.Length == 0)
					{
						SetLocalizationAttributes(node, elementComments, bamlPropertyNode.Value);
					}
				}
			}
		}
		_comments[_commentsIndex] = elementComments;
		_commentsIndex = (_commentsIndex + 1) % _comments.Length;
		return elementComments;
	}

	private static XmlElement FindElementByID(XmlDocument doc, string uid)
	{
		if (doc != null && doc.DocumentElement != null)
		{
			foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Element)
				{
					XmlElement xmlElement = (XmlElement)childNode;
					if (xmlElement.Name == "LocalizationDirectives" && xmlElement.GetAttribute("Uid") == uid)
					{
						return xmlElement;
					}
				}
			}
		}
		return null;
	}

	private void SetLocalizationAttributes(BamlStartElementNode node, ElementComments comments, string attributes)
	{
		if (!string.IsNullOrEmpty(attributes))
		{
			try
			{
				comments.LocalizationAttributes = LocComments.ParsePropertyLocalizabilityAttributes(attributes);
			}
			catch (FormatException)
			{
				RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(BamlTreeMap.GetKey(node), BamlLocalizerError.InvalidLocalizationAttributes));
			}
		}
	}

	private void SetLocalizationComments(BamlStartElementNode node, ElementComments comments, string stringComment)
	{
		if (!string.IsNullOrEmpty(stringComment))
		{
			try
			{
				comments.LocalizationComments = LocComments.ParsePropertyComments(stringComment);
			}
			catch (FormatException)
			{
				RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(BamlTreeMap.GetKey(node), BamlLocalizerError.InvalidLocalizationComments));
			}
		}
	}
}
