namespace UnityEngine.UIElements.StyleSheets;

internal struct MatchResult
{
	public MatchResultErrorCode errorCode;

	public string errorValue;

	public bool success => errorCode == MatchResultErrorCode.None;
}
