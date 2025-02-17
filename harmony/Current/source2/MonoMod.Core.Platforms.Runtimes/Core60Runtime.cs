using System;
using MonoMod.Core.Interop;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal class Core60Runtime : Core50Runtime
{
	private static readonly Guid JitVersionGuid = new Guid(1590910040u, 34171, 18653, 168, 24, 124, 1, 54, 220, 159, 115);

	protected override Guid ExpectedJitVersion => JitVersionGuid;

	protected override CoreCLR.InvokeCompileMethodPtr InvokeCompileMethodPtr => CoreCLR.V60.InvokeCompileMethodPtr;

	public Core60Runtime(ISystem system)
		: base(system)
	{
	}

	protected override Delegate CastCompileHookToRealType(Delegate del)
	{
		return del.CastDelegate<CoreCLR.V60.CompileMethodDelegate>();
	}
}
