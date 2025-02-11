namespace MS.Internal.Globalization;

internal struct BamlStringToken
{
	internal enum TokenType
	{
		Text,
		ChildPlaceHolder
	}

	internal readonly TokenType Type;

	internal readonly string Value;

	internal BamlStringToken(TokenType type, string value)
	{
		Type = type;
		Value = value;
	}
}
