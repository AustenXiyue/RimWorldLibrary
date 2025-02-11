using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.System.Xaml;

namespace System;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(SR)));

	internal static string APSException => GetResourceString("APSException");

	internal static string AddCollection => GetResourceString("AddCollection");

	internal static string AddDictionary => GetResourceString("AddDictionary");

	internal static string AmbiguousCollectionItemType => GetResourceString("AmbiguousCollectionItemType");

	internal static string AmbiguousDictionaryItemType => GetResourceString("AmbiguousDictionaryItemType");

	internal static string Animation_ChildMustBeKeyFrame => GetResourceString("Animation_ChildMustBeKeyFrame");

	internal static string Animation_InvalidAnimationUsingKeyFramesDuration => GetResourceString("Animation_InvalidAnimationUsingKeyFramesDuration");

	internal static string Animation_InvalidBaseValue => GetResourceString("Animation_InvalidBaseValue");

	internal static string Animation_InvalidResolvedKeyTimes => GetResourceString("Animation_InvalidResolvedKeyTimes");

	internal static string Animation_InvalidTimeKeyTime => GetResourceString("Animation_InvalidTimeKeyTime");

	internal static string Animation_Invalid_DefaultValue => GetResourceString("Animation_Invalid_DefaultValue");

	internal static string Animation_NoTextChildren => GetResourceString("Animation_NoTextChildren");

	internal static string ArgumentRequired => GetResourceString("ArgumentRequired");

	internal static string ArrayAddNotImplemented => GetResourceString("ArrayAddNotImplemented");

	internal static string AssemblyTagMissing => GetResourceString("AssemblyTagMissing");

	internal static string AttachableEventNotImplemented => GetResourceString("AttachableEventNotImplemented");

	internal static string AttachableMemberNotFound => GetResourceString("AttachableMemberNotFound");

	internal static string AttachedPropOnFwdRefTC => GetResourceString("AttachedPropOnFwdRefTC");

	internal static string AttachedPropertyOnDictionaryKey => GetResourceString("AttachedPropertyOnDictionaryKey");

	internal static string AttachedPropertyOnTypeConvertedOrStringProperty => GetResourceString("AttachedPropertyOnTypeConvertedOrStringProperty");

	internal static string AttributeUnhandledKind => GetResourceString("AttributeUnhandledKind");

	internal static string BadInternalsVisibleTo1 => GetResourceString("BadInternalsVisibleTo1");

	internal static string BadInternalsVisibleTo2 => GetResourceString("BadInternalsVisibleTo2");

	internal static string BadMethod => GetResourceString("BadMethod");

	internal static string BadStateObjectWriter => GetResourceString("BadStateObjectWriter");

	internal static string BadXmlnsCompat => GetResourceString("BadXmlnsCompat");

	internal static string BadXmlnsDefinition => GetResourceString("BadXmlnsDefinition");

	internal static string BadXmlnsPrefix => GetResourceString("BadXmlnsPrefix");

	internal static string BuilderStackNotEmptyOnClose => GetResourceString("BuilderStackNotEmptyOnClose");

	internal static string CanConvertFromFailed => GetResourceString("CanConvertFromFailed");

	internal static string CanConvertToFailed => GetResourceString("CanConvertToFailed");

	internal static string CannotAddPositionalParameters => GetResourceString("CannotAddPositionalParameters");

	internal static string CannotConvertStringToType => GetResourceString("CannotConvertStringToType");

	internal static string CannotConvertType => GetResourceString("CannotConvertType");

	internal static string CannotCreateBadEventDelegate => GetResourceString("CannotCreateBadEventDelegate");

	internal static string CannotCreateBadType => GetResourceString("CannotCreateBadType");

	internal static string CannotFindAssembly => GetResourceString("CannotFindAssembly");

	internal static string CannotModifyReadOnlyContainer => GetResourceString("CannotModifyReadOnlyContainer");

	internal static string CannotReassignSchemaContext => GetResourceString("CannotReassignSchemaContext");

	internal static string CannotResolveTypeForFactoryMethod => GetResourceString("CannotResolveTypeForFactoryMethod");

	internal static string CannotRetrievePartsOfWriteOnlyContainer => GetResourceString("CannotRetrievePartsOfWriteOnlyContainer");

	internal static string CannotSetBaseUri => GetResourceString("CannotSetBaseUri");

	internal static string CannotSetSchemaContext => GetResourceString("CannotSetSchemaContext");

	internal static string CannotSetSchemaContextNull => GetResourceString("CannotSetSchemaContextNull");

	internal static string CannotWriteClosedWriter => GetResourceString("CannotWriteClosedWriter");

	internal static string CannotWriteXmlSpacePreserveOnMember => GetResourceString("CannotWriteXmlSpacePreserveOnMember");

	internal static string CantAssignRootInstance => GetResourceString("CantAssignRootInstance");

	internal static string CantCreateUnknownType => GetResourceString("CantCreateUnknownType");

	internal static string CantGetWriteonlyProperty => GetResourceString("CantGetWriteonlyProperty");

	internal static string CantSetReadonlyProperty => GetResourceString("CantSetReadonlyProperty");

	internal static string CantSetUnknownProperty => GetResourceString("CantSetUnknownProperty");

	internal static string CloseInsideTemplate => GetResourceString("CloseInsideTemplate");

	internal static string CloseXamlWriterBeforeReading => GetResourceString("CloseXamlWriterBeforeReading");

	internal static string CollectionCannotContainNulls => GetResourceString("CollectionCannotContainNulls");

	internal static string CollectionNumberOfElementsMustBeLessOrEqualTo => GetResourceString("CollectionNumberOfElementsMustBeLessOrEqualTo");

	internal static string Collection_BadType => GetResourceString("Collection_BadType");

	internal static string Collection_CopyTo_ArrayCannotBeMultidimensional => GetResourceString("Collection_CopyTo_ArrayCannotBeMultidimensional");

	internal static string Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength => GetResourceString("Collection_CopyTo_IndexGreaterThanOrEqualToArrayLength");

	internal static string Collection_CopyTo_NumberOfElementsExceedsArrayLength => GetResourceString("Collection_CopyTo_NumberOfElementsExceedsArrayLength");

	internal static string ConstructImplicitType => GetResourceString("ConstructImplicitType");

	internal static string ConstructorInvocation => GetResourceString("ConstructorInvocation");

	internal static string ConstructorNotFoundForGivenPositionalParameters => GetResourceString("ConstructorNotFoundForGivenPositionalParameters");

	internal static string ConvertFromException => GetResourceString("ConvertFromException");

	internal static string ConvertToException => GetResourceString("ConvertToException");

	internal static string ConverterMustDeriveFromBase => GetResourceString("ConverterMustDeriveFromBase");

	internal static string DefaultAttachablePropertyStoreCannotAddInstance => GetResourceString("DefaultAttachablePropertyStoreCannotAddInstance");

	internal static string DeferredLoad => GetResourceString("DeferredLoad");

	internal static string DeferredPropertyNotCollected => GetResourceString("DeferredPropertyNotCollected");

	internal static string DeferredSave => GetResourceString("DeferredSave");

	internal static string DeferringLoaderInstanceNull => GetResourceString("DeferringLoaderInstanceNull");

	internal static string DependsOnMissing => GetResourceString("DependsOnMissing");

	internal static string DictionaryFirstChanceException => GetResourceString("DictionaryFirstChanceException");

	internal static string DirectiveGetter => GetResourceString("DirectiveGetter");

	internal static string DirectiveMustBeString => GetResourceString("DirectiveMustBeString");

	internal static string DirectiveNotAtRoot => GetResourceString("DirectiveNotAtRoot");

	internal static string DirectiveNotFound => GetResourceString("DirectiveNotFound");

	internal static string DuplicateMemberSet => GetResourceString("DuplicateMemberSet");

	internal static string DuplicateXmlnsCompat => GetResourceString("DuplicateXmlnsCompat");

	internal static string DuplicateXmlnsCompatAcrossAssemblies => GetResourceString("DuplicateXmlnsCompatAcrossAssemblies");

	internal static string Enum_Invalid => GetResourceString("Enum_Invalid");

	internal static string Enumerator_VerifyContext => GetResourceString("Enumerator_VerifyContext");

	internal static string EventCannotBeAssigned => GetResourceString("EventCannotBeAssigned");

	internal static string ExpandPositionalParametersWithReadOnlyProperties => GetResourceString("ExpandPositionalParametersWithReadOnlyProperties");

	internal static string ExpandPositionalParametersWithoutUnderlyingType => GetResourceString("ExpandPositionalParametersWithoutUnderlyingType");

	internal static string ExpandPositionalParametersinTypeWithNoDefaultConstructor => GetResourceString("ExpandPositionalParametersinTypeWithNoDefaultConstructor");

	internal static string ExpectedLoadPermission => GetResourceString("ExpectedLoadPermission");

	internal static string ExpectedObjectMarkupInfo => GetResourceString("ExpectedObjectMarkupInfo");

	internal static string ExpectedQualifiedAssemblyName => GetResourceString("ExpectedQualifiedAssemblyName");

	internal static string ExpectedQualifiedTypeName => GetResourceString("ExpectedQualifiedTypeName");

	internal static string FactoryReturnedNull => GetResourceString("FactoryReturnedNull");

	internal static string FileFormatException => GetResourceString("FileFormatException");

	internal static string FileFormatExceptionWithFileName => GetResourceString("FileFormatExceptionWithFileName");

	internal static string FileNotFoundExceptionMessage => GetResourceString("FileNotFoundExceptionMessage");

	internal static string ForwardRefDirectives => GetResourceString("ForwardRefDirectives");

	internal static string Freezable_CantBeFrozen => GetResourceString("Freezable_CantBeFrozen");

	internal static string FrugalList_CannotPromoteBeyondArray => GetResourceString("FrugalList_CannotPromoteBeyondArray");

	internal static string FrugalList_TargetMapCannotHoldAllData => GetResourceString("FrugalList_TargetMapCannotHoldAllData");

	internal static string GetConverterInstance => GetResourceString("GetConverterInstance");

	internal static string GetItemsException => GetResourceString("GetItemsException");

	internal static string GetItemsReturnedNull => GetResourceString("GetItemsReturnedNull");

	internal static string GetObjectNull => GetResourceString("GetObjectNull");

	internal static string GetTargetTypeOnNonAttachableMember => GetResourceString("GetTargetTypeOnNonAttachableMember");

	internal static string GetValue => GetResourceString("GetValue");

	internal static string GetterOrSetterRequired => GetResourceString("GetterOrSetterRequired");

	internal static string IncorrectGetterParamNum => GetResourceString("IncorrectGetterParamNum");

	internal static string IncorrectSetterParamNum => GetResourceString("IncorrectSetterParamNum");

	internal static string InitializationGuard => GetResourceString("InitializationGuard");

	internal static string InitializationSyntaxWithoutTypeConverter => GetResourceString("InitializationSyntaxWithoutTypeConverter");

	internal static string InvalidCharInTypeName => GetResourceString("InvalidCharInTypeName");

	internal static string InvalidClosingBracketCharacers => GetResourceString("InvalidClosingBracketCharacers");

	internal static string InvalidEvent => GetResourceString("InvalidEvent");

	internal static string InvalidExpression => GetResourceString("InvalidExpression");

	internal static string InvalidPermissionStateValue => GetResourceString("InvalidPermissionStateValue");

	internal static string InvalidPermissionType => GetResourceString("InvalidPermissionType");

	internal static string InvalidTypeArgument => GetResourceString("InvalidTypeArgument");

	internal static string InvalidTypeListString => GetResourceString("InvalidTypeListString");

	internal static string InvalidTypeString => GetResourceString("InvalidTypeString");

	internal static string InvalidXamlMemberName => GetResourceString("InvalidXamlMemberName");

	internal static string LateConstructionDirective => GetResourceString("LateConstructionDirective");

	internal static string LineNumberAndPosition => GetResourceString("LineNumberAndPosition");

	internal static string LineNumberOnly => GetResourceString("LineNumberOnly");

	internal static string ListNotIList => GetResourceString("ListNotIList");

	internal static string MalformedBracketCharacters => GetResourceString("MalformedBracketCharacters");

	internal static string MalformedPropertyName => GetResourceString("MalformedPropertyName");

	internal static string MarkupExtensionArrayBadType => GetResourceString("MarkupExtensionArrayBadType");

	internal static string MarkupExtensionArrayType => GetResourceString("MarkupExtensionArrayType");

	internal static string MarkupExtensionBadStatic => GetResourceString("MarkupExtensionBadStatic");

	internal static string MarkupExtensionNoContext => GetResourceString("MarkupExtensionNoContext");

	internal static string MarkupExtensionStaticMember => GetResourceString("MarkupExtensionStaticMember");

	internal static string MarkupExtensionTypeName => GetResourceString("MarkupExtensionTypeName");

	internal static string MarkupExtensionTypeNameBad => GetResourceString("MarkupExtensionTypeNameBad");

	internal static string MarkupExtensionWithDuplicateArity => GetResourceString("MarkupExtensionWithDuplicateArity");

	internal static string MemberHasInvalidXamlName => GetResourceString("MemberHasInvalidXamlName");

	internal static string MemberIsInternal => GetResourceString("MemberIsInternal");

	internal static string MethodInvocation => GetResourceString("MethodInvocation");

	internal static string MissingAssemblyName => GetResourceString("MissingAssemblyName");

	internal static string MissingCase => GetResourceString("MissingCase");

	internal static string MissingCaseXamlNodes => GetResourceString("MissingCaseXamlNodes");

	internal static string MissingComma1 => GetResourceString("MissingComma1");

	internal static string MissingComma2 => GetResourceString("MissingComma2");

	internal static string MissingImplicitProperty => GetResourceString("MissingImplicitProperty");

	internal static string MissingImplicitPropertyTypeCase => GetResourceString("MissingImplicitPropertyTypeCase");

	internal static string MissingKey => GetResourceString("MissingKey");

	internal static string MissingLookPropertyBit => GetResourceString("MissingLookPropertyBit");

	internal static string MissingNameProvider => GetResourceString("MissingNameProvider");

	internal static string MissingNameResolver => GetResourceString("MissingNameResolver");

	internal static string MissingPropertyCaseClrType => GetResourceString("MissingPropertyCaseClrType");

	internal static string MissingTagInNamespace => GetResourceString("MissingTagInNamespace");

	internal static string MissingTypeConverter => GetResourceString("MissingTypeConverter");

	internal static string MustBeOfType => GetResourceString("MustBeOfType");

	internal static string MustHaveName => GetResourceString("MustHaveName");

	internal static string MustNotCallSetter => GetResourceString("MustNotCallSetter");

	internal static string NameNotFound => GetResourceString("NameNotFound");

	internal static string NameScopeDuplicateNamesNotAllowed => GetResourceString("NameScopeDuplicateNamesNotAllowed");

	internal static string NameScopeException => GetResourceString("NameScopeException");

	internal static string NameScopeInvalidIdentifierName => GetResourceString("NameScopeInvalidIdentifierName");

	internal static string NameScopeNameNotEmptyString => GetResourceString("NameScopeNameNotEmptyString");

	internal static string NameScopeNameNotFound => GetResourceString("NameScopeNameNotFound");

	internal static string NameScopeOnRootInstance => GetResourceString("NameScopeOnRootInstance");

	internal static string NamespaceDeclarationCannotBeXml => GetResourceString("NamespaceDeclarationCannotBeXml");

	internal static string NamespaceDeclarationNamespaceCannotBeNull => GetResourceString("NamespaceDeclarationNamespaceCannotBeNull");

	internal static string NamespaceDeclarationPrefixCannotBeNull => GetResourceString("NamespaceDeclarationPrefixCannotBeNull");

	internal static string NamespaceNotFound => GetResourceString("NamespaceNotFound");

	internal static string NoAddMethodFound => GetResourceString("NoAddMethodFound");

	internal static string NoAttributeUsage => GetResourceString("NoAttributeUsage");

	internal static string NoConstructor => GetResourceString("NoConstructor");

	internal static string NoConstructorWithNArugments => GetResourceString("NoConstructorWithNArugments");

	internal static string NoDefaultConstructor => GetResourceString("NoDefaultConstructor");

	internal static string NoElementUsage => GetResourceString("NoElementUsage");

	internal static string NoPropertyInCurrentFrame_EM => GetResourceString("NoPropertyInCurrentFrame_EM");

	internal static string NoPropertyInCurrentFrame_EM_noType => GetResourceString("NoPropertyInCurrentFrame_EM_noType");

	internal static string NoPropertyInCurrentFrame_GO => GetResourceString("NoPropertyInCurrentFrame_GO");

	internal static string NoPropertyInCurrentFrame_GO_noType => GetResourceString("NoPropertyInCurrentFrame_GO_noType");

	internal static string NoPropertyInCurrentFrame_NS => GetResourceString("NoPropertyInCurrentFrame_NS");

	internal static string NoPropertyInCurrentFrame_SO => GetResourceString("NoPropertyInCurrentFrame_SO");

	internal static string NoPropertyInCurrentFrame_V => GetResourceString("NoPropertyInCurrentFrame_V");

	internal static string NoPropertyInCurrentFrame_V_noType => GetResourceString("NoPropertyInCurrentFrame_V_noType");

	internal static string NoSuchConstructor => GetResourceString("NoSuchConstructor");

	internal static string NoTypeInCurrentFrame_EO => GetResourceString("NoTypeInCurrentFrame_EO");

	internal static string NoTypeInCurrentFrame_SM => GetResourceString("NoTypeInCurrentFrame_SM");

	internal static string NonMEWithPositionalParameters => GetResourceString("NonMEWithPositionalParameters");

	internal static string NotAmbientProperty => GetResourceString("NotAmbientProperty");

	internal static string NotAmbientType => GetResourceString("NotAmbientType");

	internal static string NotAssignableFrom => GetResourceString("NotAssignableFrom");

	internal static string NotDeclaringTypeAttributeProperty => GetResourceString("NotDeclaringTypeAttributeProperty");

	internal static string NotSupportedOnDirective => GetResourceString("NotSupportedOnDirective");

	internal static string NotSupportedOnUnknownMember => GetResourceString("NotSupportedOnUnknownMember");

	internal static string NotSupportedOnUnknownType => GetResourceString("NotSupportedOnUnknownType");

	internal static string ObjectNotTcOrMe => GetResourceString("ObjectNotTcOrMe");

	internal static string ObjectReaderAttachedPropertyNotFound => GetResourceString("ObjectReaderAttachedPropertyNotFound");

	internal static string ObjectReaderDictionaryMethod1NotFound => GetResourceString("ObjectReaderDictionaryMethod1NotFound");

	internal static string ObjectReaderInstanceDescriptorIncompatibleArgumentTypes => GetResourceString("ObjectReaderInstanceDescriptorIncompatibleArgumentTypes");

	internal static string ObjectReaderInstanceDescriptorIncompatibleArguments => GetResourceString("ObjectReaderInstanceDescriptorIncompatibleArguments");

	internal static string ObjectReaderInstanceDescriptorInvalidMethod => GetResourceString("ObjectReaderInstanceDescriptorInvalidMethod");

	internal static string ObjectReaderMultidimensionalArrayNotSupported => GetResourceString("ObjectReaderMultidimensionalArrayNotSupported");

	internal static string ObjectReaderNoDefaultConstructor => GetResourceString("ObjectReaderNoDefaultConstructor");

	internal static string ObjectReaderNoMatchingConstructor => GetResourceString("ObjectReaderNoMatchingConstructor");

	internal static string ObjectReaderTypeCannotRoundtrip => GetResourceString("ObjectReaderTypeCannotRoundtrip");

	internal static string ObjectReaderTypeIsNested => GetResourceString("ObjectReaderTypeIsNested");

	internal static string ObjectReaderTypeNotAllowed => GetResourceString("ObjectReaderTypeNotAllowed");

	internal static string ObjectReaderXamlNamePropertyMustBeString => GetResourceString("ObjectReaderXamlNamePropertyMustBeString");

	internal static string ObjectReaderXamlNameScopeResultsInClonedObject => GetResourceString("ObjectReaderXamlNameScopeResultsInClonedObject");

	internal static string ObjectReaderXamlNamedElementAlreadyRegistered => GetResourceString("ObjectReaderXamlNamedElementAlreadyRegistered");

	internal static string ObjectReader_TypeNotVisible => GetResourceString("ObjectReader_TypeNotVisible");

	internal static string ObjectWriterTypeNotAllowed => GetResourceString("ObjectWriterTypeNotAllowed");

	internal static string OnlySupportedOnCollections => GetResourceString("OnlySupportedOnCollections");

	internal static string OnlySupportedOnCollectionsAndDictionaries => GetResourceString("OnlySupportedOnCollectionsAndDictionaries");

	internal static string OnlySupportedOnDictionaries => GetResourceString("OnlySupportedOnDictionaries");

	internal static string OpenPropertyInCurrentFrame_EO => GetResourceString("OpenPropertyInCurrentFrame_EO");

	internal static string OpenPropertyInCurrentFrame_SM => GetResourceString("OpenPropertyInCurrentFrame_SM");

	internal static string ParameterCannotBeNegative => GetResourceString("ParameterCannotBeNegative");

	internal static string ParentlessPropertyElement => GetResourceString("ParentlessPropertyElement");

	internal static string ParserAssemblyLoadVersionMismatch => GetResourceString("ParserAssemblyLoadVersionMismatch");

	internal static string ParserAttributeArgsHigh => GetResourceString("ParserAttributeArgsHigh");

	internal static string ParserAttributeArgsLow => GetResourceString("ParserAttributeArgsLow");

	internal static string PositionalParamsWrongLength => GetResourceString("PositionalParamsWrongLength");

	internal static string PrefixNotFound => GetResourceString("PrefixNotFound");

	internal static string PrefixNotInFrames => GetResourceString("PrefixNotInFrames");

	internal static string PropertyDoesNotTakeText => GetResourceString("PropertyDoesNotTakeText");

	internal static string PropertyNotImplemented => GetResourceString("PropertyNotImplemented");

	internal static string ProvideValue => GetResourceString("ProvideValue");

	internal static string ProvideValueCycle => GetResourceString("ProvideValueCycle");

	internal static string QuoteCharactersOutOfPlace => GetResourceString("QuoteCharactersOutOfPlace");

	internal static string ReferenceIsNull => GetResourceString("ReferenceIsNull");

	internal static string SavedContextSchemaContextMismatch => GetResourceString("SavedContextSchemaContextMismatch");

	internal static string SavedContextSchemaContextNull => GetResourceString("SavedContextSchemaContextNull");

	internal static string SchemaContextNotInitialized => GetResourceString("SchemaContextNotInitialized");

	internal static string SchemaContextNull => GetResourceString("SchemaContextNull");

	internal static string SecurityExceptionForSettingSandboxExternalToTrue => GetResourceString("SecurityExceptionForSettingSandboxExternalToTrue");

	internal static string SecurityXmlMissingAttribute => GetResourceString("SecurityXmlMissingAttribute");

	internal static string SecurityXmlUnexpectedTag => GetResourceString("SecurityXmlUnexpectedTag");

	internal static string SecurityXmlUnexpectedValue => GetResourceString("SecurityXmlUnexpectedValue");

	internal static string ServiceTypeAlreadyAdded => GetResourceString("ServiceTypeAlreadyAdded");

	internal static string SetConnectionId => GetResourceString("SetConnectionId");

	internal static string SetOnlyProperty => GetResourceString("SetOnlyProperty");

	internal static string SetTargetTypeOnNonAttachableMember => GetResourceString("SetTargetTypeOnNonAttachableMember");

	internal static string SetUriBase => GetResourceString("SetUriBase");

	internal static string SetValue => GetResourceString("SetValue");

	internal static string SetXmlInstance => GetResourceString("SetXmlInstance");

	internal static string SettingPropertiesIsNotAllowed => GetResourceString("SettingPropertiesIsNotAllowed");

	internal static string ShouldOverrideMethod => GetResourceString("ShouldOverrideMethod");

	internal static string ShouldSerializeFailed => GetResourceString("ShouldSerializeFailed");

	internal static string SimpleFixupsMustHaveOneName => GetResourceString("SimpleFixupsMustHaveOneName");

	internal static string StringEmpty => GetResourceString("StringEmpty");

	internal static string StringIsNullOrEmpty => GetResourceString("StringIsNullOrEmpty");

	internal static string TemplateNotCollected => GetResourceString("TemplateNotCollected");

	internal static string ThreadAlreadyStarted => GetResourceString("ThreadAlreadyStarted");

	internal static string ToStringNull => GetResourceString("ToStringNull");

	internal static string TokenizerHelperEmptyToken => GetResourceString("TokenizerHelperEmptyToken");

	internal static string TokenizerHelperExtraDataEncountered => GetResourceString("TokenizerHelperExtraDataEncountered");

	internal static string TokenizerHelperMissingEndQuote => GetResourceString("TokenizerHelperMissingEndQuote");

	internal static string TokenizerHelperPrematureStringTermination => GetResourceString("TokenizerHelperPrematureStringTermination");

	internal static string TooManyAttributes => GetResourceString("TooManyAttributes");

	internal static string TooManyAttributesOnType => GetResourceString("TooManyAttributesOnType");

	internal static string TooManyTypeConverterAttributes => GetResourceString("TooManyTypeConverterAttributes");

	internal static string TransitiveForwardRefDirectives => GetResourceString("TransitiveForwardRefDirectives");

	internal static string TypeConverterFailed => GetResourceString("TypeConverterFailed");

	internal static string TypeConverterFailed2 => GetResourceString("TypeConverterFailed2");

	internal static string TypeHasInvalidXamlName => GetResourceString("TypeHasInvalidXamlName");

	internal static string TypeHasNoContentProperty => GetResourceString("TypeHasNoContentProperty");

	internal static string TypeMetadataCannotChangeAfterUse => GetResourceString("TypeMetadataCannotChangeAfterUse");

	internal static string TypeNameCannotHavePeriod => GetResourceString("TypeNameCannotHavePeriod");

	internal static string TypeNotFound => GetResourceString("TypeNotFound");

	internal static string TypeNotPublic => GetResourceString("TypeNotPublic");

	internal static string UnclosedQuote => GetResourceString("UnclosedQuote");

	internal static string UnexpectedClose => GetResourceString("UnexpectedClose");

	internal static string UnexpectedConstructorArg => GetResourceString("UnexpectedConstructorArg");

	internal static string UnexpectedNodeType => GetResourceString("UnexpectedNodeType");

	internal static string UnexpectedParameterType => GetResourceString("UnexpectedParameterType");

	internal static string UnexpectedToken => GetResourceString("UnexpectedToken");

	internal static string UnexpectedTokenAfterME => GetResourceString("UnexpectedTokenAfterME");

	internal static string UnhandledBoolTypeBit => GetResourceString("UnhandledBoolTypeBit");

	internal static string UnknownAttributeProperty => GetResourceString("UnknownAttributeProperty");

	internal static string UnknownMember => GetResourceString("UnknownMember");

	internal static string UnknownMemberOnUnknownType => GetResourceString("UnknownMemberOnUnknownType");

	internal static string UnknownMemberSimple => GetResourceString("UnknownMemberSimple");

	internal static string UnknownType => GetResourceString("UnknownType");

	internal static string UnresolvedForwardReferences => GetResourceString("UnresolvedForwardReferences");

	internal static string UnresolvedNamespace => GetResourceString("UnresolvedNamespace");

	internal static string UriNotFound => GetResourceString("UriNotFound");

	internal static string UsableDuringInitializationOnME => GetResourceString("UsableDuringInitializationOnME");

	internal static string ValueInArrayIsNull => GetResourceString("ValueInArrayIsNull");

	internal static string ValueMustBeFollowedByEndMember => GetResourceString("ValueMustBeFollowedByEndMember");

	internal static string Visual_ArgumentOutOfRange => GetResourceString("Visual_ArgumentOutOfRange");

	internal static string WhiteSpaceInCollection => GetResourceString("WhiteSpaceInCollection");

	internal static string WhitespaceAfterME => GetResourceString("WhitespaceAfterME");

	internal static string WriterIsClosed => GetResourceString("WriterIsClosed");

	internal static string XCRChoiceAfterFallback => GetResourceString("XCRChoiceAfterFallback");

	internal static string XCRChoiceNotFound => GetResourceString("XCRChoiceNotFound");

	internal static string XCRChoiceOnlyInAC => GetResourceString("XCRChoiceOnlyInAC");

	internal static string XCRCompatCycle => GetResourceString("XCRCompatCycle");

	internal static string XCRDuplicatePreserve => GetResourceString("XCRDuplicatePreserve");

	internal static string XCRDuplicateProcessContent => GetResourceString("XCRDuplicateProcessContent");

	internal static string XCRDuplicateWildcardPreserve => GetResourceString("XCRDuplicateWildcardPreserve");

	internal static string XCRDuplicateWildcardProcessContent => GetResourceString("XCRDuplicateWildcardProcessContent");

	internal static string XCRFallbackOnlyInAC => GetResourceString("XCRFallbackOnlyInAC");

	internal static string XCRInvalidACChild => GetResourceString("XCRInvalidACChild");

	internal static string XCRInvalidAttribInElement => GetResourceString("XCRInvalidAttribInElement");

	internal static string XCRInvalidFormat => GetResourceString("XCRInvalidFormat");

	internal static string XCRInvalidPreserve => GetResourceString("XCRInvalidPreserve");

	internal static string XCRInvalidProcessContent => GetResourceString("XCRInvalidProcessContent");

	internal static string XCRInvalidRequiresAttribute => GetResourceString("XCRInvalidRequiresAttribute");

	internal static string XCRInvalidXMLName => GetResourceString("XCRInvalidXMLName");

	internal static string XCRMultipleFallbackFound => GetResourceString("XCRMultipleFallbackFound");

	internal static string XCRMustUnderstandFailed => GetResourceString("XCRMustUnderstandFailed");

	internal static string XCRNSPreserveNotIgnorable => GetResourceString("XCRNSPreserveNotIgnorable");

	internal static string XCRNSProcessContentNotIgnorable => GetResourceString("XCRNSProcessContentNotIgnorable");

	internal static string XCRRequiresAttribNotFound => GetResourceString("XCRRequiresAttribNotFound");

	internal static string XCRUndefinedPrefix => GetResourceString("XCRUndefinedPrefix");

	internal static string XCRUnknownCompatAttrib => GetResourceString("XCRUnknownCompatAttrib");

	internal static string XCRUnknownCompatElement => GetResourceString("XCRUnknownCompatElement");

	internal static string XClassMustMatchRootInstance => GetResourceString("XClassMustMatchRootInstance");

	internal static string XamlFactoryInvalidXamlNode => GetResourceString("XamlFactoryInvalidXamlNode");

	internal static string XamlMarkupExtensionWriterCannotSetSchemaContext => GetResourceString("XamlMarkupExtensionWriterCannotSetSchemaContext");

	internal static string XamlMarkupExtensionWriterCannotWriteNonstringValue => GetResourceString("XamlMarkupExtensionWriterCannotWriteNonstringValue");

	internal static string XamlMarkupExtensionWriterDuplicateMember => GetResourceString("XamlMarkupExtensionWriterDuplicateMember");

	internal static string XamlMarkupExtensionWriterInputInvalid => GetResourceString("XamlMarkupExtensionWriterInputInvalid");

	internal static string XamlTypeNameCannotGetPrefix => GetResourceString("XamlTypeNameCannotGetPrefix");

	internal static string XamlTypeNameNameIsNullOrEmpty => GetResourceString("XamlTypeNameNameIsNullOrEmpty");

	internal static string XamlTypeNameNamespaceIsNull => GetResourceString("XamlTypeNameNamespaceIsNull");

	internal static string XamlXmlWriterCannotWriteNonstringValue => GetResourceString("XamlXmlWriterCannotWriteNonstringValue");

	internal static string XamlXmlWriterDuplicateMember => GetResourceString("XamlXmlWriterDuplicateMember");

	internal static string XamlXmlWriterIsObjectFromMemberSetForArraysOrNonCollections => GetResourceString("XamlXmlWriterIsObjectFromMemberSetForArraysOrNonCollections");

	internal static string XamlXmlWriterNamespaceAlreadyHasPrefixInCurrentScope => GetResourceString("XamlXmlWriterNamespaceAlreadyHasPrefixInCurrentScope");

	internal static string XamlXmlWriterPrefixAlreadyDefinedInCurrentScope => GetResourceString("XamlXmlWriterPrefixAlreadyDefinedInCurrentScope");

	internal static string XamlXmlWriterWriteNotSupportedInCurrentState => GetResourceString("XamlXmlWriterWriteNotSupportedInCurrentState");

	internal static string XamlXmlWriterWriteObjectNotSupportedInCurrentState => GetResourceString("XamlXmlWriterWriteObjectNotSupportedInCurrentState");

	internal static string XaslTypePropertiesNotImplemented => GetResourceString("XaslTypePropertiesNotImplemented");

	internal static string XmlDataNull => GetResourceString("XmlDataNull");

	internal static string XmlValueNotReader => GetResourceString("XmlValueNotReader");

	internal static string XmlnsCompatCycle => GetResourceString("XmlnsCompatCycle");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey)
	{
		string result = null;
		try
		{
			result = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		return result;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string resourceString = GetResourceString(resourceKey);
		if (defaultString != null && resourceKey.Equals(resourceString, StringComparison.Ordinal))
		{
			return defaultString;
		}
		return resourceString;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}
}
