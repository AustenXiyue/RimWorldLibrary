using System.Globalization;

namespace System.Windows.Markup;

internal class XamlTemplateSerializer : XamlSerializer
{
	internal const string ControlTemplateTagName = "ControlTemplate";

	internal const string DataTemplateTagName = "DataTemplate";

	internal const string HierarchicalDataTemplateTagName = "HierarchicalDataTemplate";

	internal const string ItemsPanelTemplateTagName = "ItemsPanelTemplate";

	internal const string TargetTypePropertyName = "TargetType";

	internal const string DataTypePropertyName = "DataType";

	internal const string TriggersPropertyName = "Triggers";

	internal const string ResourcesPropertyName = "Resources";

	internal const string SettersPropertyName = "Setters";

	internal const string ItemsSourcePropertyName = "ItemsSource";

	internal const string ItemTemplatePropertyName = "ItemTemplate";

	internal const string ItemTemplateSelectorPropertyName = "ItemTemplateSelector";

	internal const string ItemContainerStylePropertyName = "ItemContainerStyle";

	internal const string ItemContainerStyleSelectorPropertyName = "ItemContainerStyleSelector";

	internal const string ItemStringFormatPropertyName = "ItemStringFormat";

	internal const string ItemBindingGroupPropertyName = "ItemBindingGroup";

	internal const string AlternationCountPropertyName = "AlternationCount";

	internal const string ControlTemplateTriggersFullPropertyName = "ControlTemplate.Triggers";

	internal const string ControlTemplateResourcesFullPropertyName = "ControlTemplate.Resources";

	internal const string DataTemplateTriggersFullPropertyName = "DataTemplate.Triggers";

	internal const string DataTemplateResourcesFullPropertyName = "DataTemplate.Resources";

	internal const string HierarchicalDataTemplateTriggersFullPropertyName = "HierarchicalDataTemplate.Triggers";

	internal const string HierarchicalDataTemplateItemsSourceFullPropertyName = "HierarchicalDataTemplate.ItemsSource";

	internal const string HierarchicalDataTemplateItemTemplateFullPropertyName = "HierarchicalDataTemplate.ItemTemplate";

	internal const string HierarchicalDataTemplateItemTemplateSelectorFullPropertyName = "HierarchicalDataTemplate.ItemTemplateSelector";

	internal const string HierarchicalDataTemplateItemContainerStyleFullPropertyName = "HierarchicalDataTemplate.ItemContainerStyle";

	internal const string HierarchicalDataTemplateItemContainerStyleSelectorFullPropertyName = "HierarchicalDataTemplate.ItemContainerStyleSelector";

	internal const string HierarchicalDataTemplateItemStringFormatFullPropertyName = "HierarchicalDataTemplate.ItemStringFormat";

	internal const string HierarchicalDataTemplateItemBindingGroupFullPropertyName = "HierarchicalDataTemplate.ItemBindingGroup";

	internal const string HierarchicalDataTemplateAlternationCountFullPropertyName = "HierarchicalDataTemplate.AlternationCount";

	internal const string PropertyTriggerPropertyName = "Property";

	internal const string PropertyTriggerValuePropertyName = "Value";

	internal const string PropertyTriggerSourceName = "SourceName";

	internal const string PropertyTriggerEnterActions = "EnterActions";

	internal const string PropertyTriggerExitActions = "ExitActions";

	internal const string DataTriggerBindingPropertyName = "Binding";

	internal const string EventTriggerEventName = "RoutedEvent";

	internal const string EventTriggerSourceName = "SourceName";

	internal const string EventTriggerActions = "Actions";

	internal const string MultiPropertyTriggerConditionsPropertyName = "Conditions";

	internal const string SetterTagName = "Setter";

	internal const string SetterPropertyAttributeName = "Property";

	internal const string SetterValueAttributeName = "Value";

	internal const string SetterTargetAttributeName = "TargetName";

	internal const string SetterEventAttributeName = "Event";

	internal const string SetterHandlerAttributeName = "Handler";

	internal override object GetDictionaryKey(BamlRecord startRecord, ParserContext parserContext)
	{
		object obj = null;
		int num = 0;
		BamlRecord bamlRecord = startRecord;
		short num2 = 0;
		while (bamlRecord != null)
		{
			if (bamlRecord.RecordType == BamlRecordType.ElementStart)
			{
				BamlElementStartRecord bamlElementStartRecord = bamlRecord as BamlElementStartRecord;
				if (++num != 1)
				{
					break;
				}
				num2 = bamlElementStartRecord.TypeId;
			}
			else if (bamlRecord.RecordType == BamlRecordType.Property && num == 1)
			{
				BamlPropertyRecord bamlPropertyRecord = bamlRecord as BamlPropertyRecord;
				parserContext.MapTable.GetAttributeInfoFromId(bamlPropertyRecord.AttributeId, out var ownerTypeId, out var name, out var _);
				if (ownerTypeId == num2)
				{
					if (name == "TargetType")
					{
						obj = parserContext.XamlTypeMapper.GetDictionaryKey(bamlPropertyRecord.Value, parserContext);
					}
					else if (name == "DataType")
					{
						object dictionaryKey = parserContext.XamlTypeMapper.GetDictionaryKey(bamlPropertyRecord.Value, parserContext);
						Exception ex = TemplateKey.ValidateDataType(dictionaryKey, null);
						if (ex != null)
						{
							ThrowException("TemplateBadDictionaryKey", parserContext.LineNumber, parserContext.LinePosition, ex);
						}
						obj = new DataTemplateKey(dictionaryKey);
					}
				}
			}
			else if (bamlRecord.RecordType == BamlRecordType.PropertyComplexStart || bamlRecord.RecordType == BamlRecordType.PropertyIListStart || bamlRecord.RecordType == BamlRecordType.ElementEnd)
			{
				break;
			}
			bamlRecord = bamlRecord.Next;
		}
		if (obj == null)
		{
			ThrowException("StyleNoDictionaryKey", parserContext.LineNumber, parserContext.LinePosition, null);
		}
		return obj;
	}

	private void ThrowException(string id, int lineNumber, int linePosition, Exception innerException)
	{
		string resourceString = SR.GetResourceString(id);
		XamlParseException ex;
		if (lineNumber > 0)
		{
			resourceString += " ";
			resourceString += SR.Format(SR.ParserLineAndOffset, lineNumber.ToString(CultureInfo.CurrentUICulture), linePosition.ToString(CultureInfo.CurrentUICulture));
			ex = new XamlParseException(resourceString, lineNumber, linePosition);
		}
		else
		{
			ex = new XamlParseException(resourceString);
		}
		throw ex;
	}
}
