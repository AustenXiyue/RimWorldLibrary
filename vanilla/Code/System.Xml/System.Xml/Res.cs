namespace System.Xml;

internal static class Res
{
	public const string Xml_UserException = "{0}";

	public const string Xml_DefaultException = "An XML error has occurred.";

	public const string Xml_InvalidOperation = "Operation is not valid due to the current state of the object.";

	public const string Xml_ErrorFilePosition = "An error occurred at {0}, ({1}, {2}).";

	public const string Xml_StackOverflow = "Stack overflow.";

	public const string Xslt_NoStylesheetLoaded = "No stylesheet was loaded.";

	public const string Xslt_NotCompiledStylesheet = "Type '{0}' is not a compiled stylesheet class.";

	public const string Xslt_IncompatibleCompiledStylesheetVersion = "Executing a stylesheet that was compiled using a later version of the framework is not supported. Stylesheet Version: {0}. Current Framework Version: {1}.";

	public const string Xml_AsyncIsRunningException = "An asynchronous operation is already in progress.";

	public const string Xml_ReaderAsyncNotSetException = "Set XmlReaderSettings.Async to true if you want to use Async Methods.";

	public const string Xml_UnclosedQuote = "There is an unclosed literal string.";

	public const string Xml_UnexpectedEOF = "Unexpected end of file while parsing {0} has occurred.";

	public const string Xml_UnexpectedEOF1 = "Unexpected end of file has occurred.";

	public const string Xml_UnexpectedEOFInElementContent = "Unexpected end of file has occurred. The following elements are not closed: {0}";

	public const string Xml_BadStartNameChar = "Name cannot begin with the '{0}' character, hexadecimal value {1}.";

	public const string Xml_BadNameChar = "The '{0}' character, hexadecimal value {1}, cannot be included in a name.";

	public const string Xml_BadDecimalEntity = "Invalid syntax for a decimal numeric entity reference.";

	public const string Xml_BadHexEntity = "Invalid syntax for a hexadecimal numeric entity reference.";

	public const string Xml_MissingByteOrderMark = "There is no Unicode byte order mark. Cannot switch to Unicode.";

	public const string Xml_UnknownEncoding = "System does not support '{0}' encoding.";

	public const string Xml_InternalError = "An internal error has occurred.";

	public const string Xml_InvalidCharInThisEncoding = "Invalid character in the given encoding.";

	public const string Xml_ErrorPosition = "Line {0}, position {1}.";

	public const string Xml_MessageWithErrorPosition = "{0} Line {1}, position {2}.";

	public const string Xml_UnexpectedTokenEx = "'{0}' is an unexpected token. The expected token is '{1}'.";

	public const string Xml_UnexpectedTokens2 = "'{0}' is an unexpected token. The expected token is '{1}' or '{2}'.";

	public const string Xml_ExpectingWhiteSpace = "'{0}' is an unexpected token. Expecting white space.";

	public const string Xml_TagMismatch = "The '{0}' start tag on line {1} does not match the end tag of '{2}'.";

	public const string Xml_TagMismatchEx = "The '{0}' start tag on line {1} position {2} does not match the end tag of '{3}'.";

	public const string Xml_UnexpectedEndTag = "Unexpected end tag.";

	public const string Xml_UnknownNs = "'{0}' is an undeclared prefix.";

	public const string Xml_BadAttributeChar = "'{0}', hexadecimal value {1}, is an invalid attribute character.";

	public const string Xml_ExpectExternalOrClose = "Expecting external ID, '[' or '>'.";

	public const string Xml_MissingRoot = "Root element is missing.";

	public const string Xml_MultipleRoots = "There are multiple root elements.";

	public const string Xml_InvalidRootData = "Data at the root level is invalid.";

	public const string Xml_XmlDeclNotFirst = "Unexpected XML declaration. The XML declaration must be the first node in the document, and no white space characters are allowed to appear before it.";

	public const string Xml_InvalidXmlDecl = "Syntax for an XML declaration is invalid.";

	public const string Xml_InvalidNodeType = "'{0}' is an invalid XmlNodeType.";

	public const string Xml_InvalidPIName = "'{0}' is an invalid name for processing instructions.";

	public const string Xml_InvalidXmlSpace = "'{0}' is an invalid xml:space value.";

	public const string Xml_InvalidVersionNumber = "Version number '{0}' is invalid.";

	public const string Xml_DupAttributeName = "'{0}' is a duplicate attribute name.";

	public const string Xml_BadDTDLocation = "Unexpected DTD declaration.";

	public const string Xml_ElementNotFound = "Element '{0}' was not found.";

	public const string Xml_ElementNotFoundNs = "Element '{0}' with namespace name '{1}' was not found.";

	public const string Xml_PartialContentNodeTypeNotSupportedEx = "XmlNodeType {0} is not supported for partial content parsing.";

	public const string Xml_MultipleDTDsProvided = "Cannot have multiple DTDs.";

	public const string Xml_CanNotBindToReservedNamespace = "Cannot bind to the reserved namespace.";

	public const string Xml_InvalidCharacter = "'{0}', hexadecimal value {1}, is an invalid character.";

	public const string Xml_InvalidBinHexValue = "'{0}' is not a valid BinHex text sequence.";

	public const string Xml_InvalidBinHexValueOddCount = "'{0}' is not a valid BinHex text sequence. The sequence must contain an even number of characters.";

	public const string Xml_InvalidTextDecl = "Invalid text declaration.";

	public const string Xml_InvalidBase64Value = "'{0}' is not a valid Base64 text sequence.";

	public const string Xml_UndeclaredEntity = "Reference to undeclared entity '{0}'.";

	public const string Xml_RecursiveParEntity = "Parameter entity '{0}' references itself.";

	public const string Xml_RecursiveGenEntity = "General entity '{0}' references itself.";

	public const string Xml_ExternalEntityInAttValue = "External entity '{0}' reference cannot appear in the attribute value.";

	public const string Xml_UnparsedEntityRef = "Reference to unparsed entity '{0}'.";

	public const string Xml_NotSameNametable = "Not the same name table.";

	public const string Xml_NametableMismatch = "XmlReaderSettings.XmlNameTable must be the same name table as in XmlParserContext.NameTable or XmlParserContext.NamespaceManager.NameTable, or it must be null.";

	public const string Xml_BadNamespaceDecl = "Invalid namespace declaration.";

	public const string Xml_ErrorParsingEntityName = "An error occurred while parsing EntityName.";

	public const string Xml_InvalidNmToken = "Invalid NmToken value '{0}'.";

	public const string Xml_EntityRefNesting = "Entity replacement text must nest properly within markup declarations.";

	public const string Xml_CannotResolveEntity = "Cannot resolve entity reference '{0}'.";

	public const string Xml_CannotResolveEntityDtdIgnored = "Cannot resolve entity reference '{0}' because the DTD has been ignored. To enable DTD processing set the DtdProcessing property on XmlReaderSettings to Parse and pass the settings into XmlReader.Create method.";

	public const string Xml_CannotResolveExternalSubset = "Cannot resolve external DTD subset - public ID = '{0}', system ID = '{1}'.";

	public const string Xml_CannotResolveUrl = "Cannot resolve '{0}'.";

	public const string Xml_CDATAEndInText = "']]>' is not allowed in character data.";

	public const string Xml_ExternalEntityInStandAloneDocument = "Standalone document declaration must have a value of 'no' because an external entity '{0}' is referenced.";

	public const string Xml_DtdAfterRootElement = "DTD must be defined before the document root element.";

	public const string Xml_ReadOnlyProperty = "The '{0}' property is read only and cannot be set.";

	public const string Xml_DtdIsProhibited = "DTD is prohibited in this XML document.";

	public const string Xml_DtdIsProhibitedEx = "For security reasons DTD is prohibited in this XML document. To enable DTD processing set the DtdProcessing property on XmlReaderSettings to Parse and pass the settings into XmlReader.Create method.";

	public const string Xml_ReadSubtreeNotOnElement = "ReadSubtree() can be called only if the reader is on an element node.";

	public const string Xml_DtdNotAllowedInFragment = "DTD is not allowed in XML fragments.";

	public const string Xml_CannotStartDocumentOnFragment = "WriteStartDocument cannot be called on writers created with ConformanceLevel.Fragment.";

	public const string Xml_ErrorOpeningExternalDtd = "An error has occurred while opening external DTD '{0}': {1}";

	public const string Xml_ErrorOpeningExternalEntity = "An error has occurred while opening external entity '{0}': {1}";

	public const string Xml_ReadBinaryContentNotSupported = "{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.";

	public const string Xml_ReadValueChunkNotSupported = "ReadValueChunk method is not supported on this XmlReader. Use CanReadValueChunk property to find out if an XmlReader implements it.";

	public const string Xml_InvalidReadContentAs = "The {0} method is not supported on node type {1}. If you want to read typed content of an element, use the ReadElementContentAs method.";

	public const string Xml_InvalidReadElementContentAs = "The {0} method is not supported on node type {1}.";

	public const string Xml_MixedReadElementContentAs = "ReadElementContentAs() methods cannot be called on an element that has child elements.";

	public const string Xml_MixingReadValueChunkWithBinary = "ReadValueChunk calls cannot be mixed with ReadContentAsBase64 or ReadContentAsBinHex.";

	public const string Xml_MixingBinaryContentMethods = "ReadContentAsBase64 and ReadContentAsBinHex method calls cannot be mixed with calls to ReadElementContentAsBase64 and ReadElementContentAsBinHex.";

	public const string Xml_MixingV1StreamingWithV2Binary = "ReadContentAsBase64 and ReadContentAsBinHex method calls cannot be mixed with calls to ReadChars, ReadBase64, and ReadBinHex.";

	public const string Xml_InvalidReadValueChunk = "The ReadValueAsChunk method is not supported on node type {0}.";

	public const string Xml_ReadContentAsFormatException = "Content cannot be converted to the type {0}.";

	public const string Xml_DoubleBaseUri = "BaseUri must be specified either as an argument of XmlReader.Create or on the XmlParserContext. If it is specified on both, it must be the same base URI.";

	public const string Xml_NotEnoughSpaceForSurrogatePair = "The buffer is not large enough to fit a surrogate pair. Please provide a buffer of size at least 2 characters.";

	public const string Xml_EmptyUrl = "The URL cannot be empty.";

	public const string Xml_UnexpectedNodeInSimpleContent = "Unexpected node type {0}. {1} method can only be called on elements with simple or empty content.";

	public const string Xml_InvalidWhitespaceCharacter = "The Whitespace or SignificantWhitespace node can contain only XML white space characters. '{0}' is not an XML white space character.";

	public const string Xml_IncompatibleConformanceLevel = "Cannot change conformance checking to {0}. Make sure the ConformanceLevel in XmlReaderSettings is set to Auto for wrapping scenarios.";

	public const string Xml_LimitExceeded = "The input document has exceeded a limit set by {0}.";

	public const string Xml_ClosedOrErrorReader = "The XmlReader is closed or in error state.";

	public const string Xml_CharEntityOverflow = "Invalid value of a character entity reference.";

	public const string Xml_BadNameCharWithPos = "The '{0}' character, hexadecimal value {1}, at position {2} within the name, cannot be included in a name.";

	public const string Xml_XmlnsBelongsToReservedNs = "The 'xmlns' attribute is bound to the reserved namespace 'http://www.w3.org/2000/xmlns/'.";

	public const string Xml_UndeclaredParEntity = "Reference to undeclared parameter entity '{0}'.";

	public const string Xml_InvalidXmlDocument = "Invalid XML document. {0}";

	public const string Xml_NoDTDPresent = "No DTD found.";

	public const string Xml_MultipleValidaitonTypes = "Unsupported combination of validation types.";

	public const string Xml_NoValidation = "No validation occurred.";

	public const string Xml_WhitespaceHandling = "Expected WhitespaceHandling.None, or WhitespaceHandling.All, or WhitespaceHandling.Significant.";

	public const string Xml_InvalidResetStateCall = "Cannot call ResetState when parsing an XML fragment.";

	public const string Xml_EntityHandling = "Expected EntityHandling.ExpandEntities or EntityHandling.ExpandCharEntities.";

	public const string Xml_AttlistDuplEnumValue = "'{0}' is a duplicate enumeration value.";

	public const string Xml_AttlistDuplNotationValue = "'{0}' is a duplicate notation value.";

	public const string Xml_EncodingSwitchAfterResetState = "'{0}' is an invalid value for the 'encoding' attribute. The encoding cannot be switched after a call to ResetState.";

	public const string Xml_UnexpectedNodeType = "Unexpected XmlNodeType: '{0}'.";

	public const string Xml_InvalidConditionalSection = "A conditional section is not allowed in an internal subset.";

	public const string Xml_UnexpectedCDataEnd = "']]>' is not expected.";

	public const string Xml_UnclosedConditionalSection = "There is an unclosed conditional section.";

	public const string Xml_ExpectDtdMarkup = "Expected DTD markup was not found.";

	public const string Xml_IncompleteDtdContent = "Incomplete DTD content.";

	public const string Xml_EnumerationRequired = "Enumeration data type required.";

	public const string Xml_InvalidContentModel = "Invalid content model.";

	public const string Xml_FragmentId = "Fragment identifier '{0}' cannot be part of the system identifier '{1}'.";

	public const string Xml_ExpectPcData = "Expecting 'PCDATA'.";

	public const string Xml_ExpectNoWhitespace = "White space not allowed before '?', '*', or '+'.";

	public const string Xml_ExpectOp = "Expecting '?', '*', or '+'.";

	public const string Xml_InvalidAttributeType = "'{0}' is an invalid attribute type.";

	public const string Xml_InvalidAttributeType1 = "Invalid attribute type.";

	public const string Xml_ExpectAttType = "Expecting an attribute type.";

	public const string Xml_ColonInLocalName = "'{0}' is an unqualified name and cannot contain the character ':'.";

	public const string Xml_InvalidParEntityRef = "A parameter entity reference is not allowed in internal markup.";

	public const string Xml_ExpectSubOrClose = "Expecting an internal subset or the end of the DOCTYPE declaration.";

	public const string Xml_ExpectExternalOrPublicId = "Expecting a system identifier or a public identifier.";

	public const string Xml_ExpectExternalIdOrEntityValue = "Expecting an external identifier or an entity value.";

	public const string Xml_ExpectIgnoreOrInclude = "Conditional sections must specify the keyword 'IGNORE' or 'INCLUDE'.";

	public const string Xml_UnsupportedClass = "Object type is not supported.";

	public const string Xml_NullResolver = "Resolving of external URIs was prohibited.";

	public const string Xml_RelativeUriNotSupported = "Relative URIs are not supported.";

	public const string Xml_UntrustedCodeSettingResolver = "XmlResolver can be set only by fully trusted code.";

	public const string Xml_WriterAsyncNotSetException = "Set XmlWriterSettings.Async to true if you want to use Async Methods.";

	public const string Xml_PrefixForEmptyNs = "Cannot use a prefix with an empty namespace.";

	public const string Xml_InvalidCommentChars = "An XML comment cannot contain '--', and '-' cannot be the last character.";

	public const string Xml_UndefNamespace = "The '{0}' namespace is not defined.";

	public const string Xml_EmptyName = "The empty string '' is not a valid name.";

