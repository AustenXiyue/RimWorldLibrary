using System;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;

namespace MS.Internal.Globalization;

internal sealed class LocalizableResourceBuilder
{
	private InternalBamlLocalizabilityResolver _resolver;

	private readonly LocalizabilityAttribute LocalizabilityIgnore = new LocalizabilityAttribute(LocalizationCategory.Ignore);

	internal LocalizableResourceBuilder(InternalBamlLocalizabilityResolver resolver)
	{
		_resolver = resolver;
	}

	internal BamlLocalizableResource BuildFromNode(BamlLocalizableResourceKey key, BamlTreeNode node)
	{
		if (node.Formatted)
		{
			return null;
		}
		BamlLocalizableResource bamlLocalizableResource = null;
		LocalizabilityAttribute localizability = null;
		BamlStartElementNode node2 = null;
		string localName = null;
		string formattingTag;
		switch (node.NodeType)
		{
		case BamlNodeType.StartElement:
			node2 = (BamlStartElementNode)node;
			GetLocalizabilityForElementNode(node2, out localizability, out formattingTag);
			localName = "$Content";
			break;
		case BamlNodeType.LiteralContent:
			GetLocalizabilityForElementNode((BamlStartElementNode)node.Parent, out localizability, out formattingTag);
			node2 = (BamlStartElementNode)node.Parent;
			localName = "$Content";
			break;
		case BamlNodeType.Property:
		{
			BamlStartComplexPropertyNode bamlStartComplexPropertyNode = (BamlStartComplexPropertyNode)node;
			if (LocComments.IsLocCommentsProperty(bamlStartComplexPropertyNode.OwnerTypeFullName, bamlStartComplexPropertyNode.PropertyName) || LocComments.IsLocLocalizabilityProperty(bamlStartComplexPropertyNode.OwnerTypeFullName, bamlStartComplexPropertyNode.PropertyName))
			{
				return null;
			}
			GetLocalizabilityForPropertyNode(bamlStartComplexPropertyNode, out localizability);
			localName = bamlStartComplexPropertyNode.PropertyName;
			node2 = (BamlStartElementNode)node.Parent;
			break;
		}
		default:
			Invariant.Assert(condition: false);
			break;
		}
		localizability = CombineAndPropagateInheritanceValues(node as ILocalizabilityInheritable, localizability);
		string content = null;
		if (localizability.Category != LocalizationCategory.NeverLocalize && localizability.Category != LocalizationCategory.Ignore && TryGetContent(key, node, out content))
		{
			bamlLocalizableResource = new BamlLocalizableResource();
			bamlLocalizableResource.Readable = localizability.Readability == Readability.Readable;
			bamlLocalizableResource.Modifiable = localizability.Modifiability == Modifiability.Modifiable;
			bamlLocalizableResource.Category = localizability.Category;
			bamlLocalizableResource.Content = content;
			bamlLocalizableResource.Comments = _resolver.GetStringComment(node2, localName);
		}
		return bamlLocalizableResource;
	}

	internal bool TryGetContent(BamlLocalizableResourceKey key, BamlTreeNode currentNode, out string content)
	{
		content = string.Empty;
		switch (currentNode.NodeType)
		{
		case BamlNodeType.Property:
		{
			bool result = true;
			BamlPropertyNode bamlPropertyNode = (BamlPropertyNode)currentNode;
			content = BamlResourceContentUtil.EscapeString(bamlPropertyNode.Value);
			string attrValue = content;
			if (MarkupExtensionParser.GetMarkupExtensionTypeAndArgs(ref attrValue, out var _, out var _))
			{
				LocalizabilityGroup localizabilityComment = _resolver.GetLocalizabilityComment(bamlPropertyNode.Parent as BamlStartElementNode, bamlPropertyNode.PropertyName);
				result = localizabilityComment != null && localizabilityComment.Readability == Readability.Readable;
			}
			return result;
		}
		case BamlNodeType.LiteralContent:
			content = BamlResourceContentUtil.EscapeString(((BamlLiteralContentNode)currentNode).Content);
			return true;
		case BamlNodeType.StartElement:
		{
			BamlStartElementNode bamlStartElementNode = (BamlStartElementNode)currentNode;
			if (bamlStartElementNode.Content == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (BamlTreeNode child in bamlStartElementNode.Children)
				{
					switch (child.NodeType)
					{
					case BamlNodeType.StartElement:
					{
						if (TryFormatElementContent(key, (BamlStartElementNode)child, out var content2))
						{
							stringBuilder.Append(content2);
							break;
						}
						return false;
					}
					case BamlNodeType.Text:
						stringBuilder.Append(BamlResourceContentUtil.EscapeString(((BamlTextNode)child).Content));
						break;
					}
				}
				bamlStartElementNode.Content = stringBuilder.ToString();
			}
			content = bamlStartElementNode.Content;
			return true;
		}
		default:
			return true;
		}
	}

