using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting;

/// <summary>Provides several methods for using and publishing remoted objects in SOAP format.</summary>
[ComVisible(true)]
public class SoapServices
{
	private class TypeInfo
	{
		public Hashtable Attributes;

		public Hashtable Elements;
	}

	private static Hashtable _xmlTypes = new Hashtable();

	private static Hashtable _xmlElements = new Hashtable();

	private static Hashtable _soapActions = new Hashtable();

	private static Hashtable _soapActionsMethods = new Hashtable();

	private static Hashtable _typeInfos = new Hashtable();

	/// <summary>Gets the XML namespace prefix for common language runtime types.</summary>
	/// <returns>The XML namespace prefix for common language runtime types.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string XmlNsForClrType => "http://schemas.microsoft.com/clr/";

	/// <summary>Gets the default XML namespace prefix that should be used for XML encoding of a common language runtime class that has an assembly, but no native namespace.</summary>
	/// <returns>The default XML namespace prefix that should be used for XML encoding of a common language runtime class that has an assembly, but no native namespace.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string XmlNsForClrTypeWithAssembly => "http://schemas.microsoft.com/clr/assem/";

	/// <summary>Gets the XML namespace prefix that should be used for XML encoding of a common language runtime class that is part of the mscorlib.dll file.</summary>
	/// <returns>The XML namespace prefix that should be used for XML encoding of a common language runtime class that is part of the mscorlib.dll file.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string XmlNsForClrTypeWithNs => "http://schemas.microsoft.com/clr/ns/";

	/// <summary>Gets the default XML namespace prefix that should be used for XML encoding of a common language runtime class that has both a common language runtime namespace and an assembly.</summary>
	/// <returns>The default XML namespace prefix that should be used for XML encoding of a common language runtime class that has both a common language runtime namespace and an assembly.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string XmlNsForClrTypeWithNsAndAssembly => "http://schemas.microsoft.com/clr/nsassem/";

	private SoapServices()
	{
	}

	/// <summary>Returns the common language runtime type namespace name from the provided namespace and assembly names.</summary>
	/// <returns>The common language runtime type namespace name from the provided namespace and assembly names.</returns>
	/// <param name="typeNamespace">The namespace that is to be coded. </param>
	/// <param name="assemblyName">The name of the assembly that is to be coded. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="assemblyName" /> and <paramref name="typeNamespace" /> parameters are both either null or empty. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string CodeXmlNamespaceForClrTypeNamespace(string typeNamespace, string assemblyName)
	{
		if (assemblyName == string.Empty)
		{
			return XmlNsForClrTypeWithNs + typeNamespace;
		}
		if (typeNamespace == string.Empty)
		{
			return EncodeNs(XmlNsForClrTypeWithAssembly + assemblyName);
		}
		return EncodeNs(XmlNsForClrTypeWithNsAndAssembly + typeNamespace + "/" + assemblyName);
	}

	/// <summary>Decodes the XML namespace and assembly names from the provided common language runtime namespace.</summary>
	/// <returns>true if the namespace and assembly names were successfully decoded; otherwise, false.</returns>
	/// <param name="inNamespace">The common language runtime namespace. </param>
	/// <param name="typeNamespace">When this method returns, contains a <see cref="T:System.String" /> that holds the decoded namespace name. This parameter is passed uninitialized. </param>
	/// <param name="assemblyName">When this method returns, contains a <see cref="T:System.String" /> that holds the decoded assembly name. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="inNamespace" /> parameter is null or empty. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool DecodeXmlNamespaceForClrTypeNamespace(string inNamespace, out string typeNamespace, out string assemblyName)
	{
		if (inNamespace == null)
		{
			throw new ArgumentNullException("inNamespace");
		}
		inNamespace = DecodeNs(inNamespace);
		typeNamespace = null;
		assemblyName = null;
		if (inNamespace.StartsWith(XmlNsForClrTypeWithNsAndAssembly))
		{
			int length = XmlNsForClrTypeWithNsAndAssembly.Length;
			if (length >= inNamespace.Length)
			{
				return false;
			}
			int num = inNamespace.IndexOf('/', length + 1);
			if (num == -1)
			{
				return false;
			}
			typeNamespace = inNamespace.Substring(length, num - length);
			assemblyName = inNamespace.Substring(num + 1);
			return true;
		}
		if (inNamespace.StartsWith(XmlNsForClrTypeWithNs))
		{
			int length2 = XmlNsForClrTypeWithNs.Length;
			typeNamespace = inNamespace.Substring(length2);
			return true;
		}
		if (inNamespace.StartsWith(XmlNsForClrTypeWithAssembly))
		{
			int length3 = XmlNsForClrTypeWithAssembly.Length;
			assemblyName = inNamespace.Substring(length3);
			return true;
		}
		return false;
	}

