using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Configuration;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Security.Permissions;
using System.Xml;

namespace System.Runtime.Serialization;

/// <summary>Serializes and deserializes an instance of a type into XML stream or document using the supplied .NET Framework types. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class NetDataContractSerializer : XmlObjectSerializer, IFormatter
{
	private XmlDictionaryString rootName;

	private XmlDictionaryString rootNamespace;

	private StreamingContext context;

	private SerializationBinder binder;

	private ISurrogateSelector surrogateSelector;

	private int maxItemsInObjectGraph;

	private bool ignoreExtensionDataObject;

	private FormatterAssemblyStyle assemblyFormat;

	private DataContract cachedDataContract;

	private static Hashtable typeNameCache = new Hashtable();

	private static bool? unsafeTypeForwardingEnabled;

	internal static bool UnsafeTypeForwardingEnabled
	{
		[SecuritySafeCritical]
		get
		{
			if (!unsafeTypeForwardingEnabled.HasValue)
			{
				if (NetDataContractSerializerSection.TryUnsafeGetSection(out var section))
				{
					unsafeTypeForwardingEnabled = section.EnableUnsafeTypeForwarding;
				}
				else
				{
					unsafeTypeForwardingEnabled = false;
				}
			}
			return unsafeTypeForwardingEnabled.Value;
		}
	}

	/// <summary>Gets or sets the object that enables the passing of context data that is useful while serializing or deserializing.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the context data.</returns>
	/// <filterpriority>2</filterpriority>
	public StreamingContext Context
	{
		get
		{
			return context;
		}
		set
		{
			context = value;
		}
	}

	/// <summary>Gets or sets an object that controls class loading.</summary>
	/// <returns>The <see cref="T:System.Runtime.Serialization.SerializationBinder" /> used with the current formatter.</returns>
	/// <filterpriority>2</filterpriority>
	public SerializationBinder Binder
	{
		get
		{
			return binder;
		}
		set
		{
			binder = value;
		}
	}

	/// <summary>Gets or sets an object that assists the formatter when selecting a surrogate for serialization. </summary>
	/// <returns>An <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> for selecting a surrogate.</returns>
	/// <filterpriority>2</filterpriority>
	public ISurrogateSelector SurrogateSelector
	{
		get
		{
			return surrogateSelector;
		}
		set
		{
			surrogateSelector = value;
		}
	}

	/// <summary>Gets a value that specifies a method for locating and loading assemblies.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle" /> enumeration value that specifies a method for locating and loading assemblies.</returns>
	/// <exception cref="T:System.ArgumentException">The value being set does not correspond to any of the <see cref="T:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle" /> values. </exception>
	/// <filterpriority>2</filterpriority>
	public FormatterAssemblyStyle AssemblyFormat
	{
		get
		{
			return assemblyFormat;
		}
		set
		{
			if (value != FormatterAssemblyStyle.Full && value != 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("'{0}': invalid assembly format.", value)));
			}
			assemblyFormat = value;
		}
	}

	/// <summary>Gets the maximum number of items allowed in the object to be serialized.</summary>
	/// <returns>The maximum number of items allowed in the object. The default is <see cref="F:System.Int32.MaxValue" />.</returns>
	/// <filterpriority>2</filterpriority>
	public int MaxItemsInObjectGraph => maxItemsInObjectGraph;

	/// <summary>Gets a value that specifies whether data supplied by an extension of the object is ignored.</summary>
	/// <returns>true to ignore the data supplied by an extension of the type; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IgnoreExtensionDataObject => ignoreExtensionDataObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class.  </summary>
	public NetDataContractSerializer()
		: this(new StreamingContext(StreamingContextStates.All))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class with the supplied streaming context data. </summary>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains context data.</param>
	public NetDataContractSerializer(StreamingContext context)
		: this(context, int.MaxValue, ignoreExtensionDataObject: false, FormatterAssemblyStyle.Full, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class with the supplied context data; in addition, specifies the maximum number of items in the object to be serialized, and parameters to specify whether extra data is ignored, the assembly loading method, and a surrogate selector.</summary>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains context data.</param>
	/// <param name="maxItemsInObjectGraph">The maximum number of items in the graph to serialize or deserialize. </param>
	/// <param name="ignoreExtensionDataObject">true to ignore the data supplied by an extension of the type; otherwise, false.</param>
	/// <param name="assemblyFormat">A <see cref="T:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle" /> enumeration value that specifies a method for locating and loading assemblies.</param>
	/// <param name="surrogateSelector">An implementation of the <see cref="T:System.Runtime.Serialization.ISurrogateSelector" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxItemsInObjectGraph" /> value is less than 0.</exception>
	public NetDataContractSerializer(StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
	{
		Initialize(context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class with the supplied XML root element and namespace.</summary>
	/// <param name="rootName">The name of the XML element that encloses the content to serialize or deserialize.</param>
	/// <param name="rootNamespace">The namespace of the XML element that encloses the content to serialize or deserialize.</param>
	public NetDataContractSerializer(string rootName, string rootNamespace)
		: this(rootName, rootNamespace, new StreamingContext(StreamingContextStates.All), int.MaxValue, ignoreExtensionDataObject: false, FormatterAssemblyStyle.Full, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class with the supplied context data and root name and namespace; in addition, specifies the maximum number of items in the object to be serialized, and parameters to specify whether extra data is ignored, the assembly loading method, and a surrogate selector.</summary>
	/// <param name="rootName">The name of the XML element that encloses the content to serialize or deserialize.</param>
	/// <param name="rootNamespace">The namespace of the XML element that encloses the content to serialize or deserialize.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains context data.</param>
	/// <param name="maxItemsInObjectGraph">The maximum number of items in the graph to serialize or deserialize. </param>
	/// <param name="ignoreExtensionDataObject">true to ignore the data supplied by an extension of the type; otherwise, false.</param>
	/// <param name="assemblyFormat">A <see cref="T:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle" /> enumeration value that specifies a method for locating and loading assemblies.</param>
	/// <param name="surrogateSelector">An implementation of the <see cref="T:System.Runtime.Serialization.IDataContractSurrogate" /> to handle the legacy type.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxItemsInObjectGraph" /> value is less than 0.</exception>
	public NetDataContractSerializer(string rootName, string rootNamespace, StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
	{
		XmlDictionary xmlDictionary = new XmlDictionary(2);
		Initialize(xmlDictionary.Add(rootName), xmlDictionary.Add(DataContract.GetNamespace(rootNamespace)), context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class with two parameters of type <see cref="T:System.Xml.XmlDictionaryString" /> that contain the root element and namespace used to specify the content.</summary>
	/// <param name="rootName">An <see cref="T:System.Xml.XmlDictionaryString" /> that contains the name of the XML element that encloses the content to serialize or deserialize.</param>
	/// <param name="rootNamespace">An <see cref="T:System.Xml.XmlDictionaryString" /> that contains the namespace of the XML element that encloses the content to serialize or deserialize.</param>
	public NetDataContractSerializer(XmlDictionaryString rootName, XmlDictionaryString rootNamespace)
		: this(rootName, rootNamespace, new StreamingContext(StreamingContextStates.All), int.MaxValue, ignoreExtensionDataObject: false, FormatterAssemblyStyle.Full, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.NetDataContractSerializer" /> class with the supplied context data, and root name and namespace (as <see cref="T:System.Xml.XmlDictionaryString" />  parameters); in addition, specifies the maximum number of items in the object to be serialized, and parameters to specify whether extra data found is ignored, assembly loading method, and a surrogate selector.</summary>
	/// <param name="rootName">An <see cref="T:System.Xml.XmlDictionaryString" /> that contains the root element of the content.</param>
	/// <param name="rootNamespace">An <see cref="T:System.Xml.XmlDictionaryString" /> that contains the namespace of the root element.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains context data.</param>
	/// <param name="maxItemsInObjectGraph">The maximum number of items in the graph to serialize or deserialize. </param>
	/// <param name="ignoreExtensionDataObject">true to ignore the data supplied by an extension of the type; otherwise, false.</param>
	/// <param name="assemblyFormat">A <see cref="T:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle" /> enumeration value that specifies a method for locating and loading assemblies.</param>
	/// <param name="surrogateSelector">An implementation of the <see cref="T:System.Runtime.Serialization.IDataContractSurrogate" /> to handle the legacy type.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxItemsInObjectGraph" /> value is less than 0.</exception>
	public NetDataContractSerializer(XmlDictionaryString rootName, XmlDictionaryString rootNamespace, StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
	{
		Initialize(rootName, rootNamespace, context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
	}

	private void Initialize(StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
	{
		this.context = context;
		if (maxItemsInObjectGraph < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxItemsInObjectGraph", SR.GetString("The value of this argument must be non-negative.")));
		}
		this.maxItemsInObjectGraph = maxItemsInObjectGraph;
		this.ignoreExtensionDataObject = ignoreExtensionDataObject;
		this.surrogateSelector = surrogateSelector;
		AssemblyFormat = assemblyFormat;
	}

	private void Initialize(XmlDictionaryString rootName, XmlDictionaryString rootNamespace, StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
	{
		Initialize(context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
		this.rootName = rootName;
		this.rootNamespace = rootNamespace;
	}

	/// <summary>Serializes the specified object graph using the specified writer.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to serialize with.</param>
	/// <param name="graph">The object to serialize. All child objects of this root object are automatically serialized.</param>
	public void Serialize(Stream stream, object graph)
	{
		base.WriteObject(stream, graph);
	}

	/// <summary>Deserializes an XML document or stream into an object.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the XML to deserialize.</param>
	public object Deserialize(Stream stream)
	{
		return base.ReadObject(stream);
	}

	internal override void InternalWriteObject(XmlWriterDelegator writer, object graph)
	{
		Hashtable surrogateDataContracts = null;
		DataContract dataContract = GetDataContract(graph, ref surrogateDataContracts);
		InternalWriteStartObject(writer, graph, dataContract);
		InternalWriteObjectContent(writer, graph, dataContract, surrogateDataContracts);
		InternalWriteEndObject(writer);
	}

	/// <summary>Writes the complete content (start, content, and end) of the object to the XML document or stream with the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> used to write the XML document or stream.</param>
	/// <param name="graph">The object containing the content to write.</param>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute has not been applied to the type.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">there is a problem with the instance being serialized.</exception>
	/// <exception cref="T:System.ServiceModel.QuotaExceededException">the maximum number of object to serialize has been exceeded. Check the <see cref="P:System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph" /> property.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteObject(XmlWriter writer, object graph)
	{
		WriteObjectHandleExceptions(new XmlWriterDelegator(writer), graph);
	}

	/// <summary>Writes the opening XML element using an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlDictionaryWriter" /> used to write the XML element.</param>
	/// <param name="graph">The object to serialize. All child objects of this root object are automatically serialized.</param>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute has not been applied to the type.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">there is a problem with the instance being serialized.</exception>
	/// <exception cref="T:System.ServiceModel.QuotaExceededException">the maximum number of object to serialize has been exceeded. Check the <see cref="P:System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph" /> property.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteStartObject(XmlWriter writer, object graph)
	{
		WriteStartObjectHandleExceptions(new XmlWriterDelegator(writer), graph);
	}

	/// <summary>Writes the XML content using an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> used to write the XML content.</param>
	/// <param name="graph">The object to serialize. All child objects of this root object are automatically serialized.</param>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute has not been applied to the type.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">there is a problem with the instance being serialized.</exception>
	/// <exception cref="T:System.ServiceModel.QuotaExceededException">the maximum number of object to serialize has been exceeded. Check the <see cref="P:System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph" /> property.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteObjectContent(XmlWriter writer, object graph)
	{
		WriteObjectContentHandleExceptions(new XmlWriterDelegator(writer), graph);
	}

	/// <summary>Writes the closing XML element using an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> used to write the XML document or stream.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="writer" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteEndObject(XmlWriter writer)
	{
		WriteEndObjectHandleExceptions(new XmlWriterDelegator(writer));
	}

	/// <summary>Writes the opening XML element using an <see cref="T:System.Xml.XmlDictionaryWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlDictionaryWriter" /> used to write the XML element.</param>
	/// <param name="graph">The object to serialize. All child objects of this root object are automatically serialized.</param>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute has not been applied to the type.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">there is a problem with the instance being serialized.</exception>
	/// <exception cref="T:System.ServiceModel.QuotaExceededException">the maximum number of object to serialize has been exceeded. Check the <see cref="P:System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph" /> property.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
	{
		WriteStartObjectHandleExceptions(new XmlWriterDelegator(writer), graph);
	}

	internal override void InternalWriteStartObject(XmlWriterDelegator writer, object graph)
	{
		Hashtable surrogateDataContracts = null;
		DataContract dataContract = GetDataContract(graph, ref surrogateDataContracts);
		InternalWriteStartObject(writer, graph, dataContract);
	}

	private void InternalWriteStartObject(XmlWriterDelegator writer, object graph, DataContract contract)
	{
		WriteRootElement(writer, contract, rootName, rootNamespace, CheckIfNeedsContractNsAtRoot(rootName, rootNamespace, contract));
	}

	/// <summary>Writes the XML content using an <see cref="T:System.Xml.XmlDictionaryWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlDictionaryWriter" /> used to write the XML content.</param>
	/// <param name="graph">The object to serialize. All child objects of this root object are automatically serialized.</param>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">the type being serialized does not conform to data contract rules. For example, the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute has not been applied to the type.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">there is a problem with the instance being serialized.</exception>
	/// <exception cref="T:System.ServiceModel.QuotaExceededException">the maximum number of object to serialize has been exceeded. Check the <see cref="P:System.Runtime.Serialization.DataContractSerializer.MaxItemsInObjectGraph" /> property.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
	{
		WriteObjectContentHandleExceptions(new XmlWriterDelegator(writer), graph);
	}

	internal override void InternalWriteObjectContent(XmlWriterDelegator writer, object graph)
	{
		Hashtable surrogateDataContracts = null;
		DataContract dataContract = GetDataContract(graph, ref surrogateDataContracts);
		InternalWriteObjectContent(writer, graph, dataContract, surrogateDataContracts);
	}

	private void InternalWriteObjectContent(XmlWriterDelegator writer, object graph, DataContract contract, Hashtable surrogateDataContracts)
	{
		if (MaxItemsInObjectGraph == 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString("Maximum number of items that can be serialized or deserialized in an object graph is '{0}'.", MaxItemsInObjectGraph)));
		}
		if (IsRootXmlAny(rootName, contract))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString("For type '{0}', IsAny is not supported by NetDataContractSerializer.", contract.UnderlyingType)));
		}
		if (graph == null)
		{
			XmlObjectSerializer.WriteNull(writer);
			return;
		}
		Type type = graph.GetType();
		if (contract.UnderlyingType != type)
		{
			contract = GetDataContract(graph, ref surrogateDataContracts);
		}
		XmlObjectSerializerWriteContext xmlObjectSerializerWriteContext = null;
		if (contract.CanContainReferences)
		{
			xmlObjectSerializerWriteContext = XmlObjectSerializerWriteContext.CreateContext(this, surrogateDataContracts);
			xmlObjectSerializerWriteContext.HandleGraphAtTopLevel(writer, graph, contract);
		}
		WriteClrTypeInfo(writer, contract, binder);
		contract.WriteXmlValue(writer, graph, xmlObjectSerializerWriteContext);
	}

	internal static void WriteClrTypeInfo(XmlWriterDelegator writer, DataContract dataContract, SerializationBinder binder)
	{
		if (dataContract.IsISerializable || dataContract is SurrogateDataContract)
		{
			return;
		}
		TypeInformation typeInformation = null;
		Type originalUnderlyingType = dataContract.OriginalUnderlyingType;
		string typeName = null;
		string assemblyName = null;
		binder?.BindToName(originalUnderlyingType, out assemblyName, out typeName);
		if (typeName == null)
		{
			typeInformation = GetTypeInformation(originalUnderlyingType);
			typeName = typeInformation.FullTypeName;
		}
		if (assemblyName == null)
		{
			assemblyName = ((typeInformation == null) ? GetTypeInformation(originalUnderlyingType).AssemblyString : typeInformation.AssemblyString);
			if (!UnsafeTypeForwardingEnabled && !originalUnderlyingType.Assembly.IsFullyTrusted && !IsAssemblyNameForwardingSafe(originalUnderlyingType.Assembly.FullName, assemblyName))
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString("Type '{0}' in assembly '{1}' cannot be forwarded from assembly '{2}'.", DataContract.GetClrTypeFullName(originalUnderlyingType), originalUnderlyingType.Assembly.FullName, assemblyName)));
			}
		}
		WriteClrTypeInfo(writer, typeName, assemblyName);
	}

	internal static void WriteClrTypeInfo(XmlWriterDelegator writer, Type dataContractType, SerializationBinder binder, string defaultClrTypeName, string defaultClrAssemblyName)
	{
		string typeName = null;
		string assemblyName = null;
		binder?.BindToName(dataContractType, out assemblyName, out typeName);
		if (typeName == null)
		{
			typeName = defaultClrTypeName;
		}
		if (assemblyName == null)
		{
			assemblyName = defaultClrAssemblyName;
		}
		WriteClrTypeInfo(writer, typeName, assemblyName);
	}

	internal static void WriteClrTypeInfo(XmlWriterDelegator writer, Type dataContractType, SerializationBinder binder, SerializationInfo serInfo)
	{
		TypeInformation typeInformation = null;
		string typeName = null;
		string assemblyName = null;
		binder?.BindToName(dataContractType, out assemblyName, out typeName);
		if (typeName == null)
		{
			if (serInfo.IsFullTypeNameSetExplicit)
			{
				typeName = serInfo.FullTypeName;
			}
			else
			{
				typeInformation = GetTypeInformation(serInfo.ObjectType);
				typeName = typeInformation.FullTypeName;
			}
		}
		if (assemblyName == null)
		{
			assemblyName = ((!serInfo.IsAssemblyNameSetExplicit) ? ((typeInformation == null) ? GetTypeInformation(serInfo.ObjectType).AssemblyString : typeInformation.AssemblyString) : serInfo.AssemblyName);
		}
		WriteClrTypeInfo(writer, typeName, assemblyName);
	}

	private static void WriteClrTypeInfo(XmlWriterDelegator writer, string clrTypeName, string clrAssemblyName)
	{
		if (clrTypeName != null)
		{
			writer.WriteAttributeString("z", DictionaryGlobals.ClrTypeLocalName, DictionaryGlobals.SerializationNamespace, DataContract.GetClrTypeString(clrTypeName));
		}
		if (clrAssemblyName != null)
		{
			writer.WriteAttributeString("z", DictionaryGlobals.ClrAssemblyLocalName, DictionaryGlobals.SerializationNamespace, DataContract.GetClrTypeString(clrAssemblyName));
		}
	}

	/// <summary>Writes the closing XML element using an <see cref="T:System.Xml.XmlDictionaryWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlDictionaryWriter" /> used to write the XML document or stream.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="writer" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override void WriteEndObject(XmlDictionaryWriter writer)
	{
		WriteEndObjectHandleExceptions(new XmlWriterDelegator(writer));
	}

	internal override void InternalWriteEndObject(XmlWriterDelegator writer)
	{
		writer.WriteEndElement();
	}

	/// <summary>Reads the XML stream or document with an <see cref="T:System.Xml.XmlDictionaryReader" /> and returns the deserialized object.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> used to read the XML stream or document.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="reader" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override object ReadObject(XmlReader reader)
	{
		return ReadObjectHandleExceptions(new XmlReaderDelegator(reader), verifyObjectName: true);
	}

	/// <summary>Reads the XML stream or document with an <see cref="T:System.Xml.XmlDictionaryReader" /> and returns the deserialized object; also checks whether the object data conforms to the name and namespace used to create the serializer.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> used to read the XML stream or document.</param>
	/// <param name="verifyObjectName">true to check whether the enclosing XML element name and namespace correspond to the root name and root namespace used to construct the serializer; false to skip the verification.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="reader" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override object ReadObject(XmlReader reader, bool verifyObjectName)
	{
		return ReadObjectHandleExceptions(new XmlReaderDelegator(reader), verifyObjectName);
	}

	/// <summary>Determines whether the <see cref="T:System.Xml.XmlReader" /> is positioned on an object that can be deserialized using the specified reader.</summary>
	/// <returns>true if the reader is at the start element of the stream to read; otherwise, false.</returns>
	/// <param name="reader">An <see cref="T:System.Xml.XmlReader" /> that contains the XML to read.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="reader" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override bool IsStartObject(XmlReader reader)
	{
		return IsStartObjectHandleExceptions(new XmlReaderDelegator(reader));
	}

	/// <summary>Reads the XML stream or document with an <see cref="T:System.Xml.XmlDictionaryReader" /> and returns the deserialized object; also checks whether the object data conforms to the name and namespace used to create the serializer.</summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="reader">The <see cref="T:System.Xml.XmlDictionaryReader" /> used to read the XML stream or document.</param>
	/// <param name="verifyObjectName">true to check whether the enclosing XML element name and namespace correspond to the root name and root namespace used to construct the serializer; false to skip the verification.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="reader" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
	{
		return ReadObjectHandleExceptions(new XmlReaderDelegator(reader), verifyObjectName);
	}

	/// <summary>Determines whether the <see cref="T:System.Xml.XmlDictionaryReader" /> is positioned on an object that can be deserialized using the specified reader.</summary>
	/// <returns>true, if the reader is at the start element of the stream to read; otherwise, false.</returns>
	/// <param name="reader">An <see cref="T:System.Xml.XmlDictionaryReader" /> that contains the XML to read.</param>
	/// <exception cref="T:System.ArgumentNullException">the <paramref name="reader" /> is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	public override bool IsStartObject(XmlDictionaryReader reader)
	{
		return IsStartObjectHandleExceptions(new XmlReaderDelegator(reader));
	}

	internal override object InternalReadObject(XmlReaderDelegator xmlReader, bool verifyObjectName)
	{
		if (MaxItemsInObjectGraph == 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString("Maximum number of items that can be serialized or deserialized in an object graph is '{0}'.", MaxItemsInObjectGraph)));
		}
		if (!IsStartElement(xmlReader))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(SR.GetString("Expecting state '{0}' when ReadObject is called.", XmlNodeType.Element), xmlReader));
		}
		return XmlObjectSerializerReadContext.CreateContext(this).InternalDeserialize(xmlReader, null, null, null);
	}

	internal override bool InternalIsStartObject(XmlReaderDelegator reader)
	{
		return IsStartElement(reader);
	}

	internal DataContract GetDataContract(object obj, ref Hashtable surrogateDataContracts)
	{
		return GetDataContract((obj == null) ? Globals.TypeOfObject : obj.GetType(), ref surrogateDataContracts);
	}

	internal DataContract GetDataContract(Type type, ref Hashtable surrogateDataContracts)
	{
		return GetDataContract(type.TypeHandle, type, ref surrogateDataContracts);
	}

	internal DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type, ref Hashtable surrogateDataContracts)
	{
		DataContract dataContractFromSurrogateSelector = GetDataContractFromSurrogateSelector(surrogateSelector, Context, typeHandle, type, ref surrogateDataContracts);
		if (dataContractFromSurrogateSelector != null)
		{
			return dataContractFromSurrogateSelector;
		}
		if (cachedDataContract == null)
		{
			return cachedDataContract = DataContract.GetDataContract(typeHandle, type, SerializationMode.SharedType);
		}
		DataContract dataContract = cachedDataContract;
		if (dataContract.UnderlyingType.TypeHandle.Equals(typeHandle))
		{
			return dataContract;
		}
		return DataContract.GetDataContract(typeHandle, type, SerializationMode.SharedType);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	private static ISerializationSurrogate GetSurrogate(Type type, ISurrogateSelector surrogateSelector, StreamingContext context)
	{
		ISurrogateSelector selector;
		return surrogateSelector.GetSurrogate(type, context, out selector);
	}

	internal static DataContract GetDataContractFromSurrogateSelector(ISurrogateSelector surrogateSelector, StreamingContext context, RuntimeTypeHandle typeHandle, Type type, ref Hashtable surrogateDataContracts)
	{
		if (surrogateSelector == null)
		{
			return null;
		}
		if (type == null)
		{
			type = Type.GetTypeFromHandle(typeHandle);
		}
		DataContract builtInDataContract = DataContract.GetBuiltInDataContract(type);
		if (builtInDataContract != null)
		{
			return builtInDataContract;
		}
		if (surrogateDataContracts != null)
		{
			DataContract dataContract = (DataContract)surrogateDataContracts[type];
			if (dataContract != null)
			{
				return dataContract;
			}
		}
		DataContract dataContract2 = null;
		ISerializationSurrogate surrogate = GetSurrogate(type, surrogateSelector, context);
		if (surrogate != null)
		{
			dataContract2 = new SurrogateDataContract(type, surrogate);
		}
		else if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			DataContract dataContract3 = GetDataContractFromSurrogateSelector(surrogateSelector, context, elementType.TypeHandle, elementType, ref surrogateDataContracts);
			if (dataContract3 == null)
			{
				dataContract3 = DataContract.GetDataContract(elementType.TypeHandle, elementType, SerializationMode.SharedType);
			}
			dataContract2 = new CollectionDataContract(type, dataContract3);
		}
		if (dataContract2 != null)
		{
			if (surrogateDataContracts == null)
			{
				surrogateDataContracts = new Hashtable();
			}
			surrogateDataContracts.Add(type, dataContract2);
			return dataContract2;
		}
		return null;
	}

	internal static TypeInformation GetTypeInformation(Type type)
	{
		TypeInformation typeInformation = null;
		object obj = typeNameCache[type];
		if (obj == null)
		{
			bool hasTypeForwardedFrom;
			string clrAssemblyName = DataContract.GetClrAssemblyName(type, out hasTypeForwardedFrom);
			typeInformation = new TypeInformation(DataContract.GetClrTypeFullNameUsingTypeForwardedFromAttribute(type), clrAssemblyName, hasTypeForwardedFrom);
			lock (typeNameCache)
			{
				typeNameCache[type] = typeInformation;
			}
		}
		else
		{
			typeInformation = (TypeInformation)obj;
		}
		return typeInformation;
	}

	private static bool IsAssemblyNameForwardingSafe(string originalAssemblyName, string newAssemblyName)
	{
		if (originalAssemblyName == newAssemblyName)
		{
			return true;
		}
		AssemblyName assemblyName = new AssemblyName(originalAssemblyName);
		AssemblyName assemblyName2 = new AssemblyName(newAssemblyName);
		if (string.Equals(assemblyName2.Name, "mscorlib", StringComparison.OrdinalIgnoreCase) || string.Equals(assemblyName2.Name, "mscorlib.dll", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return IsPublicKeyTokenForwardingSafe(assemblyName.GetPublicKeyToken(), assemblyName2.GetPublicKeyToken());
	}

	private static bool IsPublicKeyTokenForwardingSafe(byte[] sourceToken, byte[] destinationToken)
	{
		if (sourceToken == null || destinationToken == null || sourceToken.Length == 0 || destinationToken.Length == 0 || sourceToken.Length != destinationToken.Length)
		{
			return false;
		}
		for (int i = 0; i < sourceToken.Length; i++)
		{
			if (sourceToken[i] != destinationToken[i])
			{
				return false;
			}
		}
		return true;
	}
}
