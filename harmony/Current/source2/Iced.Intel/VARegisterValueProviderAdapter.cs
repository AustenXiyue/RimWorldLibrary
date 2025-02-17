namespace Iced.Intel;

internal sealed class VARegisterValueProviderAdapter : IVATryGetRegisterValueProvider
{
	private readonly IVARegisterValueProvider provider;

	public VARegisterValueProviderAdapter(IVARegisterValueProvider provider)
	{
		this.provider = provider;
	}

	public bool TryGetRegisterValue(Register register, int elementIndex, int elementSize, out ulong value)
	{
		value = provider.GetRegisterValue(register, elementIndex, elementSize);
		return true;
	}
}
