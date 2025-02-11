using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AncotLibrary;

public class CompTurretGun_Custom : ThingComp, IAttackTargetSearcher
{
	private const int StartShootIntervalTicks = 10;

	private static readonly CachedTexture ToggleTurretIcon = new CachedTexture("UI/Gizmos/ToggleTurret");

	public Thing gun;

	protected int burstCooldownTicksLeft;

	protected int burstWarmupTicksLeft;

	public LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;

	private bool fireAtWill = true;

	private LocalTargetInfo lastAttackedTarget = LocalTargetInfo.Invalid;

	private int lastAttackTargetTick;

	public float curRotation;

	public float floatOffset_xAxis = 0f;

	public float floatOffset_yAxis = 0f;

	public float randTime = Rand.Range(0f, 300f);

	public Thing Thing => PawnOwner;

	public CompProperties_TurretGun_Custom Props => (CompProperties_TurretGun_Custom)props;

	public Verb CurrentEffectiveVerb => AttackVerb;

	public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

	public int LastAttackTargetTick => lastAttackTargetTick;

	public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

	public Verb AttackVerb => GunCompEq.PrimaryVerb;

	private bool WarmingUp => burstWarmupTicksLeft > 0;

	private bool IsApparel => parent is Apparel;

	private bool IsBuiltIn => !IsApparel;

	public Pawn PawnOwner
	{
		get
		{
			if (!(parent is Apparel { Wearer: var wearer }))
			{
				if (parent is Pawn result)
				{
					return result;
				}
				return null;
			}
			return wearer;
		}
	}

	private CompMechAutoFight compMechAutoFight => PawnOwner.TryGetComp<CompMechAutoFight>();

	private bool isAutoFight
	{
		get
		{
			if (compMechAutoFight == null)
			{
				return false;
			}
			return compMechAutoFight.autoFight;
		}
	}

	public bool CanShoot
	{
		get
		{
			if (PawnOwner != null)
			{
				if (!PawnOwner.Spawned || PawnOwner.Downed || PawnOwner.Dead || !PawnOwner.Awake())
				{
					return false;
				}
				if (!Props.attackUndrafted && PawnOwner.IsPlayerControlled && !PawnOwner.Drafted)
				{
					if (isAutoFight)
					{
						return fireAtWill;
					}
					return false;
				}
				if (PawnOwner.stances.stunner.Stunned)
				{
					return false;
				}
				if (TurretDestroyed)
				{
					return false;
				}
				if (!fireAtWill)
				{
					return false;
				}
				CompCanBeDormant compCanBeDormant = PawnOwner.TryGetComp<CompCanBeDormant>();
				if (compCanBeDormant != null && !compCanBeDormant.Awake)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool TurretDestroyed
	{
		get
		{
			if (IsBuiltIn && AttackVerb.verbProps.linkedBodyPartsGroup != null && AttackVerb.verbProps.ensureLinkedBodyPartsGroupAlwaysUsable && PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(PawnOwner.health.hediffSet, AttackVerb.verbProps.linkedBodyPartsGroup) <= 0f)
			{
				return true;
			}
			return false;
		}
	}

	public bool AutoAttack => Props.autoAttack;

	public override void PostPostMake()
	{
		base.PostPostMake();
		if (IsBuiltIn)
		{
			MakeGun();
		}
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		base.PostPostMake();
		if (IsApparel)
		{
			MakeGun();
		}
	}

	private void MakeGun()
	{
		gun = ThingMaker.MakeThing(Props.turretDef);
		UpdateGunVerbs();
	}

	private void UpdateGunVerbs()
	{
		List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			Verb verb = allVerbs[i];
			verb.caster = PawnOwner;
			verb.castCompleteCallback = delegate
			{
				burstCooldownTicksLeft = AttackVerb.verbProps.defaultCooldownTime.SecondsToTicks();
			};
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!CanShoot)
		{
			return;
		}
		if (Props.float_xAxis != null)
		{
			floatOffset_xAxis = Mathf.Sin(((float)Find.TickManager.TicksGame + randTime) * Props.float_xAxis.floatSpeed) * Props.float_xAxis.floatAmplitude;
		}
		if (Props.float_yAxis != null)
		{
			floatOffset_yAxis = Mathf.Sin(((float)Find.TickManager.TicksGame + randTime) * Props.float_yAxis.floatSpeed) * Props.float_yAxis.floatAmplitude;
		}
		if (currentTarget.IsValid)
		{
			curRotation = (currentTarget.Cell.ToVector3Shifted() - PawnOwner.DrawPos).AngleFlat() + Props.angleOffset;
		}
		AttackVerb.VerbTick();
		if (AttackVerb.state == VerbState.Bursting)
		{
			return;
		}
		if (WarmingUp)
		{
			burstWarmupTicksLeft--;
			if (burstWarmupTicksLeft == 0)
			{
				AttackVerb.TryStartCastOn(currentTarget, surpriseAttack: false, canHitNonTargetPawns: true, preventFriendlyFire: false, nonInterruptingSelfCast: true);
				lastAttackTargetTick = Find.TickManager.TicksGame;
				lastAttackedTarget = currentTarget;
			}
			return;
		}
		if (burstCooldownTicksLeft > 0)
		{
			burstCooldownTicksLeft--;
		}
		if (burstCooldownTicksLeft <= 0 && PawnOwner.IsHashIntervalTick(10))
		{
			currentTarget = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable);
			if (currentTarget.IsValid)
			{
				burstWarmupTicksLeft = 1;
			}
			else
			{
				ResetCurrentTarget();
			}
		}
	}

	private void ResetCurrentTarget()
	{
		currentTarget = LocalTargetInfo.Invalid;
		burstWarmupTicksLeft = 0;
	}

	public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetWornGizmosExtra())
		{
			yield return item;
		}
		if (!IsApparel)
		{
			yield break;
		}
		foreach (Gizmo gizmo in GetGizmos())
		{
			yield return gizmo;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (!IsBuiltIn)
		{
			yield break;
		}
		foreach (Gizmo gizmo in GetGizmos())
		{
			yield return gizmo;
		}
	}

