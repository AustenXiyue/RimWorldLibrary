using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class AmmoSetDef : Def
{
	public List<AmmoLink> ammoTypes;

	public bool isMortarAmmoSet = false;

	public AmmoSetDef similarTo;

	public int ammoConsumedPerShot = 1;

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		foreach (AmmoLink link in ammoTypes)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, link.ammo.label, "", link.projectile.GetProjectileReadout(null), 1, null, new List<Dialog_InfoCard.Hyperlink>
			{
				new Dialog_InfoCard.Hyperlink(link.ammo)
			});
		}
	}
}
