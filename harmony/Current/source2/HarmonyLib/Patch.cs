using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

[Serializable]
public class Patch : IComparable
{
	public readonly int index;

	public readonly string owner;

	public readonly int priority;

	public readonly string[] before;

	public readonly string[] after;

	public readonly bool debug;

	[NonSerialized]
	private MethodInfo patchMethod;

	private int methodToken;

	private string moduleGUID;

	public MethodInfo PatchMethod
	{
		get
		{
			if ((object)patchMethod == null)
			{
				Module module = (from a in AppDomain.CurrentDomain.GetAssemblies()
					where !a.FullName.StartsWith("Microsoft.VisualStudio")
					select a).SelectMany((Assembly a) => a.GetLoadedModules()).First((Module m) => m.ModuleVersionId.ToString() == moduleGUID);
				patchMethod = (MethodInfo)module.ResolveMethod(methodToken);
			}
			return patchMethod;
		}
		set
		{
			patchMethod = value;
			methodToken = patchMethod.MetadataToken;
			moduleGUID = patchMethod.Module.ModuleVersionId.ToString();
		}
	}

	public Patch(MethodInfo patch, int index, string owner, int priority, string[] before, string[] after, bool debug)
	{
		if (patch is DynamicMethod)
		{
			throw new Exception("Cannot directly reference dynamic method \"" + patch.FullDescription() + "\" in Harmony. Use a factory method instead that will return the dynamic method.");
		}
		this.index = index;
		this.owner = owner;
		this.priority = ((priority == -1) ? 400 : priority);
		this.before = before ?? Array.Empty<string>();
		this.after = after ?? Array.Empty<string>();
		this.debug = debug;
		PatchMethod = patch;
	}

	public Patch(HarmonyMethod method, int index, string owner)
		: this(method.method, index, owner, method.priority, method.before, method.after, method.debug == true)
	{
	}

	internal Patch(int index, string owner, int priority, string[] before, string[] after, bool debug, int methodToken, string moduleGUID)
	{
		this.index = index;
		this.owner = owner;
		this.priority = ((priority == -1) ? 400 : priority);
		this.before = before ?? Array.Empty<string>();
		this.after = after ?? Array.Empty<string>();
		this.debug = debug;
		this.methodToken = methodToken;
		this.moduleGUID = moduleGUID;
	}

	public MethodInfo GetMethod(MethodBase original)
	{
		MethodInfo methodInfo = PatchMethod;
		if (methodInfo.ReturnType != typeof(DynamicMethod) && methodInfo.ReturnType != typeof(MethodInfo))
		{
			return methodInfo;
		}
		if (!methodInfo.IsStatic)
		{
			return methodInfo;
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Length != 1)
		{
			return methodInfo;
		}
		if (parameters[0].ParameterType != typeof(MethodBase))
		{
			return methodInfo;
		}
		return methodInfo.Invoke(null, new object[1] { original }) as MethodInfo;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is Patch)
		{
			return PatchMethod == ((Patch)obj).PatchMethod;
		}
		return false;
	}

	public int CompareTo(object obj)
	{
		return PatchInfoSerialization.PriorityComparer(obj, index, priority);
	}

	public override int GetHashCode()
	{
		return PatchMethod.GetHashCode();
	}
}
