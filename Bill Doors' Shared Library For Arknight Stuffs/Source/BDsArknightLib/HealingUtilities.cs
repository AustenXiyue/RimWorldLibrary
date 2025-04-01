using RimWorld;
using System.Collections.Generic;
using System;
using Verse;
using static HarmonyLib.Code;

namespace BDsArknightLib
{
    public static class HealingUtilities
    {
        public static void Heal(Pawn Pawn, float healPoints, float maxRegenPartHP = -1)
        {
            List<Hediff> hediffsToHeal = new List<Hediff>();
            foreach (Hediff hediff in Pawn.health.hediffSet.hediffs.InRandomOrder())
            {
                if (hediff is Hediff_Injury)
                {
                    hediffsToHeal.Add(hediff);
                    continue;
                }
                if (maxRegenPartHP >= 0 && hediff is Hediff_MissingPart missingPart && Pawn.health.hediffSet.GetMissingPartFor(missingPart.Part.parent) == null && !Pawn.health.hediffSet.GetInjuredParts().Contains(missingPart.Part.parent))
                {
                    hediffsToHeal.Add(hediff);
                }
            }
            if (hediffsToHeal.Any())
            {
                int count = hediffsToHeal.Count;
                foreach (var hediffToHeal in hediffsToHeal)
                {
                    if (healPoints <= 0) break;
                    if (hediffToHeal is Hediff_Injury)
                    {
                        float actualHeal = Math.Min(healPoints / count, hediffToHeal.Severity);
                        hediffToHeal.Severity -= actualHeal;
                        if (hediffToHeal.Severity <= 0)
                        {
                            Pawn.health.RemoveHediff(hediffToHeal);
                        }
                        healPoints -= actualHeal;
                        count--;
                        continue;
                    }
                    if (maxRegenPartHP >= 0 && hediffToHeal is Hediff_MissingPart missingPart)
                    {
                        if (CannotRegenPart(missingPart.Part)) continue;
                        float severity = missingPart.Part.def.GetMaxHealth(Pawn);
                        float actualHeal = healPoints / count;
                        if (actualHeal < severity)
                        {
                            var part = missingPart.Part;
                            Hediff replacing = HediffMaker.MakeHediff(missingPart.lastInjury ?? HediffDefOf.Cut, Pawn, part);
                            Pawn.health.RemoveHediff(hediffToHeal);
                            Pawn.health.AddHediff(replacing, part);
                            replacing.Severity = severity - actualHeal;
                            healPoints -= actualHeal;
                        }
                        else
                        {
                            healPoints -= severity;
                            Pawn.health.RemoveHediff(hediffToHeal);
                        }
                    }
                }
            }

            bool CannotRegenPart(BodyPartRecord part)
            {
                if (maxRegenPartHP < 0) return true;
                if (maxRegenPartHP == 0) return false;
                if (part.def.hitPoints <= maxRegenPartHP) return false;
                return true;
            }
        }

        public static void CureBloodLoss(Pawn Pawn, float value)
        {
            value /= Pawn.BodySize;
            if (Pawn.health.hediffSet.TryGetHediff(HediffDefOf.BloodLoss, out Hediff b))
            {
                b.Severity -= value;
            }
        }

        public static void SpawnMaintainedIfPossible(this EffecterDef effecterDef, TargetInfo A, TargetInfo B, float scale = 1f)
        {
            if (effecterDef.maintainTicks > 0)
            {
                effecterDef.SpawnMaintained(A, B, scale);
            }
            else
            {
                effecterDef.Spawn(A, B, scale);
            }
        }
        public static void SpawnMaintainedIfPossible(this EffecterDef effecterDef, LocalTargetInfo A, LocalTargetInfo B, Map map, float scale = 1f)
        {
            effecterDef.SpawnMaintainedIfPossible(A.ToTargetInfo(map), B.ToTargetInfo(map), scale);
        }
    }
}
