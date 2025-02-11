namespace System.Xaml.Schema;

/// <summary>Specifies the syntax restrictions enforced on a property when it is set in XAML, as reported by a <see cref="T:System.Xaml.XamlDirective" />.</summary>
[Flags]
public enum AllowedMemberLocations
{
	/// <summary>Property cannot be set in XAML at all. This is the default.</summary>
	None = 0,
	/// <summary>Property can be set in XAML attribute syntax.</summary>
	Attribute = 1,
	/// <summary>Property can be set in XAML property element syntax.</summary>
	MemberElement = 2,
	/// <summary>Property can be set in either <see cref="F:System.Xaml.Schema.AllowedMemberLocations.Attribute" /> or <see cref="F:System.Xaml.Schema.AllowedMemberLocations.MemberElement" /> location. (This enumeration member is defined as the combination of those values.)</summary>
	Any = 3
}
