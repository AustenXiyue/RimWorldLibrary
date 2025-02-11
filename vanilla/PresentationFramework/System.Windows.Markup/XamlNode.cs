namespace System.Windows.Markup;

internal class XamlNode
{
	internal static XamlNodeType[] ScopeStartTokens = new XamlNodeType[6]
	{
		XamlNodeType.DocumentStart,
		XamlNodeType.ElementStart,
		XamlNodeType.PropertyComplexStart,
		XamlNodeType.PropertyArrayStart,
		XamlNodeType.PropertyIListStart,
		XamlNodeType.PropertyIDictionaryStart
	};

	internal static XamlNodeType[] ScopeEndTokens = new XamlNodeType[6]
	{
		XamlNodeType.DocumentEnd,
		XamlNodeType.ElementEnd,
		XamlNodeType.PropertyComplexEnd,
		XamlNodeType.PropertyArrayEnd,
		XamlNodeType.PropertyIListEnd,
		XamlNodeType.PropertyIDictionaryEnd
	};

	private XamlNodeType _token;

	private int _lineNumber;

	private int _linePosition;

	private int _depth;

	internal XamlNodeType TokenType => _token;

	internal int LineNumber => _lineNumber;

	internal int LinePosition => _linePosition;

	internal int Depth => _depth;

	internal XamlNode(XamlNodeType tokenType, int lineNumber, int linePosition, int depth)
	{
		_token = tokenType;
		_lineNumber = lineNumber;
		_linePosition = linePosition;
		_depth = depth;
	}
}
