namespace MonoMod.Core.Platforms.Runtimes;

internal sealed class FxCLR2Runtime : FxBaseRuntime
{
	private readonly ISystem system;

	public FxCLR2Runtime(ISystem system)
	{
		this.system = system;
		Abi? abiCore = AbiCore;
		if (!abiCore.HasValue)
		{
			AbiCore = system.DefaultAbi;
		}
	}
}
