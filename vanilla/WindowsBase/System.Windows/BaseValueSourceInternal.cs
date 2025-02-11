using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal enum BaseValueSourceInternal : short
{
	Unknown,
	Default,
	Inherited,
	ThemeStyle,
	ThemeStyleTrigger,
	Style,
	TemplateTrigger,
	StyleTrigger,
	ImplicitReference,
	ParentTemplate,
	ParentTemplateTrigger,
	Local
}
