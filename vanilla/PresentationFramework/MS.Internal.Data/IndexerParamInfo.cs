namespace MS.Internal.Data;

internal struct IndexerParamInfo
{
	public string parenString;

	public string valueString;

	public IndexerParamInfo(string paren, string value)
	{
		parenString = paren;
		valueString = value;
	}
}