	public const string Xml_EmptyLocalName = "The empty string '' is not a valid local name.";

	public const string Xml_InvalidNameCharsDetail = "Invalid name character in '{0}'. The '{1}' character, hexadecimal value {2}, cannot be included in a name.";

	public const string Xml_NoStartTag = "There was no XML start tag open.";

	public const string Xml_ClosedOrError = "The Writer is closed or in error state.";

	public const string Xml_WrongToken = "Token {0} in state {1} would result in an invalid XML document.";

	public const string Xml_XmlPrefix = "Prefix \"xml\" is reserved for use by XML and can be mapped only to namespace name \"http://www.w3.org/XML/1998/namespace\".";

	public const string Xml_XmlnsPrefix = "Prefix \"xmlns\" is reserved for use by XML.";

	public const string Xml_NamespaceDeclXmlXmlns = "Prefix '{0}' cannot be mapped to namespace name reserved for \"xml\" or \"xmlns\".";

	public const string Xml_NonWhitespace = "Only white space characters should be used.";

	public const string Xml_DupXmlDecl = "Cannot write XML declaration. WriteStartDocument method has already written it.";

	public const string Xml_CannotWriteXmlDecl = "Cannot write XML declaration. XML declaration can be only at the beginning of the document.";

	public const string Xml_NoRoot = "Document does not have a root element.";

	public const string Xml_InvalidPosition = "The current position on the Reader is neither an element nor an attribute.";

	public const string Xml_IncompleteEntity = "Incomplete entity contents.";

	public const string Xml_InvalidSurrogateHighChar = "Invalid high surrogate character (0x{0}). A high surrogate character must have a value from range (0xD800 - 0xDBFF).";

	public const string Xml_InvalidSurrogateMissingLowChar = "The surrogate pair is invalid. Missing a low surrogate character.";

	public const string Xml_InvalidSurrogatePairWithArgs = "The surrogate pair (0x{0}, 0x{1}) is invalid. A high surrogate character (0xD800 - 0xDBFF) must always be paired with a low surrogate character (0xDC00 - 0xDFFF).";

	public const string Xml_RedefinePrefix = "The prefix '{0}' cannot be redefined from '{1}' to '{2}' within the same start element tag.";

	public const string Xml_DtdAlreadyWritten = "The DTD has already been written out.";

	public const string Xml_InvalidCharsInIndent = "XmlWriterSettings.{0} can contain only valid XML text content characters when XmlWriterSettings.CheckCharacters is true. {1}";

	public const string Xml_IndentCharsNotWhitespace = "XmlWriterSettings.{0} can contain only valid XML white space characters when XmlWriterSettings.CheckCharacters and XmlWriterSettings.NewLineOnAttributes are true.";

	public const string Xml_ConformanceLevelFragment = "Make sure that the ConformanceLevel setting is set to ConformanceLevel.Fragment or ConformanceLevel.Auto if you want to write an XML fragment.";

	public const string Xml_InvalidQuote = "Invalid XML attribute quote character. Valid attribute quote characters are ' and \".";

	public const string Xml_UndefPrefix = "An undefined prefix is in use.";

	public const string Xml_NoNamespaces = "Cannot set the namespace if Namespaces is 'false'.";

	public const string Xml_InvalidCDataChars = "Cannot have ']]>' inside an XML CDATA block.";

	public const string Xml_NotTheFirst = "WriteStartDocument needs to be the first call.";

	public const string Xml_InvalidPiChars = "Cannot have '?>' inside an XML processing instruction.";

	public const string Xml_InvalidNameChars = "Invalid name character in '{0}'.";

	public const string Xml_Closed = "The Writer is closed.";

	public const string Xml_InvalidPrefix = "Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML.";

	public const string Xml_InvalidIndentation = "Indentation value must be greater than 0.";

	public const string Xml_NotInWriteState = "NotInWriteState.";

	public const string Xml_SurrogatePairSplit = "The second character surrogate pair is not in the input buffer to be written.";

	public const string Xml_NoMultipleRoots = "Document cannot have multiple document elements.";

	public const string XmlBadName = "A node of type '{0}' cannot have the name '{1}'.";

	public const string XmlNoNameAllowed = "A node of type '{0}' cannot have a name.";

	public const string XmlConvert_BadUri = "The string was not recognized as a valid Uri.";

	public const string XmlConvert_BadFormat = "The string '{0}' is not a valid {1} value.";

	public const string XmlConvert_Overflow = "Value '{0}' was either too large or too small for {1}.";

	public const string XmlConvert_TypeBadMapping = "Xml type '{0}' does not support Clr type '{1}'.";

	public const string XmlConvert_TypeBadMapping2 = "Xml type '{0}' does not support a conversion from Clr type '{1}' to Clr type '{2}'.";

	public const string XmlConvert_TypeListBadMapping = "Xml type 'List of {0}' does not support Clr type '{1}'.";

	public const string XmlConvert_TypeListBadMapping2 = "Xml type 'List of {0}' does not support a conversion from Clr type '{1}' to Clr type '{2}'.";

	public const string XmlConvert_TypeToString = "Xml type '{0}' cannot convert from Clr type '{1}' unless the destination type is String or XmlAtomicValue.";

	public const string XmlConvert_TypeFromString = "Xml type '{0}' cannot convert to Clr type '{1}' unless the source value is a String or an XmlAtomicValue.";

	public const string XmlConvert_TypeNoPrefix = "The QName '{0}' cannot be represented as a String.  A prefix for namespace '{1}' cannot be found.";

	public const string XmlConvert_TypeNoNamespace = "The String '{0}' cannot be represented as an XmlQualifiedName.  A namespace for prefix '{1}' cannot be found.";

	public const string XmlConvert_NotOneCharString = "String must be exactly one character long.";

	public const string Sch_ParEntityRefNesting = "The parameter entity replacement text must nest properly within markup declarations.";

	public const string Sch_NotTokenString = "line-feed (#xA) or tab (#x9) characters, leading or trailing spaces and sequences of one or more spaces (#x20) are not allowed in 'xs:token'.";

	public const string Sch_XsdDateTimeCompare = "Cannot compare '{0}' and '{1}'.";

	public const string Sch_InvalidNullCast = "Cannot return null as a value for type '{0}'.";

	public const string Sch_InvalidDateTimeOption = "The '{0}' value for the 'dateTimeOption' parameter is not an allowed value for the 'XmlDateTimeSerializationMode' enumeration.";

	public const string Sch_StandAloneNormalization = "StandAlone is 'yes' and the value of the attribute '{0}' contains a definition in an external document that changes on normalization.";

	public const string Sch_UnSpecifiedDefaultAttributeInExternalStandalone = "Markup for unspecified default attribute '{0}' is external and standalone='yes'.";

	public const string Sch_DefaultException = "A schema error occurred.";

	public const string Sch_DupElementDecl = "The '{0}' element has already been declared.";

	public const string Sch_IdAttrDeclared = "The attribute of type ID is already declared on the '{0}' element.";

	public const string Sch_RootMatchDocType = "Root element name must match the DocType name.";

	public const string Sch_DupId = "'{0}' is already used as an ID.";

	public const string Sch_UndeclaredElement = "The '{0}' element is not declared.";

	public const string Sch_UndeclaredAttribute = "The '{0}' attribute is not declared.";

	public const string Sch_UndeclaredNotation = "The '{0}' notation is not declared.";

	public const string Sch_UndeclaredId = "Reference to undeclared ID is '{0}'.";

	public const string Sch_SchemaRootExpected = "Expected schema root. Make sure the root element is <schema> and the namespace is 'http://www.w3.org/2001/XMLSchema' for an XSD schema or 'urn:schemas-microsoft-com:xml-data' for an XDR schema.";

	public const string Sch_XSDSchemaRootExpected = "The root element of a W3C XML Schema should be <schema> and its namespace should be 'http://www.w3.org/2001/XMLSchema'.";

	public const string Sch_UnsupportedAttribute = "The '{0}' attribute is not supported in this context.";

	public const string Sch_UnsupportedElement = "The '{0}' element is not supported in this context.";

	public const string Sch_MissAttribute = "The '{0}' attribute is either invalid or missing.";

	public const string Sch_AnnotationLocation = "The 'annotation' element cannot appear at this location.";

	public const string Sch_DataTypeTextOnly = "Content must be \"textOnly\" when using DataType on an ElementType.";

	public const string Sch_UnknownModel = "The model attribute must have a value of open or closed, not '{0}'.";

	public const string Sch_UnknownOrder = "The order attribute must have a value of 'seq', 'one', or 'many', not '{0}'.";

	public const string Sch_UnknownContent = "The content attribute must have a value of 'textOnly', 'eltOnly', 'mixed', or 'empty', not '{0}'.";

	public const string Sch_UnknownRequired = "The required attribute must have a value of yes or no.";

	public const string Sch_UnknownDtType = "Reference to an unknown data type, '{0}'.";

	public const string Sch_MixedMany = "The order must be many when content is mixed.";

	public const string Sch_GroupDisabled = "The group is not allowed when ElementType has empty or textOnly content.";

	public const string Sch_MissDtvalue = "The DataType value cannot be empty.";

	public const string Sch_MissDtvaluesAttribute = "The dt:values attribute is missing.";

	public const string Sch_DupDtType = "Data type has already been declared.";

	public const string Sch_DupAttribute = "The '{0}' attribute has already been declared for this ElementType.";

	public const string Sch_RequireEnumeration = "Data type should be enumeration when the values attribute is present.";

	public const string Sch_DefaultIdValue = "An attribute or element of type xs:ID or derived from xs:ID, should not have a value constraint.";

	public const string Sch_ElementNotAllowed = "Element is not allowed when the content is empty or textOnly.";

	public const string Sch_ElementMissing = "There is a missing element.";

	public const string Sch_ManyMaxOccurs = "When the order is many, the maxOccurs attribute must have a value of '*'.";

	public const string Sch_MaxOccursInvalid = "The maxOccurs attribute must have a value of 1 or *.";

	public const string Sch_MinOccursInvalid = "The minOccurs attribute must have a value of 0 or 1.";

	public const string Sch_DtMaxLengthInvalid = "The value '{0}' is invalid for dt:maxLength.";

	public const string Sch_DtMinLengthInvalid = "The value '{0}' is invalid for dt:minLength.";

	public const string Sch_DupDtMaxLength = "The value of maxLength has already been declared.";

	public const string Sch_DupDtMinLength = "The value of minLength has already been declared.";

	public const string Sch_DtMinMaxLength = "The maxLength value must be equal to or greater than the minLength value.";

	public const string Sch_DupElement = "The '{0}' element already exists in the content model.";

	public const string Sch_DupGroupParticle = "The content model can only have one of the following; 'all', 'choice', or 'sequence'.";

	public const string Sch_InvalidValue = "The value '{0}' is invalid according to its data type.";

	public const string Sch_InvalidValueDetailed = "The value '{0}' is invalid according to its schema type '{1}' - {2}";

	public const string Sch_InvalidValueDetailedAttribute = "The attribute '{0}' has an invalid value '{1}' according to its schema type '{2}' - {3}";

	public const string Sch_MissRequiredAttribute = "The required attribute '{0}' is missing.";

	public const string Sch_FixedAttributeValue = "The value of the '{0}' attribute does not equal its fixed value.";

	public const string Sch_FixedElementValue = "The value of the '{0}' element does not equal its fixed value.";

	public const string Sch_AttributeValueDataTypeDetailed = "The '{0}' attribute is invalid - The value '{1}' is invalid according to its datatype '{2}' - {3}";

	public const string Sch_AttributeDefaultDataType = "The default value of '{0}' attribute is invalid according to its datatype.";

	public const string Sch_IncludeLocation = "The 'include' element cannot appear at this location.";

	public const string Sch_ImportLocation = "The 'import' element cannot appear at this location.";

	public const string Sch_RedefineLocation = "The 'redefine' element cannot appear at this location.";

	public const string Sch_InvalidBlockDefaultValue = "The values 'list' and 'union' are invalid for the blockDefault attribute.";

	public const string Sch_InvalidFinalDefaultValue = "The value 'substitution' is invalid for the finalDefault attribute.";

	public const string Sch_InvalidElementBlockValue = "The values 'list' and 'union' are invalid for the block attribute on element.";

	public const string Sch_InvalidElementFinalValue = "The values 'substitution', 'list', and 'union' are invalid for the final attribute on element.";

	public const string Sch_InvalidSimpleTypeFinalValue = "The values 'substitution' and 'extension' are invalid for the final attribute on simpleType.";

	public const string Sch_InvalidComplexTypeBlockValue = "The values 'substitution', 'list', and 'union' are invalid for the block attribute on complexType.";

	public const string Sch_InvalidComplexTypeFinalValue = "The values 'substitution', 'list', and 'union' are invalid for the final attribute on complexType.";

	public const string Sch_DupIdentityConstraint = "The identity constraint '{0}' has already been declared.";

	public const string Sch_DupGlobalElement = "The global element '{0}' has already been declared.";

	public const string Sch_DupGlobalAttribute = "The global attribute '{0}' has already been declared.";

	public const string Sch_DupSimpleType = "The simpleType '{0}' has already been declared.";

	public const string Sch_DupComplexType = "The complexType '{0}' has already been declared.";

	public const string Sch_DupGroup = "The group '{0}' has already been declared.";

	public const string Sch_DupAttributeGroup = "The attributeGroup '{0}' has already been declared.";

	public const string Sch_DupNotation = "The notation '{0}' has already been declared.";

	public const string Sch_DefaultFixedAttributes = "The fixed and default attributes cannot both be present.";

	public const string Sch_FixedInRef = "The fixed value constraint on the '{0}' attribute reference must match the fixed value constraint on the declaration.";

	public const string Sch_FixedDefaultInRef = "The default value constraint cannot be present on the '{0}' attribute reference if the fixed value constraint is present on the declaration.";

	public const string Sch_DupXsdElement = "'{0}' is a duplicate XSD element.";

	public const string Sch_ForbiddenAttribute = "The '{0}' attribute cannot be present.";

	public const string Sch_AttributeIgnored = "The '{0}' attribute is ignored, because the value of 'prohibited' for attribute use only prevents inheritance of an identically named attribute from the base type definition.";

	public const string Sch_ElementRef = "When the ref attribute is present, the type attribute and complexType, simpleType, key, keyref, and unique elements cannot be present.";

	public const string Sch_TypeMutualExclusive = "The type attribute cannot be present with either simpleType or complexType.";

	public const string Sch_ElementNameRef = "For element declaration, either the name or the ref attribute must be present.";

	public const string Sch_AttributeNameRef = "For attribute '{0}', either the name or the ref attribute must be present, but not both.";

	public const string Sch_TextNotAllowed = "The following text is not allowed in this context: '{0}'.";

	public const string Sch_UndeclaredType = "Type '{0}' is not declared.";

	public const string Sch_UndeclaredSimpleType = "Type '{0}' is not declared, or is not a simple type.";

	public const string Sch_UndeclaredEquivClass = "Substitution group refers to '{0}', an undeclared element.";

	public const string Sch_AttListPresence = "An attribute of type ID must have a declared default of either #IMPLIED or #REQUIRED.";

	public const string Sch_NotationValue = "'{0}' is not in the notation list.";

	public const string Sch_EnumerationValue = "'{0}' is not in the enumeration list.";

