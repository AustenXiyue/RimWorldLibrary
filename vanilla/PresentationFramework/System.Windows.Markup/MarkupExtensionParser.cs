using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using MS.Internal.Xaml.Parser;

namespace System.Windows.Markup;

internal class MarkupExtensionParser
{
	internal class UnknownMarkupExtension
	{
	}

	private IParserHelper _parserHelper;

	private ParserContext _parserContext;

	internal MarkupExtensionParser(IParserHelper parserHelper, ParserContext parserContext)
	{
		_parserHelper = parserHelper;
		_parserContext = parserContext;
	}

	internal AttributeData IsMarkupExtensionAttribute(Type declaringType, string propIdName, ref string attrValue, int lineNumber, int linePosition, int depth, object info)
	{
		if (!GetMarkupExtensionTypeAndArgs(ref attrValue, out var typeName, out var args))
		{
			return null;
		}
		return FillAttributeData(declaringType, propIdName, typeName, args, attrValue, lineNumber, linePosition, depth, info);
	}

	internal DefAttributeData IsMarkupExtensionDefAttribute(Type declaringType, ref string attrValue, int lineNumber, int linePosition, int depth)
	{
		if (!GetMarkupExtensionTypeAndArgs(ref attrValue, out var typeName, out var args))
		{
			return null;
		}
		return FillDefAttributeData(declaringType, typeName, args, attrValue, lineNumber, linePosition, depth);
	}

	internal static bool LooksLikeAMarkupExtension(string attrValue)
	{
		if (attrValue.Length < 2)
		{
			return false;
		}
		if (attrValue[0] != '{')
		{
			return false;
		}
		if (attrValue[1] == '}')
		{
			return false;
		}
		return true;
	}

	internal static string AddEscapeToLiteralString(string literalString)
	{
		string text = literalString;
		if (!string.IsNullOrEmpty(text) && text[0] == '{')
		{
			text = "{}" + text;
		}
		return text;
	}

	private KnownElements GetKnownExtensionFromType(Type extensionType, out string propName)
	{
		if (KnownTypes.Types[691] == extensionType)
		{
			propName = "TypeName";
			return KnownElements.TypeExtension;
		}
		if (KnownTypes.Types[602] == extensionType)
		{
			propName = "Member";
			return KnownElements.StaticExtension;
		}
		if (KnownTypes.Types[634] == extensionType)
		{
			propName = "Property";
			return KnownElements.TemplateBindingExtension;
		}
		if (KnownTypes.Types[189] == extensionType)
		{
			propName = "ResourceKey";
			return KnownElements.DynamicResourceExtension;
		}
		if (KnownTypes.Types[603] == extensionType)
		{
			propName = "ResourceKey";
			return KnownElements.StaticResourceExtension;
		}
		propName = string.Empty;
		return KnownElements.UnknownElement;
	}

	private bool IsSimpleTypeExtensionArgs(Type extensionType, int lineNumber, int linePosition, ref string args)
	{
		if (KnownTypes.Types[691] == extensionType)
		{
			return IsSimpleExtensionArgs(lineNumber, linePosition, "TypeName", ref args, extensionType);
		}
		return false;
	}

	private bool IsSimpleExtension(Type extensionType, int lineNumber, int linePosition, int depth, out short extensionTypeId, out bool isValueNestedExtension, out bool isValueTypeExtension, ref string args)
	{
		bool flag = false;
		extensionTypeId = 0;
		isValueNestedExtension = false;
		isValueTypeExtension = false;
		string propName;
		KnownElements knownExtensionFromType = GetKnownExtensionFromType(extensionType, out propName);
		if (knownExtensionFromType != 0)
		{
			flag = IsSimpleExtensionArgs(lineNumber, linePosition, propName, ref args, extensionType);
		}
		if (flag)
		{
			if ((knownExtensionFromType == KnownElements.DynamicResourceExtension || knownExtensionFromType == KnownElements.StaticResourceExtension) && LooksLikeAMarkupExtension(args))
			{
				AttributeData attributeData = IsMarkupExtensionAttribute(extensionType, null, ref args, lineNumber, linePosition, depth, null);
				isValueTypeExtension = attributeData.IsTypeExtension;
				flag = (isValueNestedExtension = isValueTypeExtension || attributeData.IsStaticExtension);
				if (flag)
				{
					args = attributeData.Args;
				}
				else
				{
					args += "}";
				}
			}
			if (flag)
			{
				extensionTypeId = (short)knownExtensionFromType;
			}
		}
		return flag;
	}

