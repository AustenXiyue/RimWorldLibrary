using System;
using MonoMod.Core.Interop;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal class Core50Runtime : Core31Runtime
{
	private static readonly Guid JitVersionGuid = new Guid(2783888292u, 16758, 17319, 140, 43, 160, 91, 85, 29, 79, 73);

	protected override Guid ExpectedJitVersion => JitVersionGuid;

	protected override int VtableIndexICorJitCompilerGetVersionGuid => 2;

	protected override CoreCLR.InvokeCompileMethodPtr InvokeCompileMethodPtr => CoreCLR.V21.InvokeCompileMethodPtr;

	public Core50Runtime(ISystem system)
		: base(system)
	{
	}

	protected override Delegate CastCompileHookToRealType(Delegate del)
	{
		return del.CastDelegate<CoreCLR.V21.CompileMethodDelegate>();
	}
}
