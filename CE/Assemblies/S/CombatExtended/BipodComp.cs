using System.Collections.Generic;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

public class BipodComp : CompRangedGizmoGiver
{
	public bool ShouldSetUpint = false;

	public bool IsSetUpRn;

	private int starts = 5;

	public bool ShouldSetUp
	{
		get
		{
			bool flag = false;
			if (Controller.settings.AutoSetUp)
			{
				CompFireModes compFireModes = parent.TryGetComp<CompFireModes>();
				Pawn pawn = ((Pawn_EquipmentTracker)base.ParentHolder).pawn;
				return ((compFireModes.CurrentAimMode == Props.catDef.autosetMode) | (!Props.catDef.useAutoSetMode && compFireModes.CurrentAimMode != AimMode.Snapshot)) && !IsSetUpRn && !pawn.IsCarryingPawn();
			}
			return ShouldSetUpint && !IsSetUpRn;
		}
		set
		{
		}
	}

	public CompProperties_BipodComp Props => (CompProperties_BipodComp)props;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref IsSetUpRn, "isBipodSetUp", defaultValue: false);
		base.PostExposeData();
	}

	[Multiplayer.SyncMethod]
	public void DeployUpBipod()
	{
		ShouldSetUpint = true;
	}

	[Multiplayer.SyncMethod]
	public void CloseBipod()
	{
		ShouldSetUpint = false;
		IsSetUpRn = false;
		SetUpInvert(parent);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Controller.settings.BipodMechanics)
		{
			yield break;
		}
		if (Controller.settings.AutoSetUp)
		{
			if (!(base.ParentHolder is Pawn_EquipmentTracker))
			{
				yield break;
			}
			Pawn dad = ((Pawn_EquipmentTracker)base.ParentHolder).pawn;
			if (dad.Drafted)
			{
				if (IsSetUpRn)
				{
					yield return new Command_Action
					{
						action = delegate
						{
						},
						defaultLabel = "CE_Bipod_Set_Up".Translate(),
						icon = ContentFinder<Texture2D>.Get("UI/Buttons/open_bipod")
					};
				}
				else
				{
					yield return new Command_Action
					{
						action = delegate
						{
						},
						defaultLabel = "CE_Bipod_Not_Set_Up".Translate(),
						icon = ContentFinder<Texture2D>.Get("UI/Buttons/closed_bipod")
					};
				}
			}
		}
		else
		{
			if (!(base.ParentHolder is Pawn_EquipmentTracker))
			{
				yield break;
			}
			Pawn dad2 = ((Pawn_EquipmentTracker)base.ParentHolder).pawn;
			if (dad2.Drafted)
			{
				if (!ShouldSetUpint)
				{
					yield return new Command_Action
					{
						action = DeployUpBipod,
						defaultLabel = "CE_Deploy_Bipod".Translate(),
						icon = ContentFinder<Texture2D>.Get("UI/Buttons/open_bipod")
					};
				}
				else
				{
					yield return new Command_Action
					{
						action = CloseBipod,
						defaultLabel = "CE_Close_Bipod".Translate(),
						icon = ContentFinder<Texture2D>.Get("UI/Buttons/closed_bipod")
					};
				}
			}
		}
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		IsSetUpRn = false;
		ResetVerbProps(parent);
	}

	public VerbPropertiesCE CopyVerbPropsFromThing(Thing source)
	{
		return (VerbPropertiesCE)source.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.MemberwiseClone();
	}

	public void AssignVerbProps(Thing target, VerbPropertiesCE props)
	{
		target.TryGetComp<CompEquippable>().PrimaryVerb.verbProps = props;
	}

	public void ResetVerbProps(Thing source)
	{
		VerbPropertiesCE verbPropertiesCE = (VerbPropertiesCE)source.def.verbs.Find((VerbProperties x) => x is VerbPropertiesCE).MemberwiseClone();
		AssignVerbProps(source, verbPropertiesCE);
	}

	public void SetUpInvert(Thing source)
	{
		ResetVerbProps(source);
		IsSetUpRn = false;
		VerbPropertiesCE verbPropertiesCE = CopyVerbPropsFromThing(source);
		verbPropertiesCE.recoilAmount *= Props.recoilMultoff;
		verbPropertiesCE.warmupTime *= Props.warmupPenalty;
		AssignVerbProps(source, verbPropertiesCE);
	}

	public void SetUpEnd(Thing source)
	{
		VerbPropertiesCE verbPropertiesCE = CopyVerbPropsFromThing(source);
		verbPropertiesCE.range += Props.additionalrange;
		verbPropertiesCE.recoilAmount *= Props.recoilMulton;
		verbPropertiesCE.warmupTime *= Props.warmupMult;
		IsSetUpRn = true;
		AssignVerbProps(source, verbPropertiesCE);
		CE_SoundDefOf.Interact_Bipod.PlayOneShot(new TargetInfo(source.PositionHeld, source.Map));
	}

	public void SetUpStart(Pawn pawn = null)
	{
		if (pawn == null || !pawn.Drafted)
		{
			return;
		}
		starts--;
		if (starts == 0)
		{
			if (pawn != null && pawn.jobs?.curJob?.def?.HasModExtension<JobDefBipodCancelExtension>() == true && !pawn.pather.MovingNow && ShouldSetUp)
			{
				pawn.jobs.StopAll();
				pawn.jobs.StartJob(new Job
				{
					def = CE_JobDefOf.JobDef_SetUpBipod,
					targetA = parent
				}, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			}
			if (pawn.pather.MovingNow)
			{
				SetUpInvert(parent);
			}
			starts = 5;
		}
	}
}