	private bool IsSimpleExtensionArgs(int lineNumber, int linePosition, string propName, ref string args, Type targetType)
	{
		ArrayList arrayList = TokenizeAttributes(args, lineNumber, linePosition, targetType);
		if (arrayList == null)
		{
			return false;
		}
		if (arrayList.Count == 1)
		{
			args = (string)arrayList[0];
			return true;
		}
		if (arrayList.Count == 3 && (string)arrayList[0] == propName)
		{
			args = (string)arrayList[2];
			return true;
		}
		return false;
	}

	internal static bool GetMarkupExtensionTypeAndArgs(ref string attrValue, out string typeName, out string args)
	{
		int length = attrValue.Length;
		typeName = string.Empty;
		args = string.Empty;
		if (length < 1 || attrValue[0] != '{')
		{
			return false;
		}
		bool flag = false;
		StringBuilder stringBuilder = null;
		int i;
		for (i = 1; i < length; i++)
		{
			if (char.IsWhiteSpace(attrValue[i]))
			{
				if (stringBuilder != null)
				{
					break;
				}
			}
			else if (stringBuilder == null)
			{
				if (!flag && attrValue[i] == '\\')
				{
					flag = true;
				}
				else if (attrValue[i] == '}')
				{
					if (i == 1)
					{
						attrValue = attrValue.Substring(2);
						return false;
					}
				}
				else
				{
					stringBuilder = new StringBuilder(length - i);
					stringBuilder.Append(attrValue[i]);
					flag = false;
				}
			}
			else if (!flag && attrValue[i] == '\\')
			{
				flag = true;
			}
			else
			{
				if (attrValue[i] == '}')
				{
					break;
				}
				stringBuilder.Append(attrValue[i]);
				flag = false;
			}
		}
		if (stringBuilder != null)
		{
			typeName = stringBuilder.ToString();
		}
		if (i < length - 1)
		{
			args = attrValue.Substring(i, length - i);
		}
		else if (attrValue[length - 1] == '}')
		{
			args = "}";
		}
		return true;
	}

	private DefAttributeData FillDefAttributeData(Type declaringType, string typename, string args, string attributeValue, int lineNumber, int linePosition, int depth)
	{
		bool isSimple = false;
		if (GetExtensionType(typename, attributeValue, lineNumber, linePosition, out var namespaceURI, out var targetAssemblyName, out var targetFullName, out var targetType, out var _))
		{
			isSimple = IsSimpleTypeExtensionArgs(targetType, lineNumber, linePosition, ref args);
		}
		return new DefAttributeData(targetAssemblyName, targetFullName, targetType, args, declaringType, namespaceURI, lineNumber, linePosition, depth, isSimple);
	}

	private AttributeData FillAttributeData(Type declaringType, string propIdName, string typename, string args, string attributeValue, int lineNumber, int linePosition, int depth, object info)
	{
		bool flag = false;
		short extensionTypeId = 0;
		bool isValueNestedExtension = false;
		bool isValueTypeExtension = false;
		if (GetExtensionType(typename, attributeValue, lineNumber, linePosition, out var namespaceURI, out var targetAssemblyName, out var targetFullName, out var targetType, out var serializerType) && propIdName != string.Empty)
		{
			if (propIdName == null)
			{
				if (KnownTypes.Types[691] == targetType)
				{
					flag = IsSimpleExtensionArgs(lineNumber, linePosition, "TypeName", ref args, targetType);
					isValueNestedExtension = flag;
					isValueTypeExtension = flag;
					extensionTypeId = 691;
				}
				else if (KnownTypes.Types[602] == targetType)
				{
					flag = IsSimpleExtensionArgs(lineNumber, linePosition, "Member", ref args, targetType);
					isValueNestedExtension = flag;
					extensionTypeId = 602;
				}
			}
			else
			{
				propIdName = propIdName.Trim();
				flag = IsSimpleExtension(targetType, lineNumber, linePosition, depth, out extensionTypeId, out isValueNestedExtension, out isValueTypeExtension, ref args);
			}
		}
		return new AttributeData(targetAssemblyName, targetFullName, targetType, args, declaringType, propIdName, info, serializerType, lineNumber, linePosition, depth, namespaceURI, extensionTypeId, isValueNestedExtension, isValueTypeExtension, flag);
	}

