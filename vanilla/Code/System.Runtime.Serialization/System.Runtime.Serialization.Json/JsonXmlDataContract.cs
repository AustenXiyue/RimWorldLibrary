using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json;

internal class JsonXmlDataContract : JsonDataContract
{
	public JsonXmlDataContract(XmlDataContract traditionalXmlDataContract)
		: base(traditionalXmlDataContract)
	{
	}

	public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
	{
		string s = jsonReader.ReadElementContentAsString();
		DataContractSerializer dataContractSerializer = new DataContractSerializer(base.TraditionalDataContract.UnderlyingType, GetKnownTypesFromContext(context, context?.SerializerKnownTypeList), 1, ignoreExtensionDataObject: false, preserveObjectReferences: false, null);
		MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(s));
		XmlDictionaryReaderQuotas readerQuotas = ((JsonReaderDelegator)jsonReader).ReaderQuotas;
		object obj = ((readerQuotas != null) ? dataContractSerializer.ReadObject(XmlDictionaryReader.CreateTextReader(stream, readerQuotas)) : dataContractSerializer.ReadObject(stream));
		context?.AddNewObject(obj);
		return obj;
	}

	public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
	{
		DataContractSerializer dataContractSerializer = new DataContractSerializer(Type.GetTypeFromHandle(declaredTypeHandle), GetKnownTypesFromContext(context, context?.SerializerKnownTypeList), 1, ignoreExtensionDataObject: false, preserveObjectReferences: false, null);
		MemoryStream memoryStream = new MemoryStream();
		dataContractSerializer.WriteObject(memoryStream, obj);
		memoryStream.Position = 0L;
		string value = new StreamReader(memoryStream).ReadToEnd();
		jsonWriter.WriteString(value);
	}

	private List<Type> GetKnownTypesFromContext(XmlObjectSerializerContext context, IList<Type> serializerKnownTypeList)
	{
		List<Type> list = new List<Type>();
		if (context != null)
		{
			List<XmlQualifiedName> list2 = new List<XmlQualifiedName>();
			Dictionary<XmlQualifiedName, DataContract>[] dataContractDictionaries = context.scopedKnownTypes.dataContractDictionaries;
			if (dataContractDictionaries != null)
			{
				foreach (Dictionary<XmlQualifiedName, DataContract> dictionary in dataContractDictionaries)
				{
					if (dictionary == null)
					{
						continue;
					}
					foreach (KeyValuePair<XmlQualifiedName, DataContract> item in dictionary)
					{
						if (!list2.Contains(item.Key))
						{
							list2.Add(item.Key);
							list.Add(item.Value.UnderlyingType);
						}
					}
				}
			}
			if (serializerKnownTypeList != null)
			{
				list.AddRange(serializerKnownTypeList);
			}
		}
		return list;
	}
}