	private bool TryFormatElementContent(BamlLocalizableResourceKey key, BamlStartElementNode node, out string content)
	{
		content = string.Empty;
		GetLocalizabilityForElementNode(node, out var localizability, out var formattingTag);
		localizability = CombineAndPropagateInheritanceValues(node, localizability);
		if (formattingTag != null && localizability.Category != LocalizationCategory.NeverLocalize && localizability.Category != LocalizationCategory.Ignore && localizability.Modifiability == Modifiability.Modifiable && localizability.Readability == Readability.Readable)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (node.Uid != null)
			{
				stringBuilder.AppendFormat(TypeConverterHelper.InvariantEnglishUS, "<{0} {1}=\"{2}\">", formattingTag, "Uid", BamlResourceContentUtil.EscapeString(node.Uid));
			}
			else
			{
				stringBuilder.AppendFormat(TypeConverterHelper.InvariantEnglishUS, "<{0}>", formattingTag);
			}
			string content2;
			bool num = TryGetContent(key, node, out content2);
			if (num)
			{
				stringBuilder.Append(content2);
				stringBuilder.AppendFormat(TypeConverterHelper.InvariantEnglishUS, "</{0}>", formattingTag);
				node.Formatted = true;
				content = stringBuilder.ToString();
			}
			return num;
		}
		bool result = true;
		if (node.Uid != null)
		{
			content = string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}{1}{2}", '#', BamlResourceContentUtil.EscapeString(node.Uid), ';');
		}
		else
		{
			_resolver.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(key, BamlLocalizerError.UidMissingOnChildElement));
			result = false;
		}
		return result;
	}

	private void GetLocalizabilityForElementNode(BamlStartElementNode node, out LocalizabilityAttribute localizability, out string formattingTag)
	{
		localizability = null;
		formattingTag = null;
		string assemblyName = node.AssemblyName;
		string typeFullName = node.TypeFullName;
		ElementLocalizability elementLocalizability = _resolver.GetElementLocalizability(assemblyName, typeFullName);
		LocalizabilityGroup localizabilityGroup = null;
		localizabilityGroup = _resolver.GetLocalizabilityComment(node, "$Content");
		if (localizabilityGroup != null)
		{
			localizability = localizabilityGroup.Override(elementLocalizability.Attribute);
		}
		else
		{
			localizability = elementLocalizability.Attribute;
		}
		formattingTag = elementLocalizability.FormattingTag;
	}

	private void GetLocalizabilityForPropertyNode(BamlStartComplexPropertyNode node, out LocalizabilityAttribute localizability)
	{
		localizability = null;
		string assemblyName = node.AssemblyName;
		string ownerTypeFullName = node.OwnerTypeFullName;
		string propertyName = node.PropertyName;
		if (ownerTypeFullName == null || ownerTypeFullName.Length == 0)
		{
			GetLocalizabilityForElementNode((BamlStartElementNode)node.Parent, out localizability, out var _);
			return;
		}
		LocalizabilityGroup localizabilityComment = _resolver.GetLocalizabilityComment((BamlStartElementNode)node.Parent, node.PropertyName);
		localizability = _resolver.GetPropertyLocalizability(assemblyName, ownerTypeFullName, propertyName);
		if (localizabilityComment != null)
		{
			localizability = localizabilityComment.Override(localizability);
		}
	}

	private LocalizabilityAttribute CombineAndPropagateInheritanceValues(ILocalizabilityInheritable node, LocalizabilityAttribute localizabilityFromSource)
	{
		if (node == null)
		{
			return localizabilityFromSource;
		}
		if (node.InheritableAttribute != null)
		{
			if (node.IsIgnored)
			{
				return LocalizabilityIgnore;
			}
			return node.InheritableAttribute;
		}
		if (localizabilityFromSource.Category != LocalizationCategory.Ignore && localizabilityFromSource.Category != LocalizationCategory.Inherit && localizabilityFromSource.Readability != Readability.Inherit && localizabilityFromSource.Modifiability != Modifiability.Inherit)
		{
			node.InheritableAttribute = localizabilityFromSource;
			return node.InheritableAttribute;
		}
		ILocalizabilityInheritable localizabilityAncestor = node.LocalizabilityAncestor;
		LocalizabilityAttribute localizability = localizabilityAncestor.InheritableAttribute;
		if (localizability == null)
		{
			if (localizabilityAncestor is BamlStartElementNode node2)
			{
				GetLocalizabilityForElementNode(node2, out localizability, out var _);
			}
			else
			{
				BamlStartComplexPropertyNode node3 = localizabilityAncestor as BamlStartComplexPropertyNode;
				GetLocalizabilityForPropertyNode(node3, out localizability);
			}
			CombineAndPropagateInheritanceValues(localizabilityAncestor, localizability);
			localizability = localizabilityAncestor.InheritableAttribute;
		}
		if (localizabilityFromSource.Category == LocalizationCategory.Ignore)
		{
			node.InheritableAttribute = localizability;
			node.IsIgnored = true;
			return LocalizabilityIgnore;
		}
		BamlTreeNode bamlTreeNode = (BamlTreeNode)node;
		switch (bamlTreeNode.NodeType)
		{
		case BamlNodeType.StartElement:
		case BamlNodeType.LiteralContent:
			if (localizabilityFromSource.Category == LocalizationCategory.Inherit && localizabilityFromSource.Readability == Readability.Inherit && localizabilityFromSource.Modifiability == Modifiability.Inherit)
			{
				node.InheritableAttribute = localizability;
			}
			else
			{
				node.InheritableAttribute = CreateInheritedLocalizability(localizabilityFromSource, localizability);
			}
			break;
		case BamlNodeType.Property:
		case BamlNodeType.StartComplexProperty:
		{
			ILocalizabilityInheritable localizabilityInheritable = (ILocalizabilityInheritable)bamlTreeNode.Parent;
			LocalizabilityAttribute inheritable = CombineMinimumLocalizability(localizability, localizabilityInheritable.InheritableAttribute);
			node.InheritableAttribute = CreateInheritedLocalizability(localizabilityFromSource, inheritable);
			if (localizabilityInheritable.IsIgnored && localizabilityFromSource.Category == LocalizationCategory.Inherit)
			{
				node.IsIgnored = true;
				return LocalizabilityIgnore;
			}
			break;
		}
		}
		return node.InheritableAttribute;
	}

	private LocalizabilityAttribute CreateInheritedLocalizability(LocalizabilityAttribute source, LocalizabilityAttribute inheritable)
	{
		LocalizationCategory category = ((source.Category == LocalizationCategory.Inherit) ? inheritable.Category : source.Category);
		Readability readability = ((source.Readability == Readability.Inherit) ? inheritable.Readability : source.Readability);
		Modifiability modifiability = ((source.Modifiability == Modifiability.Inherit) ? inheritable.Modifiability : source.Modifiability);
		return new LocalizabilityAttribute(category)
		{
			Readability = readability,
			Modifiability = modifiability
		};
	}

	private LocalizabilityAttribute CombineMinimumLocalizability(LocalizabilityAttribute first, LocalizabilityAttribute second)
	{
		if (first == null || second == null)
		{
			if (first != null)
			{
				return first;
			}
			return second;
		}
		Readability readability = (Readability)Math.Min((int)first.Readability, (int)second.Readability);
		Modifiability modifiability = (Modifiability)Math.Min((int)first.Modifiability, (int)second.Modifiability);
		LocalizationCategory localizationCategory = LocalizationCategory.None;
		localizationCategory = ((first.Category != LocalizationCategory.NeverLocalize && second.Category != LocalizationCategory.NeverLocalize) ? ((first.Category != LocalizationCategory.Ignore && second.Category != LocalizationCategory.Ignore) ? ((first.Category != 0) ? first.Category : second.Category) : LocalizationCategory.Ignore) : LocalizationCategory.NeverLocalize);
		return new LocalizabilityAttribute(localizationCategory)
		{
			Readability = readability,
			Modifiability = modifiability
		};
	}
}