	private bool GetExtensionType(string typename, string attributeValue, int lineNumber, int linePosition, out string namespaceURI, out string targetAssemblyName, out string targetFullName, out Type targetType, out Type serializerType)
	{
		targetAssemblyName = null;
		targetFullName = null;
		targetType = null;
		serializerType = null;
		string text = typename;
		string prefix = string.Empty;
		int num = typename.IndexOf(':');
		if (num >= 0)
		{
			prefix = typename.Substring(0, num);
			typename = typename.Substring(num + 1);
		}
		namespaceURI = _parserHelper.LookupNamespace(prefix);
		bool elementType = _parserHelper.GetElementType(extensionFirst: true, typename, namespaceURI, ref targetAssemblyName, ref targetFullName, ref targetType, ref serializerType);
		if (!elementType)
		{
			if (_parserHelper.CanResolveLocalAssemblies())
			{
				ThrowException("ParserNotMarkupExtension", attributeValue, typename, namespaceURI, lineNumber, linePosition);
				return elementType;
			}
			targetFullName = text;
			targetType = typeof(UnknownMarkupExtension);
			return elementType;
		}
		if (!KnownTypes.Types[381].IsAssignableFrom(targetType))
		{
			ThrowException("ParserNotMarkupExtension", attributeValue, typename, namespaceURI, lineNumber, linePosition);
		}
		return elementType;
	}

	internal ArrayList CompileAttributes(ArrayList markupExtensionList, int startingDepth)
	{
		ArrayList arrayList = new ArrayList(markupExtensionList.Count * 5);
		for (int i = 0; i < markupExtensionList.Count; i++)
		{
			AttributeData data = (AttributeData)markupExtensionList[i];
			CompileAttribute(arrayList, data);
		}
		return arrayList;
	}

	internal void CompileAttribute(ArrayList xamlNodes, AttributeData data)
	{
		string fullName = data.DeclaringType.Assembly.FullName;
		string fullName2 = data.DeclaringType.FullName;
		XamlTypeMapper.GetPropertyType(data.Info, out var propertyType, out var propertyCanWrite);
		XamlNode value;
		XamlNode value2;
		switch (BamlRecordManager.GetPropertyStartRecordType(propertyType, propertyCanWrite))
		{
		case BamlRecordType.PropertyArrayStart:
			value = new XamlPropertyArrayStartNode(data.LineNumber, data.LinePosition, data.Depth, data.Info, fullName, fullName2, data.PropertyName);
			value2 = new XamlPropertyArrayEndNode(data.LineNumber, data.LinePosition, data.Depth);
			break;
		case BamlRecordType.PropertyIDictionaryStart:
			value = new XamlPropertyIDictionaryStartNode(data.LineNumber, data.LinePosition, data.Depth, data.Info, fullName, fullName2, data.PropertyName);
			value2 = new XamlPropertyIDictionaryEndNode(data.LineNumber, data.LinePosition, data.Depth);
			break;
		case BamlRecordType.PropertyIListStart:
			value = new XamlPropertyIListStartNode(data.LineNumber, data.LinePosition, data.Depth, data.Info, fullName, fullName2, data.PropertyName);
			value2 = new XamlPropertyIListEndNode(data.LineNumber, data.LinePosition, data.Depth);
			break;
		default:
			value = new XamlPropertyComplexStartNode(data.LineNumber, data.LinePosition, data.Depth, data.Info, fullName, fullName2, data.PropertyName);
			value2 = new XamlPropertyComplexEndNode(data.LineNumber, data.LinePosition, data.Depth);
			break;
		}
		xamlNodes.Add(value);
		CompileAttributeCore(xamlNodes, data);
		xamlNodes.Add(value2);
	}

