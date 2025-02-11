using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using MS.Internal;

namespace System.Windows.Documents;

internal static class TextRangeSerialization
{
	private const int EmptyDocumentDepth = 1;

	internal static void WriteXaml(XmlWriter xmlWriter, ITextRange range, bool useFlowDocumentAsRoot, WpfPayload wpfPayload)
	{
		WriteXaml(xmlWriter, range, useFlowDocumentAsRoot, wpfPayload, preserveTextElements: false);
	}

	internal static void WriteXaml(XmlWriter xmlWriter, ITextRange range, bool useFlowDocumentAsRoot, WpfPayload wpfPayload, bool preserveTextElements)
	{
		Formatting formatting = Formatting.None;
		if (xmlWriter is XmlTextWriter)
		{
			formatting = ((XmlTextWriter)xmlWriter).Formatting;
			((XmlTextWriter)xmlWriter).Formatting = Formatting.None;
		}
		XamlTypeMapper defaultMapper = XmlParserDefaults.DefaultMapper;
		ITextPointer textPointer = FindSerializationCommonAncestor(range);
		bool lastParagraphMustBeMerged = !TextPointerBase.IsAfterLastParagraph(range.End) && range.End.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementStart;
		WriteRootFlowDocument(range, textPointer, xmlWriter, defaultMapper, lastParagraphMustBeMerged, useFlowDocumentAsRoot);
		List<int> ignoreList = new List<int>();
		bool ignoreWriteHyperlinkEnd;
		int elementLevel = 1 + WriteOpeningTags(range, range.Start, textPointer, xmlWriter, defaultMapper, wpfPayload == null, out ignoreWriteHyperlinkEnd, ref ignoreList, preserveTextElements);
		if (range.IsTableCellRange)
		{
			WriteXamlTableCellRange(xmlWriter, range, defaultMapper, ref elementLevel, wpfPayload, preserveTextElements);
		}
		else
		{
			WriteXamlTextSegment(xmlWriter, range.Start, range.End, defaultMapper, ref elementLevel, wpfPayload, ignoreWriteHyperlinkEnd, ignoreList, preserveTextElements);
		}
		Invariant.Assert(elementLevel >= 0, "elementLevel cannot be negative");
		while (elementLevel-- > 0)
		{
			xmlWriter.WriteFullEndElement();
		}
		if (xmlWriter is XmlTextWriter)
		{
			((XmlTextWriter)xmlWriter).Formatting = formatting;
		}
	}

	internal static void PasteXml(TextRange range, TextElement fragment)
	{
		Invariant.Assert(fragment != null);
		if (!PasteSingleEmbeddedElement(range, fragment))
		{
			AdjustFragmentForTargetRange(fragment, range);
			if (!range.IsEmpty)
			{
				range.Text = string.Empty;
			}
			Invariant.Assert(range.IsEmpty, "range must be empty in the beginning of pasting");
			if (((ITextPointer)fragment.ContentStart).CompareTo((ITextPointer)fragment.ContentEnd) != 0)
			{
				PasteTextFragment(fragment, range);
			}
		}
	}

