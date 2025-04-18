using System.Reflection;
using System.Reflection.Emit;

namespace MonoMod.Utils;

internal abstract class DMDGenerator<TSelf> : IDMDGenerator where TSelf : DMDGenerator<TSelf>, new()
{
	private static TSelf? Instance;

	protected abstract MethodInfo GenerateCore(DynamicMethodDefinition dmd, object? context);

	MethodInfo IDMDGenerator.Generate(DynamicMethodDefinition dmd, object? context)
	{
		return Postbuild(GenerateCore(dmd, context));
	}

	public static MethodInfo Generate(DynamicMethodDefinition dmd, object? context = null)
	{
		return Postbuild((Instance ?? (Instance = new TSelf())).GenerateCore(dmd, context));
	}

	internal static MethodInfo Postbuild(MethodInfo mi)
	{
		if (PlatformDetection.Runtime == RuntimeKind.Mono && !(mi is DynamicMethod) && mi.DeclaringType != null)
		{
			Module module = mi.Module;
			if ((object)module == null)
			{
				return mi;
			}
			Assembly assembly = module.Assembly;
			if ((object)assembly.GetType() == null)
			{
				return mi;
			}
			assembly.SetMonoCorlibInternal(value: true);
		}
		return mi;
	}
}
