using Verse;

namespace AncotLibrary;

public class Hediff_DamageBuffVisualizer : HediffWithComps
{
	public bool ImproperlyRemoved = true;

	private string labelcache = "";

	public override string Label
	{
		get
		{
			if (labelcache == "" || !Find.TickManager.Paused)
			{
				labelcache = "";
				int validUntil;
				int charges;
				float num = GameComponent_DamageBuffTracker.Tracker.CheckForDamageBuff(pawn, out validUntil, out charges, consume: false);
				if (num > 0f)
				{
					labelcache += "+";
				}
				labelcache += $"{num * 100f:f0}% ";
				if (validUntil > 0)
				{
					labelcache = labelcache + (validUntil - Find.TickManager.TicksGame).TicksToSeconds().ToString("f0") + "sec";
				}
				if (validUntil > 0 && charges > 0)
				{
					labelcache += "/";
				}
				if (charges > 0)
				{
					labelcache = labelcache + "x" + charges;
				}
			}
			return labelcache;
		}
	}

	public override string Description => base.Description;

	public override void PostRemoved()
	{
		if (ImproperlyRemoved)
		{
			RemoveBuff();
		}
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		RemoveBuff();
	}

	public override void Notify_Downed()
	{
		RemoveBuff();
	}

	private void RemoveBuff()
	{
		GameComponent_DamageBuffTracker.Tracker.DeregisterDamageBuff(pawn);
	}
}
