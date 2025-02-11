using System.Globalization;

namespace System.Windows.Markup;

internal class XamlStyleSerializer : XamlSerializer
{
	internal const string StyleTagName = "Style";

	internal const string TargetTypePropertyName = "TargetType";

	internal const string BasedOnPropertyName = "BasedOn";

	internal const string VisualTriggersPropertyName = "Triggers";

	internal const string ResourcesPropertyName = "Resources";

	internal const string SettersPropertyName = "Setters";

	internal const string VisualTriggersFullPropertyName = "Style.Triggers";

	internal const string SettersFullPropertyName = "Style.Setters";

	internal const string ResourcesFullPropertyName = "Style.Resources";

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
		Type result = Style.DefaultTargetType;
		bool flag = false;
		object obj = null;
		int num = 0;
		BamlRecord bamlRecord = startRecord;
		short ownerTypeId = 0;
		while (bamlRecord != null)
		{
			if (bamlRecord.RecordType == BamlRecordType.ElementStart)
			{
				BamlElementStartRecord bamlElementStartRecord = bamlRecord as BamlElementStartRecord;
				if (++num == 1)
				{
					ownerTypeId = bamlElementStartRecord.TypeId;
				}
				else if (num == 2)
				{
					result = parserContext.MapTable.GetTypeFromId(bamlElementStartRecord.TypeId);
					flag = true;
					break;
				}
			}
			else if (bamlRecord.RecordType == BamlRecordType.Property && num == 1)
			{
				BamlPropertyRecord bamlPropertyRecord = bamlRecord as BamlPropertyRecord;
				if (parserContext.MapTable.DoesAttributeMatch(bamlPropertyRecord.AttributeId, ownerTypeId, "TargetType"))
				{
					obj = parserContext.XamlTypeMapper.GetDictionaryKey(bamlPropertyRecord.Value, parserContext);
				}
			}
			else if (bamlRecord.RecordType == BamlRecordType.PropertyComplexStart || bamlRecord.RecordType == BamlRecordType.PropertyIListStart)
			{
				break;
			}
			bamlRecord = bamlRecord.Next;
		}
		if (obj == null)
		{
			if (!flag)
			{
				ThrowException("StyleNoDictionaryKey", parserContext.LineNumber, parserContext.LinePosition);
			}
			return result;
		}
		return obj;
	}

	private void ThrowException(string id, int lineNumber, int linePosition)
	{
		string resourceString = SR.GetResourceString(id);
		XamlParseException ex;
		if (lineNumber > 0)
		{
			resourceString += " ";
			resourceString += SR.Format(SR.ParserLineAndOffset, lineNumber.ToString(CultureInfo.CurrentCulture), linePosition.ToString(CultureInfo.CurrentCulture));
			ex = new XamlParseException(resourceString, lineNumber, linePosition);
		}
		else
		{
			ex = new XamlParseException(resourceString);
		}
		throw ex;
	}
}
