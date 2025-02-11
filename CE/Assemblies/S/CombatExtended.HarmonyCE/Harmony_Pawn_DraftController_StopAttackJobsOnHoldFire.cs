using System.Linq;
using CombatExtended.Compatibility;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal class Harmony_Pawn_DraftController_StopAttackJobsOnHoldFire
{
	[HarmonyPostfix]
	public static void Postfix(Pawn_DraftController __instance, bool ___fireAtWillInt)
	{
		if (___fireAtWillInt || (Multiplayer.InMultiplayer && !Multiplayer.IsExecutingCommands))
		{
			return;
		}
		Pawn_JobTracker jobs = __instance.pawn.jobs;
		foreach (QueuedJob item in jobs.jobQueue.ToList())
		{
			if (item.job.def == JobDefOf.AttackStatic)
			{
				jobs.EndCurrentOrQueuedJob(item.job, JobCondition.Incompletable);
			}
		}
		if (__instance.pawn.CurJobDef == JobDefOf.AttackStatic)
		{
			jobs.EndCurrentJob(JobCondition.Incompletable);
		}
	}
}
