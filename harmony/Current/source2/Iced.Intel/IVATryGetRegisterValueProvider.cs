namespace Iced.Intel;

internal interface IVATryGetRegisterValueProvider
{
	bool TryGetRegisterValue(Register register, int elementIndex, int elementSize, out ulong value);
}
