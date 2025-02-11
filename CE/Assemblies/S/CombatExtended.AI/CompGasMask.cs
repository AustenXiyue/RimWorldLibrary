using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended.AI;

public class CompGasMask : ICompTactics
{
	private const string GASMASK_TAG = "GasMask";

	private const int SMOKE_TICKS_OFFSET = 800;

	private int lastSmokeTick = -1;

	private bool maskEquiped = false;

	private int ticks = 0;

	public override int Priority => 50;

	public override void TickRarer()
	{
		base.TickRarer();
		if (ticks % 2 == 0)
		{
			UpdateGasMask();
		}
		ticks++;
	}

	public void UpdateGasMask()
	{
		if (!SelPawn.Faction.IsPlayerSafe() && SelPawn.Spawned && !SelPawn.Downed && SelPawn.apparel?.wornApparel != null)
		{
			if (ticks % 4 == 0)
			{
				CheckForMask();
			}
			if (lastSmokeTick < GenTicks.TicksGame && maskEquiped)
			{
				RemoveMask();
			}
		}
	}

	public void Notify_ShouldEquipGasMask()
	{
		if (lastSmokeTick < GenTicks.TicksGame && !SelPawn.Faction.IsPlayerSafe() && !SelPawn.Downed && SelPawn.apparel?.wornApparel != null)
		{
			WearMask();
			lastSmokeTick = GenTicks.TicksGame + 800;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastSmokeTick, "lastSmokeTick", -1);
		Scribe_Values.Look(ref maskEquiped, "maskEquiped", defaultValue: false);
	}

	private void WearMask()
	{
		Apparel apparel = null;
		foreach (Apparel item in CompInventory.container.Where((Thing t) => t is Apparel).Cast<Apparel>())
		{
			if (item.def.apparel?.tags?.Contains("GasMask") == true)
			{
				apparel = item;
				break;
			}
		}
		if (apparel != null)
		{
			SelPawn.inventory.innerContainer.Remove(apparel);
			SelPawn.apparel.Wear(apparel);
		}
	}

	private void RemoveMask()
	{
		Apparel apparel = null;
		foreach (Apparel item in SelPawn.apparel.wornApparel)
		{
			if (item.def.apparel?.tags?.Contains("GasMask") == true)
			{
				apparel = item;
				break;
			}
		}
		if (apparel != null)
		{
			SelPawn.apparel.Remove(apparel);
			SelPawn.inventory.innerContainer.TryAddOrTransfer(apparel);
		}
		maskEquiped = false;
	}

	private void CheckForMask()
	{
		maskEquiped = false;
		foreach (Apparel item in SelPawn.apparel.wornApparel)
		{
			if (item.def.apparel?.tags?.Contains("GasMask") == true)
			{
				maskEquiped = true;
				break;
			}
		}
	}
}
