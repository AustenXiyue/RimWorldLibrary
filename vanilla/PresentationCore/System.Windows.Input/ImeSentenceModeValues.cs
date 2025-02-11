namespace System.Windows.Input;

/// <summary>Specifies the mode of sentence conversion performed by an input method.</summary>
[Flags]
public enum ImeSentenceModeValues
{
	/// <summary>The input method does not perform any sentence conversion.</summary>
	None = 0,
	/// <summary>The input method uses plural clause sentence conversion.</summary>
	PluralClause = 1,
	/// <summary>The input method uses single Kanji/Hanja sentence conversion.</summary>
	SingleConversion = 2,
	/// <summary>The input method uses the sentence conversion method automatically.</summary>
	Automatic = 4,
	/// <summary>The input method uses phrase prediction sentence conversion.</summary>
	PhrasePrediction = 8,
	/// <summary>The input method uses conversation-style sentence conversion.</summary>
	Conversation = 0x10,
	/// <summary>The input method does not care what sentence conversion method is used; the actual sentence conversion mode is indeterminate.</summary>
	DoNotCare = int.MinValue
}
