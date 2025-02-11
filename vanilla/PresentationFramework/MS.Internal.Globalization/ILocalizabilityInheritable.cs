using System.Windows;

namespace MS.Internal.Globalization;

internal interface ILocalizabilityInheritable
{
	ILocalizabilityInheritable LocalizabilityAncestor { get; }

	LocalizabilityAttribute InheritableAttribute { get; set; }

	bool IsIgnored { get; set; }
}