	public const string Sch_EmptyAttributeValue = "The attribute value cannot be empty.";

	public const string Sch_InvalidLanguageId = "'{0}' is an invalid language identifier.";

	public const string Sch_XmlSpace = "Invalid xml:space syntax.";

	public const string Sch_InvalidXsdAttributeValue = "'{1}' is an invalid value for the '{0}' attribute.";

	public const string Sch_InvalidXsdAttributeDatatypeValue = "The value for the '{0}' attribute is invalid - {1}";

	public const string Sch_ElementValueDataTypeDetailed = "The '{0}' element is invalid - The value '{1}' is invalid according to its datatype '{2}' - {3}";

	public const string Sch_InvalidElementDefaultValue = "The default value '{0}' of element '{1}' is invalid according to the type specified by xsi:type.";

	public const string Sch_NonDeterministic = "Multiple definition of element '{0}' causes the content model to become ambiguous. A content model must be formed such that during validation of an element information item sequence, the particle contained directly, indirectly or implicitly therein with which to attempt to validate each item in the sequence in turn can be uniquely determined without examining the content or attributes of that item, and without any information about the items in the remainder of the sequence.";

	public const string Sch_NonDeterministicAnyEx = "Wildcard '{0}' allows element '{1}', and causes the content model to become ambiguous. A content model must be formed such that during validation of an element information item sequence, the particle contained directly, indirectly or implicitly therein with which to attempt to validate each item in the sequence in turn can be uniquely determined without examining the content or attributes of that item, and without any information about the items in the remainder of the sequence.";

	public const string Sch_NonDeterministicAnyAny = "Wildcards '{0}' and '{1}' have not empty intersection, and causes the content model to become ambiguous. A content model must be formed such that during validation of an element information item sequence, the particle contained directly, indirectly or implicitly therein with which to attempt to validate each item in the sequence in turn can be uniquely determined without examining the content or attributes of that item, and without any information about the items in the remainder of the sequence.";

	public const string Sch_StandAlone = "The standalone document declaration must have a value of 'no'.";

	public const string Sch_XmlNsAttribute = "The value 'xmlns' cannot be used as the name of an attribute declaration.";

	public const string Sch_AllElement = "Element '{0}' cannot appear more than once if content model type is \"all\".";

	public const string Sch_MismatchTargetNamespaceInclude = "The targetNamespace '{0}' of included/redefined schema should be the same as the targetNamespace '{1}' of the including schema.";

	public const string Sch_MismatchTargetNamespaceImport = "The namespace attribute '{0}' of an import should be the same value as the targetNamespace '{1}' of the imported schema.";

	public const string Sch_MismatchTargetNamespaceEx = "The targetNamespace parameter '{0}' should be the same value as the targetNamespace '{1}' of the schema.";

	public const string Sch_XsiTypeNotFound = "This is an invalid xsi:type '{0}'.";

	public const string Sch_XsiTypeAbstract = "The xsi:type '{0}' cannot be abstract.";

	public const string Sch_ListFromNonatomic = "A list data type must be derived from an atomic or union data type.";

	public const string Sch_UnionFromUnion = "It is an error if a union type has a member with variety union and this member cannot be substituted with its own members. This may be due to the fact that the union member is a restriction of a union with facets.";

	public const string Sch_DupLengthFacet = "This is a duplicate Length constraining facet.";

	public const string Sch_DupMinLengthFacet = "This is a duplicate MinLength constraining facet.";

	public const string Sch_DupMaxLengthFacet = "This is a duplicate MaxLength constraining facet.";

	public const string Sch_DupWhiteSpaceFacet = "This is a duplicate WhiteSpace constraining facet.";

	public const string Sch_DupMaxInclusiveFacet = "This is a duplicate MaxInclusive constraining facet.";

	public const string Sch_DupMaxExclusiveFacet = "This is a duplicate MaxExclusive constraining facet.";

	public const string Sch_DupMinInclusiveFacet = "This is a duplicate MinInclusive constraining facet.";

	public const string Sch_DupMinExclusiveFacet = "This is a duplicate MinExclusive constraining facet.";

	public const string Sch_DupTotalDigitsFacet = "This is a duplicate TotalDigits constraining facet.";

	public const string Sch_DupFractionDigitsFacet = "This is a duplicate FractionDigits constraining facet.";

	public const string Sch_LengthFacetProhibited = "The length constraining facet is prohibited for '{0}'.";

	public const string Sch_MinLengthFacetProhibited = "The MinLength constraining facet is prohibited for '{0}'.";

	public const string Sch_MaxLengthFacetProhibited = "The MaxLength constraining facet is prohibited for '{0}'.";

	public const string Sch_PatternFacetProhibited = "The Pattern constraining facet is prohibited for '{0}'.";

	public const string Sch_EnumerationFacetProhibited = "The Enumeration constraining facet is prohibited for '{0}'.";

	public const string Sch_WhiteSpaceFacetProhibited = "The WhiteSpace constraining facet is prohibited for '{0}'.";

	public const string Sch_MaxInclusiveFacetProhibited = "The MaxInclusive constraining facet is prohibited for '{0}'.";

	public const string Sch_MaxExclusiveFacetProhibited = "The MaxExclusive constraining facet is prohibited for '{0}'.";

	public const string Sch_MinInclusiveFacetProhibited = "The MinInclusive constraining facet is prohibited for '{0}'.";

	public const string Sch_MinExclusiveFacetProhibited = "The MinExclusive constraining facet is prohibited for '{0}'.";

	public const string Sch_TotalDigitsFacetProhibited = "The TotalDigits constraining facet is prohibited for '{0}'.";

	public const string Sch_FractionDigitsFacetProhibited = "The FractionDigits constraining facet is prohibited for '{0}'.";

	public const string Sch_LengthFacetInvalid = "The Length constraining facet is invalid - {0}";

	public const string Sch_MinLengthFacetInvalid = "The MinLength constraining facet is invalid - {0}";

	public const string Sch_MaxLengthFacetInvalid = "The MaxLength constraining facet is invalid - {0}";

	public const string Sch_MaxInclusiveFacetInvalid = "The MaxInclusive constraining facet is invalid - {0}";

	public const string Sch_MaxExclusiveFacetInvalid = "The MaxExclusive constraining facet is invalid - {0}";

	public const string Sch_MinInclusiveFacetInvalid = "The MinInclusive constraining facet is invalid - {0}";

	public const string Sch_MinExclusiveFacetInvalid = "The MinExclusive constraining facet is invalid - {0}";

	public const string Sch_TotalDigitsFacetInvalid = "The TotalDigits constraining facet is invalid - {0}";

	public const string Sch_FractionDigitsFacetInvalid = "The FractionDigits constraining facet is invalid - {0}";

	public const string Sch_PatternFacetInvalid = "The Pattern constraining facet is invalid - {0}";

	public const string Sch_EnumerationFacetInvalid = "The Enumeration constraining facet is invalid - {0}";

	public const string Sch_InvalidWhiteSpace = "The white space character, '{0}', is invalid.";

	public const string Sch_UnknownFacet = "This is an unknown facet.";

	public const string Sch_LengthAndMinMax = "It is an error for both length and minLength or maxLength to be present.";

	public const string Sch_MinLengthGtMaxLength = "MinLength is greater than MaxLength.";

	public const string Sch_FractionDigitsGtTotalDigits = "FractionDigits is greater than TotalDigits.";

	public const string Sch_LengthConstraintFailed = "The actual length is not equal to the specified length.";

	public const string Sch_MinLengthConstraintFailed = "The actual length is less than the MinLength value.";

	public const string Sch_MaxLengthConstraintFailed = "The actual length is greater than the MaxLength value.";

	public const string Sch_PatternConstraintFailed = "The Pattern constraint failed.";

	public const string Sch_EnumerationConstraintFailed = "The Enumeration constraint failed.";

	public const string Sch_MaxInclusiveConstraintFailed = "The MaxInclusive constraint failed.";

	public const string Sch_MaxExclusiveConstraintFailed = "The MaxExclusive constraint failed.";

	public const string Sch_MinInclusiveConstraintFailed = "The MinInclusive constraint failed.";

	public const string Sch_MinExclusiveConstraintFailed = "The MinExclusive constraint failed.";

	public const string Sch_TotalDigitsConstraintFailed = "The TotalDigits constraint failed.";

	public const string Sch_FractionDigitsConstraintFailed = "The FractionDigits constraint failed.";

	public const string Sch_UnionFailedEx = "The value '{0}' is not valid according to any of the memberTypes of the union.";

	public const string Sch_NotationRequired = "NOTATION cannot be used directly in a schema; only data types derived from NOTATION by specifying an enumeration value can be used in a schema. All enumeration facet values must match the name of a notation declared in the current schema.";

	public const string Sch_DupNotationAttribute = "No element type can have more than one NOTATION attribute specified.";

	public const string Sch_MissingPublicSystemAttribute = "NOTATION must have either the Public or System attribute present.";

	public const string Sch_NotationAttributeOnEmptyElement = "An attribute of type NOTATION must not be declared on an element declared EMPTY.";

	public const string Sch_RefNotInScope = "The Keyref '{0}' cannot find the referred key or unique in scope.";

	public const string Sch_UndeclaredIdentityConstraint = "The '{0}' identity constraint is not declared.";

	public const string Sch_RefInvalidIdentityConstraint = "Reference to an invalid identity constraint, '{0}'.";

	public const string Sch_RefInvalidCardin = "Keyref '{0}' has different cardinality as the referred key or unique element.";

	public const string Sch_ReftoKeyref = "The '{0}' Keyref can refer to key or unique only.";

	public const string Sch_EmptyXPath = "The XPath for selector or field cannot be empty.";

	public const string Sch_UnresolvedPrefix = "The prefix '{0}' in XPath cannot be resolved.";

	public const string Sch_UnresolvedKeyref = "The key sequence '{0}' in '{1}' Keyref fails to refer to some key.";

	public const string Sch_ICXpathError = "'{0}' is an invalid XPath for selector or field.";

	public const string Sch_SelectorAttr = "'{0}' is an invalid XPath for selector. Selector cannot have an XPath selection with an attribute node.";

	public const string Sch_FieldSimpleTypeExpected = "The field '{0}' is expecting an element or attribute with simple type or simple content.";

	public const string Sch_FieldSingleValueExpected = "The field '{0}' is expecting at the most one value.";

	public const string Sch_MissingKey = "The identity constraint '{0}' validation has failed. Either a key is missing or the existing key has an empty node.";

	public const string Sch_DuplicateKey = "There is a duplicate key sequence '{0}' for the '{1}' key or unique identity constraint.";

	public const string Sch_TargetNamespaceXsi = "The target namespace of an attribute declaration, whether local or global, must not match http://www.w3.org/2001/XMLSchema-instance.";

	public const string Sch_UndeclaredEntity = "Reference to an undeclared entity, '{0}'.";

	public const string Sch_UnparsedEntityRef = "Reference to an unparsed entity, '{0}'.";

	public const string Sch_MaxOccursInvalidXsd = "The value for the 'maxOccurs' attribute must be xsd:nonNegativeInteger or 'unbounded'.";

	public const string Sch_MinOccursInvalidXsd = "The value for the 'minOccurs' attribute must be xsd:nonNegativeInteger.";

	public const string Sch_MaxInclusiveExclusive = "'maxInclusive' and 'maxExclusive' cannot both be specified for the same data type.";

	public const string Sch_MinInclusiveExclusive = "'minInclusive' and 'minExclusive' cannot both be specified for the same data type.";

	public const string Sch_MinInclusiveGtMaxInclusive = "The value specified for 'minInclusive' cannot be greater than the value specified for 'maxInclusive' for the same data type.";

	public const string Sch_MinExclusiveGtMaxExclusive = "The value specified for 'minExclusive' cannot be greater than the value specified for 'maxExclusive' for the same data type.";

	public const string Sch_MinInclusiveGtMaxExclusive = "The value specified for 'minInclusive' cannot be greater than the value specified for 'maxExclusive' for the same data type.";

	public const string Sch_MinExclusiveGtMaxInclusive = "The value specified for 'minExclusive' cannot be greater than the value specified for 'maxInclusive' for the same data type.";

	public const string Sch_SimpleTypeRestriction = "'simpleType' should be the first child of restriction.";

	public const string Sch_InvalidFacetPosition = "Facet should go before 'attribute', 'attributeGroup', or 'anyAttribute'.";

	public const string Sch_AttributeMutuallyExclusive = "'{0}' and content model are mutually exclusive.";

	public const string Sch_AnyAttributeLastChild = "'anyAttribute' must be the last child.";

	public const string Sch_ComplexTypeContentModel = "The content model of a complex type must consist of 'annotation' (if present); followed by zero or one of the following: 'simpleContent', 'complexContent', 'group', 'choice', 'sequence', or 'all'; followed by zero or more 'attribute' or 'attributeGroup'; followed by zero or one 'anyAttribute'.";

	public const string Sch_ComplexContentContentModel = "Complex content restriction or extension should consist of zero or one of 'group', 'choice', 'sequence', or 'all'; followed by zero or more 'attribute' or 'attributeGroup'; followed by zero or one 'anyAttribute'.";

	public const string Sch_NotNormalizedString = "Carriage return (#xD), line feed (#xA), and tab (#x9) characters are not allowed in xs:normalizedString.";

	public const string Sch_FractionDigitsNotOnDecimal = "FractionDigits should be equal to 0 on types other then decimal.";

	public const string Sch_ContentInNill = "Element '{0}' must have no character or element children.";

	public const string Sch_NoElementSchemaFound = "Could not find schema information for the element '{0}'.";

	public const string Sch_NoAttributeSchemaFound = "Could not find schema information for the attribute '{0}'.";

	public const string Sch_InvalidNamespace = "The Namespace '{0}' is an invalid URI.";

	public const string Sch_InvalidTargetNamespaceAttribute = "The targetNamespace attribute cannot have empty string as its value.";

	public const string Sch_InvalidNamespaceAttribute = "The namespace attribute cannot have empty string as its value.";

	public const string Sch_InvalidSchemaLocation = "The SchemaLocation '{0}' is an invalid URI.";

	public const string Sch_ImportTargetNamespace = "Namespace attribute of an import must not match the real value of the enclosing targetNamespace of the <schema>.";

	public const string Sch_ImportTargetNamespaceNull = "The enclosing <schema> must have a targetNamespace, if the Namespace attribute is absent on the import element.";

	public const string Sch_GroupDoubleRedefine = "Double redefine for group.";

	public const string Sch_ComponentRedefineNotFound = "Cannot find a {0} with name '{1}' to redefine.";

	public const string Sch_GroupRedefineNotFound = "No group to redefine.";

	public const string Sch_AttrGroupDoubleRedefine = "Double redefine for attribute group.";

	public const string Sch_AttrGroupRedefineNotFound = "No attribute group to redefine.";

	public const string Sch_ComplexTypeDoubleRedefine = "Double redefine for complex type.";

	public const string Sch_ComplexTypeRedefineNotFound = "No complex type to redefine.";

	public const string Sch_SimpleToComplexTypeRedefine = "Cannot redefine a simple type as complex type.";

	public const string Sch_SimpleTypeDoubleRedefine = "Double redefine for simple type.";

