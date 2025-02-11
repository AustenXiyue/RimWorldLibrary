using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace CombatExtended.Compatibility;

public class Patches
{
	private List<IPatch> patches;

	public static List<Func<IEnumerable<ThingDef>>> UsedAmmoCallbacks = new List<Func<IEnumerable<ThingDef>>>();

	public static List<Func<Pawn, Tuple<bool, Vector2>>> CollisionBodyFactorCallbacks = new List<Func<Pawn, Tuple<bool, Vector2>>>();

	private static bool _gcbfactive = false;

	public Patches()
	{
		patches = new List<IPatch>();
		Type typeFromHandle = typeof(IPatch);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (typeFromHandle.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
					{
						IPatch item = (IPatch)type.GetConstructor(new Type[0]).Invoke(new object[0]);
						patches.Add(item);
					}
				}
			}
			catch (ReflectionTypeLoadException ex)
			{
				if (!Prefs.DevMode)
				{
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine($"[CE] ReflectionTypeLoadException while looking for compat patches in assembly {assembly.GetName().Name}: {ex}");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Loader exceptions:");
				if (ex.LoaderExceptions != null)
				{
					Exception[] loaderExceptions = ex.LoaderExceptions;
					foreach (Exception ex2 in loaderExceptions)
					{
						stringBuilder.AppendLine("   => " + ex2.ToString());
					}
				}
				Log.Warning(stringBuilder.ToString());
			}
		}
	}

	public void Install()
	{
		foreach (IPatch patch in patches)
		{
			if (patch.CanInstall())
			{
				patch.Install();
			}
		}
	}

	public static IEnumerable<ThingDef> GetUsedAmmo()
	{
		foreach (Func<IEnumerable<ThingDef>> cb in UsedAmmoCallbacks)
		{
			foreach (ThingDef item in cb())
			{
				yield return item;
			}
		}
	}

	public static void RegisterCollisionBodyFactorCallback(Func<Pawn, Tuple<bool, Vector2>> f)
	{
		CollisionBodyFactorCallbacks.Add(f);
		_gcbfactive = true;
	}

	public static bool GetCollisionBodyFactors(Pawn pawn, out Vector2 ret)
	{
		ret = default(Vector2);
		if (_gcbfactive)
		{
			foreach (Func<Pawn, Tuple<bool, Vector2>> collisionBodyFactorCallback in CollisionBodyFactorCallbacks)
			{
				Tuple<bool, Vector2> tuple = collisionBodyFactorCallback(pawn);
				if (tuple.Item1)
				{
					ret = tuple.Item2;
					return true;
				}
			}
		}
		return false;
	}
}
