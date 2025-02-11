using Verse;

namespace CombatExtended.AI;

public class CompReload : ICompTactics
{
	public override int Priority => 200;

	public override bool StartCastChecks(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		CompAmmoUser compAmmoUser = verb.EquipmentSource.TryGetComp<CompAmmoUser>();
		if (compAmmoUser == null || !compAmmoUser.HasMagazine || compAmmoUser.CurMagCount > 0)
		{
			return true;
		}
		if (verb.EquipmentSource == CurrentWeapon && compAmmoUser.UseAmmo && !compAmmoUser.HasAmmo)
		{
			CompInventory.SwitchToNextViableWeapon(useFists: true, !SelPawn.Faction.IsPlayer, stopJob: false);
			return false;
		}
		compAmmoUser.TryStartReload();
		return false;
	}
}
