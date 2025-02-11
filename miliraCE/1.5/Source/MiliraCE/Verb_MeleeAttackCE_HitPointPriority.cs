using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    public class Verb_MeleeAttackCE_HitPointPriority : Verb_MeleeAttackCE
    {
        // Copied from RimWorld.Verb_MeleeAttack
        private bool IsTargetImmobile(LocalTargetInfo target)
        {
            Thing thing = target.Thing;
            Pawn pawn = thing as Pawn;
            if (thing.def.category == ThingCategory.Pawn && !pawn.Downed)
            {
                return pawn.GetPosture() != PawnPosture.Standing;
            }
            return true;
        }

        // Copied from AncotLibrary.Verb_MeleeAttackDamage_HitPointPriority
        private BodyPartRecord SelectBodyPartBasedOnHitPoints(LocalTargetInfo target)
        {
            if (!(target.Thing is Pawn pawn))
            {
                return null;
            }
            List<BodyPartRecord> source = pawn.health.hediffSet.GetNotMissingParts().ToList();
            var list = source.Select((BodyPartRecord part) => new
            {
                Part = part,
                Weight = part.coverageAbs * (1f + (float)part.def.hitPoints)
            }).ToList();
            float num = list.Sum(wp => wp.Weight);
            float num2 = Rand.Value * num;
            float num3 = 0f;
            foreach (var item in list)
            {
                num3 += item.Weight;
                if (num3 >= num2)
                {
                    return item.Part;
                }
            }
            return list.LastOrDefault()?.Part;
        }

        // Copied from CombatExtended.Verb_MeleeAttackCE, but with default body part given by SelectBodyPartBasedOnHitPoints
        protected override IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target, bool isCrit = false)
        {
            float damAmount = verbProps.AdjustedMeleeDamageAmount(this, CasterPawn);
            var critModifier = isCrit && verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp &&
                               !CasterPawn.def.race.Animal
                               ? 2
                               : 1;
            var armorPenetration = (verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp ? ArmorPenetrationSharp : ArmorPenetrationBlunt) * critModifier;
            DamageDef damDef = verbProps.meleeDamageDef;
            BodyPartGroupDef bodyPartGroupDef = null;
            HediffDef hediffDef = null;

            if (EquipmentSource != null && EquipmentSource != CasterPawn)
            {
                if (isCrit)
                {
                    damAmount *= StatWorker_MeleeDamage.GetDamageVariationMax(CasterPawn);
                }
                else
                {
                    damAmount *= Rand.Range(StatWorker_MeleeDamage.GetDamageVariationMin(CasterPawn), StatWorker_MeleeDamage.GetDamageVariationMax(CasterPawn));
                }
            }
            else if (!CE_StatDefOf.UnarmedDamage.Worker.IsDisabledFor(CasterPawn))
            {
                damAmount += CasterPawn.GetStatValue(CE_StatDefOf.UnarmedDamage);
            }

            if (CasterIsPawn)
            {
                bodyPartGroupDef = verbProps.AdjustedLinkedBodyPartsGroup(tool);
                if (damAmount >= 1f)
                {
                    if (HediffCompSource != null)
                    {
                        hediffDef = HediffCompSource.Def;
                    }
                }
                else
                {
                    damAmount = 1f;
                    damDef = DamageDefOf.Blunt;
                }
            }

            var source = EquipmentSource != null ? EquipmentSource.def : CasterPawn.def;
            Vector3 direction = (target.Thing.Position - CasterPawn.Position).ToVector3();
            DamageDef def = damDef;
            BodyPartHeight bodyRegion = GetAttackedPartHeightCE();
            var instigatorGuilty = !CasterPawn?.Drafted ?? true;
            DamageInfo damageInfo = new DamageInfo(def, damAmount, armorPenetration, -1f, caster, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);

            if (target.Thing is Pawn pawn)
            {
                if (caster.def.race.predator && IsTargetImmobile(target))
                {
                    var hp = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Top, BodyPartDepth.Outside).FirstOrDefault(r => r.def == CE_BodyPartDefOf.Neck);
                    if (hp == null)
                    {
                        hp = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Top, BodyPartDepth.Outside).FirstOrDefault(r => r.def == BodyPartDefOf.Head);
                    }
                    damageInfo.SetHitPart(hp);
                }
                if (pawn.health.hediffSet.GetNotMissingParts(bodyRegion).Count() <= 3)
                {
                    bodyRegion = BodyPartHeight.Middle;
                }
                if ((CompMeleeTargettingGizmo?.SkillReqBP ?? false) && CompMeleeTargettingGizmo.targetBodyPart != null)
                {
                    float targetSkillDecrease = (pawn.skills?.GetSkill(SkillDefOf.Melee).Level ?? 0f) / 50f;

                    if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) > 0f)
                    {
                        targetSkillDecrease *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                    }
                    else
                    {
                        targetSkillDecrease = 0f;
                    }

                    var partToHit = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == CompMeleeTargettingGizmo.targetBodyPart).FirstOrFallback();

                    if (Rand.Chance(CompMeleeTargettingGizmo.SkillBodyPartAttackChance(partToHit) - targetSkillDecrease))
                    {
                        damageInfo.SetHitPart(partToHit);
                    }
                }
            }
            // Milira's special default hit part picker
            if (damageInfo.HitPart == null)
            {
                damageInfo.SetHitPart(SelectBodyPartBasedOnHitPoints(target));
            }

            BodyPartDepth finalDepth = BodyPartDepth.Outside;
            if (target.Thing is Pawn p)
            {

                if (damageInfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp && this.ToolCE.capacities.Any(y => y.GetModExtension<ModExtensionMeleeToolPenetration>()?.canHitInternal ?? false))
                {
                    if (Rand.Chance(damageInfo.Def.stabChanceOfForcedInternal))
                    {
                        if (ToolCE.armorPenetrationSharp > p.GetStatValueForPawn(StatDefOf.ArmorRating_Sharp, p))
                        {
                            finalDepth = BodyPartDepth.Inside;

                            if (damageInfo.HitPart != null)
                            {
                                var children = damageInfo.HitPart.GetDirectChildParts();
                                if (children.Count() > 0)
                                {
                                    damageInfo.SetHitPart(children.RandomElementByWeight(x => x.coverage));
                                }
                            }
                        }
                    }
                }
            }

            damageInfo.SetBodyRegion(bodyRegion, finalDepth);
            damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
            damageInfo.SetWeaponHediff(hediffDef);
            damageInfo.SetAngle(direction);
            damageInfo.SetTool(tool);
            yield return damageInfo;
            if (this.tool != null && this.tool.extraMeleeDamages != null)
            {
                foreach (ExtraDamage extraDamage in this.tool.extraMeleeDamages)
                {
                    if (Rand.Chance(extraDamage.chance))
                    {
                        damAmount = extraDamage.amount;
                        damAmount = Rand.Range(damAmount * 0.8f, damAmount * 1.2f);
                        var extraDamageInfo = new DamageInfo(extraDamage.def, damAmount, extraDamage.AdjustedArmorPenetration(this, this.CasterPawn), -1f, this.caster, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
                        extraDamageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                        extraDamageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
                        extraDamageInfo.SetWeaponHediff(hediffDef);
                        extraDamageInfo.SetAngle(direction);

                        if (damageInfo.HitPart != null)
                        {
                            extraDamageInfo.SetHitPart(damageInfo.HitPart);
                        }

                        yield return extraDamageInfo;
                    }
                }
            }

            if (isCrit && !CasterPawn.def.race.Animal && verbProps.meleeDamageDef.armorCategory != DamageArmorCategoryDefOf.Sharp && target.Thing.def.race.IsFlesh)
            {
                var critAmount = GenMath.RoundRandom(damageInfo.Amount * 0.25f);
                var critDinfo = new DamageInfo(DamageDefOf.Stun, critAmount, armorPenetration,
                                               -1, caster, null, source, instigatorGuilty: instigatorGuilty);
                critDinfo.SetBodyRegion(bodyRegion, BodyPartDepth.Outside);
                critDinfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
                critDinfo.SetWeaponHediff(hediffDef);
                critDinfo.SetAngle(direction);
                yield return critDinfo;
            }
        }
    }
}
