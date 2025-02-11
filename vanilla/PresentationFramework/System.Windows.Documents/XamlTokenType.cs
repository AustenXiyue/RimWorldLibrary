namespace System.Windows.Documents;

internal enum XamlTokenType
{
	XTokInvalid,
	XTokEOF,
	XTokCharacters,
	XTokEntity,
	XTokStartElement,
	XTokEndElement,
	XTokCData,
	XTokPI,
	XTokComment,
	XTokWS
}
