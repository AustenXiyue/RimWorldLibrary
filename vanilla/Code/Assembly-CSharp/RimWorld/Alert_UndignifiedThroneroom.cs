using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_UndignifiedThroneroom : Alert
{
	private List<Pawn> targetsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	public List<Pawn> Targets
	{
		get
		{
			targetsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				foreach (Pawn freeColonist in maps[i].mapPawns.FreeColonists)
				{
					if (freeColonist.royalty != null && !freeColonist.Suspended && freeColonist.royalty.AnyUnmetThroneroomRequirements())
					{
						targetsResult.Add(freeColonist);
					}
				}
			}
			return targetsResult;
		}
	}

	public Alert_UndignifiedThroneroom()
	{
		defaultLabel = "UndignifiedThroneroom".Translate();
		defaultExplanation = "UndignifiedThroneroomDesc".Translate();
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}

	public override TaggedString GetExplanation()
	{
		return defaultExplanation + "\n" + targetsResult.Select(delegate(Pawn t)
		{
			RoyalTitle royalTitle = t.royalty.HighestTitleWithThroneRoomRequirements();
			RoyalTitleDef royalTitleDef = (royalTitle.RoomRequirementGracePeriodActive(t) ? royalTitle.def.GetPreviousTitle(royalTitle.faction) : royalTitle.def);
			string[] array = t.royalty.GetUnmetThroneroomRequirements(includeOnGracePeriod: false).ToArray();
			string[] array2 = t.royalty.GetUnmetThroneroomRequirements(includeOnGracePeriod: true, onlyOnGracePeriod: true).ToArray();
			sb.Length = 0;
			if (array.Length != 0)
			{
				sb.Append(t.LabelShort + " (" + royalTitleDef.GetLabelFor(t.gender) + "):\n" + array.ToLineList("  - "));
			}
			if (array2.Length != 0)
			{
				if (array.Length != 0)
				{
					sb.Append("\n\n");
				}
				sb.Append(t.LabelShort + " (" + royalTitle.def.GetLabelFor(t.gender) + ", " + "RoomRequirementGracePeriodDesc".Translate(royalTitle.RoomRequirementGracePeriodDaysLeft.ToString("0.0")) + ")" + ":\n" + array2.ToLineList("  - "));
			}
			return sb.ToString();
		}).ToLineList("\n");
	}
}
