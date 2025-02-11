namespace System.Windows;

/// <summary>Specifies the modifiability value of a <see cref="T:System.Windows.LocalizabilityAttribute" /> for a binary XAML (BAML) class or class member.</summary>
public enum Modifiability
{
	/// <summary>Targeted value is not modifiable by localizers.</summary>
	Unmodifiable,
	/// <summary>Targeted value is modifiable by localizers.</summary>
	Modifiable,
	/// <summary>Targeted value modifiability is inherited from its parent node.</summary>
	Inherit
}
