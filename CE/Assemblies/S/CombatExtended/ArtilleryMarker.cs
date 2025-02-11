using Verse;

namespace CombatExtended;

internal class ArtilleryMarker : AttachableThing
{
	public const string MarkerDef = "ArtilleryMarker";

	public Pawn caster;

	public float aimingAccuracy = 1f;

	public float sightsEfficiency = 1f;

	public float lightingShift = 0f;

	public float weatherShift = 0f;

	private int lifetimeTicks = 1800;

	public override string InspectStringAddon => "CE_MarkedForArtillery".Translate() + " " + (lifetimeTicks / 60).ToString() + " s";

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref caster, "caster");
		Scribe_Values.Look(ref aimingAccuracy, "aimingAccuracy", 0f);
		Scribe_Values.Look(ref sightsEfficiency, "sightsEfficiency", 0f);
		Scribe_Values.Look(ref lightingShift, "lightingShift", 0f);
		Scribe_Values.Look(ref weatherShift, "weatherShift", 0f);
		Scribe_Values.Look(ref lifetimeTicks, "lifetimeTicks", 0);
	}

	public override void Tick()
	{
		lifetimeTicks--;
		if (lifetimeTicks <= 0)
		{
			Destroy();
		}
	}

	public override void AttachTo(Thing parent)
	{
		if (parent != null)
		{
			CompAttachBase compAttachBase = parent.TryGetComp<CompAttachBase>();
			if (compAttachBase != null && parent.HasAttachment(ThingDef.Named("ArtilleryMarker")))
			{
				ArtilleryMarker artilleryMarker = (ArtilleryMarker)parent.GetAttachment(ThingDef.Named("ArtilleryMarker"));
				artilleryMarker.Destroy();
			}
		}
		base.AttachTo(parent);
	}
}
