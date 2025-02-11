namespace MS.Internal.TextFormatting;

internal static class DoubleWideChar
{
	internal static int GetChar(CharacterBuffer charBuffer, int ichText, int cchText, int charNumber, out int wordCount)
	{
		if (charNumber < cchText - 1 && (charBuffer[ichText + charNumber] & 0xFC00) == 55296 && (charBuffer[ichText + charNumber + 1] & 0xFC00) == 56320)
		{
			wordCount = 2;
			return (((charBuffer[ichText + charNumber] & 0x3FF) << 10) | (charBuffer[ichText + charNumber + 1] & 0x3FF)) + 65536;
		}
		wordCount = 1;
		return charBuffer[ichText + charNumber];
	}
}
