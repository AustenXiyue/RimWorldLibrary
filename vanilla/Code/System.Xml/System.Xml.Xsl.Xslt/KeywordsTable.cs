namespace System.Xml.Xsl.Xslt;

internal class KeywordsTable
{
	public XmlNameTable NameTable;

	public string AnalyzeString;

	public string ApplyImports;

	public string ApplyTemplates;

	public string Assembly;

	public string Attribute;

	public string AttributeSet;

	public string CallTemplate;

	public string CaseOrder;

	public string CDataSectionElements;

	public string Character;

	public string CharacterMap;

	public string Choose;

	public string Comment;

	public string Copy;

	public string CopyOf;

	public string Count;

	public string DataType;

	public string DecimalFormat;

	public string DecimalSeparator;

	public string DefaultCollation;

	public string DefaultValidation;

	public string Digit;

	public string DisableOutputEscaping;

	public string DocTypePublic;

	public string DocTypeSystem;

	public string Document;

	public string Element;

	public string Elements;

	public string Encoding;

	public string ExcludeResultPrefixes;

	public string ExtensionElementPrefixes;

	public string Fallback;

	public string ForEach;

	public string ForEachGroup;

	public string Format;

	public string From;

	public string Function;

	public string GroupingSeparator;

	public string GroupingSize;

	public string Href;

	public string Id;

	public string If;

	public string ImplementsPrefix;

	public string Import;

	public string ImportSchema;

	public string Include;

	public string Indent;

	public string Infinity;

	public string Key;

	public string Lang;

	public string Language;

	public string LetterValue;

	public string Level;

	public string Match;

	public string MatchingSubstring;

	public string MediaType;

	public string Message;

	public string Method;

	public string MinusSign;

	public string Mode;

	public string Name;

	public string Namespace;

	public string NamespaceAlias;

	public string NaN;

	public string NextMatch;

	public string NonMatchingSubstring;

	public string Number;

	public string OmitXmlDeclaration;

	public string Order;

	public string Otherwise;

	public string Output;

	public string OutputCharacter;

	public string OutputVersion;

	public string Param;

	public string PatternSeparator;

	public string Percent;

	public string PerformSort;

	public string PerMille;

	public string PreserveSpace;

	public string Priority;

	public string ProcessingInstruction;

	public string Required;

	public string ResultDocument;

	public string ResultPrefix;

	public string Script;

	public string Select;

	public string Separator;

	public string Sequence;

	public string Sort;

	public string Space;

	public string Standalone;

	public string StripSpace;

	public string Stylesheet;

	public string StylesheetPrefix;

	public string Template;

	public string Terminate;

	public string Test;

	public string Text;

	public string Transform;

	public string UrnMsxsl;

	public string UriXml;

	public string UriXsl;

	public string UriWdXsl;

	public string Use;

	public string UseAttributeSets;

	public string UseWhen;

	public string Using;

	public string Value;

	public string ValueOf;

	public string Variable;

	public string Version;

	public string When;

	public string WithParam;

	public string Xml;

	public string Xmlns;

	public string XPathDefaultNamespace;

	public string ZeroDigit;