	private static void WriteXamlTextSegment(XmlWriter xmlWriter, ITextPointer rangeStart, ITextPointer rangeEnd, XamlTypeMapper xamlTypeMapper, ref int elementLevel, WpfPayload wpfPayload, bool ignoreWriteHyperlinkEnd, List<int> ignoreList, bool preserveTextElements)
	{
		if (elementLevel == 1 && typeof(Run).IsAssignableFrom(rangeStart.ParentType))
		{
			elementLevel++;
			xmlWriter.WriteStartElement(typeof(Run).Name);
		}
		ITextPointer textPointer = rangeStart.CreatePointer();
		while (rangeEnd.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart)
		{
			rangeEnd = rangeEnd.GetNextContextPosition(LogicalDirection.Backward);
		}
		while (textPointer.CompareTo(rangeEnd) < 0)
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.ElementStart:
			{
				TextElement textElement = (TextElement)textPointer.GetAdjacentElement(LogicalDirection.Forward);
				if (textElement is Hyperlink)
				{
					if (IsHyperlinkInvalid(textPointer, rangeEnd))
					{
						ignoreWriteHyperlinkEnd = true;
						textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
						break;
					}
				}
				else if (textElement != null)
				{
					TextElementEditingBehaviorAttribute textElementEditingBehaviorAttribute = (TextElementEditingBehaviorAttribute)Attribute.GetCustomAttribute(textElement.GetType(), typeof(TextElementEditingBehaviorAttribute));
					if (textElementEditingBehaviorAttribute != null && !textElementEditingBehaviorAttribute.IsTypographicOnly && IsPartialNonTypographic(textPointer, rangeEnd))
					{
						ITextPointer textPointer3 = textPointer.CreatePointer();
						textPointer3.MoveToElementEdge(ElementEdge.BeforeEnd);
						ignoreList.Add(textPointer3.Offset);
						textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
						break;
					}
				}
				elementLevel++;
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				WriteStartXamlElement(null, textPointer, xmlWriter, xamlTypeMapper, wpfPayload == null, preserveTextElements);
				break;
			}
			case TextPointerContext.ElementEnd:
				if (ignoreWriteHyperlinkEnd && textPointer.GetAdjacentElement(LogicalDirection.Forward) is Hyperlink)
				{
					ignoreWriteHyperlinkEnd = false;
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					break;
				}
				if (ignoreList.Count > 0)
				{
					ITextPointer textPointer2 = textPointer.CreatePointer();
					textPointer2.MoveToElementEdge(ElementEdge.BeforeEnd);
					if (ignoreList.Contains(textPointer2.Offset))
					{
						ignoreList.Remove(textPointer2.Offset);
						textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
						break;
					}
				}
				elementLevel--;
				if (TextSchema.IsBreak(textPointer.ParentType))
				{
					xmlWriter.WriteEndElement();
				}
				else
				{
					xmlWriter.WriteFullEndElement();
				}
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			case TextPointerContext.Text:
			{
				int textRunLength = textPointer.GetTextRunLength(LogicalDirection.Forward);
				char[] array = new char[textRunLength];
				textRunLength = TextPointerBase.GetTextWithLimit(textPointer, LogicalDirection.Forward, array, 0, textRunLength, rangeEnd);
				textRunLength = StripInvalidSurrogateChars(array, textRunLength);
				xmlWriter.WriteChars(array, 0, textRunLength);
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			}
			case TextPointerContext.EmbeddedElement:
			{
				object adjacentElement = textPointer.GetAdjacentElement(LogicalDirection.Forward);
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				WriteEmbeddedObject(adjacentElement, xmlWriter, wpfPayload);
				break;
			}
			default:
				Invariant.Assert(condition: false, "unexpected value of runType");
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				break;
			}
		}
	}

	private static void WriteXamlTableCellRange(XmlWriter xmlWriter, ITextRange range, XamlTypeMapper xamlTypeMapper, ref int elementLevel, WpfPayload wpfPayload, bool preserveTextElements)
	{
		Invariant.Assert(range.IsTableCellRange, "range is expected to be in IsTableCellRange state");
		List<TextSegment> textSegments = range.TextSegments;
		int num = -1;
		bool ignoreWriteHyperlinkEnd = false;
		List<int> ignoreList = new List<int>();
		for (int i = 0; i < textSegments.Count; i++)
		{
			TextSegment textSegment = textSegments[i];
			if (i > 0)
			{
				ITextPointer textPointer = textSegment.Start.CreatePointer();
				while (!typeof(TableRow).IsAssignableFrom(textPointer.ParentType))
				{
					Invariant.Assert(typeof(TextElement).IsAssignableFrom(textPointer.ParentType), "pointer must be still in a scope of TextElement");
					textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
				}
				Invariant.Assert(typeof(TableRow).IsAssignableFrom(textPointer.ParentType), "pointer must be in a scope of TableRow");
				textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
				ITextRange range2 = new TextRange(textSegment.Start, textSegment.End);
				elementLevel += WriteOpeningTags(range2, textSegment.Start, textPointer, xmlWriter, xamlTypeMapper, wpfPayload == null, out ignoreWriteHyperlinkEnd, ref ignoreList, preserveTextElements);
			}
			WriteXamlTextSegment(xmlWriter, textSegment.Start, textSegment.End, xamlTypeMapper, ref elementLevel, wpfPayload, ignoreWriteHyperlinkEnd, ignoreList, preserveTextElements);
			Invariant.Assert(elementLevel >= 4, "At the minimun we expected to stay within four elements: Section(wrapper),Table,TableRowGroup,TableRow");
			if (num < 0)
			{
				num = elementLevel;
			}
			Invariant.Assert(num == elementLevel, "elementLevel is supposed to be unchanged between segments of table cell range");
			elementLevel--;
			xmlWriter.WriteFullEndElement();
		}
	}

	private static int WriteOpeningTags(ITextRange range, ITextPointer thisElement, ITextPointer scope, XmlWriter xmlWriter, XamlTypeMapper xamlTypeMapper, bool reduceElement, out bool ignoreWriteHyperlinkEnd, ref List<int> ignoreList, bool preserveTextElements)
	{
		ignoreWriteHyperlinkEnd = false;
		if (thisElement.HasEqualScope(scope))
		{
			return 0;
		}
		Invariant.Assert(typeof(TextElement).IsAssignableFrom(thisElement.ParentType), "thisElement is expected to be a TextElement");
		ITextPointer textPointer = thisElement.CreatePointer();
		textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		int num = WriteOpeningTags(range, textPointer, scope, xmlWriter, xamlTypeMapper, reduceElement, out ignoreWriteHyperlinkEnd, ref ignoreList, preserveTextElements);
		bool flag = false;
		bool flag2 = false;
		if (thisElement.ParentType == typeof(Hyperlink))
		{
			if (TextPointerBase.IsAtNonMergeableInlineStart(range.Start))
			{
				ITextPointer textPointer2 = thisElement.CreatePointer();
				textPointer2.MoveToElementEdge(ElementEdge.BeforeStart);
				flag = IsHyperlinkInvalid(textPointer2, range.End);
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			TextElementEditingBehaviorAttribute textElementEditingBehaviorAttribute = (TextElementEditingBehaviorAttribute)Attribute.GetCustomAttribute(thisElement.ParentType, typeof(TextElementEditingBehaviorAttribute));
			if (textElementEditingBehaviorAttribute != null && !textElementEditingBehaviorAttribute.IsTypographicOnly)
			{
				if (TextPointerBase.IsAtNonMergeableInlineStart(range.Start))
				{
					ITextPointer textPointer3 = thisElement.CreatePointer();
					textPointer3.MoveToElementEdge(ElementEdge.BeforeStart);
					flag2 = IsPartialNonTypographic(textPointer3, range.End);
				}
				else
				{
					flag2 = true;
				}
			}
		}
		if (flag)
		{
			ignoreWriteHyperlinkEnd = true;
			return num;
		}
		if (flag2)
		{
			ITextPointer textPointer4 = thisElement.CreatePointer();
			textPointer4.MoveToElementEdge(ElementEdge.BeforeEnd);
			ignoreList.Add(textPointer4.Offset);
			return num;
		}
		WriteStartXamlElement(range, thisElement, xmlWriter, xamlTypeMapper, reduceElement, preserveTextElements);
		return num + 1;
	}

	private static void WriteStartXamlElement(ITextRange range, ITextPointer textReader, XmlWriter xmlWriter, XamlTypeMapper xamlTypeMapper, bool reduceElement, bool preserveTextElements)
	{
		Type parentType = textReader.ParentType;
		Type type = TextSchema.GetStandardElementType(parentType, reduceElement);
		if (type == typeof(InlineUIContainer) || type == typeof(BlockUIContainer))
		{
			Invariant.Assert(!reduceElement);
			InlineUIContainer inlineUIContainer = textReader.GetAdjacentElement(LogicalDirection.Backward) as InlineUIContainer;
			BlockUIContainer blockUIContainer = textReader.GetAdjacentElement(LogicalDirection.Backward) as BlockUIContainer;
			if ((inlineUIContainer == null || !(inlineUIContainer.Child is Image)) && (blockUIContainer == null || !(blockUIContainer.Child is Image)))
			{
				type = TextSchema.GetStandardElementType(parentType, reduceElement: true);
			}
		}
		else if (preserveTextElements)
		{
			type = parentType;
		}
		int num;
		if (preserveTextElements)
		{
			num = ((!TextSchema.IsKnownType(parentType)) ? 1 : 0);
			if (num != 0)
			{
				int num2 = type.Module.Name.LastIndexOf('.');
				string ns = string.Concat(str3: (num2 == -1) ? type.Module.Name : type.Module.Name.Substring(0, num2), str0: "clr-namespace:", str1: type.Namespace, str2: ";assembly=");
				string @namespace = type.Namespace;
				xmlWriter.WriteStartElement(@namespace, type.Name, ns);
				goto IL_0118;
			}
		}
		else
		{
			num = 0;
		}
		xmlWriter.WriteStartElement(type.Name);
		goto IL_0118;
		IL_0118:
		DependencyObject complexProperties = new DependencyObject();
		WriteInheritableProperties(type, textReader, xmlWriter, onlyAffected: true, complexProperties);
		WriteNoninheritableProperties(type, textReader, xmlWriter, onlyAffected: true, complexProperties);
		if (num != 0)
		{
			WriteLocallySetProperties(type, textReader, xmlWriter, complexProperties);
		}
		WriteComplexProperties(xmlWriter, complexProperties, type);
		if (type == typeof(Table) && textReader is TextPointer)
		{
			WriteTableColumnsInformation(range, (Table)((TextPointer)textReader).Parent, xmlWriter, xamlTypeMapper);
		}
	}

	private static void WriteTableColumnsInformation(ITextRange range, Table table, XmlWriter xmlWriter, XamlTypeMapper xamlTypeMapper)
	{
		TableColumnCollection columns = table.Columns;
		if (!TextRangeEditTables.GetColumnRange(range, table, out var firstColumnIndex, out var lastColumnIndex))
		{
			firstColumnIndex = 0;
			lastColumnIndex = columns.Count - 1;
		}
		Invariant.Assert(firstColumnIndex >= 0, "startColumn index is supposed to be non-negative");
		if (columns.Count > 0)
		{
			string localName = table.GetType().Name + ".Columns";
			xmlWriter.WriteStartElement(localName);
			for (int i = firstColumnIndex; i <= lastColumnIndex && i < columns.Count; i++)
			{
				WriteXamlAtomicElement(columns[i], xmlWriter, reduceElement: false);
			}
			xmlWriter.WriteEndElement();
		}
	}

	private static void WriteRootFlowDocument(ITextRange range, ITextPointer context, XmlWriter xmlWriter, XamlTypeMapper xamlTypeMapper, bool lastParagraphMustBeMerged, bool useFlowDocumentAsRoot)
	{
		Type type;
		if (useFlowDocumentAsRoot)
		{
			type = typeof(FlowDocument);
		}
		else
		{
			Type parentType = context.ParentType;
			type = ((!(parentType == null) && !typeof(Paragraph).IsAssignableFrom(parentType) && (!typeof(Inline).IsAssignableFrom(parentType) || typeof(AnchoredBlock).IsAssignableFrom(parentType))) ? typeof(Section) : typeof(Span));
		}
		xmlWriter.WriteStartElement(type.Name, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
		xmlWriter.WriteAttributeString("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
		xmlWriter.WriteAttributeString("xml:space", "preserve");
		DependencyObject complexProperties = new DependencyObject();
		if (useFlowDocumentAsRoot)
		{
			WriteInheritablePropertiesForFlowDocument(((TextPointer)context).Parent, xmlWriter, complexProperties);
		}
		else
		{
			WriteInheritableProperties(type, context, xmlWriter, onlyAffected: false, complexProperties);
		}
		if (type == typeof(Span))
		{
			WriteNoninheritableProperties(typeof(Span), context, xmlWriter, onlyAffected: false, complexProperties);
		}
		if (type == typeof(Section) && lastParagraphMustBeMerged)
		{
			xmlWriter.WriteAttributeString("HasTrailingParagraphBreakOnPaste", "False");
		}
		WriteComplexProperties(xmlWriter, complexProperties, type);
	}

	private static void WriteInheritablePropertiesForFlowDocument(DependencyObject context, XmlWriter xmlWriter, DependencyObject complexProperties)
	{
		DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(typeof(FlowDocument));
		foreach (DependencyProperty dependencyProperty in inheritableProperties)
		{
			object obj = context.ReadLocalValue(dependencyProperty);
			if (obj != DependencyProperty.UnsetValue)
			{
				string stringValue = DPTypeDescriptorContext.GetStringValue(dependencyProperty, obj);
				if (stringValue != null)
				{
					stringValue = FilterNaNStringValueForDoublePropertyType(stringValue, dependencyProperty.PropertyType);
					string localName = ((dependencyProperty != FrameworkContentElement.LanguageProperty) ? ((dependencyProperty.OwnerType == typeof(Typography)) ? ("Typography." + dependencyProperty.Name) : dependencyProperty.Name) : "xml:lang");
					xmlWriter.WriteAttributeString(localName, stringValue);
				}
				else
				{
					complexProperties.SetValue(dependencyProperty, obj);
				}
			}
		}
	}

	private static void WriteInheritableProperties(Type elementTypeStandardized, ITextPointer context, XmlWriter xmlWriter, bool onlyAffected, DependencyObject complexProperties)
	{
		ITextPointer textPointer = null;
		if (onlyAffected)
		{
			textPointer = context.CreatePointer();
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		}
		DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(elementTypeStandardized);
		foreach (DependencyProperty dependencyProperty in inheritableProperties)
		{
			object value = context.GetValue(dependencyProperty);
			if (value == null)
			{
				continue;
			}
			object value2 = null;
			if (onlyAffected)
			{
				value2 = textPointer.GetValue(dependencyProperty);
			}
			if (!onlyAffected || !TextSchema.ValuesAreEqual(value, value2))
			{
				string stringValue = DPTypeDescriptorContext.GetStringValue(dependencyProperty, value);
				if (stringValue != null)
				{
					stringValue = FilterNaNStringValueForDoublePropertyType(stringValue, dependencyProperty.PropertyType);
					string localName = ((dependencyProperty != FrameworkContentElement.LanguageProperty) ? GetPropertyNameForElement(dependencyProperty, elementTypeStandardized, forceComplexName: false) : "xml:lang");
					xmlWriter.WriteAttributeString(localName, stringValue);
				}
				else
				{
					complexProperties.SetValue(dependencyProperty, value);
				}
			}
		}
	}

	private static void WriteNoninheritableProperties(Type elementTypeStandardized, ITextPointer context, XmlWriter xmlWriter, bool onlyAffected, DependencyObject complexProperties)
	{
		DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(elementTypeStandardized);
		ITextPointer textPointer = (onlyAffected ? null : context.CreatePointer());
		foreach (DependencyProperty dependencyProperty in noninheritableProperties)
		{
			Type parentType = context.ParentType;
			object value;
			if (onlyAffected)
			{
				value = context.GetValue(dependencyProperty);
			}
			else
			{
				Invariant.Assert(elementTypeStandardized == typeof(Span), "Request for contextual properties is expected for Span wrapper only");
				value = context.GetValue(dependencyProperty);
				if (value == null || TextDecorationCollection.Empty.ValueEquals(value as TextDecorationCollection))
				{
					if (dependencyProperty == Inline.BaselineAlignmentProperty || dependencyProperty == TextElement.TextEffectsProperty)
					{
						continue;
					}
					textPointer.MoveToPosition(context);
					while ((value == null || TextDecorationCollection.Empty.ValueEquals(value as TextDecorationCollection)) && typeof(Inline).IsAssignableFrom(textPointer.ParentType))
					{
						textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
						value = textPointer.GetValue(dependencyProperty);
						parentType = textPointer.ParentType;
					}
				}
			}
			if (((dependencyProperty != Block.MarginProperty || (!typeof(Paragraph).IsAssignableFrom(parentType) && !typeof(List).IsAssignableFrom(parentType))) && (dependencyProperty != Block.PaddingProperty || !typeof(List).IsAssignableFrom(parentType))) || !Paragraph.IsMarginAuto((Thickness)value))
			{
				WriteNoninheritableProperty(xmlWriter, dependencyProperty, value, parentType, onlyAffected, complexProperties, context.ReadLocalValue(dependencyProperty));
			}
		}
	}

	private static void WriteNoninheritableProperty(XmlWriter xmlWriter, DependencyProperty property, object propertyValue, Type propertyOwnerType, bool onlyAffected, DependencyObject complexProperties, object localValue)
	{
		bool flag = false;
		if (propertyValue != null && propertyValue != DependencyProperty.UnsetValue)
		{
			if (!onlyAffected)
			{
				flag = true;
			}
			else
			{
				PropertyMetadata metadata = property.GetMetadata(propertyOwnerType);
				flag = metadata == null || !TextSchema.ValuesAreEqual(propertyValue, metadata.DefaultValue) || localValue != DependencyProperty.UnsetValue;
			}
		}
		if (flag)
		{
			string stringValue = DPTypeDescriptorContext.GetStringValue(property, propertyValue);
			if (stringValue != null)
			{
				stringValue = FilterNaNStringValueForDoublePropertyType(stringValue, property.PropertyType);
				xmlWriter.WriteAttributeString(property.Name, stringValue);
			}
			else
			{
				complexProperties.SetValue(property, propertyValue);
			}
		}
	}

	private static void WriteLocallySetProperties(Type elementTypeStandardized, ITextPointer context, XmlWriter xmlWriter, DependencyObject complexProperties)
	{
		if (!(context is TextPointer))
		{
			return;
		}
		LocalValueEnumerator localValueEnumerator = context.GetLocalValueEnumerator();
		DependencyProperty[] inheritableProperties = TextSchema.GetInheritableProperties(elementTypeStandardized);
		DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(elementTypeStandardized);
		while (localValueEnumerator.MoveNext())
		{
			DependencyProperty property = localValueEnumerator.Current.Property;
			if (!property.ReadOnly && !IsPropertyKnown(property, inheritableProperties, noninheritableProperties) && !TextSchema.IsKnownType(property.OwnerType))
			{
				object obj = context.ReadLocalValue(property);
				string stringValue = DPTypeDescriptorContext.GetStringValue(property, obj);
				if (stringValue != null)
				{
					stringValue = FilterNaNStringValueForDoublePropertyType(stringValue, property.PropertyType);
					string propertyNameForElement = GetPropertyNameForElement(property, elementTypeStandardized, forceComplexName: false);
					xmlWriter.WriteAttributeString(propertyNameForElement, stringValue);
				}
				else
				{
					complexProperties.SetValue(property, obj);
				}
			}
		}
	}

	private static bool IsPropertyKnown(DependencyProperty propertyToTest, DependencyProperty[] inheritableProperties, DependencyProperty[] nonInheritableProperties)
	{
		for (int i = 0; i < inheritableProperties.Length; i++)
		{
			if (inheritableProperties[i] == propertyToTest)
			{
				return true;
			}
		}
		for (int j = 0; j < nonInheritableProperties.Length; j++)
		{
			if (nonInheritableProperties[j] == propertyToTest)
			{
				return true;
			}
		}
		return false;
	}

	private static void WriteComplexProperties(XmlWriter xmlWriter, DependencyObject complexProperties, Type elementType)
	{
		LocalValueEnumerator localValueEnumerator = complexProperties.GetLocalValueEnumerator();
		localValueEnumerator.Reset();
		while (localValueEnumerator.MoveNext())
		{
			LocalValueEntry current = localValueEnumerator.Current;
			string propertyNameForElement = GetPropertyNameForElement(current.Property, elementType, forceComplexName: true);
			xmlWriter.WriteStartElement(propertyNameForElement);
			string data = XamlWriter.Save(current.Value);
			xmlWriter.WriteRaw(data);
			xmlWriter.WriteEndElement();
		}
	}

	private static string GetPropertyNameForElement(DependencyProperty property, Type elementType, bool forceComplexName)
	{
		if (DependencyProperty.FromName(property.Name, elementType) == property)
		{
			if (forceComplexName)
			{
				return elementType.Name + "." + property.Name;
			}
			return property.Name;
		}
		return property.OwnerType.Name + "." + property.Name;
	}

	private static void WriteXamlAtomicElement(DependencyObject element, XmlWriter xmlWriter, bool reduceElement)
	{
		Type standardElementType = TextSchema.GetStandardElementType(element.GetType(), reduceElement);
		DependencyProperty[] noninheritableProperties = TextSchema.GetNoninheritableProperties(standardElementType);
		xmlWriter.WriteStartElement(standardElementType.Name);
		foreach (DependencyProperty dependencyProperty in noninheritableProperties)
		{
			object obj = element.ReadLocalValue(dependencyProperty);
			if (obj != null && obj != DependencyProperty.UnsetValue)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(dependencyProperty.PropertyType);
				Invariant.Assert(converter != null, "typeConverter==null: is not expected for atomic elements");
				Invariant.Assert(converter.CanConvertTo(typeof(string)), "type is expected to be convertable into string type");
				string text = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, obj, typeof(string));
				Invariant.Assert(text != null, "expecting non-null stringValue");
				xmlWriter.WriteAttributeString(dependencyProperty.Name, text);
			}
		}
		xmlWriter.WriteEndElement();
	}

	private static void WriteEmbeddedObject(object embeddedObject, XmlWriter xmlWriter, WpfPayload wpfPayload)
	{
		if (wpfPayload != null && embeddedObject is Image)
		{
			Image image = (Image)embeddedObject;
			if (image.Source == null || string.IsNullOrEmpty(image.Source.ToString()))
			{
				return;
			}
			string text = wpfPayload.AddImage(image);
			if (text == null)
			{
				return;
			}
			Type typeFromHandle = typeof(Image);
			xmlWriter.WriteStartElement(typeFromHandle.Name);
			DependencyProperty[] imageProperties = TextSchema.ImageProperties;
			DependencyObject complexProperties = new DependencyObject();
			foreach (DependencyProperty dependencyProperty in imageProperties)
			{
				if (dependencyProperty != Image.SourceProperty)
				{
					object value = image.GetValue(dependencyProperty);
					WriteNoninheritableProperty(xmlWriter, dependencyProperty, value, typeFromHandle, onlyAffected: true, complexProperties, image.ReadLocalValue(dependencyProperty));
				}
			}
			xmlWriter.WriteStartElement(typeof(Image).Name + "." + Image.SourceProperty.Name);
			xmlWriter.WriteStartElement(typeof(BitmapImage).Name);
			xmlWriter.WriteAttributeString(BitmapImage.UriSourceProperty.Name, text);
			xmlWriter.WriteAttributeString(BitmapImage.CacheOptionProperty.Name, "OnLoad");
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			WriteComplexProperties(xmlWriter, complexProperties, typeFromHandle);
			xmlWriter.WriteEndElement();
		}
		else
		{
			xmlWriter.WriteString(" ");
		}
	}

	private static bool PasteSingleEmbeddedElement(TextRange range, TextElement fragment)
	{
		if (fragment.ContentStart.GetOffsetToPosition(fragment.ContentEnd) == 3)
		{
			TextElement textElement = fragment.ContentStart.GetAdjacentElement(LogicalDirection.Forward) as TextElement;
			FrameworkElement frameworkElement = null;
			if (textElement is BlockUIContainer)
			{
				frameworkElement = ((BlockUIContainer)textElement).Child as FrameworkElement;
				if (frameworkElement != null)
				{
					((BlockUIContainer)textElement).Child = null;
				}
			}
			else if (textElement is InlineUIContainer)
			{
				frameworkElement = ((InlineUIContainer)textElement).Child as FrameworkElement;
				if (frameworkElement != null)
				{
					((InlineUIContainer)textElement).Child = null;
				}
			}
			if (frameworkElement != null)
			{
				range.InsertEmbeddedUIElement(frameworkElement);
				return true;
			}
		}
		return false;
	}

	private static void PasteTextFragment(TextElement fragment, TextRange range)
	{
		Invariant.Assert(range.IsEmpty, "range must be empty at this point - emptied by a caller");
		Invariant.Assert(fragment is Section || fragment is Span, "The wrapper element must be a Section or Span");
		TextPointer textPointer = TextRangeEditTables.EnsureInsertionPosition(range.End);
		if (textPointer.HasNonMergeableInlineAncestor)
		{
			PasteNonMergeableTextFragment(fragment, range);
		}
		else
		{
			PasteMergeableTextFragment(fragment, range, textPointer);
		}
	}

	private static void PasteNonMergeableTextFragment(TextElement fragment, TextRange range)
	{
		string textInternal = TextRangeBase.GetTextInternal(fragment.ElementStart, fragment.ElementEnd);
		range.Text = textInternal;
		range.Select(range.Start, range.End);
	}

	private static void PasteMergeableTextFragment(TextElement fragment, TextRange range, TextPointer insertionPosition)
	{
		TextPointer elementStart;
		TextPointer textPointer;
		if (fragment is Span)
		{
			insertionPosition = TextRangeEdit.SplitFormattingElements(insertionPosition, keepEmptyFormatting: false);
			Invariant.Assert(insertionPosition.Parent is Paragraph, "insertionPosition must be in a scope of a Paragraph after splitting formatting elements");
			fragment.RepositionWithContent(insertionPosition);
			elementStart = fragment.ElementStart;
			textPointer = fragment.ElementEnd;
			fragment.Reposition(null, null);
			ValidateMergingPositions(typeof(Inline), elementStart, textPointer);
			ApplyContextualProperties(elementStart, textPointer, fragment);
		}
		else
		{
			CorrectLeadingNestedLists((Section)fragment);
			bool num = SplitParagraphForPasting(ref insertionPosition);
			fragment.RepositionWithContent(insertionPosition);
			elementStart = fragment.ElementStart;
			textPointer = fragment.ElementEnd.GetPositionAtOffset(0, LogicalDirection.Forward);
			fragment.Reposition(null, null);
			ValidateMergingPositions(typeof(Block), elementStart, textPointer);
			ApplyContextualProperties(elementStart, textPointer, fragment);
			if (num)
			{
				MergeParagraphsAtPosition(elementStart, mergingOnFragmentStart: true);
			}
			if (!((Section)fragment).HasTrailingParagraphBreakOnPaste)
			{
				MergeParagraphsAtPosition(textPointer, mergingOnFragmentStart: false);
			}
		}
		if (fragment is Section && ((Section)fragment).HasTrailingParagraphBreakOnPaste)
		{
			textPointer = textPointer.GetInsertionPosition(LogicalDirection.Forward);
		}
		range.Select(elementStart, textPointer);
	}

	private static void CorrectLeadingNestedLists(Section fragment)
	{
		for (List list = fragment.Blocks.FirstBlock as List; list != null; list = list2)
		{
			ListItem firstListItem = list.ListItems.FirstListItem;
			if (firstListItem == null || firstListItem.NextListItem != null || !(firstListItem.Blocks.FirstBlock is List list2))
			{
				break;
			}
			firstListItem.Reposition(null, null);
			list.Reposition(null, null);
		}
	}

	private static bool SplitParagraphForPasting(ref TextPointer insertionPosition)
	{
		bool flag = true;
		TextPointer textPointer = insertionPosition;
		while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && TextSchema.IsFormattingType(textPointer.Parent.GetType()))
		{
			textPointer = textPointer.GetNextContextPosition(LogicalDirection.Backward);
		}
		while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && TextSchema.AllowsParagraphMerging(textPointer.Parent.GetType()))
		{
			flag = false;
			textPointer = textPointer.GetNextContextPosition(LogicalDirection.Backward);
		}
		if (!flag)
		{
			insertionPosition = textPointer;
		}
		else
		{
			insertionPosition = TextRangeEdit.InsertParagraphBreak(insertionPosition, moveIntoSecondParagraph: false);
		}
		if (insertionPosition.Parent is List)
		{
			insertionPosition = TextRangeEdit.SplitElement(insertionPosition);
		}
		return flag;
	}

	private static void MergeParagraphsAtPosition(TextPointer position, bool mergingOnFragmentStart)
	{
		TextPointer textPointer = position;
		while (textPointer != null && !(textPointer.Parent is Paragraph))
		{
			textPointer = ((textPointer.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementEnd) ? null : textPointer.GetNextContextPosition(LogicalDirection.Backward));
		}
		if (textPointer == null)
		{
			return;
		}
		Invariant.Assert(textPointer.Parent is Paragraph, "We suppose have a first paragraph found");
		Paragraph paragraph = (Paragraph)textPointer.Parent;
		textPointer = position;
		while (textPointer != null && !(textPointer.Parent is Paragraph))
		{
			textPointer = ((textPointer.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart) ? null : textPointer.GetNextContextPosition(LogicalDirection.Forward));
		}
		if (textPointer != null)
		{
			Invariant.Assert(textPointer.Parent is Paragraph, "We suppose a second paragraph found");
			Paragraph paragraph2 = (Paragraph)textPointer.Parent;
			if (TextRangeEditLists.ParagraphsAreMergeable(paragraph, paragraph2))
			{
				TextRangeEditLists.MergeParagraphs(paragraph, paragraph2);
			}
			else if (mergingOnFragmentStart && paragraph.TextRange.IsEmpty)
			{
				paragraph.RepositionWithContent(null);
			}
			else if (!mergingOnFragmentStart && paragraph2.TextRange.IsEmpty)
			{
				paragraph2.RepositionWithContent(null);
			}
		}
	}

	private static void ValidateMergingPositions(Type itemType, TextPointer start, TextPointer end)
	{
		if (start.CompareTo(end) < 0)
		{
			TextPointerContext pointerContext = start.GetPointerContext(LogicalDirection.Forward);
			TextPointerContext pointerContext2 = end.GetPointerContext(LogicalDirection.Backward);
			Invariant.Assert(pointerContext == TextPointerContext.ElementStart, "Expecting first opening tag of pasted fragment");
			Invariant.Assert(pointerContext2 == TextPointerContext.ElementEnd, "Expecting last closing tag of pasted fragment");
			Invariant.Assert(itemType.IsAssignableFrom(start.GetAdjacentElement(LogicalDirection.Forward).GetType()), "The first pasted fragment item is expected to be a " + itemType.Name);
			Invariant.Assert(itemType.IsAssignableFrom(end.GetAdjacentElement(LogicalDirection.Backward).GetType()), "The last pasted fragment item is expected to be a " + itemType.Name);
			TextPointerContext pointerContext3 = start.GetPointerContext(LogicalDirection.Backward);
			TextPointerContext pointerContext4 = end.GetPointerContext(LogicalDirection.Forward);
			Invariant.Assert(pointerContext3 == TextPointerContext.ElementStart || pointerContext3 == TextPointerContext.ElementEnd || pointerContext3 == TextPointerContext.None, "Bad context preceding a pasted fragment");
			Invariant.Assert(pointerContext3 != TextPointerContext.ElementEnd || itemType.IsAssignableFrom(start.GetAdjacentElement(LogicalDirection.Backward).GetType()), "An element preceding a pasted fragment is expected to be a " + itemType.Name);
			Invariant.Assert(pointerContext4 == TextPointerContext.ElementStart || pointerContext4 == TextPointerContext.ElementEnd || pointerContext4 == TextPointerContext.None, "Bad context following a pasted fragment");
			Invariant.Assert(pointerContext4 != TextPointerContext.ElementStart || itemType.IsAssignableFrom(end.GetAdjacentElement(LogicalDirection.Forward).GetType()), "An element following a pasted fragment is expected to be a " + itemType.Name);
		}
	}

	private static void AdjustFragmentForTargetRange(TextElement fragment, TextRange range)
	{
		if (fragment is Section && ((Section)fragment).HasTrailingParagraphBreakOnPaste)
		{
			((Section)fragment).HasTrailingParagraphBreakOnPaste = range.End.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.None;
		}
	}

	private static void ApplyContextualProperties(TextPointer start, TextPointer end, TextElement propertyBag)
	{
		Invariant.Assert(propertyBag.IsEmpty && propertyBag.Parent == null, "propertyBag is supposed to be an empty element outside any tree");
		LocalValueEnumerator localValueEnumerator = propertyBag.GetLocalValueEnumerator();
		while (start.CompareTo(end) < 0 && localValueEnumerator.MoveNext())
		{
			LocalValueEntry current = localValueEnumerator.Current;
			DependencyProperty property = current.Property;
			if (TextSchema.IsCharacterProperty(property) && TextSchema.IsParagraphProperty(property))
			{
				if (TextSchema.IsBlock(propertyBag.GetType()))
				{
					ApplyContextualProperty(typeof(Block), start, end, property, current.Value);
				}
				else
				{
					ApplyContextualProperty(typeof(Inline), start, end, property, current.Value);
				}
			}
			else if (TextSchema.IsCharacterProperty(property))
			{
				ApplyContextualProperty(typeof(Inline), start, end, property, current.Value);
			}
			else if (TextSchema.IsParagraphProperty(property))
			{
				ApplyContextualProperty(typeof(Block), start, end, property, current.Value);
			}
		}
		TextRangeEdit.MergeFormattingInlines(start);
		TextRangeEdit.MergeFormattingInlines(end);
	}

	private static void ApplyContextualProperty(Type targetType, TextPointer start, TextPointer end, DependencyProperty property, object value)
	{
		if (TextSchema.ValuesAreEqual(start.Parent.GetValue(property), value))
		{
			return;
		}
		start = start.GetNextContextPosition(LogicalDirection.Forward);
		while (start != null && start.CompareTo(end) < 0)
		{
			TextPointerContext pointerContext = start.GetPointerContext(LogicalDirection.Backward);
			if (pointerContext == TextPointerContext.ElementStart)
			{
				TextElement textElement = (TextElement)start.Parent;
				if (textElement.ReadLocalValue(property) != DependencyProperty.UnsetValue || !TextSchema.ValuesAreEqual(textElement.GetValue(property), textElement.Parent.GetValue(property)))
				{
					start = textElement.ElementEnd;
				}
				else if (targetType.IsAssignableFrom(textElement.GetType()))
				{
					start = textElement.ElementEnd;
					if (targetType == typeof(Block) && start.CompareTo(end) > 0)
					{
						break;
					}
					if (!TextSchema.ValuesAreEqual(value, textElement.GetValue(property)))
					{
						textElement.ClearValue(property);
						if (!TextSchema.ValuesAreEqual(value, textElement.GetValue(property)))
						{
							textElement.SetValue(property, value);
						}
						TextRangeEdit.MergeFormattingInlines(textElement.ElementStart);
					}
				}
				else
				{
					start = start.GetNextContextPosition(LogicalDirection.Forward);
				}
			}
			else
			{
				Invariant.Assert(pointerContext != TextPointerContext.None, "TextPointerContext.None is not expected");
				start = start.GetNextContextPosition(LogicalDirection.Forward);
			}
		}
	}

	private static ITextPointer FindSerializationCommonAncestor(ITextRange range)
	{
		ITextPointer textPointer = range.Start.CreatePointer();
		ITextPointer textPointer2 = range.End.CreatePointer();
		while (!textPointer.HasEqualScope(textPointer2))
		{
			textPointer2.MoveToPosition(range.End);
			while (typeof(TextElement).IsAssignableFrom(textPointer2.ParentType) && !textPointer2.HasEqualScope(textPointer))
			{
				textPointer2.MoveToElementEdge(ElementEdge.AfterEnd);
			}
			if (textPointer2.HasEqualScope(textPointer))
			{
				break;
			}
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		}
		while (!IsAcceptableAncestor(textPointer, range))
		{
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		}
		if (typeof(TextElement).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.AfterStart);
			ITextPointer hyperlinkStart = GetHyperlinkStart(range);
			if (hyperlinkStart != null)
			{
				textPointer = hyperlinkStart;
			}
		}
		else
		{
			textPointer.MoveToPosition(textPointer.TextContainer.Start);
		}
		return textPointer;
	}

	private static bool IsAcceptableAncestor(ITextPointer commonAncestor, ITextRange range)
	{
		if (typeof(TableRow).IsAssignableFrom(commonAncestor.ParentType) || typeof(TableRowGroup).IsAssignableFrom(commonAncestor.ParentType) || typeof(Table).IsAssignableFrom(commonAncestor.ParentType) || typeof(BlockUIContainer).IsAssignableFrom(commonAncestor.ParentType) || typeof(List).IsAssignableFrom(commonAncestor.ParentType) || (typeof(Inline).IsAssignableFrom(commonAncestor.ParentType) && TextSchema.HasTextDecorations(commonAncestor.GetValue(Inline.TextDecorationsProperty))))
		{
			return false;
		}
		ITextPointer textPointer = commonAncestor.CreatePointer();
		while (typeof(TextElement).IsAssignableFrom(textPointer.ParentType))
		{
			TextElementEditingBehaviorAttribute textElementEditingBehaviorAttribute = (TextElementEditingBehaviorAttribute)Attribute.GetCustomAttribute(textPointer.ParentType, typeof(TextElementEditingBehaviorAttribute));
			if (textElementEditingBehaviorAttribute != null && !textElementEditingBehaviorAttribute.IsTypographicOnly)
			{
				return false;
			}
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
		}
		return true;
	}

	private static int StripInvalidSurrogateChars(char[] text, int length)
	{
		Invariant.Assert(text.Length >= length, "Asserting that text.Length >= length");
		int i;
		for (i = 0; i < length; i++)
		{
			char c = text[i];
			if (char.IsHighSurrogate(c) || char.IsLowSurrogate(c) || IsBadCode(c))
			{
				break;
			}
		}
		int num;
		if (i == length)
		{
			num = length;
		}
		else
		{
			num = i;
			for (; i < length; i++)
			{
				if (char.IsHighSurrogate(text[i]))
				{
					if (i + 1 < length && char.IsLowSurrogate(text[i + 1]))
					{
						text[num] = text[i];
						text[num + 1] = text[i + 1];
						num += 2;
						i++;
					}
				}
				else if (!char.IsLowSurrogate(text[i]) && !IsBadCode(text[i]))
				{
					text[num] = text[i];
					num++;
				}
			}
		}
		return num;
	}

	private static bool IsBadCode(char code)
	{
		if (code < ' ' && code != '\t' && code != '\n')
		{
			return code != '\r';
		}
		return false;
	}

	private static bool IsPartialNonTypographic(ITextPointer textReader, ITextPointer rangeEnd)
	{
		bool result = false;
		Invariant.Assert(textReader.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart);
		textReader.CreatePointer();
		ITextPointer textPointer = textReader.CreatePointer();
		textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
		textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
		if (textPointer.CompareTo(rangeEnd) > 0)
		{
			result = true;
		}
		return result;
	}

	private static bool IsHyperlinkInvalid(ITextPointer textReader, ITextPointer rangeEnd)
	{
		Invariant.Assert(textReader.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart);
		Invariant.Assert(typeof(Hyperlink).IsAssignableFrom(textReader.GetElementType(LogicalDirection.Forward)));
		bool result = false;
		_ = (Hyperlink)textReader.GetAdjacentElement(LogicalDirection.Forward);
		ITextPointer textPointer = textReader.CreatePointer();
		ITextPointer textPointer2 = textReader.CreatePointer();
		textPointer2.MoveToNextContextPosition(LogicalDirection.Forward);
		textPointer2.MoveToElementEdge(ElementEdge.AfterEnd);
		if (textPointer2.CompareTo(rangeEnd) > 0)
		{
			result = true;
		}
		else
		{
			while (textPointer.CompareTo(textPointer2) < 0)
			{
				if (textPointer.GetAdjacentElement(LogicalDirection.Forward) is InlineUIContainer inlineUIContainer && !(inlineUIContainer.Child is Image))
				{
					result = true;
					break;
				}
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			}
		}
		return result;
	}

	private static ITextPointer GetHyperlinkStart(ITextRange range)
	{
		ITextPointer textPointer = null;
		if (TextPointerBase.IsAtNonMergeableInlineStart(range.Start) && TextPointerBase.IsAtNonMergeableInlineEnd(range.End))
		{
			textPointer = range.Start.CreatePointer(LogicalDirection.Forward);
			while (textPointer.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && !typeof(Hyperlink).IsAssignableFrom(textPointer.ParentType))
			{
				textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
			}
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
			textPointer.Freeze();
		}
		return textPointer;
	}

	private static string FilterNaNStringValueForDoublePropertyType(string stringValue, Type propertyType)
	{
		if (propertyType == typeof(double) && string.Compare(stringValue, "NaN", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return "Auto";
		}
		return stringValue;
	}
}
