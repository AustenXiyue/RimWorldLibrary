namespace System.Windows.Input;

[Flags]
internal enum RawKeyboardActions
{
	None = 0,
	AttributesChanged = 1,
	Activate = 2,
	Deactivate = 4,
	KeyDown = 8,
	KeyUp = 0x10
}
