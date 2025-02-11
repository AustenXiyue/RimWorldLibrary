using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class Apparel_Shield : Apparel
{
	private const float YOffsetBehind = 0.00390625f;

	private const float YOffsetPostHead = 0.03515625f;

	private const float YOffsetPrimaryEquipmentUnder = 0f;

	private const float YOffsetPrimaryEquipmentOver = 5f / 128f;

	private const float YOffsetIntervalClothes = 0.00390625f;

	private const float YOffsetStatus = 0.04296875f;

	public const string OneHandedTag = "CE_OneHandedWeapon";

	private bool drawShield
	{
		get
		{
			int result;
			if (def.apparel.renderNodeProperties.NullOrEmpty())
			{
				if (!base.Wearer.Drafted)
				{
					Job curJob = base.Wearer.CurJob;
					if (curJob == null || !curJob.def.alwaysShowWeapon)
					{
						result = ((base.Wearer.mindState.duty?.def.alwaysShowWeapon ?? false) ? 1 : 0);
						goto IL_0069;
					}
				}
				result = 1;
			}
			else
			{
				result = 0;
			}
			goto IL_0069;
			IL_0069:
			return (byte)result != 0;
		}
	}

	private bool IsTall => def.GetModExtension<ShieldDefExtension>()?.drawAsTall ?? false;

	public override void PostMake()
	{
		base.PostMake();
		if (def.apparel.renderNodeProperties.NullOrEmpty())
		{
			Log.WarningOnce(def.defName + " using obsolete render system", def.defName.GetHashCodeSafe());
		}
	}

	public override bool AllowVerbCast(Verb verb)
	{
		ThingWithComps thingWithComps = base.Wearer.equipment?.Primary;
		return thingWithComps == null || (thingWithComps.def.weaponTags?.Contains("CE_OneHandedWeapon") ?? false);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		RoyalTitleDef royalTitleDef = (from t in DefDatabase<FactionDef>.AllDefsListForReading.SelectMany((FactionDef f) => f.RoyalTitlesAwardableInSeniorityOrderForReading)
			where t.requiredApparel != null && t.requiredApparel.Any((ApparelRequirement req) => req.ApparelMeetsRequirement(def, allowUnmatched: false))
			orderby t.seniority descending
			select t).FirstOrDefault();
		if (royalTitleDef != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_Apparel_MaxSatisfiedTitle".Translate(), royalTitleDef.GetLabelCapForBothGenders(), "Stat_Thing_Apparel_MaxSatisfiedTitle_Desc".Translate(), 2752, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(royalTitleDef)
			});
		}
		List<BodyPartGroupDef> shieldCoverage = def.GetModExtension<ShieldDefExtension>()?.shieldCoverage;
		if (shieldCoverage != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "CE_Shield_Coverage".Translate(), ShieldDefExtension.GetShieldProtectedAreas(BodyDefOf.Human, def), "CE_Shield_Coverage_Desc".Translate(), 800);
		}
	}

	public override void DrawWornExtras()
	{
		if (base.Wearer == null || !base.Wearer.Spawned || !drawShield)
		{
			return;
		}
		float angle = 0f;
		Vector3 drawPos = base.Wearer.Drawer.DrawPos;
		drawPos.y = ((base.Wearer.Rotation == Rot4.West || base.Wearer.Rotation == Rot4.South) ? AltitudeLayer.PawnUnused.AltitudeFor() : AltitudeLayer.Pawn.AltitudeFor());
		Vector3 s = new Vector3(1f, 1f, 1f);
		if (base.Wearer.Rotation == Rot4.North)
		{
			drawPos.x -= 0.1f;
			drawPos.z -= (IsTall ? (-0.1f) : 0.2f);
		}
		else if (base.Wearer.Rotation == Rot4.South)
		{
			drawPos.x += 0.1f;
			drawPos.z -= (IsTall ? (-0.05f) : 0.2f);
		}
		else if (base.Wearer.Rotation == Rot4.East)
		{
			if (IsTall)
			{
				drawPos.x += 0.1f;
			}
			drawPos.z -= (IsTall ? (-0.05f) : 0.2f);
			angle = 22.5f;
		}
		else if (base.Wearer.Rotation == Rot4.West)
		{
			if (IsTall)
			{
				drawPos.x -= 0.1f;
			}
			drawPos.z -= (IsTall ? (-0.05f) : 0.2f);
			angle = 337.5f;
		}
		Material matSingle = Graphic.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor, DrawColorTwo).MatSingle;
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
		Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, 0);
	}
}
