namespace Iced.Intel;

internal interface IVARegisterValueProvider
{
	ulong GetRegisterValue(Register register, int elementIndex, int elementSize);
}
