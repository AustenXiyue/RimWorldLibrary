using System.Collections.Generic;
using HarmonyLib;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(CompAbilities), "GetGizmos")]
public static class CompAbilities_GetGizmos_Patch
{
	public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, CompAbilities __instance)
	{
		Pawn parent = ((ThingComp)(object)__instance).parent as Pawn;
		Hediff_PsycastAbilities psycasts = parent.Psycasts();
		if (psycasts != null && (parent?.Drafted ?? false))
		{
			foreach (Gizmo psySetGizmo in psycasts.GetPsySetGizmos())
			{
				yield return psySetGizmo;
			}
		}
		Command_Ability command = default(Command_Ability);
		foreach (Gizmo gizmo in gizmos)
		{
			int num;
			if (psycasts != null)
			{
				command = (Command_Ability)(object)((gizmo is Command_Ability) ? gizmo : null);
				if (command != null)
				{
					num = (((Def)(object)command.ability.def).HasModExtension<AbilityExtension_Psycast>() ? 1 : 0);
					goto IL_016a;
				}
			}
			num = 0;
			goto IL_016a;
			IL_016a:
			if (num != 0)
			{
				if (psycasts.ShouldShow(command.ability))
				{
					yield return (Gizmo)(object)command;
				}
			}
			else
			{
				yield return gizmo;
			}
			command = null;
		}
	}
}
