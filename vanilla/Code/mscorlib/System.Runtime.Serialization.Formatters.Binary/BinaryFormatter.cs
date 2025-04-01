using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security;

namespace System.Runtime.Serialization.Formatters.Binary;

/// <summary>Serializes and deserializes an object, or an entire graph of connected objects, in binary format.</summary>
[ComVisible(true)]
public sealed class BinaryFormatter : IRemotingFormatter, IFormatter
{
	internal ISurrogateSelector m_surrogates;

	internal StreamingContext m_context;

	internal SerializationBinder m_binder;

	internal FormatterTypeStyle m_typeFormat = FormatterTypeStyle.TypesAlways;

	internal FormatterAssemblyStyle m_assemblyFormat;

	internal TypeFilterLevel m_securityLevel = TypeFilterLevel.Full;

	internal object[] m_crossAppDomainArray;

	private static Dictionary<Type, TypeInformation> typeNameCache = new Dictionary<Type, TypeInformation>();

	/// <summary>Gets or sets the format in which type descriptions are laid out in the serialized stream.</summary>
	/// <returns>The style of type layouts to use.</returns>
	public FormatterTypeStyle TypeFormat
	{
		get
		{
			return m_typeFormat;
		}
		set
		{
			m_typeFormat = value;
		}
	}

