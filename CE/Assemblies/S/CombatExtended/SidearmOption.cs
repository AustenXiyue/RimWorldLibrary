using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class SidearmOption
{
	public FloatRange sidearmMoney = FloatRange.Zero;

	public FloatRange magazineCount = FloatRange.Zero;

	public List<string> weaponTags;

	public float generateChance = 1f;

	public AttachmentOption attachments;
}
