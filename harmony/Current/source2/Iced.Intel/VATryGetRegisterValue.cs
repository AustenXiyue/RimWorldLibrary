namespace Iced.Intel;

internal delegate bool VATryGetRegisterValue(Register register, int elementIndex, int elementSize, out ulong value);
