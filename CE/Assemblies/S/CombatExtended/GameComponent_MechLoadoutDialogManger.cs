using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class GameComponent_MechLoadoutDialogManger : GameComponent
{
	private List<CompMechAmmo> _compMechAmmoQueue = new List<CompMechAmmo>();

	public GameComponent_MechLoadoutDialogManger(Game game)
	{
	}

	public override void GameComponentUpdate()
	{
		if (_compMechAmmoQueue.Count <= 0)
		{
			return;
		}
		List<CompMechAmmo> list = new List<CompMechAmmo>(_compMechAmmoQueue);
		ThingDef thingDef = list[0].ParentPawn?.equipment?.Primary?.def;
		foreach (CompMechAmmo item in list)
		{
			if (item.ParentPawn?.equipment?.Primary?.def != thingDef)
			{
				Messages.Message("MTA_CannotSetLoadoutForMultipleEquipment".Translate(), MessageTypeDefOf.RejectInput);
				_compMechAmmoQueue.Clear();
				return;
			}
		}
		_compMechAmmoQueue.Clear();
		Find.WindowStack.Add(new Dialog_SetMagCountBatched(list));
	}

	public void RegisterCompMechAmmo(CompMechAmmo compMechAmmo)
	{
		if (!_compMechAmmoQueue.Contains(compMechAmmo))
		{
			_compMechAmmoQueue.Add(compMechAmmo);
		}
	}
}