	/// <summary>Retrieves field type from XML attribute name, namespace, and the <see cref="T:System.Type" /> of the containing object.</summary>
	/// <param name="containingType">The <see cref="T:System.Type" /> of the object that contains the field. </param>
	/// <param name="xmlAttribute">The XML attribute name of the field type. </param>
	/// <param name="xmlNamespace">The XML namespace of the field type. </param>
	/// <param name="type">When this method returns, contains a <see cref="T:System.Type" /> of the field. This parameter is passed uninitialized. </param>
	/// <param name="name">When this method returns, contains a <see cref="T:System.String" /> that holds the name of the field. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void GetInteropFieldTypeAndNameFromXmlAttribute(Type containingType, string xmlAttribute, string xmlNamespace, out Type type, out string name)
	{
		GetInteropFieldInfo(((TypeInfo)_typeInfos[containingType])?.Attributes, xmlAttribute, xmlNamespace, out type, out name);
	}

	/// <summary>Retrieves the <see cref="T:System.Type" /> and name of a field from the provided XML element name, namespace, and the containing type.</summary>
	/// <param name="containingType">The <see cref="T:System.Type" /> of the object that contains the field. </param>
	/// <param name="xmlElement">The XML element name of field. </param>
	/// <param name="xmlNamespace">The XML namespace of the field type. </param>
	/// <param name="type">When this method returns, contains a <see cref="T:System.Type" /> of the field. This parameter is passed uninitialized. </param>
	/// <param name="name">When this method returns, contains a <see cref="T:System.String" /> that holds the name of the field. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void GetInteropFieldTypeAndNameFromXmlElement(Type containingType, string xmlElement, string xmlNamespace, out Type type, out string name)
	{
		GetInteropFieldInfo(((TypeInfo)_typeInfos[containingType])?.Elements, xmlElement, xmlNamespace, out type, out name);
	}

	private static void GetInteropFieldInfo(Hashtable fields, string xmlName, string xmlNamespace, out Type type, out string name)
	{
		if (fields != null)
		{
			FieldInfo fieldInfo = (FieldInfo)fields[GetNameKey(xmlName, xmlNamespace)];
			if (fieldInfo != null)
			{
				type = fieldInfo.FieldType;
				name = fieldInfo.Name;
				return;
			}
		}
		type = null;
		name = null;
	}

	private static string GetNameKey(string name, string namspace)
	{
		if (namspace == null)
		{
			return name;
		}
		return name + " " + namspace;
	}

	/// <summary>Retrieves the <see cref="T:System.Type" /> that should be used during deserialization of an unrecognized object type with the given XML element name and namespace.</summary>
	/// <returns>The <see cref="T:System.Type" /> of object associated with the specified XML element name and namespace.</returns>
	/// <param name="xmlElement">The XML element name of the unknown object type. </param>
	/// <param name="xmlNamespace">The XML namespace of the unknown object type. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static Type GetInteropTypeFromXmlElement(string xmlElement, string xmlNamespace)
	{
		lock (_xmlElements.SyncRoot)
		{
			return (Type)_xmlElements[xmlElement + " " + xmlNamespace];
		}
	}

	/// <summary>Retrieves the object <see cref="T:System.Type" /> that should be used during deserialization of an unrecognized object type with the given XML type name and namespace.</summary>
	/// <returns>The <see cref="T:System.Type" /> of object associated with the specified XML type name and namespace.</returns>
	/// <param name="xmlType">The XML type of the unknown object type. </param>
	/// <param name="xmlTypeNamespace">The XML type namespace of the unknown object type. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static Type GetInteropTypeFromXmlType(string xmlType, string xmlTypeNamespace)
	{
		lock (_xmlTypes.SyncRoot)
		{
			return (Type)_xmlTypes[xmlType + " " + xmlTypeNamespace];
		}
	}

