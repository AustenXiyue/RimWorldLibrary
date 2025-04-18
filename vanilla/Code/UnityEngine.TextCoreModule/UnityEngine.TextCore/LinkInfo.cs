namespace UnityEngine.TextCore;

internal struct LinkInfo
{
	public int hashCode;

	public int linkIdFirstCharacterIndex;

	public int linkIdLength;

	public int linkTextfirstCharacterIndex;

	public int linkTextLength;

	internal char[] linkId;

	internal void SetLinkId(char[] text, int startIndex, int length)
	{
		if (linkId == null || linkId.Length < length)
		{
			linkId = new char[length];
		}
		for (int i = 0; i < length; i++)
		{
			linkId[i] = text[startIndex + i];
		}
	}

	public string GetLinkText(TextInfo textInfo)
	{
		string text = string.Empty;
		for (int i = linkTextfirstCharacterIndex; i < linkTextfirstCharacterIndex + linkTextLength; i++)
		{
			text += textInfo.textElementInfo[i].character;
		}
		return text;
	}

	public string GetLinkId()
	{
		return new string(linkId, 0, linkIdLength);
	}
}
