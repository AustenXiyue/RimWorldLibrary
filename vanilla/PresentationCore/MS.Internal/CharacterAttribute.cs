using System.Runtime.InteropServices;

namespace MS.Internal;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CharacterAttribute
{
	internal byte Script;

	internal byte ItemClass;

	internal ushort Flags;

	internal byte BreakType;

	internal DirectionClass BiDi;

	internal short LineBreak;
}
