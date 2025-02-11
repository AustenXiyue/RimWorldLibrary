namespace System.Windows;

/// <summary>Specifies the readability value of a <see cref="T:System.Windows.LocalizabilityAttribute" /> for a binary XAML (BAML) class or class member.</summary>
public enum Readability
{
	/// <summary>Targeted value is not readable.</summary>
	Unreadable,
	/// <summary>Targeted value is readable text.</summary>
	Readable,
	/// <summary>Targeted value readability is inherited from its parent node.</summary>
	Inherit
}