	private static string GetAssemblyName(MethodBase mb)
	{
		if (mb.DeclaringType.Assembly == typeof(object).Assembly)
		{
			return string.Empty;
		}
		return mb.DeclaringType.Assembly.GetName().Name;
	}

	/// <summary>Returns the SOAPAction value associated with the method specified in the given <see cref="T:System.Reflection.MethodBase" />.</summary>
	/// <returns>The SOAPAction value associated with the method specified in the given <see cref="T:System.Reflection.MethodBase" />.</returns>
	/// <param name="mb">The <see cref="T:System.Reflection.MethodBase" /> that contains the method for which a SOAPAction is requested. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string GetSoapActionFromMethodBase(MethodBase mb)
	{
		return InternalGetSoapAction(mb);
	}

	/// <summary>Determines the type and method name of the method associated with the specified SOAPAction value.</summary>
	/// <returns>true if the type and method name were successfully recovered; otherwise, false.</returns>
	/// <param name="soapAction">The SOAPAction of the method for which the type and method names were requested. </param>
	/// <param name="typeName">When this method returns, contains a <see cref="T:System.String" /> that holds the type name of the method in question. This parameter is passed uninitialized. </param>
	/// <param name="methodName">When this method returns, contains a <see cref="T:System.String" /> that holds the method name of the method in question. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The SOAPAction value does not start and end with quotes. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool GetTypeAndMethodNameFromSoapAction(string soapAction, out string typeName, out string methodName)
	{
		lock (_soapActions.SyncRoot)
		{
			MethodBase methodBase = (MethodBase)_soapActionsMethods[soapAction];
			if (methodBase != null)
			{
				typeName = methodBase.DeclaringType.AssemblyQualifiedName;
				methodName = methodBase.Name;
				return true;
			}
		}
		typeName = null;
		methodName = null;
		int num = soapAction.LastIndexOf('#');
		if (num == -1)
		{
			return false;
		}
		methodName = soapAction.Substring(num + 1);
		if (!DecodeXmlNamespaceForClrTypeNamespace(soapAction.Substring(0, num), out var typeNamespace, out var assemblyName))
		{
			return false;
		}
		if (assemblyName == null)
		{
			typeName = typeNamespace + ", " + typeof(object).Assembly.GetName().Name;
		}
		else
		{
			typeName = typeNamespace + ", " + assemblyName;
		}
		return true;
	}

	/// <summary>Returns XML element information that should be used when serializing the given type.</summary>
	/// <returns>true if the requested values have been set flagged with <see cref="T:System.Runtime.Remoting.Metadata.SoapTypeAttribute" />; otherwise, false.</returns>
	/// <param name="type">The object <see cref="T:System.Type" /> for which the XML element and namespace names were requested. </param>
	/// <param name="xmlElement">When this method returns, contains a <see cref="T:System.String" /> that holds the XML element name of the specified object type. This parameter is passed uninitialized. </param>
	/// <param name="xmlNamespace">When this method returns, contains a <see cref="T:System.String" /> that holds the XML namespace name of the specified object type. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool GetXmlElementForInteropType(Type type, out string xmlElement, out string xmlNamespace)
	{
		SoapTypeAttribute soapTypeAttribute = (SoapTypeAttribute)InternalRemotingServices.GetCachedSoapAttribute(type);
		if (!soapTypeAttribute.IsInteropXmlElement)
		{
			xmlElement = null;
			xmlNamespace = null;
			return false;
		}
		xmlElement = soapTypeAttribute.XmlElementName;
		xmlNamespace = soapTypeAttribute.XmlNamespace;
		return true;
	}

	/// <summary>Retrieves the XML namespace used during remote calls of the method specified in the given <see cref="T:System.Reflection.MethodBase" />.</summary>
	/// <returns>The XML namespace used during remote calls of the specified method.</returns>
	/// <param name="mb">The <see cref="T:System.Reflection.MethodBase" /> of the method for which the XML namespace was requested. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string GetXmlNamespaceForMethodCall(MethodBase mb)
	{
		return CodeXmlNamespaceForClrTypeNamespace(mb.DeclaringType.FullName, GetAssemblyName(mb));
	}