	internal void CompileAttributeCore(ArrayList xamlNodes, AttributeData data)
	{
		string text = null;
		string xmlNamespace = null;
		ArrayList arrayList = TokenizeAttributes(data.Args, data.LineNumber, data.LinePosition, data.TargetType);
		if (data.TargetType == typeof(UnknownMarkupExtension))
		{
			text = data.TargetFullName;
			string prefix = string.Empty;
			int num = text.IndexOf(':');
			if (num >= 0)
			{
				prefix = text.Substring(0, num);
				text = text.Substring(num + 1);
			}
			xmlNamespace = _parserHelper.LookupNamespace(prefix);
			xamlNodes.Add(new XamlUnknownTagStartNode(data.LineNumber, data.LinePosition, ++data.Depth, xmlNamespace, text));
		}
		else
		{
			xamlNodes.Add(new XamlElementStartNode(data.LineNumber, data.LinePosition, ++data.Depth, data.TargetAssemblyName, data.TargetFullName, data.TargetType, data.SerializerType));
		}
		xamlNodes.Add(new XamlEndAttributesNode(data.LineNumber, data.LinePosition, data.Depth, compact: true));
		int listIndex = 0;
		if (arrayList != null && (arrayList.Count == 1 || (arrayList.Count > 1 && !(arrayList[1] is string) && (char)arrayList[1] == ',')))
		{
			WriteConstructorParams(xamlNodes, arrayList, data, ref listIndex);
		}
		WriteProperties(xamlNodes, arrayList, listIndex, data);
		if (data.TargetType == typeof(UnknownMarkupExtension))
		{
			xamlNodes.Add(new XamlUnknownTagEndNode(data.LineNumber, data.LinePosition, data.Depth--, text, xmlNamespace));
		}
		else
		{
			xamlNodes.Add(new XamlElementEndNode(data.LineNumber, data.LinePosition, data.Depth--));
		}
	}

	internal ArrayList CompileDictionaryKeys(ArrayList complexDefAttributesList, int startingDepth)
	{
		ArrayList arrayList = new ArrayList(complexDefAttributesList.Count * 5);
		for (int i = 0; i < complexDefAttributesList.Count; i++)
		{
			DefAttributeData data = (DefAttributeData)complexDefAttributesList[i];
			CompileDictionaryKey(arrayList, data);
		}
		return arrayList;
	}

	internal void CompileDictionaryKey(ArrayList xamlNodes, DefAttributeData data)
	{
		ArrayList arrayList = TokenizeAttributes(data.Args, data.LineNumber, data.LinePosition, data.TargetType);
		xamlNodes.Add(new XamlKeyElementStartNode(data.LineNumber, data.LinePosition, ++data.Depth, data.TargetAssemblyName, data.TargetFullName, data.TargetType, null));
		xamlNodes.Add(new XamlEndAttributesNode(data.LineNumber, data.LinePosition, data.Depth, compact: true));
		int listIndex = 0;
		if (arrayList != null && (arrayList.Count == 1 || (arrayList.Count > 1 && !(arrayList[1] is string) && (char)arrayList[1] == ',')))
		{
			WriteConstructorParams(xamlNodes, arrayList, data, ref listIndex);
		}
		WriteProperties(xamlNodes, arrayList, listIndex, data);
		xamlNodes.Add(new XamlKeyElementEndNode(data.LineNumber, data.LinePosition, data.Depth--));
	}

