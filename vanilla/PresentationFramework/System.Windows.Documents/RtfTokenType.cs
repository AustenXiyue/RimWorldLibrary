namespace System.Windows.Documents;

internal enum RtfTokenType
{
	TokenInvalid,
	TokenEOF,
	TokenText,
	TokenTextSymbol,
	TokenPictureData,
	TokenNewline,
	TokenNullChar,
	TokenControl,
	TokenDestination,
	TokenHex,
	TokenGroupStart,
	TokenGroupEnd
}
