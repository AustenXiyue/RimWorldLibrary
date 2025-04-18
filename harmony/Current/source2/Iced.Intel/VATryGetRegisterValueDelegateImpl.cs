namespace Iced.Intel;

internal sealed class VATryGetRegisterValueDelegateImpl : IVATryGetRegisterValueProvider
{
	private readonly VATryGetRegisterValue getRegisterValue;

	public VATryGetRegisterValueDelegateImpl(VATryGetRegisterValue getRegisterValue)
	{
		this.getRegisterValue = getRegisterValue;
	}

	public bool TryGetRegisterValue(Register register, int elementIndex, int elementSize, out ulong value)
	{
		return getRegisterValue(register, elementIndex, elementSize, out value);
	}
}
