using Verse;

namespace AncotLibrary;

public class CompWeaponAbility : ThingComp
{
	private Pawn wielder;

	private int currentCoolDown;

	public CompProperties_WeaponAbility Props => (CompProperties_WeaponAbility)props;

	public override void Notify_Equipped(Pawn pawn)
	{
		wielder = pawn;
		wielder.abilities.GainAbility(Props.abilityDef);
		wielder.abilities.GetAbility(Props.abilityDef, includeTemporary: true).StartCooldown(currentCoolDown);
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		currentCoolDown = wielder.abilities.GetAbility(Props.abilityDef, includeTemporary: true).CooldownTicksRemaining;
		wielder.abilities.RemoveAbility(Props.abilityDef);
		wielder = null;
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		if (wielder != null)
		{
			wielder.abilities.RemoveAbility(Props.abilityDef);
			wielder = null;
		}
		base.PostDestroy(mode, previousMap);
	}

	public override void PostExposeData()
	{
		Scribe_References.Look(ref wielder, "wielder");
		Scribe_Values.Look(ref currentCoolDown, "currentCoolDown", 0);
		base.PostExposeData();
	}
}
