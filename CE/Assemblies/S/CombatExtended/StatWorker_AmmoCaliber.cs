using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_AmmoCaliber : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		int result;
		if (base.ShouldShowFor(req))
		{
			AmmoDef obj = req.Def as AmmoDef;
			result = ((obj != null && !obj.Users.NullOrEmpty()) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
	{
		if (!(statRequest.Def is AmmoDef { Users: var users }) || users.NullOrEmpty())
		{
			yield break;
		}
		foreach (ThingDef user in users)
		{
			yield return new Dialog_InfoCard.Hyperlink(user);
		}
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (req.Def is AmmoDef { Users: var users } ammoDef && !users.NullOrEmpty())
		{
			List<AmmoSetDef> ammoSetDefs = ammoDef.AmmoSetDefs;
			int count = ammoSetDefs.Count;
			foreach (AmmoSetDef ammoSet in ammoSetDefs)
			{
				string[] value = (from x in users
					where count == 1 || x.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet == ammoSet
					select x into y
					select y.label.CapitalizeFirst()).ToArray();
				ThingDef projectile = ammoSet.ammoTypes.Find((AmmoLink x) => x.ammo == req.Def as AmmoDef).projectile;
				stringBuilder.AppendLine(ammoSet.LabelCap + " (" + string.Join(", ", value) + "):\n" + projectile.GetProjectileReadout(null));
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		List<AmmoSetDef> list = (optionalReq.Def as AmmoDef)?.AmmoSetDefs;
		return list.FirstOrDefault().LabelCap + ((list.Count > 1) ? (" (+" + (list.Count - 1) + ")") : "");
	}
}
