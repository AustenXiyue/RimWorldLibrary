using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_Stabilize : JobDriver
{
	private const float baseTendDuration = 60f;

	private Pawn Patient => pawn.CurJob.targetA.Thing as Pawn;

	private Medicine Medicine => pawn.CurJob.targetB.Thing as Medicine;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(base.TargetA, job) && pawn.Reserve(base.TargetB, job);
	}

	public void MakeMedicineFilth(Medicine medicine)
	{
		MedicineFilthExtension medicineFilthExtension = medicine.def.GetModExtension<MedicineFilthExtension>() ?? new MedicineFilthExtension();
		if (medicineFilthExtension.filthDefName == null || !Rand.Chance(medicineFilthExtension.filthSpawnChance))
		{
			return;
		}
		int randomInRange = medicineFilthExtension.filthSpawnQuantity.RandomInRange;
		List<IntVec3> list = GenAdj.AdjacentCells8WayRandomized();
		for (int i = 0; i < randomInRange; i++)
		{
			IntVec3 c = pawn.Position + list[i];
			if (c.InBounds(pawn.Map))
			{
				FilthMaker.TryMakeFilth(c, pawn.Map, medicineFilthExtension.filthDefName);
			}
		}
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => Patient == null || Medicine == null);
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
		this.FailOnNotDowned(TargetIndex.A);
		AddEndCondition(delegate
		{
			if (Patient.health.hediffSet.GetHediffsTendable().Any((Hediff h) => h.CanBeStabilized()))
			{
				return JobCondition.Ongoing;
			}
			Medicine.Destroy();
			MakeMedicineFilth(Medicine);
			return JobCondition.Incompletable;
		});
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, storageMode: false);
		int duration = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 60f);
		Toil waitToil = Toils_General.Wait(duration).WithProgressBarToilDelay(TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
		yield return waitToil;
		yield return new Toil
		{
			initAction = delegate
			{
				float xp = ((!Patient.RaceProps.Animal) ? 125f : (50f * Medicine.def.MedicineTendXpGainFactor));
				pawn.skills.Learn(SkillDefOf.Medicine, xp);
				foreach (Hediff item in from x in Patient.health.hediffSet.GetHediffsTendable()
					orderby x.BleedRate descending
					select x)
				{
					if (item.CanBeStabilized())
					{
						HediffComp_Stabilize hediffComp_Stabilize = item.TryGetComp<HediffComp_Stabilize>();
						hediffComp_Stabilize.Stabilize(pawn, Medicine);
						break;
					}
				}
			},
			defaultCompleteMode = ToilCompleteMode.Instant
		};
		yield return Toils_Jump.Jump(waitToil);
	}
}