	public const string Sch_ComplexToSimpleTypeRedefine = "Cannot redefine a complex type as simple type.";

	public const string Sch_SimpleTypeRedefineNotFound = "No simple type to redefine.";

	public const string Sch_MinMaxGroupRedefine = "When group is redefined, the real value of both minOccurs and maxOccurs attribute must be 1 (or absent).";

	public const string Sch_MultipleGroupSelfRef = "Multiple self-reference within a group is redefined.";

	public const string Sch_MultipleAttrGroupSelfRef = "Multiple self-reference within an attribute group is redefined.";

	public const string Sch_InvalidTypeRedefine = "If type is being redefined, the base type has to be self-referenced.";

	public const string Sch_InvalidElementRef = "If ref is present, all of <complexType>, <simpleType>, <key>, <keyref>, <unique>, nillable, default, fixed, form, block, and type must be absent.";

	public const string Sch_MinGtMax = "minOccurs value cannot be greater than maxOccurs value.";

	public const string Sch_DupSelector = "Selector cannot appear twice in one identity constraint.";

	public const string Sch_IdConstraintNoSelector = "Selector must be present.";

	public const string Sch_IdConstraintNoFields = "At least one field must be present.";

	public const string Sch_IdConstraintNoRefer = "The referring attribute must be present.";

	public const string Sch_SelectorBeforeFields = "Cannot define fields before selector.";

	public const string Sch_NoSimpleTypeContent = "SimpleType content is missing.";

	public const string Sch_SimpleTypeRestRefBase = "SimpleType restriction should have either the base attribute or a simpleType child, but not both.";

	public const string Sch_SimpleTypeRestRefBaseNone = "SimpleType restriction should have either the base attribute or a simpleType child to indicate the base type for the derivation.";

	public const string Sch_SimpleTypeListRefBase = "SimpleType list should have either the itemType attribute or a simpleType child, but not both.";

	public const string Sch_SimpleTypeListRefBaseNone = "SimpleType list should have either the itemType attribute or a simpleType child to indicate the itemType of the list.";

	public const string Sch_SimpleTypeUnionNoBase = "Either the memberTypes attribute must be non-empty or there must be at least one simpleType child.";

	public const string Sch_NoRestOrExtQName = "'restriction' or 'extension' child is required for complexType '{0}' in namespace '{1}', because it has a simpleContent or complexContent child.";

	public const string Sch_NoRestOrExt = "'restriction' or 'extension' child is required for complexType with simpleContent or complexContent child.";

	public const string Sch_NoGroupParticle = "'sequence', 'choice', or 'all' child is required.";

	public const string Sch_InvalidAllMin = "'all' must have 'minOccurs' value of 0 or 1.";

	public const string Sch_InvalidAllMax = "'all' must have {max occurs}=1.";

	public const string Sch_InvalidFacet = "The 'value' attribute must be present in facet.";

	public const string Sch_AbstractElement = "The element '{0}' is abstract or its type is abstract.";

	public const string Sch_XsiTypeBlockedEx = "The xsi:type attribute value '{0}' is not valid for the element '{1}', either because it is not a type validly derived from the type in the schema, or because it has xsi:type derivation blocked.";

	public const string Sch_InvalidXsiNill = "If the 'nillable' attribute is false in the schema, the 'xsi:nil' attribute must not be present in the instance.";

	public const string Sch_SubstitutionNotAllowed = "Element '{0}' cannot substitute in place of head element '{1}' because it has block='substitution'.";

	public const string Sch_SubstitutionBlocked = "Member element {0}'s type cannot be derived by restriction or extension from head element {1}'s type, because it has block='restriction' or 'extension'.";

	public const string Sch_InvalidElementInEmptyEx = "The element '{0}' cannot contain child element '{1}' because the parent element's content model is empty.";

	public const string Sch_InvalidElementInTextOnlyEx = "The element '{0}' cannot contain child element '{1}' because the parent element's content model is text only.";

	public const string Sch_InvalidTextInElement = "The element {0} cannot contain text.";

	public const string Sch_InvalidElementContent = "The element {0} has invalid child element {1}.";

	public const string Sch_InvalidElementContentComplex = "The element {0} has invalid child element {1} - {2}";

	public const string Sch_IncompleteContent = "The element {0} has incomplete content.";

	public const string Sch_IncompleteContentComplex = "The element {0} has incomplete content - {2}";

	public const string Sch_InvalidTextInElementExpecting = "The element {0} cannot contain text. List of possible elements expected: {1}.";

	public const string Sch_InvalidElementContentExpecting = "The element {0} has invalid child element {1}. List of possible elements expected: {2}.";

	public const string Sch_InvalidElementContentExpectingComplex = "The element {0} has invalid child element {1}. List of possible elements expected: {2}. {3}";

	public const string Sch_IncompleteContentExpecting = "The element {0} has incomplete content. List of possible elements expected: {1}.";

	public const string Sch_IncompleteContentExpectingComplex = "The element {0} has incomplete content. List of possible elements expected: {1}. {2}";

	public const string Sch_InvalidElementSubstitution = "The element {0} cannot substitute for a local element {1} expected in that position.";

	public const string Sch_ElementNameAndNamespace = "'{0}' in namespace '{1}'";

	public const string Sch_ElementName = "'{0}'";

	public const string Sch_ContinuationString = "{0}as well as";

	public const string Sch_AnyElementNS = "any element in namespace '{0}'";

	public const string Sch_AnyElement = "any element";

	public const string Sch_InvalidTextInEmpty = "The element cannot contain text. Content model is empty.";

	public const string Sch_InvalidWhitespaceInEmpty = "The element cannot contain white space. Content model is empty.";

	public const string Sch_InvalidPIComment = "The element cannot contain comment or processing instruction. Content model is empty.";

	public const string Sch_InvalidAttributeRef = "If ref is present, all of 'simpleType', 'form', 'type', and 'use' must be absent.";

	public const string Sch_OptionalDefaultAttribute = "The 'use' attribute must be optional (or absent) if the default attribute is present.";

	public const string Sch_AttributeCircularRef = "Circular attribute reference.";

	public const string Sch_IdentityConstraintCircularRef = "Circular identity constraint reference.";

	public const string Sch_SubstitutionCircularRef = "Circular substitution group affiliation.";

	public const string Sch_InvalidAnyAttribute = "Invalid namespace in 'anyAttribute'.";

	public const string Sch_DupIdAttribute = "Duplicate ID attribute.";

	public const string Sch_InvalidAllElementMax = "The {max occurs} of all the particles in the {particles} of an all group must be 0 or 1.";

	public const string Sch_InvalidAny = "Invalid namespace in 'any'.";

	public const string Sch_InvalidAnyDetailed = "The value of the namespace attribute of the element or attribute wildcard is invalid - {0}";

	public const string Sch_InvalidExamplar = "Cannot be nominated as the {substitution group affiliation} of any other declaration.";

	public const string Sch_NoExamplar = "Reference to undeclared substitution group affiliation.";

	public const string Sch_InvalidSubstitutionMember = "'{0}' cannot be a member of substitution group with head element '{1}'.";

	public const string Sch_RedefineNoSchema = "'SchemaLocation' must successfully resolve if <redefine> contains any child other than <annotation>.";

	public const string Sch_ProhibitedAttribute = "The '{0}' attribute is not allowed.";

	public const string Sch_TypeCircularRef = "Circular type reference.";

	public const string Sch_TwoIdAttrUses = "Two distinct members of the attribute uses must not have type definitions which are both xs:ID or are derived from xs:ID.";

	public const string Sch_AttrUseAndWildId = "It is an error if there is a member of the attribute uses of a type definition with type xs:ID or derived from xs:ID and another attribute with type xs:ID matches an attribute wildcard.";

	public const string Sch_MoreThanOneWildId = "It is an error if more than one attribute whose type is xs:ID or is derived from xs:ID, matches an attribute wildcard on an element.";

	public const string Sch_BaseFinalExtension = "The base type is the final extension.";

	public const string Sch_NotSimpleContent = "The content type of the base type must be a simple type definition or it must be mixed, and simpleType child must be present.";

	public const string Sch_NotComplexContent = "The content type of the base type must not be a simple type definition.";

	public const string Sch_BaseFinalRestriction = "The base type is final restriction.";

	public const string Sch_BaseFinalList = "The base type is the final list.";

	public const string Sch_BaseFinalUnion = "The base type is the final union.";

	public const string Sch_UndefBaseRestriction = "Undefined complexType '{0}' is used as a base for complex type restriction.";

	public const string Sch_UndefBaseExtension = "Undefined complexType '{0}' is used as a base for complex type extension.";

	public const string Sch_DifContentType = "The derived type and the base type must have the same content type.";

	public const string Sch_InvalidContentRestriction = "Invalid content type derivation by restriction.";

	public const string Sch_InvalidContentRestrictionDetailed = "Invalid content type derivation by restriction. {0}";

	public const string Sch_InvalidBaseToEmpty = "If the derived content type is Empty, then the base content type should also be Empty or Mixed with Emptiable particle according to rule 5.3 of Schema Component Constraint: Derivation Valid (Restriction, Complex).";

	public const string Sch_InvalidBaseToMixed = "If the derived content type is Mixed, then the base content type should also be Mixed according to rule 5.4 of Schema Component Constraint: Derivation Valid (Restriction, Complex).";

	public const string Sch_DupAttributeUse = "The attribute '{0}' already exists.";

	public const string Sch_InvalidParticleRestriction = "Invalid particle derivation by restriction.";

	public const string Sch_InvalidParticleRestrictionDetailed = "Invalid particle derivation by restriction - '{0}'.";

	public const string Sch_ForbiddenDerivedParticleForAll = "'Choice' or 'any' is forbidden as derived particle when the base particle is 'all'.";

	public const string Sch_ForbiddenDerivedParticleForElem = "Only 'element' is valid as derived particle when the base particle is 'element'.";

	public const string Sch_ForbiddenDerivedParticleForChoice = "'All' or 'any' is forbidden as derived particle when the base particle is 'choice'.";

	public const string Sch_ForbiddenDerivedParticleForSeq = "'All', 'any', and 'choice' are forbidden as derived particles when the base particle is 'sequence'.";

	public const string Sch_ElementFromElement = "Derived element '{0}' is not a valid restriction of base element '{1}' according to Elt:Elt -- NameAndTypeOK.";

	public const string Sch_ElementFromAnyRule1 = "The namespace of element '{0}'is not valid with respect to the wildcard's namespace constraint in the base, Elt:Any -- NSCompat Rule 1.";

	public const string Sch_ElementFromAnyRule2 = "The occurrence range of element '{0}'is not a valid restriction of the wildcard's occurrence range in the base, Elt:Any -- NSCompat Rule2.";

	public const string Sch_AnyFromAnyRule1 = "The derived wildcard's occurrence range is not a valid restriction of the base wildcard's occurrence range, Any:Any -- NSSubset Rule 1.";

	public const string Sch_AnyFromAnyRule2 = "The derived wildcard's namespace constraint must be an intensional subset of the base wildcard's namespace constraint, Any:Any -- NSSubset Rule2.";

	public const string Sch_AnyFromAnyRule3 = "The derived wildcard's 'processContents' must be identical to or stronger than the base wildcard's 'processContents', where 'strict' is stronger than 'lax' and 'lax' is stronger than 'skip', Any:Any -- NSSubset Rule 3.";

	public const string Sch_GroupBaseFromAny1 = "Every member of the derived group particle must be a valid restriction of the base wildcard, NSRecurseCheckCardinality Rule 1.";

	public const string Sch_GroupBaseFromAny2 = "The derived particle's occurrence range at ({0}, {1}) is not a valid restriction of the base wildcard's occurrence range at ({2}, {3}), NSRecurseCheckCardinality Rule 2.";

	public const string Sch_ElementFromGroupBase1 = "The derived element {0} at ({1}, {2}) is not a valid restriction of the base sequence particle at ({3}, {4}) according to Elt:All/Choice/Sequence -- RecurseAsIfGroup.";

	public const string Sch_ElementFromGroupBase2 = "The derived element {0} at ({1}, {2}) is not a valid restriction of the base choice particle at ({3}, {4}) according to Elt:All/Choice/Sequence -- RecurseAsIfGroup.";

	public const string Sch_ElementFromGroupBase3 = "The derived element {0} at ({1}, {2}) is not a valid restriction of the base all particle at ({3}, {4}) according to Elt:All/Choice/Sequence -- RecurseAsIfGroup.";

	public const string Sch_GroupBaseRestRangeInvalid = "The derived particle's range is not a valid restriction of the base particle's range according to All:All,Sequence:Sequence -- Recurse Rule 1 or Choice:Choice -- RecurseLax.";

	public const string Sch_GroupBaseRestNoMap = "The derived particle cannot have more members than the base particle - All:All,Sequence:Sequence -- Recurse Rule 2 / Choice:Choice -- RecurseLax.";

	public const string Sch_GroupBaseRestNotEmptiable = "All particles in the {particles} of the base particle which are not mapped to by any particle in the {particles} of the derived particle should be emptiable - All:All,Sequence:Sequence -- Recurse Rule 2 / Choice:Choice -- RecurseLax.";

	public const string Sch_SeqFromAll = "The derived sequence particle at ({0}, {1}) is not a valid restriction of the base all particle at ({2}, {3}) according to Sequence:All -- RecurseUnordered.";

	public const string Sch_SeqFromChoice = "The derived sequence particle at ({0}, {1}) is not a valid restriction of the base choice particle at ({2}, {3}) according to Sequence:Choice -- MapAndSum.";

	public const string Sch_UndefGroupRef = "Reference to undeclared model group '{0}'.";

	public const string Sch_GroupCircularRef = "Circular group reference.";

	public const string Sch_AllRefNotRoot = "The group ref to 'all' is not the root particle, or it is being used as an extension.";

	public const string Sch_AllRefMinMax = "The group ref to 'all' must have {min occurs}= 0 or 1 and {max occurs}=1.";

	public const string Sch_NotAllAlone = "'all' is not the only particle in a group, or is being used as an extension.";

	public const string Sch_AttributeGroupCircularRef = "Circular attribute group reference.";

	public const string Sch_UndefAttributeGroupRef = "Reference to undeclared attribute group '{0}'.";

	public const string Sch_InvalidAttributeExtension = "Invalid attribute extension.";

	public const string Sch_InvalidAnyAttributeRestriction = "The base any attribute must be a superset of the derived 'anyAttribute'.";

	public const string Sch_AttributeRestrictionProhibited = "Invalid attribute restriction. Attribute restriction is prohibited in base type.";

	public const string Sch_AttributeRestrictionInvalid = "Invalid attribute restriction. Derived attribute's type is not a valid restriction of the base attribute's type.";

	public const string Sch_AttributeFixedInvalid = "Invalid attribute restriction. Derived attribute's fixed value must be the same as the base attribute's fixed value.";

	public const string Sch_AttributeUseInvalid = "Derived attribute's use has to be required if base attribute's use is required.";

	public const string Sch_AttributeRestrictionInvalidFromWildcard = "The {base type definition} must have an {attribute wildcard} and the {target namespace} of the R's {attribute declaration} must be valid with respect to that wildcard.";

	public const string Sch_NoDerivedAttribute = "The base attribute '{0}' whose use = 'required' does not have a corresponding derived attribute while redefining attribute group '{1}'.";

