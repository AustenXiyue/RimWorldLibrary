using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class AttachmentDef : ThingDef
{
	public List<string> slotTags;

	public List<string> attachmentTags;

	public GraphicData attachmentGraphicData;

	public GraphicData outlineGraphicData;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statMultipliers;

	public List<StatModifier> statReplacers;

	[Unsaved(false, allowLoading = false)]
	public bool statsValidated = false;

	public void ValidateStats()
	{
		if (statsValidated)
		{
			Log.Warning("CE: called ValidateStats for a valid attachment stat configuration! " + defName);
			return;
		}
		if (slotTags == null)
		{
			slotTags = new List<string>();
		}
		if (attachmentTags == null)
		{
			attachmentTags = new List<string>();
		}
		if (statOffsets == null)
		{
			statOffsets = new List<StatModifier>();
		}
		if (statMultipliers == null)
		{
			statMultipliers = new List<StatModifier>();
		}
		if (statReplacers == null)
		{
			statReplacers = new List<StatModifier>();
		}
		statsValidated = true;
		StatModifier statModifier = statBases.FirstOrFallback((StatModifier s) => s.stat == StatDefOf.Mass);
		if (statModifier != null && !statOffsets.Any((StatModifier m) => m.stat == StatDefOf.Mass))
		{
			statOffsets.Add(statModifier);
		}
		statModifier = statBases.FirstOrFallback((StatModifier s) => s.stat == CE_StatDefOf.Bulk);
		if (statModifier != null && !statOffsets.Any((StatModifier m) => m.stat == CE_StatDefOf.Bulk))
		{
			statOffsets.Add(statModifier);
		}
		statModifier = statBases.FirstOrFallback((StatModifier s) => s.stat == StatDefOf.MarketValue);
		if (statModifier != null && !statOffsets.Any((StatModifier m) => m.stat == StatDefOf.MarketValue))
		{
			statOffsets.Add(statModifier);
		}
		statModifier = statBases.FirstOrFallback((StatModifier s) => s.stat == StatDefOf.Flammability);
		if (statModifier != null && !statOffsets.Any((StatModifier m) => m.stat == StatDefOf.Flammability))
		{
			statMultipliers.Add(statModifier);
		}
		statModifier = statBases.FirstOrFallback((StatModifier s) => s.stat == CE_StatDefOf.MagazineCapacity);
		if (statModifier != null && !statReplacers.Any((StatModifier m) => m.stat == CE_StatDefOf.MagazineCapacity))
		{
			statReplacers.Add(statModifier);
		}
	}
}
