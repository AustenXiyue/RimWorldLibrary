using System.Reflection;
using Mono.Cecil;

namespace MonoMod.Utils;

internal class DynamicMethodReference : MethodReference
{
	public MethodInfo DynamicMethod { get; }

	public DynamicMethodReference(ModuleDefinition module, MethodInfo dm)
		: base("", Helpers.ThrowIfNull(module, "module").TypeSystem.Void)
	{
		DynamicMethod = dm;
	}
}
