using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace System.Runtime.Serialization;

internal class CodeExporter
{
	private DataContractSet dataContractSet;

	private CodeCompileUnit codeCompileUnit;

	private ImportOptions options;

	private Dictionary<string, string> namespaces;

	private Dictionary<string, string> clrNamespaces;

	private static readonly string wildcardNamespaceMapping = "*";

	private static readonly string typeNameFieldName = "typeName";

	private static readonly object codeUserDataActualTypeKey = new object();

	private static readonly object surrogateDataKey = typeof(IDataContractSurrogate);

	private const int MaxIdentifierLength = 511;

	private bool GenerateSerializableTypes
	{
		get
		{
			if (options != null)
			{
				return options.GenerateSerializable;
			}
			return false;
		}
	}

	private bool GenerateInternalTypes
	{
		get
		{
			if (options != null)
			{
				return options.GenerateInternal;
			}
			return false;
		}
	}

	private bool EnableDataBinding
	{
		get
		{
			if (options != null)
			{
				return options.EnableDataBinding;
			}
			return false;
		}
	}

	private CodeDomProvider CodeProvider
	{
		get
		{
			if (options != null)
			{
				return options.CodeProvider;
			}
			return null;
		}
	}

	private bool SupportsDeclareEvents
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.Supports(GeneratorSupport.DeclareEvents);
			}
			return true;
		}
	}

	private bool SupportsDeclareValueTypes
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.Supports(GeneratorSupport.DeclareValueTypes);
			}
			return true;
		}
	}

	private bool SupportsGenericTypeReference
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.Supports(GeneratorSupport.GenericTypeReference);
			}
			return true;
		}
	}

	private bool SupportsAssemblyAttributes
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.Supports(GeneratorSupport.AssemblyAttributes);
			}
			return true;
		}
	}

	private bool SupportsPartialTypes
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.Supports(GeneratorSupport.PartialTypes);
			}
			return true;
		}
	}

	private bool SupportsNestedTypes
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.Supports(GeneratorSupport.NestedTypes);
			}
			return true;
		}
	}

	private string FileExtension
	{
		[SecuritySafeCritical]
		get
		{
			if (CodeProvider != null)
			{
				return CodeProvider.FileExtension;
			}
			return string.Empty;
		}
	}

	private Dictionary<string, string> Namespaces => namespaces;

	private Dictionary<string, string> ClrNamespaces => clrNamespaces;

	private XmlQualifiedName GenericListName => DataContract.GetStableName(Globals.TypeOfListGeneric);

	private CollectionDataContract GenericListContract => dataContractSet.GetDataContract(Globals.TypeOfListGeneric) as CollectionDataContract;

	private XmlQualifiedName GenericDictionaryName => DataContract.GetStableName(Globals.TypeOfDictionaryGeneric);

	private CollectionDataContract GenericDictionaryContract => dataContractSet.GetDataContract(Globals.TypeOfDictionaryGeneric) as CollectionDataContract;

	private CodeTypeReference CodeTypeIPropertyChange => GetCodeTypeReference(typeof(INotifyPropertyChanged));

	private CodeThisReferenceExpression ThisReference => new CodeThisReferenceExpression();

	private CodePrimitiveExpression NullReference => new CodePrimitiveExpression(null);

	private CodeParameterDeclarationExpression SerializationInfoParameter => new CodeParameterDeclarationExpression(GetCodeTypeReference(Globals.TypeOfSerializationInfo), "info");

	private CodeParameterDeclarationExpression StreamingContextParameter => new CodeParameterDeclarationExpression(GetCodeTypeReference(Globals.TypeOfStreamingContext), "context");

	private CodeAttributeDeclaration SerializableAttribute => new CodeAttributeDeclaration(GetCodeTypeReference(Globals.TypeOfSerializableAttribute));

	private CodeMemberProperty NodeArrayProperty => CreateProperty(GetCodeTypeReference(Globals.TypeOfXmlNodeArray), "Nodes", "nodesField", isValueType: false);

	private CodeMemberField NodeArrayField => new CodeMemberField
	{
		Type = GetCodeTypeReference(Globals.TypeOfXmlNodeArray),
		Name = "nodesField",
		Attributes = MemberAttributes.Private
	};

	private CodeMemberMethod ReadXmlMethod
	{
		get
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = "ReadXml";
			CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(typeof(XmlReader), "reader");
			codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
			codeMemberMethod.Attributes = (MemberAttributes)24578;
			codeMemberMethod.ImplementationTypes.Add(Globals.TypeOfIXmlSerializable);
			CodeAssignStatement codeAssignStatement = new CodeAssignStatement();
			codeAssignStatement.Left = new CodeFieldReferenceExpression(ThisReference, "nodesField");
			codeAssignStatement.Right = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(GetCodeTypeReference(Globals.TypeOfXmlSerializableServices)), XmlSerializableServices.ReadNodesMethodName, new CodeArgumentReferenceExpression(codeParameterDeclarationExpression.Name));
			codeMemberMethod.Statements.Add(codeAssignStatement);
			return codeMemberMethod;
		}
	}

	private CodeMemberMethod WriteXmlMethod
	{
		get
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = "WriteXml";
			CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(typeof(XmlWriter), "writer");
			codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
			codeMemberMethod.Attributes = (MemberAttributes)24578;
			codeMemberMethod.ImplementationTypes.Add(Globals.TypeOfIXmlSerializable);
			codeMemberMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(GetCodeTypeReference(Globals.TypeOfXmlSerializableServices)), XmlSerializableServices.WriteNodesMethodName, new CodeArgumentReferenceExpression(codeParameterDeclarationExpression.Name), new CodePropertyReferenceExpression(ThisReference, "Nodes")));
			return codeMemberMethod;
		}
	}

	private CodeMemberMethod GetSchemaMethod => new CodeMemberMethod
	{
		Name = "GetSchema",
		Attributes = (MemberAttributes)24578,
		ImplementationTypes = { Globals.TypeOfIXmlSerializable },
		ReturnType = GetCodeTypeReference(typeof(XmlSchema)),
		Statements = { (CodeStatement)new CodeMethodReturnStatement(NullReference) }
	};

	private CodeMemberMethod GetSchemaStaticMethod
	{
		get
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = "ExportSchema";
			codeMemberMethod.ReturnType = GetCodeTypeReference(Globals.TypeOfXmlQualifiedName);
			CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(Globals.TypeOfXmlSchemaSet, "schemas");
			codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
			codeMemberMethod.Attributes = (MemberAttributes)24579;
			codeMemberMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(GetCodeTypeReference(typeof(XmlSerializableServices))), XmlSerializableServices.AddDefaultSchemaMethodName, new CodeArgumentReferenceExpression(codeParameterDeclarationExpression.Name), new CodeFieldReferenceExpression(null, typeNameFieldName)));
			codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, typeNameFieldName)));
			return codeMemberMethod;
		}
	}

	private CodeConstructor ISerializableBaseConstructor
	{
		get
		{
			CodeConstructor codeConstructor = new CodeConstructor();
			codeConstructor.Attributes = MemberAttributes.Public;
			codeConstructor.Parameters.Add(SerializationInfoParameter);
			codeConstructor.Parameters.Add(StreamingContextParameter);
			CodeAssignStatement codeAssignStatement = new CodeAssignStatement();
			codeAssignStatement.Left = new CodePropertyReferenceExpression(ThisReference, "info");
			codeAssignStatement.Right = new CodeArgumentReferenceExpression("info");
			codeConstructor.Statements.Add(codeAssignStatement);
			if (EnableDataBinding && SupportsDeclareEvents && string.CompareOrdinal(FileExtension, "vb") != 0)
			{
				codeConstructor.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(ThisReference, PropertyChangedEvent.Name), NullReference));
			}
			return codeConstructor;
		}
	}

	private CodeConstructor ISerializableDerivedConstructor => new CodeConstructor
	{
		Attributes = MemberAttributes.Public,
		Parameters = { SerializationInfoParameter, StreamingContextParameter },
		BaseConstructorArgs = 
		{
			(CodeExpression)new CodeVariableReferenceExpression("info"),
			(CodeExpression)new CodeVariableReferenceExpression("context")
		}
	};

	private CodeMemberField SerializationInfoField => new CodeMemberField
	{
		Type = GetCodeTypeReference(Globals.TypeOfSerializationInfo),
		Name = "info",
		Attributes = MemberAttributes.Private
	};

	private CodeMemberProperty SerializationInfoProperty => CreateProperty(GetCodeTypeReference(Globals.TypeOfSerializationInfo), "SerializationInfo", "info", isValueType: false);

	private CodeMemberMethod GetObjectDataMethod
	{
		get
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = "GetObjectData";
			codeMemberMethod.Parameters.Add(SerializationInfoParameter);
			codeMemberMethod.Parameters.Add(StreamingContextParameter);
			codeMemberMethod.Attributes = (MemberAttributes)24578;
			codeMemberMethod.ImplementationTypes.Add(Globals.TypeOfISerializable);
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			codeConditionStatement.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(ThisReference, "SerializationInfo"), CodeBinaryOperatorType.IdentityEquality, NullReference);
			codeConditionStatement.TrueStatements.Add(new CodeMethodReturnStatement());
			CodeVariableDeclarationStatement codeVariableDeclarationStatement = new CodeVariableDeclarationStatement();
			codeVariableDeclarationStatement.Type = GetCodeTypeReference(Globals.TypeOfSerializationInfoEnumerator);
			codeVariableDeclarationStatement.Name = "enumerator";
			codeVariableDeclarationStatement.InitExpression = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(ThisReference, "SerializationInfo"), "GetEnumerator");
			CodeVariableDeclarationStatement codeVariableDeclarationStatement2 = new CodeVariableDeclarationStatement();
			codeVariableDeclarationStatement2.Type = GetCodeTypeReference(Globals.TypeOfSerializationEntry);
			codeVariableDeclarationStatement2.Name = "entry";
			codeVariableDeclarationStatement2.InitExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("enumerator"), "Current");
			CodeExpressionStatement codeExpressionStatement = new CodeExpressionStatement();
			CodePropertyReferenceExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("entry"), "Name");
			CodePropertyReferenceExpression codePropertyReferenceExpression2 = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("entry"), "Value");
			codeExpressionStatement.Expression = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("info"), "AddValue", codePropertyReferenceExpression, codePropertyReferenceExpression2);
			CodeIterationStatement codeIterationStatement = new CodeIterationStatement();
			codeIterationStatement.TestExpression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("enumerator"), "MoveNext");
			CodeStatement initStatement = (codeIterationStatement.IncrementStatement = new CodeSnippetStatement(string.Empty));
			codeIterationStatement.InitStatement = initStatement;
			codeIterationStatement.Statements.Add(codeVariableDeclarationStatement2);
			codeIterationStatement.Statements.Add(codeExpressionStatement);
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeMemberMethod.Statements.Add(codeVariableDeclarationStatement);
			codeMemberMethod.Statements.Add(codeIterationStatement);
			return codeMemberMethod;
		}
	}

	private CodeMemberField ExtensionDataObjectField => new CodeMemberField
	{
		Type = GetCodeTypeReference(Globals.TypeOfExtensionDataObject),
		Name = "extensionDataField",
		Attributes = MemberAttributes.Private
	};

	private CodeMemberProperty ExtensionDataObjectProperty
	{
		get
		{
			CodeMemberProperty obj = new CodeMemberProperty
			{
				Type = GetCodeTypeReference(Globals.TypeOfExtensionDataObject),
				Name = "ExtensionData",
				Attributes = (MemberAttributes)24578,
				ImplementationTypes = { Globals.TypeOfIExtensibleDataObject }
			};
			CodeMethodReturnStatement value = new CodeMethodReturnStatement
			{
				Expression = new CodeFieldReferenceExpression(ThisReference, "extensionDataField")
			};
			obj.GetStatements.Add(value);
			CodeAssignStatement value2 = new CodeAssignStatement
			{
				Left = new CodeFieldReferenceExpression(ThisReference, "extensionDataField"),
				Right = new CodePropertySetValueReferenceExpression()
			};
			obj.SetStatements.Add(value2);
			return obj;
		}
	}

	private CodeMemberMethod RaisePropertyChangedEventMethod
	{
		get
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.Name = "RaisePropertyChanged";
			codeMemberMethod.Attributes = MemberAttributes.Final;
			CodeArgumentReferenceExpression codeArgumentReferenceExpression = new CodeArgumentReferenceExpression("propertyName");
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), codeArgumentReferenceExpression.ParameterName));
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("propertyChanged");
			codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangedEventHandler), codeVariableReferenceExpression.VariableName, new CodeEventReferenceExpression(ThisReference, PropertyChangedEvent.Name)));
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement(new CodeBinaryOperatorExpression(codeVariableReferenceExpression, CodeBinaryOperatorType.IdentityInequality, NullReference));
			codeMemberMethod.Statements.Add(codeConditionStatement);
			codeConditionStatement.TrueStatements.Add(new CodeDelegateInvokeExpression(codeVariableReferenceExpression, ThisReference, new CodeObjectCreateExpression(typeof(PropertyChangedEventArgs), codeArgumentReferenceExpression)));
			return codeMemberMethod;
		}
	}

	private CodeMemberEvent PropertyChangedEvent => new CodeMemberEvent
	{
		Attributes = MemberAttributes.Public,
		Name = "PropertyChanged",
		Type = GetCodeTypeReference(typeof(PropertyChangedEventHandler)),
		ImplementationTypes = { Globals.TypeOfIPropertyChange }
	};

	internal CodeExporter(DataContractSet dataContractSet, ImportOptions options, CodeCompileUnit codeCompileUnit)
	{
		this.dataContractSet = dataContractSet;
		this.codeCompileUnit = codeCompileUnit;
		AddReferencedAssembly(Assembly.GetExecutingAssembly());
		this.options = options;
		namespaces = new Dictionary<string, string>();
		clrNamespaces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<XmlQualifiedName, DataContract> item in dataContractSet)
		{
			DataContract value = item.Value;
			if (value.IsBuiltInDataContract || value is CollectionDataContract)
			{
				continue;
			}
			ContractCodeDomInfo contractCodeDomInfo = GetContractCodeDomInfo(value);
			if (contractCodeDomInfo.IsProcessed && !contractCodeDomInfo.UsesWildcardNamespace)
			{
				string clrNamespace = contractCodeDomInfo.ClrNamespace;
				if (clrNamespace != null && !clrNamespaces.ContainsKey(clrNamespace))
				{
					clrNamespaces.Add(clrNamespace, value.StableName.Namespace);
					namespaces.Add(value.StableName.Namespace, clrNamespace);
				}
			}
		}
		if (this.options != null)
		{
			foreach (KeyValuePair<string, string> @namespace in options.Namespaces)
			{
				string key = @namespace.Key;
				string text = @namespace.Value;
				if (text == null)
				{
					text = string.Empty;
				}
				if (clrNamespaces.TryGetValue(text, out var value2))
				{
					if (key != value2)
					{
						throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("CLR namespace is mapped multiple times. Current data contract namespace is '{0}', found '{1}' for CLR namespace '{2}'.", value2, key, text)));
					}
				}
				else
				{
					clrNamespaces.Add(text, key);
				}
				if (namespaces.TryGetValue(key, out var value3))
				{
					if (text != value3)
					{
						namespaces.Remove(key);
						namespaces.Add(key, text);
					}
				}
				else
				{
					namespaces.Add(key, text);
				}
			}
		}
		foreach (CodeNamespace namespace2 in codeCompileUnit.Namespaces)
		{
			string text2 = namespace2.Name ?? string.Empty;
			if (!clrNamespaces.ContainsKey(text2))
			{
				clrNamespaces.Add(text2, null);
			}
			if (text2.Length != 0)
			{
				continue;
			}
			foreach (CodeTypeDeclaration type in namespace2.Types)
			{
				AddGlobalTypeName(type.Name);
			}
		}
	}

	private void AddReferencedAssembly(Assembly assembly)
	{
		string fileName = Path.GetFileName(assembly.Location);
		bool flag = false;
		StringEnumerator enumerator = codeCompileUnit.ReferencedAssemblies.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (string.Compare(enumerator.Current, fileName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					flag = true;
					break;
				}
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		if (!flag)
		{
			codeCompileUnit.ReferencedAssemblies.Add(fileName);
		}
	}

	private bool TryGetReferencedType(XmlQualifiedName stableName, DataContract dataContract, out Type type)
	{
		if (dataContract == null)
		{
			if (dataContractSet.TryGetReferencedCollectionType(stableName, dataContract, out type))
			{
				return true;
			}
			if (dataContractSet.TryGetReferencedType(stableName, dataContract, out type))
			{
				if (CollectionDataContract.IsCollection(type))
				{
					type = null;
					return false;
				}
				return true;
			}
			return false;
		}
		if (dataContract is CollectionDataContract)
		{
			return dataContractSet.TryGetReferencedCollectionType(stableName, dataContract, out type);
		}
		if (dataContract is XmlDataContract { IsAnonymous: not false } xmlDataContract)
		{
			stableName = SchemaImporter.ImportActualType(xmlDataContract.XsdType.Annotation, stableName, dataContract.StableName);
		}
		return dataContractSet.TryGetReferencedType(stableName, dataContract, out type);
	}

	[SecurityCritical]
	internal void Export()
	{
		try
		{
			foreach (KeyValuePair<XmlQualifiedName, DataContract> item in dataContractSet)
			{
				DataContract value = item.Value;
				if (value.IsBuiltInDataContract)
				{
					continue;
				}
				ContractCodeDomInfo contractCodeDomInfo = GetContractCodeDomInfo(value);
				if (contractCodeDomInfo.IsProcessed)
				{
					continue;
				}
				if (value is ClassDataContract)
				{
					ClassDataContract classDataContract = (ClassDataContract)value;
					if (classDataContract.IsISerializable)
					{
						ExportISerializableDataContract(classDataContract, contractCodeDomInfo);
					}
					else
					{
						ExportClassDataContractHierarchy(classDataContract.StableName, classDataContract, contractCodeDomInfo, new Dictionary<XmlQualifiedName, object>());
					}
				}
				else if (value is CollectionDataContract)
				{
					ExportCollectionDataContract((CollectionDataContract)value, contractCodeDomInfo);
				}
				else if (value is EnumDataContract)
				{
					ExportEnumDataContract((EnumDataContract)value, contractCodeDomInfo);
				}
				else
				{
					if (!(value is XmlDataContract))
					{
						throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("An internal error has occurred. Unexpected contract type '{0}' for type '{1}' encountered.", DataContract.GetClrTypeFullName(value.GetType()), DataContract.GetClrTypeFullName(value.UnderlyingType))));
					}
					ExportXmlDataContract((XmlDataContract)value, contractCodeDomInfo);
				}
				contractCodeDomInfo.IsProcessed = true;
			}
			if (dataContractSet.DataContractSurrogate != null)
			{
				CodeNamespace[] array = new CodeNamespace[codeCompileUnit.Namespaces.Count];
				codeCompileUnit.Namespaces.CopyTo(array, 0);
				CodeNamespace[] array2 = array;
				foreach (CodeNamespace codeNamespace in array2)
				{
					InvokeProcessImportedType(codeNamespace.Types);
				}
			}
		}
		finally
		{
			CodeGenerator.ValidateIdentifiers(codeCompileUnit);
		}
	}

	private void ExportClassDataContractHierarchy(XmlQualifiedName typeName, ClassDataContract classContract, ContractCodeDomInfo contractCodeDomInfo, Dictionary<XmlQualifiedName, object> contractNamesInHierarchy)
	{
		if (contractNamesInHierarchy.ContainsKey(classContract.StableName))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Type '{0}' in '{1}' namespace cannot be imported: {2}", typeName.Name, typeName.Namespace, SR.GetString("Circular type reference was found for '{0}' in '{1}' namespace.", classContract.StableName.Name, classContract.StableName.Namespace))));
		}
		contractNamesInHierarchy.Add(classContract.StableName, null);
		ClassDataContract baseContract = classContract.BaseContract;
		if (baseContract != null)
		{
			ContractCodeDomInfo contractCodeDomInfo2 = GetContractCodeDomInfo(baseContract);
			if (!contractCodeDomInfo2.IsProcessed)
			{
				ExportClassDataContractHierarchy(typeName, baseContract, contractCodeDomInfo2, contractNamesInHierarchy);
				contractCodeDomInfo2.IsProcessed = true;
			}
		}
		ExportClassDataContract(classContract, contractCodeDomInfo);
	}

	private void InvokeProcessImportedType(CollectionBase collection)
	{
		object[] array = new object[collection.Count];
		((ICollection)collection).CopyTo((Array)array, 0);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (!(array2[i] is CodeTypeDeclaration codeTypeDeclaration))
			{
				continue;
			}
			CodeTypeDeclaration codeTypeDeclaration2 = DataContractSurrogateCaller.ProcessImportedType(dataContractSet.DataContractSurrogate, codeTypeDeclaration, codeCompileUnit);
			if (codeTypeDeclaration2 != codeTypeDeclaration)
			{
				((IList)collection).Remove((object)codeTypeDeclaration);
				if (codeTypeDeclaration2 != null)
				{
					((IList)collection).Add((object)codeTypeDeclaration2);
				}
			}
			if (codeTypeDeclaration2 != null)
			{
				InvokeProcessImportedType(codeTypeDeclaration2.Members);
			}
		}
	}

	internal CodeTypeReference GetCodeTypeReference(DataContract dataContract)
	{
		if (dataContract.IsBuiltInDataContract)
		{
			return GetCodeTypeReference(dataContract.UnderlyingType);
		}
		ContractCodeDomInfo contractCodeDomInfo = GetContractCodeDomInfo(dataContract);
		GenerateType(dataContract, contractCodeDomInfo);
		return contractCodeDomInfo.TypeReference;
	}

	private CodeTypeReference GetCodeTypeReference(Type type)
	{
		AddReferencedAssembly(type.Assembly);
		return new CodeTypeReference(type);
	}

	internal CodeTypeReference GetElementTypeReference(DataContract dataContract, bool isElementTypeNullable)
	{
		CodeTypeReference codeTypeReference = GetCodeTypeReference(dataContract);
		if (dataContract.IsValueType && isElementTypeNullable)
		{
			codeTypeReference = WrapNullable(codeTypeReference);
		}
		return codeTypeReference;
	}

	private ContractCodeDomInfo GetContractCodeDomInfo(DataContract dataContract)
	{
		ContractCodeDomInfo contractCodeDomInfo = dataContractSet.GetContractCodeDomInfo(dataContract);
		if (contractCodeDomInfo == null)
		{
			contractCodeDomInfo = new ContractCodeDomInfo();
			dataContractSet.SetContractCodeDomInfo(dataContract, contractCodeDomInfo);
		}
		return contractCodeDomInfo;
	}

	private void GenerateType(DataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		if (contractCodeDomInfo.IsProcessed)
		{
			return;
		}
		CodeTypeReference referencedType = GetReferencedType(dataContract);
		if (referencedType != null)
		{
			contractCodeDomInfo.TypeReference = referencedType;
			contractCodeDomInfo.ReferencedTypeExists = true;
			return;
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
		if (typeDeclaration != null)
		{
			return;
		}
		string clrNamespace = GetClrNamespace(dataContract, contractCodeDomInfo);
		CodeNamespace codeNamespace = GetCodeNamespace(clrNamespace, dataContract.StableName.Namespace, contractCodeDomInfo);
		typeDeclaration = GetNestedType(dataContract, contractCodeDomInfo);
		if (typeDeclaration == null)
		{
			string identifier = XmlConvert.DecodeName(dataContract.StableName.Name);
			identifier = GetClrIdentifier(identifier, "GeneratedType");
			if (NamespaceContainsType(codeNamespace, identifier) || GlobalTypeNameConflicts(clrNamespace, identifier))
			{
				int num = 1;
				string text;
				while (true)
				{
					text = AppendToValidClrIdentifier(identifier, num.ToString(NumberFormatInfo.InvariantInfo));
					if (!NamespaceContainsType(codeNamespace, text) && !GlobalTypeNameConflicts(clrNamespace, text))
					{
						break;
					}
					if (num == int.MaxValue)
					{
						throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Cannot compute unique name for '{0}'.", identifier)));
					}
					num++;
				}
				identifier = text;
			}
			typeDeclaration = CreateTypeDeclaration(identifier, dataContract);
			codeNamespace.Types.Add(typeDeclaration);
			if (string.IsNullOrEmpty(clrNamespace))
			{
				AddGlobalTypeName(identifier);
			}
			contractCodeDomInfo.TypeReference = new CodeTypeReference((clrNamespace == null || clrNamespace.Length == 0) ? identifier : (clrNamespace + "." + identifier));
			if (GenerateInternalTypes)
			{
				typeDeclaration.TypeAttributes = TypeAttributes.NotPublic;
			}
			else
			{
				typeDeclaration.TypeAttributes = TypeAttributes.Public;
			}
		}
		if (dataContractSet.DataContractSurrogate != null)
		{
			typeDeclaration.UserData.Add(surrogateDataKey, dataContractSet.GetSurrogateData(dataContract));
		}
		contractCodeDomInfo.TypeDeclaration = typeDeclaration;
	}

	private CodeTypeDeclaration GetNestedType(DataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		if (!SupportsNestedTypes)
		{
			return null;
		}
		string name = dataContract.StableName.Name;
		int num = name.LastIndexOf('.');
		if (num <= 0)
		{
			return null;
		}
		string name2 = name.Substring(0, num);
		DataContract dataContract2 = dataContractSet[new XmlQualifiedName(name2, dataContract.StableName.Namespace)];
		if (dataContract2 == null)
		{
			return null;
		}
		string identifier = XmlConvert.DecodeName(name.Substring(num + 1));
		identifier = GetClrIdentifier(identifier, "GeneratedType");
		ContractCodeDomInfo contractCodeDomInfo2 = GetContractCodeDomInfo(dataContract2);
		GenerateType(dataContract2, contractCodeDomInfo2);
		if (contractCodeDomInfo2.ReferencedTypeExists)
		{
			return null;
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo2.TypeDeclaration;
		if (TypeContainsNestedType(typeDeclaration, identifier))
		{
			int num2 = 1;
			string text;
			while (true)
			{
				text = AppendToValidClrIdentifier(identifier, num2.ToString(NumberFormatInfo.InvariantInfo));
				if (!TypeContainsNestedType(typeDeclaration, text))
				{
					break;
				}
				num2++;
			}
			identifier = text;
		}
		CodeTypeDeclaration codeTypeDeclaration = CreateTypeDeclaration(identifier, dataContract);
		typeDeclaration.Members.Add(codeTypeDeclaration);
		contractCodeDomInfo.TypeReference = new CodeTypeReference(contractCodeDomInfo2.TypeReference.BaseType + "+" + identifier);
		if (GenerateInternalTypes)
		{
			codeTypeDeclaration.TypeAttributes = TypeAttributes.NestedAssembly;
		}
		else
		{
			codeTypeDeclaration.TypeAttributes = TypeAttributes.NestedPublic;
		}
		return codeTypeDeclaration;
	}

	private static CodeTypeDeclaration CreateTypeDeclaration(string typeName, DataContract dataContract)
	{
		CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(typeName);
		CodeAttributeDeclaration value = new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName);
		CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(typeof(GeneratedCodeAttribute).FullName);
		AssemblyName name = Assembly.GetExecutingAssembly().GetName();
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Name)));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Version.ToString())));
		if (!(dataContract is EnumDataContract))
		{
			codeTypeDeclaration.CustomAttributes.Add(value);
		}
		codeTypeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
		return codeTypeDeclaration;
	}

	[SecuritySafeCritical]
	private CodeTypeReference GetReferencedType(DataContract dataContract)
	{
		Type type = null;
		CodeTypeReference surrogatedTypeReference = GetSurrogatedTypeReference(dataContract);
		if (surrogatedTypeReference != null)
		{
			return surrogatedTypeReference;
		}
		if (TryGetReferencedType(dataContract.StableName, dataContract, out type) && !type.IsGenericTypeDefinition && !type.ContainsGenericParameters)
		{
			if (dataContract is XmlDataContract)
			{
				if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
				{
					XmlDataContract xmlDataContract = (XmlDataContract)dataContract;
					if (xmlDataContract.IsTypeDefinedOnImport)
					{
						if (!xmlDataContract.Equals(dataContractSet.GetDataContract(type)))
						{
							throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Referenced type '{0}' does not match the expected type '{1}' in '{2}' namespace.", type.AssemblyQualifiedName, dataContract.StableName.Name, dataContract.StableName.Namespace)));
						}
					}
					else
					{
						xmlDataContract.IsValueType = type.IsValueType;
						xmlDataContract.IsTypeDefinedOnImport = true;
					}
					return GetCodeTypeReference(type);
				}
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Type '{0}' must be IXmlSerializable. Contract type: '{1}', contract name: '{2}' in '{3}' namespace.", DataContract.GetClrTypeFullName(type), DataContract.GetClrTypeFullName(Globals.TypeOfIXmlSerializable), dataContract.StableName.Name, dataContract.StableName.Namespace)));
			}
			if (dataContractSet.GetDataContract(type).Equals(dataContract))
			{
				surrogatedTypeReference = GetCodeTypeReference(type);
				surrogatedTypeReference.UserData.Add(codeUserDataActualTypeKey, type);
				return surrogatedTypeReference;
			}
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Referenced type '{0}' does not match the expected type '{1}' in '{2}' namespace.", type.AssemblyQualifiedName, dataContract.StableName.Name, dataContract.StableName.Namespace)));
		}
		if (dataContract.GenericInfo != null)
		{
			XmlQualifiedName expandedStableName = dataContract.GenericInfo.GetExpandedStableName();
			if (expandedStableName != dataContract.StableName)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Generic type name mismatch. Expected '{0}' in '{1}' namespace, got '{2}' in '{3}' namespace instead.", dataContract.StableName.Name, dataContract.StableName.Namespace, expandedStableName.Name, expandedStableName.Namespace)));
			}
			surrogatedTypeReference = GetReferencedGenericType(dataContract.GenericInfo, out var dataContract2);
			if (dataContract2 != null && !dataContract2.Equals(dataContract))
			{
				type = (Type)surrogatedTypeReference.UserData[codeUserDataActualTypeKey];
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Referenced type '{0}' does not match the expected type '{1}' in '{2}' namespace.", type.AssemblyQualifiedName, dataContract2.StableName.Name, dataContract2.StableName.Namespace)));
			}
			return surrogatedTypeReference;
		}
		return GetReferencedCollectionType(dataContract as CollectionDataContract);
	}

	private CodeTypeReference GetReferencedCollectionType(CollectionDataContract collectionContract)
	{
		if (collectionContract == null)
		{
			return null;
		}
		if (HasDefaultCollectionNames(collectionContract))
		{
			if (!TryGetReferencedDictionaryType(collectionContract, out var typeReference))
			{
				DataContract itemContract = collectionContract.ItemContract;
				if (collectionContract.IsDictionary)
				{
					GenerateKeyValueType(itemContract as ClassDataContract);
				}
				bool isItemTypeNullable = collectionContract.IsItemTypeNullable;
				if (!TryGetReferencedListType(itemContract, isItemTypeNullable, out typeReference))
				{
					return new CodeTypeReference(GetElementTypeReference(itemContract, isItemTypeNullable), 1);
				}
			}
			return typeReference;
		}
		return null;
	}

	private bool HasDefaultCollectionNames(CollectionDataContract collectionContract)
	{
		DataContract itemContract = collectionContract.ItemContract;
		if (collectionContract.ItemName != itemContract.StableName.Name)
		{
			return false;
		}
		if (collectionContract.IsDictionary && (collectionContract.KeyName != "Key" || collectionContract.ValueName != "Value"))
		{
			return false;
		}
		XmlQualifiedName arrayTypeName = itemContract.GetArrayTypeName(collectionContract.IsItemTypeNullable);
		if (collectionContract.StableName.Name == arrayTypeName.Name)
		{
			return collectionContract.StableName.Namespace == arrayTypeName.Namespace;
		}
		return false;
	}

	private bool TryGetReferencedDictionaryType(CollectionDataContract collectionContract, out CodeTypeReference typeReference)
	{
		if (collectionContract.IsDictionary && SupportsGenericTypeReference)
		{
			if (!TryGetReferencedType(GenericDictionaryName, GenericDictionaryContract, out var type))
			{
				type = Globals.TypeOfDictionaryGeneric;
			}
			ClassDataContract obj = collectionContract.ItemContract as ClassDataContract;
			DataMember dataMember = obj.Members[0];
			DataMember dataMember2 = obj.Members[1];
			CodeTypeReference elementTypeReference = GetElementTypeReference(dataMember.MemberTypeContract, dataMember.IsNullable);
			CodeTypeReference elementTypeReference2 = GetElementTypeReference(dataMember2.MemberTypeContract, dataMember2.IsNullable);
			if (elementTypeReference != null && elementTypeReference2 != null)
			{
				typeReference = GetCodeTypeReference(type);
				typeReference.TypeArguments.Add(elementTypeReference);
				typeReference.TypeArguments.Add(elementTypeReference2);
				return true;
			}
		}
		typeReference = null;
		return false;
	}

	private bool TryGetReferencedListType(DataContract itemContract, bool isItemTypeNullable, out CodeTypeReference typeReference)
	{
		if (SupportsGenericTypeReference && TryGetReferencedType(GenericListName, GenericListContract, out var type))
		{
			typeReference = GetCodeTypeReference(type);
			typeReference.TypeArguments.Add(GetElementTypeReference(itemContract, isItemTypeNullable));
			return true;
		}
		typeReference = null;
		return false;
	}

	private CodeTypeReference GetSurrogatedTypeReference(DataContract dataContract)
	{
		IDataContractSurrogate dataContractSurrogate = dataContractSet.DataContractSurrogate;
		if (dataContractSurrogate != null)
		{
			Type referencedTypeOnImport = DataContractSurrogateCaller.GetReferencedTypeOnImport(dataContractSurrogate, dataContract.StableName.Name, dataContract.StableName.Namespace, dataContractSet.GetSurrogateData(dataContract));
			if (referencedTypeOnImport != null)
			{
				CodeTypeReference codeTypeReference = GetCodeTypeReference(referencedTypeOnImport);
				codeTypeReference.UserData.Add(codeUserDataActualTypeKey, referencedTypeOnImport);
				return codeTypeReference;
			}
		}
		return null;
	}

	private CodeTypeReference GetReferencedGenericType(GenericInfo genInfo, out DataContract dataContract)
	{
		dataContract = null;
		if (!SupportsGenericTypeReference)
		{
			return null;
		}
		if (!TryGetReferencedType(genInfo.StableName, null, out var type))
		{
			if (genInfo.Parameters != null)
			{
				return null;
			}
			dataContract = dataContractSet[genInfo.StableName];
			if (dataContract == null)
			{
				return null;
			}
			if (dataContract.GenericInfo != null)
			{
				return null;
			}
			return GetCodeTypeReference(dataContract);
		}
		bool flag = type != Globals.TypeOfNullable;
		CodeTypeReference codeTypeReference = GetCodeTypeReference(type);
		codeTypeReference.UserData.Add(codeUserDataActualTypeKey, type);
		if (genInfo.Parameters != null)
		{
			DataContract[] array = new DataContract[genInfo.Parameters.Count];
			for (int i = 0; i < genInfo.Parameters.Count; i++)
			{
				GenericInfo genericInfo = genInfo.Parameters[i];
				XmlQualifiedName expandedStableName = genericInfo.GetExpandedStableName();
				DataContract dataContract2 = dataContractSet[expandedStableName];
				CodeTypeReference codeTypeReference2;
				bool flag2;
				if (dataContract2 != null)
				{
					codeTypeReference2 = GetCodeTypeReference(dataContract2);
					flag2 = dataContract2.IsValueType;
				}
				else
				{
					codeTypeReference2 = GetReferencedGenericType(genericInfo, out dataContract2);
					flag2 = codeTypeReference2 != null && codeTypeReference2.ArrayRank == 0;
				}
				array[i] = dataContract2;
				if (dataContract2 == null)
				{
					flag = false;
				}
				if (codeTypeReference2 == null)
				{
					return null;
				}
				if (type == Globals.TypeOfNullable && !flag2)
				{
					return codeTypeReference2;
				}
				codeTypeReference.TypeArguments.Add(codeTypeReference2);
			}
			if (flag)
			{
				dataContract = DataContract.GetDataContract(type).BindGenericParameters(array, new Dictionary<DataContract, DataContract>());
			}
		}
		return codeTypeReference;
	}

	private bool NamespaceContainsType(CodeNamespace ns, string typeName)
	{
		foreach (CodeTypeDeclaration type in ns.Types)
		{
			if (string.Compare(typeName, type.Name, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool GlobalTypeNameConflicts(string clrNamespace, string typeName)
	{
		if (string.IsNullOrEmpty(clrNamespace))
		{
			return clrNamespaces.ContainsKey(typeName);
		}
		return false;
	}

	private void AddGlobalTypeName(string typeName)
	{
		if (!clrNamespaces.ContainsKey(typeName))
		{
			clrNamespaces.Add(typeName, null);
		}
	}

	private bool TypeContainsNestedType(CodeTypeDeclaration containingType, string typeName)
	{
		foreach (CodeTypeMember member in containingType.Members)
		{
			if (member is CodeTypeDeclaration && string.Compare(typeName, ((CodeTypeDeclaration)member).Name, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		return false;
	}

	private string GetNameForAttribute(string name)
	{
		string text = XmlConvert.DecodeName(name);
		if (string.CompareOrdinal(name, text) == 0)
		{
			return name;
		}
		string strB = DataContract.EncodeLocalName(text);
		if (string.CompareOrdinal(name, strB) != 0)
		{
			return name;
		}
		return text;
	}

	private void AddSerializableAttribute(bool generateSerializable, CodeTypeDeclaration type, ContractCodeDomInfo contractCodeDomInfo)
	{
		if (generateSerializable)
		{
			type.CustomAttributes.Add(SerializableAttribute);
			AddImportStatement(Globals.TypeOfSerializableAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
		}
	}

	private void ExportClassDataContract(ClassDataContract classDataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		GenerateType(classDataContract, contractCodeDomInfo);
		if (contractCodeDomInfo.ReferencedTypeExists)
		{
			return;
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
		if (SupportsPartialTypes)
		{
			typeDeclaration.IsPartial = true;
		}
		if (classDataContract.IsValueType && SupportsDeclareValueTypes)
		{
			typeDeclaration.IsStruct = true;
		}
		else
		{
			typeDeclaration.IsClass = true;
		}
		string nameForAttribute = GetNameForAttribute(classDataContract.StableName.Name);
		CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfDataContractAttribute));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute)));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(classDataContract.StableName.Namespace)));
		if (classDataContract.IsReference)
		{
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsReference", new CodePrimitiveExpression(classDataContract.IsReference)));
		}
		typeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
		AddImportStatement(Globals.TypeOfDataContractAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
		AddSerializableAttribute(GenerateSerializableTypes, typeDeclaration, contractCodeDomInfo);
		AddKnownTypes(classDataContract, contractCodeDomInfo);
		bool raisePropertyChanged = EnableDataBinding && SupportsDeclareEvents;
		if (classDataContract.BaseContract == null)
		{
			if (!typeDeclaration.IsStruct)
			{
				typeDeclaration.BaseTypes.Add(Globals.TypeOfObject);
			}
			AddExtensionData(contractCodeDomInfo);
			AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
		}
		else
		{
			ContractCodeDomInfo contractCodeDomInfo2 = GetContractCodeDomInfo(classDataContract.BaseContract);
			typeDeclaration.BaseTypes.Add(contractCodeDomInfo2.TypeReference);
			AddBaseMemberNames(contractCodeDomInfo2, contractCodeDomInfo);
			if (contractCodeDomInfo2.ReferencedTypeExists)
			{
				Type type = (Type)contractCodeDomInfo2.TypeReference.UserData[codeUserDataActualTypeKey];
				ThrowIfReferencedBaseTypeSealed(type, classDataContract);
				if (!Globals.TypeOfIExtensibleDataObject.IsAssignableFrom(type))
				{
					AddExtensionData(contractCodeDomInfo);
				}
				if (!Globals.TypeOfIPropertyChange.IsAssignableFrom(type))
				{
					AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
				}
				else
				{
					raisePropertyChanged = false;
				}
			}
		}
		if (classDataContract.Members == null)
		{
			return;
		}
		for (int i = 0; i < classDataContract.Members.Count; i++)
		{
			DataMember dataMember = classDataContract.Members[i];
			CodeTypeReference elementTypeReference = GetElementTypeReference(dataMember.MemberTypeContract, dataMember.IsNullable && dataMember.MemberTypeContract.IsValueType);
			string nameForAttribute2 = GetNameForAttribute(dataMember.Name);
			string memberName = GetMemberName(nameForAttribute2, contractCodeDomInfo);
			string memberName2 = GetMemberName(AppendToValidClrIdentifier(memberName, "Field"), contractCodeDomInfo);
			CodeMemberField codeMemberField = new CodeMemberField();
			codeMemberField.Type = elementTypeReference;
			codeMemberField.Name = memberName2;
			codeMemberField.Attributes = MemberAttributes.Private;
			CodeMemberProperty codeMemberProperty = CreateProperty(elementTypeReference, memberName, memberName2, dataMember.MemberTypeContract.IsValueType && SupportsDeclareValueTypes, raisePropertyChanged);
			if (dataContractSet.DataContractSurrogate != null)
			{
				codeMemberProperty.UserData.Add(surrogateDataKey, dataContractSet.GetSurrogateData(dataMember));
			}
			CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfDataMemberAttribute));
			if (nameForAttribute2 != codeMemberProperty.Name)
			{
				codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute2)));
			}
			if (dataMember.IsRequired)
			{
				codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("IsRequired", new CodePrimitiveExpression(dataMember.IsRequired)));
			}
			if (!dataMember.EmitDefaultValue)
			{
				codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("EmitDefaultValue", new CodePrimitiveExpression(dataMember.EmitDefaultValue)));
			}
			if (dataMember.Order != 0)
			{
				codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(dataMember.Order)));
			}
			codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration2);
			if (GenerateSerializableTypes && !dataMember.IsRequired)
			{
				CodeAttributeDeclaration value = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfOptionalFieldAttribute));
				codeMemberField.CustomAttributes.Add(value);
			}
			typeDeclaration.Members.Add(codeMemberField);
			typeDeclaration.Members.Add(codeMemberProperty);
		}
	}

	private bool CanDeclareAssemblyAttribute(ContractCodeDomInfo contractCodeDomInfo)
	{
		if (SupportsAssemblyAttributes)
		{
			return !contractCodeDomInfo.UsesWildcardNamespace;
		}
		return false;
	}

	private bool NeedsExplicitNamespace(string dataContractNamespace, string clrNamespace)
	{
		return DataContract.GetDefaultStableNamespace(clrNamespace) != dataContractNamespace;
	}

	internal ICollection<CodeTypeReference> GetKnownTypeReferences(DataContract dataContract)
	{
		Dictionary<XmlQualifiedName, DataContract> knownTypeContracts = GetKnownTypeContracts(dataContract);
		if (knownTypeContracts == null)
		{
			return null;
		}
		ICollection<DataContract> values = knownTypeContracts.Values;
		if (values == null || values.Count == 0)
		{
			return null;
		}
		List<CodeTypeReference> list = new List<CodeTypeReference>();
		foreach (DataContract item in values)
		{
			list.Add(GetCodeTypeReference(item));
		}
		return list;
	}

	private Dictionary<XmlQualifiedName, DataContract> GetKnownTypeContracts(DataContract dataContract)
	{
		if (dataContractSet.KnownTypesForObject != null && SchemaImporter.IsObjectContract(dataContract))
		{
			return dataContractSet.KnownTypesForObject;
		}
		if (dataContract is ClassDataContract)
		{
			ContractCodeDomInfo contractCodeDomInfo = GetContractCodeDomInfo(dataContract);
			if (!contractCodeDomInfo.IsProcessed)
			{
				GenerateType(dataContract, contractCodeDomInfo);
			}
			if (contractCodeDomInfo.ReferencedTypeExists)
			{
				return GetKnownTypeContracts((ClassDataContract)dataContract, new Dictionary<DataContract, object>());
			}
		}
		return null;
	}

	private Dictionary<XmlQualifiedName, DataContract> GetKnownTypeContracts(ClassDataContract dataContract, Dictionary<DataContract, object> handledContracts)
	{
		if (handledContracts.ContainsKey(dataContract))
		{
			return dataContract.KnownDataContracts;
		}
		handledContracts.Add(dataContract, null);
		if (dataContract.Members != null)
		{
			bool flag = false;
			foreach (DataMember member in dataContract.Members)
			{
				DataContract memberTypeContract = member.MemberTypeContract;
				if (!flag && dataContractSet.KnownTypesForObject != null && SchemaImporter.IsObjectContract(memberTypeContract))
				{
					AddKnownTypeContracts(dataContract, dataContractSet.KnownTypesForObject);
					flag = true;
				}
				else if (memberTypeContract is ClassDataContract)
				{
					ContractCodeDomInfo contractCodeDomInfo = GetContractCodeDomInfo(memberTypeContract);
					if (!contractCodeDomInfo.IsProcessed)
					{
						GenerateType(memberTypeContract, contractCodeDomInfo);
					}
					if (contractCodeDomInfo.ReferencedTypeExists)
					{
						AddKnownTypeContracts(dataContract, GetKnownTypeContracts((ClassDataContract)memberTypeContract, handledContracts));
					}
				}
			}
		}
		return dataContract.KnownDataContracts;
	}

	[SecuritySafeCritical]
	private void AddKnownTypeContracts(ClassDataContract dataContract, Dictionary<XmlQualifiedName, DataContract> knownContracts)
	{
		if (knownContracts == null || knownContracts.Count == 0)
		{
			return;
		}
		if (dataContract.KnownDataContracts == null)
		{
			dataContract.KnownDataContracts = new Dictionary<XmlQualifiedName, DataContract>();
		}
		foreach (KeyValuePair<XmlQualifiedName, DataContract> knownContract in knownContracts)
		{
			if (dataContract.StableName != knownContract.Key && !dataContract.KnownDataContracts.ContainsKey(knownContract.Key) && !knownContract.Value.IsBuiltInDataContract)
			{
				dataContract.KnownDataContracts.Add(knownContract.Key, knownContract.Value);
			}
		}
	}

	private void AddKnownTypes(ClassDataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		Dictionary<XmlQualifiedName, DataContract> knownTypeContracts = GetKnownTypeContracts(dataContract, new Dictionary<DataContract, object>());
		if (knownTypeContracts == null || knownTypeContracts.Count == 0)
		{
			return;
		}
		foreach (DataContract item in (IEnumerable<DataContract>)knownTypeContracts.Values)
		{
			CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfKnownTypeAttribute));
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(GetCodeTypeReference(item))));
			contractCodeDomInfo.TypeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
		}
		AddImportStatement(Globals.TypeOfKnownTypeAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
	}

	private CodeTypeReference WrapNullable(CodeTypeReference memberType)
	{
		if (!SupportsGenericTypeReference)
		{
			return memberType;
		}
		CodeTypeReference codeTypeReference = GetCodeTypeReference(Globals.TypeOfNullable);
		codeTypeReference.TypeArguments.Add(memberType);
		return codeTypeReference;
	}

	private void AddExtensionData(ContractCodeDomInfo contractCodeDomInfo)
	{
		if (contractCodeDomInfo != null && contractCodeDomInfo.TypeDeclaration != null)
		{
			CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
			typeDeclaration.BaseTypes.Add(DataContract.GetClrTypeFullName(Globals.TypeOfIExtensibleDataObject));
			CodeMemberField extensionDataObjectField = ExtensionDataObjectField;
			if (GenerateSerializableTypes)
			{
				CodeAttributeDeclaration value = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfNonSerializedAttribute));
				extensionDataObjectField.CustomAttributes.Add(value);
			}
			typeDeclaration.Members.Add(extensionDataObjectField);
			contractCodeDomInfo.GetMemberNames().Add(extensionDataObjectField.Name, null);
			CodeMemberProperty extensionDataObjectProperty = ExtensionDataObjectProperty;
			typeDeclaration.Members.Add(extensionDataObjectProperty);
			contractCodeDomInfo.GetMemberNames().Add(extensionDataObjectProperty.Name, null);
		}
	}

	private void AddPropertyChangedNotifier(ContractCodeDomInfo contractCodeDomInfo, bool isValueType)
	{
		if (EnableDataBinding && SupportsDeclareEvents && contractCodeDomInfo != null && contractCodeDomInfo.TypeDeclaration != null)
		{
			CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
			typeDeclaration.BaseTypes.Add(CodeTypeIPropertyChange);
			CodeMemberEvent propertyChangedEvent = PropertyChangedEvent;
			typeDeclaration.Members.Add(propertyChangedEvent);
			CodeMemberMethod raisePropertyChangedEventMethod = RaisePropertyChangedEventMethod;
			if (!isValueType)
			{
				raisePropertyChangedEventMethod.Attributes |= MemberAttributes.Family;
			}
			typeDeclaration.Members.Add(raisePropertyChangedEventMethod);
			contractCodeDomInfo.GetMemberNames().Add(propertyChangedEvent.Name, null);
			contractCodeDomInfo.GetMemberNames().Add(raisePropertyChangedEventMethod.Name, null);
		}
	}

	private void ThrowIfReferencedBaseTypeSealed(Type baseType, DataContract dataContract)
	{
		if (baseType.IsSealed)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Cannod drive from sealed reference type '{2}', for '{0}' element in '{1}' namespace.", dataContract.StableName.Name, dataContract.StableName.Namespace, DataContract.GetClrTypeFullName(baseType))));
		}
	}

	private void ExportEnumDataContract(EnumDataContract enumDataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		GenerateType(enumDataContract, contractCodeDomInfo);
		if (contractCodeDomInfo.ReferencedTypeExists)
		{
			return;
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
		typeDeclaration.IsEnum = true;
		typeDeclaration.BaseTypes.Add(EnumDataContract.GetBaseType(enumDataContract.BaseContractName));
		if (enumDataContract.IsFlags)
		{
			typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfFlagsAttribute)));
			AddImportStatement(Globals.TypeOfFlagsAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
		}
		string nameForAttribute = GetNameForAttribute(enumDataContract.StableName.Name);
		CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfDataContractAttribute));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute)));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(enumDataContract.StableName.Namespace)));
		typeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
		AddImportStatement(Globals.TypeOfDataContractAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
		if (enumDataContract.Members == null)
		{
			return;
		}
		for (int i = 0; i < enumDataContract.Members.Count; i++)
		{
			string name = enumDataContract.Members[i].Name;
			long num = enumDataContract.Values[i];
			CodeMemberField codeMemberField = new CodeMemberField();
			if (enumDataContract.IsULong)
			{
				codeMemberField.InitExpression = new CodeSnippetExpression(enumDataContract.GetStringFromEnumValue(num));
			}
			else
			{
				codeMemberField.InitExpression = new CodePrimitiveExpression(num);
			}
			codeMemberField.Name = GetMemberName(name, contractCodeDomInfo);
			CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfEnumMemberAttribute));
			if (codeMemberField.Name != name)
			{
				codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Value", new CodePrimitiveExpression(name)));
			}
			codeMemberField.CustomAttributes.Add(codeAttributeDeclaration2);
			typeDeclaration.Members.Add(codeMemberField);
		}
	}

	private void ExportISerializableDataContract(ClassDataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		GenerateType(dataContract, contractCodeDomInfo);
		if (contractCodeDomInfo.ReferencedTypeExists)
		{
			return;
		}
		if (DataContract.GetDefaultStableNamespace(contractCodeDomInfo.ClrNamespace) != dataContract.StableName.Namespace)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Invalid CLR namespace '{3}' is generated for ISerializable type '{0}' in '{1}' namespace. Data contract namespace from the URI would be generated as '{2}'.", dataContract.StableName.Name, dataContract.StableName.Namespace, DataContract.GetDataContractNamespaceFromUri(dataContract.StableName.Namespace), contractCodeDomInfo.ClrNamespace)));
		}
		string nameForAttribute = GetNameForAttribute(dataContract.StableName.Name);
		int num = nameForAttribute.LastIndexOf('.');
		string text = ((num <= 0 || num == nameForAttribute.Length - 1) ? nameForAttribute : nameForAttribute.Substring(num + 1));
		if (contractCodeDomInfo.TypeDeclaration.Name != text)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Invalid CLR name '{2}' is generated for ISerializable type '{0}' in '{1}' namespace.", dataContract.StableName.Name, dataContract.StableName.Namespace, contractCodeDomInfo.TypeDeclaration.Name)));
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
		if (SupportsPartialTypes)
		{
			typeDeclaration.IsPartial = true;
		}
		if (dataContract.IsValueType && SupportsDeclareValueTypes)
		{
			typeDeclaration.IsStruct = true;
		}
		else
		{
			typeDeclaration.IsClass = true;
		}
		AddSerializableAttribute(generateSerializable: true, typeDeclaration, contractCodeDomInfo);
		AddKnownTypes(dataContract, contractCodeDomInfo);
		if (dataContract.BaseContract == null)
		{
			if (!typeDeclaration.IsStruct)
			{
				typeDeclaration.BaseTypes.Add(Globals.TypeOfObject);
			}
			typeDeclaration.BaseTypes.Add(DataContract.GetClrTypeFullName(Globals.TypeOfISerializable));
			typeDeclaration.Members.Add(ISerializableBaseConstructor);
			typeDeclaration.Members.Add(SerializationInfoField);
			typeDeclaration.Members.Add(SerializationInfoProperty);
			typeDeclaration.Members.Add(GetObjectDataMethod);
			AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
		}
		else
		{
			ContractCodeDomInfo contractCodeDomInfo2 = GetContractCodeDomInfo(dataContract.BaseContract);
			GenerateType(dataContract.BaseContract, contractCodeDomInfo2);
			typeDeclaration.BaseTypes.Add(contractCodeDomInfo2.TypeReference);
			if (contractCodeDomInfo2.ReferencedTypeExists)
			{
				Type baseType = (Type)contractCodeDomInfo2.TypeReference.UserData[codeUserDataActualTypeKey];
				ThrowIfReferencedBaseTypeSealed(baseType, dataContract);
			}
			typeDeclaration.Members.Add(ISerializableDerivedConstructor);
		}
	}

	private void GenerateKeyValueType(ClassDataContract keyValueContract)
	{
		if (keyValueContract != null && dataContractSet[keyValueContract.StableName] == null)
		{
			ContractCodeDomInfo contractCodeDomInfo = dataContractSet.GetContractCodeDomInfo(keyValueContract);
			if (contractCodeDomInfo == null)
			{
				contractCodeDomInfo = new ContractCodeDomInfo();
				dataContractSet.SetContractCodeDomInfo(keyValueContract, contractCodeDomInfo);
				ExportClassDataContract(keyValueContract, contractCodeDomInfo);
				contractCodeDomInfo.IsProcessed = true;
			}
		}
	}

	private void ExportCollectionDataContract(CollectionDataContract collectionContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		GenerateType(collectionContract, contractCodeDomInfo);
		if (contractCodeDomInfo.ReferencedTypeExists)
		{
			return;
		}
		string nameForAttribute = GetNameForAttribute(collectionContract.StableName.Name);
		if (!SupportsGenericTypeReference)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("For '{0}' in '{1}' namespace, generic type cannot be referenced as the base type.", nameForAttribute, collectionContract.StableName.Namespace)));
		}
		DataContract itemContract = collectionContract.ItemContract;
		bool isItemTypeNullable = collectionContract.IsItemTypeNullable;
		CodeTypeReference typeReference;
		bool flag = TryGetReferencedDictionaryType(collectionContract, out typeReference);
		if (!flag)
		{
			if (collectionContract.IsDictionary)
			{
				GenerateKeyValueType(collectionContract.ItemContract as ClassDataContract);
			}
			if (!TryGetReferencedListType(itemContract, isItemTypeNullable, out typeReference))
			{
				if (!SupportsGenericTypeReference)
				{
					string text = "ArrayOf" + itemContract.StableName.Name;
					string collectionNamespace = DataContract.GetCollectionNamespace(itemContract.StableName.Namespace);
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Referenced base type does not exist. Data contract name: '{0}' in '{1}' namespace, expected type: '{2}' in '{3}' namespace. Collection can be '{4}' or '{5}'.", nameForAttribute, collectionContract.StableName.Namespace, text, collectionNamespace, DataContract.GetClrTypeFullName(Globals.TypeOfIListGeneric), DataContract.GetClrTypeFullName(Globals.TypeOfICollectionGeneric))));
				}
				typeReference = GetCodeTypeReference(Globals.TypeOfListGeneric);
				typeReference.TypeArguments.Add(GetElementTypeReference(itemContract, isItemTypeNullable));
			}
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
		typeDeclaration.BaseTypes.Add(typeReference);
		CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfCollectionDataContractAttribute));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(nameForAttribute)));
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(collectionContract.StableName.Namespace)));
		if (collectionContract.IsReference)
		{
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsReference", new CodePrimitiveExpression(collectionContract.IsReference)));
		}
		codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("ItemName", new CodePrimitiveExpression(GetNameForAttribute(collectionContract.ItemName))));
		if (flag)
		{
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("KeyName", new CodePrimitiveExpression(GetNameForAttribute(collectionContract.KeyName))));
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("ValueName", new CodePrimitiveExpression(GetNameForAttribute(collectionContract.ValueName))));
		}
		typeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
		AddImportStatement(Globals.TypeOfCollectionDataContractAttribute.Namespace, contractCodeDomInfo.CodeNamespace);
		AddSerializableAttribute(GenerateSerializableTypes, typeDeclaration, contractCodeDomInfo);
	}

	private void ExportXmlDataContract(XmlDataContract xmlDataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		GenerateType(xmlDataContract, contractCodeDomInfo);
		if (contractCodeDomInfo.ReferencedTypeExists)
		{
			return;
		}
		CodeTypeDeclaration typeDeclaration = contractCodeDomInfo.TypeDeclaration;
		if (SupportsPartialTypes)
		{
			typeDeclaration.IsPartial = true;
		}
		if (xmlDataContract.IsValueType)
		{
			typeDeclaration.IsStruct = true;
		}
		else
		{
			typeDeclaration.IsClass = true;
			typeDeclaration.BaseTypes.Add(Globals.TypeOfObject);
		}
		AddSerializableAttribute(GenerateSerializableTypes, typeDeclaration, contractCodeDomInfo);
		typeDeclaration.BaseTypes.Add(DataContract.GetClrTypeFullName(Globals.TypeOfIXmlSerializable));
		typeDeclaration.Members.Add(NodeArrayField);
		typeDeclaration.Members.Add(NodeArrayProperty);
		typeDeclaration.Members.Add(ReadXmlMethod);
		typeDeclaration.Members.Add(WriteXmlMethod);
		typeDeclaration.Members.Add(GetSchemaMethod);
		if (xmlDataContract.IsAnonymous && !xmlDataContract.HasRoot)
		{
			typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfXmlSchemaProviderAttribute), new CodeAttributeArgument(NullReference), new CodeAttributeArgument("IsAny", new CodePrimitiveExpression(true))));
		}
		else
		{
			typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfXmlSchemaProviderAttribute), new CodeAttributeArgument(new CodePrimitiveExpression("ExportSchema"))));
			CodeMemberField codeMemberField = new CodeMemberField(Globals.TypeOfXmlQualifiedName, typeNameFieldName);
			codeMemberField.Attributes |= (MemberAttributes)20483;
			XmlQualifiedName xmlQualifiedName = (xmlDataContract.IsAnonymous ? SchemaImporter.ImportActualType(xmlDataContract.XsdType.Annotation, xmlDataContract.StableName, xmlDataContract.StableName) : xmlDataContract.StableName);
			codeMemberField.InitExpression = new CodeObjectCreateExpression(Globals.TypeOfXmlQualifiedName, new CodePrimitiveExpression(xmlQualifiedName.Name), new CodePrimitiveExpression(xmlQualifiedName.Namespace));
			typeDeclaration.Members.Add(codeMemberField);
			typeDeclaration.Members.Add(GetSchemaStaticMethod);
			bool flag = (xmlDataContract.TopLevelElementName != null && xmlDataContract.TopLevelElementName.Value != xmlDataContract.StableName.Name) || (xmlDataContract.TopLevelElementNamespace != null && xmlDataContract.TopLevelElementNamespace.Value != xmlDataContract.StableName.Namespace);
			if (flag || !xmlDataContract.IsTopLevelElementNullable)
			{
				CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfXmlRootAttribute));
				if (flag)
				{
					if (xmlDataContract.TopLevelElementName != null)
					{
						codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("ElementName", new CodePrimitiveExpression(xmlDataContract.TopLevelElementName.Value)));
					}
					if (xmlDataContract.TopLevelElementNamespace != null)
					{
						codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(xmlDataContract.TopLevelElementNamespace.Value)));
					}
				}
				if (!xmlDataContract.IsTopLevelElementNullable)
				{
					codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsNullable", new CodePrimitiveExpression(false)));
				}
				typeDeclaration.CustomAttributes.Add(codeAttributeDeclaration);
			}
		}
		AddPropertyChangedNotifier(contractCodeDomInfo, typeDeclaration.IsStruct);
	}

	private CodeNamespace GetCodeNamespace(string clrNamespace, string dataContractNamespace, ContractCodeDomInfo contractCodeDomInfo)
	{
		if (contractCodeDomInfo.CodeNamespace != null)
		{
			return contractCodeDomInfo.CodeNamespace;
		}
		CodeNamespaceCollection codeNamespaceCollection = codeCompileUnit.Namespaces;
		foreach (CodeNamespace item in codeNamespaceCollection)
		{
			if (item.Name == clrNamespace)
			{
				contractCodeDomInfo.CodeNamespace = item;
				return item;
			}
		}
		CodeNamespace codeNamespace2 = new CodeNamespace(clrNamespace);
		codeNamespaceCollection.Add(codeNamespace2);
		if (CanDeclareAssemblyAttribute(contractCodeDomInfo) && NeedsExplicitNamespace(dataContractNamespace, clrNamespace))
		{
			CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(DataContract.GetClrTypeFullName(Globals.TypeOfContractNamespaceAttribute));
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(dataContractNamespace)));
			codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("ClrNamespace", new CodePrimitiveExpression(clrNamespace)));
			codeCompileUnit.AssemblyCustomAttributes.Add(codeAttributeDeclaration);
		}
		contractCodeDomInfo.CodeNamespace = codeNamespace2;
		return codeNamespace2;
	}

	private string GetMemberName(string memberName, ContractCodeDomInfo contractCodeDomInfo)
	{
		memberName = GetClrIdentifier(memberName, "GeneratedMember");
		if (memberName == contractCodeDomInfo.TypeDeclaration.Name)
		{
			memberName = AppendToValidClrIdentifier(memberName, "Member");
		}
		if (contractCodeDomInfo.GetMemberNames().ContainsKey(memberName))
		{
			string text = null;
			int num = 1;
			while (true)
			{
				text = AppendToValidClrIdentifier(memberName, num.ToString(NumberFormatInfo.InvariantInfo));
				if (!contractCodeDomInfo.GetMemberNames().ContainsKey(text))
				{
					break;
				}
				num++;
			}
			memberName = text;
		}
		contractCodeDomInfo.GetMemberNames().Add(memberName, null);
		return memberName;
	}

	private void AddBaseMemberNames(ContractCodeDomInfo baseContractCodeDomInfo, ContractCodeDomInfo contractCodeDomInfo)
	{
		if (baseContractCodeDomInfo.ReferencedTypeExists)
		{
			return;
		}
		Dictionary<string, object> memberNames = baseContractCodeDomInfo.GetMemberNames();
		Dictionary<string, object> memberNames2 = contractCodeDomInfo.GetMemberNames();
		foreach (KeyValuePair<string, object> item in memberNames)
		{
			memberNames2.Add(item.Key, item.Value);
		}
	}

	[SecuritySafeCritical]
	private static string GetClrIdentifier(string identifier, string defaultIdentifier)
	{
		if (identifier.Length <= 511 && CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
		{
			return identifier;
		}
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < identifier.Length; i++)
		{
			if (stringBuilder.Length >= 511)
			{
				break;
			}
			char c = identifier[i];
			if (IsValid(c))
			{
				if (flag && !IsValidStart(c))
				{
					stringBuilder.Append("_");
				}
				stringBuilder.Append(c);
				flag = false;
			}
		}
		if (stringBuilder.Length == 0)
		{
			return defaultIdentifier;
		}
		return stringBuilder.ToString();
	}

	private static string AppendToValidClrIdentifier(string identifier, string appendString)
	{
		int num = 511 - identifier.Length;
		int length = appendString.Length;
		if (num < length)
		{
			identifier = identifier.Substring(0, 511 - length);
		}
		identifier += appendString;
		return identifier;
	}

	private string GetClrNamespace(DataContract dataContract, ContractCodeDomInfo contractCodeDomInfo)
	{
		string value = contractCodeDomInfo.ClrNamespace;
		bool usesWildcardNamespace = false;
		if (value == null)
		{
			if (!Namespaces.TryGetValue(dataContract.StableName.Namespace, out value))
			{
				if (Namespaces.TryGetValue(wildcardNamespaceMapping, out value))
				{
					usesWildcardNamespace = true;
				}
				else
				{
					value = GetClrNamespace(dataContract.StableName.Namespace);
					if (ClrNamespaces.ContainsKey(value))
					{
						string text = null;
						int num = 1;
						while (true)
						{
							text = ((value.Length == 0) ? "GeneratedNamespace" : value) + num.ToString(NumberFormatInfo.InvariantInfo);
							if (!ClrNamespaces.ContainsKey(text))
							{
								break;
							}
							num++;
						}
						value = text;
					}
					AddNamespacePair(dataContract.StableName.Namespace, value);
				}
			}
			contractCodeDomInfo.ClrNamespace = value;
			contractCodeDomInfo.UsesWildcardNamespace = usesWildcardNamespace;
		}
		return value;
	}

	private void AddNamespacePair(string dataContractNamespace, string clrNamespace)
	{
		Namespaces.Add(dataContractNamespace, clrNamespace);
		ClrNamespaces.Add(clrNamespace, dataContractNamespace);
	}

	private void AddImportStatement(string clrNamespace, CodeNamespace codeNamespace)
	{
		if (clrNamespace == codeNamespace.Name)
		{
			return;
		}
		CodeNamespaceImportCollection imports = codeNamespace.Imports;
		foreach (CodeNamespaceImport item in imports)
		{
			if (item.Namespace == clrNamespace)
			{
				return;
			}
		}
		imports.Add(new CodeNamespaceImport(clrNamespace));
	}

	private static string GetClrNamespace(string dataContractNamespace)
	{
		if (dataContractNamespace == null || dataContractNamespace.Length == 0)
		{
			return string.Empty;
		}
		Uri result = null;
		StringBuilder stringBuilder = new StringBuilder();
		if (Uri.TryCreate(dataContractNamespace, UriKind.RelativeOrAbsolute, out result))
		{
			Dictionary<string, object> fragments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (!result.IsAbsoluteUri)
			{
				AddToNamespace(stringBuilder, result.OriginalString, fragments);
			}
			else
			{
				string absoluteUri = result.AbsoluteUri;
				if (absoluteUri.StartsWith("http://schemas.datacontract.org/2004/07/", StringComparison.Ordinal))
				{
					AddToNamespace(stringBuilder, absoluteUri.Substring("http://schemas.datacontract.org/2004/07/".Length), fragments);
				}
				else
				{
					string host = result.Host;
					if (host != null)
					{
						AddToNamespace(stringBuilder, host, fragments);
					}
					string pathAndQuery = result.PathAndQuery;
					if (pathAndQuery != null)
					{
						AddToNamespace(stringBuilder, pathAndQuery, fragments);
					}
				}
			}
		}
		if (stringBuilder.Length == 0)
		{
			return string.Empty;
		}
		int num = stringBuilder.Length;
		if (stringBuilder[stringBuilder.Length - 1] == '.')
		{
			num--;
		}
		num = Math.Min(511, num);
		return stringBuilder.ToString(0, num);
	}

	private static void AddToNamespace(StringBuilder builder, string fragment, Dictionary<string, object> fragments)
	{
		if (fragment == null)
		{
			return;
		}
		bool flag = true;
		int length = builder.Length;
		int num = 0;
		for (int i = 0; i < fragment.Length; i++)
		{
			if (builder.Length >= 511)
			{
				break;
			}
			char c = fragment[i];
			if (IsValid(c))
			{
				if (flag && !IsValidStart(c))
				{
					builder.Append("_");
				}
				builder.Append(c);
				num++;
				flag = false;
			}
			else if ((c == '.' || c == '/' || c == ':') && (builder.Length == 1 || (builder.Length > 1 && builder[builder.Length - 1] != '.')))
			{
				AddNamespaceFragment(builder, length, num, fragments);
				builder.Append('.');
				length = builder.Length;
				num = 0;
				flag = true;
			}
		}
		AddNamespaceFragment(builder, length, num, fragments);
	}

	private static void AddNamespaceFragment(StringBuilder builder, int fragmentOffset, int fragmentLength, Dictionary<string, object> fragments)
	{
		if (fragmentLength == 0)
		{
			return;
		}
		string text = builder.ToString(fragmentOffset, fragmentLength);
		if (fragments.ContainsKey(text))
		{
			int num = 1;
			string text2;
			string text3;
			while (true)
			{
				text2 = num.ToString(NumberFormatInfo.InvariantInfo);
				text3 = AppendToValidClrIdentifier(text, text2);
				if (!fragments.ContainsKey(text3))
				{
					break;
				}
				if (num == int.MaxValue)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString("Cannot compute unique name for '{0}'.", text)));
				}
				num++;
			}
			builder.Append(text2);
			text = text3;
		}
		fragments.Add(text, null);
	}

	private static bool IsValidStart(char c)
	{
		return char.GetUnicodeCategory(c) != UnicodeCategory.DecimalDigitNumber;
	}

	private static bool IsValid(char c)
	{
		UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
		if ((uint)unicodeCategory <= 6u || unicodeCategory == UnicodeCategory.DecimalDigitNumber || unicodeCategory == UnicodeCategory.ConnectorPunctuation)
		{
			return true;
		}
		return false;
	}

	private CodeMemberProperty CreateProperty(CodeTypeReference type, string propertyName, string fieldName, bool isValueType)
	{
		return CreateProperty(type, propertyName, fieldName, isValueType, EnableDataBinding && SupportsDeclareEvents);
	}

	private CodeMemberProperty CreateProperty(CodeTypeReference type, string propertyName, string fieldName, bool isValueType, bool raisePropertyChanged)
	{
		CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
		codeMemberProperty.Type = type;
		codeMemberProperty.Name = propertyName;
		codeMemberProperty.Attributes = MemberAttributes.Final;
		if (GenerateInternalTypes)
		{
			codeMemberProperty.Attributes |= MemberAttributes.Assembly;
		}
		else
		{
			codeMemberProperty.Attributes |= MemberAttributes.Public;
		}
		CodeMethodReturnStatement codeMethodReturnStatement = new CodeMethodReturnStatement();
		codeMethodReturnStatement.Expression = new CodeFieldReferenceExpression(ThisReference, fieldName);
		codeMemberProperty.GetStatements.Add(codeMethodReturnStatement);
		CodeAssignStatement codeAssignStatement = new CodeAssignStatement();
		codeAssignStatement.Left = new CodeFieldReferenceExpression(ThisReference, fieldName);
		codeAssignStatement.Right = new CodePropertySetValueReferenceExpression();
		if (raisePropertyChanged)
		{
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement();
			CodeExpression codeExpression = new CodeFieldReferenceExpression(ThisReference, fieldName);
			CodeExpression codeExpression2 = new CodePropertySetValueReferenceExpression();
			codeExpression = (isValueType ? new CodeMethodInvokeExpression(codeExpression, "Equals", codeExpression2) : new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(Globals.TypeOfObject), "ReferenceEquals", codeExpression, codeExpression2));
			codeExpression2 = new CodePrimitiveExpression(true);
			codeConditionStatement.Condition = new CodeBinaryOperatorExpression(codeExpression, CodeBinaryOperatorType.IdentityInequality, codeExpression2);
			codeConditionStatement.TrueStatements.Add(codeAssignStatement);
			codeConditionStatement.TrueStatements.Add(new CodeMethodInvokeExpression(ThisReference, RaisePropertyChangedEventMethod.Name, new CodePrimitiveExpression(propertyName)));
			codeMemberProperty.SetStatements.Add(codeConditionStatement);
		}
		else
		{
			codeMemberProperty.SetStatements.Add(codeAssignStatement);
		}
		return codeMemberProperty;
	}
}
