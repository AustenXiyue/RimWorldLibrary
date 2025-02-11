namespace System.Windows.Input;

/// <summary>Describes a mode of input conversion to be performed by an input method.</summary>
[Flags]
public enum ImeConversionModeValues
{
	/// <summary>The input method uses a native character (Hiragana, Hangul, Chinese) conversion mode.</summary>
	Native = 1,
	/// <summary>The input method uses Katakana conversion mode.</summary>
	Katakana = 2,
	/// <summary>The input method uses full-shape conversion mode.</summary>
	FullShape = 4,
	/// <summary>The input method uses Roman character conversion mode.</summary>
	Roman = 8,
	/// <summary>The input method uses character code conversion mode.</summary>
	CharCode = 0x10,
	/// <summary>The input method will not perform any input conversion.</summary>
	NoConversion = 0x20,
	/// <summary>The input method uses EUDC (end user defined character) conversion mode.</summary>
	Eudc = 0x40,
	/// <summary>The input method uses symbol conversion mode.</summary>
	Symbol = 0x80,
	/// <summary>The input method uses fixed conversion mode.</summary>
	Fixed = 0x100,
	/// <summary>The input method uses alphanumeric conversion mode.</summary>
	Alphanumeric = 0x200,
	/// <summary>The input method does not care what input conversion method is used; the actual conversion method is indeterminate.</summary>
	DoNotCare = int.MinValue
}
