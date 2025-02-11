namespace System.Windows.Controls;

[Flags]
internal enum SetPropertyFlags : byte
{
	WindowTitle = 1,
	WindowHeight = 2,
	WindowWidth = 4,
	Title = 8,
	ShowsNavigationUI = 0x10,
	None = 0
}
