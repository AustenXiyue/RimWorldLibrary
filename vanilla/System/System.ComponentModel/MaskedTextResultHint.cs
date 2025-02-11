namespace System.ComponentModel;

/// <summary>Specifies values that succinctly describe the results of a masked text parsing operation.</summary>
public enum MaskedTextResultHint
{
	/// <summary>Unknown. The result of the operation could not be determined.</summary>
	Unknown = 0,
	/// <summary>Success. The operation succeeded because a literal, prompt or space character was an escaped character. For more information about escaped characters, see the <see cref="M:System.ComponentModel.MaskedTextProvider.VerifyEscapeChar(System.Char,System.Int32)" /> method.</summary>
	CharacterEscaped = 1,
	/// <summary>Success. The primary operation was not performed because it was not needed; therefore, no side effect was produced.</summary>
	NoEffect = 2,
	/// <summary>Success. The primary operation was not performed because it was not needed, but the method produced a side effect. For example, the <see cref="Overload:System.ComponentModel.MaskedTextProvider.RemoveAt" /> method can delete an unassigned edit position, which causes left-shifting of subsequent characters in the formatted string. </summary>
	SideEffect = 3,
	/// <summary>Success. The primary operation succeeded.</summary>
	Success = 4,
	/// <summary>Operation did not succeed.An input character was encountered that was not a member of the ASCII character set.</summary>
	AsciiCharacterExpected = -1,
	/// <summary>Operation did not succeed.An input character was encountered that was not alphanumeric. .</summary>
	AlphanumericCharacterExpected = -2,
	/// <summary>Operation did not succeed. An input character was encountered that was not a digit.</summary>
	DigitExpected = -3,
	/// <summary>Operation did not succeed. An input character was encountered that was not a letter.</summary>
	LetterExpected = -4,
	/// <summary>Operation did not succeed. An input character was encountered that was not a signed digit.</summary>
	SignedDigitExpected = -5,
	/// <summary>Operation did not succeed. The program encountered an  input character that was not valid. For more information about characters that are not valid, see the <see cref="M:System.ComponentModel.MaskedTextProvider.IsValidInputChar(System.Char)" /> method.</summary>
	InvalidInput = -51,
	/// <summary>Operation did not succeed. The prompt character is not valid at input, perhaps because the <see cref="P:System.ComponentModel.MaskedTextProvider.AllowPromptAsInput" /> property is set to false. </summary>
	PromptCharNotAllowed = -52,
	/// <summary>Operation did not succeed. There were not enough edit positions available to fulfill the request.</summary>
	UnavailableEditPosition = -53,
	/// <summary>Operation did not succeed. The current position in the formatted string is a literal character. </summary>
	NonEditPosition = -54,
	/// <summary>Operation did not succeed. The specified position is not in the range of the target string; typically it is either less than zero or greater then the length of the target string.</summary>
	PositionOutOfRange = -55
}
