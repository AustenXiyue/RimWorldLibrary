using System;
using MonoMod.Core.Interop;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal class Core30Runtime : Core21Runtime
{
	private static readonly Guid JitVersionGuid = new Guid(3590962897u, 30769, 18940, 189, 73, 182, 240, 84, 221, 77, 70);

	protected override Guid ExpectedJitVersion => JitVersionGuid;

	protected override CoreCLR.InvokeCompileMethodPtr InvokeCompileMethodPtr => CoreCLR.V21.InvokeCompileMethodPtr;

	public Core30Runtime(ISystem system)
		: base(system)
	{
	}

	protected override Delegate CastCompileHookToRealType(Delegate del)
	{
		return del.CastDelegate<CoreCLR.V21.CompileMethodDelegate>();
	}
}
