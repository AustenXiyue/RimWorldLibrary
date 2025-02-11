using System.Collections;
using System.IO;

namespace System.Windows.Markup;

internal class BamlRecordManager
{
	private BamlRecord[] _readCache = new BamlRecord[57];

	private BamlRecord[] _writeCache;

	internal BamlRecord ReadNextRecord(BinaryReader bamlBinaryReader, long bytesAvailable, BamlRecordType recordType)
	{
		BamlRecord bamlRecord;
		switch (recordType)
		{
		case BamlRecordType.AssemblyInfo:
			bamlRecord = new BamlAssemblyInfoRecord();
			break;
		case BamlRecordType.TypeInfo:
			bamlRecord = new BamlTypeInfoRecord();
			break;
		case BamlRecordType.TypeSerializerInfo:
			bamlRecord = new BamlTypeInfoWithSerializerRecord();
			break;
		case BamlRecordType.AttributeInfo:
			bamlRecord = new BamlAttributeInfoRecord();
			break;
		case BamlRecordType.StringInfo:
			bamlRecord = new BamlStringInfoRecord();
			break;
		case BamlRecordType.DefAttributeKeyString:
			bamlRecord = new BamlDefAttributeKeyStringRecord();
			break;
		case BamlRecordType.DefAttributeKeyType:
			bamlRecord = new BamlDefAttributeKeyTypeRecord();
			break;
		case BamlRecordType.KeyElementStart:
			bamlRecord = new BamlKeyElementStartRecord();
			break;
		default:
			bamlRecord = _readCache[(uint)recordType];
			if (bamlRecord == null || bamlRecord.IsPinned)
			{
				bamlRecord = (_readCache[(uint)recordType] = AllocateRecord(recordType));
			}
			break;
		}
		bamlRecord.Next = null;
		if (bamlRecord != null)
		{
			if (bamlRecord.LoadRecordSize(bamlBinaryReader, bytesAvailable) && bytesAvailable >= bamlRecord.RecordSize)
			{
				bamlRecord.LoadRecordData(bamlBinaryReader);
			}
			else
			{
				bamlRecord = null;
			}
		}
		return bamlRecord;
	}

	internal static IAddChild AsIAddChild(object obj)
	{
		return obj as IAddChildInternal;
	}

	internal static bool TreatAsIAddChild(Type parentObjectType)
	{
		return KnownTypes.Types[275].IsAssignableFrom(parentObjectType);
	}

	internal static BamlRecordType GetPropertyStartRecordType(Type propertyType, bool propertyCanWrite)
	{
		if (propertyType.IsArray)
		{
			return BamlRecordType.PropertyArrayStart;
		}
		if (typeof(IDictionary).IsAssignableFrom(propertyType))
		{
			return BamlRecordType.PropertyIDictionaryStart;
		}
		if (typeof(IList).IsAssignableFrom(propertyType) || TreatAsIAddChild(propertyType) || (typeof(IEnumerable).IsAssignableFrom(propertyType) && !propertyCanWrite))
		{
			return BamlRecordType.PropertyIListStart;
		}
		return BamlRecordType.PropertyComplexStart;
	}

	internal BamlRecord CloneRecord(BamlRecord record)
	{
		BamlRecord bamlRecord = record.RecordType switch
		{
			BamlRecordType.ElementStart => (!(record is BamlNamedElementStartRecord)) ? new BamlElementStartRecord() : new BamlNamedElementStartRecord(), 
			BamlRecordType.PropertyCustom => (!(record is BamlPropertyCustomWriteInfoRecord)) ? new BamlPropertyCustomRecord() : new BamlPropertyCustomWriteInfoRecord(), 
			_ => AllocateRecord(record.RecordType), 
		};
		record.Copy(bamlRecord);
		return bamlRecord;
	}

	private BamlRecord AllocateWriteRecord(BamlRecordType recordType)
	{
		if (recordType == BamlRecordType.PropertyCustom)
		{
			return new BamlPropertyCustomWriteInfoRecord();
		}
		return AllocateRecord(recordType);
	}

