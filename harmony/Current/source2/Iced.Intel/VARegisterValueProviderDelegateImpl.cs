using System;

namespace Iced.Intel;

internal sealed class VARegisterValueProviderDelegateImpl : IVATryGetRegisterValueProvider
{
	private readonly VAGetRegisterValue getRegisterValue;

	public VARegisterValueProviderDelegateImpl(VAGetRegisterValue getRegisterValue)
	{
		this.getRegisterValue = getRegisterValue ?? throw new ArgumentNullException("getRegisterValue");
	}

	public bool TryGetRegisterValue(Register register, int elementIndex, int elementSize, out ulong value)
	{
		value = getRegisterValue(register, elementIndex, elementSize);
		return true;
	}
}
