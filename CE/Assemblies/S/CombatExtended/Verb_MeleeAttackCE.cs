using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

public class Verb_MeleeAttackCE : Verb_MeleeAttack
{
	private const float DefaultHitChance = 0.6f;

	private const float ShieldBlockChance = 0.75f;

	private const int KnockdownDuration = 120;

	private const float KnockdownMassRequirement = 5f;

	private const float HitXP = 200f;

	private const float DodgeXP = 50f;

	private const float ParryXP = 50f;

	private const float CritXP = 100f;

	private const float BaseCritChance = 0.1f;

	private const float BaseDodgeChance = 0.1f;

	private const float BaseParryChance = 0.2f;

	protected bool isCrit;

	private CompMeleeTargettingGizmo compMeleeTargettingGizmo;

	private bool meleeTargettingInitialized;

	public ToolCE ToolCE => tool as ToolCE;

	public static Verb_MeleeAttackCE LastAttackVerb { get; protected set; }

	protected virtual float PenetrationSkillMultiplier => 1f + (float)(CasterPawn?.skills?.GetSkill(SkillDefOf.Melee).Level).GetValueOrDefault() * (1f / 76f);

	protected virtual float PenetrationOtherMultipliers => CasterIsPawn ? Mathf.Pow(verbProps.GetDamageFactorFor(this, CasterPawn), 0.75f) : 1f;

	public float ArmorPenetrationSharp => ((tool as ToolCE)?.armorPenetrationSharp * PenetrationOtherMultipliers * PenetrationSkillMultiplier * (base.EquipmentSource?.GetStatValue(CE_StatDefOf.MeleePenetrationFactor) ?? 1f)).GetValueOrDefault();

	public float ArmorPenetrationBlunt => ((tool as ToolCE)?.armorPenetrationBlunt * PenetrationOtherMultipliers * PenetrationSkillMultiplier * (base.EquipmentSource?.GetStatValue(CE_StatDefOf.MeleePenetrationFactor) ?? 1f)).GetValueOrDefault();