	public const string Sch_UnexpressibleAnyAttribute = "The 'anyAttribute' is not expressible.";

	public const string Sch_RefInvalidAttribute = "Reference to invalid attribute '{0}'.";

	public const string Sch_ElementCircularRef = "Circular element reference.";

	public const string Sch_RefInvalidElement = "Reference to invalid element '{0}'.";

	public const string Sch_ElementCannotHaveValue = "Element's type does not allow fixed or default value constraint.";

	public const string Sch_ElementInMixedWithFixed = "Although the '{0}' element's content type is mixed, it cannot have element children, because it has a fixed value constraint in the schema.";

	public const string Sch_ElementTypeCollision = "Elements with the same name and in the same scope must have the same type.";

	public const string Sch_InvalidIncludeLocation = "Cannot resolve the 'schemaLocation' attribute.";

	public const string Sch_CannotLoadSchema = "Cannot load the schema for the namespace '{0}' - {1}";

	public const string Sch_CannotLoadSchemaLocation = "Cannot load the schema from the location '{0}' - {1}";

	public const string Sch_LengthGtBaseLength = "It is an error if 'length' is among the members of {facets} of {base type definition} and {value} is greater than the {value} of the parent 'length'.";

	public const string Sch_MinLengthGtBaseMinLength = "It is an error if 'minLength' is among the members of {facets} of {base type definition} and {value} is less than the {value} of the parent 'minLength'.";

	public const string Sch_MaxLengthGtBaseMaxLength = "It is an error if 'maxLength' is among the members of {facets} of {base type definition} and {value} is greater than the {value} of the parent 'maxLength'.";

	public const string Sch_MaxMinLengthBaseLength = "It is an error for both 'length' and either 'minLength' or 'maxLength' to be members of {facets}, unless they are specified in different derivation steps. In which case the following must be true: the {value} of 'minLength' <= the {value} of 'length' <= the {value} of 'maxLength'.";

	public const string Sch_MaxInclusiveMismatch = "It is an error if the derived 'maxInclusive' facet value is greater than the parent 'maxInclusive' facet value.";

	public const string Sch_MaxExclusiveMismatch = "It is an error if the derived 'maxExclusive' facet value is greater than the parent 'maxExclusive' facet value.";

	public const string Sch_MinInclusiveMismatch = "It is an error if the derived 'minInclusive' facet value is less than the parent 'minInclusive' facet value.";

	public const string Sch_MinExclusiveMismatch = "It is an error if the derived 'minExclusive' facet value is less than the parent 'minExclusive' facet value.";

	public const string Sch_MinExlIncMismatch = "It is an error if the derived 'minExclusive' facet value is less than or equal to the parent 'minInclusive' facet value.";

	public const string Sch_MinExlMaxExlMismatch = "It is an error if the derived 'minExclusive' facet value is greater than or equal to the parent 'maxExclusive' facet value.";

	public const string Sch_MinIncMaxExlMismatch = "It is an error if the derived 'minInclusive' facet value is greater than or equal to the parent 'maxExclusive' facet value.";

	public const string Sch_MinIncExlMismatch = "It is an error if the derived 'minInclusive' facet value is less than or equal to the parent 'minExclusive' facet value.";

	public const string Sch_MaxIncExlMismatch = "It is an error if the derived 'maxInclusive' facet value is greater than or equal to the parent 'maxExclusive' facet value.";

	public const string Sch_MaxExlIncMismatch = "It is an error if the derived 'maxExclusive' facet value is greater than or equal to the parent 'maxInclusive' facet value.";

	public const string Sch_TotalDigitsMismatch = "It is an error if the derived 'totalDigits' facet value is greater than the parent 'totalDigits' facet value.";

	public const string Sch_FacetBaseFixed = "Values that are declared as {fixed} in a base type can not be changed in a derived type.";

	public const string Sch_WhiteSpaceRestriction1 = "It is an error if 'whiteSpace' is among the members of {facets} of {base type definition}, {value} is 'replace' or 'preserve', and the {value} of the parent 'whiteSpace' is 'collapse'.";

	public const string Sch_WhiteSpaceRestriction2 = "It is an error if 'whiteSpace' is among the members of {facets} of {base type definition}, {value} is 'preserve', and the {value} of the parent 'whiteSpace' is 'replace'.";

	public const string Sch_XsiNilAndFixed = "There must be no fixed value when an attribute is 'xsi:nil' and has a value of 'true'.";

	public const string Sch_MixSchemaTypes = "Different schema types cannot be mixed.";

	public const string Sch_XSDSchemaOnly = "'XmlSchemaSet' can load only W3C XML Schemas.";

	public const string Sch_InvalidPublicAttribute = "Public attribute '{0}' is an invalid URI.";

	public const string Sch_InvalidSystemAttribute = "System attribute '{0}' is an invalid URI.";

	public const string Sch_TypeAfterConstraints = "'simpleType' or 'complexType' cannot follow 'unique', 'key' or 'keyref'.";

	public const string Sch_XsiNilAndType = "There can be no type value when attribute is 'xsi:nil' and has value 'true'.";

	public const string Sch_DupSimpleTypeChild = "'simpleType' should have only one child 'union', 'list', or 'restriction'.";

	public const string Sch_InvalidIdAttribute = "Invalid 'id' attribute value: {0}";

	public const string Sch_InvalidNameAttributeEx = "Invalid 'name' attribute value '{0}': '{1}'.";

	public const string Sch_InvalidAttribute = "Invalid '{0}' attribute: '{1}'.";

	public const string Sch_EmptyChoice = "Empty choice cannot be satisfied if 'minOccurs' is not equal to 0.";

	public const string Sch_DerivedNotFromBase = "The data type of the simple content is not a valid restriction of the base complex type.";

	public const string Sch_NeedSimpleTypeChild = "Simple content restriction must have a simple type child if the content type of the base type is not a simple type definition.";

	public const string Sch_InvalidCollection = "The schema items collection cannot contain an object of type 'XmlSchemaInclude', 'XmlSchemaImport', or 'XmlSchemaRedefine'.";

	public const string Sch_UnrefNS = "Namespace '{0}' is not available to be referenced in this schema.";

	public const string Sch_InvalidSimpleTypeRestriction = "Restriction of 'anySimpleType' is not allowed.";

	public const string Sch_MultipleRedefine = "Multiple redefines of the same schema will be ignored.";

	public const string Sch_NullValue = "Value cannot be null.";

	public const string Sch_ComplexContentModel = "Content model validation resulted in a large number of states, possibly due to large occurrence ranges. Therefore, content model may not be validated accurately.";

	public const string Sch_SchemaNotPreprocessed = "All schemas in the set should be successfully preprocessed prior to compilation.";

	public const string Sch_SchemaNotRemoved = "The schema could not be removed because other schemas in the set have dependencies on this schema or its imports.";

	public const string Sch_ComponentAlreadySeenForNS = "An element or attribute information item has already been validated from the '{0}' namespace. It is an error if 'xsi:schemaLocation', 'xsi:noNamespaceSchemaLocation', or an inline schema occurs for that namespace.";

	public const string Sch_DefaultAttributeNotApplied = "Default attribute '{0}' for element '{1}' could not be applied as the attribute namespace is not mapped to a prefix in the instance document.";

	public const string Sch_NotXsiAttribute = "The attribute '{0}' does not match one of the four allowed attributes in the 'xsi' namespace.";

	public const string Sch_SchemaDoesNotExist = "Schema does not exist in the set.";

	public const string XmlDocument_ValidateInvalidNodeType = "Validate method can be called only on nodes of type Document, DocumentFragment, Element, or Attribute.";

	public const string XmlDocument_NodeNotFromDocument = "Cannot validate '{0}' because its owner document is not the current document.";

	public const string XmlDocument_NoNodeSchemaInfo = "Schema information could not be found for the node passed into Validate. The node may be invalid in its current position. Navigate to the ancestor that has schema information, then call Validate again.";

	public const string XmlDocument_NoSchemaInfo = "The XmlSchemaSet on the document is either null or has no schemas in it. Provide schema information before calling Validate.";

	public const string Sch_InvalidStartTransition = "It is invalid to call the '{0}' method in the current state of the validator. The '{1}' method must be called before proceeding with validation.";

	public const string Sch_InvalidStateTransition = "The transition from the '{0}' method to the '{1}' method is not allowed.";

	public const string Sch_InvalidEndValidation = "The 'EndValidation' method cannot not be called when all the elements have not been validated. 'ValidateEndElement' calls corresponding to 'ValidateElement' calls might be missing.";

	public const string Sch_InvalidEndElementCall = "It is invalid to call the 'ValidateEndElement' overload that takes in a 'typedValue' after 'ValidateText' or 'ValidateWhitespace' methods have been called.";

	public const string Sch_InvalidEndElementCallTyped = "It is invalid to call the 'ValidateEndElement' overload that takes in a 'typedValue' for elements with complex content.";

	public const string Sch_InvalidEndElementMultiple = "The call to the '{0}' method does not match a corresponding call to 'ValidateElement' method.";

	public const string Sch_DuplicateAttribute = "The '{0}' attribute has already been validated and is a duplicate attribute.";

	public const string Sch_InvalidPartialValidationType = "The partial validation type has to be 'XmlSchemaElement', 'XmlSchemaAttribute', or 'XmlSchemaType'.";

	public const string Sch_SchemaElementNameMismatch = "The element name '{0}' does not match the name '{1}' of the 'XmlSchemaElement' set as a partial validation type.";

	public const string Sch_SchemaAttributeNameMismatch = "The attribute name '{0}' does not match the name '{1}' of the 'XmlSchemaAttribute' set as a partial validation type.";

	public const string Sch_ValidateAttributeInvalidCall = "If the partial validation type is 'XmlSchemaElement' or 'XmlSchemaType', the 'ValidateAttribute' method cannot be called.";

	public const string Sch_ValidateElementInvalidCall = "If the partial validation type is 'XmlSchemaAttribute', the 'ValidateElement' method cannot be called.";

	public const string Sch_EnumNotStarted = "Enumeration has not started. Call MoveNext.";

	public const string Sch_EnumFinished = "Enumeration has already finished.";

	public const string SchInf_schema = "The supplied xml instance is a schema or contains an inline schema. This class cannot infer a schema for a schema.";

	public const string SchInf_entity = "Inference cannot handle entity references. Pass in an 'XmlReader' that expands entities.";

	public const string SchInf_simplecontent = "Expected simple content. Schema was not created using this tool.";

	public const string SchInf_extension = "Expected 'Extension' within 'SimpleContent'. Schema was not created using this tool.";

	public const string SchInf_particle = "Particle cannot exist along with 'ContentModel'.";

	public const string SchInf_ct = "Complex type expected to exist with at least one 'Element' at this point.";

	public const string SchInf_seq = "sequence expected to contain elements only. Schema was not created using this tool.";

	public const string SchInf_noseq = "The supplied schema contains particles other than Sequence and Choice. Only schemas generated by this tool are supported.";

	public const string SchInf_noct = "Expected ComplexType. Schema was not generated using this tool.";

	public const string SchInf_UnknownParticle = "Expected Element. Schema was not generated using this tool.";

	public const string SchInf_schematype = "Inference can only handle simple built-in types for 'SchemaType'.";

	public const string SchInf_NoElement = "There is no element to infer schema.";

	public const string Xp_UnclosedString = "This is an unclosed string.";

	public const string Xp_ExprExpected = "'{0}' is an invalid expression.";

	public const string Xp_InvalidArgumentType = "The argument to function '{0}' in '{1}' cannot be converted to a node-set.";

	public const string Xp_InvalidNumArgs = "Function '{0}' in '{1}' has an invalid number of arguments.";

	public const string Xp_InvalidName = "'{0}' has an invalid qualified name.";

	public const string Xp_InvalidToken = "'{0}' has an invalid token.";

	public const string Xp_NodeSetExpected = "Expression must evaluate to a node-set.";

	public const string Xp_NotSupported = "The XPath query '{0}' is not supported.";

	public const string Xp_InvalidPattern = "'{0}' is an invalid XSLT pattern.";

	public const string Xp_InvalidKeyPattern = "'{0}' is an invalid key pattern. It either contains a variable reference or 'key()' function.";

	public const string Xp_BadQueryObject = "This is an invalid object. Only objects returned from Compile() can be passed as input.";

	public const string Xp_UndefinedXsltContext = "XsltContext is needed for this query because of an unknown function.";

	public const string Xp_NoContext = "Namespace Manager or XsltContext needed. This query has a prefix, variable, or user-defined function.";

	public const string Xp_UndefVar = "The variable '{0}' is undefined.";

	public const string Xp_UndefFunc = "The function '{0}()' is undefined.";

	public const string Xp_FunctionFailed = "Function '{0}()' has failed.";

	public const string Xp_CurrentNotAllowed = "The 'current()' function cannot be used in a pattern.";

	public const string Xp_QueryTooComplex = "The xpath query is too complex.";

	public const string Xdom_DualDocumentTypeNode = "This document already has a 'DocumentType' node.";

	public const string Xdom_DualDocumentElementNode = "This document already has a 'DocumentElement' node.";

	public const string Xdom_DualDeclarationNode = "This document already has an 'XmlDeclaration' node.";

	public const string Xdom_Import = "Cannot import nodes of type '{0}'.";

	public const string Xdom_Import_NullNode = "Cannot import a null node.";

	public const string Xdom_NoRootEle = "The document does not have a root element.";

	public const string Xdom_Attr_Name = "The attribute local name cannot be empty.";

	public const string Xdom_AttrCol_Object = "An 'Attributes' collection can only contain 'Attribute' objects.";

	public const string Xdom_AttrCol_Insert = "The reference node must be a child of the current node.";

	public const string Xdom_NamedNode_Context = "The named node is from a different document context.";

	public const string Xdom_Version = "Wrong XML version information. The XML must match production \"VersionNum ::= '1.' [0-9]+\".";

	public const string Xdom_standalone = "Wrong value for the XML declaration standalone attribute of '{0}'.";

	public const string Xdom_Ele_Prefix = "The prefix of an element name cannot start with 'xml'.";

	public const string Xdom_Ent_Innertext = "The 'InnerText' of an 'Entity' node is read-only and cannot be set.";

	public const string Xdom_EntRef_SetVal = "'EntityReference' nodes have no support for setting value.";

	public const string Xdom_WS_Char = "The string for white space contains an invalid character.";

	public const string Xdom_Node_SetVal = "Cannot set a value on node type '{0}'.";

	public const string Xdom_Empty_LocalName = "The local name for elements or attributes cannot be null or an empty string.";

	public const string Xdom_Set_InnerXml = "Cannot set the 'InnerXml' for the current node because it is either read-only or cannot have children.";

	public const string Xdom_Attr_InUse = "The 'Attribute' node cannot be inserted because it is already an attribute of another element.";

	public const string Xdom_Enum_ElementList = "The element list has changed. The enumeration operation failed to continue.";

	public const string Xdom_Invalid_NT_String = "'{0}' does not represent any 'XmlNodeType'.";

	public const string Xdom_InvalidCharacter_EntityReference = "Cannot create an 'EntityReference' node with a name starting with '#'.";

	public const string Xdom_IndexOutOfRange = "The index being passed in is out of range.";