	/// <summary>Retrieves the XML namespace used during the generation of responses to the remote call to the method specified in the given <see cref="T:System.Reflection.MethodBase" />.</summary>
	/// <returns>The XML namespace used during the generation of responses to a remote method call.</returns>
	/// <param name="mb">The <see cref="T:System.Reflection.MethodBase" /> of the method for which the XML namespace was requested. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string GetXmlNamespaceForMethodResponse(MethodBase mb)
	{
		return CodeXmlNamespaceForClrTypeNamespace(mb.DeclaringType.FullName, GetAssemblyName(mb));
	}

	/// <summary>Returns XML type information that should be used when serializing the given <see cref="T:System.Type" />.</summary>
	/// <returns>true if the requested values have been set flagged with <see cref="T:System.Runtime.Remoting.Metadata.SoapTypeAttribute" />; otherwise, false.</returns>
	/// <param name="type">The object <see cref="T:System.Type" /> for which the XML element and namespace names were requested. </param>
	/// <param name="xmlType">The XML type of the specified object <see cref="T:System.Type" />. </param>
	/// <param name="xmlTypeNamespace">The XML type namespace of the specified object <see cref="T:System.Type" />. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool GetXmlTypeForInteropType(Type type, out string xmlType, out string xmlTypeNamespace)
	{
		SoapTypeAttribute soapTypeAttribute = (SoapTypeAttribute)InternalRemotingServices.GetCachedSoapAttribute(type);
		if (!soapTypeAttribute.IsInteropXmlType)
		{
			xmlType = null;
			xmlTypeNamespace = null;
			return false;
		}
		xmlType = soapTypeAttribute.XmlTypeName;
		xmlTypeNamespace = soapTypeAttribute.XmlTypeNamespace;
		return true;
	}

	/// <summary>Returns a Boolean value that indicates whether the specified namespace is native to the common language runtime.</summary>
	/// <returns>true if the given namespace is native to the common language runtime; otherwise, false.</returns>
	/// <param name="namespaceString">The namespace to check in the common language runtime. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool IsClrTypeNamespace(string namespaceString)
	{
		return namespaceString.StartsWith(XmlNsForClrType);
	}

	/// <summary>Determines if the specified SOAPAction is acceptable for a given <see cref="T:System.Reflection.MethodBase" />.</summary>
	/// <returns>true if the specified SOAPAction is acceptable for a given <see cref="T:System.Reflection.MethodBase" />; otherwise, false.</returns>
	/// <param name="soapAction">The SOAPAction to check against the given <see cref="T:System.Reflection.MethodBase" />. </param>
	/// <param name="mb">The <see cref="T:System.Reflection.MethodBase" /> the specified SOAPAction is checked against. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool IsSoapActionValidForMethodBase(string soapAction, MethodBase mb)
	{
		GetTypeAndMethodNameFromSoapAction(soapAction, out var typeName, out var methodName);
		if (methodName != mb.Name)
		{
			return false;
		}
		string assemblyQualifiedName = mb.DeclaringType.AssemblyQualifiedName;
		return typeName == assemblyQualifiedName;
	}

