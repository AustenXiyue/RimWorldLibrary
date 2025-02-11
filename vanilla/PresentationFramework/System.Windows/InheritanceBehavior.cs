namespace System.Windows;

/// <summary>Indicates the current mode of lookup for both property value inheritance, a resource lookup, and a RelativeSource FindAncestor lookup.</summary>
public enum InheritanceBehavior
{
	/// <summary>Property value inheritance lookup will query the current element and continue walking up the element tree to the page root.</summary>
	Default,
	/// <summary>Property value inheritance lookup will not query the current element or any further.</summary>
	SkipToAppNow,
	/// <summary>Property value inheritance lookup will query the current element but not any further.</summary>
	SkipToAppNext,
	/// <summary>Property value inheritance lookup will not query the current element or any further.</summary>
	SkipToThemeNow,
	/// <summary>Property value inheritance lookup will query the current element but not any further.</summary>
	SkipToThemeNext,
	/// <summary>Property value inheritance lookup will not query the current element or any further.</summary>
	SkipAllNow,
	/// <summary>Property value inheritance lookup will query the current element but not any further.</summary>
	SkipAllNext
}
