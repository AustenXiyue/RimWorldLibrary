using BillDoorsFramework;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BDsArknightLib
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        private static Harmony harmonyInstance;

        internal static Harmony HarmonyInstance
        {
            get
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new Harmony("BDsArknightLib");
                }
                return harmonyInstance;
            }
        }

        static PatchMain()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "Apply")]
    static class DamageWorker_AddInjury_Prefix
    {
        [HarmonyPrefix]
        static void Prefix(ref DamageInfo dinfo, Thing thing, DamageWorker_AddInjury __instance)
        {
            __instance.def.GetModExtension<ModExtension_IceBreaker>()?.ApplyBonus(ref dinfo, thing);
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    static class TakeDamage_PreFix
    {
        [HarmonyPrefix]
        static void Prefix(ref DamageInfo dinfo)
        {
            if (dinfo.Instigator == null) return;
            if (dinfo.Instigator is Pawn pawn)
            {
                if (GameComponent_DamageBuffTracker.Tracker.CheckForDamageBuff(pawn) is DamageBuffWorker buff)
                {
                    buff.Apply(ref dinfo);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryGenerateRaidInfo")]
    static class TryGenerateRaidInfo_PreFix
    {
        static readonly MethodInfo searchField = typeof(PawnsArrivalModeWorker).GetMethod("Arrive", BindingFlags.Public | BindingFlags.Instance);
        static IncidentWorker_Raid instance;
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> transp(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            for (int i = 0; i <= list.Count; ++i)
            {
                if (list[i].opcode == OpCodes.Callvirt && (MethodInfo)list[i].operand == searchField)
                {
                    MethodInfo overrideMethod = typeof(TryGenerateRaidInfo_PreFix).GetMethod("InsertMethod", BindingFlags.Static | BindingFlags.Public);
                    list.InsertRange(i + 1, new CodeInstruction[]
                        {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldind_Ref),
                    new CodeInstruction(OpCodes.Call,overrideMethod),
                        });
                    break;
                }
            }
            return list;
        }

        public static void InsertMethod(IncidentWorker instance, IncidentParms parms, List<Pawn> pawns)
        {
            if (instance.def.GetModExtension<ModExtension_CramInPDCs>() is ModExtension_CramInPDCs ext)
            {
                var list = ext.GetCharacters(parms.faction).ToList();
                parms.raidArrivalMode.Worker.Arrive(list, parms);
                foreach (var v in list)
                {
                    pawns.Add(v);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn_DrawTracker), "get_DrawPos")]
    static class Pawn_DrawTracker_PostFix
    {
        static FieldInfo pawnInfo => typeof(Pawn_DrawTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPostfix]
        static void Postfix(Pawn_DrawTracker __instance, ref Vector3 __result)
        {
            var pawn = pawnInfo.GetValue(__instance) as Pawn;
            if (GameComponent_InvoluntaryMovingTracker.Tracker.DataForPawn(pawn) is InvoluntaryMovingData data)
            {
                __result += data.drawPosOffset;
            }
        }
    }
}