	/// <summary>Preloads every <see cref="T:System.Type" /> found in the specified <see cref="T:System.Reflection.Assembly" /> from the information found in the <see cref="T:System.Runtime.Remoting.Metadata.SoapTypeAttribute" /> associated with each type.</summary>
	/// <param name="assembly">The <see cref="T:System.Reflection.Assembly" /> for each type of which to call <see cref="M:System.Runtime.Remoting.SoapServices.PreLoad(System.Type)" />. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void PreLoad(Assembly assembly)
	{
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			PreLoad(types[i]);
		}
	}

	/// <summary>Preloads the given <see cref="T:System.Type" /> based on values set in a <see cref="T:System.Runtime.Remoting.Metadata.SoapTypeAttribute" /> on the type.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> to preload. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void PreLoad(Type type)
	{
		TypeInfo typeInfo = _typeInfos[type] as TypeInfo;
		if (typeInfo != null)
		{
			return;
		}
		if (GetXmlTypeForInteropType(type, out var xmlType, out var xmlTypeNamespace))
		{
			RegisterInteropXmlType(xmlType, xmlTypeNamespace, type);
		}
		if (GetXmlElementForInteropType(type, out xmlType, out xmlTypeNamespace))
		{
			RegisterInteropXmlElement(xmlType, xmlTypeNamespace, type);
		}
		lock (_typeInfos.SyncRoot)
		{
			typeInfo = new TypeInfo();
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				SoapFieldAttribute soapFieldAttribute = (SoapFieldAttribute)InternalRemotingServices.GetCachedSoapAttribute(fieldInfo);
				if (!soapFieldAttribute.IsInteropXmlElement())
				{
					continue;
				}
				string nameKey = GetNameKey(soapFieldAttribute.XmlElementName, soapFieldAttribute.XmlNamespace);
				if (soapFieldAttribute.UseAttribute)
				{
					if (typeInfo.Attributes == null)
					{
						typeInfo.Attributes = new Hashtable();
					}
					typeInfo.Attributes[nameKey] = fieldInfo;
				}
				else
				{
					if (typeInfo.Elements == null)
					{
						typeInfo.Elements = new Hashtable();
					}
					typeInfo.Elements[nameKey] = fieldInfo;
				}
			}
			_typeInfos[type] = typeInfo;
		}
	}

	/// <summary>Associates the given XML element name and namespace with a run-time type that should be used for deserialization.</summary>
	/// <param name="xmlElement">The XML element name to use in deserialization. </param>
	/// <param name="xmlNamespace">The XML namespace to use in deserialization. </param>
	/// <param name="type">The run-time <see cref="T:System.Type" /> to use in deserialization. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void RegisterInteropXmlElement(string xmlElement, string xmlNamespace, Type type)
	{
		lock (_xmlElements.SyncRoot)
		{
			_xmlElements[xmlElement + " " + xmlNamespace] = type;
		}
	}

	/// <summary>Associates the given XML type name and namespace with the run-time type that should be used for deserialization.</summary>
	/// <param name="xmlType">The XML type to use in deserialization. </param>
	/// <param name="xmlTypeNamespace">The XML namespace to use in deserialization. </param>
	/// <param name="type">The run-time <see cref="T:System.Type" /> to use in deserialization. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void RegisterInteropXmlType(string xmlType, string xmlTypeNamespace, Type type)
	{
		lock (_xmlTypes.SyncRoot)
		{
			_xmlTypes[xmlType + " " + xmlTypeNamespace] = type;
		}
	}

	/// <summary>Associates the specified <see cref="T:System.Reflection.MethodBase" /> with the SOAPAction cached with it.</summary>
	/// <param name="mb">The <see cref="T:System.Reflection.MethodBase" /> of the method to associate with the SOAPAction cached with it. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void RegisterSoapActionForMethodBase(MethodBase mb)
	{
		InternalGetSoapAction(mb);
	}

	private static string InternalGetSoapAction(MethodBase mb)
	{
		lock (_soapActions.SyncRoot)
		{
			string text = (string)_soapActions[mb];
			if (text == null)
			{
				text = ((SoapMethodAttribute)InternalRemotingServices.GetCachedSoapAttribute(mb)).SoapAction;
				_soapActions[mb] = text;
				_soapActionsMethods[text] = mb;
			}
			return text;
		}
	}

	/// <summary>Associates the provided SOAPAction value with the given <see cref="T:System.Reflection.MethodBase" /> for use in channel sinks.</summary>
	/// <param name="mb">The <see cref="T:System.Reflection.MethodBase" /> to associate with the provided SOAPAction. </param>
	/// <param name="soapAction">The SOAPAction value to associate with the given <see cref="T:System.Reflection.MethodBase" />. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void RegisterSoapActionForMethodBase(MethodBase mb, string soapAction)
	{
		lock (_soapActions.SyncRoot)
		{
			_soapActions[mb] = soapAction;
			_soapActionsMethods[soapAction] = mb;
		}
	}

	private static string EncodeNs(string ns)
	{
		ns = ns.Replace(",", "%2C");
		ns = ns.Replace(" ", "%20");
		return ns.Replace("=", "%3D");
	}

	private static string DecodeNs(string ns)
	{
		ns = ns.Replace("%2C", ",");
		ns = ns.Replace("%20", " ");
		return ns.Replace("%3D", "=");
	}
}