	public const string Xdom_Document_Innertext = "The 'InnerText' of a 'Document' node is read-only and cannot be set.";

	public const string Xpn_BadPosition = "Operation is not valid due to the current position of the navigator.";

	public const string Xpn_MissingParent = "The current position of the navigator is missing a valid parent.";

	public const string Xpn_NoContent = "No content generated as the result of the operation.";

	public const string Xdom_Load_NoDocument = "The document to be loaded could not be found.";

	public const string Xdom_Load_NoReader = "There is no reader from which to load the document.";

	public const string Xdom_Node_Null_Doc = "Cannot create a node without an owner document.";

	public const string Xdom_Node_Insert_Child = "Cannot insert a node or any ancestor of that node as a child of itself.";

	public const string Xdom_Node_Insert_Contain = "The current node cannot contain other nodes.";

	public const string Xdom_Node_Insert_Path = "The reference node is not a child of this node.";

	public const string Xdom_Node_Insert_Context = "The node to be inserted is from a different document context.";

	public const string Xdom_Node_Insert_Location = "Cannot insert the node in the specified location.";

	public const string Xdom_Node_Insert_TypeConflict = "The specified node cannot be inserted as the valid child of this node, because the specified node is the wrong type.";

	public const string Xdom_Node_Remove_Contain = "The current node cannot contain other nodes, so the node to be removed is not its child.";

	public const string Xdom_Node_Remove_Child = "The node to be removed is not a child of this node.";

	public const string Xdom_Node_Modify_ReadOnly = "This node is read-only. It cannot be modified.";

	public const string Xdom_TextNode_SplitText = "The 'Text' node is not connected in the DOM live tree. No 'SplitText' operation could be performed.";

	public const string Xdom_Attr_Reserved_XmlNS = "The namespace declaration attribute has an incorrect 'namespaceURI': '{0}'.";

	public const string Xdom_Node_Cloning = "'Entity' and 'Notation' nodes cannot be cloned.";

	public const string Xnr_ResolveEntity = "The node is not an expandable 'EntityReference' node.";

	public const string XPathDocument_MissingSchemas = "An XmlSchemaSet must be provided to validate the document.";

	public const string XPathDocument_NotEnoughSchemaInfo = "Element should have prior schema information to call this method.";

	public const string XPathDocument_ValidateInvalidNodeType = "Validate and CheckValidity are only allowed on Root or Element nodes.";

	public const string XPathDocument_SchemaSetNotAllowed = "An XmlSchemaSet is only allowed as a parameter on the Root node.";

	public const string XmlBin_MissingEndCDATA = "CDATA end token is missing.";

	public const string XmlBin_InvalidQNameID = "Invalid QName ID.";

	public const string XmlBinary_UnexpectedToken = "Unexpected BinaryXml token.";

	public const string XmlBinary_InvalidSqlDecimal = "Unable to parse data as SQL_DECIMAL.";

	public const string XmlBinary_InvalidSignature = "Invalid BinaryXml signature.";

	public const string XmlBinary_InvalidProtocolVersion = "Invalid BinaryXml protocol version.";

	public const string XmlBinary_UnsupportedCodePage = "Unsupported BinaryXml codepage.";

	public const string XmlBinary_InvalidStandalone = "Invalid BinaryXml standalone token.";

	public const string XmlBinary_NoParserContext = "BinaryXml Parser does not support initialization with XmlParserContext.";

	public const string XmlBinary_ListsOfValuesNotSupported = "Lists of BinaryXml value tokens not supported.";

	public const string XmlBinary_CastNotSupported = "Token '{0}' does not support a conversion to Clr type '{1}'.";

	public const string XmlBinary_NoRemapPrefix = "Prefix '{0}' is already assigned to namespace '{1}' and cannot be reassigned to '{2}' on this tag.";

	public const string XmlBinary_AttrWithNsNoPrefix = "Attribute '{0}' has namespace '{1}' but no prefix.";

	public const string XmlBinary_ValueTooBig = "The value is too big to fit into an Int32. The arithmetic operation resulted in an overflow.";

	public const string SqlTypes_ArithOverflow = "Arithmetic Overflow.";

	public const string SqlTypes_ArithTruncation = "Numeric arithmetic causes truncation.";

	public const string SqlTypes_DivideByZero = "Divide by zero error encountered.";

	public const string XmlMissingType = "Invalid serialization assembly: Required type {0} cannot be found in the generated assembly '{1}'.";

	public const string XmlUnsupportedType = "{0} is an unsupported type.";

	public const string XmlSerializerUnsupportedType = "{0} is an unsupported type. Please use [XmlIgnore] attribute to exclude members of this type from serialization graph.";

	public const string XmlSerializerUnsupportedMember = "Cannot serialize member '{0}' of type '{1}', see inner exception for more details.";

	public const string XmlUnsupportedTypeKind = "The type {0} may not be serialized.";

	public const string XmlUnsupportedSoapTypeKind = "The type {0} may not be serialized with SOAP-encoded messages. Set the Use for your message to Literal.";

	public const string XmlUnsupportedIDictionary = "The type {0} is not supported because it implements IDictionary.";

	public const string XmlUnsupportedIDictionaryDetails = "Cannot serialize member {0} of type {1}, because it implements IDictionary.";

	public const string XmlDuplicateTypeName = "A type with the name {0} has already been added in namespace {1}.";

	public const string XmlSerializableNameMissing1 = "Schema Id is missing. The schema returned from {0}.GetSchema() must have an Id.";

	public const string XmlConstructorInaccessible = "{0} cannot be serialized because it does not have a parameterless constructor.";

	public const string XmlTypeInaccessible = "{0} is inaccessible due to its protection level. Only public types can be processed.";

	public const string XmlTypeStatic = "{0} cannot be serialized. Static types cannot be used as parameters or return types.";

	public const string XmlNoDefaultAccessors = "You must implement a default accessor on {0} because it inherits from ICollection.";

	public const string XmlNoAddMethod = "To be XML serializable, types which inherit from {2} must have an implementation of Add({1}) at all levels of their inheritance hierarchy. {0} does not implement Add({1}).";

	public const string XmlReadOnlyPropertyError = "Cannot deserialize type '{0}' because it contains property '{1}' which has no public setter.";

	public const string XmlAttributeSetAgain = "'{0}.{1}' already has attributes.";

	public const string XmlIllegalWildcard = "Cannot use wildcards at the top level of a schema.";

	public const string XmlIllegalArrayElement = "An element declared at the top level of a schema cannot have maxOccurs > 1. Provide a wrapper element for '{0}' by using XmlArray or XmlArrayItem instead of XmlElementAttribute, or by using the Wrapped parameter style.";

	public const string XmlIllegalForm = "There was an error exporting '{0}': elements declared at the top level of a schema cannot be unqualified.";

	public const string XmlBareTextMember = "There was an error exporting '{0}': bare members cannot contain text content.";

	public const string XmlBareAttributeMember = "There was an error exporting '{0}': bare members cannot be attributes.";

	public const string XmlReflectionError = "There was an error reflecting '{0}'.";

	public const string XmlTypeReflectionError = "There was an error reflecting type '{0}'.";

	public const string XmlPropertyReflectionError = "There was an error reflecting property '{0}'.";

	public const string XmlFieldReflectionError = "There was an error reflecting field '{0}'.";

	public const string XmlInvalidDataTypeUsage = "'{0}' is an invalid value for the {1} property. The property may only be specified for primitive types.";

	public const string XmlInvalidXsdDataType = "Value '{0}' cannot be used for the {1} property. The datatype '{2}' is missing.";

	public const string XmlDataTypeMismatch = "'{0}' is an invalid value for the {1} property. {0} cannot be converted to {2}.";

	public const string XmlIllegalTypeContext = "{0} cannot be used as: 'xml {1}'.";

	public const string XmlUdeclaredXsdType = "The type, {0}, is undeclared.";

	public const string XmlAnyElementNamespace = "The element {0} has been attributed with an XmlAnyElementAttribute and a namespace {1}, but no name. When a namespace is supplied, a name is also required. Supply a name or remove the namespace.";

	public const string XmlInvalidConstantAttribute = "Only XmlEnum may be used on enumerated constants.";

	public const string XmlIllegalDefault = "The default value for XmlAttribute or XmlElement may only be specified for primitive types.";

	public const string XmlIllegalAttributesArrayAttribute = "XmlAttribute and XmlAnyAttribute cannot be used in conjunction with XmlElement, XmlText, XmlAnyElement, XmlArray, or XmlArrayItem.";

	public const string XmlIllegalElementsArrayAttribute = "XmlElement, XmlText, and XmlAnyElement cannot be used in conjunction with XmlAttribute, XmlAnyAttribute, XmlArray, or XmlArrayItem.";

	public const string XmlIllegalArrayArrayAttribute = "XmlArray and XmlArrayItem cannot be used in conjunction with XmlAttribute, XmlAnyAttribute, XmlElement, XmlText, or XmlAnyElement.";

	public const string XmlIllegalAttribute = "For non-array types, you may use the following attributes: XmlAttribute, XmlText, XmlElement, or XmlAnyElement.";

	public const string XmlIllegalType = "The type for {0} may not be specified for primitive types.";

	public const string XmlIllegalAttrOrText = "Cannot serialize member '{0}' of type {1}. XmlAttribute/XmlText cannot be used to encode complex types.";

	public const string XmlIllegalSoapAttribute = "Cannot serialize member '{0}' of type {1}. SoapAttribute cannot be used to encode complex types.";

	public const string XmlIllegalAttrOrTextInterface = "Cannot serialize member '{0}' of type {1}. XmlAttribute/XmlText cannot be used to encode types implementing {2}.";

	public const string XmlIllegalAttributeFlagsArray = "XmlAttribute cannot be used to encode array of {1}, because it is marked with FlagsAttribute.";

	public const string XmlIllegalAnyElement = "Cannot serialize member of type {0}: XmlAnyElement can only be used with classes of type XmlNode or a type deriving from XmlNode.";

	public const string XmlInvalidIsNullable = "IsNullable may not be 'true' for value type {0}.  Please consider using Nullable<{0}> instead.";

	public const string XmlInvalidNotNullable = "IsNullable may not be set to 'false' for a Nullable<{0}> type. Consider using '{0}' type or removing the IsNullable property from the {1} attribute.";

	public const string XmlInvalidFormUnqualified = "The Form property may not be 'Unqualified' when an explicit Namespace property is present.";

	public const string XmlDuplicateNamespace = "The namespace, {0}, is a duplicate.";

	public const string XmlElementHasNoName = "This element has no name. Please review schema type '{0}' from namespace '{1}'.";

	public const string XmlAttributeHasNoName = "This attribute has no name.";

	public const string XmlElementImportedTwice = "The element, {0}, from namespace, {1}, was imported in two different contexts: ({2}, {3}).";

	public const string XmlHiddenMember = "Member {0}.{1} of type {2} hides base class member {3}.{4} of type {5}. Use XmlElementAttribute or XmlAttributeAttribute to specify a new name.";

	public const string XmlInvalidXmlOverride = "Member '{0}.{1}' hides inherited member '{2}.{3}', but has different custom attributes.";

	public const string XmlMembersDeriveError = "These members may not be derived.";

	public const string XmlTypeUsedTwice = "The type '{0}' from namespace '{1}' was used in two different ways.";

	public const string XmlMissingGroup = "Group {0} is missing.";

	public const string XmlMissingAttributeGroup = "The attribute group {0} is missing.";

	public const string XmlMissingDataType = "The datatype '{0}' is missing.";

	public const string XmlInvalidEncoding = "Referenced type '{0}' is only valid for encoded SOAP.";

	public const string XmlMissingElement = "The element '{0}' is missing.";

	public const string XmlMissingAttribute = "The attribute {0} is missing.";

	public const string XmlMissingMethodEnum = "The method for enum {0} is missing.";

	public const string XmlNoAttributeHere = "Cannot write a node of type XmlAttribute as an element value. Use XmlAnyAttributeAttribute with an array of XmlNode or XmlAttribute to write the node as an attribute.";

	public const string XmlNeedAttributeHere = "The node must be either type XmlAttribute or a derived type.";

	public const string XmlElementNameMismatch = "This element was named '{0}' from namespace '{1}' but should have been named '{2}' from namespace '{3}'.";

	public const string XmlUnsupportedDefaultType = "The default value type, {0}, is unsupported.";

	public const string XmlUnsupportedDefaultValue = "The formatter {0} cannot be used for default values.";

	public const string XmlInvalidDefaultValue = "Value '{0}' cannot be converted to {1}.";

	public const string XmlInvalidDefaultEnumValue = "Enum {0} cannot be converted to {1}.";

	public const string XmlUnknownNode = "{0} was not expected.";

	public const string XmlUnknownConstant = "Instance validation error: '{0}' is not a valid value for {1}.";

	public const string XmlSerializeError = "There is an error in the XML document.";

	public const string XmlSerializeErrorDetails = "There is an error in XML document ({0}, {1}).";

	public const string XmlCompilerError = "Unable to generate a temporary class (result={0}).";

	public const string XmlSchemaDuplicateNamespace = "There are more then one schema with targetNamespace='{0}'.";

	public const string XmlSchemaCompiled = "Cannot add schema to compiled schemas collection.";

	public const string XmlInvalidSchemaExtension = "'{0}' is not a valid SchemaExtensionType.";

	public const string XmlInvalidArrayDimentions = "SOAP-ENC:arrayType with multidimensional array found at {0}. Only single-dimensional arrays are supported. Consider using an array of arrays instead.";

	public const string XmlInvalidArrayTypeName = "The SOAP-ENC:arrayType references type is named '{0}'; a type named '{1}' was expected at {2}.";

	public const string XmlInvalidArrayTypeNamespace = "The SOAP-ENC:arrayType references type is from namespace '{0}'; the namespace '{1}' was expected at {2}.";

	public const string XmlMissingArrayType = "SOAP-ENC:arrayType was missing at {0}.";

	public const string XmlEmptyArrayType = "SOAP-ENC:arrayType was empty at {0}.";

	public const string XmlInvalidArraySyntax = "SOAP-ENC:arrayType must end with a ']' character.";

	public const string XmlInvalidArrayTypeSyntax = "Invalid wsd:arrayType syntax: '{0}'.";

	public const string XmlMismatchedArrayBrackets = "SOAP-ENC:arrayType has mismatched brackets.";

	public const string XmlInvalidArrayLength = "SOAP-ENC:arrayType could not handle '{1}' as the length of the array.";

	public const string XmlMissingHref = "The referenced element with ID '{0}' is located outside the current document and cannot be retrieved.";

	public const string XmlInvalidHref = "The referenced element with ID '{0}' was not found in the document.";

	public const string XmlUnknownType = "The specified type was not recognized: name='{0}', namespace='{1}', at {2}.";

	public const string XmlAbstractType = "The specified type is abstract: name='{0}', namespace='{1}', at {2}.";

	public const string XmlMappingsScopeMismatch = "Exported mappings must come from the same importer.";

	public const string XmlMethodTypeNameConflict = "The XML element '{0}' from namespace '{1}' references a method and a type. Change the method's message name using WebMethodAttribute or change the type's root element using the XmlRootAttribute.";