	private IEnumerable<Gizmo> GetGizmos()
	{
		if (PawnOwner.Faction == Faction.OfPlayer && (PawnOwner.Drafted || isAutoFight))
		{
			yield return new Command_Toggle
			{
				defaultLabel = "CommandToggleTurret".Translate(),
				defaultDesc = "CommandToggleTurretDesc".Translate(),
				isActive = () => fireAtWill,
				icon = ToggleTurretIcon.Texture,
				toggleAction = delegate
				{
					fireAtWill = !fireAtWill;
					PawnOwner.Drawer.renderer.renderTree.SetDirty();
				}
			};
		}
	}

	public override List<PawnRenderNode> CompRenderNodes()
	{
		if (!Props.renderNodeProperties.NullOrEmpty() && PawnOwner != null)
		{
			List<PawnRenderNode> list = new List<PawnRenderNode>();
			foreach (PawnRenderNodeProperties renderNodeProperty in Props.renderNodeProperties)
			{
				PawnRenderNode_TurretGun_Custom pawnRenderNode_TurretGun_Custom = new PawnRenderNode_TurretGun_Custom(PawnOwner, renderNodeProperty, PawnOwner.Drawer.renderer.renderTree);
				pawnRenderNode_TurretGun_Custom.turretComp = this;
				if (IsApparel)
				{
					pawnRenderNode_TurretGun_Custom.apparel = (Apparel)parent;
				}
				list.Add(pawnRenderNode_TurretGun_Custom);
			}
			return list;
		}
		return base.CompRenderNodes();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (Props.turretDef != null)
		{
			if (IsApparel)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, "Ancot.Turret".Translate(), Props.turretDef.LabelCap, "Ancot.TurretDescI".Translate(), 5600, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(Props.turretDef)));
			}
			if (IsBuiltIn)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, "Ancot.Turret".Translate(), Props.turretDef.LabelCap, "Ancot.TurretDescII".Translate(), 5600, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(Props.turretDef)));
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
		Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
		Scribe_Values.Look(ref floatOffset_xAxis, "floatOffset_xAxis", 0f);
		Scribe_Values.Look(ref floatOffset_yAxis, "floatOffset_yAxis", 0f);
		Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
		Scribe_Deep.Look(ref gun, "gun");
		Scribe_Values.Look(ref fireAtWill, "fireAtWill", defaultValue: true);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (gun == null && PawnOwner != null)
			{
				Log.Error("CompTurrentGun had null gun after loading. Recreating.");
				MakeGun();
			}
			else
			{
				UpdateGunVerbs();
			}
		}
	}
}
