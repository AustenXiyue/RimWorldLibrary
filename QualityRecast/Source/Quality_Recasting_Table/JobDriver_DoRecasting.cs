using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Quality_Recasting_Table;

public class JobDriver_DoRecasting : JobDriver
{
	public float A;

	public float B;

	public float C;

	public float D;

	public float E;

	public float F;

	public float G;

	private Mote warmupMote;

	public float a1 = 0.05f;

	public float b1 = 0.15f;

	public float c1 = 0.25f;

	public float d1 = 0.25f;

	public float e1 = 0.15f;

	public float f1 = 0.1f;

	public float g1 = 0.05f;

	public float a2 = 0.05f;

	public float b2 = 0.1f;

	public float c2 = 0.25f;

	public float d2 = 0.2f;

	public float e2 = 0.15f;

	public float f2 = 0.15f;

	public float g2 = 0.1f;

	public float a3 = 0.05f;

	public float b3 = 0.05f;

	public float c3 = 0.15f;

	public float d3 = 0.15f;

	public float e3 = 0.25f;

	public float f3 = 0.2f;

	public float g3 = 0.15f;

	public Thing ReforgedItem => job.GetTarget(TargetIndex.A).Thing;

	public Building RecastingTable
	{
		get
		{
			Thing thing = job.GetTarget(TargetIndex.B).Thing;
			if (thing is Building result)
			{
				return result;
			}
			return null;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(ReforgedItem, job, 1, -1, null, errorOnFailed) && pawn.Reserve(RecastingTable, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		CompRefuelable compRefuelable = RecastingTable.TryGetComp<CompRefuelable>();
		float currentFuel = compRefuelable.Fuel;
		if (currentFuel < Quality_Recasting_Table_Main.Instance.Quality_Recasting_Table_Setting.fuelspent)
		{
			Messages.Message("Low_fuel".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.CautionInput);
			yield break;
		}
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B);
		Toil toil = Toils_General.Wait(240);
		toil.WithProgressBarToilDelay(TargetIndex.B);
		toil.FailOnDespawnedOrNull(TargetIndex.B);
		toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
		yield return toil;
		yield return Toils_General.Do(DoRecasting);
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, null, storageMode: false);
	}

	public void DoRecasting()
	{
		CompRefuelable compRefuelable = RecastingTable.TryGetComp<CompRefuelable>();
		float fuel = compRefuelable.Fuel;
		if (!(fuel < Quality_Recasting_Table_Main.Instance.Quality_Recasting_Table_Setting.fuelspent))
		{
			QualityCategory randomQualityWithProbability = GetRandomQualityWithProbability();
			Thing thing = ReforgedItem;
			if (thing is MinifiedThing minifiedThing)
			{
				thing = minifiedThing.InnerThing;
			}
			CompQuality compQuality = thing?.TryGetComp<CompQuality>();
			Letter(randomQualityWithProbability);
			compQuality?.SetQuality(randomQualityWithProbability, ArtGenerationContext.Outsider);
			compRefuelable.ConsumeFuel(Quality_Recasting_Table_Main.Instance.Quality_Recasting_Table_Setting.fuelspent);
			SoundDef tR_RecastCompleted = RecastingDefOf.TR_RecastCompleted;
			tR_RecastCompleted.PlayOneShot(new TargetInfo(RecastingTable.Position, RecastingTable.Map));
		}
	}

	public void Datasettings()
	{
		int recastprobabilitysetting = Quality_Recasting_Table_Main.Instance.Quality_Recasting_Table_Setting.recastprobabilitysetting;
		if (recastprobabilitysetting == 1)
		{
			A = a1;
			B = b1;
			C = c1;
			D = d1;
			E = e1;
			F = f1;
			G = g1;
		}
		if (recastprobabilitysetting == 2)
		{
			A = a2;
			B = b2;
			C = c2;
			D = d2;
			E = e2;
			F = f2;
			G = g2;
		}
		if (recastprobabilitysetting == 3)
		{
			A = a3;
			B = b3;
			C = c3;
			D = d3;
			E = e3;
			F = f3;
			G = g3;
		}
	}

