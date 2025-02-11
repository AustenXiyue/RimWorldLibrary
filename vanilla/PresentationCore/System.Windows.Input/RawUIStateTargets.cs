namespace System.Windows.Input;

[Flags]
internal enum RawUIStateTargets
{
	None = 0,
	HideFocus = 1,
	HideAccelerators = 2,
	Active = 4
}
