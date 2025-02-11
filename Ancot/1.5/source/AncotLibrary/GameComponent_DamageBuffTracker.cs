using System.Collections.Generic;
using System.Text;
using Verse;

namespace AncotLibrary;

public class GameComponent_DamageBuffTracker : GameComponent
{
	private Dictionary<Pawn, DamageBuffTracker> DamageBuffList = new Dictionary<Pawn, DamageBuffTracker>();

	private List<Pawn> PawnsList;

	private List<DamageBuffTracker> DBCTL;

	private StringBuilder stb = new StringBuilder();

	public static GameComponent_DamageBuffTracker Tracker => Current.Game?.GetComponent<GameComponent_DamageBuffTracker>();

	public Dictionary<Pawn, DamageBuffTracker> DamageBuff_Duration_ListReadOnly => DamageBuffList;

	public GameComponent_DamageBuffTracker(Game game)
	{
	}

	public float CheckForDamageBuff(Pawn pawn, bool consume = true)
	{
		int validUntil;
		int charges;
		return CheckForDamageBuff(pawn, out validUntil, out charges, consume);
	}

	public float CheckForDamageBuff(Pawn pawn, out int validUntil, out int charges, bool consume = true)
	{
		validUntil = 0;
		charges = 0;
		if (DamageBuffList.ContainsKey(pawn))
		{
			DamageBuffTracker damageBuffTracker = DamageBuffList[pawn];
			if (damageBuffTracker.validUntil > 0 && Find.TickManager.TicksGame > damageBuffTracker.validUntil)
			{
				DamageBuffList.Remove(pawn);
				return 0f;
			}
			if (damageBuffTracker.charges == 0)
			{
				DamageBuffList.Remove(pawn);
			}
			validUntil = damageBuffTracker.validUntil;
			charges = damageBuffTracker.charges;
			if (consume)
			{
				damageBuffTracker.charges--;
			}
			return damageBuffTracker.pct;
		}
		return 0f;
	}

	public void RegisterDamageBuffDuration(Pawn pawn, float buff, int duration = -1, int charge = -1, HediffDef trackerHediff = null)
	{
		if (duration < 0 && charge < 0)
		{
			Log.Error($"Attempted to give a damage buff to {pawn} without duration or charges");
			return;
		}
		if (DamageBuffList.ContainsKey(pawn))
		{
			DamageBuffTracker damageBuffTracker = DamageBuffList[pawn];
			if (damageBuffTracker.pct < buff)
			{
				damageBuffTracker.pct = buff;
				if (duration > 0)
				{
					damageBuffTracker.validUntil = Find.TickManager.TicksGame + duration;
				}
				if (charge > 0)
				{
					damageBuffTracker.charges = charge;
				}
				return;
			}
		}
		if (buff < -0.999f)
		{
			buff = -0.999f;
		}
		DamageBuffTracker damageBuffTracker2 = new DamageBuffTracker
		{
			pct = buff
		};
		if (duration > 0)
		{
			damageBuffTracker2.validUntil = Find.TickManager.TicksGame + duration;
		}
		if (charge > 0)
		{
			damageBuffTracker2.charges = charge;
		}
		DamageBuffList.Add(pawn, damageBuffTracker2);
		if (trackerHediff != null && trackerHediff.hediffClass == typeof(Hediff_DamageBuffVisualizer))
		{
			pawn.health.AddHediff(trackerHediff, null, null);
		}
	}

	public void DeregisterDamageBuff(Pawn pawn)
	{
		List<Hediff_DamageBuffVisualizer> resultHediffs = new List<Hediff_DamageBuffVisualizer>();
		pawn.health.hediffSet.GetHediffs(ref resultHediffs);
		foreach (Hediff_DamageBuffVisualizer item in resultHediffs)
		{
			item.ImproperlyRemoved = false;
			pawn.health.RemoveHediff(item);
		}
		DamageBuffList.Remove(pawn);
	}

	public void SelfCheck()
	{
	}

	public override void ExposeData()
	{
		Scribe_Collections.Look(ref DamageBuffList, "DamageBuffList", LookMode.Reference, LookMode.Deep, ref PawnsList, ref DBCTL);
	}
}
