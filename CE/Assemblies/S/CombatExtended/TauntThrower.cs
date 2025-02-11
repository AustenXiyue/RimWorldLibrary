using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace CombatExtended;

public class TauntThrower : MapComponent
{
	private const int minTicksBetweenTaunts = 180;

	private Dictionary<Pawn, int> tauntTickTracker = new Dictionary<Pawn, int>();

	public TauntThrower(Map map)
		: base(map)
	{
	}

	private bool AllowThrowTauntNow(Pawn pawn)
	{
		if (!Controller.settings.ShowTaunts || pawn == null || !pawn.def.race.Humanlike)
		{
			return false;
		}
		return TimedOut(pawn);
	}

	private bool TimedOut(Pawn pawn)
	{
		tauntTickTracker.TryGetValue(pawn, out var value);
		return Find.TickManager.TicksGame - value > 180;
	}

	public void TryThrowTaunt(RulePackDef rulePack, Pawn pawn)
	{
		if (AllowThrowTauntNow(pawn))
		{
			string text = GrammarResolver.Resolve(rulePack.RulesPlusIncludes[0].keyword, new GrammarRequest
			{
				Includes = { rulePack }
			});
			if (text.NullOrEmpty())
			{
				Log.Warning("CE tried throwing invalid taunt for " + pawn.ToString());
			}
			else
			{
				MoteMakerCE.ThrowText(pawn.Position.ToVector3Shifted(), pawn.Map, text);
			}
			int ticksGame = Find.TickManager.TicksGame;
			if (!tauntTickTracker.ContainsKey(pawn))
			{
				tauntTickTracker.Add(pawn, ticksGame);
			}
			else
			{
				tauntTickTracker[pawn] = ticksGame;
			}
		}
	}

	public override void MapComponentTick()
	{
		if ((Find.TickManager.TicksGame + map.uniqueID * 3) % 10 == 0)
		{
			KeyValuePair<Pawn, int>[] array = tauntTickTracker.Where((KeyValuePair<Pawn, int> kvp) => TimedOut(kvp.Key)).ToArray();
			foreach (KeyValuePair<Pawn, int> keyValuePair in array)
			{
				tauntTickTracker.Remove(keyValuePair.Key);
			}
		}
	}
}
