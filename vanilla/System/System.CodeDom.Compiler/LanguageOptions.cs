namespace System.CodeDom.Compiler;

/// <summary>Defines identifiers that indicate special features of a language.</summary>
[Flags]
public enum LanguageOptions
{
	/// <summary>The language has default characteristics.</summary>
	None = 0,
	/// <summary>The language is case-insensitive.</summary>
	CaseInsensitive = 1
}