	/// <summary>Gets or sets the behavior of the deserializer with regards to finding and loading assemblies.</summary>
	/// <returns>One of the <see cref="T:System.Runtime.Serialization.Formatters.FormatterAssemblyStyle" /> values that specifies the deserializer behavior.</returns>
	public FormatterAssemblyStyle AssemblyFormat
	{
		get
		{
			return m_assemblyFormat;
		}
		set
		{
			m_assemblyFormat = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Runtime.Serialization.Formatters.TypeFilterLevel" /> of automatic deserialization the <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter" /> performs.</summary>
	/// <returns>The <see cref="T:System.Runtime.Serialization.Formatters.TypeFilterLevel" /> that represents the current automatic deserialization level.</returns>
	public TypeFilterLevel FilterLevel
	{
		get
		{
			return m_securityLevel;
		}
		set
		{
			m_securityLevel = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> that controls type substitution during serialization and deserialization.</summary>
	/// <returns>The surrogate selector to use with this formatter.</returns>
	public ISurrogateSelector SurrogateSelector
	{
		get
		{
			return m_surrogates;
		}
		set
		{
			m_surrogates = value;
		}
	}

	/// <summary>Gets or sets an object of type <see cref="T:System.Runtime.Serialization.SerializationBinder" /> that controls the binding of a serialized object to a type.</summary>
	/// <returns>The serialization binder to use with this formatter.</returns>
	public SerializationBinder Binder
	{
		get
		{
			return m_binder;
		}
		set
		{
			m_binder = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Runtime.Serialization.StreamingContext" /> for this formatter.</summary>
	/// <returns>The streaming context to use with this formatter.</returns>
	public StreamingContext Context
	{
		get
		{
			return m_context;
		}
		set
		{
			m_context = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter" /> class with default values.</summary>
	public BinaryFormatter()
	{
		m_surrogates = null;
		m_context = new StreamingContext(StreamingContextStates.All);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter" /> class with a given surrogate selector and streaming context.</summary>
	/// <param name="selector">The <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> to use. Can be null. </param>
	/// <param name="context">The source and destination for the serialized data. </param>
	public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
	{
		m_surrogates = selector;
		m_context = context;
	}

	/// <summary>Deserializes the specified stream into an object graph.</summary>
	/// <returns>The top (root) of the object graph.</returns>
	/// <param name="serializationStream">The stream from which to deserialize the object graph. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <paramref name="serializationStream" /> supports seeking, but its length is 0. -or-The target type is a <see cref="T:System.Decimal" />, but the value is out of range of the <see cref="T:System.Decimal" /> type.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public object Deserialize(Stream serializationStream)
	{
		return Deserialize(serializationStream, null);
	}

	[SecurityCritical]
	internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck)
	{
		return Deserialize(serializationStream, handler, fCheck, null);
	}

	/// <summary>Deserializes the specified stream into an object graph. The provided <see cref="T:System.Runtime.Remoting.Messaging.HeaderHandler" /> handles any headers in that stream.</summary>
	/// <returns>The deserialized object or the top object (root) of the object graph.</returns>
	/// <param name="serializationStream">The stream from which to deserialize the object graph. </param>
	/// <param name="handler">The <see cref="T:System.Runtime.Remoting.Messaging.HeaderHandler" /> that handles any headers in the <paramref name="serializationStream" />. Can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <paramref name="serializationStream" /> supports seeking, but its length is 0. -or-The target type is a <see cref="T:System.Decimal" />, but the value is out of range of the <see cref="T:System.Decimal" /> type.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public object Deserialize(Stream serializationStream, HeaderHandler handler)
	{
		return Deserialize(serializationStream, handler, fCheck: true);
	}

	/// <summary>Deserializes a response to a remote method call from the provided <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>The deserialized response to the remote method call.</returns>
	/// <param name="serializationStream">The stream from which to deserialize the object graph. </param>
	/// <param name="handler">The <see cref="T:System.Runtime.Remoting.Messaging.HeaderHandler" /> that handles any headers in the <paramref name="serializationStream" />. Can be null. </param>
	/// <param name="methodCallMessage">The <see cref="T:System.Runtime.Remoting.Messaging.IMethodCallMessage" /> that contains details about where the call came from. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <paramref name="serializationStream" /> supports seeking, but its length is 0. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public object DeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
	{
		return Deserialize(serializationStream, handler, fCheck: true, methodCallMessage);
	}

	/// <summary>Deserializes the specified stream into an object graph. The provided <see cref="T:System.Runtime.Remoting.Messaging.HeaderHandler" /> handles any headers in that stream.</summary>
	/// <returns>The deserialized object or the top object (root) of the object graph.</returns>
	/// <param name="serializationStream">The stream from which to deserialize the object graph. </param>
	/// <param name="handler">The <see cref="T:System.Runtime.Remoting.Messaging.HeaderHandler" /> that handles any headers in the <paramref name="serializationStream" />. Can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <paramref name="serializationStream" /> supports seeking, but its length is 0. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, SerializationFormatter" />
	/// </PermissionSet>
	[ComVisible(false)]
	[SecurityCritical]
	public object UnsafeDeserialize(Stream serializationStream, HeaderHandler handler)
	{
		return Deserialize(serializationStream, handler, fCheck: false);
	}

	/// <summary>Deserializes a response to a remote method call from the provided <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>The deserialized response to the remote method call.</returns>
	/// <param name="serializationStream">The stream from which to deserialize the object graph. </param>
	/// <param name="handler">The <see cref="T:System.Runtime.Remoting.Messaging.HeaderHandler" /> that handles any headers in the <paramref name="serializationStream" />. Can be null. </param>
	/// <param name="methodCallMessage">The <see cref="T:System.Runtime.Remoting.Messaging.IMethodCallMessage" /> that contains details about where the call came from. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <paramref name="serializationStream" /> supports seeking, but its length is 0. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	[ComVisible(false)]
	public object UnsafeDeserializeMethodResponse(Stream serializationStream, HeaderHandler handler, IMethodCallMessage methodCallMessage)
	{
		return Deserialize(serializationStream, handler, fCheck: false, methodCallMessage);
	}

	[SecurityCritical]
	internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, IMethodCallMessage methodCallMessage)
	{
		return Deserialize(serializationStream, handler, fCheck, isCrossAppDomain: false, methodCallMessage);
	}

	[SecurityCritical]
	internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage)
	{
		if (serializationStream == null)
		{
			throw new ArgumentNullException("serializationStream", Environment.GetResourceString("Parameter '{0}' cannot be null.", serializationStream));
		}
		if (serializationStream.CanSeek && serializationStream.Length == 0L)
		{
			throw new SerializationException(Environment.GetResourceString("Attempting to deserialize an empty stream."));
		}
		InternalFE internalFE = new InternalFE();
		internalFE.FEtypeFormat = m_typeFormat;
		internalFE.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
		internalFE.FEassemblyFormat = m_assemblyFormat;
		internalFE.FEsecurityLevel = m_securityLevel;
		ObjectReader objectReader = new ObjectReader(serializationStream, m_surrogates, m_context, internalFE, m_binder);
		objectReader.crossAppDomainArray = m_crossAppDomainArray;
		return objectReader.Deserialize(handler, new __BinaryParser(serializationStream, objectReader), fCheck, isCrossAppDomain, methodCallMessage);
	}

	/// <summary>Serializes the object, or graph of objects with the specified top (root), to the given stream.</summary>
	/// <param name="serializationStream">The stream to which the graph is to be serialized. </param>
	/// <param name="graph">The object at the root of the graph to serialize. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. -or-The <paramref name="graph" /> is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An error has occurred during serialization, such as if an object in the <paramref name="graph" /> parameter is not marked as serializable. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public void Serialize(Stream serializationStream, object graph)
	{
		Serialize(serializationStream, graph, null);
	}

	/// <summary>Serializes the object, or graph of objects with the specified top (root), to the given stream attaching the provided headers.</summary>
	/// <param name="serializationStream">The stream to which the object is to be serialized. </param>
	/// <param name="graph">The object at the root of the graph to serialize. </param>
	/// <param name="headers">Remoting headers to include in the serialization. Can be null. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="serializationStream" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An error has occurred during serialization, such as if an object in the <paramref name="graph" /> parameter is not marked as serializable. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public void Serialize(Stream serializationStream, object graph, Header[] headers)
	{
		Serialize(serializationStream, graph, headers, fCheck: true);
	}

	[SecurityCritical]
	internal void Serialize(Stream serializationStream, object graph, Header[] headers, bool fCheck)
	{
		if (serializationStream == null)
		{
			throw new ArgumentNullException("serializationStream", Environment.GetResourceString("Parameter '{0}' cannot be null.", serializationStream));
		}
		InternalFE internalFE = new InternalFE();
		internalFE.FEtypeFormat = m_typeFormat;
		internalFE.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
		internalFE.FEassemblyFormat = m_assemblyFormat;
		ObjectWriter objectWriter = new ObjectWriter(m_surrogates, m_context, internalFE, m_binder);
		__BinaryWriter serWriter = new __BinaryWriter(serializationStream, objectWriter, m_typeFormat);
		objectWriter.Serialize(graph, headers, serWriter, fCheck);
		m_crossAppDomainArray = objectWriter.crossAppDomainArray;
	}

	internal static TypeInformation GetTypeInformation(Type type)
	{
		lock (typeNameCache)
		{
			TypeInformation value = null;
			if (!typeNameCache.TryGetValue(type, out value))
			{
				bool hasTypeForwardedFrom;
				string clrAssemblyName = FormatterServices.GetClrAssemblyName(type, out hasTypeForwardedFrom);
				value = new TypeInformation(FormatterServices.GetClrTypeFullName(type), clrAssemblyName, hasTypeForwardedFrom);
				typeNameCache.Add(type, value);
			}
			return value;
		}
	}
}
