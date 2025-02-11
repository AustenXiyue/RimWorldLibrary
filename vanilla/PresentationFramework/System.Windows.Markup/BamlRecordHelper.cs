namespace System.Windows.Markup;

internal static class BamlRecordHelper
{
	internal static bool IsMapTableRecordType(BamlRecordType bamlRecordType)
	{
		if (bamlRecordType - 27 <= BamlRecordType.Property)
		{
			return true;
		}
		return false;
	}

	internal static bool IsDebugBamlRecordType(BamlRecordType recordType)
	{
		if (recordType == BamlRecordType.LineNumberAndPosition || recordType == BamlRecordType.LinePosition)
		{
			return true;
		}
		return false;
	}

	internal static bool HasDebugExtensionRecord(bool isDebugBamlStream, BamlRecord bamlRecord)
	{
		if (isDebugBamlStream && bamlRecord.Next != null && IsDebugBamlRecordType(bamlRecord.Next.RecordType))
		{
			return true;
		}
		return false;
	}

	internal static bool DoesRecordTypeHaveDebugExtension(BamlRecordType recordType)
	{
		switch (recordType)
		{
		case BamlRecordType.ElementStart:
		case BamlRecordType.ElementEnd:
		case BamlRecordType.Property:
		case BamlRecordType.PropertyComplexStart:
		case BamlRecordType.PropertyArrayStart:
		case BamlRecordType.PropertyIListStart:
		case BamlRecordType.PropertyIDictionaryStart:
		case BamlRecordType.XmlnsProperty:
		case BamlRecordType.PIMapping:
		case BamlRecordType.PropertyTypeReference:
		case BamlRecordType.PropertyWithExtension:
		case BamlRecordType.PropertyWithConverter:
		case BamlRecordType.KeyElementStart:
		case BamlRecordType.ConnectionId:
		case BamlRecordType.ContentProperty:
		case BamlRecordType.StaticResourceStart:
		case BamlRecordType.PresentationOptionsAttribute:
			return true;
		case BamlRecordType.DocumentStart:
		case BamlRecordType.DocumentEnd:
		case BamlRecordType.PropertyCustom:
		case BamlRecordType.PropertyComplexEnd:
		case BamlRecordType.PropertyArrayEnd:
		case BamlRecordType.PropertyIListEnd:
		case BamlRecordType.PropertyIDictionaryEnd:
		case BamlRecordType.LiteralContent:
		case BamlRecordType.Text:
		case BamlRecordType.TextWithConverter:
		case BamlRecordType.RoutedEvent:
		case BamlRecordType.ClrEvent:
		case BamlRecordType.XmlAttribute:
		case BamlRecordType.ProcessingInstruction:
		case BamlRecordType.Comment:
		case BamlRecordType.DefTag:
		case BamlRecordType.DefAttribute:
		case BamlRecordType.EndAttributes:
		case BamlRecordType.AssemblyInfo:
		case BamlRecordType.TypeInfo:
		case BamlRecordType.TypeSerializerInfo:
		case BamlRecordType.AttributeInfo:
		case BamlRecordType.StringInfo:
		case BamlRecordType.PropertyStringReference:
		case BamlRecordType.DeferableContentStart:
		case BamlRecordType.DefAttributeKeyString:
		case BamlRecordType.DefAttributeKeyType:
		case BamlRecordType.KeyElementEnd:
		case BamlRecordType.ConstructorParametersStart:
		case BamlRecordType.ConstructorParametersEnd:
		case BamlRecordType.ConstructorParameterType:
		case BamlRecordType.NamedElementStart:
		case BamlRecordType.StaticResourceEnd:
		case BamlRecordType.StaticResourceId:
		case BamlRecordType.TextWithId:
		case BamlRecordType.LineNumberAndPosition:
		case BamlRecordType.LinePosition:
		case BamlRecordType.OptimizedStaticResource:
		case BamlRecordType.PropertyWithStaticResourceId:
			return false;
		default:
			return false;
		}
	}
}