	public QualityCategory GetRandomQualityWithProbability()
	{
		Datasettings();
		Dictionary<QualityCategory, float> dictionary = new Dictionary<QualityCategory, float>
		{
			{
				QualityCategory.Awful,
				A
			},
			{
				QualityCategory.Poor,
				B
			},
			{
				QualityCategory.Normal,
				C
			},
			{
				QualityCategory.Good,
				D
			},
			{
				QualityCategory.Excellent,
				E
			},
			{
				QualityCategory.Masterwork,
				F
			},
			{
				QualityCategory.Legendary,
				G
			}
		};
		float value = Random.value;
		float num = 0f;
		foreach (KeyValuePair<QualityCategory, float> item in dictionary)
		{
			num += item.Value;
			if (value <= num)
			{
				return item.Key;
			}
		}
		return QualityCategory.Normal;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref a1, "A1", 0.05f);
		Scribe_Values.Look(ref b1, "B1", 0.15f);
		Scribe_Values.Look(ref c1, "C1", 0.25f);
		Scribe_Values.Look(ref d1, "D1", 0.25f);
		Scribe_Values.Look(ref e1, "E1", 0.15f);
		Scribe_Values.Look(ref f1, "F1", 0.1f);
		Scribe_Values.Look(ref g1, "G1", 0.05f);
		Scribe_Values.Look(ref a2, "A2", 0.05f);
		Scribe_Values.Look(ref b2, "B2", 0.1f);
		Scribe_Values.Look(ref c2, "C2", 0.25f);
		Scribe_Values.Look(ref d2, "D2", 0.2f);
		Scribe_Values.Look(ref e2, "E2", 0.15f);
		Scribe_Values.Look(ref f2, "F2", 0.15f);
		Scribe_Values.Look(ref g2, "G2", 0.1f);
		Scribe_Values.Look(ref a3, "A3", 0.05f);
		Scribe_Values.Look(ref b3, "B3", 0.05f);
		Scribe_Values.Look(ref c3, "C3", 0.15f);
		Scribe_Values.Look(ref d3, "D3", 0.15f);
		Scribe_Values.Look(ref e3, "E3", 0.25f);
		Scribe_Values.Look(ref f3, "F3", 0.2f);
		Scribe_Values.Look(ref g3, "G3", 0.15f);
	}

	public void Letter(QualityCategory T)
	{
		if (Quality_Recasting_Table_Main.Instance.Quality_Recasting_Table_Setting.enableEnvelope)
		{
			if (T == QualityCategory.Awful)
			{
				Letter1();
			}
			if (T == QualityCategory.Poor)
			{
				Letter2();
			}
			if (T == QualityCategory.Normal)
			{
				Letter3();
			}
			if (T == QualityCategory.Good)
			{
				Letter4();
			}
			if (T == QualityCategory.Excellent)
			{
				Letter5();
			}
		}
		if (T == QualityCategory.Masterwork)
		{
			Letter6();
		}
		if (T == QualityCategory.Legendary)
		{
			Letter7();
		}
	}

	public void Letter1()
	{
		TaggedString baseLetterLabel = "Awful_letterLabel".Translate();
		TaggedString str = "Awful_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}

	public void Letter2()
	{
		TaggedString baseLetterLabel = "Poor_letterLabel".Translate();
		TaggedString str = "Poor_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}

	public void Letter3()
	{
		TaggedString baseLetterLabel = "Normal_letterLabel".Translate();
		TaggedString str = "Normal_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}

	public void Letter4()
	{
		TaggedString baseLetterLabel = "Good_letterLabel".Translate();
		TaggedString str = "Good_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}

	public void Letter5()
	{
		TaggedString baseLetterLabel = "Excellent_letterLabel".Translate();
		TaggedString str = "Excellent_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}

	public void Letter6()
	{
		TaggedString baseLetterLabel = "Masterwork_letterLabel".Translate();
		TaggedString str = "Masterwork_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}

	public void Letter7()
	{
		TaggedString baseLetterLabel = "Legendary_letterLabel".Translate();
		TaggedString str = "Legendary_letterText".Translate();
		LetterDef positiveEvent = LetterDefOf.PositiveEvent;
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		LookTargets lookTargets = new LookTargets(ReforgedItem);
		NamedArgument[] array = new NamedArgument[1] { pawn.Name.ToStringShort.Named("name") };
		IncidentWorker.SendIncidentLetter(baseLetterLabel, str.Formatted(array), positiveEvent, parms, lookTargets, null, array);
	}
}
