namespace System.Xaml;

[Flags]
internal enum BoolMemberBits
{
	ReadOnly = 1,
	WriteOnly = 2,
	Event = 4,
	Unknown = 8,
	Ambient = 0x10,
	ReadPublic = 0x20,
	WritePublic = 0x40,
	Default = 0x60,
	Directive = 0x60,
	AllValid = -65536
}