	private BamlRecord AllocateRecord(BamlRecordType recordType)
	{
		switch (recordType)
		{
		case BamlRecordType.DocumentStart:
			return new BamlDocumentStartRecord();
		case BamlRecordType.DocumentEnd:
			return new BamlDocumentEndRecord();
		case BamlRecordType.ConnectionId:
			return new BamlConnectionIdRecord();
		case BamlRecordType.ElementStart:
			return new BamlElementStartRecord();
		case BamlRecordType.ElementEnd:
			return new BamlElementEndRecord();
		case BamlRecordType.DeferableContentStart:
			return new BamlDeferableContentStartRecord();
		case BamlRecordType.DefAttributeKeyString:
			return new BamlDefAttributeKeyStringRecord();
		case BamlRecordType.DefAttributeKeyType:
			return new BamlDefAttributeKeyTypeRecord();
		case BamlRecordType.LiteralContent:
			return new BamlLiteralContentRecord();
		case BamlRecordType.Property:
			return new BamlPropertyRecord();
		case BamlRecordType.PropertyWithConverter:
			return new BamlPropertyWithConverterRecord();
		case BamlRecordType.PropertyStringReference:
			return new BamlPropertyStringReferenceRecord();
		case BamlRecordType.PropertyTypeReference:
			return new BamlPropertyTypeReferenceRecord();
		case BamlRecordType.PropertyWithExtension:
			return new BamlPropertyWithExtensionRecord();
		case BamlRecordType.PropertyCustom:
			return new BamlPropertyCustomRecord();
		case BamlRecordType.PropertyComplexStart:
			return new BamlPropertyComplexStartRecord();
		case BamlRecordType.PropertyComplexEnd:
			return new BamlPropertyComplexEndRecord();
		case BamlRecordType.RoutedEvent:
			return new BamlRoutedEventRecord();
		case BamlRecordType.PropertyArrayStart:
			return new BamlPropertyArrayStartRecord();
		case BamlRecordType.PropertyArrayEnd:
			return new BamlPropertyArrayEndRecord();
		case BamlRecordType.PropertyIListStart:
			return new BamlPropertyIListStartRecord();
		case BamlRecordType.PropertyIListEnd:
			return new BamlPropertyIListEndRecord();
		case BamlRecordType.PropertyIDictionaryStart:
			return new BamlPropertyIDictionaryStartRecord();
		case BamlRecordType.PropertyIDictionaryEnd:
			return new BamlPropertyIDictionaryEndRecord();
		case BamlRecordType.Text:
			return new BamlTextRecord();
		case BamlRecordType.TextWithConverter:
			return new BamlTextWithConverterRecord();
		case BamlRecordType.TextWithId:
			return new BamlTextWithIdRecord();
		case BamlRecordType.XmlnsProperty:
			return new BamlXmlnsPropertyRecord();
		case BamlRecordType.PIMapping:
			return new BamlPIMappingRecord();
		case BamlRecordType.DefAttribute:
			return new BamlDefAttributeRecord();
		case BamlRecordType.PresentationOptionsAttribute:
			return new BamlPresentationOptionsAttributeRecord();
		case BamlRecordType.KeyElementStart:
			return new BamlKeyElementStartRecord();
		case BamlRecordType.KeyElementEnd:
			return new BamlKeyElementEndRecord();
		case BamlRecordType.ConstructorParametersStart:
			return new BamlConstructorParametersStartRecord();
		case BamlRecordType.ConstructorParametersEnd:
			return new BamlConstructorParametersEndRecord();
		case BamlRecordType.ConstructorParameterType:
			return new BamlConstructorParameterTypeRecord();
		case BamlRecordType.ContentProperty:
			return new BamlContentPropertyRecord();
		case BamlRecordType.AssemblyInfo:
		case BamlRecordType.TypeInfo:
		case BamlRecordType.TypeSerializerInfo:
		case BamlRecordType.AttributeInfo:
		case BamlRecordType.StringInfo:
			return null;
		case BamlRecordType.StaticResourceStart:
			return new BamlStaticResourceStartRecord();
		case BamlRecordType.StaticResourceEnd:
			return new BamlStaticResourceEndRecord();
		case BamlRecordType.StaticResourceId:
			return new BamlStaticResourceIdRecord();
		case BamlRecordType.LineNumberAndPosition:
			return new BamlLineAndPositionRecord();
		case BamlRecordType.LinePosition:
			return new BamlLinePositionRecord();
		case BamlRecordType.OptimizedStaticResource:
			return new BamlOptimizedStaticResourceRecord();
		case BamlRecordType.PropertyWithStaticResourceId:
			return new BamlPropertyWithStaticResourceIdRecord();
		default:
			return null;
		}
	}

	internal BamlRecord GetWriteRecord(BamlRecordType recordType)
	{
		if (_writeCache == null)
		{
			_writeCache = new BamlRecord[57];
		}
		BamlRecord bamlRecord = _writeCache[(uint)recordType];
		if (bamlRecord == null)
		{
			bamlRecord = AllocateWriteRecord(recordType);
		}
		else
		{
			_writeCache[(uint)recordType] = null;
		}
		bamlRecord.RecordSize = -1;
		return bamlRecord;
	}

	internal void ReleaseWriteRecord(BamlRecord record)
	{
		if (!record.IsPinned)
		{
			if (_writeCache[(uint)record.RecordType] != null)
			{
				throw new InvalidOperationException(SR.ParserMultiBamls);
			}
			_writeCache[(uint)record.RecordType] = record;
		}
	}
}
