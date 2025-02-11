namespace System.Windows.Input;

[Flags]
internal enum RawStylusActions
{
	None = 0,
	Activate = 1,
	Deactivate = 2,
	Down = 4,
	Up = 8,
	Move = 0x10,
	InAirMove = 0x20,
	InRange = 0x40,
	OutOfRange = 0x80,
	SystemGesture = 0x100
}
