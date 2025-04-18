using System;

namespace MonoMod.Core.Platforms.Runtimes;

internal class Core80Runtime : Core70Runtime
{
	private static readonly Guid JitVersionGuid = new Guid(1271838981u, 54608, 19037, 177, 235, 39, 111, byte.MaxValue, 104, 209, 131);

	protected override Guid ExpectedJitVersion => JitVersionGuid;

	protected override int VtableIndexICorJitInfoAllocMem => 154;

	protected override int ICorJitInfoFullVtableCount => 170;

	public Core80Runtime(ISystem system, IArchitecture arch)
		: base(system, arch)
	{
	}
}
