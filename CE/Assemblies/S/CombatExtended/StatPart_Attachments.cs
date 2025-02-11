using System.Linq;
using RimWorld;

namespace CombatExtended;

public class StatPart_Attachments : StatPart
{
	public override string ExplanationPart(StatRequest req)
	{
		if (!req.HasThing || !(req.Thing?.def is WeaponPlatformDef))
		{
			return "";
		}
		WeaponPlatform weaponPlatform = (WeaponPlatform)req.Thing;
		if (weaponPlatform.Platform?.attachmentLinks == null)
		{
			return "";
		}
		return parentStat.ExplainAttachmentsStat(weaponPlatform.CurLinks);
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing?.def is WeaponPlatformDef)
		{
			WeaponPlatform weaponPlatform = (WeaponPlatform)req.Thing;
			if (weaponPlatform.Platform?.attachmentLinks != null)
			{
				parentStat.TransformValue(weaponPlatform.CurLinks.ToList(), ref val);
			}
		}
	}
}