	public bool Enabled
	{
		get
		{
			if (ToolCE?.requiredAttachment != null && CasterIsPawn && CasterPawn?.equipment?.Primary is WeaponPlatform { CurLinks: var curLinks })
			{
				for (int i = 0; i < curLinks.Length; i++)
				{
					if (curLinks[i].attachment == ToolCE.requiredAttachment)
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
	}

	public CompMeleeTargettingGizmo CompMeleeTargettingGizmo
	{
		get
		{
			if (!meleeTargettingInitialized)
			{
				meleeTargettingInitialized = true;
				if (CasterIsPawn && CasterPawn.MentalStateDef != MentalStateDefOf.SocialFighting)
				{
					compMeleeTargettingGizmo = CasterPawn.TryGetComp<CompMeleeTargettingGizmo>();
				}
			}
			return compMeleeTargettingGizmo;
		}
	}

	public override bool Available()
	{
		return Enabled && base.Available();
	}

	public override bool IsUsableOn(Thing target)
	{
		return Enabled && base.IsUsableOn(target);
	}

	public override void WarmupComplete()
	{
		meleeTargettingInitialized = false;
		base.WarmupComplete();
	}

	public override bool TryCastShot()
	{
		Pawn casterPawn = CasterPawn;
		if (casterPawn.stances.FullBodyBusy)
		{
			return false;
		}
		Thing thing = currentTarget.Thing;
		if (!CanHitTarget(thing))
		{
			Log.Warning(string.Concat(casterPawn, " meleed ", thing, " from out of melee position."));
		}
		casterPawn.rotationTracker.Face(thing.DrawPos);
		bool flag = IsTargetImmobile(currentTarget);
		if (!flag && casterPawn.skills != null)
		{
			casterPawn.skills.Learn(SkillDefOf.Melee, 200f * verbProps.AdjustedFullCycleTime(this, casterPawn));
		}
		TargetInfo targetInfo = new TargetInfo(thing.PositionHeld, thing.MapHeld);
		string text = "";
		Pawn pawn = thing as Pawn;
		bool result;
		SoundDef soundDef;
		if (Rand.Chance(GetHitChance(thing)))
		{
			if (!flag && !surpriseAttack && Rand.Chance(pawn.GetStatValue(StatDefOf.MeleeDodgeChance)))
			{
				result = false;
				soundDef = SoundMiss();
				CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesDodge, alwaysShow: false);
				text = "TextMote_Dodge".Translate();
				pawn.skills?.Learn(SkillDefOf.Melee, 50f * verbProps.AdjustedFullCycleTime(this, casterPawn));
			}
			else
			{
				float defenderSkillMult = 1f + (base.EquipmentSource?.GetStatValue(CE_StatDefOf.MeleeCounterParryBonus) ?? 0f);
				float comparativeChanceAgainst = GetComparativeChanceAgainst(pawn, casterPawn, CE_StatDefOf.MeleeParryChance, 0.2f, defenderSkillMult);
				if (!surpriseAttack && pawn != null && CanDoParry(pawn) && Rand.Chance(comparativeChanceAgainst))
				{
					float comparativeChanceAgainst2 = GetComparativeChanceAgainst(pawn, casterPawn, CE_StatDefOf.MeleeCritChance, 0.1f);
					Apparel apparel = pawn.apparel.WornApparel.FirstOrDefault((Apparel x) => x is Apparel_Shield);
					Thing parryThing = ((apparel != null && Rand.Chance(0.75f)) ? apparel : (pawn.equipment?.Primary ?? pawn));
					bool flag2 = Rand.Chance(comparativeChanceAgainst2);
					if (Rand.Chance(comparativeChanceAgainst2))
					{
						DoParry(pawn, parryThing, isRiposte: true, flag2);
						text = "CE_TextMote_Riposted".Translate();
						CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesDeflect, alwaysShow: false);
						pawn.skills?.Learn(SkillDefOf.Melee, 150f * verbProps.AdjustedFullCycleTime(this, casterPawn));
					}
					else
					{
						DoParry(pawn, parryThing, isRiposte: false, flag2);
						text = "CE_TextMote_Blocked".Translate();
						if (flag2)
						{
							text = "CE_TextMote_Parried".Translate();
						}
						CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesMiss, alwaysShow: false);
						pawn.skills?.Learn(SkillDefOf.Melee, 50f * verbProps.AdjustedFullCycleTime(this, casterPawn));
					}
					result = false;
					soundDef = SoundMiss();
				}
				else
				{
					BattleLogEntry_MeleeCombat log = CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesHit, alwaysShow: false);
					DamageWorker.DamageResult damageResult;
					if (surpriseAttack || Rand.Chance(GetComparativeChanceAgainst(casterPawn, pawn, CE_StatDefOf.MeleeCritChance, 0.1f)))
					{
						isCrit = true;
						damageResult = ApplyMeleeDamageToTarget(currentTarget);
						text = (casterPawn.def.race.Animal ? ((TaggedString)null) : "CE_TextMote_CriticalHit".Translate());
						casterPawn.skills?.Learn(SkillDefOf.Melee, 100f * verbProps.AdjustedFullCycleTime(this, casterPawn));
					}
					else
					{
						damageResult = ApplyMeleeDamageToTarget(currentTarget);
					}
					damageResult.AssociateWithLog(log);
					if (pawn != null && damageResult.totalDamageDealt > 0f)
					{
						ApplyMeleeSlaveSuppression(pawn, damageResult.totalDamageDealt);
					}
					result = true;
					soundDef = ((thing.def.category == ThingCategory.Building) ? SoundHitBuilding() : SoundHitPawn());
				}
			}
		}
		else
		{
			result = false;
			soundDef = SoundMiss();
			CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesMiss, alwaysShow: false);
		}
		if (!text.NullOrEmpty())
		{
			MoteMakerCE.ThrowText(thing.PositionHeld.ToVector3Shifted(), thing.MapHeld, text);
		}
		if (targetInfo.Map != null)
		{
			soundDef.PlayOneShot(targetInfo);
		}
		if (casterPawn.Spawned)
		{
			casterPawn.Drawer.Notify_MeleeAttackOn(thing);
		}
		if (pawn != null && !pawn.Dead && pawn.Spawned)
		{
			pawn.stances.stagger.StaggerFor(95);
			if (casterPawn.MentalStateDef != MentalStateDefOf.SocialFighting || pawn.MentalStateDef != MentalStateDefOf.SocialFighting)
			{
				pawn.mindState.meleeThreat = casterPawn;
				pawn.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
			}
		}
		casterPawn.rotationTracker?.FaceCell(thing.Position);
		casterPawn.caller?.Notify_DidMeleeAttack();
		return result;
	}

	public BodyPartHeight GetAttackedPartHeightCE()
	{
		BodyPartHeight result = BodyPartHeight.Undefined;
		if (CompMeleeTargettingGizmo != null && base.CurrentTarget.Thing is Pawn target)
		{
			return CompMeleeTargettingGizmo.finalHeight(target);
		}
		return result;
	}

	protected virtual IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target, bool isCrit = false)
	{
		float damAmount = verbProps.AdjustedMeleeDamageAmount(this, CasterPawn);
		int critModifier = ((!isCrit || verbProps.meleeDamageDef.armorCategory != DamageArmorCategoryDefOf.Sharp || CasterPawn.def.race.Animal) ? 1 : 2);
		float armorPenetration = ((verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp) ? ArmorPenetrationSharp : ArmorPenetrationBlunt) * (float)critModifier;
		DamageDef damDef = verbProps.meleeDamageDef;
		BodyPartGroupDef bodyPartGroupDef = null;
		HediffDef hediffDef = null;
		if (base.EquipmentSource != null && base.EquipmentSource != CasterPawn)
		{
			damAmount = ((!isCrit) ? (damAmount * Rand.Range(StatWorker_MeleeDamageBase.GetDamageVariationMin(CasterPawn), StatWorker_MeleeDamageBase.GetDamageVariationMax(CasterPawn))) : (damAmount * StatWorker_MeleeDamageBase.GetDamageVariationMax(CasterPawn)));
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
				if (base.HediffCompSource != null)
				{
					hediffDef = base.HediffCompSource.Def;
				}
			}
			else
			{
				damAmount = 1f;
				damDef = DamageDefOf.Blunt;
			}
		}
		ThingDef source = ((base.EquipmentSource != null) ? base.EquipmentSource.def : CasterPawn.def);
		Vector3 direction = (target.Thing.Position - CasterPawn.Position).ToVector3();
		DamageDef def = damDef;
		BodyPartHeight bodyRegion = GetAttackedPartHeightCE();
		Pawn casterPawn = CasterPawn;
		bool instigatorGuilty = casterPawn == null || !casterPawn.Drafted;
		DamageInfo damageInfo = new DamageInfo(def, damAmount, armorPenetration, -1f, caster, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
		Thing thing = target.Thing;
		if (thing is Pawn pawn)
		{
			if (caster.def.race.predator && IsTargetImmobile(target))
			{
				BodyPartRecord hp = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Top, BodyPartDepth.Outside).FirstOrDefault((BodyPartRecord r) => r.def == CE_BodyPartDefOf.Neck);
				if (hp == null)
				{
					hp = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Top, BodyPartDepth.Outside).FirstOrDefault((BodyPartRecord r) => r.def == BodyPartDefOf.Head);
				}
				damageInfo.SetHitPart(hp);
			}
			if (pawn.health.hediffSet.GetNotMissingParts(bodyRegion).Count() <= 3)
			{
				bodyRegion = BodyPartHeight.Middle;
			}
			if ((CompMeleeTargettingGizmo?.SkillReqBP ?? false) && CompMeleeTargettingGizmo.targetBodyPart != null)
			{
				float targetSkillDecrease = (((float?)pawn.skills?.GetSkill(SkillDefOf.Melee).Level) ?? 0f) / 50f;
				targetSkillDecrease = ((!(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) > 0f)) ? 0f : (targetSkillDecrease * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving)));
				BodyPartRecord partToHit = (from x in pawn.health.hediffSet.GetNotMissingParts()
					where x.def == CompMeleeTargettingGizmo.targetBodyPart
					select x).FirstOrFallback();
				if (Rand.Chance(CompMeleeTargettingGizmo.SkillBodyPartAttackChance(partToHit) - targetSkillDecrease))
				{
					damageInfo.SetHitPart(partToHit);
				}
			}
		}
		BodyPartDepth finalDepth = BodyPartDepth.Outside;
		thing = target.Thing;
		if (thing is Pawn p && damageInfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp && ToolCE.capacities.Any((ToolCapacityDef y) => y.GetModExtension<ModExtensionMeleeToolPenetration>()?.canHitInternal ?? false) && Rand.Chance(damageInfo.Def.stabChanceOfForcedInternal) && ToolCE.armorPenetrationSharp > p.GetStatValueForPawn(StatDefOf.ArmorRating_Sharp, p))
		{
			finalDepth = BodyPartDepth.Inside;
			if (damageInfo.HitPart != null)
			{
				IEnumerable<BodyPartRecord> children = damageInfo.HitPart.GetDirectChildParts();
				if (children.Count() > 0)
				{
					damageInfo.SetHitPart(children.RandomElementByWeight((BodyPartRecord x) => x.coverage));
				}
			}
		}
		damageInfo.SetBodyRegion(bodyRegion, finalDepth);
		damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
		damageInfo.SetWeaponHediff(hediffDef);
		damageInfo.SetAngle(direction);
		damageInfo.SetTool(tool);
		yield return damageInfo;
		if (tool != null && tool.extraMeleeDamages != null)
		{
			foreach (ExtraDamage extraDamage in tool.extraMeleeDamages)
			{
				if (Rand.Chance(extraDamage.chance))
				{
					damAmount = extraDamage.amount;
					DamageInfo extraDamageInfo = new DamageInfo(amount: Rand.Range(damAmount * 0.8f, damAmount * 1.2f), def: extraDamage.def, armorPenetration: extraDamage.AdjustedArmorPenetration(this, CasterPawn), angle: -1f, instigator: caster, hitPart: null, weapon: source, category: DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget: null, instigatorGuilty: instigatorGuilty);
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
			DamageInfo critDinfo = new DamageInfo(amount: GenMath.RoundRandom(damageInfo.Amount * 0.25f), def: DamageDefOf.Stun, armorPenetration: armorPenetration, angle: -1f, instigator: caster, hitPart: null, weapon: source, category: DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget: null, instigatorGuilty: instigatorGuilty);
			critDinfo.SetBodyRegion(bodyRegion, BodyPartDepth.Outside);
			critDinfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
			critDinfo.SetWeaponHediff(hediffDef);
			critDinfo.SetAngle(direction);
			yield return critDinfo;
		}
	}

	private float GetHitChance(LocalTargetInfo target)
	{
		if (surpriseAttack)
		{
			return 1f;
		}
		if (IsTargetImmobile(target))
		{
			return 1f;
		}
		if (CasterPawn.skills != null)
		{
			float num = CasterPawn.GetStatValue(StatDefOf.MeleeHitChance);
			switch (GetAttackedPartHeightCE())
			{
			case BodyPartHeight.Bottom:
				num *= 0.8f;
				break;
			case BodyPartHeight.Top:
				num *= 0.7f;
				break;
			}
			return num;
		}
		return 0.6f;
	}

	public override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
	{
		DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
		IEnumerable<DamageInfo> enumerable = DamageInfosToApply(target, isCrit);
		bool flag = false;
		float num = 0f;
		foreach (DamageInfo item in enumerable)
		{
			if (target.ThingDestroyed)
			{
				break;
			}
			if (item.Height == BodyPartHeight.Top)
			{
				flag = true;
			}
			LastAttackVerb = this;
			damageResult = target.Thing.TakeDamage(item);
			num += damageResult.totalDamageDealt;
			LastAttackVerb = null;
		}
		if (isCrit && CasterPawn.def.race.Animal)
		{
			Pawn pawn = target.Thing as Pawn;
			float num2 = pawn.GetStatValue(StatDefOf.Mass);
			RacePropertiesExtensionCE modExtension = pawn.def.GetModExtension<RacePropertiesExtensionCE>();
			if (modExtension != null)
			{
				num2 *= modExtension.bodyShape.width / modExtension.bodyShape.height;
			}
			if (flag)
			{
				num2 *= 0.5f;
			}
			if (pawn != null && !pawn.Dead && Rand.Chance(CasterPawn.GetStatValue(StatDefOf.Mass) / num2 - 0.25f))
			{
				MoteMakerCE.ThrowText(pawn.PositionHeld.ToVector3Shifted(), pawn.MapHeld, "CE_TextMote_Knockdown".Translate());
				pawn.stances?.SetStance(new Stance_Cooldown(120, pawn, null));
				Job job = JobMaker.MakeJob(CE_JobDefOf.WaitKnockdown);
				job.expiryInterval = 120;
				pawn.jobs?.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: false, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			}
		}
		isCrit = false;
		damageResult.totalDamageDealt = num;
		return damageResult;
	}

	protected virtual bool CanDoParry(Pawn pawn)
	{
		if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike || pawn.WorkTagIsDisabled(WorkTags.Violent) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || IsTargetImmobile((LocalTargetInfo)pawn) || pawn.MentalStateDef == MentalStateDefOf.SocialFighting)
		{
			return false;
		}
		ParryTracker component = pawn.Map.GetComponent<ParryTracker>();
		if (component == null)
		{
			Log.Error("CE failed to find ParryTracker to check pawn " + pawn.ToString());
			return true;
		}
		return component.CheckCanParry(pawn);
	}

	protected virtual void DoParry(Pawn defender, Thing parryThing, bool isRiposte = false, bool deflected = false)
	{
		if (parryThing != null)
		{
			foreach (DamageInfo item in DamageInfosToApply(defender))
			{
				if (!deflected)
				{
					LastAttackVerb = this;
					ArmorUtilityCE.ApplyParryDamage(item, parryThing);
					LastAttackVerb = null;
				}
			}
		}
		if (isRiposte)
		{
			SoundDef soundDef = null;
			if (parryThing is Apparel_Shield)
			{
				bool instigatorGuilty = !defender.Drafted;
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, 6f, parryThing.GetStatValue(CE_StatDefOf.MeleePenetrationFactor), -1f, defender, null, parryThing.def, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
				dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
				dinfo.SetAngle((CasterPawn.Position - defender.Position).ToVector3());
				caster.TakeDamage(dinfo);
				if (!parryThing.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
				{
					soundDef = parryThing.Stuff.stuffProps.soundMeleeHitBlunt;
				}
			}
			else if (!(defender.meleeVerbs.TryGetMeleeVerb(caster) is Verb_MeleeAttackCE verb_MeleeAttackCE))
			{
				Log.Error("CE failed to get attack verb for riposte from Pawn " + defender.ToString());
			}
			else
			{
				verb_MeleeAttackCE.ApplyMeleeDamageToTarget(caster);
				soundDef = ((Verb_MeleeAttack)verb_MeleeAttackCE).SoundHitPawn();
			}
			soundDef?.PlayOneShot(new TargetInfo(caster.PositionHeld, caster.MapHeld));
		}
		ParryTracker parryTracker = defender.MapHeld?.GetComponent<ParryTracker>();
		if (parryTracker == null)
		{
			Log.Error("CE failed to find ParryTracker to register pawn " + defender.ToString());
		}
		else
		{
			parryTracker.RegisterParryFor(defender, verbProps.AdjustedCooldownTicks(this, defender));
		}
	}

	protected static float GetComparativeChanceAgainst(Pawn attacker, Pawn defender, StatDef stat, float baseChance, float defenderSkillMult = 1f)
	{
		if (attacker == null || defender == null)
		{
			return 0f;
		}
		float num = (stat.Worker.IsDisabledFor(attacker) ? 0f : attacker.GetStatValue(stat));
		float num2 = (stat.Worker.IsDisabledFor(defender) ? 0f : (defender.GetStatValue(stat) * defenderSkillMult));
		return Mathf.Clamp01(baseChance + num - num2);
	}
}
