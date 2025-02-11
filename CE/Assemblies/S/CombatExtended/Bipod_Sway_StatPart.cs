using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Bipod_Sway_StatPart : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		BipodComp bipodComp = req.Thing.TryGetComp<BipodComp>();
		if (Controller.settings.BipodMechanics && bipodComp != null)
		{
			if (bipodComp.IsSetUpRn)
			{
				val *= bipodComp.Props.swayMult;
			}
			else
			{
				val *= bipodComp.Props.swayPenalty;
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		BipodComp bipodComp = req.Thing.TryGetComp<BipodComp>();
		if (bipodComp != null)
		{
			if (bipodComp.IsSetUpRn)
			{
				return "Bipod IS set up -" + " x".Colorize(Color.green) + bipodComp.Props.swayMult.ToString().Colorize(Color.green);
			}
			return "Bipod is NOT set up - " + "x".Colorize(Color.red) + bipodComp.Props.swayPenalty.ToString().Colorize(Color.red);
		}
		return "";
	}
}
