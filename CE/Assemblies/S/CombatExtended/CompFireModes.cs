using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class CompFireModes : CompRangedGizmoGiver
{
	private Verb verbInt = null;

	private List<FireMode> availableFireModes = new List<FireMode>(Enum.GetNames(typeof(FireMode)).Length);

	private List<AimMode> availableAimModes = new List<AimMode>(Enum.GetNames(typeof(AimMode)).Length);

	private FireMode currentFireModeInt;

	private AimMode currentAimModeInt;

	private bool newComp = true;

	public TargettingMode targetMode = TargettingMode.torso;

	private bool IsTurretMannable = false;

	public CompProperties_FireModes Props => (CompProperties_FireModes)props;

	public List<AimMode> AvailableAimModes => availableAimModes;

	public List<FireMode> AvailableFireModes => availableFireModes;

	private Verb Verb
	{
		get
		{
			if (verbInt == null)
			{
				CompEquippable compEquippable = parent.TryGetComp<CompEquippable>();
				if (compEquippable != null)
				{
					verbInt = compEquippable.PrimaryVerb;
				}
				else
				{
					Log.ErrorOnce(parent.LabelCap + " has CompFireModes but no CompEquippable", 50020);
				}
			}
			return verbInt;
		}
	}

	public Thing Caster => Verb.caster;

	public Pawn CasterPawn => Caster as Pawn;

	public float HandLing
	{
		get
		{
			if (Caster is Pawn)
			{
				return CasterPawn.GetStatValue(StatDefOf.ShootingAccuracyPawn);
			}
			IsTurretMannable = Caster.TryGetComp<CompMannable>() != null;
			return 0f;
		}
	}

	public FireMode CurrentFireMode
	{
		get
		{
			return currentFireModeInt;
		}
		set
		{
			currentFireModeInt = value;
		}
	}

	public AimMode CurrentAimMode
	{
		get
		{
			return currentAimModeInt;
		}
		set
		{
			currentAimModeInt = value;
		}
	}

	public Texture2D TrueIcon
	{
		get
		{
			string text = "";
			switch (targetMode)
			{
			case TargettingMode.torso:
				text = "center";
				break;
			case TargettingMode.legs:
				text = "legs";
				break;
			case TargettingMode.head:
				text = "head";
				break;
			case TargettingMode.automatic:
				text = "auto";
				break;
			}
			return ContentFinder<Texture2D>.Get("UI/Buttons/Targetting/" + text);
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		LongEventHandler.ExecuteWhenFinished(InitAvailableFireModes);
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref currentFireModeInt, "currentFireMode", FireMode.AutoFire);
		Scribe_Values.Look(ref currentAimModeInt, "currentAimMode", AimMode.AimedShot);
		Scribe_Values.Look(ref targetMode, "currentTargettingMode", TargettingMode.torso);
		Scribe_Values.Look(ref newComp, "newComp", defaultValue: false);
	}

	public void InitAvailableFireModes()
	{
		availableFireModes.Clear();
		availableAimModes.Add(AimMode.AimedShot);
		if (parent.GetStatValue(CE_StatDefOf.BurstShotCount) > 1f || Props.noSingleShot)
		{
			availableFireModes.Add(FireMode.AutoFire);
		}
		if (Props.aimedBurstShotCount > 1)
		{
			if (Props.aimedBurstShotCount >= Verb.verbProps.burstShotCount)
			{
				Log.Warning(parent.LabelCap + " burst fire shot count is same or higher than auto fire");
			}
			else
			{
				availableFireModes.Add(FireMode.BurstFire);
			}
		}
		if (!Props.noSingleShot)
		{
			availableFireModes.Add(FireMode.SingleFire);
		}
		if (!Props.noSnapshot)
		{
			availableAimModes.Add(AimMode.Snapshot);
			availableAimModes.Add(AimMode.SuppressFire);
		}
		if (newComp || !availableFireModes.Contains(currentFireModeInt) || !availableAimModes.Contains(currentAimModeInt))
		{
			newComp = false;
			ResetModes();
		}
	}

	[Multiplayer.SyncMethod]
	public void ToggleFireMode()
	{
		int num = availableFireModes.IndexOf(currentFireModeInt);
		num = (num + 1) % availableFireModes.Count;
		currentFireModeInt = availableFireModes.ElementAt(num);
		if (availableFireModes.Count > 1)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_FireModes, KnowledgeAmount.Total);
		}
	}

	[Multiplayer.SyncMethod]
	public void ToggleAimMode()
	{
		int num = availableAimModes.IndexOf(currentAimModeInt);
		num = (num + 1) % availableAimModes.Count;
		currentAimModeInt = availableAimModes.ElementAt(num);
		if (availableAimModes.Count > 1)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_AimModes, KnowledgeAmount.Total);
		}
	}

	[Multiplayer.SyncMethod]
	public void ChangeTargetMode()
	{
		switch (targetMode)
		{
		case TargettingMode.torso:
			targetMode = TargettingMode.head;
			break;
		case TargettingMode.head:
			targetMode = TargettingMode.legs;
			break;
		case TargettingMode.legs:
			targetMode = TargettingMode.automatic;
			break;
		case TargettingMode.automatic:
			targetMode = TargettingMode.torso;
			break;
		}
	}

	public void ResetModes()
	{
		if (availableFireModes.Count > 0)
		{
			currentFireModeInt = availableFireModes.ElementAt(0);
		}
		currentAimModeInt = Props.aiAimMode;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (CasterPawn?.Faction != Faction.OfPlayer)
		{
			yield break;
		}
		foreach (Command item in GenerateGizmos())
		{
			yield return item;
		}
	}

	public IEnumerable<Command> GenerateGizmos()
	{
		Command_Action toggleFireModeGizmo = new Command_Action
		{
			action = ToggleFireMode,
			defaultLabel = ("CE_" + currentFireModeInt.ToString() + "Label").Translate(),
			defaultDesc = "CE_ToggleFireModeDesc".Translate(),
			icon = ContentFinder<Texture2D>.Get("UI/Buttons/" + currentFireModeInt),
			tutorTag = ((availableFireModes.Count > 1) ? "CE_FireModeToggle" : null)
		};
		if (availableFireModes.Count > 1)
		{
			toggleFireModeGizmo.tutorTag = "CE_FireModeToggle";
			LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_FireModes, parent, OpportunityType.GoodToKnow);
		}
		yield return toggleFireModeGizmo;
		Command_Action toggleAimModeGizmo = new Command_Action
		{
			action = ToggleAimMode,
			defaultLabel = ("CE_" + currentAimModeInt.ToString() + "Label").Translate(),
			defaultDesc = "CE_ToggleAimModeDesc".Translate(),
			icon = ContentFinder<Texture2D>.Get("UI/Buttons/" + currentAimModeInt)
		};
		if (availableAimModes.Count > 1)
		{
			toggleAimModeGizmo.tutorTag = "CE_AimModeToggle";
			LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_AimModes, parent, OpportunityType.GoodToKnow);
		}
		yield return toggleAimModeGizmo;
		if (CurrentAimMode != AimMode.SuppressFire && ((HandLing > 2.45f) | IsTurretMannable))
		{
			yield return new Command_Action
			{
				defaultLabel = "Targeted area: " + targetMode,
				defaultDesc = "",
				icon = TrueIcon,
				action = ChangeTargetMode
			};
		}
	}
}