	public const string XmlCannotReconcileAccessor = "The top XML element '{0}' from namespace '{1}' references distinct types {2} and {3}. Use XML attributes to specify another XML name or namespace for the element or types.";

	public const string XmlCannotReconcileAttributeAccessor = "The global XML attribute '{0}' from namespace '{1}' references distinct types {2} and {3}. Use XML attributes to specify another XML name or namespace for the attribute or types.";

	public const string XmlCannotReconcileAccessorDefault = "The global XML item '{0}' from namespace '{1}' has mismatch default value attributes: '{2}' and '{3}' and cannot be mapped to the same schema item. Use XML attributes to specify another XML name or namespace for one of the items, or make sure that the default values match.";

	public const string XmlInvalidTypeAttributes = "XmlRoot and XmlType attributes may not be specified for the type {0}.";

	public const string XmlInvalidAttributeUse = "XML attributes may not be specified for the type {0}.";

	public const string XmlTypesDuplicate = "Types '{0}' and '{1}' both use the XML type name, '{2}', from namespace '{3}'. Use XML attributes to specify a unique XML name and/or namespace for the type.";

	public const string XmlInvalidSoapArray = "An array of type {0} may not be used with XmlArrayType.Soap.";

	public const string XmlCannotIncludeInSchema = "The type {0} may not be exported to a schema because the IncludeInSchema property of the XmlType attribute is 'false'.";

	public const string XmlSoapCannotIncludeInSchema = "The type {0} may not be exported to a schema because the IncludeInSchema property of the SoapType attribute is 'false'.";

	public const string XmlInvalidSerializable = "The type {0} may not be used in this context. To use {0} as a parameter, return type, or member of a class or struct, the parameter, return type, or member must be declared as type {0} (it cannot be object). Objects of type {0} may not be used in un-typed collections, such as ArrayLists.";

	public const string XmlInvalidUseOfType = "The type {0} may not be used in this context.";

	public const string XmlUnxpectedType = "The type {0} was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically.";

	public const string XmlUnknownAnyElement = "The XML element '{0}' from namespace '{1}' was not expected. The XML element name and namespace must match those provided via XmlAnyElementAttribute(s).";

	public const string XmlMultipleAttributeOverrides = "{0}. {1} already has attributes.";

	public const string XmlInvalidEnumAttribute = "Only SoapEnum may be used on enum constants.";

	public const string XmlInvalidReturnPosition = "The return value must be the first member.";

	public const string XmlInvalidElementAttribute = "Only SoapElementAttribute or SoapAttributeAttribute may be used on members.";

	public const string XmlInvalidVoid = "The type Void is not valid in this context.";

	public const string XmlInvalidContent = "Invalid content {0}.";

	public const string XmlInvalidSchemaElementType = "Types must be declared at the top level in the schema. Please review schema type '{0}' from namespace '{1}': element '{2}' is using anonymous type declaration, anonymous types are not supported with encoded SOAP.";

	public const string XmlInvalidSubstitutionGroupUse = "Substitution group may not be used with encoded SOAP. Please review type declaration '{0}' from namespace '{1}'.";

	public const string XmlElementMissingType = "Please review type declaration '{0}' from namespace '{1}': element '{2}' does not specify a type.";

	public const string XmlInvalidAnyAttributeUse = "Any may not be specified. Attributes are not supported with encoded SOAP. Please review schema type '{0}' from namespace '{1}'.";

	public const string XmlSoapInvalidAttributeUse = "Attributes are not supported with encoded SOAP. Please change definition of schema type '{0}' from namespace '{1}': use elements (not attributes) for fields/parameters.";

	public const string XmlSoapInvalidChoice = "Choice is not supported with encoded SOAP. Please change definition of schema type '{0}' from namespace '{1}': use all or sequence (not choice) for fields/parameters.";

	public const string XmlSoapUnsupportedGroupRef = "The ref syntax for groups is not supported with encoded SOAP. Please change definition of schema type '{0}' from namespace '{1}': replace the group reference with local group declaration.";

	public const string XmlSoapUnsupportedGroupRepeat = "Group may not repeat.  Unbounded groups are not supported with encoded SOAP. Please change definition of schema type '{0}' from namespace '{1}'.";

	public const string XmlSoapUnsupportedGroupNested = "Nested groups may not be used with encoded SOAP. Please change definition of schema type '{0}' from namespace '{1}'.";

	public const string XmlSoapUnsupportedGroupAny = "Any may not be used with encoded SOAP. Please change definition of schema type '{0}' from namespace '{1}'.";

	public const string XmlInvalidEnumContent = "Invalid content '{0}' for enumerated data type {1}.";

	public const string XmlInvalidAttributeType = "{0} may not be used on parameters or return values when they are not wrapped.";

	public const string XmlInvalidBaseType = "Type {0} cannot derive from {1} because it already has base type {2}.";

	public const string XmlPrimitiveBaseType = "Type '{0}' from namespace '{1}' is not a complex type and cannot be used as a {2}.";

	public const string XmlInvalidIdentifier = "Identifier '{0}' is not CLS-compliant.";

	public const string XmlGenError = "There was an error generating the XML document.";

	public const string XmlInvalidXmlns = "Invalid namespace attribute: xmlns:{0}=\"\".";

	public const string XmlCircularReference = "A circular reference was detected while serializing an object of type {0}.";

	public const string XmlCircularReference2 = "A circular type reference was detected in anonymous type '{0}'.  Please change '{0}' to be a named type by setting {1}={2} in the type definition.";

	public const string XmlAnonymousBaseType = "Illegal type derivation: Type '{0}' derives from anonymous type '{1}'. Please change '{1}' to be a named type by setting {2}={3} in the type definition.";

	public const string XmlMissingSchema = "Missing schema targetNamespace=\"{0}\".";

	public const string XmlNoSerializableMembers = "Cannot serialize object of type '{0}'. The object does not have serializable members.";

	public const string XmlIllegalOverride = "Error: Type '{0}' could not be imported because it redefines inherited member '{1}' with a different type. '{1}' is declared as type '{3}' on '{0}', but as type '{2}' on base class '{4}'.";

	public const string XmlReadOnlyCollection = "Could not deserialize {0}. Parameterless constructor is required for collections and enumerators.";

	public const string XmlRpcNestedValueType = "Cannot serialize {0}. Nested structs are not supported with encoded SOAP.";

	public const string XmlRpcRefsInValueType = "Cannot serialize {0}. References in structs are not supported with encoded SOAP.";

	public const string XmlRpcArrayOfValueTypes = "Cannot serialize {0}. Arrays of structs are not supported with encoded SOAP.";

	public const string XmlDuplicateElementName = "The XML element '{0}' from namespace '{1}' is already present in the current scope. Use XML attributes to specify another XML name or namespace for the element.";

	public const string XmlDuplicateAttributeName = "The XML attribute '{0}' from namespace '{1}' is already present in the current scope. Use XML attributes to specify another XML name or namespace for the attribute.";

	public const string XmlBadBaseElement = "Element '{0}' from namespace '{1}' is not a complex type and cannot be used as a {2}.";

	public const string XmlBadBaseType = "Type '{0}' from namespace '{1}' is not a complex type and cannot be used as a {2}.";

	public const string XmlUndefinedAlias = "Namespace prefix '{0}' is not defined.";

	public const string XmlChoiceIdentifierType = "Type of choice identifier '{0}' is inconsistent with type of '{1}'. Please use {2}.";

	public const string XmlChoiceIdentifierArrayType = "Type of choice identifier '{0}' is inconsistent with type of '{1}'. Please use array of {2}.";

	public const string XmlChoiceIdentifierTypeEnum = "Choice identifier '{0}' must be an enum.";

	public const string XmlChoiceIdentiferMemberMissing = "Missing '{0}' member needed for serialization of choice '{1}'.";

	public const string XmlChoiceIdentiferAmbiguous = "Ambiguous choice identifier. There are several members named '{0}'.";

	public const string XmlChoiceIdentiferMissing = "You need to add {0} to the '{1}' member.";

	public const string XmlChoiceMissingValue = "Type {0} is missing enumeration value '{1}' for element '{2}' from namespace '{3}'.";

	public const string XmlChoiceMissingAnyValue = "Type {0} is missing enumeration value '##any:' corresponding to XmlAnyElementAttribute.";

	public const string XmlChoiceMismatchChoiceException = "Value of {0} mismatches the type of {1}; you need to set it to {2}.";

	public const string XmlArrayItemAmbiguousTypes = "Ambiguous types specified for member '{0}'.  Items '{1}' and '{2}' have the same type.  Please consider using {3} with {4} instead.";

	public const string XmlUnsupportedInterface = "Cannot serialize interface {0}.";

	public const string XmlUnsupportedInterfaceDetails = "Cannot serialize member {0} of type {1} because it is an interface.";

	public const string XmlUnsupportedRank = "Cannot serialize object of type {0}. Multidimensional arrays are not supported.";

	public const string XmlUnsupportedInheritance = "Using {0} as a base type for a class is not supported by XmlSerializer.";

	public const string XmlIllegalMultipleText = "Cannot serialize object of type '{0}' because it has multiple XmlText attributes. Consider using an array of strings with XmlTextAttribute for serialization of a mixed complex type.";

	public const string XmlIllegalMultipleTextMembers = "XmlText may not be used on multiple parameters or return values.";

	public const string XmlIllegalArrayTextAttribute = "Member '{0}' cannot be encoded using the XmlText attribute. You may use the XmlText attribute to encode primitives, enumerations, arrays of strings, or arrays of XmlNode.";

	public const string XmlIllegalTypedTextAttribute = "Cannot serialize object of type '{0}'. Consider changing type of XmlText member '{0}.{1}' from {2} to string or string array.";

	public const string XmlIllegalSimpleContentExtension = "Cannot serialize object of type '{0}'. Base type '{1}' has simpleContent and can only be extended by adding XmlAttribute elements. Please consider changing XmlText member of the base class to string array.";

	public const string XmlInvalidCast = "Cannot assign object of type {0} to an object of type {1}.";

	public const string XmlInvalidCastWithId = "Cannot assign object of type {0} to an object of type {1}. The error occurred while reading node with id='{2}'.";

	public const string XmlInvalidArrayRef = "Invalid reference id='{0}'. Object of type {1} cannot be stored in an array of this type. Details: array index={2}.";

	public const string XmlInvalidNullCast = "Cannot assign null value to an object of type {1}.";

	public const string XmlMultipleXmlns = "Cannot serialize object of type '{0}' because it has multiple XmlNamespaceDeclarations attributes.";

	public const string XmlMultipleXmlnsMembers = "XmlNamespaceDeclarations may not be used on multiple parameters or return values.";

	public const string XmlXmlnsInvalidType = "Cannot use XmlNamespaceDeclarations attribute on member '{0}' of type {1}.  This attribute is only valid on members of type {2}.";

	public const string XmlSoleXmlnsAttribute = "XmlNamespaceDeclarations attribute cannot be used in conjunction with any other custom attributes.";

	public const string XmlConstructorHasSecurityAttributes = "The type '{0}' cannot be serialized because its parameterless constructor is decorated with declarative security permission attributes. Consider using imperative asserts or demands in the constructor.";

	public const string XmlPropertyHasSecurityAttributes = "The property '{0}' on type '{1}' cannot be serialized because it is decorated with declarative security permission attributes. Consider using imperative asserts or demands in the property accessors.";

	public const string XmlMethodHasSecurityAttributes = "The type '{0}' cannot be serialized because the {1}({2}) method is decorated with declarative security permission attributes. Consider using imperative asserts or demands in the method.";

	public const string XmlDefaultAccessorHasSecurityAttributes = "The type '{0}' cannot be serialized because its default accessor is decorated with declarative security permission attributes. Consider using imperative asserts or demands in the accessor.";

	public const string XmlInvalidChoiceIdentifierValue = "Invalid or missing value of the choice identifier '{1}' of type '{0}[]'.";

	public const string XmlAnyElementDuplicate = "The element '{0}' has been attributed with duplicate XmlAnyElementAttribute(Name=\"{1}\", Namespace=\"{2}\").";

	public const string XmlChoiceIdDuplicate = "Enum values in the XmlChoiceIdentifier '{0}' have to be unique.  Value '{1}' already present.";

	public const string XmlChoiceIdentifierMismatch = "Value '{0}' of the choice identifier '{1}' does not match element '{2}' from namespace '{3}'.";

	public const string XmlUnsupportedRedefine = "Cannot import schema for type '{0}' from namespace '{1}'. Redefine not supported.";

	public const string XmlDuplicateElementInScope = "The XML element named '{0}' from namespace '{1}' is already present in the current scope.";

	public const string XmlDuplicateElementInScope1 = "The XML element named '{0}' from namespace '{1}' is already present in the current scope. Elements with the same name in the same scope must have the same type.";

	public const string XmlNoPartialTrust = "One or more assemblies referenced by the XmlSerializer cannot be called from partially trusted code.";

	public const string XmlInvalidEncodingNotEncoded1 = "The encoding style '{0}' is not valid for this call because this XmlSerializer instance does not support encoding. Use the SoapReflectionImporter to initialize an XmlSerializer that supports encoding.";

	public const string XmlInvalidEncoding3 = "The encoding style '{0}' is not valid for this call. Valid values are '{1}' for SOAP 1.1 encoding or '{2}' for SOAP 1.2 encoding.";

	public const string XmlInvalidSpecifiedType = "Member '{0}' of type {1} cannot be serialized.  Members with names ending on 'Specified' suffix have special meaning to the XmlSerializer: they control serialization of optional ValueType members and have to be of type {2}.";

	public const string XmlUnsupportedOpenGenericType = "Type {0} is not supported because it has unbound generic parameters.  Only instantiated generic types can be serialized.";

	public const string XmlMismatchSchemaObjects = "Warning: Cannot share {0} named '{1}' from '{2}' namespace. Several mismatched schema declarations were found.";

	public const string XmlCircularTypeReference = "Type '{0}' from targetNamespace='{1}' has invalid definition: Circular type reference.";

	public const string XmlCircularGroupReference = "Group '{0}' from targetNamespace='{1}' has invalid definition: Circular group reference.";

	public const string XmlRpcLitElementNamespace = "{0}='{1}' is not supported with rpc\\literal SOAP. The wrapper element has to be unqualified.";

	public const string XmlRpcLitElementNullable = "{0}='{1}' is not supported with rpc\\literal SOAP. The wrapper element cannot be nullable.";

	public const string XmlRpcLitElements = "Multiple accessors are not supported with rpc\\literal SOAP, you may use the following attributes: XmlArray, XmlArrayItem, or single XmlElement.";

	public const string XmlRpcLitArrayElement = "Input or output values of an rpc\\literal method cannot have maxOccurs > 1. Provide a wrapper element for '{0}' by using XmlArray or XmlArrayItem instead of XmlElement attribute.";

	public const string XmlRpcLitAttributeAttributes = "XmlAttribute and XmlAnyAttribute cannot be used with rpc\\literal SOAP, you may use the following attributes: XmlArray, XmlArrayItem, or single XmlElement.";

	public const string XmlRpcLitAttributes = "XmlText, XmlAnyElement, or XmlChoiceIdentifier cannot be used with rpc\\literal SOAP, you may use the following attributes: XmlArray, XmlArrayItem, or single XmlElement.";

