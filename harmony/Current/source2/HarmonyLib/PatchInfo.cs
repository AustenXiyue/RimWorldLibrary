using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HarmonyLib;

[Serializable]
public class PatchInfo
{
	public Patch[] prefixes = Array.Empty<Patch>();

	public Patch[] postfixes = Array.Empty<Patch>();

	public Patch[] transpilers = Array.Empty<Patch>();

	public Patch[] finalizers = Array.Empty<Patch>();

	public bool Debugging
	{
		get
		{
			if (!prefixes.Any((Patch p) => p.debug) && !postfixes.Any((Patch p) => p.debug) && !transpilers.Any((Patch p) => p.debug))
			{
				return finalizers.Any((Patch p) => p.debug);
			}
			return true;
		}
	}

	internal void AddPrefixes(string owner, params HarmonyMethod[] methods)
	{
		prefixes = Add(owner, methods, prefixes);
	}

	[Obsolete("This method only exists for backwards compatibility since the class is public.")]
	public void AddPrefix(MethodInfo patch, string owner, int priority, string[] before, string[] after, bool debug)
	{
		AddPrefixes(owner, new HarmonyMethod(patch, priority, before, after, debug));
	}

	public void RemovePrefix(string owner)
	{
		prefixes = Remove(owner, prefixes);
	}

	internal void AddPostfixes(string owner, params HarmonyMethod[] methods)
	{
		postfixes = Add(owner, methods, postfixes);
	}

	[Obsolete("This method only exists for backwards compatibility since the class is public.")]
	public void AddPostfix(MethodInfo patch, string owner, int priority, string[] before, string[] after, bool debug)
	{
		AddPostfixes(owner, new HarmonyMethod(patch, priority, before, after, debug));
	}

	public void RemovePostfix(string owner)
	{
		postfixes = Remove(owner, postfixes);
	}

	internal void AddTranspilers(string owner, params HarmonyMethod[] methods)
	{
		transpilers = Add(owner, methods, transpilers);
	}

	[Obsolete("This method only exists for backwards compatibility since the class is public.")]
	public void AddTranspiler(MethodInfo patch, string owner, int priority, string[] before, string[] after, bool debug)
	{
		AddTranspilers(owner, new HarmonyMethod(patch, priority, before, after, debug));
	}

	public void RemoveTranspiler(string owner)
	{
		transpilers = Remove(owner, transpilers);
	}

	internal void AddFinalizers(string owner, params HarmonyMethod[] methods)
	{
		finalizers = Add(owner, methods, finalizers);
	}

	[Obsolete("This method only exists for backwards compatibility since the class is public.")]
	public void AddFinalizer(MethodInfo patch, string owner, int priority, string[] before, string[] after, bool debug)
	{
		AddFinalizers(owner, new HarmonyMethod(patch, priority, before, after, debug));
	}

	public void RemoveFinalizer(string owner)
	{
		finalizers = Remove(owner, finalizers);
	}

	public void RemovePatch(MethodInfo patch)
	{
		prefixes = prefixes.Where((Patch p) => p.PatchMethod != patch).ToArray();
		postfixes = postfixes.Where((Patch p) => p.PatchMethod != patch).ToArray();
		transpilers = transpilers.Where((Patch p) => p.PatchMethod != patch).ToArray();
		finalizers = finalizers.Where((Patch p) => p.PatchMethod != patch).ToArray();
	}

	private static Patch[] Add(string owner, HarmonyMethod[] add, Patch[] current)
	{
		if (add.Length == 0)
		{
			return current;
		}
		int initialIndex = current.Length;
		List<Patch> list = new List<Patch>();
		list.AddRange(current);
		list.AddRange(add.Where((HarmonyMethod method) => method != null).Select((HarmonyMethod method, int i) => new Patch(method, i + initialIndex, owner)));
		return list.ToArray();
	}

	private static Patch[] Remove(string owner, Patch[] current)
	{
		if (!(owner == "*"))
		{
			return current.Where((Patch patch) => patch.owner != owner).ToArray();
		}
		return Array.Empty<Patch>();
	}
}
