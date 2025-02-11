namespace System.Windows.Input;

[Flags]
internal enum RawMouseActions
{
	None = 0,
	AttributesChanged = 1,
	Activate = 2,
	Deactivate = 4,
	RelativeMove = 8,
	AbsoluteMove = 0x10,
	VirtualDesktopMove = 0x20,
	Button1Press = 0x40,
	Button1Release = 0x80,
	Button2Press = 0x100,
	Button2Release = 0x200,
	Button3Press = 0x400,
	Button3Release = 0x800,
	Button4Press = 0x1000,
	Button4Release = 0x2000,
	Button5Press = 0x4000,
	Button5Release = 0x8000,
	VerticalWheelRotate = 0x10000,
	HorizontalWheelRotate = 0x20000,
	QueryCursor = 0x40000,
	CancelCapture = 0x80000
}