	public const string XmlSequenceMembers = "Explicit sequencing may not be used on parameters or return values.  Please remove {0} property from custom attributes.";

	public const string XmlRpcLitXmlns = "Input or output values of an rpc\\literal method cannot have an XmlNamespaceDeclarations attribute (member '{0}').";

	public const string XmlDuplicateNs = "Illegal namespace declaration xmlns:{0}='{1}'. Namespace alias '{0}' already defined in the current scope.";

	public const string XmlAnonymousInclude = "Cannot include anonymous type '{0}'.";

	public const string RefSyntaxNotSupportedForElements0 = "Element reference syntax not supported with encoded SOAP. Replace element reference '{0}' from namespace '{1}' with a local element declaration.";

	public const string XmlSchemaIncludeLocation = "Schema attribute schemaLocation='{1}' is not supported on objects of type {0}.  Please set {0}.Schema property.";

	public const string XmlSerializableSchemaError = "Schema type information provided by {0} is invalid: {1}";

	public const string XmlGetSchemaMethodName = "'{0}' is an invalid language identifier.";

	public const string XmlGetSchemaMethodMissing = "You must implement public static {0}({1}) method on {2}.";

	public const string XmlGetSchemaMethodReturnType = "Method {0}.{1}() specified by {2} has invalid signature: return type must be compatible with {3}.";

	public const string XmlGetSchemaEmptyTypeName = "{0}.{1}() must return a valid type name.";

	public const string XmlGetSchemaTypeMissing = "{0}.{1}() must return a valid type name. Type '{2}' cannot be found in the targetNamespace='{3}'.";

	public const string XmlGetSchemaInclude = "Multiple schemas with targetNamespace='{0}' returned by {1}.{2}().  Please use only the main (parent) schema, and add the others to the schema Includes.";

	public const string XmlSerializableAttributes = "Only XmlRoot attribute may be specified for the type {0}. Please use {1} to specify schema type.";

	public const string XmlSerializableMergeItem = "Cannot merge schemas with targetNamespace='{0}'. Several mismatched declarations were found: {1}";

	public const string XmlSerializableBadDerivation = "Type '{0}' from namespace '{1}' declared as derivation of type '{2}' from namespace '{3}, but corresponding CLR types are not compatible.  Cannot convert type '{4}' to '{5}'.";

	public const string XmlSerializableMissingClrType = "Type '{0}' from namespace '{1}' does not have corresponding IXmlSerializable type. Please consider adding {2} to '{3}'.";

	public const string XmlCircularDerivation = "Circular reference in derivation of IXmlSerializable type '{0}'.";

	public const string XmlSerializerAccessDenied = "Access to the temp directory is denied.  The process under which XmlSerializer is running does not have sufficient permission to access the temp directory.  CodeDom will use the user account the process is using to do the compilation, so if the user doesn\ufffdt have access to system temp directory, you will not be able to compile.  Use Path.GetTempPath() API to find out the temp directory location.";

	public const string XmlIdentityAccessDenied = "Access to the temp directory is denied.  Identity '{0}' under which XmlSerializer is running does not have sufficient permission to access the temp directory.  CodeDom will use the user account the process is using to do the compilation, so if the user doesn\ufffdt have access to system temp directory, you will not be able to compile.  Use Path.GetTempPath() API to find out the temp directory location.";

	public const string XmlMelformMapping = "This mapping was not crated by reflection importer and cannot be used in this context.";

	public const string XmlSchemaSyntaxErrorDetails = "Schema with targetNamespace='{0}' has invalid syntax. {1} Line {2}, position {3}.";

	public const string XmlSchemaElementReference = "Element reference '{0}' declared in schema type '{1}' from namespace '{2}'.";

	public const string XmlSchemaAttributeReference = "Attribute reference '{0}' declared in schema type '{1}' from namespace '{2}'.";

	public const string XmlSchemaItem = "Schema item '{1}' from namespace '{0}'. {2}";

	public const string XmlSchemaNamedItem = "Schema item '{1}' named '{2}' from namespace '{0}'. {3}";

	public const string XmlSchemaContentDef = "Check content definition of schema type '{0}' from namespace '{1}'. {2}";

	public const string XmlSchema = "Schema with targetNamespace='{0}' has invalid syntax. {1}";

	public const string XmlSerializerCompileFailed = "Cannot load dynamically generated serialization assembly. In some hosting environments assembly load functionality is restricted, consider using pre-generated serializer. Please see inner exception for more information.";

	public const string XmlSerializableRootDupName = "Cannot reconcile schema for '{0}'. Please use [XmlRoot] attribute to change default name or namespace of the top-level element to avoid duplicate element declarations: element name='{1}' namespace='{2}'.";

	public const string XmlDropDefaultAttribute = "DefaultValue attribute on members of type {0} is not supported in this version of the .Net Framework.";

	public const string XmlDropAttributeValue = "'{0}' attribute on items of type '{1}' is not supported in this version of the .Net Framework.  Ignoring {0}='{2}' attribute.";

	public const string XmlDropArrayAttributeValue = "'{0}' attribute on array-like elements is not supported in this version of the .Net Framework.  Ignoring {0}='{1}' attribute on element name='{2}'.";

	public const string XmlDropNonPrimitiveAttributeValue = "'{0}' attribute supported only for primitive types.  Ignoring {0}='{1}' attribute.";

	public const string XmlNotKnownDefaultValue = "Schema importer extension {0} failed to parse '{1}'='{2}' attribute of type {3} from namespace='{4}'.";

	public const string XmlRemarks = "<remarks/>";

	public const string XmlCodegenWarningDetails = "CODEGEN Warning: {0}";

	public const string XmlExtensionComment = "This type definition was generated by {0} schema importer extension.";

	public const string XmlExtensionDuplicateDefinition = "Schema importer extension {0} generated duplicate type definitions: {1}.";

	public const string XmlImporterExtensionBadLocalTypeName = "Schema importer extension {0} returned invalid type information: '{1}' is not a valid type name.";

	public const string XmlImporterExtensionBadTypeName = "Schema importer extension {0} returned invalid type information for xsd type {1} from namespace='{2}': '{3}' is not a valid type name.";

	public const string XmlConfigurationDuplicateExtension = "Duplicate extension name.  schemaImporterExtension with name '{0}' already been added.";

	public const string XmlPregenMissingDirectory = "Could not find directory to save XmlSerializer generated assembly: {0}.";

	public const string XmlPregenMissingTempDirectory = "Could not find TEMP directory to save XmlSerializer generated assemblies.";

	public const string XmlPregenTypeDynamic = "Cannot pre-generate serialization assembly for type '{0}'. Pre-generation of serialization assemblies is not supported for dynamic types. Save the assembly and load it from disk to use it with XmlSerialization.";

	public const string XmlSerializerExpiredDetails = "Pre-generated serializer '{0}' has expired. You need to re-generate serializer for '{1}'.";

	public const string XmlSerializerExpired = "Pre-generated assembly '{0}' CodeBase='{1}' has expired.";

	public const string XmlPregenAssemblyDynamic = "Cannot pre-generate serialization assembly. Pre-generation of serialization assemblies is not supported for dynamic assemblies. Save the assembly and load it from disk to use it with XmlSerialization.";

	public const string XmlNotSerializable = "Type '{0}' is not serializable.";

	public const string XmlPregenOrphanType = "Cannot pre-generate serializer for multiple assemblies. Type '{0}' does not belong to assembly {1}.";

	public const string XmlPregenCannotLoad = "Could not load file or assembly '{0}' or one of its dependencies. The system cannot find the file specified.";

	public const string XmlPregenInvalidXmlSerializerAssemblyAttribute = "Invalid XmlSerializerAssemblyAttribute usage. Please use {0} property or {1} property.";

	public const string XmlSequenceInconsistent = "Inconsistent sequencing: if used on one of the class's members, the '{0}' property is required on all particle-like members, please explicitly set '{0}' using XmlElement, XmlAnyElement or XmlArray custom attribute on class member '{1}'.";

	public const string XmlSequenceUnique = "'{1}' values must be unique within the same scope. Value '{0}' is in use. Please change '{1}' property on '{2}'.";

	public const string XmlSequenceHierarchy = "There was an error processing type '{0}'. Type member '{1}' declared in '{2}' is missing required '{3}' property. If one class in the class hierarchy uses explicit sequencing feature ({3}), then its base class and all derived classes have to do the same.";

	public const string XmlSequenceMatch = "If multiple custom attributes specified on a single member only one of them have to have explicit '{0}' property, however if more that one attribute has the explicit '{0}', all values have to match.";

	public const string XmlDisallowNegativeValues = "Negative values are prohibited.";

	public const string Xml_BadComment = "This is an invalid comment syntax.  Expected '-->'.";

	public const string Xml_NumEntityOverflow = "The numeric entity value is too large.";

	public const string Xml_UnexpectedCharacter = "'{0}', hexadecimal value {1}, is an unexpected character.";

	public const string Xml_UnexpectedToken1 = "This is an unexpected token. The expected token is '|' or ')'.";

	public const string Xml_TagMismatchFileName = "The '{0}' start tag on line '{1}' doesn't match the end tag of '{2}' in file '{3}'.";

	public const string Xml_ReservedNs = "This is a reserved namespace.";

	public const string Xml_BadElementData = "The element data is invalid.";

	public const string Xml_UnexpectedElement = "The <{0}> tag from namespace {1} is not expected.";

	public const string Xml_TagNotInTheSameEntity = "<{0}> and </{0}> are not defined in the same entity.";

	public const string Xml_InvalidPartialContentData = "There is invalid partial content data.";

	public const string Xml_CanNotStartWithXmlInNamespace = "Namespace qualifiers beginning with 'xml' are reserved, and cannot be used in user-specified namespaces.";

	public const string Xml_UnparsedEntity = "The '{0}' entity is not an unparsed entity.";

	public const string Xml_InvalidContentForThisNode = "Invalid content for {0} NodeType.";

	public const string Xml_MissingEncodingDecl = "Encoding declaration is required in an XmlDeclaration in an external entity.";

	public const string Xml_InvalidSurrogatePair = "The surrogate pair is invalid.";

	public const string Sch_ErrorPosition = "An error occurred at {0}, ({1}, {2}).";

	public const string Sch_ReservedNsDecl = "The '{0}' prefix is reserved.";

	public const string Sch_NotInSchemaCollection = "The '{0}' schema does not exist in the XmlSchemaCollection.";

	public const string Sch_NotationNotAttr = "This NOTATION should be used only on attributes.";

	public const string Sch_InvalidContent = "The element '{0}' has invalid content.";

	public const string Sch_InvalidContentExpecting = "The element '{0}' has invalid content. Expected '{1}'.";

	public const string Sch_InvalidTextWhiteSpace = "The element cannot contain text or white space. Content model is empty.";

	public const string Sch_XSCHEMA = "x-schema can load only XDR schemas.";

	public const string Sch_DubSchema = "Schema for targetNamespace '{0}' already present in collection and being used for validation.";

	public const string Xp_TokenExpected = "A token was expected.";

	public const string Xp_NodeTestExpected = "A NodeTest was expected at {0}.";

	public const string Xp_NumberExpected = "A number was expected.";

	public const string Xp_QueryExpected = "A query was expected.";

	public const string Xp_InvalidArgument = "'{0}' function in '{1}' has an invalid argument. Possibly ')' is missing.";

	public const string Xp_FunctionExpected = "A function was expected.";

	public const string Xp_InvalidPatternString = "{0} is an invalid XSLT pattern.";

	public const string Xp_BadQueryString = "The XPath expression passed into Compile() is null or empty.";

	public const string XdomXpNav_NullParam = "The parameter (other) being passed in is null.";

	public const string Xdom_Load_NodeType = "XmlLoader.Load(): Unexpected NodeType: {0}.";

	public const string XmlMissingMethod = "{0} was not found in {1}.";

	public const string XmlIncludeSerializableError = "Type {0} is derived from {1} and therefore cannot be used with attribute XmlInclude.";

	public const string XmlCompilerDynModule = "Unable to generate a serializer for type {0} from assembly {1} because the assembly may be dynamic. Save the assembly and load it from disk to use it with XmlSerialization.";

	public const string XmlInvalidSchemaType = "Types must be declared at the top level in the schema.";

	public const string XmlInvalidAnyUse = "Any may not be specified.";

	public const string XmlSchemaSyntaxError = "Schema with targetNamespace='{0}' has invalid syntax.";

	public const string XmlDuplicateChoiceElement = "The XML element named '{0}' from namespace '{1}' is already present in the current scope. Elements with the same name in the same scope must have the same type.";

	public const string XmlConvert_BadTimeSpan = "The string was not recognized as a valid TimeSpan value.";

	public const string XmlConvert_BadBoolean = "The string was not recognized as a valid Boolean value.";

	public const string Xml_UnexpectedToken = "This is an unexpected token. The expected token is '{0}'.";

	public const string Xml_PartialContentNodeTypeNotSupported = "This NodeType is not supported for partial content parsing.";

	public const string Sch_AttributeValueDataType = "The '{0}' attribute has an invalid value according to its data type.";

	public const string Sch_ElementValueDataType = "The '{0}' element has an invalid value according to its data type.";

	public const string Sch_NonDeterministicAny = "The content model must be deterministic. Wildcard declaration along with a local element declaration causes the content model to become ambiguous.";

	public const string Sch_MismatchTargetNamespace = "The attribute targetNamespace does not match the designated namespace URI.";

	public const string Sch_UnionFailed = "Union does not support this value.";

	public const string Sch_XsiTypeBlocked = "The element '{0}' has xsi:type derivation blocked.";

	public const string Sch_InvalidElementInEmpty = "The element cannot contain child element. Content model is empty.";

	public const string Sch_InvalidElementInTextOnly = "The element cannot contain a child element. Content model is text only.";

	public const string Sch_InvalidNameAttribute = "Invalid 'name' attribute value: {0}.";

	public const string XmlInternalError = "Internal error.";

	public const string XmlInternalErrorDetails = "Internal error: {0}.";

	public const string XmlInternalErrorMethod = "Internal error: missing generated method for {0}.";

	public const string XmlInternalErrorReaderAdvance = "Internal error: deserialization failed to advance over underlying stream.";

	public const string Enc_InvalidByteInEncoding = "Invalid byte was found at index {0}.";

	public const string Arg_ExpectingXmlTextReader = "The XmlReader passed in to construct this XmlValidatingReaderImpl must be an instance of a System.Xml.XmlTextReader.";

	public const string Arg_CannotCreateNode = "Cannot create node of type {0}.";

	public const string Arg_IncompatibleParamType = "Type is incompatible.";

	public const string XmlNonCLSCompliantException = "Non-CLS Compliant Exception.";

	public const string Xml_CannotFindFileInXapPackage = "Cannot find file '{0}' in the application xap package.";

	public const string Xml_XapResolverCannotOpenUri = "Cannot open '{0}'. The Uri parameter must be a relative path pointing to content inside the Silverlight application's XAP package. If you need to load content from an arbitrary Uri, please see the documentation on Loading XML content using WebClient/HttpWebRequest.";

	public static string GetString(string name)
	{
		return name;
	}

	public static string GetString(string name, params object[] args)
	{
		if (args == null)
		{
			return name;
		}
		return global::SR.GetString(name, args);
	}
}