	private ArrayList TokenizeAttributes(string args, int lineNumber, int linePosition, Type extensionType)
	{
		if (extensionType == typeof(UnknownMarkupExtension))
		{
			return null;
		}
		int maxConstructorArguments = 0;
		ParameterInfo[] array = FindLongestConstructor(extensionType, out maxConstructorArguments);
		Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters> dictionary = _parserContext.InitBracketCharacterCacheForType(extensionType);
		Stack<char> stack = new Stack<char>();
		int num = 0;
		bool flag = array != null && maxConstructorArguments > 0;
		bool flag2 = false;
		ArrayList arrayList = null;
		int length = args.Length;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		char c = '\'';
		int num2 = 0;
		StringBuilder stringBuilder = null;
		int num3 = 0;
		string text = null;
		MS.Internal.Xaml.Parser.SpecialBracketCharacters specialBracketCharacters = null;
		if (flag && dictionary != null)
		{
			text = ((maxConstructorArguments > 0) ? array[num].Name : null);
			if (!string.IsNullOrEmpty(text))
			{
				specialBracketCharacters = GetBracketCharacterForProperty(text, dictionary);
			}
		}
		for (num3 = 0; num3 < length; num3++)
		{
			if (flag6)
			{
				break;
			}
			if (!flag4 && args[num3] == '\\')
			{
				flag4 = true;
				continue;
			}
			if (!flag5 && !char.IsWhiteSpace(args[num3]))
			{
				flag5 = true;
			}
			if (!(flag3 || num2 > 0 || flag5))
			{
				continue;
			}
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(length);
				arrayList = new ArrayList(1);
			}
			if (flag4)
			{
				stringBuilder.Append('\\');
				stringBuilder.Append(args[num3]);
				flag4 = false;
				continue;
			}
			if (flag3 || num2 > 0)
			{
				if (flag3 && args[num3] == c)
				{
					flag3 = false;
					flag5 = false;
					AddToTokenList(arrayList, stringBuilder, trim: false);
					continue;
				}
				if (num2 > 0 && args[num3] == '}')
				{
					num2--;
				}
				else if (args[num3] == '{')
				{
					num2++;
				}
				stringBuilder.Append(args[num3]);
				continue;
			}
			if (flag2)
			{
				stringBuilder.Append(args[num3]);
				if (specialBracketCharacters.StartsEscapeSequence(args[num3]))
				{
					stack.Push(args[num3]);
				}
				else if (specialBracketCharacters.EndsEscapeSequence(args[num3]))
				{
					if (specialBracketCharacters.Match(stack.Peek(), args[num3]))
					{
						stack.Pop();
					}
					else
					{
						ThrowException("ParserMarkupExtensionInvalidClosingBracketCharacers", args[num3].ToString(), lineNumber, linePosition);
					}
				}
				if (stack.Count == 0)
				{
					flag2 = false;
				}
				continue;
			}
			switch (args[num3])
			{
			case '"':
			case '\'':
				if (stringBuilder.Length != 0)
				{
					ThrowException("ParserMarkupExtensionNoQuotesInName", args, lineNumber, linePosition);
				}
				flag3 = true;
				c = args[num3];
				break;
			case ',':
			case '=':
				if (flag && args[num3] == ',')
				{
					flag = ++num < maxConstructorArguments;
					if (flag)
					{
						text = array[num].Name;
						specialBracketCharacters = GetBracketCharacterForProperty(text, dictionary);
					}
				}
				if (stringBuilder != null && stringBuilder.Length > 0)
				{
					AddToTokenList(arrayList, stringBuilder, trim: true);
					if (stack.Count != 0)
					{
						ThrowException("ParserMarkupExtensionMalformedBracketCharacers", stack.Peek().ToString(), lineNumber, linePosition);
					}
				}
				else if (arrayList.Count == 0)
				{
					ThrowException("ParserMarkupExtensionDelimiterBeforeFirstAttribute", args, lineNumber, linePosition);
				}
				else if (arrayList[arrayList.Count - 1] is char)
				{
					ThrowException("ParserMarkupExtensionBadDelimiter", args, lineNumber, linePosition);
				}
				if (args[num3] == '=')
				{
					flag = false;
					text = (string)arrayList[arrayList.Count - 1];
					specialBracketCharacters = GetBracketCharacterForProperty(text, dictionary);
				}
				arrayList.Add(args[num3]);
				flag5 = false;
				break;
			case '}':
				flag6 = true;
				if (stringBuilder != null)
				{
					if (stringBuilder.Length > 0)
					{
						AddToTokenList(arrayList, stringBuilder, trim: true);
					}
					else if (arrayList.Count > 0 && arrayList[arrayList.Count - 1] is char)
					{
						ThrowException("ParserMarkupExtensionBadDelimiter", args, lineNumber, linePosition);
					}
				}
				break;
			case '{':
				num2++;
				stringBuilder.Append(args[num3]);
				break;
			default:
				if (specialBracketCharacters != null && specialBracketCharacters.StartsEscapeSequence(args[num3]))
				{
					stack.Clear();
					stack.Push(args[num3]);
					flag2 = true;
				}
				stringBuilder.Append(args[num3]);
				break;
			}
		}
		if (!flag6)
		{
			ThrowException("ParserMarkupExtensionNoEndCurlie", "}", lineNumber, linePosition);
		}
		else if (num3 < length)
		{
			ThrowException("ParserMarkupExtensionTrailingGarbage", "}", args.Substring(num3, length - num3), lineNumber, linePosition);
		}
		return arrayList;
	}

	private static void AddToTokenList(ArrayList list, StringBuilder sb, bool trim)
	{
		if (trim)
		{
			int num = sb.Length - 1;
			while (char.IsWhiteSpace(sb[num]))
			{
				num--;
			}
			sb.Length = num + 1;
		}
		list.Add(sb.ToString());
		sb.Length = 0;
	}

	private ParameterInfo[] FindLongestConstructor(Type extensionType, out int maxConstructorArguments)
	{
		ParameterInfo[] result = null;
		ConstructorInfo[] constructors = extensionType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		maxConstructorArguments = 0;
		ConstructorInfo[] array = constructors;
		for (int i = 0; i < array.Length; i++)
		{
			ParameterInfo[] parameters = array[i].GetParameters();
			if (parameters.Length >= maxConstructorArguments)
			{
				maxConstructorArguments = parameters.Length;
				result = parameters;
			}
		}
		return result;
	}

	private void WriteConstructorParams(ArrayList xamlNodes, ArrayList list, DefAttributeData data, ref int listIndex)
	{
		if (list == null || listIndex >= list.Count)
		{
			return;
		}
		xamlNodes.Add(new XamlConstructorParametersStartNode(data.LineNumber, data.LinePosition, ++data.Depth));
		while (listIndex < list.Count)
		{
			if (!(list[listIndex] is string))
			{
				ThrowException("ParserMarkupExtensionBadConstructorParam", data.Args, data.LineNumber, data.LinePosition);
			}
			if (list.Count > listIndex + 1 && list[listIndex + 1] is char && (char)list[listIndex + 1] == '=')
			{
				break;
			}
			string attrValue = (string)list[listIndex];
			AttributeData attributeData = IsMarkupExtensionAttribute(data.DeclaringType, string.Empty, ref attrValue, data.LineNumber, data.LinePosition, data.Depth, null);
			if (attributeData == null)
			{
				RemoveEscapes(ref attrValue);
				xamlNodes.Add(new XamlTextNode(data.LineNumber, data.LinePosition, data.Depth, attrValue, null));
			}
			else
			{
				CompileAttributeCore(xamlNodes, attributeData);
			}
			listIndex += 2;
		}
		xamlNodes.Add(new XamlConstructorParametersEndNode(data.LineNumber, data.LinePosition, data.Depth--));
	}

	private void WriteProperties(ArrayList xamlNodes, ArrayList list, int listIndex, DefAttributeData data)
	{
		if (list == null || listIndex >= list.Count)
		{
			return;
		}
		ArrayList arrayList = new ArrayList(list.Count / 4);
		for (int i = listIndex; i < list.Count; i += 4)
		{
			if (i > list.Count - 3 || list[i + 1] is string || (char)list[i + 1] != '=')
			{
				ThrowException("ParserMarkupExtensionNoNameValue", data.Args, data.LineNumber, data.LinePosition);
			}
			string text = list[i] as string;
			text = text.Trim();
			if (arrayList.Contains(text))
			{
				ThrowException("ParserDuplicateMarkupExtensionProperty", text, data.LineNumber, data.LinePosition);
			}
			arrayList.Add(text);
			int num = text.IndexOf(':');
			string text2 = ((num < 0) ? text : text.Substring(num + 1));
			string prefix = ((num < 0) ? string.Empty : text.Substring(0, num));
			string attributeNamespaceUri = ResolveAttributeNamespaceURI(prefix, text2, data.TargetNamespaceUri);
			GetAttributeContext(data.TargetType, data.TargetNamespaceUri, attributeNamespaceUri, text2, out var dynamicObject, out var _, out var _, out var _, out var _);
			string attrValue = list[i + 2] as string;
			AttributeData attributeData = IsMarkupExtensionAttribute(data.TargetType, text, ref attrValue, data.LineNumber, data.LinePosition, data.Depth, dynamicObject);
			list[i + 2] = attrValue;
			if (data.IsUnknownExtension)
			{
				break;
			}
			if (attributeData != null)
			{
				if (attributeData.IsSimple)
				{
					CompileProperty(xamlNodes, text, attributeData.Args, data.TargetType, data.TargetNamespaceUri, attributeData, attributeData.LineNumber, attributeData.LinePosition, attributeData.Depth);
				}
				else
				{
					CompileAttribute(xamlNodes, attributeData);
				}
			}
			else
			{
				CompileProperty(xamlNodes, text, (string)list[i + 2], data.TargetType, data.TargetNamespaceUri, null, data.LineNumber, data.LinePosition, data.Depth);
			}
		}
	}

	private string ResolveAttributeNamespaceURI(string prefix, string name, string parentURI)
	{
		if (!string.IsNullOrEmpty(prefix))
		{
			return _parserHelper.LookupNamespace(prefix);
		}
		int num = name.IndexOf('.');
		if (-1 == num)
		{
			return parentURI;
		}
		return _parserHelper.LookupNamespace("");
	}

	private MS.Internal.Xaml.Parser.SpecialBracketCharacters GetBracketCharacterForProperty(string propertyName, Dictionary<string, MS.Internal.Xaml.Parser.SpecialBracketCharacters> bracketCharacterCache)
	{
		MS.Internal.Xaml.Parser.SpecialBracketCharacters result = null;
		if (bracketCharacterCache != null && bracketCharacterCache.ContainsKey(propertyName))
		{
			result = bracketCharacterCache[propertyName];
		}
		return result;
	}

	private void CompileProperty(ArrayList xamlNodes, string name, string value, Type parentType, string parentTypeNamespaceUri, AttributeData data, int lineNumber, int linePosition, int depth)
	{
		RemoveEscapes(ref name);
		RemoveEscapes(ref value);
		int num = name.IndexOf(':');
		string text = ((num < 0) ? name : name.Substring(num + 1));
		string text2 = ((num < 0) ? string.Empty : name.Substring(0, num));
		string text3 = ResolveAttributeNamespaceURI(text2, text, parentTypeNamespaceUri);
		if (string.IsNullOrEmpty(text3))
		{
			ThrowException("ParserPrefixNSProperty", text2, name, lineNumber, linePosition);
		}
		if (GetAttributeContext(parentType, parentTypeNamespaceUri, text3, text, out var dynamicObject, out var assemblyName, out var typeFullName, out var _, out var dynamicObjectName) != AttributeContext.Property)
		{
			ThrowException("ParserMarkupExtensionUnknownAttr", text, parentType.FullName, lineNumber, linePosition);
		}
		if (data != null && data.IsSimple)
		{
			if (data.IsTypeExtension)
			{
				string valueTypeFullName = value;
				string valueAssemblyName = null;
				Type typeFromBaseString = _parserContext.XamlTypeMapper.GetTypeFromBaseString(value, _parserContext, throwOnError: true);
				if (typeFromBaseString != null)
				{
					valueTypeFullName = typeFromBaseString.FullName;
					valueAssemblyName = typeFromBaseString.Assembly.FullName;
				}
				XamlPropertyWithTypeNode value2 = new XamlPropertyWithTypeNode(data.LineNumber, data.LinePosition, data.Depth, dynamicObject, assemblyName, typeFullName, text, valueTypeFullName, valueAssemblyName, typeFromBaseString, string.Empty, string.Empty);
				xamlNodes.Add(value2);
			}
			else
			{
				XamlPropertyWithExtensionNode value3 = new XamlPropertyWithExtensionNode(data.LineNumber, data.LinePosition, data.Depth, dynamicObject, assemblyName, typeFullName, text, value, data.ExtensionTypeId, data.IsValueNestedExtension, data.IsValueTypeExtension);
				xamlNodes.Add(value3);
			}
		}
		else
		{
			XamlPropertyNode value4 = new XamlPropertyNode(lineNumber, linePosition, depth, dynamicObject, assemblyName, typeFullName, dynamicObjectName, value, BamlAttributeUsage.Default, complexAsSimple: true);
			xamlNodes.Add(value4);
		}
	}

	internal static void RemoveEscapes(ref string value)
	{
		StringBuilder stringBuilder = null;
		bool flag = true;
		for (int i = 0; i < value.Length; i++)
		{
			if (flag && value[i] == '\\')
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(value.Length);
					stringBuilder.Append(value.Substring(0, i));
				}
				flag = false;
			}
			else if (stringBuilder != null)
			{
				stringBuilder.Append(value[i]);
				flag = true;
			}
		}
		if (stringBuilder != null)
		{
			value = stringBuilder.ToString();
		}
	}

	private AttributeContext GetAttributeContext(Type elementBaseType, string elementBaseTypeNamespaceUri, string attributeNamespaceUri, string attributeLocalName, out object dynamicObject, out string assemblyName, out string typeFullName, out Type declaringType, out string dynamicObjectName)
	{
		AttributeContext result = AttributeContext.Unknown;
		dynamicObject = null;
		assemblyName = null;
		typeFullName = null;
		declaringType = null;
		dynamicObjectName = null;
		MemberInfo clrInfo = _parserContext.XamlTypeMapper.GetClrInfo(isEvent: false, elementBaseType, attributeNamespaceUri, attributeLocalName, ref dynamicObjectName);
		if (null != clrInfo)
		{
			result = AttributeContext.Property;
			dynamicObject = clrInfo;
			declaringType = clrInfo.DeclaringType;
			typeFullName = declaringType.FullName;
			assemblyName = declaringType.Assembly.FullName;
		}
		return result;
	}

	private void ThrowException(string id, string parameter1, int lineNumber, int linePosition)
	{
		string message = SR.Format(SR.GetResourceString(id), parameter1);
		ThrowExceptionWithLine(message, lineNumber, linePosition);
	}

	private void ThrowException(string id, string parameter1, string parameter2, int lineNumber, int linePosition)
	{
		string message = SR.Format(SR.GetResourceString(id), parameter1, parameter2);
		ThrowExceptionWithLine(message, lineNumber, linePosition);
	}

	private void ThrowException(string id, string parameter1, string parameter2, string parameter3, int lineNumber, int linePosition)
	{
		string message = SR.Format(SR.GetResourceString(id), parameter1, parameter2, parameter3);
		ThrowExceptionWithLine(message, lineNumber, linePosition);
	}

	private void ThrowExceptionWithLine(string message, int lineNumber, int linePosition)
	{
		message += " ";
		message += SR.Format(SR.ParserLineAndOffset, lineNumber.ToString(CultureInfo.CurrentCulture), linePosition.ToString(CultureInfo.CurrentCulture));
		throw new XamlParseException(message, lineNumber, linePosition);
	}
}