	public KeywordsTable(XmlNameTable nt)
	{
		NameTable = nt;
		AnalyzeString = nt.Add("analyze-string");
		ApplyImports = nt.Add("apply-imports");
		ApplyTemplates = nt.Add("apply-templates");
		Assembly = nt.Add("assembly");
		Attribute = nt.Add("attribute");
		AttributeSet = nt.Add("attribute-set");
		CallTemplate = nt.Add("call-template");
		CaseOrder = nt.Add("case-order");
		CDataSectionElements = nt.Add("cdata-section-elements");
		Character = nt.Add("character");
		CharacterMap = nt.Add("character-map");
		Choose = nt.Add("choose");
		Comment = nt.Add("comment");
		Copy = nt.Add("copy");
		CopyOf = nt.Add("copy-of");
		Count = nt.Add("count");
		DataType = nt.Add("data-type");
		DecimalFormat = nt.Add("decimal-format");
		DecimalSeparator = nt.Add("decimal-separator");
		DefaultCollation = nt.Add("default-collation");
		DefaultValidation = nt.Add("default-validation");
		Digit = nt.Add("digit");
		DisableOutputEscaping = nt.Add("disable-output-escaping");
		DocTypePublic = nt.Add("doctype-public");
		DocTypeSystem = nt.Add("doctype-system");
		Document = nt.Add("document");
		Element = nt.Add("element");
		Elements = nt.Add("elements");
		Encoding = nt.Add("encoding");
		ExcludeResultPrefixes = nt.Add("exclude-result-prefixes");
		ExtensionElementPrefixes = nt.Add("extension-element-prefixes");
		Fallback = nt.Add("fallback");
		ForEach = nt.Add("for-each");
		ForEachGroup = nt.Add("for-each-group");
		Format = nt.Add("format");
		From = nt.Add("from");
		Function = nt.Add("function");
		GroupingSeparator = nt.Add("grouping-separator");
		GroupingSize = nt.Add("grouping-size");
		Href = nt.Add("href");
		Id = nt.Add("id");
		If = nt.Add("if");
		ImplementsPrefix = nt.Add("implements-prefix");
		Import = nt.Add("import");
		ImportSchema = nt.Add("import-schema");
		Include = nt.Add("include");
		Indent = nt.Add("indent");
		Infinity = nt.Add("infinity");
		Key = nt.Add("key");
		Lang = nt.Add("lang");
		Language = nt.Add("language");
		LetterValue = nt.Add("letter-value");
		Level = nt.Add("level");
		Match = nt.Add("match");
		MatchingSubstring = nt.Add("matching-substring");
		MediaType = nt.Add("media-type");
		Message = nt.Add("message");
		Method = nt.Add("method");
		MinusSign = nt.Add("minus-sign");
		Mode = nt.Add("mode");
		Name = nt.Add("name");
		Namespace = nt.Add("namespace");
		NamespaceAlias = nt.Add("namespace-alias");
		NaN = nt.Add("NaN");
		NextMatch = nt.Add("next-match");
		NonMatchingSubstring = nt.Add("non-matching-substring");
		Number = nt.Add("number");
		OmitXmlDeclaration = nt.Add("omit-xml-declaration");
		Otherwise = nt.Add("otherwise");
		Order = nt.Add("order");
		Output = nt.Add("output");
		OutputCharacter = nt.Add("output-character");
		OutputVersion = nt.Add("output-version");
		Param = nt.Add("param");
		PatternSeparator = nt.Add("pattern-separator");
		Percent = nt.Add("percent");
		PerformSort = nt.Add("perform-sort");
		PerMille = nt.Add("per-mille");
		PreserveSpace = nt.Add("preserve-space");
		Priority = nt.Add("priority");
		ProcessingInstruction = nt.Add("processing-instruction");
		Required = nt.Add("required");
		ResultDocument = nt.Add("result-document");
		ResultPrefix = nt.Add("result-prefix");
		Script = nt.Add("script");
		Select = nt.Add("select");
		Separator = nt.Add("separator");
		Sequence = nt.Add("sequence");
		Sort = nt.Add("sort");
		Space = nt.Add("space");
		Standalone = nt.Add("standalone");
		StripSpace = nt.Add("strip-space");
		Stylesheet = nt.Add("stylesheet");
		StylesheetPrefix = nt.Add("stylesheet-prefix");
		Template = nt.Add("template");
		Terminate = nt.Add("terminate");
		Test = nt.Add("test");
		Text = nt.Add("text");
		Transform = nt.Add("transform");
		UrnMsxsl = nt.Add("urn:schemas-microsoft-com:xslt");
		UriXml = nt.Add("http://www.w3.org/XML/1998/namespace");
		UriXsl = nt.Add("http://www.w3.org/1999/XSL/Transform");
		UriWdXsl = nt.Add("http://www.w3.org/TR/WD-xsl");
		Use = nt.Add("use");
		UseAttributeSets = nt.Add("use-attribute-sets");
		UseWhen = nt.Add("use-when");
		Using = nt.Add("using");
		Value = nt.Add("value");
		ValueOf = nt.Add("value-of");
		Variable = nt.Add("variable");
		Version = nt.Add("version");
		When = nt.Add("when");
		WithParam = nt.Add("with-param");
		Xml = nt.Add("xml");
		Xmlns = nt.Add("xmlns");
		XPathDefaultNamespace = nt.Add("xpath-default-namespace");
		ZeroDigit = nt.Add("zero-digit");
	}
}
