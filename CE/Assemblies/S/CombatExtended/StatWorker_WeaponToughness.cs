using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_WeaponToughness : StatWorker
{
	public const float parryChanceFactor = 5f;

	public float GetHolderToughnessFactor(Pawn pawn)
	{
		Pawn_ApparelTracker apparel = pawn.apparel;
		pawn.apparel = null;
		Pawn_EquipmentTracker equipment = pawn.equipment;
		pawn.equipment = null;
		float statValue;
		try
		{
			statValue = pawn.GetStatValue(CE_StatDefOf.MeleeParryChance);
		}
		finally
		{
			pawn.apparel = apparel;
			pawn.equipment = equipment;
		}
		return statValue * 5f;
	}

	public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
	{
		Thing thing = req.Thing;
		if (thing == null)
		{
			return;
		}
		if (stat.parts != null)
		{
			foreach (StatPart part in stat.parts)
			{
				part.TransformValue(req, ref val);
			}
		}
		if (thing.Stuff != null)
		{
			val *= thing.Stuff.GetModExtension<StuffToughnessMultiplierExtensionCE>()?.toughnessMultiplier ?? 1f;
		}
		Pawn pawn = thing.TryGetComp<CompEquippable>()?.Holder;
		if (pawn?.equipment?.Primary == thing)
		{
			val *= GetHolderToughnessFactor(pawn);
		}
	}

	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		string text = "";
		if (stat.parts != null)
		{
			foreach (StatPart part in stat.parts)
			{
				if (!part.ExplanationPart(req).NullOrEmpty())
				{
					text = text + "\n" + part.ExplanationPart(req);
				}
			}
		}
		if (req.Thing.Stuff != null)
		{
			text += "\n" + "CE_StatsReport_WeaponToughness_StuffEffect".Translate() + (req.Thing.Stuff.GetModExtension<StuffToughnessMultiplierExtensionCE>()?.toughnessMultiplier ?? 1f).ToStringPercent();
		}
		if (req.Thing != null && req.Thing?.TryGetComp<CompEquippable>()?.Holder != null)
		{
			text += "\n" + "CE_StatsReport_WeaponToughness_HolderEffect".Translate() + GetHolderToughnessFactor(req.Thing.TryGetComp<CompEquippable>().Holder).ToStringPercent();
		}
		return text + ("\n" + "StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(finalVal, stat.toStringNumberSense));
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		return Controller.settings.ShowExtraStats && req.HasThing && req.Thing.def.IsWeapon && base.ShouldShowFor(req);
	}
}
